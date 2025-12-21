using Aspose.Cells;
using Autofac;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace SDO.APP.RPT.Excel
{
    /// <summary>
    /// 計畫期程一覽表
    /// </summary>
    public class RPTProjectScheOverview : XlsBuilder
    {
        private readonly IProjectListDac projectListDac;
        private readonly IProjectAdjustDac projectAdjustDac;
        private readonly IProjectDac projectDac;
        private readonly IProjectExecuteService projectExecuteService;
        private readonly IProjectCommonDac commonDac;

        /// <summary>
        /// 計畫基本資料
        /// </summary>
        ProjectListModel projectBasicData;
        /// <summary>
        /// 立案期程
        /// </summary>
        List<ProjectScheOverviewModel> projectOriginSchedule;
        /// <summary>
        /// 計畫調整歷程資料
        /// </summary>
        List<ProjectScheOverviewModel> projectAdjustScheHisList;
        /// <summary>
        /// 計畫調整中資料
        /// </summary>
        List<ProjectScheOverviewModel> projectAdjustSchedule;
        /// <summary>
        /// 計畫預定期程資料
        /// </summary>
        List<ProjectScheOverviewModel> projectEstimatedSchedule;
        /// <summary>
        /// 計畫實際期程資料
        /// </summary>
        List<ProjectScheOverviewModel> projectActualSchedule;
        /// <summary>
        /// 計畫期程資料
        /// </summary>
        List<ProjectCusCheckpointModel> projectSchedule;
        /// <summary>
        /// 落後類型
        /// </summary>
        string delayKind;

        /// <summary>
        /// 計畫開始日期
        /// </summary>
        DateTime? projectStartDate;
        /// <summary>
        /// 期程表頭對應欄位號
        /// </summary>
        private Dictionary<string, int> scheduleHeaderMappingColumn = new Dictionary<string, int>();
        /// <summary>
        /// 基本欄位欄數(序號、項目、時間)
        /// </summary>
        private int baseColumnNum = 8;
        /// <summary>
        /// 固定資料列數(含表頭)
        /// </summary>
        private int baseRowNum = 8;
        /// <summary>
        /// 表頭期程跨月數
        /// </summary>
        private int headerMonthRange = 0;
        /// <summary>
        /// 期程資料序號總數
        /// </summary>
        private int Seq = 1;
        /// <summary>
        /// 總欄數
        /// </summary>
        private int maxColumnNum;
        /// <summary>
        /// 
        /// </summary>
        ProjectScheOverviewQueryModel model;

        /// <summary>
        /// 非工程類甘特圖顏色
        /// </summary>
        Dictionary<string, List<Color>> checkptColorSchema = new Dictionary<string, List<Color>>
        {
            // 立案期程、期程調整歷程
            {"History", new List<Color>()
                {
                    Color.FromArgb(255,242,204),
                    Color.FromArgb(217,225,242),
                    Color.FromArgb(226,239,218),
                    Color.FromArgb(221,235,247),
                    Color.FromArgb(231,230,230)
                }
            },
            // 現行預定進度
            {"Estimated", new List<Color>()
                {
                    Color.FromArgb(255,230,153),
                    Color.FromArgb(180,198,231),
                    Color.FromArgb(198,224,180),
                    Color.FromArgb(189,215,238),
                    Color.FromArgb(208,206,206)
                }
            },
            // 本次申請期程調整
            {"Apply", new List<Color>()
                {
                    Color.FromArgb(255,217,102),
                    Color.FromArgb(142,169,219),
                    Color.FromArgb(169,208,142),
                    Color.FromArgb(155,194,230),
                    Color.FromArgb(174,170,170)
                }
            },
            // 實際執行進度
            {"Actual", new List<Color>()
                {
                    Color.FromArgb(191,143,0),
                    Color.FromArgb(48,84,150),
                    Color.FromArgb(84,130,53),
                    Color.FromArgb(47,117,181),
                    Color.FromArgb(128,128,128),
                }
            }
        };
        /// <summary>
        /// 落後顏色
        /// </summary>
        private Color delayColor = Color.FromArgb(204, 0, 0);
        /// <summary>
        /// 外框樣式
        /// </summary>
        CellBorderType defaultBorderType = CellBorderType.Thin;
        /// <summary>
        /// 須設定邊界Cell
        /// </summary>
        List<Cell> cellsNeedToSetBorder = new();

        /// <summary>
        /// 圖例資料
        /// </summary>
        List<List<ScheOverviewChartLegendModel>> chartLegendData = new();

        public RPTProjectScheOverview(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "ProjectScheOverviewRPT.xlsx";
            projectListDac = coms.Resolve<IProjectListDac>();
            projectAdjustDac = coms.Resolve<IProjectAdjustDac>();
            projectDac = coms.Resolve<IProjectDac>();
            projectExecuteService = coms.Resolve<IProjectExecuteService>();
            commonDac = coms.Resolve<IProjectCommonDac>();
        }

        protected override async Task GetData()
        {
            model = (ProjectScheOverviewQueryModel)Parameter.ObjectModel;
            // 計畫基本資料
            projectBasicData = (await projectListDac.GetProjectList(
                new ProjectListQueryModel() { PROJECT_NO = model.PROJECT_NO })).FirstOrDefault();
            // 計畫開始日期
            projectStartDate = (await projectDac.GetProjectCheckpoint(model.PROJECT_NO))?.CONTROL_DATE1;
            // 上個填報週期
            ProjectFillCycleModel cycleData = GetLastFillCycle(await commonDac.GetCycleData());
            string cycleStr = $"{cycleData.PROJECT_YEAR}/{cycleData.PROJECT_MONTH}";
            delayKind = await projectExecuteService.GetDelayKind(model.PROJECT_NO);

            projectOriginSchedule = ProcessData(await projectAdjustDac.GetProjectOriginSchedule(model.PROJECT_NO), false);
            var adjustScheduleData = await projectAdjustDac.GetProjectScheAdjustList(model.PROJECT_NO);
            projectAdjustSchedule = ProcessData(adjustScheduleData.Item1);
            projectAdjustScheHisList = ProcessData(adjustScheduleData.Item2);
            projectSchedule = (await projectDac.GetProjectCusCheckpoint(model.PROJECT_NO)).ToList();
            List<ProjectScheOverviewModel> projectEstimatedSche = projectSchedule.Select(item =>
                new ProjectScheOverviewModel()
                {
                    ESTIMATED_ENDDATE = item.ESTIMATED_ENDDATE,
                    ACTUAL_ENDDATE = item.ACTUAL_ENDDATE,
                    CONTROL_DATE1 = projectStartDate,
                    ItemDateStr = cycleStr,
                    CTRL_POINT = item.CTRL_POINT,
                    CHECKITEM_NAME = item.CHECKITEM_NAME,
                    CP_KIND = projectBasicData.CP_KIND
                }).ToList();
            List<ProjectScheOverviewModel> projectActualSche = projectSchedule.Select(item =>
                new ProjectScheOverviewModel()
                {
                    ESTIMATED_ENDDATE = item.ESTIMATED_ENDDATE,
                    ACTUAL_ENDDATE = item.ACTUAL_ENDDATE,
                    CONTROL_DATE1 = projectStartDate,
                    ItemDateStr = cycleStr,
                    CTRL_POINT = item.CTRL_POINT,
                    CHECKITEM_NAME = item.CHECKITEM_NAME,
                    CP_KIND = projectBasicData.CP_KIND
                }).ToList();
            projectEstimatedSchedule = ProcessData(projectEstimatedSche, false);
            projectActualSchedule = ProcessActualChkptData(projectActualSche);
        }

        /// <summary>
        /// 資料加工 並處理工程類期程資料
        /// 將工程類檢核點歸為「設計」、「施工」、「驗收」階段
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isAdjData">是否為調整資料(可能會多筆)</param>
        /// <returns></returns>
        private List<ProjectScheOverviewModel> ProcessData(List<ProjectScheOverviewModel> data, bool isAdjData = true)
        {
            if (!data.Any())
                return new List<ProjectScheOverviewModel>();
            List<ProjectScheOverviewModel> result = new();
            // 排除非工程類資料
            if (data.Where(x => x.CP_KIND == "1").Any())
            {
                return data;
            }

            List<string> projAdjIds = isAdjData ? data.Select(x => x.PROJ_ADJ_ID).Distinct().ToList() : new List<string>() { "0" };

            foreach (string adjId in projAdjIds)
            {
                List<ProjectScheOverviewModel> scheData = isAdjData ? data.Where(x => x.PROJ_ADJ_ID == adjId).ToList()
                    : data;
                // 工程類
                if (scheData.First().CP_KIND == "0")
                {
                    // 找出開工、竣工Index
                    int tenderIdx = scheData.FindIndex(x => x.CTRL_POINT == "F");
                    int startWorkIdx = scheData.FindIndex(x => x.CTRL_POINT == "A");
                    int endWorkIdx = scheData.FindIndex(x => x.CTRL_POINT == "B");

                    // 設計階段(計畫開始 - 預算書圖核定/統包需求書核定)
                    if (tenderIdx > 0)
                    {
                        ProjectScheOverviewModel designSchedule = scheData[tenderIdx-1];
                        designSchedule.CHECKITEM_NAME = "設計";
                        result.Add(designSchedule);
                    }

                    // 招標階段(預算書圖核定/統包需求書核定 - 開工)
                    if (startWorkIdx > tenderIdx)
                    {
                        ProjectScheOverviewModel tenderSchedule = scheData[startWorkIdx];
                        tenderSchedule.CHECKITEM_NAME = "招標";
                        result.Add(tenderSchedule);
                    }

                    // 施工階段(開工 - 竣工)
                    if (endWorkIdx > startWorkIdx)
                    {
                        ProjectScheOverviewModel inProgressSchedule = scheData[endWorkIdx];
                        inProgressSchedule.CHECKITEM_NAME = "施工";
                        result.Add(inProgressSchedule);
                    }

                    // 驗收階段(竣工後)
                    ProjectScheOverviewModel acceptanceSchedule = scheData.Last();
                    acceptanceSchedule.CHECKITEM_NAME = "驗收";
                    result.Add(acceptanceSchedule);
                }
                // 非工程類
                else
                {
                    result.AddRange(scheData);
                }
            }
            return result;
        }

        /// <summary>
        /// 處理實際完成日期資料
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public List<ProjectScheOverviewModel> ProcessActualChkptData(List<ProjectScheOverviewModel> data)
        {
            if (!data.Any())
                return new List<ProjectScheOverviewModel>();

            List<ProjectScheOverviewModel> result = new();
            // 非工程類資料
            if (data.Any(x => x.CP_KIND == "1"))
            {
                // 實際執行進度資料(且有實際完成日期未填報) 需另外加工
                // 僅顯示已完成檢核點以及進行中的檢核點
                if (data.Any(x => !x.ACTUAL_ENDDATE.HasValue))
                {
                    // 加入有實際完成日期檢核點
                    result.AddRange(data.Where(x => x.ACTUAL_ENDDATE.HasValue).ToList());
                    // 進行中的檢核點
                    result.Add(data.Where(x => !x.ACTUAL_ENDDATE.HasValue).First());
                    return result;
                }
                else
                {
                    return data;
                }
            }
            // 工程類資料
            else
            {
                // 找出各指標性檢核點
                int tenderIdx = data.FindIndex(x => x.CTRL_POINT == "F");    // 預算書圖核定/統包需求書核定 Index
                int startWorkIdx = data.FindIndex(x => x.CTRL_POINT == "A"); // 開工 Index
                int endWorkIdx = data.FindIndex(x => x.CTRL_POINT == "B");   // 竣工 Index

                if (startWorkIdx > 0 && endWorkIdx > startWorkIdx)
                {
                    // 各階段檢核點
                    Dictionary<string, ProjectScheOverviewModel> eachChkPtDicionary = new Dictionary<string, ProjectScheOverviewModel>()
                    {
                        {"設計", data[tenderIdx-1]},      // 設計階段 : 計畫開始日期至「預算書圖核定/統包需求書核定」
                        {"招標", data[startWorkIdx]},     // 招標階段 : 「預算書圖核定/統包需求書核定」至 開工
                        {"施工", data[endWorkIdx]},       // 施工階段 : 開工至竣工
                        {"驗收", data.Last()}             // 驗收階段 : 竣工至最後一項檢核點
                    };

                    // 設計階段檢核點
                    foreach (KeyValuePair<string, ProjectScheOverviewModel> schedule in eachChkPtDicionary)
                    {
                        if (schedule.Value.ACTUAL_ENDDATE != null)
                        {
                            schedule.Value.CHECKITEM_NAME = schedule.Key;
                            result.Add(schedule.Value);
                        }
                        else
                        {
                            // 找到進行中檢核點
                            ProjectScheOverviewModel inProgressChkpt = data.Where(x => !x.ACTUAL_ENDDATE.HasValue).First();
                            if(inProgressChkpt.ESTIMATED_ENDDATE < DateTime.Now.Date)// 落後
                            {
                                schedule.Value.CHECKITEM_NAME = $"{schedule.Key}尚未完成「{inProgressChkpt.CHECKITEM_NAME}」";
                                schedule.Value.ESTIMATED_ENDDATE = inProgressChkpt.ESTIMATED_ENDDATE;
                            }
                            result.Add(schedule.Value);
                            return result;// 直接回傳
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 取得最後一筆有實際完成日期的檢核點
        /// </summary>
        /// <param name="listData"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private ProjectScheOverviewModel FindLastCompleteCheckItem(List<ProjectScheOverviewModel> listData, ProjectScheOverviewModel item)
        {
            int itemIdx = listData.IndexOf(item);
            if(item.ACTUAL_ENDDATE != null || itemIdx == 0)
            {
                return item;
            }
            else
            {
                // 往前一個檢核點項目找
                return FindLastCompleteCheckItem(listData, listData[itemIdx - 1]);
            }
        }

        protected override void MakeContent()
        {
            CellReplaceByExcel(new
            {
                Date = DateTime.Now.ToTwDateString(),
                ProjectNo = projectBasicData.PROJECT_NO,
                ProjectName = projectBasicData.PROJECT_NAME,
                ExecOrganName = projectBasicData.EXEC_ORGAN_NAME,
                BudgetTotal = projectBasicData.BUDGET_TOTAL.ToString("N0")
            });

            // 計算所有
            (DateTime? rangeStDate, DateTime? rangeEndDate) = GetHeaderRange();
            if (rangeStDate != null && rangeEndDate != null)
            {
                // 設定表頭
                SetHeader(rangeStDate, rangeEndDate);
                // 設定期程資料
                SetScheduleData();
            }

            // 處理基本欄水平合併
            for (int rowNum = 0; rowNum < 6; rowNum++)
                Sheet.Cells.Merge(rowNum, 0, 1, maxColumnNum);

            // 設定框線
            SetBorder();
            // 產生圖例
            GenerateChartLegend();
        }

        /// <summary>
        /// 設定資料
        /// </summary>
        private void SetScheduleData()
        {
            // 立案期程
            if (model.ScheTypes.Contains("1") && projectOriginSchedule.Any())
            {
                List<Color> colorSchema = checkptColorSchema["History"];
                SetScheData(projectOriginSchedule, "立案期程", ref Seq, colorSchema);
                SetChartLegendData("歷次", colorSchema, projectOriginSchedule);
            }

            // 期程調整歷程(分月/總期程)
            if (model.ScheTypes.Contains("2") && projectAdjustScheHisList.Any())
            {
                List<string> projectAdjIds = projectAdjustScheHisList.Select(x => x.PROJ_ADJ_ID).Distinct().ToList();
                List<Color> colorSchema = checkptColorSchema["History"];
                int scheTotalSeq = 0, scheEachSeq = 0;// 第N次總期程調整、分月期程調整
                foreach (string adjId in projectAdjIds)
                {
                    List<ProjectScheOverviewModel> projectAdjustScheHis = projectAdjustScheHisList.Where(x => x.PROJ_ADJ_ID == adjId).ToList();
                    // 期程調整類別
                    string scheType = projectAdjustScheHis.First().SCHE_TYPE;
                    string itemName = "第{0}次\n{1}期程調整";
                    // 計算第N次總期程/分月期程調整
                    if (scheType == "M")
                    {
                        scheEachSeq++;
                        itemName = string.Format(itemName, scheEachSeq, "分月");
                    }
                    else if (scheType == "Y")
                    {
                        scheTotalSeq++;
                        itemName = string.Format(itemName, scheTotalSeq, "總");
                    }
                    SetScheData(projectAdjustScheHis, itemName, ref Seq, colorSchema);
                    // 調整期程圖例僅需記錄一次且與立案期程圖例資料共用
                    if (!chartLegendData.Any())
                        SetChartLegendData("歷次", colorSchema, projectAdjustScheHis);
                }
            }

            // 現行預定進度
            if (model.ScheTypes.Contains("3") && projectEstimatedSchedule.Any())
            {
                List<Color> colorSchema = checkptColorSchema["Estimated"];
                SetScheData(projectEstimatedSchedule, "現行預定進度", ref Seq, colorSchema);
                SetChartLegendData("現行",colorSchema, projectEstimatedSchedule);
            }

            // 本次申請期程調整
            if (model.ScheTypes.Contains("4") && projectAdjustSchedule.Any())
            {
                List<Color> colorSchema = checkptColorSchema["Apply"];
                string scheTypeStr = projectAdjustSchedule.First().SCHE_TYPE == "Y" ? "總" : "分月";
                SetScheData(projectAdjustSchedule, $"本次申請{scheTypeStr}期程調整", ref Seq, colorSchema);
                SetChartLegendData("本次申請調整之", colorSchema, projectAdjustSchedule);
            }

            //  實際執行進度
            if (model.ScheTypes.Contains("5") && projectActualSchedule.Any())
            {
                List<Color> colorSchema = checkptColorSchema["Actual"];
                List<ProjectScheOverviewModel> actualSchedule = projectActualSchedule.Select(x => new ProjectScheOverviewModel()
                {
                    ESTIMATED_ENDDATE = x.ESTIMATED_ENDDATE,
                    ACTUAL_ENDDATE = x.ACTUAL_ENDDATE,
                    CONTROL_DATE1 = x.CONTROL_DATE1,
                    ItemDateStr = x.ItemDateStr,
                    CHECKITEM_NAME = x.CHECKITEM_NAME,
                    CP_KIND= x.CP_KIND
                }).ToList();

                SetScheData(actualSchedule, "實際執行進度", ref Seq,colorSchema,true);
                // 實際執行進度圖例資料
                SetActualChkptChartLegendData(actualSchedule);
            }
        }

        /// <summary>
        /// 取得期程表頭起訖
        /// </summary>
        /// <returns></returns>
        private (DateTime? rangeStDate, DateTime? rangeEndDate) GetHeaderRange()
        {
            DateTime? rangeSt = null, rangeEnd = null;
            // 所有期程資料
            List<ProjectScheOverviewModel> allScheduleData = new();
            allScheduleData.AddRange(projectOriginSchedule); // 立案期程
            allScheduleData.AddRange(projectAdjustScheHisList);// 調整歷程
            allScheduleData.AddRange(projectAdjustSchedule);   // 申請中調整
            //預定期程
            allScheduleData.AddRange(projectEstimatedSchedule); 
            // 實際期程
            allScheduleData.AddRange(projectActualSchedule.Select(x=> new ProjectScheOverviewModel 
                { CONTROL_DATE1 = x.CONTROL_DATE1, ESTIMATED_ENDDATE = x.ACTUAL_ENDDATE}).ToList());    

            // 找出所有期程中最早及最晚日期
            if (allScheduleData.Any())
            {
                rangeSt = allScheduleData.Where(x => x.CONTROL_DATE1.HasValue)
                    .OrderBy(x => x.CONTROL_DATE1.Value).First().CONTROL_DATE1.Value;
                rangeEnd = allScheduleData.Where(x=>x.ESTIMATED_ENDDATE.HasValue)
                    .OrderBy(x=>x.ESTIMATED_ENDDATE.Value).Last().ESTIMATED_ENDDATE.Value;

                rangeSt = rangeSt.Value >= DateTime.Now.Date ? DateTime.Now : rangeSt.Value;
                rangeEnd = rangeEnd.Value <= DateTime.Now.Date ? DateTime.Now : rangeEnd.Value;
            }
            return (rangeSt,rangeEnd);
        }

        /// <summary>
        /// 設定表頭
        /// </summary>
        /// <param name="rangeSt"></param>
        /// <param name="rangeEnd"></param>
        private void SetHeader(DateTime? rangeSt, DateTime? rangeEnd)
        {
            if(rangeSt == null || rangeEnd == null) return;

            // 總月數
            int headerMonthRange = GetMonthDiff(rangeSt.Value, rangeEnd.Value);

            // 若總月數整除3 且 起始月不在各季的第一個月(1,4,7,10)時，需補上完整一個季
            if (headerMonthRange % 3 == 0 && (rangeSt.Value.Month - 1) % 3 != 0)
                headerMonthRange += 3;

            // 月數範圍必須為完整的一季
            while(headerMonthRange % 3 != 0)
                headerMonthRange++;
            // 取得總欄數
            maxColumnNum = baseColumnNum + headerMonthRange;
            this.headerMonthRange = headerMonthRange;
            // 表頭日期
            DateTime headerDate = rangeSt.Value;
            // 起始月必須為1,4,7,10 月(各季起始月)
            while ((headerDate.Month - 1) % 3 != 0)
                headerDate = headerDate.AddMonths(-1);
            Cells cells = Sheet.Cells;
            // 年、季表頭起始欄、列號
            int headerStartColNum = baseColumnNum + 1;// 基本欄的下一欄開始
            int headerYearRowNum = 7, headerMonthRowNum = 8 ;

            //DateTime headerDate = rangeSt.Value;
            int mergeYear = 0; // 合併年
            // 設定資料
            for (int i = 1;i <= headerMonthRange; i++)
            {
                int? yearHMergeCnt = null; // 年水平合併欄位數
                int? monthHMergeCnt = null; // 月水平合併欄位數
                // 第幾季
                string quarter = $"Q{Math.Ceiling((decimal)headerDate.Month / 3)}";
                // 僅第1,4,7,10月須水平合併3欄
                if(headerDate.Month % 3 == 1)
                    monthHMergeCnt = 3; 

                // 計算年表頭水平合併數
                if(mergeYear!= headerDate.Year)
                {
                    // 若當前月離最後日期超過12個月
                    // 或 小於12個月且跨年度的情況下
                    if( headerMonthRange - i >= 12 ||
                        (headerMonthRange - i < 12 && headerDate.Year != rangeEnd.Value.Year))
                    {
                        // 因起始月為1月則須水平合併12格儲存格
                        // 因起始月為4月則須水平合併9格儲存格以此類推
                        // 而兩者加總最終會為13，故用此值計算年水平合併儲存格數
                        yearHMergeCnt = 13 - headerDate.Month;
                    }
                    // 當前月離結束日不滿12個月
                    else
                    {
                        yearHMergeCnt = headerMonthRange - i + 1;
                    }
                    mergeYear = headerDate.Year;
                }
                // 欄號
                string columnName = GetEnColumn(headerStartColNum);

                cellsNeedToSetBorder.Add(SetColumn(cells[$"{columnName}{headerYearRowNum}"],
                    headerDate.Year-1911, hMergeCnt: yearHMergeCnt)) ;
                cellsNeedToSetBorder.Add(SetColumn(cells[$"{columnName}{headerMonthRowNum}"],
                    quarter, hMergeCnt: monthHMergeCnt));

                scheduleHeaderMappingColumn.Add($"{headerDate.ToTwDateString("yyy/MM")}", headerStartColNum);
                headerDate = headerDate.AddMonths(1);
                headerStartColNum += 1;
            }
        }

        /// <summary>
        /// 設定期程資料
        /// </summary>
        /// <param name="models"></param>
        /// <param name="itemName"></param>
        /// <param name="seq"></param>
        /// <param name="colorSchema"></param>
        private void SetScheData(List<ProjectScheOverviewModel> models, string itemName, ref int seq, List<Color> colorSchema,bool isActualChkpt = false)
        {
            // 列號
            int rn = baseRowNum + seq;
            Cells cells = Sheet.Cells;
            // 序號、項目、時間
            SetColumn(cells[$"A{rn}"], seq);
            SetColumn(cells[$"B{rn}"], itemName, hMergeCnt: 4);
            SetColumn(cells[$"F{rn}"], models.FirstOrDefault()?.ItemDateStr ?? "", hMergeCnt: 3);

            // 基本欄位欄外框(序號、項目、時間)
            for (int i = 1; i <= baseColumnNum; i++)
                cellsNeedToSetBorder.Add(cells[$"{GetEnColumn(i)}{rn}"]);

            // 檢核點開始日期(前一項檢核點預定完成日期的隔月，第一項則為計畫開始日期)
            DateTime checkpointSt = models.First().CONTROL_DATE1.Value;
            int checkItemIdx = 0;
            foreach (ProjectScheOverviewModel model in models)
            {
                DateTime? estimatedEndDate = model.ESTIMATED_ENDDATE;
                if (estimatedEndDate.HasValue)
                {
                    // 背景色
                    Color bgColor = checkItemIdx >= colorSchema.Count ? GetRandomColor() : colorSchema[checkItemIdx];

                    // 實際執行進度是否落後
                    if (isActualChkpt && !model.ACTUAL_ENDDATE.HasValue)
                    {
                        bool isDelay = estimatedEndDate < DateTime.Now.Date;
                        estimatedEndDate = DateTime.Now.Date;
                        if(isDelay)
                            bgColor = delayColor;
                    }

                    // 檢核點跨月數
                    int checkpointMonthRange = GetMonthDiff(checkpointSt, estimatedEndDate.Value);
                    // 檢核點起始欄
                    string chkptStColumn = scheduleHeaderMappingColumn.TryGetValue(checkpointSt.ToTwDateString("yyy/MM"), out int chkptStColNum)
                        ? GetEnColumn(chkptStColNum) : null ;
                    // 檢核點結束欄
                    string chkptEndColumn = GetEnColumn(chkptStColNum + checkpointMonthRange-1);

                    if (chkptStColumn!=null)
                    {
                       Cell cell = cells[$"{chkptStColumn}{rn}"];
                        SetColumn(cell, model.CHECKITEM_NAME);

                        Aspose.Cells.Range range = Sheet.Cells.CreateRange($"{chkptStColumn}{rn}", $"{chkptEndColumn}{rn}");
                        Style style = SetBgColor(cell, bgColor);
                        style.HorizontalAlignment = TextAlignmentType.Left;
                        range.SetStyle(style);

                        // 下一項檢核點開始日期為前一項的預定完成日期隔月
                        checkpointSt = estimatedEndDate.Value.AddMonths(1);
                        checkItemIdx++;
                    }
                }
            }
            seq++;
        }

        /// <summary>
        /// 設定CellBorder 
        /// </summary>
        /// <param name="cell"></param>
        private void SetCellBorder(Cell cell)
        {
            WorkbookBuilder.SetBorder(cell, defaultBorderType, Color.Black);
        }

        /// <summary>
        /// 設定期程資料外框
        /// </summary>
        private void SetRangeBorder()
        {
            for(int rowNum = baseRowNum + 1; rowNum < baseRowNum + Seq; rowNum++)
            {
                for(int colNum = baseColumnNum + 1; colNum < baseColumnNum + headerMonthRange; colNum+=3)
                {
                    Aspose.Cells.Range range = Sheet.Cells.CreateRange(
                        $"{GetEnColumn(colNum)}{rowNum}", $"{GetEnColumn(colNum+2)}{rowNum}");
                    range.SetOutlineBorders(defaultBorderType, Color.Black);
                }
            }
        }

        /// <summary>
        /// 取得月數差異
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private int GetMonthDiff(DateTime start, DateTime end)
        {
            int monthDiff = ((end.Year - start.Year) * 12) + end.Month - start.Month + 1;
            return monthDiff <=0 ? 1: monthDiff;
        }

        /// <summary>
        /// 依據落後案件設定文字顏色
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private static Style SetBgColor(Cell cell,Color color = default)
        {
            Style style = cell.GetStyle();
            style.ForegroundColor = color.IsEmpty? GetRandomColor():color;
            style.Pattern = BackgroundType.Solid;
            return style;
        }

        /// <summary>
        /// 取得隨機顏色
        /// </summary>
        /// <returns></returns>
        private static Color GetRandomColor()
        {
            List<KnownColor> colorList = Enum.GetValues(typeof(KnownColor))
                .Cast<KnownColor>()
                .Where(clr => !Color.FromKnownColor(clr).IsSystemColor)
                .ToList();

            Random rand = new Random(DateTime.Now.Ticks.GetHashCode());
            int randNum = rand.Next(colorList.Count);
            return Color.FromKnownColor(colorList[randNum]);
        }

        /// <summary>
        /// 設定外框
        /// </summary>
        private void SetBorder()
        {
            // 基本欄位外框
            foreach (Cell cell in cellsNeedToSetBorder)
                SetCellBorder(cell);

            // 設定期程資料外框
            SetRangeBorder();
        }

        /// <summary>
        /// 檢查是否為工程類
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool isEngType (ProjectScheOverviewModel model)
        {
            return model.CP_KIND == "0";
        }

        /// <summary>
        /// 設定圖例資料
        /// </summary>
        /// <param name="type"></param>
        /// <param name="colorSchema"></param>
        /// <param name="schedules"></param>
        private void SetChartLegendData(string type, List<Color> colorSchema, List<ProjectScheOverviewModel> schedules)
        {
            List<ScheOverviewChartLegendModel> eachscheduleCharts = new();
            int idx = 0;
            foreach(ProjectScheOverviewModel schedule in schedules)
            {
                // 工程類 : XXX 階段 ; 非工程類 : XXX 期程
                string titleTail = isEngType(schedule)? "階段":"期程";
                eachscheduleCharts.Add(new ScheOverviewChartLegendModel() 
                { 
                    Color = idx >= colorSchema.Count ? GetRandomColor() : colorSchema[idx],
                    Title = colorSchema[idx] == delayColor ? 
                        schedule.CHECKITEM_NAME: $"{type}「{schedule.CHECKITEM_NAME}」{titleTail}"
                });
                idx++;
            }
            chartLegendData.Add(eachscheduleCharts);
        }

        /// <summary>
        /// 設定實際完成日期圖例資料
        /// </summary>
        /// <param name="actualSchedules"></param>
        private void SetActualChkptChartLegendData(List<ProjectScheOverviewModel> actualSchedules)
        {
            List<ProjectScheOverviewModel> chartLegendData = actualSchedules.Select(x => new ProjectScheOverviewModel()
            {
                ESTIMATED_ENDDATE = x.ESTIMATED_ENDDATE,
                ACTUAL_ENDDATE = x.ACTUAL_ENDDATE,
                CHECKITEM_NAME = x.CHECKITEM_NAME,
                CP_KIND = x.CP_KIND
            }).ToList();

            List<Color> actualChkptColorSchema = checkptColorSchema["Actual"];
            // 有檢核點未完成 (僅顯示已完成 & 進行中檢核點 圖例 且有落後須改為紅色)
            if (actualSchedules.Any(x => !x.ACTUAL_ENDDATE.HasValue))
            {
                // 已完成檢核點
                List<ProjectScheOverviewModel> finishedSchedules = chartLegendData.Where(x => x.ACTUAL_ENDDATE.HasValue).ToList();
                // 進行中檢核點
                ProjectScheOverviewModel inProgressSchedule = chartLegendData.First(x => !x.ACTUAL_ENDDATE.HasValue);
                // 落後 
                if(inProgressSchedule.ESTIMATED_ENDDATE < DateTime.Now.Date)
                {
                    actualChkptColorSchema.Insert(finishedSchedules.Count, delayColor);
                    inProgressSchedule.CHECKITEM_NAME = "落後階段";
                }
                chartLegendData.Clear();
                chartLegendData.AddRange(finishedSchedules);
                chartLegendData.Add(inProgressSchedule);
            }
            SetChartLegendData("實際", actualChkptColorSchema, chartLegendData);
        } 

        /// <summary>
        /// 圖表
        /// </summary>
        private void GenerateChartLegend()
        {
            // 圖例起始Row在檢核點資料下兩列
            int startRow = baseRowNum + Seq + 1;
            int rn = startRow; 

            Cell descCell = Sheet.Cells[$"A{startRow}"];
            Style textStyle = descCell.GetStyle();
            textStyle.HorizontalAlignment = TextAlignmentType.Left;
            SetColumn(descCell, "圖例說明如下：",style:textStyle);
            foreach(List<ScheOverviewChartLegendModel> eachschduleTypeCharts in chartLegendData)
            {
                rn += 2;
                int colNum = 1;
                
                foreach (ScheOverviewChartLegendModel checkItem in eachschduleTypeCharts)
                {
                    Aspose.Cells.Range range = Sheet.Cells.CreateRange($"{GetEnColumn(colNum)}{rn}", $"{GetEnColumn(colNum+1)}{rn}");
                    // 圖例背景色
                    Style style = SetBgColor(Sheet.Cells[$"{GetEnColumn(colNum)}{rn}"], checkItem.Color);
                    range.SetStyle(style);
                    Cell cell = Sheet.Cells[$"{GetEnColumn(colNum + 2)}{rn}"];
                    SetColumn(cell, checkItem.Title,style: textStyle);
                    colNum += 10; // 各檢核點預設間隔10欄
                }
            }
            // 設定列高
            SetRowHeight(startRow-1, rn, 20);
        }

        /// <summary>
        /// 設定Row Height
        /// </summary>
        /// <param name="startRowIdx"></param>
        /// <param name="endRowIdx"></param>
        /// <param name="rowHeight"></param>
        private void SetRowHeight(int startRowIdx, int endRowIdx, int rowHeight)
        {
            if(startRowIdx >= 0)
            {
                for(int rowIndex = startRowIdx; rowIndex <= endRowIdx; rowIndex++)
                {
                    Sheet.Cells.SetRowHeight(rowIndex,rowHeight);
                }
            }
        }

        /// <summary>
        /// 取得上個填報週期資料
        /// </summary>
        /// <param name="cycleData">所有填報週期資料</param>
        /// <returns></returns>
        private ProjectFillCycleModel GetLastFillCycle(List<ProjectFillCycleModel> cycleData)
        {
            if (cycleData.Any())
            {
                if (DateTime.Now.Date > cycleData.First().FILL_END_DATE)
                    return cycleData.First();
                else 
                    return cycleData.ElementAt(1);
            }

            return new ProjectFillCycleModel();
        }
    }
}

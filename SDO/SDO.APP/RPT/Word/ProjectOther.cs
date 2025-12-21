using Aspose.Words;
using Autofac;
using SDO.Base.RPT.Enums;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 預覽列印 三、執行情形 （五）其它資訊
    /// </summary>
    public class ProjectOther : WContentBuilder
    {
        private readonly IProjectExecuteService projectExecuteService;
        // 其它資訊資料
        ProjectFillOtherModel projectOtherData;
        List<bool> checkList;
        int count = 0;

        public ProjectOther(IComponentContext coms) : base(coms)
        {
            this.projectExecuteService = coms.Resolve<IProjectExecuteService>();
        }

        protected override async Task GetData()
        {
            projectOtherData = await projectExecuteService.GetProjectFillOther(Parameter.PROJECT_NO);
            checkList = (List<bool>)Parameter.ObjectModel;
        }

        protected override void Content()
        {
            Bid();
            Activity();
            Review();
            Tender();
        }

        /// <summary>
        /// 招標情形
        /// </summary>
        private void Bid()
        {
            if (checkList[0])
            {
                SetTitle($"{++count}.招標情形", fontSize: 14);
                foreach (ProjectBidModel item in projectOtherData.ProjectBid)
                {
                    if (projectOtherData.ProjectBidDetail.Where(x => x.BID_KIND == item.BID_KIND).Any() ||
                        item.AWARD_BID_DATE.HasValue ||
                        !string.IsNullOrEmpty(item.BID_TENDER))
                    {
                        Table = Builder.StartTable();
                        SetThColumn(item.BID_NAME, backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center, hMergeCount: 3);
                        Builder.EndRow();

                        SetThColumn("決標日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                        SetTdColumn(item.AWARD_BID_DATE.ToTwDateString(), hMergeCnt: 2);
                        Builder.EndRow();

                        SetThColumn("決標廠商", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                        SetTdColumn(item.BID_TENDER, hMergeCnt: 2);
                        Builder.EndRow();

                        SetThColumn("流標次數", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                        SetThColumn("流標日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                        SetThColumn("流標原因", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                        Builder.EndRow();
                        BidDetail(projectOtherData.ProjectBidDetail.Where(x => x.BID_KIND == item.BID_KIND && x.DETAIL_TYPE == 0).ToList());

                        SetThColumn("廢標次數", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                        SetThColumn("廢標日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                        SetThColumn("廢標原因", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                        Builder.EndRow();
                        BidDetail(projectOtherData.ProjectBidDetail.Where(x => x.BID_KIND == item.BID_KIND && x.DETAIL_TYPE == 1).ToList());
                        Builder.EndTable();
                        InitTable(Table, new List<double> { 15, 20, 65 });
                        Builder.Writeln(string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 流標/廢標歷程
        /// </summary>
        /// <param name="data"></param>
        private void BidDetail(List<ProjectBidDetailModel> data)
        {
            if (!data.Any())
            {
                SetTdColumn("無資料", alignment: AlignmentEnum.Center, hMergeCnt: 3);
                Builder.EndRow();
            }
            int count = 1;
            foreach (ProjectBidDetailModel item in data)
            {
                SetTdColumn(count, alignment: AlignmentEnum.Center);
                SetTdColumn(item.DETAIL_DATE.ToTwDateString(), alignment: AlignmentEnum.Center);
                SetTdColumn(item.DETAIL_REASON);
                Builder.EndRow();
                count++;
            }
        }

        /// <summary>
        /// 相關活動
        /// </summary>
        private void Activity()
        {
            if (checkList[1])
            {
                SetTitle($"{++count}.相關活動", fontSize: 14);
                Table = Builder.StartTable();
                foreach (ProjectActivityModel item in projectOtherData.ProjectActivity)
                {
                    SetThColumn(item.ACTIVITY_KIND_NAME, backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                    if (item.ACTIVITY_KIND == "04" && item.editType == 0)
                    {
                        SetTdColumn("無資料", hMergeCnt: 3);
                        Builder.EndRow();
                    }
                    else
                    {
                        string activity = item.ACTIVITY_KIND == "04"
                            ? item.ACTIVITY_NAME
                            : !item.IS_ACTIVITY.HasValue ? "" : item.IS_ACTIVITY.Value ? "有" : "無";
                        SetTdColumn(activity);
                        SetThColumn("活動日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                        SetTdColumn(item.ACTIVITY_DATE.ToTwDateString());
                        Builder.EndRow();
                    }
                }
                Builder.EndTable();
                InitTable(Table, new List<double> { 30, 35, 15, 20 });
                Builder.Writeln(string.Empty);
            }
        }

        /// <summary>
        /// 相關審查
        /// </summary>
        private void Review()
        {
            if (checkList[2])
            {
                SetTitle($"{++count}.相關審查", fontSize: 14);
                Table = Builder.StartTable();
                foreach (SetParamModel item in projectOtherData.SetParam)
                {
                    foreach (ProjectReviewModel review in projectOtherData.ProjectReview.Where(x => x.REVIEW_KIND == item.SET_TYPE).ToList())
                    {
                        SetThColumn(item.SET_VALUE, backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                        string value = review.REVIEW_KIND != "04"
                                   ? !review.IS_REVIEW.HasValue ? "" : review.IS_REVIEW.Value ? "有" : "無"
                                   : review.OTH_RVWNAME;
                        SetTdColumn(value);
                        SetThColumn("送件日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                        SetTdColumn(review.SEND_DATE.ToTwDateString());
                        SetThColumn("核定日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                        SetTdColumn(review.REVIEW_DATE.ToTwDateString());
                        Builder.EndRow();
                    }
                    if (item.SET_TYPE == "04" && !projectOtherData.ProjectReview.Where(x => x.REVIEW_KIND == "04").ToList().Any())
                    {
                        SetThColumn(item.SET_VALUE, backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                        SetTdColumn("無資料", hMergeCnt: 5);
                        Builder.EndRow();
                    }
                }
                Builder.EndTable();
                InitTable(Table, new List<double> { 30, 14, 14, 14, 14, 14 });
                Builder.Writeln(string.Empty);
            }
        }

        /// <summary>
        /// 廠商資訊
        /// </summary>
        private void Tender()
        {
            if (checkList[3])
            {
                SetTitle($"{++count}.廠商資訊", fontSize: 14);
                Table = Builder.StartTable();
                SetThColumn("廠商類別", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                SetThColumn("廠商名稱", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                SetThColumn("統編", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                SetThColumn("地址", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                SetThColumn("聯絡人", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                SetThColumn("聯絡人電話", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                SetThColumn("聯絡人Email", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
                Builder.EndRow();

                foreach (ProjectTenderModel item in projectOtherData.ProjectTender)
                {
                    SetTdColumn(item.TENDER_KIND_NAME);
                    SetTdColumn(item.TENDER_NAME);
                    SetTdColumn(item.REG_NO);
                    SetTdColumn(item.TENDER_ADDR);
                    SetTdColumn(item.CONTACT_NAME);
                    SetTdColumn(item.CONTACT_PHONE);
                    SetTdColumn(item.CONTACT_EMAIL);
                    Builder.EndRow();
                }
                Builder.EndTable();
                InitTable(Table, new List<double> { 12, 16, 12, 22, 9, 14, 15 });
            }
        }
    }
}

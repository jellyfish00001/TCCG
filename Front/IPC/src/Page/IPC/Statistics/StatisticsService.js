import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { GetSetParam, getPlanYearList, getOrganList, getOrgByUsr } from "../../../Basic/CommonService";
import { FormatDate } from '../../../Basic/SDOExtension';
import { Download } from "../../../Basic/Download";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 下拉選單初始值
 */
const initDdlData = {
    PLAN_YEAR: [],
    MASTER_ORGAN: [],
    EXEC_ORGAN: [],
    ASS_ORGAN: [],
    AGCY_ORGAN: [],
    SPEC_NOTE: [],
    Month: [],
    PROJECT_YEAR_STATUS: [],
    TUBE_STATUS: [],
    EXEC_RATE: [],
    DELAY_TYPE: [],
    CTRL_CHK_POINT_TYPE: [],
    SORT_TYPE_1: [],
    SORT_TYPE_2: [],
    SORT_TYPE_3: []
}

/**
 * 報表清單
 * @returns 
 */
const getStatisticsList = () => {
    const result = [
        { ID: 1, NAME: "每月案件統計表", MEMO: "查詢案件已結案、進度符合或超前、進度落後、撤銷列管等狀態之件數及清單。" },
        { ID: 2, NAME: "每月案件地區統計表", MEMO: "以「行政區」方式呈現案件列管狀態、開竣工及執行情形等資料明細。" },
        { ID: 3, NAME: "每月案件機關統計表", MEMO: "以「機關別」方式呈現案件列管狀態、開竣工及執行情形等資料明細。" },
        { ID: 4, NAME: "每月案件連續落後統計表", MEMO: "查詢落後案件之件數、清單及連續落後狀態。" },
        { ID: 5, NAME: "每月未完成進度填報清單", MEMO: "查詢每月進度未完成填報清單。" },
        { ID: 6, NAME: "檢核點屆期預告", MEMO: "查詢檢核點當月、次月屆期之件數及明細。" },
        { ID: 7, NAME: "特定工作項目屆期情形", MEMO: "查詢工程決標、開竣工、驗收等6項特定工作項目當月、次月屆期，且尚未完成之件數及明細。" },
        { ID: 8, NAME: "落後案件特定工作項目逾期情形", MEMO: "查詢工程決標、開竣工、驗收等6項特定工作項目進度落後之之件數及明細。" },
        { ID: 9, NAME: "預算執行情形明細表", MEMO: "可匯出列管案件預算執行明細（含執行率未達80%之責任歸屬及原因等）。" },
        { ID: 10, NAME: "選項列管案件計畫歷次調整審查表", MEMO: "查詢案件期程調整歷程資料，簡版僅供工程類計畫查詢。" },
        { ID: 11, NAME: "年終考核案件成績表", MEMO: "查詢年終考核項目權重及明細。" },
        { ID: 12, NAME: "平時管考意見備註統計表", MEMO: "平時管考意見備註統計表" },
        { ID: 13, NAME: "重大建設系統介接公共工程雲雲端服務網資料統計表", MEMO: "重大建設系統介接公共工程雲雲端服務網資料統計表" },
    ]
    return result;
}

/**
 * 下拉選單
 * @returns 
 */
const getAllDropDowns = async () => {
    let responses = await Promise.all([
        // 年度
        getPlanYearList(true),
        // 取得機關
        getOrganList(),
        // 特殊加註
        GetSetParam("SPEC_NOTE", "", true),
        // 執行落後類型
        GetSetParam("DELAY_CLASS", "", true),
        // 列管狀態
        GetSetParam("TUBE_STATUS", "", true),
        // 執行率
        GetSetParam("EXEC_RATE", "", true),
        // 工作項目
        GetSetParam("CTRL_CHK_POINT_TYPE", "", true),
        // 取得登入者的機關
        getOrgByUsr()
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));
    result[1].unshift({ text: "請選擇", value: "" });
    result[4].unshift({ SET_VALUE: "請選擇", SET_TYPE: "" });
    result[7].unshift({ text: "請選擇", value: "" });
    return result;
}

/**
 * 組下拉選單資料
 * @returns 
 */
const getPageDropData = async (checkIsHANDRole) => {
    const dropDowns = await getAllDropDowns();
    if (dropDowns.length <= 0) {
        return initDdlData;
    }

    let organs = (checkIsHANDRole ? dropDowns[7] : dropDowns[1]);
    return {
        // 年度
        PLAN_YEAR: [...dropDowns[0]],
        // 主管機關 MASTER_DEPT
        MASTER_ORGAN: [...organs],
        // 執行機關 EXEC_DEPT
        EXEC_ORGAN: [...organs],
        // 協辦機關 ASS_DEPT
        ASS_ORGAN: [...organs],
        // 代辦機關 AGCY_DEPT
        AGCY_ORGAN: [...organs],
        // 特殊加註
        SPEC_NOTE: [...dropDowns[2]],
        // 月份
        Month: getMonthList(),
        PROJECT_YEAR_STATUS: [
            { value: "A", text: "含之前所有案件" },
            { value: "B", text: "含之前未結案件" }
        ],
        // 列管狀態
        TUBE_STATUS: [...dropDowns[4]],
        // 執行率
        EXEC_RATE: [...dropDowns[5]],
        // 落後類別
        DELAY_TYPE: [...dropDowns[3]],
        CTRL_CHK_POINT_TYPE: [...dropDowns[6]],
        SORT_TYPE_1: [
            { text: "落後件數", value: "A" },
            { text: "依執行機關", value: "B" },
            { text: "落後比率", value: "C" },
            { text: "綜合排序(落後件數及落後比率)", value: "D" },
        ],
        SORT_TYPE_2: [
            { label: "依地區", value: "A" },
            { label: "列管件數", value: "C" },
            { label: "總金額", value: "D" }
        ],
        SORT_TYPE_3: [
            { label: "依機關群組", value: "B" },
            { label: "列管件數", value: "C" },
            { label: "總金額", value: "D" }
        ]
    };
}

/**
 * 月份下拉
 * @returns 
 */
const getMonthList = () => {
    let result = [];
    for (let i = 1; i <= 12; i++) {
        result.push({ text: i, value: i });
    }
    return result;
}

// 紀錄上個月日期
let lastMonDate = new Date();
lastMonDate.setMonth(lastMonDate.getMonth() - 1, 1);

/**
 * 查詢條件初始值
 */
const initData = {
    STATISTICS_ID: "",
    STATISTICS_NAME: "",
    // 計畫年度
    PROJECT_YEAR: FormatDate(new Date(), "tYY"),
    // 計畫年度-結束
    PROJECT_YEAR_E: FormatDate(new Date(), "tYY"),
    // 含之前所有案件、含之前未結案件
    PROJECT_YEAR_STATUS: "B",
    // 統計年月 (預設上月)
    STATISTICS_YEAR: FormatDate(lastMonDate, "tYY"),
    STATISTICS_MONTH: lastMonDate.getMonth() + 1,
    YEAR_MONTH: null,
    // 結案年月
    CLOSE_S_YEAR: FormatDate(new Date(), "tYY"),
    CLOSE_S_MONTH: 1,
    CLOSE_YM_S: null,
    CLOSE_E_YEAR: FormatDate(new Date(), "tYY"),
    CLOSE_E_MONTH: 12,
    CLOSE_YM_E: null,
    // 主管機關
    MASTER_DEPT: "",
    // 執行機關
    EXEC_DEPT: "",
    // 協辦機關
    ASS_DEPT: "",
    // 代辦機關
    AGCY_DEPT: "",
    // 列管狀態
    TUBE_STATUS: "",
    // 特殊加註
    SPEC_NOTE: [],
    // 計畫總經費(元)
    PROJECT_EXS_S: null,
    PROJECT_EXS_E: null,
    // 執行率(%)
    EXEC_RATE: "S2",
    // 執行落後類型
    DELAY_TYPE: [],
    // 計畫編號
    PROJECT_NO: "",
    // 工作項目
    CTRL_CHK_POINT_TYPE: [],
    // 排序
    SORT_TYPE_1: "D",
    SORT_GROUPBY_DEPT: false,
    SORT_TYPE_2: "A",
    SORT_TYPE_3: "B"
}

/**
 * 下載報表
 * @param {string} data 資料
 */
const exportRPT = async (data) => {
    let url = APIUrl + 'RPT/RPTStatistics';
    Download(url, 'POST', data);
}

/**
 * 取得工程類計畫清單
 * @returns 
 */
const getEngineeringProjects = async () => {
    let url = APIUrl + 'RPT/getEngineeringProjects';
    let response = await api.Post(url);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

const StatisticsService = {
    initDdlData,
    getStatisticsList: getStatisticsList,
    getAllDropDowns: getAllDropDowns,
    getPageData: getPageDropData,
    initData: initData,
    exportRPT,
    getEngineeringProjects
}

export default StatisticsService;

import { api } from "../../../Basic/ApiFetch";
import { getMonthList, getOrganList, getPlanYearList, getCodeTownByCityId, GetParamByItems, getOrgByUsr } from "../../../Basic/CommonService";
import { FormatDate } from "../../../Basic/SDOExtension";
import { Download } from "../../../Basic/Download";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";

let APIUrl = getGlobalServerConfig().backEndUrl.get();
const year = FormatDate(new Date(), "tYY");
// 紀錄上個月日期
let lastMonDate = new Date();
lastMonDate.setMonth(lastMonDate.getMonth() - 1, 1);

/**
 * 查詢條件初始值
 */
export const getQueryCondition = (isHand, orgId) => {
    let data = {
        PIS_SELECT: "",
        PROJECT_YEAR_S: year,
        PROJECT_YEAR_E: year,
        PROJECT_YEAR_STATUS: "B",
        // 統計年月 (預設上月)
        STATISTICS_YEAR: FormatDate(lastMonDate, "tYY"),
        STATISTICS_MONTH: lastMonDate.getMonth() + 1,
        MASTER_DEPT: "",
        EXEC_DEPT: isHand ? orgId : "",
        ASS_DEPT: "",
        AGCY_DEPT: "",
        PROJECT_EXS_S: null,
        PROJECT_EXS_E: null,
        AREA: "",
        BUILD_KIND: "", //建設類別
        MAIN_BUILD: "",//主要建設
        SUB_BUILD: "",//附屬設施
        REVIEWITEM: "",
        SPEC_NOTE: "",
        CONFERENCE_GENRE: "",
        CREATEDTIME_S: null,
        CREATEDTIME_E: null,
        TUBE_STATUS: "",
        MERGE_STATUS: "",
        CP_KIND: "",//執行類別
        RUNWAY_C: "",//執行方式 
        COM_IPCMEMO: "", //平時管考意見備註
        EXEC_STAGE: "",
        CTRL_POINT: "A",
        COMPLETED_START: null,
        COMPLETED_END: null,
        IPC_ACT_PRG_STAR: null,
        IPC_ACT_PRG_END: null,
        DELAY_TYPE: "",
        DELAY_DAY_START: null,
        DELAY_DAY_END: null,
        DELAY_PRG: null,
        COMPARE_PCC_INFO: [], //比對標案系統-比對資訊
        COMPARE_PCC_CONDITION: "", //比對標案系統-比對條件
        ALERT_TYPE_1: "", //預警類型1
        ALERT_TYPE_2_INFO: "", //預警類型2-比對資訊
        ALERT_TYPE_2_CONDITION: "", //預警類型2-比對條件
        AUDIT_OPINION: "",//管考意見
        PROJECT_NO: "",
        PROJECT_NAME: ""
    }
    return data;
}

/**
 * 取得自選欄位
 * @param {*} isRdec 
 * @returns 
 */
export const getOptionColumns = async (isRdec) => {
    let url = APIUrl + 'Statistics/GetOptionColumns';
    let response = await api.Post(url, JSON.stringify(isRdec), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 下拉選單
 * @param {*} isHand 
 * @returns 
 */
export const getAllDropDowns = async (isHand) => {
    const setItems = {
        "COM_PLANKIND": false,               //建設類別
        "COM_REVIEWITEM": false,             //相關審查
        "SPEC_NOTE": false,                  //特殊加註
        "COM_CONFERENCEGENRE": false,        //會議種類
        "TUBE_STATUS": true,                 //列管狀態
        "PROMERGESTATUS": true,              //分案或併案
        "CP_KIND": true,                     //執行類別
        "EXEC_STAGE": true,                  //執行階段
        "DELAY_CLASS": false,                //落後類型
        "CMP_COND_1": false,                 //比對標案系統-比對資訊
        "CMP_COND_2": false,                 //比對標案系統-比對條件
        "CMP_COND_3": false,                 //預警類型1
        "CTRL_CHK_POINT_TYPE": false,        //預警類型2-比對資訊
        "CMP_COND_4": false,                 //預警類型2-比對條件
        "COM_IPCMEMO": true                  //平時管考意見備註
    };
    let responses = await Promise.all([
        // 計畫年度0
        getPlanYearList(true),
        // 機關，主辦權限只能出現自己機關，管考權限才可出現全部機關
        isHand ? getOrgByUsr() : getOrganList(),
        // 地區別2 
        getCodeTownByCityId("H", "", true),
        // 取得多組system param
        GetParamByItems(setItems, true)
    ])

    //responses轉Json
    let responsesJson = responses.map(res => res.json());

    let dropDowns = []
    dropDowns = await (Promise.all(responsesJson));
    if (dropDowns.length <= 0) {
        return [];
    }

    dropDowns[1].unshift({ text: "請選擇", value: "" });
    dropDowns[2].unshift({ text: "請選擇", value: "" });

    let result = dropDowns[3] ?? [];
    let key = ["PROJECT_YEAR", "ORGAN", "AREA"]
    key.forEach((x, i) => {
        result[x] = [...dropDowns[i]];
    })
    // 統計年月
    result["MONTH"] = getMonthList();

    // 控制點(開/竣工)
    result["CTRL_POINT"] = getCtrlPointList();

    result["PROJECT_YEAR_STATUS"] = [
        { value: "A", text: "含之前所有案件" },
        { value: "B", text: "含之前未結案件" }
    ];
    return result;
}

/**
 * 控制點(開/竣工)
 * @returns 
 */
export const getCtrlPointList = () => {
    let result = [];
    result.push({ SET_VALUE: "預定開工", SET_TYPE: "A" });
    result.push({ SET_VALUE: "預定竣工", SET_TYPE: "B" });
    return result;
}

/**
 * 取得綜合查詢結果
 * @param {*} condition
 * @returns 
 */
export const getUnitingQuery = async (condition) => {
    let url = APIUrl + 'Statistics/GetUnitingQuery';

    let response = await api.Post(url, JSON.stringify(condition), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 下載報表
 * @param {string} data 資料
 */
export const exportRPT = async (data) => {
    let url = APIUrl + 'RPT/RPTUnitingQuery';
    Download(url, 'POST', data);
}


import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { GetSetParam, getPlanYearList, getOrganList, getOrgByUsr } from "../../../Basic/CommonService";
import { FormatDate } from '../../../Basic/SDOExtension';
import { Download } from "../../../Basic/Download";

const APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得報表清單
 * @returns data 報表清單
 */
export const GetReportsList = () => {
    let data = [
        { ID: 1, NAME: "季委託研究計畫列管表", MEMO: "" },
        { ID: 2, NAME: "本府委託研究計畫成果及運用情形調查列管表", MEMO: "" },
        { ID: 3, NAME: "委託研究計畫執行情形調查表", MEMO: "" },
        { ID: 4, NAME: "參採情形總表", MEMO: "" },
        { ID: 5, NAME: "續列管委託研究計畫成果及運用情形調查表", MEMO: "" }
    ]
    return data;
}

/**
 * 查詢條件初始值
 */
export const GetInitData = async () => {
    let data = {
        // 填報年度，預設當年民國年
        PLAN_YEAR: FormatDate(new Date(), "tYY"),
        // 季別，預設第一季
        SEASON_TYPE: "",
        // 局處
        ORG_ID: "",
        // 計畫編號
        PLAN_NO: "",
        // 計畫名稱
        PLAN_NAME: "",
        // 研究成果整體評估 / 參採情形
        SITUATION_TYPE: "",
        // 研究期程_起 / 計畫期程年月_起
        PLAN_START_DATE: "",
        // 研究期程_迄 / 計畫期程年月_迄
        PLAN_END_DATE: ""
    }
    return data;
    
}

/**
 * 下載報表
 * @param {string} data 資料
 */
export const exportRPT = async (data) => {
    let url = APIUrl + 'RDRPT/RDReport';
    Download(url, 'POST', data);
}

/**
 * 下拉選單初始值
 */
export const initDdlData = () => {
    let data = {
        // 填報年度，預設當年民國年
        PLAN_YEAR: [],
        // 研究成果整體評估 / 參採情形
        SITUATION_TYPE: [],
        // 季別，預設第一季
        SEASON_TYPE: [],
        // 局處
        ORG_ID: []
    }
    return data;
}

/**
 * 下拉選單
 * @returns 
 */
export const GetAllDropDowns = async () => {
    let responses = await Promise.all([
        // 填報年度
        getPlanYearList(true),
        // 季別
        GetSetParam("RDSeasonType", "", true),
        // 研究成果整體評估 / 參採情形
        GetSetParam("RDSituationType", "", true),
        // 局處
        getOrganList(),
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));
    // 季別
    result[1].unshift({ SET_VALUE: "請選擇", SET_TYPE: "" });
    // 研究成果整體評估 / 參採情形
    result[2].unshift({ SET_VALUE: "請選擇", SET_TYPE: "" });
    // 局處
    result[3].unshift({ text: "請選擇", value: "" });

    let data = {
        // 填報年度
        PLAN_YEAR: [...result[0]],
        // 季別
        SEASON_TYPE: [...result[1]],
        // 研究成果整體評估 / 參採情形
        SITUATION_TYPE: [...result[2]],
        // 局處
        ORG_ID: [...result[3]],
    }
    return data;
}

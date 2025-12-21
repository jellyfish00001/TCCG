import { api } from '../../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../../Route/RootMiddleware';
import { GetSetParam, GetSingleSetParam } from "../../../../Basic/CommonService";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

// 表單初始資料
export const initData = {
    PROJ_ADJ_ID: "",
    PROJECT_NO: "", // 計畫列管編號
    PROJECT_NAME: "", // 計畫名稱
    AW_KIND: "", // 申請項目
    BUDGET: null, // 計畫總經費–總預算經費
    PROCUREMENT_AMT: null, // 計畫總經費–發包金額
    TENDER_AWARDING_AMT: null, // 計畫總經費–決標金額
    TENDER_PROJ: "", // 承包廠商–專案管理
    TENDER_DESIGN: "", // 承包廠商–設計單位
    TENDER_SUPV: "", // 承包廠商–監造單位
    TENDER_CONST: "", // 承包廠商–施工單位
    IS_BUDGET_CENTRAL: null, // 有無獲得中央補助款
    NO_OD_REASON: "", // 無核定函原因
    IS_EFFECT_BUDGET: null, // 有無影響補助經費請領
    EFFECT_BUDGET_MEMO: "", // 影響經費請領說明
    CUR_EXECUTION: "", // 目前執行情形
    Reasons: [], // 調整原因
    OTHER_REASON: null, // 其他調整原因
    ADJUST_REASON: null, // 調整原因
    REVIEW_COMMENTS: null, // 審查原因(退回補正用)
    REVIEW_RESULT: null // 審查結果(退回補正用)
};

// 檔案初始資料
export const initFiles = {
    PROJECT_NO: "",
    FILE_KIND: "", // SET_PARAM.SET_ITEM='FILE_KIND'
    EditFiles: [],
};

/**
 * 取得主辦申請調整基本資料原因
 * @param {*} PROJ_ADJ_ID 調整檔流水號 
 * @returns 
 */
const getExecReasonSchedule = async (PROJ_ADJ_ID) => {
    let url = APIUrl + 'ProjectAdjust/GetExecReasonSchedule';
    let form = new FormData();
    form.append('PROJ_ADJ_ID', PROJ_ADJ_ID);
    let response = await api.Post(url, form, new Headers(), false);
    return response;
}

/**
 * 取得頁面所需資料
 * @param {*} PROJ_ADJ_ID 調整檔流水號 
 * @returns 
 */
export const getPageData = async (PROJ_ADJ_ID) => {
    let responses = await Promise.all([
        // 取得申請項目    
        GetSingleSetParam("AW_KIND", "AW02", true),
        // 取得期程調整原因checkbox清單
        GetSetParam("ADJUST_REASON", "", true),
        // 取得申請調整期程原因
        getExecReasonSchedule(PROJ_ADJ_ID)
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    return result;
}

/**
 * 存檔 主辦申請調整撤銷原因
 * @param {*} data 
 * @returns 
 */
export const saveExecReason = async (data) => {
    let url = APIUrl + 'ProjectAdjust/SaveExecReason/';
    let response = await api.Post(url, JSON.stringify(data), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

/**
 * 移除單筆暫存檔案
 * @param {String} uid 檔案ID
 * @returns 
 */
export const removeTempFile = async (uid) => {
    let url = APIUrl + 'UploadFile/RemoveTempFile';

    let response = await api.Post(url, JSON.stringify(uid), null, false);
    let result = null;
    if (response != null && response.ok) {
        result = response.json();
    }
    return result;
}

import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import { GetSetParam, GetSingleSetParam } from "../../../Basic/CommonService";
import { Download } from '../../../Basic/Download';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

export const initData = {
    PROJECT_NO: "", // 計畫列管編號
    PROJECT_NAME: "", // 計畫名稱
    AW_KIND: "", // 申請項目
    PROJECT_AW_STATUS: "", // 調整狀態
    EXEC_UNIT: null, // 執行機關
    Reasons: [], // 調整原因
    OTHER_REASON: "",
    ADJUST_REASON: null, // 計畫調整原因說明
    REVIEW_COMMENTS: null, // 管考意見
    REVIEW_RESULT: null, // 管考審核結果
    LOG_ID: 0 // 調整前的資料所對應的LOG_ID
};

/**
 * 管考取得主辦調整原因
 * @param PROJ_ADJ_ID 調整流水號
 * @returns 
 */
const getExecReasonByAudit = async (PROJ_ADJ_ID) => {
    let url = APIUrl + 'ProjectAdjust/GetExecReasonByAudit';
    let formData = new FormData();
    formData.append('PROJ_ADJ_ID', PROJ_ADJ_ID);
    formData.append('AW_KIND', "AW01");
    return await api.Post(url, formData, new Headers(), false);
}

/**
 * 取得頁面所需資料
 * @param PROJ_ADJ_ID 調整流水號
 * @returns 
 */
export const getPageData = async (PROJ_ADJ_ID) => {
    let responses = await Promise.all([
        // 取得資料
        getExecReasonByAudit(PROJ_ADJ_ID),
        // 取得申請項目為 AW01 的資料
        GetSingleSetParam("AW_KIND", "AW01", true),
        // 取得調整原因清單
        GetSetParam("ADJUST_REASON", "", true)
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    return result;
}

/**
 * 存檔 主辦申請調整撤銷原因
 * @returns 
 */
export const saveAuditReview = async (data) => {
    let url = APIUrl + 'ProjectAdjust/SaveAuditReview/';
    let response = await api.Post(url, JSON.stringify(data), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

/**
 * 管考取得佐證資料壓縮檔
 * @param PROJECT_NO 列管編號
 * @param PROJ_ADJ_ID 調整流水號
 * @returns 
 */
export const getZip = async (PROJECT_NO, PROJ_ADJ_ID) => {
    let url = APIUrl + 'ProjectAdjust/DownAdjustZip';
    let formData = new FormData();
    formData.append('PROJECT_NO', PROJECT_NO);
    formData.append('PROJ_ADJ_ID', PROJ_ADJ_ID);
    formData.append('AW_KIND', "AW01");
    Download(url, 'POST', formData, new Headers());
}

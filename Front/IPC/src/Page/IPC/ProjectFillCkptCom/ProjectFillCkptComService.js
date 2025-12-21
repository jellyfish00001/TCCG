import { api } from '../../../Basic/ApiFetch';
import { FormatDate, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得頁面所有資料
 * @param {*} projectNo
 * @returns {Array}
 */
export const getPageData = async (projectNo) => {
    let url = APIUrl + 'ProjectExecute/GetProjectFillCkptCom';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

/**
 * 儲存計畫聯繫資訊
 * @param {*} requestData
 * @returns
 */
export const saveProjectFillCkptCom = async (requestData) => {
    let url = APIUrl + 'ProjectExecute/SaveProjectFillCkptCom';
    let form = new FormData();
    let fileData = requestData.file != null ? requestData.file.getRawFile() :
        new File([""], "empty");
    // 因需另外夾帶檔案，API使用form接收參數，則須將其餘欄位手動mapping
    // P.S. 若直接將null值寫至form，且後端欄位為字串，會接到"null"
    //      故可以利用form的空字串傳至後端為null特性，將其轉為空字串
    //      若該欄位空字串 & null意義不同時，需另外處理
    form.append('model[PROJECT_NO]', requestData.model.PROJECT_NO);
    if (requestData.model.CONTRACT_FINISH_DATE != null)
        form.append('model[CONTRACT_FINISH_DATE_FOR_SAVE]', FormatDate(requestData.model.CONTRACT_FINISH_DATE, 'YYYY/MM/DD'));

    form.append('model[REAL_CONTACT]', requestData.model.REAL_CONTACT ?? "");
    form.append('model[REAL_TEL]', requestData.model.REAL_TEL ?? "");
    form.append('model[REAL_EMAIL]', requestData.model.REAL_EMAIL ?? "");
    form.append('model[PCC_PROJECT_UID]', requestData.model.PCC_PROJECT_UID ?? "");
    form.append('model[PCC_PROJECT_NO]', requestData.model.PCC_PROJECT_NO ?? "");
    form.append('model[PCC_PROJECT_NAME]', requestData.model.PCC_PROJECT_NAME ?? "");
    form.append('model[FACTORY_CONTACT]', requestData.model.FACTORY_CONTACT ?? "");
    form.append('model[FACTORY_TEL]', requestData.model.FACFACTORY_TEL ?? "");
    form.append('model[IS_TYCG_PROJECT]', requestData.model.IS_TYCG_PROJECT);
    form.append('model[IS_USER_FTY_DATA]', requestData.model.IS_USER_FTY_DATA);

    form.append('model[CancelSend]', requestData.model.CancelSend);
    form.append('model[DeleteEngData]', requestData.model.DeleteEngData);
    // 刪除檔案識別碼
    if (requestData.model.RemovedFileIds && requestData.model.RemovedFileIds.length > 0) {
        requestData.model.RemovedFileIds.forEach((id, i) => {
            form.append(`model[RemovedFileIds][${i}]`, id);
        })
    }
    // 檢核點資料
    requestData.model.CustomChkItemModels.forEach((cusModel, i) => {
        form.append(`model[CustomChkItemModels][${i}][SEQ]`, cusModel.SEQ);
        form.append(`model[CustomChkItemModels][${i}][CTRL_POINT]`, cusModel.CTRL_POINT);
        form.append(`model[CustomChkItemModels][${i}][CHECKITEM_SEQ]`, cusModel.CHECKITEM_SEQ);
        form.append(`model[CustomChkItemModels][${i}][PROJECT_NO]`, cusModel.PROJECT_NO);
        if (!IsNullOrEmpty(cusModel.ACTUAL_ENDDATE_OLD)) {
            form.append(`model[CustomChkItemModels][${i}][ACTUAL_ENDDATE_OLD]`, FormatDate(cusModel.ACTUAL_ENDDATE_OLD, 'YYYY-MM-DD'));
        }
        if (!IsNullOrEmpty(cusModel.ACTUAL_ENDDATE)) {
            form.append(`model[CustomChkItemModels][${i}][ACTUAL_ENDDATE_FOR_SAVE]`, FormatDate(cusModel.ACTUAL_ENDDATE, 'YYYY-MM-DD'));
        }
    });
    form.append('model[PROCUREMENT_AMT]', requestData.model.PROCUREMENT_AMT);
    form.append('model[TENDER_AWARDING_AMT]', requestData.model.TENDER_AWARDING_AMT);
    form.append('file', fileData);
    let response = await api.Post(url, form, new Headers(), false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得關聯工程會資料
 * @param {*} requestModel 
 */
export const getProjectMapPCC = async (requestModel) => {
    let url = APIUrl + 'PCC/GetProjectMapPCC';
    let response = await api.Post(url, JSON.stringify(requestModel));
    if (response.ok) {
        return await response.json();
    }
    return null;
}

/**
 * 取得工程會基本資料
 * @param {*} pccProjectUid 
 */
export const getPccmDs01 = async (pccProjectUid) => {
    let url = APIUrl + 'PCC/GetPccmDs01';
    let response = await api.Post(url, JSON.stringify(pccProjectUid), null, false);
    if (response.ok) {
        return await response.json();
    }
    return null;
}

/**
 * 取得工程標案執行進度
 * @param {*} pccProjectUid 
 */
export const getPCCExeProgress = async (pccProjectUid) => {
    let url = APIUrl + 'PCC/GetPCCExeProgress';
    let response = await api.Post(url, JSON.stringify(pccProjectUid), null, false);
    if (response.ok) {
        return await response.json();
    }
    return null;
}

/**
 * 關聯工程會標案資料
 * @param {string} PROJECT_NO 
 * @param {string} uid 
 * @param {string} startWorkDate 
 * @returns 
 */
export const saveProjectMapPCC = async (PROJECT_NO, uid, startWorkDate) => {
    const url = APIUrl + 'PCC/SaveProjectMapPCC';
    const form = new FormData();
    form.append('PROJECT_NO', PROJECT_NO);
    form.append('PCC_PROJECT_UID', uid);
    form.append('START_WORK', startWorkDate);

    const response = await api.Post(url, form, new Headers())
    if (response.ok) {
        return await response.json();
    }
    return null;
}

/**
 * 介接/取消介接 工程會標案管理系統
 * @param {string} PROJECT_NO
 * @param {boolean} IS_USER_FTY_DATA 
 * @returns 
 */
export const saveProjectUsePCC = async (projectNo, isUserFtyData) => {
    const url = APIUrl + 'PCC/SaveProjectUsePCC';
    const form = new FormData();
    form.append('PROJECT_NO', projectNo);
    form.append('IS_USER_FTY_DATA', isUserFtyData);
    const response = await api.Post(url, form, new Headers())
    if (response.ok) {
        return await response.json();
    }
    return null;
}

/**
 * 工程標案工程概要資料
 * @param {*} pccProjectUid 
 */
export const getPCCDs07 = async (pccProjectUid) => {
    let url = APIUrl + 'PCC/GetPCCDs07';
    let response = await api.Post(url, JSON.stringify(pccProjectUid), null, false);
    if (response.ok) {
        return await response.json();
    }
    return null;
}

/**
 * 工程標案決標資料
 * @param {*} pccProjectUid 
 */
export const getPCCDs09 = async (pccProjectUid) => {
    let url = APIUrl + 'PCC/getPCCDs09';
    let response = await api.Post(url, JSON.stringify(pccProjectUid), null, false);
    if (response.ok) {
        return await response.json();
    }
    return null;
}

/**
 * 清空當次週期已填報的檢核點完成日期、辦理情形及落後原因
 * @param {string} PROJECT_NO
 * @returns 
 */
export const clearCycleData = async (PROJECT_NO) => {
    const url = APIUrl + 'ProjectExecute/ClearCycleData';
    const response = await api.Post(url, JSON.stringify(PROJECT_NO));
    if (response.ok) {
        return await response.json();
    }
    return null;
}
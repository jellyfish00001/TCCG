//*****共用的Service*****

import { getGlobalServerConfig } from '../Route/RootMiddleware';
import { api } from './ApiFetch';
import { GetBasicData } from './BasicData';
import { Download } from './Download';
import { IsNullOrEmpty, openPage } from './SDOExtension';

let APIUrl = getGlobalServerConfig().backEndUrl.get();
let DdlAPIUrl = APIUrl + 'DropDown'

/**
 * 取得system param (預設排除停用)
 * @returns 
 */
export const GetSetParam = async (setItem, defaultValue = "", isReturnPromise = false, delFlg = false) => {
    let url = APIUrl + 'SetParam/GetParamByItem';
    let form = new FormData();
    form.append('setItem', setItem);
    // 不給則預設 MainDBKey
    // 指定DB 1 = SCDBKey, 2 = RISDBKey, 3 = IPCDBKey, 4 = INNDBKey, 5 = PWSDBKey, 6 = RDDBKey
    form.append('fromWhere', 5);
    if (delFlg != null)
        form.append('delFlg', delFlg);
    let response = await api.Post(url, form, new Headers(), false);
    if (isReturnPromise) {
        return response;
    }
    let result = [];
    if (response.ok) {
        result = await response.json();
        if (!IsNullOrEmpty(defaultValue)) {
            result.unshift({ SET_TYPE: "", SET_VALUE: defaultValue })
        }
    }
    return result;
}

/**
 * 取得多組system param 
 * @returns 
 */
export const GetParamByItems = async (item, isReturnPromise = false) => {
    let url = APIUrl + 'SetParam/GetParamByItems';
    let response = await api.Post(url, JSON.stringify(item), null, false);
    if (isReturnPromise) {
        return response;
    }
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 設定停用字眼
 * @returns 
 */
export const SetDelFlgName = async (data) => {
    return data.map(x => {
        x.SET_VALUE = `${x.SET_VALUE}${x.DEL_FLG ? "(已停用)" : ""}`
        return x
    })
}

/**
 * 取得單筆 system param 
 * @returns 
 */
export const GetSingleSetParam = async (setItem, setType, isReturnPromise = false) => {
    let url = APIUrl + 'SetParam/' + setItem + '/' + setType;
    let response = await api.Get(url, null, null, false);
    if (isReturnPromise) {
        return response;
    }
    let result = {};
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 月份下拉
 * @returns 
 */
export const getMonthList = () => {
    let result = [];
    for (let i = 1; i <= 12; i++) {
        result.push({ text: i, value: i });
    }
    return result;
}

/**
 * 取得計畫年度下拉清單資料
 * @param {*} isReturnPromise 
 * @param {*} year 
 * @returns 
 */
export const getPlanYearList = async (isReturnPromise = false, year, type) => {
    let url = DdlAPIUrl + '/GetProjectYearList';
    let form = new FormData();
    // 預設系統年度往回推10年
    form.append('year', year ?? 10);
    form.append('type', type ?? "");
    let response = await api.Post(url, form, new Headers(), false);

    if (isReturnPromise)
        return response;
    let rtn = [];
    if (response.ok) {
        rtn = await response.json();
    }
    return rtn;
}

/**
 * 取得機關下拉清單
 * @returns 
 */
export const getOrganList = async () => {
    let url = DdlAPIUrl + '/GetOrganList';
    let response = await api.Get(url, null, null, false);
    let rtn = [];
    if (response.ok) {
        rtn = await response.json();
        if (rtn.length > 0) {
            rtn = [{ text: '請選擇' ,value: null }, ...rtn];
        }
    }
    return rtn;
}

/**
 * 取得基金下拉清單
 * @returns 
 */
export const getFundList = async () => {
    let url = DdlAPIUrl + '/GetFundList';
    let response = await api.Get(url, null, null, false);
    let rtn = [];
    if (response.ok) {
        rtn = await response.json();
        if (rtn.length > 0) {
            rtn.forEach(item => {
                item.value = Number(item.value);
            });
            rtn = [{ text: '請選擇' ,value: null }, ...rtn];
        }
    }
    return rtn;
}

/**
 * 取得基金下拉清單
 * @returns 
 */
export const getFundOrgList = async () => {
    let url = DdlAPIUrl + '/GetFundOrgList';
    let response = await api.Get(url, null, null, false);
    let rtn = [];
    if (response.ok) {
        rtn = await response.json();
        rtn = [{ text: '請選擇' ,value: null }, ...rtn];
    }
    return rtn;
}

/**
 * 取得登入者的機關下拉選單資料
 * @returns 
 */
export const getOrgByUsr = async () => {
    let url = DdlAPIUrl + '/GetOrgByUsr';
    return await api.Get(url, null, null, false);
}

/**
 * 根據機關取得人員下拉選單
 * @param {*} ouId 
 * @param {*} undertakerType 1:主管機關 2:執行機關 3:協辦機關 4:代辦機關
 * @param {*} defaultValue 
 * @param {*} isEnable 是否啟用
 * @returns 
 */
export const getUserListByOuId = async (ouId, undertakerType, defaultValue = "", isEnable = true) => {
    let url = DdlAPIUrl + '/GetUserByOrg';
    let form = new FormData();
    form.append('OU_ID', ouId);
    form.append('UNDERTAKER_TYPE', undertakerType);
    form.append('IS_ENABLE', isEnable);
    let response = await api.Post(url, form, new Headers(), false);
    let rtn = [];
    if (response.ok) {
        rtn = await response.json();
        if (!IsNullOrEmpty(defaultValue)) {
            rtn.unshift({ text: defaultValue, value: "" })
        }
    }

    return rtn;
}

/**
 * 取得 辦理地點 (區)
 * @param {*} cityId 預設桃園市
 * @param {*} defaultValue 
 * @returns 
 */
export const getCodeTownByCityId = async (cityId = "H", defaultValue = "", isReturnPromise = false) => {
    let url = DdlAPIUrl + '/GetCodeTownByCityId';
    let response = await api.Post(url, JSON.stringify(cityId), null, false);
    if (isReturnPromise) {
        return response;
    }

    let rtn = [];
    if (response.ok) {
        rtn = await response.json();
        if (!IsNullOrEmpty(defaultValue)) {
            rtn.unshift({ text: defaultValue, value: "" })
        }
    }
    return rtn;
}

/**
* 下載已上傳的檔案
* @param {*} identityField 
*/
export const downProjectAttachment = async (identityField, DBKey = 5) => {
    let url = APIUrl + 'ProjectCommon/DownProjectAttachment';
    let formData = new FormData();
    formData.append('IDENTITY_FIELD', identityField);
    formData.append('DBKey', DBKey);
    Download(url, 'POST', formData, new Headers())
}

/**
 * 下載附件壓縮檔
 * @param {*} identityFields 
 * @param {*} title 壓縮檔檔名
 */
export const downProjectAttachmentZip = async (identityFields, title) => {
    let url = APIUrl + 'ProjectCommon/DownProjectAttachmentZip';
    let formData = new FormData();
    for (let i = 0; i < identityFields.length; i++) {
        formData.append("IDENTITY_FIELDs", identityFields[i]);
    }
    formData.append('title', title);
    Download(url, 'POST', formData, new Headers());
}

/**
 * 取得當期填報週期資料
 * @returns 
 */
export const getCurrentCycleData = async () => {
    let url = APIUrl + 'ProjectCommon/GetCurrentCycleData';
    let response = await api.Get(url, null, null, false);
    if (response.ok) {
        return response.json();
    } else {
        return null;
    }
}

/**
 * 取得SC連結
 * @param {*} dominName 
 * @param {*} apId 
 */
export const GetScLink = async (dominName, apId) => {
    let url = APIUrl + 'SCApplication/GetScLink/' + dominName + '/' + apId;
    let response = await api.Get(url, null, new Headers(), false);
    let data = {};
    if (response.ok) {
        data = await response.json();
    }

    return data.message.replaceAll("&amp;", "&");
}

/**
 * 檢查登入者是否有管考權限(管考角色)
 * @returns 
 */
export const CheckIsRDECRole = async () => {
    let roles = await GetBasicData("roles");
    return roles.filter(x => x.AP_ID === 'IPC3' && x.ROLE_ID === 'RDEC_RDEC_ROL_IPC3').length > 0;
}

/**
 * 檢查登入者是否"只有"主辦權限(主辦角色)
 * @returns 
 */
export const CheckIsHANDRole = async () => {
    let roles = await GetBasicData("roles");
    if (roles.length !== 1) {
        return false;
    }
    else {
        let role = roles[0];
        return role.AP_ID === 'IPC3' && role.ROLE_ID === 'HAND_USER_ROL_IPC3';
    }
}

/**
 * 取得已使用的代碼清單
 * @param {string} setItem 代碼類別
 * @returns 
 */
export const GetUsedCode = async (setItem) => {
    let url = APIUrl + 'ProjectCommon/GetUsedCode';
    let response = await api.Post(url, JSON.stringify(setItem), null, false);
    if (response.ok) {
        return await response.json();
    }
    return [];
}

/**
 * 取得是否使用國發會界接資料
 * @param {string} projectNo 計畫編號
 * @returns true or false
 */
export const GetIsUserFtyData = async (projectNo) => {
    let url = APIUrl + 'Project/GetIsUserFtyData';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    if (response.ok) {
        return await response.json();
    }
    return false;
}

// #region sessionStorage 只能開一個分頁
/**
 * 設定StorageData
 * @param {*} param 
 */
export const SetStorageData = (param, name = "chapterInfo", currentWindow = null) => {
    if (currentWindow != null) {
        currentWindow.sessionStorage.setItem(name, JSON.stringify(param));
    } else {
        sessionStorage.setItem(name, JSON.stringify(param));
    }
}

/**
 * 取得StorageData
 * @returns 
 */
export const GetStorageData = (name = "chapterInfo") => {
    let sessionData = sessionStorage.getItem(name);
    let result = JSON.parse(sessionData);
    return result;
}
//#endregion

/**
 * 設定計畫預覽列印資訊
 * @param {*} isRdecFun 是否為管考功能
 * @param {*} funRole 功能面向角色
 * @param {*} projectNo 計畫編號
 * @param {*} projectName 計畫名稱
 * @param {*} isSSOLogin 是否從單一入口進入
 */
export const setPrintInfo = async (isRdecFun, funRole, projectNo, projectName, isSSOLogin) => {
    let data = {
        isRdecFun: isRdecFun,
        funRole: funRole,
        projectNo: projectNo,
        projectName: projectName,
        isSSOLogin: isSSOLogin
    };
    SetStorageData(data, "printInfo");
}

/**
 * 開啟計畫預覽列印
 * @param {*} item 
 * @param {*} source 資料來源：chapter(章節表)、list(清單)
 * @param {*} funRole 
 */
export const openProjectPrint = async (item, source = "chapter", funRole = null) => {
    funRole = source === "chapter" ? item.funRole : funRole;
    // const isRdecFun = source === "chapter" ? item.isRdecFun : funRole === 2;
    const isRdecFun = false;
    const projectNo = source === "chapter" ? item.projectNo : item.PROJECT_NO;
    const projectName = source === "chapter" ? item.projectName : item.PROJECT_NAME;
    setPrintInfo(isRdecFun, funRole, projectNo, projectName, false);
    // 同一個計畫只會有一個分頁，用PROJECT_NO當NAME
    openPage(process.env.PUBLIC_URL + '/ProjectPrint', `Print${projectNo}`, `預覽列印(${projectNo})`)
}

/**
 * 開啟計畫差異比對
 * @param {*} data 
 */
export const openDiffCompare = async (data) => {
    SetStorageData(data, "printInfo");
    // 同一個計畫只會有一個分頁，用PROJECT_NO當NAME
    openPage(process.env.PUBLIC_URL + '/ProjectPrint', `DiffCompare${data.projectNo}`, `差異比對(${data.projectNo})`)
}
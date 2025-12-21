import { api } from '../../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../../Route/RootMiddleware';
import { Download } from "../../../../Basic/Download";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

// 檔案初始資料
export const initFiles = {
    PROJECT_NO: "",
    FILE_KIND: "", // SET_PARAM.SET_ITEM='FILE_KIND'
    EditFiles: [],
};

/**
 * 取得主辦調整檢核結果
 * @param {string} PROJECT_NO 列管編號
 * @param {*} PROJ_ADJ_ID 調整流水號
 * @returns 
 */
export const getAdjustChk = async (PROJECT_NO, PROJ_ADJ_ID) => {
    let url = APIUrl + 'ProjectAdjust/GetAdjustChk';
    let form = new FormData();
    form.append('PROJECT_NO', PROJECT_NO);
    form.append('PROJ_ADJ_ID', PROJ_ADJ_ID);
    let response = await api.Post(url, form, new Headers());
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

/**
 * 存檔 主辦申請調整撤銷原因
 * @param {*} data 
 * @returns 
 */
export const sendExecAdjust = async (data) => {
    let url = APIUrl + 'ProjectAdjust/SendExecAdjust/';
    let response = await api.Post(url, JSON.stringify(data), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

/**
 * 儲存計畫檔案資料
 * @param {*} data 
 * @returns 
 */
export const saveScheAttach = async (data) => {
    let url = APIUrl + 'ProjectAdjust/SaveScheAttach';
    let response = await api.Post(url, JSON.stringify(data));
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

/**
 * 下載申請表
 * @param {string} PROJECT_NO 列管編號
 * @param {*} PROJ_ADJ_ID 調整流水號
 */
export const exportRPT = async (PROJECT_NO, PROJ_ADJ_ID) => {
    let url = APIUrl + 'RPT/RPTAdjustSchedule';
    let form = new FormData();
    form.append('PROJECT_NO', PROJECT_NO);
    form.append('PROJ_ADJ_ID', PROJ_ADJ_ID);
    Download(url, 'POST', form, new Headers());
}

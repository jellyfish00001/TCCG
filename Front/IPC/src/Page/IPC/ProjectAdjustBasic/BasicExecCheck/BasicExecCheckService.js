import { api } from '../../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

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
export const SendExecAdjust = async (data) => {
    let url = APIUrl + 'ProjectAdjust/SendExecAdjust/';
    let response = await api.Post(url, JSON.stringify(data), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

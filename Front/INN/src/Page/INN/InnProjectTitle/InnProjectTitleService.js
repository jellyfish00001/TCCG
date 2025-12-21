import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得專題代號及名稱
 * @param {string} INN_YEAE 年度
 * @returns 
 */
export const getInnProjedtTitle = async (PLAN_YEAR) => {
    let url = APIUrl + 'ProjectTitle/GetInnProjectTitle';
    let response = await api.Post(url, JSON.stringify(PLAN_YEAR));
    let result = {};
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存資料
 * @param {*} requestData
 * @returns
 */
export const saveInnProjectTitle = async (requestData) => {
    let url = APIUrl + 'ProjectTitle/SaveInnProjectTitle';
    let response = await api.Post(url, JSON.stringify(requestData));
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}
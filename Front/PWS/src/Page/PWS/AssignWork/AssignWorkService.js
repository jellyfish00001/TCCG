import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得年度下拉選單
 * @returns 
 */
export const getPlanYear = async () => {
    let url = APIUrl + 'Deadline/GetPlanYear';
    let response = await api.Post(url, null, null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 取得截止時間
 * @param {*} data 
 * @returns 
 */
export const getPlanDeadline = async (data) => {
    let url = APIUrl + 'Deadline/GetPWSSDAssignment';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 設定截止日期
 * @param {*} data 
 * @returns 
 */
export const expirationDate = async (data) => {
    let url = APIUrl + 'Deadline/SavePWSSDAssignment';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};


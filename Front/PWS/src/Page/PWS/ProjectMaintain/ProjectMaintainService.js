import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { getPlanYearList } from '../../../Basic/CommonService';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 年度下拉選單
 * @returns 
 */
export const getYearList = async () => {
    let responses = await Promise.all([
        // 取得年度
        getPlanYearList(true, 20, "A"),
    ])
    let responseJson = responses.map(res => res.json());
    let result = await Promise.all(responseJson);
    return result;
};

/**
 * 取得年度
 * @returns 
 */
export const getMaintainYear = async () => {
    let url = APIUrl + 'MaintainYear/GetPWSSDYearSet';
    let response = await api.Post(url, null, null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 維護年度
 * @param {*} data
 * @returns 
 */
export const saveMaintainYear = async (data) => {
    let url = APIUrl + 'MaintainYear/SavePWSSDYearSet';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};





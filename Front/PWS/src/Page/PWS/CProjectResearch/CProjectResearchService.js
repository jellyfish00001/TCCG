import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { AddNoColumn } from "../../../Basic/SDOExtension";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/** 
* 存入歷年執行情形
* @param {*} saveData
* @returns
*/
export const saveThreeYearPlan = async (saveData) => {
    let url = APIUrl + 'ThreeYearPlan/SaveThreeYearPlan';
    let response = await api.Post(url, JSON.stringify(saveData), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/** 
* 取得歷年執行情形
* @param {*} saveData
* @returns
*/
export const getThreeYearPlan = async (PLANNO) => {
    let url = APIUrl + 'ThreeYearPlan/GetThreeYearPlan';
    let response = await api.Post(url, JSON.stringify(PLANNO), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return AddNoColumn(result);
};
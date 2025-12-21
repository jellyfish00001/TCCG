import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 驗證計畫
 * @param {*} planNo //計畫編號
 * @returns 
 */
export const GetPlanErrorData = async (planNo, projectKind) => {
    let url = APIUrl + 'Submit/CheckProjectFillSubmit';
    let form = new FormData();
    form.append('PROJECT_NO', planNo);
    form.append('PLANKIND', projectKind);
    let response = await api.Post(url, form, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 取得計畫基本資料
 * @param {*} planNo //計畫編號
 * @returns 
 */
export const SavePlanErrorData = async (planNo) => {
    let url = APIUrl + 'Submit/SavePlanFillAddSubmit';
    let response = await api.Post(url, JSON.stringify(planNo), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};
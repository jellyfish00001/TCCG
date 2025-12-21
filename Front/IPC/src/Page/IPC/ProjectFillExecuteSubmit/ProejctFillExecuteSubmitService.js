import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 驗證執行情形送出
 * @param {*} projectNo
 * @returns
 */
export const checkProjectFillExecuteSubmit = async (projectNo) => {
    let url = APIUrl + 'ProjectExecute/CheckProjectFillExecuteSubmit';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 執行情形送出 送出/結案申請
 * @param {*} projectNo
 * @param {*} saveType
 * @returns
 */
export const saveProjectFillExecuteSubmit = async (projectNo, saveType) => {
    let url = APIUrl + 'ProjectExecute/SaveProjectFillExecuteSubmit';
    let form = new FormData()
    form.append('PROJECT_NO', projectNo);
    form.append('SaveType', saveType)
    let response = await api.Post(url, form, new Headers(), false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}







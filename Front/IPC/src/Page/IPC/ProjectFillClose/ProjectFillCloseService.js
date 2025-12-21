import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得結案資料/結案審核
 * @param {*} projectNo
 * @returns
 */
export const getProjectFillClose = async (projectNo) => {
    let url = APIUrl + 'ProjectClosed/GetProjectFillClose';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存結案資料/結案審核
 * @param {*} requestData
 * @returns
 */
export const saveProjectFillClose = async (requestData) => {
    let url = APIUrl + 'ProjectClosed/SaveProjectFillClose';
    let response = await api.Post(url, JSON.stringify(requestData));
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}







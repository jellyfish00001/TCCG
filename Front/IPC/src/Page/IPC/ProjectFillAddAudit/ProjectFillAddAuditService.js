import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 儲存立案審核資料
 * @param {*} requestData
 * @returns
 */
export const SaveProjectFillAddAudit = async (requestData) => {
    let url = APIUrl + 'Project/SaveProjectFillAddAudit';
    let response = await api.Post(url, JSON.stringify(requestData), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得立案審核資料
 * @param {*} projectNo
 * @returns
 */
export const loadFormData = async (projectNo) => {
    let url = APIUrl + 'Project/GetProjectFillAddAudit';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}







import { api } from '../../../Basic/ApiFetch';
import { AddNoColumn, IsNullOrEmpty, HtmlDecode } from '../../../Basic/SDOExtension';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { Download } from '../../../Basic/Download'
import { GetSetParam } from '../../../Basic/CommonService';
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 驗證計劃送出
 * @param {*} projectNo
 * @returns
 */
export const checkProjectCanSubmit = async (projectNo) => {
    let url = APIUrl + 'Project/CheckProjectCanSubmit';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存計劃立案送審
 * @param {*} projectNo
 * @returns
 */
export const saveProjectFillAddSubmit = async (projectNo) => {
    let url = APIUrl + 'Project/SaveProjectFillAddSubmit';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}







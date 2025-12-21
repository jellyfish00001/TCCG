import { api } from "../../../Basic/ApiFetch";
import { Download } from "../../../Basic/Download";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得計畫異動記錄清單
 * @param {*} 
 * @returns 
 */
export const getProjectLogList = async (projectNo) => {
    let url = APIUrl + 'ProjectList/GetProjectLogList';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 下載計畫報表
 * @param {*} model 
 */
export const downcompareDiff = async (model) => {
    model.GoogleMapAPIKey = process.env.REACT_APP_GOOGLE_API_KEY;
    let url = APIUrl + 'RPT/RPTProjectPrint';
    Download(url, 'POST', model);
}

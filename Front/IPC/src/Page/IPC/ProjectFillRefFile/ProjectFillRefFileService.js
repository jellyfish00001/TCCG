import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得參考資料
 * @returns 
 */
const getRefFile = async () => {
    let url = APIUrl + 'ProjectCommon/GetRefFile';
    let response = await api.Post(url);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存參考資料
 * @param {*} files 
 * @returns 
 */
const saveRefFile = async (files) => {
    let url = APIUrl + 'ProjectCommon/SaveRefFile';
    let response = await api.Post(url, JSON.stringify(files), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

const ProjectFillRefFileService = {
    getRefFile: getRefFile,
    saveRefFile: saveRefFile,
}
export default ProjectFillRefFileService;
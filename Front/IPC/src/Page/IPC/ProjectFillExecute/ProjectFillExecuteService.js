import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";


let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得計畫每月辦理情形(單筆)
 * @param {*} projectNo 
 * @param {*} seq (可傳可不傳)
 * @returns 
 */
export const getProjecFillExecute = async (projectNo, seq) => {
    let url = APIUrl + 'ProjectExecute/GetProjecFillExecute';
    let formData = new FormData();
    formData.append("PROJECT_NO", projectNo);
    formData.append("SEQ", seq);
    let response = await api.Post(url, formData, new Headers(), false);

    let result = {};
    if (response.ok) {
        result = await response.json();

    }
    return result;
}

/**
 * 取得計畫每月辦理情形清單
 * @param {*} projectNo 
 * @param {*} dataType 資料種類 0:預設近5個月 1:全部
 * @returns 
 */
export const getProjecFillExecuteList = async (projectNo, dataType) => {
    let url = APIUrl + 'ProjectExecute/GetProjecFillExecuteList';
    let formData = new FormData();
    formData.append("PROJECT_NO", projectNo);
    formData.append("DATA_TYPE", dataType);
    let response = await api.Post(url, formData, new Headers(), false);

    let result = [];
    if (response.ok) {
        result = await response.json();

    }
    return result;
}

/**
 * 儲存計劃每月辦理情形
 * @param {*} model 
 * @returns 
 */
export const saveProjecFillExecute = async (model) => {
    let url = APIUrl + 'ProjectExecute/SaveProjecFillExecute';
    let response = await api.Post(url, JSON.stringify(model), null, false);

    let result = [];
    if (response.ok) {
        result = await response.json();

    }
    return result;
}


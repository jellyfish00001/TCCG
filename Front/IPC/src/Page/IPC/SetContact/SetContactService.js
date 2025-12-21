import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得機關窗口維護
 * @returns 
 */
const getSetContact = async () => {
    let url = APIUrl + 'SetParam/GetSetContact';
    let response = await api.Post(url);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得機關聯絡窗口
 * @param {*} organ 
 * @returns 
 */
const getSetContactByOrgan = async (organ) => {
    let url = APIUrl + 'SetParam/GetSetContactByOrgan';
    let response = await api.Post(url, JSON.stringify(organ), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存機關窗口維護
 * @param {*} models 
 * @returns 
 */
const saveSetContact = async (models) => {
    let url = APIUrl + 'SetParam/SaveSetContact';
    let response = await api.Post(url, JSON.stringify(models), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

const SetContactService = {
    getSetContact: getSetContact,
    getSetContactByOrgan: getSetContactByOrgan,
    saveSetContact: saveSetContact,
}
export default SetContactService;
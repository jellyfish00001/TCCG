import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

/**
 * 工程標案同步
 * @param {*} projectNos
 * @returns 
 */
export const syncData = async (projectNos) => {
    let url = getGlobalServerConfig().backEndUrl.get() + 'Pcc/SyncPCCData';
    let response = await api.Post(url, JSON.stringify(projectNos));
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}
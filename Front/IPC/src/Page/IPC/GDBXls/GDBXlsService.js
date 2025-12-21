import { saveAs } from '@progress/kendo-drawing/pdf';
import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { Download } from '../../../Basic/Download';

/**
 * 取得工程標案資料集下拉清單
 * @returns 
 */
export const getPccDSNOs = async () => {
    let url = getGlobalServerConfig().backEndUrl.get() + 'Pcc/GetPccSrcTables';
    let response = await api.Get(url);
    let result = null;
    if (response.ok) {
        result = await response.json();
        result.unshift({ text: "請選擇", value: "" })
    }
    return result;
}

/**
 * 工程標案 excel
 * @param {*} data
 * @returns 
 */
export const getPccmXls = async (data) => {
    let url = getGlobalServerConfig().backEndUrl.get() + 'Pcc/GetPccmXls';
    let response = await api.Post(url, JSON.stringify(data));
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

export const DownloadPccmXls = async (data) => {
    let url = getGlobalServerConfig().backEndUrl.get() + 'Pcc/DownloadPccmXls';
    Download(url, 'POST', data);
}
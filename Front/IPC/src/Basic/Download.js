//*****下載*****

import { apiFetch, api } from './ApiFetch';
import { showGlobalMessageBox, getGlobalServerConfig } from '../Route/RootMiddleware';
import { saveAs } from '@progress/kendo-file-saver';

/**
 * 下載檔案
 * @typedef {string} controller
 * @typedef {string} act
 * @typedef {object} form
 * @prop {controller} controller
 * @prop {act} action
 * @prop {form} formdata
 */
export const downloadFile = async (backurl, controller, act, form, NonCustomContentType) => {
    try {
        // let backurl=ServerConfig.backEndUrl;
        backurl = backurl == null ? getGlobalServerConfig().backEndUrl : backurl
        /* 檔案下載 ajax */
        // let request = new Request(backurl+'/'+controller+'/'+act,  { method: 'POST', body: form });
        // let response = await apiFetch(request, !NonCustomContentType);
        let response = await api.Post(backurl + '/' + controller + '/' + act, form, new Headers());
        if (response.ok) {
            let fileName = decodeURIComponent(response.headers.get("content-disposition")?.split("UTF-8''")[1] ?? 'undefined');
            if (fileName != "") {
                let blob = await response.blob();
                if (fileName.toLowerCase().includes(".pdf")) {
                    var file = new Blob([blob], { type: 'application/pdf' });
                    var fileURL = URL.createObjectURL(file);
                    window.open(fileURL);
                }
                else
                    saveAs(blob, fileName)
                // window.open("data:application/pdf;base64, " + base64EncodedPDF);
                // saveAs(blob, fileName)
                // let href = window.URL.createObjectURL(blob);
                // let link = document.createElement('a');
                // link.download = fileName;
                // link.href = href;
                // link.click();
            }
            else {
                showGlobalMessageBox("無檔案可下載");
            }
        }
    }
    catch (e) {
        console.log(e);
    }
}

/**
 * 匯出Grid資料
 * @param {*} gridData 
 * @param {*} format 
 * @param {*} outputName 
 */
export const ExportGrid = async (gridData, format, outputName = '') => {
    let columns = [];
    gridData.Columns.forEach(item => {
        if (item.field != null)
            columns.push({ Title: item.title, Field: item.field })
        // for 動態欄位查詢，ex:綜合查詢
        else if (item.exportTitle != null && item.exportField)
            columns.push({ Title: item.exportTitle, Field: item.exportField })
    });
    let data = gridData.props.exportData;
    let obj = { Header: columns, Data: data, Format: format, OutputName: outputName };
    Download(getGlobalServerConfig().backEndUrl.get() + 'RPT/ExportGrid', 'POST', obj)
}

/**
 * 下載檔案
 * @param {*} url 
 * @param {*} method 
 * @param {*} requestBody 
 * @param {*} headers 
 */
export const Download = async (url, method, requestBody = null, headers = null) => {
    let response;
    if (method === "GET") {
        response = await api.Get(url);
    }
    else if (method === "POST") {
        if (requestBody == null) {
            response = await api.Post(url);
        }
        else if (headers != null) {
            response = await api.Post(url, requestBody, headers);
        } else {
            response = await api.Post(url, JSON.stringify(requestBody));
        }
    }

    if (response == null || !response.ok) {
        showGlobalMessageBox("無檔案可下載");
    }
    else {
        let fileName = decodeURIComponent(response.headers.get("content-disposition")?.split("UTF-8''")[1] ?? 'undefined');
        if (fileName !== "") {
            let blob = await response.blob();
            // PDF另開視窗預覽
            // if (fileName.toLowerCase().includes(".pdf")) {
            //     var file = new Blob([blob], { type: 'application/pdf' });
            //     var fileURL = URL.createObjectURL(file);
            //     window.open(fileURL);
            // }
            // else
            saveAs(blob, fileName);
        }
        else {
            showGlobalMessageBox("無檔案可下載");
        }
    }
}
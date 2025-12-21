import React from 'react';
import { api } from '../../../Basic/ApiFetch';
import { downProjectAttachment } from '../../../Basic/CommonService';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { Download } from "../../../Basic/Download";

let APIUrl = getGlobalServerConfig().backEndUrl.get();


/**
 * 取得計畫狀態
 * @param {*} projectNo 
 * @returns 
 */
export const getProjectStatus = async (projectNo) => {
    let url = APIUrl + 'Project/GetProjectStatus';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得計畫章節表
 * @param {*} operation 
 * @param {*} funRole 
 * @param {*} projectNo 
 * @param {*} apId
 * @returns 
 */
export const getProjectChapter = async (operation, funRole, projectNo, apId) => {
    let url = APIUrl + 'Project/GetProjectChapter';
    // let form = new FormData();
    // form.append('operation', operation);
    // form.append('userRole', funRole);
    // form.append('projectNo', projectNo);
    // let response = await api.Post(url, form, new Headers(), false);
    let requestObj = {
        OPERATION: operation,
        USER_ROLE: funRole,
        PROJECT_NO: projectNo,
        ApId: apId
    }
    let response = await api.Post(url, JSON.stringify(requestObj), null, false);

    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    let file = await getRefFile();

    // 需客製樣式的function
    let customfunStyle = new Array(
        "ProjectFillBudgetExec",  //預算執行情形
        "ProjectFillField", //實地查證情形
        "ProjectFillOther", //其他資訊
        "ProjectFillClose", //結案資料
        // "ProjectFillExecuteSubmit" //執行情形送出
    );

    result.map(x => {
        if (x.CHAPTER_ID == "ProjectFillRefFile") {
            x.content = getDownLoadFileList(file)
        }
        if (customfunStyle.includes(x.CHAPTER_ID))
            x.className = "k-header_yellow";
    })

    return result;
}

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

const getDownLoadFileList = (data) => {
    return (
        <ul>
            {
                data.map(x => {
                    return (
                        <li>
                            <a href='/' onClick={(e) => {
                                e.preventDefault();
                                downProjectAttachment(x.IDENTITY_FIELD)
                            }}>{x.FILE_NAME}</a>
                        </li>
                    )
                })
            }
        </ul>
    )
}

/**
 * 產出計畫年終考核評分表
 * @param {*} projectNo 
 */
export const downProjectFillYearAssRPT = async (projectNo) => {
    let url = APIUrl + 'RPT/RPTProjectFillYearAss';
    Download(url, 'POST', projectNo);
}

/**
 * 取得作業階段
 * @param {*} projectNo 
 * @returns 
 */
export const getWorkStage = async (projectNo) => {
    let url = APIUrl + 'DropDown/GetWorkStage';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}
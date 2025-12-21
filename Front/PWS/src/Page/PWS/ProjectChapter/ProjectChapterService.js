import React from 'react';
import { api } from '../../../Basic/ApiFetch';
import { downProjectAttachment } from '../../../Basic/CommonService';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { Download } from "../../../Basic/Download";
import { openChapterPage } from '../../../Basic/SDOExtension';

let APIUrl = getGlobalServerConfig().backEndUrl.get();


/**
 * 開啟章節表
 * @param {*} item 
 * @returns 
 */
export const openProjectChapter = async (item, role) => {
    //#region 資料處理
    const { PLANNO, PLANNAME, PLANKIND, PLANID, PLANYEAR, IS_SEND } = item;
    let data = {
        funRole: role, //功能面向角色
        projectNo: PLANNO,
        projectName: PLANNAME,
        projectId: PLANID,
        projectYear: PLANYEAR,
        projectKind: PLANKIND,
        projectIsSend: IS_SEND
    };
    //#endregion 
    let title = "";
    switch (PLANKIND) {
        case "1":
            title = "重大施政"; break;
        case "2":
            title = "委託研究"; break;
    }

    switch (role) {
        case 5:
            title += "專案小組"; break;
        default:
            break;
    }

    // 開啟章節表(同一個計畫只會有一個分頁，用PROJECT_NO當NAME)
    const url = process.env.PUBLIC_URL + '/ProjectChapter';
    openChapterPage(url, PLANNO, `${title}(${PLANNO})`, data);
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
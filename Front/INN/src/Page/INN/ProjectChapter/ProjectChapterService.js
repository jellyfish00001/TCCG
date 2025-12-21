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
    const { PROJECT_NO, PROJECT_NAME, PROJECT_TYPE } = item;
    let data = {
        funRole: role, //功能面向角色
        projectNo: PROJECT_NO,
        projectName: PROJECT_NAME,
    };
    //#endregion 

    let title = "";
    switch (PROJECT_TYPE) {
        case "PLAN_A":
            title = "重大施政"; break;
        case "PLAN_B":
            title = "委託研究"; break;
    }

    switch (role) {
        case 0:
            title += "資料登錄"; break;
        case 1:
            title += "計畫審查"; break;
        default:
            break;
    }



    // 開啟章節表(同一個計畫只會有一個分頁，用PROJECT_NO當NAME)
    const url = process.env.PUBLIC_URL + '/ProjectChapter';
    openChapterPage(url, PROJECT_NO, `${title}(${PROJECT_NO})`, data);
}

/**
 * 取得計畫章節表
 * @param {*} operation 
 * @param {*} funRole 
 * @param {*} projectNo 
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
    let file = await getRefFile();

    // // 需客製樣式的function
    // let customfunStyle = new Array(
    //     "ProjectFillBudgetExec",  //預算執行情形
    //     "ProjectFillField", //實地查證情形
    //     "ProjectFillOther", //其他資訊
    //     "ProjectFillClose", //結案資料
    //     // "ProjectFillExecuteSubmit" //執行情形送出
    // );

    result.map(x => {
        if (x.CHAPTER_ID == "ProjectFillRefFile") {
            x.content = getDownLoadFileList(file)
        }
        // if (customfunStyle.includes(x.CHAPTER_ID))
        //     x.className = "k-header_yellow";
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
import { GetSetParam, getPlanYearList, getOrganList, getCodeTownByCityId, getInnPropsalType } from "../../../Basic/CommonService";
import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { openChapterPage } from "../../../Basic/SDOExtension";
import { Download } from "../../../Basic/Download";
import { IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得創新提案管理清單
 * @param {*} requestModel 查詢條件
 * @returns 
 */
export const getProjectList = async (requestModel) => {
    let url = APIUrl + 'ProjectManage/GetInnProjectManage';
    let response = await api.Post(url, JSON.stringify(requestModel), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}


/**
 * 儲存資料資料
 * @param {*} data 
 * @returns 
 */
export const DeleteInnProject = async (data) => {
    let result = null;
    let url = APIUrl + 'InnProject/SaveInnBasic';
    
    let response = await api.Post(url, JSON.stringify(data), null, false);
    if (response.ok) {
        result = await response.json();
    }
    return result;
}


/**
 * 開啟章節表
 * @param {*} item  
 * @returns 
 */
export const openProjectChapter = async (item) => {
    const INN_PLAN_NO = item.PLAN_NO;
    let data = {
        projectNo: INN_PLAN_NO,
        projectName: item.PLAN_NAME,
        IsManage:item.IsManage
    };
    let title = "提案登錄"
    // 開啟章節表(同一個計畫只會有一個分頁，用PROJECT_NO當NAME)
    const url = process.env.PUBLIC_URL + '/ProjectChapter';
    openChapterPage(url, INN_PLAN_NO, `${title}(${INN_PLAN_NO})`, data);
}

export const downProjectPrint = async(model)=>{
    let url = APIUrl + 'InnRPT/ProjectPrint'
    Download(url, 'POST', model);
}

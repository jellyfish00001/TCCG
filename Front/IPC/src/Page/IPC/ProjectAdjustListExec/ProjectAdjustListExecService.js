import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import { getPlanYearList, GetSetParam, SetDelFlgName } from "../../../Basic/CommonService";
import { openChapterPage } from "../../../Basic/SDOExtension";
import { GetBasicData } from '../../../Basic/BasicData';
import { loadCheckPoint } from "../ProjectList/ProjectListService";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 調整撤銷狀態預設值 => A01(基本資料調整中)、A03(基本資料調整退回)、B01(期程調整中)、B03(期程調整退回)、W03(撤銷退回)
 */
export const projAwStatusDefaultList = ['A01', 'A03', 'B01', 'B03', 'W03'];

/**
 * 初始篩選條件
 */
export const initQueryData = {
    PROJECT_YEAR: 0,
    PROJECT_NAME: "",
    PROJECT_AW_STATUS: [],
    PROJECT_NO: "",
    CP_KIND: "",
    RUNWAY_C: "",
    SPEC_NOTE: "",
    refreshCnt: 0
};

/**
 * 取得計畫列表
 * @param {*} requestModel 篩選條件
 * @param {boolean} isReturnPromise 是否回傳Promise
 * @returns 
 */
export const getProjectList = async (requestModel, isReturnPromise = false) => {
    let url = APIUrl + 'ProjectAdjust/GetAdjustList';
    requestModel = { ...requestModel, IsReview: 0 };
    let response = await api.Post(url, JSON.stringify(requestModel), null, false);
    if (isReturnPromise) {
        return response;
    }
    if (response.ok) {
        return response.json();
    }
    return [];
}

/**
 * 取得頁面所需資料
 * @param {*} requestModel 篩選條件
 * @returns 
 */
export const getPageData = async (requestModel) => {
    let responses = await Promise.all([
        // 取得申請項目    
        GetSetParam("AW_KIND", "", true),
        getProjectList(requestModel, true)
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    return result;
}

/**
 * 取得所有查詢條件下拉式選單
 * @returns 
 */
export const getAllDropDowns = async () => {
    let responses = await Promise.all([
        GetSetParam('PROJECT_AW_STATUS', '', true),      // 取得調整撤銷狀態
        GetSetParam('CP_KIND', '', true),                // 取得執行方式類別
        GetSetParam('SPEC_NOTE', '', true, null),        // 取得特殊加註
        getPlanYearList(true),                           // 取得計畫年度
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));
    if (result) {
        //設定停用字眼
        SetDelFlgName(result[2]);
        //加入預設值請選擇
        result = addDefautItem(result);
        return result;
    } else {
        return [];
    }
}

/**
 * 加入下拉選單預設值
 * @param {array} datas 下拉選單資料
 * @returns array
 */
export const addDefautItem = (datas) => {
    let newDataList = [];
    //加入預設值請選擇
    datas.forEach(dropDownData => {
        if (dropDownData.length > 0) {
            let newDropDownData = [];
            // 將下拉清單資料Key,Value固定為Text,Value
            if (dropDownData[0].text == undefined || dropDownData[0].value == undefined) {
                newDropDownData = dropDownData.map(x => { return { text: x.SET_VALUE, value: x.SET_TYPE } })
            } else {
                newDropDownData = [...dropDownData];
            }

            // 調整撤銷狀態多選不需加入請選擇
            if (dropDownData != datas[0])
                newDropDownData.unshift({ text: '請選擇', value: "" })
            newDataList.push(newDropDownData)
        }
    });
    return newDataList;
}

/**
 * 取得執行方式
 * @param {*} cpKind 執行方式類別
 * @returns 
 */
export const getRunWayC = async (cpKind) => {
    return await loadCheckPoint(cpKind);
}

/**
 * 取得計畫名稱下拉選單
 * @param projName 篩選的計畫名稱
 * @returns 
 */
export const getProjectNameDropDown = async (projName) => {
    let url = APIUrl + 'ProjectList/GetProjectList';
    let requestModel = { PROJECT_NAME: projName, PROJECT_STATUS: ["4"], isAdjustList: 1 };
    let response = await api.Post(url, JSON.stringify(requestModel), null, false);
    const orgId = await GetBasicData("orgId");
    let result = null;
    if (response.ok) {
        result = await response.json();
        result = result.filter(x => x.EXEC_ORGAN_C == orgId);
    }
    return result;
}

/**
 * 存檔 新增主辦申請調整計畫
 * @returns 
 */
export const AddAdujustExec = async (data) => {
    let url = APIUrl + 'ProjectAdjust/AddAdujustExec/';
    let form = new FormData();
    form.append('PROJECT_NO', data.PROJECT_NO);
    form.append('AW_KIND', data.AW_KIND);
    let response = await api.Post(url, form, new Headers(), false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 主辦取消調整
 * @param {*} PROJECT_NO 列管編號
 * @param {*} PROJ_ADJ_ID 調整流水號
 * @param {*} AW_KIND 調整項目
 * @returns 
 */
export const SaveExecCancel = async (PROJECT_NO, PROJ_ADJ_ID, AW_KIND) => {
    let url = APIUrl + 'ProjectAdjust/SaveExecCancel/';
    let form = new FormData();
    form.append('PROJECT_NO', PROJECT_NO);
    form.append('PROJ_ADJ_ID', PROJ_ADJ_ID);
    form.append('AW_KIND', AW_KIND);
    let response = await api.Post(url, form, new Headers(), true);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 開啟章節表(for 調整撤銷用)
 * @param {object} item 所需參數 AW_KIND、PROJ_ADJ_ID、PROJECT_NO、PROJECT_NAME
 */
export const openAdjustProjectChapter = async (item) => {
    const { funRole, AW_KIND, PROJ_ADJ_ID, PROJECT_NO, PROJECT_NAME, isShowBtn = true } = item;

    let operation = '';
    switch (AW_KIND) {
        case 'AW01': operation = 'A1'; break;
        case 'AW02': operation = 'A2'; break;
        case 'AW03': operation = 'A3'; break;
    }

    let data = {
        funRole: funRole, //功能面向角色
        operation: operation,
        awKind: AW_KIND,
        projAdjId: PROJ_ADJ_ID,
        projectNo: PROJECT_NO,
        projectName: PROJECT_NAME,
        isShowBtn: isShowBtn,
    };

    // 開啟章節表(同一個計畫只會有一個分頁，用PROJ_ADJ_ID當NAME)
    const url = process.env.PUBLIC_URL + '/ProjectChapter';
    openChapterPage(url, PROJ_ADJ_ID, `調整撤銷(${PROJECT_NO})`, data)
}

/**
 * 取得審核結果名稱
 * @param {string} reviewResult 審核結果代碼
 * @returns 
 */
export const getReviewResult = (reviewResult) => {
    let reviewResultName = "";
    switch (reviewResult) {
        case 'Y': reviewResultName = "審核通過"; break;
        case 'N': reviewResultName = "審核未通過"; break;
        case 'R': reviewResultName = "退回修正"; break;
    }
    return reviewResultName;
}

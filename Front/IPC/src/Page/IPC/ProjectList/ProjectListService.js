import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig, showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { getCurrentCycleData, getOrganList, getPlanYearList, GetSetParam, SetDelFlgName, CheckIsHANDRole, getOrgByUsr } from "../../../Basic/CommonService";
import { openChapterPage } from "../../../Basic/SDOExtension";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

//取得所有查詢條件下拉式選單
export const getAllDropDowns = async () => {
    let checkIsHANDRole = await CheckIsHANDRole();
    let responses = await Promise.all([
        GetSetParam('PROJECT_STATUS', '', true),         // 取得作業階段
        GetSetParam('CP_KIND', '', true),                // 取得執行方式
        GetSetParam('SPEC_NOTE', '', true, null),        // 取得特殊加註
        checkIsHANDRole ? getOrgByUsr() : getOrganList(),// 取得機關清單
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
const addDefautItem = (datas) => {
    let newDataList = [];
    //加入預設值請選擇
    datas.forEach(dropDownData => {
        if (dropDownData.length > 0) {
            let newDropDownData = [];
            // 將下拉清單資料Key,Value固定為Text,Value
            if (dropDownData[0].text === undefined || dropDownData[0].value === undefined) {
                newDropDownData = dropDownData.map(x => { return { text: x.SET_VALUE, value: x.SET_TYPE } })
            } else {
                newDropDownData = [...dropDownData];
            }

            // 計畫狀態多選不需加入請選擇
            if (dropDownData !== datas[0] && dropDownData !== datas[4])
                newDropDownData.unshift({ text: '請選擇', value: "" })
            newDataList.push(newDropDownData)
        }
    });
    return newDataList;
}

/**
 * 取得執行方式
 * @param {*} cpKind 
 * @returns 
 */
export const loadCheckPoint = async (cpKind) => {
    let url = APIUrl + 'SetParam/GetCodeCheckpoint';
    let formData = new FormData();
    formData.append('CP_KIND', cpKind);
    formData.append('isShowDel', true);
    let response = await api.Post(url, formData, new Headers(), false);
    if (response.ok) {
        let result = await response.json();
        if (result.length > 0) {
            let rtnResult = result.map(x => {
                return {
                    text: `${x.CHECKPOINT_CLASS}${x.DEL_FLG ? "(已停用)" : ""}`,
                    value: x.CHECKPOINT_CLASS_ID.toString()
                }
            });
            rtnResult.unshift({ value: "", text: '請選擇' })
            return rtnResult;
        }
    }
    return [];
}

/**
 * 取得計畫列表
 * @param {*} requestModel 查詢條件
 * @returns 
 */
export const getProjectList = async (requestModel) => {
    let url = APIUrl + 'ProjectList/GetProjectList';
    let response = await api.Post(url, JSON.stringify(requestModel), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 釘選/取消釘選 計畫
 * @param {*} projectNo
 * @param {*} pisSelectVal
 * @returns 
 */
export const updateProjectPisSelect = async (projectNo, pisSelectVal) => {
    let url = APIUrl + 'ProjectList/AddDelFavoriateProject';
    let form = new FormData();
    form.append('PROJECT_NO', projectNo);
    form.append('PIS_SELECT', pisSelectVal);
    let response = await api.Post(url, form, new Headers(), false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 刪除計畫
 * @param {*} projectNos 
 * @returns 
 */
export const saveProjectCanceled = async (projectNos) => {
    let url = APIUrl + 'ProjectList/SaveProjectCanceled';
    let response = await api.Post(url, JSON.stringify(projectNos))
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

/**
 * 開啟章節表
 * @param {*} item 
 * @param {*} role 
 * @returns 
 */
export const openProjectChapter = async (item, role) => {
    //#region 資料處理
    const { PROJECT_NO, PROJECT_NAME, PROJECT_STATUS_C, EXEC_ORGAN_C } = item;
    const CHPageInfo = getChapterPage(PROJECT_STATUS_C, role);
    // 取得週期資料
    let cycleData = await getCurrentCycleData();

    if (cycleData == null) {
        showGlobalMessageBox("週期資料有誤，請洽系統管理員");
        return
    }

    let data = {
        funRole: role, //功能面向角色
        operation: CHPageInfo.operation,
        workStage: CHPageInfo.workStage,
        projectNo: PROJECT_NO,
        projectName: PROJECT_NAME,
        chapter: CHPageInfo.chapter,
        cycleData: `${cycleData.PROJECT_YEAR}_${cycleData.PROJECT_MONTH}`,
        execOrgan: EXEC_ORGAN_C
    };
    //#endregion 

    let title = "";
    switch (role) {
        case 0:
            title = "資料登錄"; break;
        case 2:
            title = "計畫審查"; break;
        default:
            break;
    }

    // 開啟章節表(同一個計畫只會有一個分頁，用PROJECT_NO當NAME)
    const url = process.env.PUBLIC_URL + '/ProjectChapter';
    openChapterPage(url, PROJECT_NO, `${title}(${PROJECT_NO})`, data);
}

/**
 * 取得計畫狀態對應章節表階段
 * @param {*} projectStatus 作業階段
 * @param {*} funRole 0:填報;2:管考
 * @returns 
 */
export const getChapterPage = (projectStatus, funRole) => {
    let operation = 'P1';
    let workStage = 'S1';
    let chapter = '';
    switch (projectStatus) {
        // 立案審核
        case '2':
            workStage = 'S1';
            // 管考，需預設"立案審核"章節
            if (funRole === 2) {
                chapter = 'ProjectFillAddAudit';
            }
            break;
        // 立案退回
        case '3':
            workStage = 'S1';
            // 填報，需預設"立案送審"章節
            if (funRole === 0) {
                chapter = 'ProjectFillAddSubmit';
            }
            break;
        // 執行情形
        case '4':
            workStage = 'S2'; break;
        // 結案審核
        case '5':
            workStage = 'S2';
            // 管考，需預設"結案審核"章節
            if (funRole === 2) {
                chapter = 'ProjectFillCloseAudit';
            }
            break;
        // 結案退回
        case '6':
            workStage = 'S2';
            // 填報，需預設"執行情形送出"章節
            if (funRole === 0) {
                chapter = 'ProjectFillExecuteSubmit';
            }
            break;
        default:
            break;
    }
    return {
        operation: operation,
        workStage: workStage,
        chapter: chapter
    }
}
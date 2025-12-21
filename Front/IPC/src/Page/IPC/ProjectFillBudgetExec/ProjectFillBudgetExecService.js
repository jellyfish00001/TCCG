import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { GetSetParam } from "../../../Basic/CommonService";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得所有參數資料
 * @returns 
 */
const getAllDdlData = async () => {
    let responses = await Promise.all([
        // 原因            
        GetSetParam("IPCBGTEXECFAILED", "", true),
        // 責任歸屬
        GetSetParam("IPCBGTEXECFAILEDDUTY", "", true),
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));
    return result;
}

/**
 * 取得計畫預算執行情形
 * @returns 
 */
const getProjectFillBudgetExec = async (projectNo) => {
    let url = APIUrl + 'ProjectExecute/GetProjectFillBudgetExec';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存計畫預算執行情形
 * @param {*} model 
 * @returns 
 */
const saveProjectFillBudgetExec = async (model) => {
    let url = APIUrl + 'ProjectExecute/SaveProjectFillBudgetExec';
    let response = await api.Post(url, JSON.stringify(model), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

const bgtExecInitData = {
    SEQ: 0,
    EXEC_YEAR: "",
    EXEC_MONTH: "",
    GT_EXPANDED_BUDGET: 0,
    GT_ACT_BUDGET: 0,
    GT_AP: 0,
    GT_BALANCE: 0,
    GT_TOTAL: 0,
    GT_EXEC_RATE: 0,
    YEAR_BUDGET_EXPANDED: 0,
    YEAR_BUDGET_ALLOCATED: 0,
    YEAR_EXEC_BUDGET: 0,
    YEAR_EXEC_RATE: 0,
    EXEC_RATE_FAILED_NOTE: "",
    EXEC_NOTE: "",
    FailedMappingData: [],
    FailedDutyMappingData: [],
    failedSelectedData: [],
    dutySelectedData: [],
}

const initData = {
    // 填報週期
    ProjectFillCycle: {},
    // 預算執行情形累計執行情形
    ProjectBudgetExecute: [],
}

const ProjectFillBudgetExecService = {
    getAllDdlData: getAllDdlData,
    getProjectFillBudgetExec: getProjectFillBudgetExec,
    saveProjectFillBudgetExec: saveProjectFillBudgetExec,
    initData: initData,
    bgtExecInitData: bgtExecInitData,
}
export default ProjectFillBudgetExecService;
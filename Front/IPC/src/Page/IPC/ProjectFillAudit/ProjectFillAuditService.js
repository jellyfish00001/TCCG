import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { GetSetParam, getPlanYearList } from "../../../Basic/CommonService";
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

const getAllDropDowns = async () => {
    let responses = await Promise.all([
        // 特殊加註
        GetSetParam("SPEC_NOTE", "", true),
        // 會議種類
        GetSetParam("COM_CONFERENCEGENRE", "", true),
        // 分案或併案
        GetSetParam("PROMERGESTATUS", "", true),
        // 管考備註項目
        GetSetParam("COM_IPCMEMO", "", true),
        // 年度
        getPlanYearList(true),
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));
    return result;
}

/**
 * 取得平時管考
 * @returns 
 */
const getProjectFillAudit = async (projectNo) => {
    let url = APIUrl + 'ProjectExecute/GetProjectFillAudit';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存平時管考
 * @param {*} models 
 * @returns 
 */
const saveProjectFillAudit = async (models) => {
    let url = APIUrl + 'ProjectExecute/SaveProjectFillAudit';
    let response = await api.Post(url, JSON.stringify(models), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

/**
 * 管考意見寄信
 * @param {*} projectNo 
 * @returns 
 */
const sendProjectAuditOpinionMail = async (projectNo) => {
    let url = APIUrl + 'ProjectExecute/SendProjectAuditOpinionMail';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

const ProjectBasicInitData = {
    // 計畫編號
    PROJECT_NO: "",
    // 結案日期
    FINISH_DATE: null,
    // 計畫狀態
    PROJECT_STATUS: "",
    // 初審成績(A)
    SCORE_A: null,
    // 年度考核備註
    NOTES_FOR_BUDGET: "",
    // 其他管考備註
    NOTES_FOR_SCHEDULE: "",
}

const initData = {
    // 計畫基本資料
    ProjectBasic: ProjectBasicInitData,
    // 管考審核意見
    ProjectEngineeringAuditOpinion: [],
    // 會議列管
    ProjectConference: [],
    // 逾期繳交填報紀錄
    ProjectDelayfill: [],
    // 計畫分併案記錄檔
    ProjectMergeLog: [],
    // 撤銷資料
    RevokeData: {},
    // 特殊加註計畫參數值對應資料
    SpecNoteMappingData: [],
    // 實地查證情形
    ProjectFactFinding: [],
    // 計畫結案明細資料
    ProjectCloseDetails: [],
    // 未於期限內提出計畫調整資料
    DelayApply: [],
}

// 會議列管欄位驗證
const validateConferenceField = Yup.object().shape({
    CONFERENCE_GENRE: Yup.string().required("此欄位為必填").nullable(),
    CONFERENCE_TIME: Yup.date().required("此欄位為必填").nullable(),
    CONFERENCE_NUM: Yup.number().integer("須為整數").required('此欄位為必填').nullable(),
});

// 逾期繳交填報紀錄欄位驗證
const validateDelayfillField = Yup.object().shape({
    FILL_TIME: Yup.date().required("此欄位為必填").nullable(),
    FILL_REASON: Yup.string().required("此欄位為必填").nullable(),
});

// 計畫分併案記錄檔紀錄欄位驗證
const validateMergeLogField = Yup.object().shape({
    MERGE_STATUS: Yup.string().required("此欄位為必填").nullable(),
    PROMERGE_DATE: Yup.date().required("此欄位為必填").nullable(),
});

// 實地查證情形欄位驗證
const validateFactFindingField = Yup.object().shape({
    FFDATE: Yup.date().required("此欄位為必填").nullable(),
    FFCOMMENT: Yup.string().required("此欄位為必填").nullable(),
});

// 結案明細資料欄位驗證
const validateCloseDetailsField = Yup.object().shape({
    REF_MEMO: Yup.string().required("此欄位為必填").nullable(),
});

// 計畫基本資料欄位驗證
const validateProjectBasicField = Yup.object().shape({
    SCORE_A: Yup.number().integer("須為整數").nullable(),
});

const ProjectFillAuditService = {
    getAllDropDowns: getAllDropDowns,
    getProjectFillAudit: getProjectFillAudit,
    saveProjectFillAudit: saveProjectFillAudit,
    sendProjectAuditOpinionMail: sendProjectAuditOpinionMail,
    initData: initData,
    ProjectBasicInitData: ProjectBasicInitData,
    validateConferenceField: validateConferenceField,
    validateDelayfillField: validateDelayfillField,
    validateMergeLogField: validateMergeLogField,
    validateFactFindingField: validateFactFindingField,
    validateCloseDetailsField: validateCloseDetailsField,
    validateProjectBasicField: validateProjectBasicField,
}
export default ProjectFillAuditService;
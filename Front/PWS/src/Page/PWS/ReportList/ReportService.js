import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { FormatDate } from '../../../Basic/SDOExtension';
import { Download } from "../../../Basic/Download";
import { getPlanYearList, GetSetParam, getOrganList, getFundOrgList } from "../../../Basic/CommonService";
import { GetBasicData } from "../../../Basic/BasicData";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 報表清單
 * @returns getPlanYearList
 */
const getStatisticsList = () => {
    const result = [
        {RPT_ID:"RPTProjectPolicyReview", ID: 1, NAME: "重大施政計畫先期審查表", MEMO: "" },
        {RPT_ID:"RPTProjectPolicyPublicBudgetReview", ID: 2, NAME: "重大施政計畫專案小組先期審查結果彙整表（局處計畫列表）公務預算", MEMO: "" },
        {RPT_ID:"RPTPolicyPublicBudgetReview", ID: 2, NAME: "重大施政計畫專案小組先期審查結果彙整表（局處計畫列表）公務預算", MEMO: "" },
        {RPT_ID:"RPTProjectPolicyFundBudgetReview", ID: 3, NAME: "重大施政計畫專案小組先期審查結果彙整表（局處計畫列表）基金預算", MEMO: "" },
        {RPT_ID:"RPTPolicyFundBudgetReview", ID: 3, NAME: "重大施政計畫專案小組先期審查結果彙整表（局處計畫列表）基金預算", MEMO: "" },
        {RPT_ID:"RPTAllProjectOrgResultList", ID: 4, NAME: "重大施政計畫審查結果彙整表（全部局處總表）", MEMO: "" },
        {RPT_ID:"RPTAllProjectFundResultList", ID: 5, NAME: "重大施政計畫審查結果彙整表（全部基金總表）", MEMO: "" },
        {RPT_ID:"RPTProjectList", ID: 6, NAME: "辦理性別影響評估計畫一覽表", MEMO: "" },
        {RPT_ID:"RPTProjectEntList", ID: 7, NAME: "先期審查-委託研究計畫先期審查計畫表(管考+機關)", MEMO: "" },
        {RPT_ID:"RPTProjectEntMg", ID: 8, NAME: "先期審查-委託研究計畫審查結果彙整表(管考)", MEMO: "" },
        {RPT_ID:"RPTProjectEntOrg", ID: 9, NAME: "先期審查-委託研究計畫審查結果彙整表(機關)", MEMO: "" },
        
    ]
    return result;
}

/**
 * 組下拉選單資料
 * @returns 
 */
const getPageDropData = async () => {
    // 年度
    let dropDowns  = await getPlanYearList(false, 10, "B");
    dropDowns.unshift({ text: "114", value: "114" });
    // 取是否送審下拉
    let IsDropDown = await GetSetParam('AUDIT_STATUS', "");
    IsDropDown = IsDropDown.map(item => {
        return {
            ...item,
            SET_TYPE: parseInt(item.SET_TYPE)
        };
    });
    IsDropDown.unshift({ SET_VALUE: "請選擇", SET_TYPE: null });
    // 取是否送審下拉
    let IsSendDropDown = await GetSetParam('PROJECT_STATUS', "");
    IsSendDropDown = IsSendDropDown.map(item => {
        return {
            ...item,
            SET_TYPE: parseInt(item.SET_TYPE)
        };
    });
    IsSendDropDown.unshift({ SET_VALUE: "請選擇", SET_TYPE: null });
    // 取得機關下拉選單
    let organList = await getOrganList();
    // 移除organList資料中text最後三個字為"區公所"的選項
    organList = organList.filter(option => !option.text.endsWith("區公所"));
    // 基金機關
    let fundOrg = await getFundOrgList();
    return {
        PLAN_YEAR: dropDowns,
        IS_SEND: IsSendDropDown,
        SEND_STATUS: IsDropDown,
        AUDIT_STATUS: IsDropDown,
        OU_ID: organList,
        FUND_OU_ID: fundOrg,
    };
}

// 紀錄上個月日期
let lastMonDate = new Date();
lastMonDate.setMonth(lastMonDate.getMonth() - 1, 1);

/**
 * 腳色是否為總管
 */
const roleType = async () => {
    let roles = await GetBasicData('allRoles');
    if(roles.length > 0)
    return roles[0].ROLE_ID == 'RDEC_MGR_ROL_PWS'
}

/**
 * 查詢條件初始值
 */
const initData = {
    // 計畫年度
    PWS_YEAR: (new Date().getFullYear()- 1911 + 1).toString(),
    IS_SEND: null,
    SEND_STATUS: null,
    AUDIT_STATUS: null,
    OU_ID: null,
    FUND_OU_ID: null,
    CREATEUNITOUID: null,
}

/**
 * 下載報表
 * @param {string} data 資料
 */
export const exportRPT = async (data) => {
    let url = APIUrl + 'PWSRPT/RPTPWSReport';
    Download(url, 'POST', data);
}

/**
 * 查詢機關的單位
 * @param {*} OU_ID 
 * @returns 
 */
export const getUnitList = async (OU_ID) => {
    let url = APIUrl + 'DropDown/GetUnitList';
    let response = await api.Post(url, JSON.stringify(OU_ID), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

const ReportService = {
    getStatisticsList: getStatisticsList,
    getPageData: getPageDropData,
    initData: initData,
    exportRPT,
    Ischoose:roleType,
}

export default ReportService;
import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import { getPlanYearList, getMonthList, GetSetParam, getFundList, getOrganList } from "../../../Basic/CommonService";
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得計畫基本資料
 * @param {*} planNo //計畫編號
 * @returns 
 */
export const GetPlanBasicA = async (planNo) => {
    let url = APIUrl + 'PlanBasicA/GetPWSSDPLANMAIN';
    let response = await api.Post(url, JSON.stringify(planNo), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
        result.PlanCrossAMTA = AddNoColumn(result.PlanCrossAMTA);
    }
    return result;
};

/**
 * 儲存計畫基本資料
 * @param {*} data  //計畫基本資料
 * @returns 
 */
export const SavePWSSDPLANMAIN = async (data) => {
    let url = APIUrl + 'PlanBasicA/SavePWSSDPLANMAIN';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 儲存計畫基本資料
 * @param {*} data  //計畫基本資料
 * @returns 
 */
export const SavePWSSDPLANMAINB = async (data) => {
    let url = APIUrl + 'PlanBasicB/SavePWSSDPLANMAIN';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 處理年度下拉選單(西元轉民國年分)
 * @param {*} dateString 
 * @returns 
 */
const getYearString = (dateString) => {
    if (!dateString) {
        const currentYear = new Date().getFullYear() - 1911;
        return currentYear.toString();
    }
    const date = new Date(dateString);
    const year = date.getFullYear() - 1911;
    return year.toString();
};

/**
 * 處理月份下拉選單(取月分)
 * @param {*} dateString 
 * @returns 
 */
const getMonthString = (dateString) => {
    if (!dateString) {
        return 1;
    }
    const date = new Date(dateString);
    const month = date.getMonth() + 1;
    return month;
};

/**
 * 取畫面資料及下拉選單處裡
 * @param {*} projectNo 
 * @returns 
 */
export const getData = async (projectNo) => {
    // 取得年度下拉選單
    let year = await getPlanYearList(false, 20, "A");
    // 取得月份下拉選單
    let month = await getMonthList();
    // 取得基金下拉選單
    let fundList = await getFundList();
    // 取得機關下拉選單
    let organList = await getOrganList();
    // 移除organList資料中text最後三個字為"區公所"的選項
    organList = organList.filter(option => !option.text.endsWith("區公所"));
    // 取得執行類別
    let cpKindData = await GetSetParam('CP_KIND', '');
    // 取得表單資料
    let result = await GetPlanBasicA(projectNo);
    return {
        yearOptions: year,
        monthOptions: month,
        fundOptions: fundList,
        organOptions: organList,
        cpKindDropdown: cpKindData,
        formData: {
            ...result,
            startYear: getYearString(result.PLANSTARTDATE),
            startMonth: getMonthString(result.PLANSTARTDATE),
            endYear: getYearString(result.PLANENDDATE),
            endMonth: getMonthString(result.PLANENDDATE),
            thisYearMoney: result.PUBLICMONEY+result.FUNDMONEY+result.CENTERMONEY+result.OTHERMONEY,
            RUNWAY_C: result.RUNWAY_C == null ? "" : result.RUNWAY_C,
            OU_ID: result.ORGOUNAME.endsWith("區公所")  && result.OU_ID == result.CREATEORGOUID ? null : result.OU_ID,
        },
        isCross: getYearString(result.PLANSTARTDATE) !== getYearString(result.PLANENDDATE),
    };
};

// 檔案上傳預設值
export const initFiles ={
    PROJECT_NO: "",
    FILE_KIND: "",
    EditFiles: [],
}

/**
 * 日期欄位檢查
 * @param {*} value
 * @returns 
 */
export const validationSchema = Yup.object().shape({
    startYear: Yup.string(),
    startMonth: Yup.string(),
    endYear: Yup.string(),
    endMonth: Yup.string().test(
        'dateRange',
        '結束日期必須大於或等於開始日期',
        function(value) {
            const { startYear, startMonth, endYear } = this.parent;
            // 確保所有相關欄位都有值
            if (startYear && startMonth && endYear && value) {
                const startDate = new Date(`${startYear}-${startMonth}-01`);
                const endDate = new Date(`${endYear}-${value}-01`);
                return endDate >= startDate;
            }
            // 如果某些欄位沒有值，則不進行該驗證
            return true;
        }
    ),
});

/**
 * 儲存跨年度計畫
 * @param {*} data  //跨年度資料
 * @returns 
 */
export const saveCrossAMTA = async (data) => {
    let url = APIUrl + 'PlanBasicA/SaveCrossAMTA';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 取得執行方式
 * @param {*} cpKind 
 * @param {*} isShowDel 
 * @returns 
 */
export const loadCheckPoint = async (cpKind, isShowDel) => {
    let url = APIUrl + 'SetParam/GetCodeCheckpoint';
    let formData = new FormData();
    formData.append('CP_KIND', cpKind);
    formData.append('isShowDel', isShowDel);
    let response = await api.Post(url, formData, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
        if (result.length > 0) {
            result.unshift({ CHECKPOINT_CLASS_ID: "", CHECKPOINT_CLASS: '請選擇' })
        }
    }
    return result;
};

/**
 * 取得自訂檢核點
 * @param {*} CheckPointClassId 
 * @returns 
 */
export const loadCusItem = async (CheckPointClassId) => {
    let url = APIUrl + 'SetParam/GetCusChkItem';
    let formData = new FormData();
    formData.append('CHK_POINT_CLASS_ID', CheckPointClassId);
    formData.append('forSettings', false);
    let response = await api.Post(url, formData, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return AddNoColumn(result);
};

/**
 * 取得預算來源
 * @param {*} levelMark 1:本府預算、2:中央部會
 * @returns 
 */
export const getCodePlanItem = async (levelMark) => {
    let url = APIUrl + 'SetParam/GetCodePlanItem';
    let form = new FormData();
    form.append('LEVEL_MARK', levelMark);
    form.append('DEL_FLG', false);
    let response = await api.Post(url, form, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 跨年度計畫下拉選單資料判斷
 * @param {*} value 
 * @returns 
 */
export async function handleDropDownKindChange(value) {
    let data;
    let planItems = [];

    switch (value) {
        case '1':
            // 本府預算
            data = await getCodePlanItem('1');
            planItems = data.map(item => ({ text: item.PLAN_ITEM_NAME, value: item.PLAN_ITEM_ID }));
            break;
        case '2':
            // 中央部會
            data = await getCodePlanItem('2');
            planItems = data.map(item => ({ text: item.PLAN_ITEM_NAME, value: item.PLAN_ITEM_ID }));
            break;
        case '3':
            // 基金預算
            data = await getFundList();
            planItems = data.map(item => ({ text: item.text, value: item.value == null ? "" : item.value.toString() }));
            break;
        case '4':
            // 預算來源
            data = await GetSetParam('BUDGETTYPE', '');
            planItems = data.map(item => ({ text: item.SET_VALUE, value: item.SET_TYPE }));
            break;
        default:
            planItems = [{ text: "請選擇來源", value: "" }];
            break;
    }
    if(value !== '3'){
        planItems.unshift({ text: '請選擇', value: "" });
    }
    return planItems;
}

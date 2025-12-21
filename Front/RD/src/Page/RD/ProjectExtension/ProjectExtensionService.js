import { ServerConfig } from '../../../Basic/BasicData';
import { api } from '../../../Basic/ApiFetch';
import * as Yup from 'yup';
import { FormatDate, IsNullOrEmpty, AddNoColumn } from "../../../Basic/SDOExtension";

// URL
const APIUrl = ServerConfig.backEndUrl;

// 檔案初始資料
export const initFiles = {
    PROJECT_NO: "",
    FILE_KIND: "", // SET_PARAM.SET_ITEM='FILE_KIND'
    EditFiles: [],
};

/**
 * 透過計畫編號取得 展延紀錄清單
 * @param {string} planNo 計畫編號
 */
export const GetRDExtensionList = async (planNo) => {
    let url = APIUrl  + 'ProjectExtension/GetRDExtensionList';
    let form = new FormData();
    form.append('PLAN_NO', planNo);
    let response = await api.Post(url, form, new Headers(), false)
    if (response.ok) {
        let data = await response.json();
        // 重組清單資料：1.組原計畫期程資料並轉民國年 2.組調整計畫期程資料並轉民國年 3.加上序號
        const newData = data.map((obj, index) => ({
            ...obj,
            ORG_PLAN_DATE: ProcessPlanDate(obj.PLAN_START_DATE, obj.PLAN_END_DATE),
            ADJ_PLAN_DATE: ProcessPlanDate(obj.PLAN_START_DATE, obj.EXTP_LANEND_DATE),
            // NO: index + 1,
            APPLY_DATE: FormatDate(obj.APPLY_DATE, "tYY年MM月"),
            REVIEW_RESULT: IsNullOrEmpty(obj.REVIEW_RESULT) ? "尚未審核" : data.REVIEW_RESULT,
        }));

        return AddNoColumn(newData);
    } else {
        return null;
    }
}

/**
 * 處理計畫起訖資料
 * @param {*} planStartDate 計畫期程 - 起
 * @param {*} planEndDate 計畫期程 - 迄
 */
const ProcessPlanDate = (planStartDate, planEndDate) => {
    // 若其中一個是空，則傳空字串
    if(IsNullOrEmpty(planStartDate) || IsNullOrEmpty(planEndDate)){
        return "";
    }else{
        // 組計畫期程資料並轉民國年
        return FormatDate(planStartDate, "tYY年MM月") + "~" + FormatDate(planEndDate, "tYY年MM月");
    }
}

/**
 * 透過展延編號取得 展延紀錄明細
 * @param {string} extensionNo 計畫流水號
 */
export const GetRDExtension = async (extensionNo) => {
    let url = APIUrl  + 'ProjectExtension/GetRDExtension';
    let form = new FormData();
    form.append('EXTENSION_NO', extensionNo);
    let response = await api.Post(url, form, new Headers())
    if (response.ok) {
        let result = await response.json();
        const yearMonthArr = getYearAndMonthInit(result.EXTP_LANEND_DATE);
        // 重組清單資料：1.組原計畫期程資料並轉民國年 2.組調整計畫期程資料並轉民國年
        const newResult = {
            ...result,
            ORG_PLAN_DATE: ProcessPlanDate(result.PLAN_START_DATE, result.PLAN_END_DATE),
            EXTP_LANEND_YEAR: yearMonthArr[0], // 調整計畫期程-年 
            EXTP_LANEND_MONTH: yearMonthArr[1] // 調整計畫期程-月
        };
        return newResult;
    }
}

/**
 * 將日期分解成年跟月下拉選單初始值
 * @param {*} date 日期
 * @return {Array} [yearInit, monthInit] 
 */
const getYearAndMonthInit = (date) => {
    let yearMonthArr = ["", ""];
    if(IsNullOrEmpty(date)){
        return yearMonthArr;
    }else{
        yearMonthArr[0] = (new Date(date).getFullYear() - 1911).toString();
        yearMonthArr[1] = new Date(date).getMonth() + 1; 
    }
    return yearMonthArr;
}

/**
 * 建立展延紀錄
 * @param {object} data {EXTENSION_YEAR: ...} 展延申請年
 */
export const SaveRDExtension = async (data) => {
    let url = APIUrl  + 'ProjectExtension/SaveRDExtension';
    let response = await api.Post(url, JSON.stringify(data), null)
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

/**
 * 儲存執行情形明細資料
 * @param {object} data form 資料
 */
export const SaveRDResPolicyIndex = async (data) => {
    let url = APIUrl  + 'ProjectExtension/SaveRDExtension';
    let response = await api.Post(url, JSON.stringify(data))
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

// 展延申請驗證規則
export const validationSchema = Yup.object().shape({
    EXT_REASON: Yup.string().nullable().required('展延原因必填').max(50),
    EXTP_LANEND_YEAR: Yup.string().nullable().required('調整計畫期程年度必填'),
    EXTP_LANEND_MONTH: Yup.string().nullable().required('調整計畫期程月份必填')
});

// 評核指標 GRID 資料驗證
export const validataPolicyField = Yup.object().shape({
    POLICY_KIND: Yup.string().required(),
    POLICY_INDEX_DESC: Yup.string().nullable().required().max(100, '字數限制100字以內'),
    RES_FINISH_DATE: Yup.string().required(),
    POLICY_EXTP_LANEND_DATE: Yup.string().required()
});
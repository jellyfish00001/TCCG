import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { IsNullOrEmpty, telRegex } from "../../../Basic/SDOExtension";
import * as Yup from 'yup';
// URL
const APIUrl = ServerConfig.backEndUrl;

// 檔案初始資料
export const initFiles = {
    PROJECT_NO: "",
    FILE_KIND: "", // SET_PARAM.SET_ITEM='FILE_KIND'
    EditFiles: [],
};

/**
 * 透過計畫編號取得計劃基本資料
 * @param {string} planNo 計畫編號
 * @returns {object} 計畫基本資料
 */
export const getPlanByNo = async (planNo) => {
    let url = APIUrl + 'ProjectBasic/GetRDResearchBasic';
    let form = new FormData();
    form.append('PLAN_NO', planNo);
    let response = await api.Post(url, form, new Headers());
    if (response.ok) {
        let result = await response.json();
        // 將計畫期程起跟迄拆解年跟月
        let newResult = {
            ...result,
            PLAN_START_YEAR: getYearAndMonthInit(result.PLAN_START_DATE)[0],
            PLAN_START_MONTH: getYearAndMonthInit(result.PLAN_START_DATE)[1],
            PLAN_END_YEAR: getYearAndMonthInit(result.PLAN_END_DATE)[0],
            PLAN_END_MONTH: getYearAndMonthInit(result.PLAN_END_DATE)[1],
        }
        return newResult;
    } else {
        return null;
    }
}

/**
 * 將日期分解成年跟月下拉選單初始值
 * @param {*} date 日期
 * @return {Array} [年初始值, 月初始值] 
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
 * 存檔送出儲存資料計畫（編輯）
 * @param {object} data 計畫
 * @returns {object} 結果物件{ message: ..., success: ...}
 */
export const savePlan = async (data) => {
    let url = APIUrl + 'ProjectBasic/SaveRDResearchBasic';
    let response = await api.Post(url, JSON.stringify(data), null, false);
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

// 基本資料驗證規則（評核指標 GRID 驗證另外寫）
export const validationSchema = Yup.object().shape({
    PLAN_NAME: Yup.string().nullable().required('研究名稱必填').max(100, '字數限制100字以內'),
    ENTRUST_UNIT_NAME: Yup.string().nullable().required('受託單位必填').max(100, '字數限制100字以內'),
    RESEARCH_NAME: Yup.string().nullable().required('研究主持人必填').max(50, '字數限制50字以內'),
    PLAN_START_YEAR: Yup.string().nullable().required('計畫期程起始年必填'),
    PLAN_START_MONTH: Yup.string().nullable().required('計畫期程起始月必填'),
    PLAN_END_YEAR: Yup.string().nullable().required('計畫期程結束年必填'),
    PLAN_END_MONTH: Yup.string().nullable().required('計畫期程結束月必填'),
    PLAN_CAUSE: Yup.string().nullable().required('研究原因及目的必填').max(2000, '字數限制2000字以內'),
    PLAN_CONTENT: Yup.string().nullable().required('計畫項目內容必填').max(2000, '字數限制2000字以內'),
    PLAN_EXPECTED: Yup.string().nullable().required('預期研究成果必填').max(2000, '字數限制2000字以內'),
    CONTACT_NAME: Yup.string().nullable().required('承辦人必填').max(50, '字數限制50字以內'),
    CONTACT_TEL: Yup.string().nullable().required('電話/分機必填').max(30, '字數限制30字以內').matches(telRegex, { message: '請輸入正確的電話格式' }),
    CONTACT_EMAIL: Yup.string().nullable().required('承辦人Email必填').email("E-mail格式不正確").max(60, '字數限制60字以內'),
    ASSIGNE_EMAIL: Yup.string().nullable().email("E-mail格式不正確").max(60, '字數限制60字以內'),
});

// 評核指標 GRID 資料驗證
export const validataPolicyField = Yup.object().shape({
    POLICY_KIND: Yup.string().nullable().required("此欄位為必填"),
    POLICY_INDEX_DESC: Yup.string().nullable().required().max(100, '字數限制100字以內'),
    RES_FINISH_DATE: Yup.string().nullable().required()
});
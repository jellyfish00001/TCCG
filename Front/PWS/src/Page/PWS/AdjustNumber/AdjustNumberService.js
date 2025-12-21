import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { getPlanYearList, GetSetParam, getFundList } from "../../../Basic/CommonService";
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得優先順序計畫基本資料
 * @param {*} data //from表單查詢條件
 * @returns 
 */
export const GetProjectList = async (data) => {
    let url = APIUrl + 'AdjustNumber/GetProjectList';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};
/**
 * 儲存計畫基本資料
 * @param {*} data  //grid資料
 * @returns 
 */
export const SetAdjustNumber = async (data) => {
    let url = APIUrl + 'AdjustNumber/SetAdjustNumber';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

// 下拉選單資訊取得
export const loadDropdownData = async () => {
    // 取得年度下拉選單
    const Year = await getPlanYearList(false, 20, "A");
    // 取得基金下拉選單
    const fundList = await getFundList();
    // 取得計畫類別
    const planType = await GetSetParam('PLAN_KIND', '');
    // 取得計畫狀態
    const StateOptions = await GetSetParam('PROJECT_STATUS', '');
    // 取得經費來源下拉選單
    const funding = await GetSetParam('BUDGETTYPE_NUM', '');
    return {
        yearOptions: Year,
        fundOptions: fundList,
        planTypeOptions: planType,
        stateOptions: [{ SET_VALUE: "請選擇", SET_TYPE: "" }, ...StateOptions],
        fundFromOptions: funding,
    };
};

/**
 * 驗證規則
 * @returns 
 */
export const validateFormData = (data) => {
    if (data.PLANKIND === "1") {
        return Yup.object().shape({
            BUDGETTYPE: Yup.string().required(),
            FUNDNO: Yup.string().when('BUDGETTYPE', {
                is: '2',
                then: Yup.string().required(),
                otherwise: Yup.string().nullable(),
            }),
        }).validate(data, { abortEarly: false });
    } else {
        return Yup.object().shape({}).validate(data, { abortEarly: false });
    }
};
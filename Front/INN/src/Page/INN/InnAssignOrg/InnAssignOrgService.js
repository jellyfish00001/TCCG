import { GetSetParam, getPlanYearList, getOrganList, getCodeTownByCityId } from "../../../Basic/CommonService";
import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { IsNullOrEmpty, SetMaskOnOff} from "../../../Basic/SDOExtension";
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得每年截止時間
 * @param {string} INN_YEAE 年度
 * @returns 
 */
export const getInnPlanDate = async (PLAN_YEAR) => {
    let url = APIUrl + 'AssignOrg/GetInnAssignOrg';
    let response = await api.Post(url, JSON.stringify(PLAN_YEAR));
    let result = {};
    if (response.ok) {
        result = await response.json();
    }
    return result;
}


/**
 * 年度下拉選單(改直接await getPlanYearList)
 * @param {string} INN_YEAE 年度
 * @returns 
 */
export const getInnPlanYear= async () => {
    let responses = await Promise.all([
        // 取得年度
        getPlanYearList(true, 10, "B"),
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));
    // 年度
    result[0].unshift({ text: "請選擇", value: "" });
    return result;
}


/**
 * 儲存資料
 * @param {*} savedData
 * @returns
 */
export const saveInnAssignOrg = async (savedData) => {
    let url = APIUrl + 'AssignOrg/SaveInnAssignOrg';
    let response = await api.Post(url, JSON.stringify(savedData));
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}





import { getPlanYearList, getOrganList, getOrgByUsr } from "../../../Basic/CommonService";
import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得下拉清單
 * @returns 
 */
const getAllDropDowns = async (checkIsHANDRole) => {
    let responses = await Promise.all([
        // 取得機關
        checkIsHANDRole ? getOrgByUsr() : getOrganList(),
        // 取得計畫年度
        getPlanYearList(true),
    ]);

    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    // 機關
    result[0].unshift({ text: "請選擇", value: "" });

    // 計畫年度
    result[1].unshift({ text: "請選擇", value: "" });
    return result;
}

/**
 * 取得落後案件清單
 * @param {*} formData 查詢條件
 */
const GetDelayPlan = async (formData) => {
    let url = APIUrl + 'Superior/GetDelayPlans';
    let response = await api.Post(url, JSON.stringify(formData));
    let result = [];
    if (response.ok) {
        result = await response.json();
    }

    return result;
}

const SuperiorDelayPlanService = {
    getAllDropDowns,
    GetDelayPlan
}

export default SuperiorDelayPlanService;

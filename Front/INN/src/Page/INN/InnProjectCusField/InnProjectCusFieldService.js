import { GetSetParam, getPlanYearList, getOrganList, getCodeTownByCityId } from "../../../Basic/CommonService";
import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { IsNullOrEmpty, SetMaskOnOff} from "../../../Basic/SDOExtension";
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得專題代號及名稱
 * @param {string} INN_YEAE 年度
 * @returns 
 */
export const getProjectCusField = async (PLAN_YEAR) => {
    let url = APIUrl + 'ProjectCusField/GetInnProjectCusField';
    let response = await api.Post(url, JSON.stringify(PLAN_YEAR));
    let result = {};
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 存檔
 * @param {string} requestData 
 * @returns 
 */
export const savedata = async (requestData) => {
    let url = APIUrl + 'ProjectCusField/SaveInnProjectCusField';
    let response = await api.Post(url, JSON.stringify(requestData));
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}
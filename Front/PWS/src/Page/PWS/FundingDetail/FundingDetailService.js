import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { getPlanYearList } from '../../../Basic/CommonService';
import { AddNoColumn } from '../../../Basic/SDOExtension';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

// 取得經費需求細項
export const GetDAMTB = async (data) => {
    let url = APIUrl + 'DAMTB/GetPWSSDPLANFUND';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return AddNoColumn(result) ;
};

// 存入經費需求細項
export const SaveDAMTB = async (data) => {
    let url = APIUrl + 'DAMTB/SavePWSSDPLANFUND';
    let response = await api.Post(url, JSON.stringify(data), null, true);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

// // 取得操作手冊清單
// const getUserGuides = async () => {
//     let url = APIUrl + 'UserGuide/GetUserGuides';
//     let response = await api.Post(url);
//     let result = [];
//     if (response.ok) {
//         result = await response.json();
//     }
//     return result;
// }

// /**
//  * 取得每年截止時間
//  * @param {string} INN_YEAE 年度
//  * @returns 
//  */
// export const getInnPlanDate = async (PLAN_YEAR) => {
//     let url = APIUrl + 'InnAssignOrg/GetInnAssignOrg';
//     let response = await api.Post(url, JSON.stringify(PLAN_YEAR));
//     let result = "";
//     if (response.ok) {
//         result = await response.json();
//     }
//     return result;
// }
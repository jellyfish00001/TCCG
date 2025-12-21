import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { getPlanYearList } from '../../../Basic/CommonService';
import { AddNoColumn } from '../../../Basic/SDOExtension';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得計畫基本資料
 * @param {*} data //from表單查詢條件
 * @returns 
 */
export const GetPWSProjectList = async (data) => {
    let url = APIUrl + 'ReviewList/GetPWSReviewList';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    let gridData = result.map(item => {
        if (item.PLANKIND == "1") {
            switch (item.PLANDATETYPE) {
                case "1":
                    return { ...item, PLANDATETYPE: "新興計畫" };
                case "2":
                    return { ...item, PLANDATETYPE: "延續性計畫" };
                case "3":
                    return { ...item, PLANDATETYPE: "延續性單一計畫" };
                default:
                    return item;
            }
        } else if (item.PLANKIND == "2") {
            switch (item.PLANDATETYPE) {
                case "1":
                    return { ...item, PLANDATETYPE: "單一年度計畫" };
                case "2":
                    return { ...item, PLANDATETYPE: "跨年度計畫" };
                default:
                    return item;
            }
        } else {
            return item;
        }
    });
    return gridData;
};

/**
 * 退回計畫
 * @param {*} data //from表單查詢條件
 * @returns 
 */
export const ReturnProject = async (data) => {
    let url = APIUrl + 'ReviewList/SetPWSProject';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

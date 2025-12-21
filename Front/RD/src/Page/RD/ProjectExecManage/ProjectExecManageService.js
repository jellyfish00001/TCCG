import { ServerConfig } from '../../../Basic/BasicData';
import { api } from '../../../Basic/ApiFetch';
import * as Yup from 'yup';
import { IsNullOrEmpty, AddNoColumn, FormatDate } from "../../../Basic/SDOExtension";

// URL
const APIUrl = ServerConfig.backEndUrl;

// 檔案初始資料
export const initFiles = {
    PROJECT_NO: "",
    FILE_KIND: "", // SET_PARAM.SET_ITEM='FILE_KIND'
    EditFiles: [],
};

/**
 * 透過計畫編號取得 參採情形/結案成果資料/續列管一年內參採情形
 * @param {string} planNo 計畫編號
 */
export const GetRDResPolicyList = async (planNo) => {
    let url = APIUrl  + 'ProjectExecution/GetRDResPolicyList';
    let form = new FormData();
    form.append('PLAN_NO', planNo);
    let response = await api.Post(url, JSON.stringify({'PLAN_NO':planNo}), null, false)
    if (response.ok) {
        let result = await response.json();
        // 重組清單資料：1.預定完成期程轉民國年 2.加上序號 3.若執行進度無資料，預設是空
        const newData = result.map((obj, index) => ({
            ...obj,
            RES_FINISH_DATE: FormatDate(obj["RES_FINISH_DATE"], "tYY/MM/DD"),
            PROGRESS_TYPE: IsNullOrEmpty(obj["PROGRESS_TYPE"]) ? obj["PROGRESS_TYPE"] : ""
        }));
        return AddNoColumn(newData);
    } else {
        return null;
    }
}

/**
 * 撈取執行情形明細資料
 * @param {int} seq 計畫流水號
 */
export const GetRDResPolicyIndex = async (seq) => {
    let url = APIUrl  + 'ProjectExecution/GetRDResPolicyIndex';
    let form = new FormData();
    form.append('SEQ', seq);
    let response = await api.Post(url, form, new Headers())
    if (response.ok) {
        let result = await response.json();
        // 如果沒有執行進度，給空值，讓下拉選單選一第一個
        if(IsNullOrEmpty(result["PROGRESS_TYPE"])){
            result["PROGRESS_TYPE"] = "";
        }
        return result;
    } else {
        return null;
    }
}

/**
 * 儲存執行情形明細資料
 * @param {object} data form 資料
 */
export const SaveRDResPolicyIndex = async (data) => {
    let url = APIUrl  + 'ProjectExecution/SaveRDResPolicyIndex';
    let response = await api.Post(url, JSON.stringify(data), null)
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

// 執行情形驗證規則
export const validationSchema = Yup.object().shape({
    PROGRESS_TYPE: Yup.string().nullable().required('執行進度必填').max(50),
    EXECUTION_DESC: Yup.string().nullable().required('執行情形簡述必填').max(2000, '字數限制2000字以內'),
    BEHIND_REASON: Yup.string().nullable().max(2000, '字數限制2000字以內'),
    SOLUTIONS: Yup.string().nullable().max(2000, '字數限制2000字以內')
});
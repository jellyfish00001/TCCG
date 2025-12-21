import { ServerConfig } from '../../../Basic/BasicData';
import { api } from '../../../Basic/ApiFetch';
import * as Yup from 'yup';

// URL
const APIUrl = ServerConfig.backEndUrl;

/**
 * 儲存審核紀錄
 * @param {object} item
 */
export const SaveRDAudit = async (item) => {
    let url = APIUrl  + 'RDProjectAudit/SaveRDAudit';
    let response = await api.Post(url, JSON.stringify(item), null, false)
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

/**
 * 取得審核紀錄清單
 * @param {string} PLAN_NO 計畫編號
 */
export const GetRDAuditList = async (PLAN_NO) => {
    let url = APIUrl  + 'RDProjectAudit/GetRDAuditList';
    let form = new FormData();
    form.append('MAIN_NO', PLAN_NO);
    let response = await api.Post(url, form, new Headers())
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

// 評核指標 GRID 資料驗證
export const validationSchema = Yup.object().shape({
    REVIEW_COMMENTS: Yup.string().nullable().required("審查意見欄位必填").max(500, '字數限制500字以內'),
    REVIEW_RESULT: Yup.string().nullable().required("審查結果欄位必填")
});
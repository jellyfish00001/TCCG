import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import { GetSetParam } from "../../../Basic/CommonService";
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

// 初始佐證資料
export const initFiles = {
    PROJECT_NO: "",
    FILE_KIND: "19", // SET_PARAM.SET_ITEM='FILE_KIND'
    EditFiles: []
};

// 初始表單資料
export const initData = {
    PROJECT_NO: "", // 計畫列管編號
    PROJECT_NAME: "", // 計畫名稱
    AW_KIND: "", // 申請項目
    Reasons: [], // 撤銷原因
    OTHER_REASON: null, // 其他撤銷原因
    ADJUST_REASON: null, // 計畫撤銷原因說明
    APPRV_DATE: new Date(), // 核准日期
    Files: initFiles, // 佐證資料
    REVIEW_COMMENTS: null, // 審查原因(退回補正用)
    REVIEW_RESULT: null // 審查結果(退回補正用)
}

/**
 * 取得主辦申請撤銷原因資料
 * @param {*} PROJ_ADJ_ID 調整流水號
 * @returns 
 */
const getExecReason = async (PROJ_ADJ_ID) => {
    let url = APIUrl + 'ProjectAdjust/GetExecReasonBasic';
    let form = new FormData();
    form.append('PROJ_ADJ_ID', PROJ_ADJ_ID);
    form.append('AW_KIND', "AW03");
    return await api.Post(url, form, new Headers(), false);
}

/**
 * 取得頁面所需資料
 * @param PROJ_ADJ_ID 調整撤銷流水號
 * @returns 若傳入 PROJ_ADJ_ID 非 null，回傳表單資料和撤銷原因，PROJ_ADJ_ID 為 null 只回傳撤銷原因
 */
export const getWindowData = async (PROJ_ADJ_ID) => {
    if (PROJ_ADJ_ID != null) {
        let responses = await Promise.all([
            getExecReason(PROJ_ADJ_ID), // 表單資料
            GetSetParam("REVOKE_REASON", "", true)// 取得撤銷原因
        ])
        //responses轉Json
        let responsesJson = responses.map(res => res.json());
        let result = await (Promise.all(responsesJson));
        return result;
    }
    else {
        return await GetSetParam("REVOKE_REASON");
    }
}

/**
 * 存檔 主辦申請調整撤銷原因
 * @returns 
 */
export const saveExecReason = async (data) => {
    let url = APIUrl + 'ProjectAdjust/SaveExecReason/';
    let response = await api.Post(url, JSON.stringify(data), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

// 欄位驗證
export const validateField = Yup.object().shape({
    PROJECT_NO: Yup.string().nullable().required("此欄位為必填"),
    OTHER_REASON: Yup.string().nullable().test(
        {
            message: "此欄位必填",
            test: function (value) {
                return this.parent.Reasons.find(v => v.SET_TYPE == "99") && IsNullOrEmpty(value) ? false : true;
            }
        }),
    ADJUST_REASON: Yup.string().nullable().required("此欄位為必填"),
    APPRV_DATE: Yup.string().required("此欄位為必填"),
});

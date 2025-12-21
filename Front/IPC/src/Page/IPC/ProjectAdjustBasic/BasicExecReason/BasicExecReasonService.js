import { api } from '../../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../../Route/RootMiddleware';
import { GetSetParam, GetSingleSetParam } from "../../../../Basic/CommonService";
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得主辦申請調整基本資料原因
 * @param {*} PROJ_ADJ_ID 列管編號 
 * @returns 
 */
const getExecReasonBasic = async (PROJ_ADJ_ID) => {
    let url = APIUrl + 'ProjectAdjust/GetExecReasonBasic';
    let form = new FormData();
    form.append('PROJ_ADJ_ID', PROJ_ADJ_ID);
    form.append('AW_KIND', "AW01");
    let response = await api.Post(url, form, new Headers(), false);
    return response;
}

/**
 * 取得頁面所需資料
 * @param {*} PROJ_ADJ_ID 調整檔流水號
 * @returns 
 */
export const getPageData = async (PROJ_ADJ_ID) => {
    let responses = await Promise.all([
        // 取得申請項目    
        GetSingleSetParam("AW_KIND", "AW01", true),
        // 取得基本資料調整原因checkbox清單
        GetSetParam("ADJUST_REASON", "", true),
        // 取得申請調整基本資料原因
        getExecReasonBasic(PROJ_ADJ_ID)
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    return result;
}

/**
 * 存檔 主辦申請調整撤銷原因
 * @param {*} data 
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
    PROJECT_NAME: Yup.string().required("此欄位為必填"),
    OTHER_REASON: Yup.string().nullable().test(
        {
            message: "此欄位必填",
            test: function (value) {
                return this.parent.Reasons.find(v => v.SET_TYPE == "99") && IsNullOrEmpty(value) ? false : true;
            }
        }),
    ADJUST_REASON: Yup.string().nullable().required("此欄位為必填"),
});

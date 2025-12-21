import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import * as Yup from 'yup';
import { GetSetParam } from "../../../Basic/CommonService";
import SetCodeService from "../CodeMaintain/SetCodeService";
import { IsNullOrEmpty } from "../../../Basic/SDOExtension";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得每月辦理情形下拉清單
 * @returns 
 */
export const getAllDropDowns = async () => {
    let responses = await Promise.all([
        // 取得落後類別           
        GetSetParam("DELAY_TYPE", "", true),
        // 取得責任歸屬           
        GetSetParam("DELAY_RESPON", "", true),
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    // 落後類別
    result[0].unshift({ SET_VALUE: "請選擇", SET_TYPE: "" });
    // 責任歸屬
    result[1].unshift({ SET_VALUE: "請選擇", SET_TYPE: "" });
    return result;
}

/**
 * 取得落後項目ddl data
 * @returns 
 */
export const getDelayClass = async (value) => {
    let data = await SetCodeService.loadCodeDelayClass(value, false);
    data.unshift({ DELAY_CLASS_SUB_ITEM: "請選擇", DELAY_CLASS_SUB_ID: "" });
    return data;
}

/**
 * 取得計畫落後原因分析(單筆)
 * @param {*} projectNo 
 * @param {*} seq (可傳可不傳)
 * @param {*} isRdecFun 
 * @returns 
 */
export const getProjectFillDelay = async (projectNo, seq, isRdecFun) => {
    let url = APIUrl + 'ProjectExecute/GetProjectFillDelay';
    let formData = new FormData();
    formData.append("PROJECT_NO", projectNo);
    formData.append("SEQ", seq);
    formData.append("isRdecFun", isRdecFun);
    let response = await api.Post(url, formData, new Headers(), false);

    let result = {};
    if (response.ok) {
        result = await response.json();
        if (!IsNullOrEmpty(result)) {
            result = {
                ...result,
                DEADLINES: IsNullOrEmpty(result.DEADLINES) ? null : new Date(result.DEADLINES)
            }
        }
    }
    return result;
}

/**
 * 取得計畫落後原因分析
 * @param {*} projectNo 
 * @param {*} dataType 資料種類 0:預設近5個月 1:全部
 * @returns 
 */
export const getProjectFillDelayList = async (projectNo, dataType) => {
    let url = APIUrl + 'ProjectExecute/GetProjectFillDelayList';
    let formData = new FormData();
    formData.append("PROJECT_NO", projectNo);
    formData.append("DATA_TYPE", dataType);
    let response = await api.Post(url, formData, new Headers(), false);

    let result = [];
    if (response.ok) {
        result = await response.json();

    }
    return result;
}

/**
 * 儲存計畫落後原因
 * @param {*} model 
 * @returns 
 */
export const saveProjectFillDelay = async (model) => {
    let url = APIUrl + 'ProjectExecute/SaveProjectFillDelay';
    let response = await api.Post(url, JSON.stringify(model), null, false);

    let result = [];
    if (response.ok) {
        result = await response.json();

    }
    return result;
}

/**
 * 刪除計畫落後原因
 * @param {*} model 
 * @returns 
 */
export const deleteProjectDelayCausal = async (seq) => {
    let url = APIUrl + 'ProjectExecute/DeleteProjectDelayCausal';
    let response = await api.Post(url, JSON.stringify(seq.toString()), null, false);

    let result = [];
    if (response.ok) {
        result = await response.json();

    }
    return result;
}

//#region 存檔驗證
Yup.setLocale({
    //設定必填欄位顯示的訊息
    mixed: {
        required: "此欄位必填",
    }
});

/**
 * 欄位驗證
 */
export const validateField = Yup.object().shape({
    DELAY_CLASS_C: Yup.string().required().nullable(),
    DELAY_SUBCLASS_C: Yup.string().required().nullable(),
    DELAY_RESPON: Yup.string().required().nullable(),
    DELAY_CAUSAL: Yup.string().required().nullable(),
    SOLUTION: Yup.string().required().nullable(),
    COORDINATION: Yup.string().required().nullable(),
    DEADLINES: Yup.date().required().nullable(),
});
//#endregion
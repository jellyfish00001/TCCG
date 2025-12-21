import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得其他資料
 * @param {*} projectNo 
 * @returns 
 */
const getProjectFillOther = async (projectNo) => {
    let url = APIUrl + 'ProjectExecute/GetProjectFillOther';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存其他資料
 * @param {*} models 
 * @returns 
 */
const saveProjectFillOther = async (models) => {
    let url = APIUrl + 'ProjectExecute/SaveProjectFillOther';
    let response = await api.Post(url, JSON.stringify(models), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

// 招標情形欄位驗證
const validateBidField = Yup.object().shape({
    AWARD_BID_DATE: Yup.date().nullable(),
    BID_TENDER: Yup.string().nullable()
        .test('checkRequired', '此為必填欄位', function (item) {
            return (!IsNullOrEmpty(this.parent.AWARD_BID_DATE) && IsNullOrEmpty(item)) ? false : true;
        })
});

// 招標情形grid欄位驗證
const validateBidGridField = Yup.object().shape({
    DETAIL_DATE: Yup.date().required("此欄位為必填").nullable(),
    DETAIL_REASON: Yup.string().required("此欄位為必填").nullable()
});

// 相關活動欄位驗證
const validateActivityField = Yup.object().shape({
    IS_ACTIVITY: Yup.bool().nullable(),
    ACTIVITY_DATE: Yup.date().nullable()
        .test('ACTIVITY_DATE', '此為必填欄位', function (item) {
            return (this.parent.IS_ACTIVITY && IsNullOrEmpty(item)) ? false : true;
        })
    ,
    ACTIVITY_NAME: Yup.string().nullable()
        .test('ACTIVITY_NAME', '此為必填欄位', function (item) {
            return (this.parent.ACTIVITY_KIND == "04" && this.parent.IS_ACTIVITY && IsNullOrEmpty(item)) ? false : true;
        })
});

// 相關活動grid欄位驗證
const validateActivityGridField = Yup.object().shape({
    ACTIVITY_DATE: Yup.date().required("此欄位為必填"),
    ACTIVITY_NAME: Yup.string().required("此欄位為必填"),
});

// 相關審查欄位驗證
const validateReviewField = Yup.object().shape({
    IS_REVIEW: Yup.bool().nullable(),
    SEND_DATE: Yup.date().nullable().test('SEND_DATE', '此為必填欄位', function (item) {
        return (this.parent.IS_REVIEW && IsNullOrEmpty(item)) ? false : true;
    }),
    REVIEW_DATE: Yup.date().nullable()
});

// 相關審查grid欄位驗證
const validateReviewGridField = Yup.object().shape({
    OTH_RVWNAME: Yup.string().required("此欄位為必填"),
    SEND_DATE: Yup.date().required("此欄位為必填").nullable(),
    REVIEW_DATE: Yup.date().nullable()
});

// 廠商資訊欄位驗證
const validateTenderField = Yup.object().shape({
    TENDER_KIND: Yup.string().required('此欄位為必填'),
    TENDER_NAME: Yup.string().required('此欄位為必填'),
});

const ProjectFillOtherService = {
    getProjectFillOther: getProjectFillOther,
    saveProjectFillOther: saveProjectFillOther,
    validateBidField: validateBidField,
    validateBidGridField: validateBidGridField,
    validateActivityField: validateActivityField,
    validateActivityGridField: validateActivityGridField,
    validateReviewField: validateReviewField,
    validateReviewGridField: validateReviewGridField,
    validateTenderField: validateTenderField,
}
export default ProjectFillOtherService;
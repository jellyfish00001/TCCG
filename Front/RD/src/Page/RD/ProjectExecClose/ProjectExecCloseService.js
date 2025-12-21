import { ServerConfig } from '../../../Basic/BasicData';
import { api } from '../../../Basic/ApiFetch';
import { IsNullOrEmpty, FormatDate } from "../../../Basic/SDOExtension";
import * as Yup from 'yup';

// URL
const APIUrl = ServerConfig.backEndUrl;

// 檔案初始資料
export const initFiles = {
    PROJECT_NO: "",
    FILE_KIND: "", // SET_PARAM.SET_ITEM='FILE_KIND'
    EditFiles: [],
};

/**
 * 透過計畫編號取得 結案成果資料/續列管一年內參採情形
 * @param {string} planNo 計畫編號
 * @param {string} actionName API Action Name
 */
export const GetRDResSituation = async (planNo, actionName) => {
    let url = APIUrl + 'ProjectSituation/' + actionName;
    let form = new FormData();
    form.append('PLAN_NO', planNo);
    let response = await api.Post(url, form, new Headers())
    if (response.ok) {
        let result = await response.json();
        // 如果沒有結案日期，給預設值日系統日，有結案日期要轉民國年
        if(IsNullOrEmpty(result["CLOSING_DATE"])){
            result["CLOSING_DATE"] = FormatDate(new Date(), 'tYY年MM月DD日');
        }else{
            result["CLOSING_DATE"] = FormatDate(new Date(result["CLOSING_DATE"]), 'tYY年MM月DD日');
        }
        // 如果沒有研究建議處理情形，給空值，下拉選單預選會第一個
        if(IsNullOrEmpty(result["SITUATION_TYPE"])){
            result["SITUATION_TYPE"] = "";
        }
        // 如果沒有續列管一年內採行情形，給空值，下拉選單預選會第一個
        if(IsNullOrEmpty(result["CONTINUE_SITUAITON_TYPE"])){
            result["CONTINUE_SITUAITON_TYPE"] = "";
        }
        return result;
    } else {
        return null;
    }
}

/**
 * 儲存參採情形/結案成果資料/續列管一年內參採情形
 * @param {string} data 表單送出資料
 * @param {string} actionName API Action Name
 */
export const SaveRDResSituation = async (data, actionName) => {
    let url = APIUrl + 'ProjectSituation/' + actionName;
    let response = await api.Post(url, JSON.stringify(data), null, false)
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

/**
 * 民國(TW)轉西元(AD)日期
 * EX：民國113年02月26日 => 2024/02/26
 * @param {string} date 民國日期
 * @return {date} 西元日期
 */
export const ConvertTwToAd = (date) => {
    // 排除一個或多個非數字的值，並轉成陣列 => [113, 02, 26]
    let newDate = date.split(/\D+/);
    // 年 + 1911
    newDate[0] = (parseInt(newDate[0]) + 1911).toString();
    // 以「-」符號串接起來年月日
    let joinNewDate = newDate[0] + "-" + newDate[1] + "-" + newDate[2]
    // 將陣列以「-」串接，建立 Date
    return new Date(joinNewDate);
}

// 驗證規則
export const validationSchema = Yup.object().shape({
    SITUATION_TYPE: Yup.string().nullable().required('研究建議處理情形必填'),
    SITUATION_DESC: Yup.string().nullable().required('採行情形簡述必填'),
    // 沒有結案日期才要驗證 CONTINUE_SITUAITON_DESC
    CONTINUE_SITUAITON_DESC: Yup.string().nullable()
        .when('CLOSING_DATE', {
            is: (CLOSING_DATE) => IsNullOrEmpty(CLOSING_DATE),
            then: Yup.string().required('續列管一年參採情形簡述必填')
        }),
    CONTINUE_SITUAITON_TYPE: Yup.string().nullable()
        .when('CLOSING_DATE', {
            is: (CLOSING_DATE) => IsNullOrEmpty(CLOSING_DATE),
            then: Yup.string().required('續列管一年參採情形必填')
        })
});
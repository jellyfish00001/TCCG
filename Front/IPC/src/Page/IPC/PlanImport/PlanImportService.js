import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { Download } from '../../../Basic/Download'
import { getOrganList } from "../../../Basic/CommonService";
let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 下載範本檔
 * @param {*} tempFileName 存於ftp範本檔名 
 * @param {*} downFileName 下載結果檔名
 * @returns 
 */
export const downTemplateFile = async (tempFileName, downFileName) => {
    let url = APIUrl + 'Download/DownTemplateFile';
    let form = new FormData();
    form.append('tempFileName', tempFileName);
    form.append('downFileName', downFileName);
    Download(url, 'POST', form, new Headers());
}

/**
 * 匯入計畫基本資料
 * @param {*} requestModel 匯入檔案 & 年度
 * @returns 
 */
export const importGeneralProject = async (planYear, files) => {
    let url = APIUrl + 'ImportProject/ImportGeneralProject';
    let form = new FormData();
    form.append('PlanYear', planYear);
    form.append('ImportFile', files[0].getRawFile());
    let response = await api.Post(url, form, new Headers(), false);

    if (response.ok) {
        return await response.json();
    }
    return null;
}

/**
 * 取得先期計畫列表
 * @param {*} requestModel 查詢條件
 * @returns 
 */
export const getPWSSDPlanList = async (requestModel) => {
    let result = [];
    let url = APIUrl + 'ImportProject/QueryPWSSDPlanList'
    let response = await api.Post(url, JSON.stringify(requestModel), null, false);
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 匯入先期計畫資料
 * @param {Array} planIds 計畫編號
 * @returns 
 */
export const importPWSSDPlans = async (planIds) => {
    let result = [];
    let url = APIUrl + 'ImportProject/ImportPWSSDPlans'
    let response = await api.Post(url, JSON.stringify(planIds), null, false);
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得先期計畫匯入下拉清單
 * @returns 
 */
export const loadAllDropDowns = async () => {
    let responses = await Promise.all([
        getOrganList(),                         // 取得機關
        getSendStatusList()                     // 取得審核狀態
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));
    if (result) {
        //加入預設值請選擇
        return addDefaultItem(result);
    } else {
        return [];
    }
}

/**
 * 加入下拉選單預設值
 * @param {array} datas 下拉選單資料
 * @returns array
 */
export const addDefaultItem = (datas, text = "請選擇") => {
    //加入預設值請選擇
    datas.forEach(dropDownData => {
        dropDownData.unshift({ value: "", text: text })
    });
    return datas;
}

/**
 * 取得審核狀態下拉清單
 * @returns 
 */
const getSendStatusList = async () => {
    let url = APIUrl + 'DropDown/GetSendStatusList'
    return await api.Get(url, null, null, false);
}





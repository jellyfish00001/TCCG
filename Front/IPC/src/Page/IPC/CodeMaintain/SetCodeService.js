import { api } from '../../../Basic/ApiFetch';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得自訂檢核點
 * @param {*} CheckPointClassId 
 * @returns 
 */
const loadCusItem = async (CheckPointClassId) => {
    let url = APIUrl + 'SetParam/GetCusChkItem';
    let formData = new FormData();
    formData.append('CHK_POINT_CLASS_ID', CheckPointClassId);
    formData.append('forSettings', true);
    let response = await api.Post(url, formData, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存自訂檢核點
 * @param {*} models 
 * @returns 
 */
const saveCusItem = async (models) => {
    let url = APIUrl + 'SetParam/SaveCusChkItem';
    let response = await api.Post(url, JSON.stringify(models), null, false);
    let result = null
    if (response) {
        result = response.json();
    }
    return result;
}

/**
 * 取得執行方式
 * @param {*} cpKind 
 * @returns 
 */
const loadCheckPoint = async (cpKind) => {
    let url = APIUrl + 'SetParam/GetCodeCheckpoint';
    let formData = new FormData();
    formData.append('CP_KIND', cpKind);
    formData.append('isShowDel', true);
    let response = await api.Post(url, formData, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存執行方式
 * @param {*} model 
 */
const saveCheckPoint = async (model) => {
    let url = APIUrl + 'SetParam/SaveCodeCheckpoint';
    let response = await api.Post(url, JSON.stringify(model), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

/**
 * 取得所有下拉選單資料
 * @returns 
 */
const loadAllDropDowns = async (arr) => {
    let responses = await Promise.all(arr);
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));
    return result;
}

// 儲存代碼設定檔
const saveParam = async (data) => {
    let result = null;
    let url = APIUrl + 'SetParam/SaveParamItem';
    let response = await api.Post(url, JSON.stringify(data), null, false);
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得落後原因類別
 * @param {*} delayClassId 
 * @param {*} delFlg 
 * @returns 
 */
const loadCodeDelayClass = async (delayClassId, delFlg) => {
    let url = APIUrl + 'SetParam/GetCodeDelayClass';
    let formData = new FormData();
    formData.append('DELAY_CLASS_ID', delayClassId);
    if (!IsNullOrEmpty(delFlg)) {
        formData.append('DEL_FLG', delFlg);
    }
    let response = await api.Post(url, formData, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存落後原因類別
 * @param {*} model 
 */
const saveCodeDelayClass = async (model) => {
    let url = APIUrl + 'SetParam/SaveCodeDelayClass';
    let response = await api.Post(url, JSON.stringify(model), null, false);
    let result = null;
    if (response.ok) {
        result = response.json();
    }
    return result;
}

/**
 * 取得預算來源
 * @param {*} levelMark 1:本府預算、2:中央部會
 * @returns 
 */
const loadCodePlanItem = async (levelMark) => {
    let url = APIUrl + 'SetParam/GetCodePlanItem';
    let form = new FormData();
    form.append('LEVEL_MARK', levelMark);
    let response = await api.Post(url, form, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存預算來源
 * @param {*} models 
 */
const saveCodePlanItem = async (models) => {
    let url = APIUrl + 'SetParam/SaveCodePlanItem';
    let response = await api.Post(url, JSON.stringify(models), null, false);
    let result = null;
    if (response.ok) {
        result = response.json();
    }
    return result;
}

/**
 * 判斷有無該年度工作日
 * @param {*} year 
 * @returns 
 */
const loadWorkingDayCountByYear = async (year) => {
    let url = APIUrl + 'SetParam/GetWorkingDayCountByYear';
    let response = await api.Post(url, JSON.stringify(year), null, false);
    let result = 0;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得工作日
 * @param {*} startDate 
 * @param {*} endDate 
 * @returns 
 */
const loadWorkingDay = async (startDate, endDate) => {
    let url = APIUrl + 'SetParam/GetWorkingDay';
    let formData = new FormData();
    formData.append('startDate', startDate);
    formData.append('endDate', endDate);
    let response = await api.Post(url, formData, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 產生年度工作日
 * @param {*} year 
 * @returns 
 */
const generateWorkingDay = async (year) => {
    let url = APIUrl + 'SetParam/GenerateWorkingDay';
    let response = await api.Post(url, JSON.stringify(year), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存該年度之工作日
 * @param {*} models 
 * @returns 
 */
const saveWorkingDay = async (models) => {
    let url = APIUrl + 'SetParam/SaveWorkingDay';
    let response = await api.Post(url, JSON.stringify(models), null, false);
    let result = null;
    if (response.ok) {
        result = response.json();
    }
    return result;
}

/**
 * 取得計畫落後原因的落後項目代碼清單
 * @returns 
 */
const loadDelayClasses = async () => {
    let url = APIUrl + 'ProjectCommon/GetDelayClasses';
    let response = await api.Post(url);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得計畫落後原因的落後項目清單
 * @returns 
 */
const loadDelaySubClasses = async () => {
    let url = APIUrl + 'ProjectCommon/GetDelaySubClasses';
    let response = await api.Post(url);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得計畫經費來源的預算編號清單
 * @param {*} levelMark 1:本府預算、2:中央部會
 * @returns 
 */
const loadPlanItems = async (levelMark) => {
    let url = APIUrl + 'ProjectCommon/GetPlanItems';
    let form = new FormData();
    form.append('LEVEL_MARK', levelMark);
    let response = await api.Post(url, form, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

export const SetCodeService = {
    loadCheckPoint: loadCheckPoint,
    saveCheckPoint: saveCheckPoint,
    loadCusItem: loadCusItem,
    saveCusItem: saveCusItem,
    loadAllDropDowns: loadAllDropDowns,
    saveParam: saveParam,
    loadCodeDelayClass: loadCodeDelayClass,
    saveCodeDelayClass: saveCodeDelayClass,
    loadCodePlanItem: loadCodePlanItem,
    saveCodePlanItem: saveCodePlanItem,
    loadWorkingDayCountByYear: loadWorkingDayCountByYear,
    loadWorkingDay: loadWorkingDay,
    generateWorkingDay: generateWorkingDay,
    saveWorkingDay: saveWorkingDay,
    loadDelayClasses: loadDelayClasses,
    loadDelaySubClasses: loadDelaySubClasses,
    loadPlanItems: loadPlanItems
}

export default SetCodeService;
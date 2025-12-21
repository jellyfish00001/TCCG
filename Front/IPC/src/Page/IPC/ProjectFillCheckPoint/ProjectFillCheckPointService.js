import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import * as Yup from 'yup';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得自訂檢核點
 * @param {*} CheckPointClassId 
 * @returns 
 */
export const loadCusItem = async (CheckPointClassId) => {
    let url = APIUrl + 'SetParam/GetCusChkItem';
    let formData = new FormData();
    formData.append('CHK_POINT_CLASS_ID', CheckPointClassId);
    formData.append('forSettings', false);
    let response = await api.Post(url, formData, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得執行方式
 * @param {*} cpKind 
 * @param {*} isShowDel 
 * @returns 
 */
export const loadCheckPoint = async (cpKind, isShowDel) => {
    let url = APIUrl + 'SetParam/GetCodeCheckpoint';
    let formData = new FormData();
    formData.append('CP_KIND', cpKind);
    formData.append('isShowDel', isShowDel);
    let response = await api.Post(url, formData, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
        if (result.length > 0) {
            result.unshift({ CHECKPOINT_CLASS_ID: "", CHECKPOINT_CLASS: '請選擇' })
        }
    }
    return result;
}

/**
 * 取得檢核點設定資料
 * @param {*} id 列管編號 PROJECT_NO 或 PROJ_ADJ_ID 
 * @param {*} type 取哪裡的資料，無: 主檔，"adjust": 調整檔
 * @returns 
 */
export const loadFormData = async (id, type = null) => {
    let url = APIUrl + 'Project/GetProjectCheckpoint';
    if (type == "adjust") {
        url = APIUrl + 'ProjectAdjust/GetAdjustCheckPoint';
    }

    let response = await api.Post(url, JSON.stringify(id), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 存取檢核點設定資料
 * @param {*} requestData
 * @param {*} type 存到哪裡的資料，無: 主檔，"adjust": 調整檔
 * @returns 
 */
export const saveData = async (requestData, type = null) => {
    let url = APIUrl + 'Project/SaveProjectCheckpoint';
    if (type == "adjust") {
        url = APIUrl + 'ProjectAdjust/SetAdjustCheckPoint';
    }

    let response = await api.Post(url, JSON.stringify(requestData), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
* 驗證
*/
export const validataHelper = Yup.object().shape({
    CHECKITEM_NAME: Yup.string().required('此欄位為必填'),
    PROGRESS: Yup.number()
        .max(100, '不得超過100')
        .required('此欄位為必填'),
    ESTIMATED_ENDDATE: Yup.string().required()
});

/**
 * 存檔驗證
 * @param {*} formData 表格資料 
 * @param {*} sortedData Grid資料
 * @param {*} checkpoint 執行方式
 * @returns 錯誤訊息
 */
export const checkDataValid = async (formData, sortedData, checkpoint) => {
    // 檢查是否有選擇執行方式
    if (IsNullOrEmpty(checkpoint)) {
        return "請選擇執行方式";
    }

    // 若為工程類，檢查自訂檢核點是否均在"開工檢核點之前"
    if (formData.CP_KIND === '0') {
        // 開工檢核點進度
        let startWorkProgress = sortedData.find(x => x.CTRL_POINT === 'A').PROGRESS;
        if (sortedData.find(x => (x.IS_NEW || x.CHECKITEM_SEQ === 0) && x.editType !== 3 && x.PROGRESS >= startWorkProgress)) {
            return "自訂檢核點需要在開工之前";
        }
    }

    return "";
}


/**
 * 物件轉換，轉成可存檔Model對應欄位
 * @param {*} projectNo 列管編號
 * @param {*} dataList 待轉換資料
 * @param {*} checkItemColName 檢核點名稱DB欄位
 * @param {*} projStartDate 計畫開始日期 (用於計算第一項檢核點所需時間)
 * @returns 
 */
export const changeCheckItemDateModel = (projectNo, dataList, checkItemColName, projStartDate = null) => {
    // 一天的毫秒數
    const oneDay = 24 * 60 * 60 * 1000; // hours*minutes*seconds*milliseconds

    return dataList.map((d, index) => {
        let estimateEndDate = d.ESTIMATED_ENDDATE === undefined || d.ESTIMATED_ENDDATE === null ? '' : new Date(d.ESTIMATED_ENDDATE);
        let diffDays = 0;
        let diffMonth = 0;
        // 計算所需時間(月)
        if (projStartDate != null) {
            // 計算第一項檢核點所需時間是以計畫開始日期為基準
            let previousDate = projStartDate;
            if (index !== 0) {
                previousDate = dataList[index - 1].ESTIMATED_ENDDATE === undefined
                    ? new Date()
                    : dataList[index - 1].ESTIMATED_ENDDATE;
            }
            let currDate = estimateEndDate;
            if (!IsNullOrEmpty(estimateEndDate)) {
                // @ts-ignore
                diffDays = Math.round(Math.abs((new Date(previousDate) - new Date(currDate)) / oneDay));
                diffMonth = Math.round((diffDays / 30) * 10) / 10
            }
        }

        return {
            SEQ: checkItemColName === 'NAME' ? 0 : d.SEQ,
            PROJECT_NO: projectNo,
            // 若來源為設定檔，則該值為設定檔欄位
            CHECKITEM_SEQ: checkItemColName === 'NAME' ? d.SEQ.toString() : d.CHECKITEM_SEQ,
            CHECKITEM_NAME: d[checkItemColName],
            PROGRESS: d.PROGRESS,
            ESTIMATED_ENDDATE: estimateEndDate,
            ACTUAL_ENDDATE: d.ACTUAL_ENDDATE,
            ORI_ESTIMATED_ENDDATE: d.ORI_ESTIMATED_ENDDATE,
            IS_DELAY: 0,
            diffMonth: diffMonth,
            CTRL_POINT: d.CTRL_POINT,
            IS_NEW: false,
            editType: checkItemColName === 'NAME' ? 1 : 0
        }
    })
}




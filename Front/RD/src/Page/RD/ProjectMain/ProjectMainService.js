import { ServerConfig } from '../../../Basic/BasicData';
import { api } from '../../../Basic/ApiFetch';
import { FormatDate, openChapterPage, IsNullOrEmpty } from "../../../Basic/SDOExtension";
import * as Yup from 'yup';

// URL
const APIUrl = ServerConfig.backEndUrl;

/**
 * 查詢條件初始值
 * @returns {object} 查詢條件初始值物件
 */
export const getQueryData = () => {
    // 取得系統民國年
    const systemDate = FormatDate(new Date(), 'tYY/MM/DD');
    const year = systemDate.split('/')[0];
    // 查詢條件初始參數值
    let data = {
        PLAN_YEAR: year,
        PLAN_NO: "",
        PLAN_NAME: "",
        ENTRUST_UNIT_NAME: "",
        OU_ID: ""
    }
    return data;
}

/**
 * 取得委託研究計畫清單
 * @param {object} item
 */
export const GetRDProjectManage = async (item) => {
    let url = APIUrl  + 'RDProjectManage/GetRDProjectManage';
    let response = await api.Post(url, JSON.stringify(item), null, false)
    if (response.ok) {
        let data = await response.json();
        // 重組清單資料：組計畫期程資料並轉民國年
        const newData = data.map((obj, index) => ({
            ...obj,
            PLAN_DATE: ProcessPlanDate(obj.PLAN_START_DATE, obj.PLAN_END_DATE),
        }));
        return newData;
    } else {
        return null;
    }
}

/**
 * 處理計畫起訖資料
 * @param {*} planStartDate 計畫期程 - 起
 * @param {*} planEndDate 計畫期程 - 迄
 */
const ProcessPlanDate = (planStartDate, planEndDate) => {
    // 若其中一個是空，則傳空字串
    if(IsNullOrEmpty(planStartDate) || IsNullOrEmpty(planEndDate)){
        return "";
    }else{
        // 組計畫期程資料並轉民國年
        return FormatDate(planStartDate, "tYY年MM月") + "~" + FormatDate(planEndDate, "tYY年MM月");
    }
}

/**
 * 產生組織樹資料
 * @param {array} data 計畫清單資料
 * @returns 
 */
export const genOrgTreeData = (data) => {
    // 取得計畫列表機關清單
    let projOrgPanelDataList = data
        .map(proj => {
            let projCnt = data.filter(x => x.OU_ID == proj.OU_ID).length;
            return {
                OU_ID: proj.OU_ID,
                title: proj.EXEC_ORG_NAME + '(' + projCnt + ')',
                order: proj.EXEC_ORG_ORDER
            }
        });
    // Distinct projOrgPanelDataList
    let distinctData = projOrgPanelDataList.filter((value, index, self) =>
        index === self.findIndex((t) => (
            t.OU_ID === value.OU_ID && t.title === value.title //OU_NAME
        ))
    );
    distinctData.sort((a, b) => a.order - b.order);
    distinctData.unshift({ OU_ID: "", title: `桃園市政府(${data.length})` });
    return distinctData;
}

/**
 * Window 儲存(新增)計畫
 * @param {string} planName 計畫名稱 
 * @returns {object} 結果物件{ message: 結果訊息, data: 計畫編號}
 */
export const SaveRDResearchBasic = async (planName) => {
    // 計畫年度是存 string，這裡要把數字轉字串
    let planYear = (new Date().getFullYear() - 1911).toString();
    let url = APIUrl + 'ProjectBasic/SaveRDResearchBasic';
    let data = {'PLAN_NAME': planName, 'PLAN_YEAR': planYear};
    let response = await api.Post(url, JSON.stringify(data), null, false)
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

/**
 * 刪除、撤銷計畫計畫
 * @param {array} planNos 多筆計畫編號
 * @param {string} execKind 類型（D 刪除、R 撤銷、L1 送出鎖定、L2 解除鎖定） 
 * @returns {object} 結果物件{ message: ..., success: ...}
 */
export const SaveRDBasicStatus = async (planNos, execKind) => {
    let url = APIUrl + 'ProjectBasic/SaveRDBasicStatus';
    let response = await api.Post(url, JSON.stringify({PLAN_NOS:planNos, EXEC_KIND:execKind})) //JSON.stringify({planNos:projectNos, execKind:kind}) //formData, new Headers(), false
    if (response.ok) {
        return await response.json();
    } else {
        return null;
    }
}

/**
 * 開啟章節表
 * @param {object} item 存入 Storage Data 資料物件
 * @returns 
 */
export const openProjectChapter = async (item) => {
    let title = "研究發展作業系統-委託研究計劃-計畫章節-基本資料"
    // 開啟章節表(同一個計畫只會有一個分頁，用 PLAN_NO 當 NAME)
    const url = process.env.PUBLIC_URL + '/ProjectChapter';
    openChapterPage(url, item.PLAN_NO, `${title}(${item.PLAN_NO})`, item);

}

// 新增計畫驗證規則
export const validationSchema = Yup.object().shape({
    PLAN_NAME: Yup.string().nullable().required('計畫名稱必填').max(100)
});


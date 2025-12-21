import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { GetSetParam} from "../../../Basic/CommonService";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得計畫基本資料
 * @param {*} planNo //計畫編號
 * @returns 
 */
export const GetPlanData = async (planNo) => {
    let url = APIUrl + 'ReviewProject/GetReviewProject';
    let response = await api.Post(url, JSON.stringify(planNo), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 儲存計畫基本資料
 * @param {*} data  //計畫基本資料
 * @returns 
 */
export const SavePlanReview = async (data) => {
    let url = APIUrl + 'ReviewProject/SetReviewProject';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 取得重大關聯資料
 * @returns 
 */
export const GetPlanDataList = async () => {
    let url = APIUrl + 'ReviewProject/GetOtherProject';
    let response = await api.Post(url, JSON.stringify(), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 退回計畫
 * @param {*} planNo  //計畫基本資料
 * @returns 
 */
export const SendBackProject = async (planNo) => {
    let url = APIUrl + 'ReviewProject/SendBackProject';
    let response = await api.Post(url, JSON.stringify(planNo), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 取畫面資料及下拉選單處裡
 * @param {*} projectNo 
 * @returns 
 */
export const getLoadData = async (projectNo) => {
    // 取得計畫基本資料
    let data = await GetPlanData(projectNo)
    let fromData = {
        ...data,
        gridData:[
            {
                Number: data.PLANORDERNUMBER,
                Check: "建議核列",
                PublicBudget: data.PUBLIC1,
                FundBudget: data.FUND1,
            },
            {
                Number: data.PLANORDERNUMBER,
                Check: "不建議核列",
                PublicBudget: data.PUBLIC2,
                FundBudget: data.FUND2,
            },
        ],
        publicMoney:data.PLANTOTMONEY-data.FUNDMONEY,
        FundMoney:data.FUNDMONEY,
    }
    // 關聯性
    let relevance = await GetSetParam('PLAN_REF', '');
    relevance.unshift({ SET_VALUE: '請選擇', SET_TYPE: null });
    // 執行績效
    let exePerformance = await GetSetParam('EXE_PERFORMANCE', '');
    exePerformance.unshift({ SET_VALUE: '請選擇', SET_TYPE: null });
    // 評核類別
    let bougetReview = await GetSetParam('SUB_PLANDATETYPE', '');
    // 依評核類別過濾顯示下拉選項
    switch (fromData.PLANDATETYPE) {
        case '3':
            bougetReview = bougetReview.filter(item => parseInt(item.SET_TYPE) >= 11 && parseInt(item.SET_TYPE) <= 12);
            break;
        case '2':
            bougetReview = bougetReview.filter(item => parseInt(item.SET_TYPE) >= 4 && parseInt(item.SET_TYPE) <= 10);
            break;
        case '1':
            bougetReview = bougetReview.filter(item => parseInt(item.SET_TYPE) >= 1 && parseInt(item.SET_TYPE) <= 3);
            break;
    }    
    bougetReview.unshift({ SET_VALUE: '請選擇', SET_TYPE: null });
    return {
        fromData: fromData,
        relevance: relevance,
        exePerformance: exePerformance,
        bougetReview: bougetReview,
        AuditTemplateModels: data.AuditTemplateModels,
    };
};
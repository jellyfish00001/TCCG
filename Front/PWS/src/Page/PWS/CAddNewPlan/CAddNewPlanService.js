import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { getPlanYearList } from '../../../Basic/CommonService';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import { getMonthList, getFundList, getOrganList} from "../../../Basic/CommonService";
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

// 儲存計畫基本資料
export const SavePlanBasicB = async (data) => {
    let url = APIUrl + 'PlanBasicB/SavePWSSDPlanMain';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 取得計畫基本資料
 * @param {*} PLANNO 
 * @returns 
 */
export const GetPlanBasicB = async (PLANNO) => {
    let url = APIUrl + 'PlanBasicB/GetPWSSDPlanMain';
    let response = await api.Post(url, JSON.stringify(PLANNO), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
        result.PlanCrossAMTAB = AddNoColumn(result.PlanCrossAMTB);
    }
    return result;
};

// 檔案上傳預設值
export const initFiles ={
    PROJECT_NO: "",
    FILE_KIND: "",
    EditFiles: [],
}

/**
 * 日期欄位檢查
 * @param {*} value
 * @returns 
 */
export const validationSchema = Yup.object().shape({
    startYear: Yup.string(),
    startMonth: Yup.string(),
    endYear: Yup.string(),
    endMonth: Yup.string().test(
        'dateRange',
        '結束日期必須大於或等於開始日期',
        function(value) {
            const { startYear, startMonth, endYear } = this.parent;
            // 確保所有相關欄位都有值
            if (startYear && startMonth && endYear && value) {
                const startDate = new Date(`${startYear}-${startMonth}-01`);
                const endDate = new Date(`${endYear}-${value}-01`);
                return endDate >= startDate;
            }
            // 如果某些欄位沒有值，則不進行該驗證
            return true;
        }
    ),
});

/**
 * 處理年度下拉選單(西元轉民國年分)
 * @param {*} dateString 
 * @returns 
 */
const getYearString = (dateString) => {
    if (!dateString) {
        const currentYear = new Date().getFullYear() - 1911;
        return currentYear.toString();
    }
    const date = new Date(dateString);
    const year = date.getFullYear() - 1911;
    return year.toString();
};

/**
 * 處理月份下拉選單(取月分)
 * @param {*} dateString 
 * @returns 
 */
const getMonthString = (dateString) => {
    if (!dateString) {
        return 1;
    }
    const date = new Date(dateString);
    const month = date.getMonth() + 1;
    return month;
};

/**
 * 取畫面資料及下拉選單處裡
 * @param {*} projectNo 
 * @returns 
 */
export const getData = async (projectNo) => {
    // 取得年度下拉選單
    let Year = await getPlanYearList(false, 20, "A");
    // 取得月份下拉選單
    let Month = await getMonthList();
    // 取得基金下拉選單
    let FundList = await getFundList();
    // 取得機關下拉選單
    let OrganList = await getOrganList();
    // 取得表單資料
    let result = await GetPlanBasicB(projectNo);
    let FromData = {
        ...result,
        startYear: getYearString(result.PLANSTARTDATE),
        startMonth: getMonthString(result.PLANSTARTDATE),
        endYear: getYearString(result.PLANENDDATE),
        endMonth: getMonthString(result.PLANENDDATE),
        bidYear: getYearString(result.AWARDYM),
        bidMonth: getMonthString(result.AWARDYM),
        cenYear: getYearString(result.MIDREPORTYM),
        cenMonth: getMonthString(result.MIDREPORTYM),
        lastYear: getYearString(result.FINAKREPORTYM),
        lastMonth: getMonthString(result.FINAKREPORTYM),
        finishYear: getYearString(result.CLOSEYM),
        finishMonth: getMonthString(result.CLOSEYM),
        thisYearMoney: result.PUBLICMONEY+result.FUNDMONEY+result.CENTERMONEY+result.OTHERMONEY,
    }
    return {
        Year,
        Month,
        FundList,
        OrganList,
        FromData
    };
};

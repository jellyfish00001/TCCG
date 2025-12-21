import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { GetSetParam, getPlanYearList, getOrganList, getOrgByUsr } from "../../../Basic/CommonService";
import { FormatDate } from '../../../Basic/SDOExtension';
import { Download } from "../../../Basic/Download";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 下拉選單初始值
 */
const initDdlData = {
    PLAN_YEAR: []
}

/**
 * 報表清單
 * @returns getPlanYearList
 */
const getStatisticsList = () => {
    const result = [
        {RPT_ID:"RPTPlanList", ID: 1, NAME: "各年度提案資料清冊", MEMO: "" },
        {RPT_ID:"RPTDeptStatistics", ID: 2, NAME: "各主題機關提案數統計表", MEMO: "" },
        {RPT_ID:"RPTYearStatistics", ID: 3, NAME: "各年度提案數統計表", MEMO: "" },
        
    ]
    return result;
}

/**
 * 下拉選單
 * @returns 
 */
const getAllDropDowns = async () => {
    let responses  = await getPlanYearList(false, 10, "B")
    responses.unshift({ text: "請選擇", value: "" });

    return responses;
}

/**
 * 組下拉選單資料
 * @returns 
 */
const getPageDropData = async () => {
    const dropDowns = await getAllDropDowns();
    if (dropDowns.length <= 0) {
        return initDdlData;
    }
    return {
        // 年度
        PLAN_YEAR: [...dropDowns],

    };
}



/**
 * 查詢條件初始值
 */
const initData = {
    // 計畫年度
    INN_YEAR: FormatDate(new Date(), "tYY"),
}

/**
 * 下載報表
 * @param {string} data 資料
 */
const exportRPT = async (data) => {
    let url = APIUrl + 'InnRPT/RPTInnStatistics';
    Download(url, 'POST', data);
}



const StatisticsService = {
    initDdlData,
    getStatisticsList: getStatisticsList,
    getAllDropDowns: getAllDropDowns,
    getPageData: getPageDropData,
    initData: initData,
    exportRPT
}

export default StatisticsService;
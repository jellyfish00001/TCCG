import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { getCurrentCycleData } from '../../../Basic/CommonService';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension'
import { AddNoColumn } from "../../../Basic/SDOExtension";
import { Download } from "../../../Basic/Download";
const baseUrl = getGlobalServerConfig().backEndUrl.get();

// 已完成填報週期 global variable 
let finishedCycletYY = "";
let finishedCycleMonth = "";

/**
 * 取得已填報完成週期
 */
export const getFillCompleteCycle = async () => {
    if (finishedCycletYY && finishedCycleMonth) {
        return { DAB_YEAR_YYY: finishedCycletYY, DAB_MONTH: finishedCycleMonth };
    }
    // 取得當前填報週期
    const currentCycle = await getCurrentCycleData();
    if (currentCycle != null) {
        // 比較時間 (填報結束日隔天早上6點)
        let comparedDate = addDays(new Date(currentCycle.FILL_END_DATE), 1);
        comparedDate.setHours(6);

        // 若已超過填報週期，則取得當前填報週期
        if (new Date() >= comparedDate) {
            finishedCycletYY = currentCycle.PROJECT_YEAR;
            finishedCycleMonth = currentCycle.PROJECT_MONTH;
        } else { // 若仍在填報週期，則取得上個填報週期
            const isCrossYear = currentCycle.PROJECT_MONTH_INT === 1;// 上個週期是否跨年
            if (isCrossYear) {
                finishedCycletYY = (currentCycle.PROJECT_YEAR_INT - 1).toString();
                finishedCycleMonth = "12";
            } else {
                finishedCycletYY = currentCycle.PROJECT_YEAR;
                finishedCycleMonth = (currentCycle.PROJECT_MONTH_INT - 1).toString().padStart(2, "0");
            }
        }
    }
    return { DAB_YEAR_YYY: finishedCycletYY, DAB_MONTH: finishedCycleMonth }
}

/**
 * 添加日數
 * @param {*} date 
 * @param {*} days 
 * @returns 
 */
const addDays = (date, days) => {
    const newDate = new Date(date);
    newDate.setDate(date.getDate() + days);
    return newDate;
}


/**
 * 產生儀錶板資料
 * @param {*} year
 * @param {*} month
 * @returns 
 */
export const generateMonthlyDAB = async (year, month) => {
    let url = baseUrl + 'DashBoard/GenerateDashBoardData';
    let requestForm = new FormData();
    requestForm.append('year', year);
    requestForm.append('month', month);
    let response = await api.Post(url, requestForm, new Headers(), false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 重要儀表板 - 取得頁面所需統計資料
 * @param {*} requestObj 
 * @returns 
 */
export const getDashBoardSummary = async (requestObj) => {
    let url = baseUrl + 'DashBoard/GetDashBoardSummaryData';
    let response = await api.Post(url, JSON.stringify(requestObj), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 重要儀表板 - 取得機關統計資料
 * @param {*} requestObj 
 * @returns 
 */
export const getOrgProjectSummary = async (requestObj) => {
    let url = baseUrl + 'DashBoard/GetOrgProjectSummary';
    let response = await api.Post(url, JSON.stringify(requestObj), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 件數及經費情形
 * @param {*} requestObj 
 * @returns 
 */
export const getDashBoardCountAndBudget = async (requestObj) => {
    let url = baseUrl + 'DashBoard/GetDashBoardCountAndBudget';
    let response = await api.Post(url, JSON.stringify(requestObj), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得歷年列管情形資料
 * @param {*} orgId 
 * @returns 
 */
export const getPastYearsData = async (orgId) => {
    let url = baseUrl + 'DashBoard/GetPastYearsData';
    let requestBody = { EXEC_ORGAN_C: orgId };
    let response = await api.Post(url, JSON.stringify(orgId), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得重大工程進度資料
 * @param {*} requestObj 
 */
export const getEngProgress = async (requestObj) => {
    let url = baseUrl + 'DashBoard/GetDashBoardEngProgress';
    let response = await api.Post(url, JSON.stringify(requestObj), null, false);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取落後案件清單
 */
export const getDelayList = async (requestObj) => {
    let url = baseUrl + 'DashBoard/GetMajorProjectDelayStatus';
    let response = await api.Post(url, JSON.stringify(requestObj), null, false);
    let result = null;
    if (response.ok) {
        result = AddNoColumn(await response.json());
    }
    return result;
}

/**
 * 取可能影響補助款
 */
export const GetProjectDelayList = async (requestObj) => {
    let url = baseUrl + 'DashBoard/GetProjectDelayList';
    let response = await api.Post(url, JSON.stringify(requestObj), null, false);
    let result = null;
    if (response.ok) {
        result = AddNoColumn(await response.json());
    }
    return result;
}

// 點擊取得報表
export const getStatisticsRPT = (DAB_YEAR_YYY, DAB_MONTH, RPT_ID, Type = "", isShow = true) => {
    // 去除DAB_MONTH參數MM月分中的0
    let MONTH = DAB_MONTH.replace(/^0+/, '');
    let RPTName = "";
    switch (RPT_ID) {
        case 1:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月重大建設計畫列管情形`;
            break;
        case 2:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月重大建設計畫落後情形`;
            break;
        case 3:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月各區重大建設計畫落後情形`;
            break;
        case 4:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月重大建設計畫執行機關落後綜合排名`;
            break;
        case 5:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月重大建設計畫落後案件清單`;
            break;
        case 6:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月執行中重大建設計畫列管情形`;
            break;
        case 7:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月重大建設計畫落後情形`;
            break;
        case 8:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月各區重大建設計畫落後情形`;
            break;
        case 9:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月重大建設計畫執行機關落後綜合排名`;
            break;
        case 10:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月重大建設計畫落後案件清單`;
            break;
        case 11:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月各區重大工程進度(簡版)`;
            break;
        case 12:
            RPTName = `桃園市政府${DAB_YEAR_YYY}年${MONTH}月各區重大工程進度(詳版)`;
            break;
    }
    // 報表參數
    var model = {
        // 填報年
        DAB_YEAR_YYY: DAB_YEAR_YYY,
        // 填報月
        DAB_MONTH: DAB_MONTH,
        // 報表代號
        STATISTICS_ID: RPT_ID,
        // 報表名稱
        STATISTICS_NAME: RPTName,
        // 報表類型
        Type: Type,
        // 刪除已結案及撤銷列管案件
        isDelectOrCancel: isShow
    };
    exportDashboardRPT(model);
};

/**
 * 匯出報表
 * @param {*} data 
 */
export const exportDashboardRPT = async (data) => {
    let url = baseUrl + 'RPT/RPTDashboard';
    await Download(url, 'POST', data);
}

/**
 * 取得地圖資料
 * @param {*} data 
 */
export const getMapData = (data) => {
    let areaPaths = [
        { townId: "H02", name: "桃園區", top: "104px", right: "130px" },
        { townId: "H03", name: "中壢區", top: "138px", right: "189px" },
        { townId: "H09", name: "八德區", top: "156px", right: "135px" },
        { townId: "H06", name: "蘆竹區", top: "52px", right: "131px" },
        { townId: "H05", name: "楊梅區", top: "187px", right: "261px" },
        { townId: "H07", name: "大園區", top: "63px", right: "206px" },
        { townId: "H10", name: "龍潭區", top: "247px", right: "200px" },
        { townId: "H08", name: "龜山區", top: "95px", right: "82px" },
        { townId: "H11", name: "平鎮區", top: "191px", right: "194px" },
        { townId: "H04", name: "大溪區", top: "227px", right: "129px" },
        { townId: "H12", name: "新屋區", top: "137px", right: "315px" },
        { townId: "H13", name: "觀音區", top: "96px", right: "273px" },
        { townId: "H14", name: "復興區", top: "344px", right: "68px" }
    ];

    let result = [];

    areaPaths.forEach(x => {
        const item = data.find(y => y.SET_TYPE == x.townId);
        result.push({
            name: x.name,
            top: x.top,
            right: x.right,
            value: item?.value ?? 0
        })
    })
    return result;
}

// 重大工程進度地區錨點
export const getProgressMapTownPosition = [
    { townId: "H02", name: "桃園區", top: "152px", right: "133px" },
    { townId: "H03", name: "中壢區", top: "186px", right: "182px" },
    { townId: "H09", name: "八德區", top: "203px", right: "129px" },
    { townId: "H06", name: "蘆竹區", top: "100px", right: "123px" },
    { townId: "H05", name: "楊梅區", top: "234px", right: "256px" },
    { townId: "H07", name: "大園區", top: "110px", right: "202px" },
    { townId: "H10", name: "龍潭區", top: "294px", right: "194px" },
    { townId: "H08", name: "龜山區", top: "142px", right: "76px" },
    { townId: "H11", name: "平鎮區", top: "238px", right: "187px" },
    { townId: "H04", name: "大溪區", top: "274px", right: "124px" },
    { townId: "H12", name: "新屋區", top: "184px", right: "308px" },
    { townId: "H13", name: "觀音區", top: "143px", right: "272px" },
    { townId: "H14", name: "復興區", top: "392px", right: "62px" }
];

// 建設類別配色
export const buildKindColorScheme = [
    'rgb(113,191,71)',
    'rgb(40,171,227)',
    'rgb(251,102,72)',
    'rgb(251,136,73)',
    'rgb(93,171,173)',
    'rgb(72,191,137)',
    'rgb(108,92,153)',
    'rgb(128,161,194)',
    'rgb(136,101,144)',
    'rgb(67,127,63)'
]

/**
 * 轉為億元單位並取至小數點後兩位
 * @param {*} budget 
 * @returns 
 */
export const formatBudget = (budget) => {
    return (budget / 100000000).toFixed(2)
}

// 選取樣式/未選取樣式
export const selectedColor = 'rgb(39, 133, 158)';
export const unselectedColor = 'rgb(98, 182, 183)';



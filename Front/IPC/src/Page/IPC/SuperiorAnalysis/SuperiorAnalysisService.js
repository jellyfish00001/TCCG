import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import { getOrganList, getOrgByUsr } from '../../../Basic/CommonService';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得機關下拉清單
 * @returns 
 */
const getOrganData = async (checkIsHANDRole) => {
    let response = checkIsHANDRole ? await getOrgByUsr() : await getOrganList();
    let result = await response.json();
    result.unshift({ text: "請選擇", value: "" });
    return result;
}

/**
 * 取得計畫件數
 * @param {*} formData 查詢條件
 * @returns 
 */
const getAnalysis = async (formData) => {
    let url = APIUrl + 'Superior/GetAnalysis';
    let response = await api.Post(url, JSON.stringify(formData));
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得計畫件數
 * @param {*} data 查詢結果
 * @returns 
 */
const getStatCnt = (data) => {
    let categories = [];
    let seriousBehinds = [];
    let behinds = [];
    let conforms = [];

    data.forEach(x => {
        categories.push(`${x.Text}_${x.Key}`);
        seriousBehinds.push(x.Values[0]);
        behinds.push(x.Values[1]);
        conforms.push(x.Values[2]);
    });

    let result = {
        Categories: categories,
        Data: [
            { name: "落後>=5%", data: seriousBehinds, color: "red" },
            { name: "落後<5%", data: behinds, color: "yellow" },
            { name: "進度符合", data: conforms, color: "#82D900" }
        ]
    };
    return result;
}

/**
 * 取得計畫金額
 * @param {*} data 查詢結果
 * @returns 
 */
const getStatBudgetAmt = (data) => {
    let categories = [];
    let conforms = [];
    let behinds = [];
    let seriousBehinds = [];

    data.forEach(x => {
        categories.push(`${x.Text}_${x.Key}`);
        conforms.push(x.OtherData === "A" ? x.Values[0] : 0);
        behinds.push(x.OtherData === "B" ? x.Values[0] : 0);
        seriousBehinds.push(x.OtherData === "C" ? x.Values[0] : 0);
    });

    let result = {
        Categories: categories,
        Data: [
            { name: "綠 (無落後案件)", data: conforms, color: "#82D900" },
            { name: "黃 (落後案件比例<50%)", data: behinds, color: "yellow" },
            { name: "紅 (落後案件比例>=50%)", data: seriousBehinds, color: "red" }
        ]
    };
    return result;
}

/**
 * 取得件數圓餅圖
 * @param {*} formData 查詢條件
 * @returns 
 */
const getPieChartStat = async (formData) => {
    let url = APIUrl + 'Superior/GetPieChartStat';
    let response = await api.Post(url, JSON.stringify(formData));
    let data = {};
    if (response.ok) {
        data = await response.json();
    }

    let result = [];

    // 符合
    if (data.Values[0] > 0) {
        result.push({ category: "符合", value: data.Values[0], color: "#82D900" });
    }

    // 落後
    if (data.Values[1] > 0) {
        result.push({ category: "落後", value: data.Values[1], color: "red" });
    }

    return result;
}

/**
 * 取得圖表統計明細
 * @param {*} formData 查詢條件
 * @returns 
 */
const getAnalysisDetail = async (formData) => {
    let url = APIUrl + 'Superior/GetAnalysisDetail';
    let response = await api.Post(url, JSON.stringify(formData));
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得明細圖表統計
 * @param {*} data 查詢結果
 * @param {*} isGetMatch 是否取得符合
 * @param {*} chartType 圖表類別
 * @returns 
 */
const getDetailChartStat = (data, isGetMatch, chartType) => {
    let categories = [];
    let values = [];

    data.forEach(x => {
        categories.push(`${x.Key}_${x.Text}_${chartType}`);
        values.push(x.Values[0]);
    });

    let result = {
        Categories: categories,
        Data: [
            { name: isGetMatch ? "符合" : "落後", data: values, color: isGetMatch ? "#82D900" : "red" }
        ]
    };

    return result;
}

/**
 * 取得落後圓餅圖
 * @param {*} data 查詢結果
 * @returns 
 */
const getDelayPieData = (data) => {
    let result = [];

    if (data.Values === null) {
        return result;
    }

    // 開工前進度落後
    if (data.Values[0] > 0) {
        result.push({ category: "D1_開工前進度落後(D1)", value: data.Values[0], color: "red" });
    }

    // 施工進度落後
    if (data.Values[1] > 0) {
        result.push({ category: "D2_施工進度落後(D2)", value: data.Values[1], color: "#FF9797" });
    }

    // 竣工後進度落後
    if (data.Values[2] > 0) {
        result.push({ category: "D3_竣工後進度落後(D3)", value: data.Values[2], color: "#FFD2D2" });
    }

    // null
    if (data.Values[3] > 0) {
        result.push({ category: "null_null", value: data.Values[3], color: "#A9A9A9" });
    }

    return result;
}

/**
 * 取得落後項目
 * @param {*} data 查詢結果
 * @returns 
 */
const getDelayItemData = (data) => {
    let categories = [];
    let behinds = [];

    data.forEach(x => {
        categories.push(`${x.Key}_${x.Text}`);
        behinds.push(x.Values[0]);
    });

    let result = {
        Categories: categories,
        Data: [
            { name: "落後", data: behinds, color: "red" }
        ]
    };
    return result;
}

/**
 * 取得分析計劃清單
 * @param {*} formData 查詢條件
 * @returns 
 */
const getAnalyzePlan = async (formData) => {
    let url = APIUrl + 'Superior/GetAnalyzePlan';
    let response = await api.Post(url, JSON.stringify(formData));
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

const IPCAnalyzeService = {
    getOrganData,
    getAnalysis,
    getStatCnt,
    getStatBudgetAmt,

    getPieChartStat,
    getAnalysisDetail,
    getDetailChartStat,
    getDelayPieData,
    getDelayItemData,

    getAnalyzePlan
}

export default IPCAnalyzeService;
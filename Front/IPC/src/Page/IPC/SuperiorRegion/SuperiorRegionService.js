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
 * 取得區域統計清單
 * @param {*} formData 查詢條件 
 * @returns 
 */
const getRegion = async (formData) => {
    let url = APIUrl + 'Superior/GetRegions';
    let response = await api.Post(url, JSON.stringify(formData));
    let result = [];
    if (response.ok) {
        result = await response.json();
    }

    return result;
}

/**
 * 取得長條圖資料
 * @param {*} data 查詢結果資料
 * @returns 
 */
const getChartData = (data) => {
    let categories = [];
    let seriousBehinds = [];
    let behinds = [];
    let conforms = [];

    data.forEach(x => {
        categories.push(`${x.Key}_${x.Text}`);
        seriousBehinds.push(x.Values[0]);
        behinds.push(x.Values[1]);
        conforms.push(x.Values[2]);
    });

    const result = {
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
 * 取得地圖資料
 * @param {*} data 
 */
const getMapData = (data) => {
    const areaPaths = [
        { name: "桃園區", top: "75px", right: "134px" },
        { name: "中壢區", top: "108px", right: "195px" },
        { name: "八德區", top: "126px", right: "143px" },
        { name: "蘆竹區", top: "22px", right: "137px" },
        { name: "楊梅區", top: "157px", right: "270px" },
        { name: "大園區", top: "33px", right: "216px" },
        { name: "龍潭區", top: "217px", right: "208px" },
        { name: "龜山區", top: "65px", right: "90px" },
        { name: "平鎮區", top: "161px", right: "201px" },
        { name: "大溪區", top: "197px", right: "138px" },
        { name: "新屋區", top: "107px", right: "322px" },
        { name: "觀音區", top: "66px", right: "286px" },
        { name: "復興區", top: "315px", right: "76px" }
    ];

    let result = [];
    data.forEach(x => {
        const areaPath = areaPaths.find(y => y.name === x.Text);
        if (areaPath !== undefined) {
            const cnt = x.Values[0] + x.Values[1] + x.Values[2];
            result.push({ name: x.Text, top: areaPath.top, right: areaPath.right, cnt: cnt });
        }
    })
    return result;
}

/**
 * 取得區域統計計劃清單
 * @param {*} formData 查詢條件
 * @param {*} TOWN_C 辦理地點
 * @returns 
 */
const getRegionPlan = async (formData, TOWN_C) => {
    let body = {
        MASTER_DEPT: formData.MASTER_DEPT,
        EXEC_DEPT: formData.EXEC_DEPT,
        ENGNEER_STAGE: formData.ENGNEER_STAGE,
        IS_HAND_ROLE: formData.IS_HAND_ROLE,
        TOWN_C: TOWN_C
    };

    let url = APIUrl + 'Superior/GetRegionPlan';
    let response = await api.Post(url, JSON.stringify(body));
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

const SuperiorRegionService = {
    getOrganData,
    getRegion,
    getChartData,
    getMapData,
    getRegionPlan
}

export default SuperiorRegionService;
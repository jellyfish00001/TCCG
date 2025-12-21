import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import { GetSetParam, getOrganList, getOrgByUsr } from '../../../Basic/CommonService';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得下拉清單
 * @returns 
 */
const getAllDropDowns = async (checkIsHANDRole) => {
    let responses = await Promise.all([
        // 取得機關
        checkIsHANDRole ? getOrgByUsr() : getOrganList(),
        // 取得類別
        GetSetParam('COM_PLANKIND', '', true),
    ]);

    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    // 機關
    result[0].unshift({ text: "請選擇", value: "" });

    return result;
}

/**
 * 取得資料
 * @param {*} formData 查詢條件 
 */
const getProgess = async (formData) => {
    let buildKinds = [];
    formData.BUILD_KIND.forEach(x => {
        buildKinds.push(x.SET_TYPE);
    });

    let body = {
        BUILD_KIND: buildKinds, // 建設類別
        MASTER_DEPT: formData.MASTER_DEPT, // 主管機關
        EXEC_DEPT: formData.EXEC_DEPT, // 執行機關
        IS_HAND_ROLE: formData.IS_HAND_ROLE, // 是否只有主辦權限
    }
    let url = APIUrl + 'Superior/GetProgesses';
    let response = await api.Post(url, JSON.stringify(body));
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得地圖資料
 * @param {*} data 
 */
const getMapData = (data) => {
    const areaPaths = [
        { name: "桃園區", top: "96px", right: "184px" },
        { name: "中壢區", top: "143px", right: "269px" },
        { name: "八德區", top: "168px", right: "193px" },
        { name: "蘆竹區", top: "19px", right: "184px" },
        { name: "楊梅區", top: "213px", right: "376px" },
        { name: "大園區", top: "34px", right: "296px" },
        { name: "龍潭區", top: "302px", right: "286px" },
        { name: "龜山區", top: "80px", right: "114px" },
        { name: "平鎮區", top: "219px", right: "277px" },
        { name: "大溪區", top: "272px", right: "182px" },
        { name: "新屋區", top: "140px", right: "453px" },
        { name: "觀音區", top: "80px", right: "399px" },
        { name: "復興區", top: "442px", right: "93px" }
    ];

    let result = [];
    data.forEach(x => {
        const areaPath = areaPaths.find(y => y.name === x.TOWN_NAME);
        const isExist = result.find(y => y.name === x.TOWN_NAME);

        if (areaPath !== undefined && isExist === undefined) {
            result.push({ name: x.TOWN_NAME, top: areaPath.top, right: areaPath.right });
        }
    })
    return result;
}

const SuperiorProgessService = {
    getAllDropDowns,
    getProgess,
    getMapData
}

export default SuperiorProgessService;
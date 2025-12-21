import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import { GetSetParam, getOrganList, getCodeTownByCityId } from "../../../Basic/CommonService";
import { getCodePlanItem } from "../ProjectFillBasic/ProjectFillBasicService";
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { Download } from "../../../Basic/Download";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得計畫預覽資料
 * @param {*} projectNo 
 * @param {*} type 
 * @returns 
 */
const getProjectPrint = async (projectNo, type) => {
    let url = APIUrl + 'Project/GetProjectPrint';
    let form = new FormData();
    form.append('PROJECT_NO', projectNo);
    form.append('type', type);
    let response = await api.Post(url, form, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 下載計畫報表
 * @param {*} model 
 */
export const downProjectPrint = async (model) => {
    let url = APIUrl + 'RPT/RPTProjectPrint';
    Download(url, 'POST', model);
}

/**
 * 差異比對結果
 * @param {*} model 
 */
export const getDiffCompare = async (model) => {
    let url = APIUrl + 'RPT/RPTProjectPrint';
    let response = await api.Post(url, JSON.stringify(model), null, false);
    let result = [];
    if (response.ok) {
        result = await response.text();
    }
    return result;
}

/**
 * 查詢下拉選單資料
 * @returns 
 */
const getDdlData = () => {
    let result = [
        { text: "基本資料", value: "A" },
        { text: "基本資料及執行情形(無管考備註)", value: "B" },
        { text: "基本資料及執行情形", value: "C" },
    ];
    return result;
}

/**
 * 取得基本資料下拉
 * @returns 
 */
const getProjectBasicDdl = async () => {
    let responses = await Promise.all([
        // 預算類型
        GetSetParam('BUDGETCLASS', '', true),
        // 中央預算來源
        getCodePlanItem('2'),
        // // 本府預算來源
        getCodePlanItem('1'),
        // 取得建設項目類別            
        GetSetParam('BUILD_KIND_TYPE', '', true),
        // 取得建設類別            
        GetSetParam('COM_PLANKIND', '', true),
        // 取得相關審查            
        GetSetParam("COM_REVIEWITEM", "", true),
        // 取得機關
        getOrganList(),
        // 辦理地點
        getCodeTownByCityId("H", "", true),
        // 計畫狀態
        GetSetParam('PROJECT_STATUS', '', true),
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

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


// 找代碼的中文
const findText = (ddl, textField, valueField, value) => {
    if (IsNullOrEmpty(ddl) || IsNullOrEmpty(value)) {
        return "";
    }
    else {
        let result = ddl.find(x => x[valueField] == value)
        return result == undefined ? "" : result[textField];
    }
}

const ProjectPrintService = {
    getProjectPrint: getProjectPrint,
    downProjectPrint: downProjectPrint,
    getDdlData: getDdlData,
    getProjectBasicDdl: getProjectBasicDdl,
    loadAllDropDowns: loadAllDropDowns,
    findText: findText,
    getDiffCompare: getDiffCompare
}
export default ProjectPrintService;
import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import { SetStorageData } from "../../../Basic/CommonService";
import { Download } from '../../../Basic/Download';
import { openPage } from "../../../Basic/SDOExtension";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 開啟調整內容
 * @param {*} item
 */
export const openAdjustContent = async (item) => {
    SetStorageData(item, "printInfo");
    // 同一個計畫只會有一個分頁，用projAdjId當NAME
    openPage(process.env.PUBLIC_URL + '/AdjustContent', `Adjust${item.projAdjId.toString()}`, `調整內容(${item.projectNo})`)
}

/**
 * 差異比對結果
 * @param {*} model 
 */
export const getDiffCompare = async (model) => {
    let url = APIUrl + 'RPT/RPTProjectAdjustDiff';
    let response = await api.Post(url, JSON.stringify(model));
    let result = [];
    if (response.ok) {
        result = await response.text();
    }
    return result;
}

/**
 * 下載計畫報表
 * @param {*} model 
 */
export const downProjectAdjustContent = async (model) => {
    let url = APIUrl + 'RPT/RPTProjectAdjustDiff';
    Download(url, 'POST', model);
}

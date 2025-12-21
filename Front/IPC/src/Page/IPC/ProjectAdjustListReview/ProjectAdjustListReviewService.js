import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import { getOrganList, getPlanYearList, GetSetParam } from "../../../Basic/CommonService";
import { addDefautItem } from "../ProjectAdjustListExec/ProjectAdjustListExecService";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 調整撤銷狀態預設值 => A02(調整基本資料審核)、B02(調整期程審核)、W02(撤銷審核)
 */
export const projAwStatusDefaultList = ['A02', 'B02', 'W02'];

/**
 * 初始篩選條件
 */
export const initQueryData = {
    PROJECT_YEAR: 0,
    PROJECT_NAME: "",
    PROJECT_AW_STATUS: [],
    PROJECT_NO: "",
    RUNWAY_C: "",
    SPEC_NOTE: "",
    EXEC_ORGAN_C: "",
    refreshCnt: 0
};

/**
 * 取得所有查詢條件下拉式選單
 * @returns 
 */
export const getAllDropDowns = async () => {
    let responses = await Promise.all([
        GetSetParam('PROJECT_AW_STATUS', '', true),      // 取得調整撤銷狀態
        GetSetParam('SPEC_NOTE', '', true, null),        // 取得特殊加註
        getOrganList(),                                  // 取得機關清單
        getPlanYearList(true),                           // 取得計畫年度
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));
    if (result) {
        //加入預設值請選擇
        result = addDefautItem(result);
        return result;
    } else {
        return [];
    }
}

/**
 * 產生組織樹資料
 * @param {*} data 計畫清單資料
 * @returns 
 */
export const genOrgTreeData = (data) => {
    // 取得計畫列表機關清單
    let projOrgPanelDataList = data
        .map(proj => {
            let projCnt = data.filter(x => x.EXEC_ORG_C == proj.EXEC_ORG_C).length;
            return {
                OU_ID: proj.EXEC_ORG_C,
                title: proj.EXEC_ORG_NAME + '(' + projCnt + ')',
                order: proj.EXEC_ORG_ORDER
            }
        });
    // Distinct projOrgPanelDataList
    let distinctData = projOrgPanelDataList.filter((value, index, self) =>
        index === self.findIndex((t) => (
            t.OU_ID === value.OU_ID && t.OU_NAME === value.OU_NAME
        ))
    );
    distinctData.sort((a, b) => a.order - b.order);
    distinctData.unshift({ OU_ID: "", title: `桃園市政府(${data.length})` });
    return distinctData;
}

/**
 * 取得計畫列表
 * @param {*} requestModel 篩選條件
 * @returns 
 */
export const getProjectList = async (requestModel) => {
    let url = APIUrl + 'ProjectAdjust/GetAdjustList';
    requestModel = {...requestModel, IsReview: 1};
    let response = await api.Post(url, JSON.stringify(requestModel), null, false);
    if (response.ok) {
        return response.json();
    } else {
        return [];
    }
}

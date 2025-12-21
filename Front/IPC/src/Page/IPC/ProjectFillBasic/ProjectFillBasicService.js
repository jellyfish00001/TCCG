import { GetSetParam, getPlanYearList, getOrganList, getCodeTownByCityId } from "../../../Basic/CommonService";
import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { IsNullOrEmpty } from "../../../Basic/SDOExtension";
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得計畫基本資料下拉清單
 * @returns 
 */
export const getAllDropDowns = async () => {
    let responses = await Promise.all([
        // 取得相關審查            
        GetSetParam("COM_REVIEWITEM", "", true),
        // 取得機關
        getOrganList(),
        // 辦理地點
        getCodeTownByCityId("H", "", true)
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    // 機關
    result[1].unshift({ text: "請選擇機關", value: "" });
    // 辦理地點
    result[2].unshift({ text: "請選擇", value: "" });
    return result;
}

export const getBudgetSourceAllDropDowns = async () => {
    let responses = await Promise.all([
        // 取得計畫年度
        getPlanYearList(true, 10, "A"),
        // 預算類型
        GetSetParam('BUDGETCLASS', '', true),
        // 中央預算來源            
        getCodePlanItem('2'),
        // 本府預算來源            
        getCodePlanItem('1')
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    // 計畫年度
    result[0].unshift({ text: "請選擇", value: "" });
    // 預算類型
    result[1].unshift({ SET_VALUE: "請選擇", SET_TYPE: "" });
    // 中央預算來源
    result[2].unshift({ PLAN_ITEM_NAME: "請選擇", PLAN_ITEM_ID: "" });
    // 本府預算來源
    result[3].unshift({ PLAN_ITEM_NAME: "請選擇", PLAN_ITEM_ID: "" });
    return result;
}

/**
 * 取得預算來源
 * @param {*} levelMark 1:本府預算、2:中央部會
 * @returns 
 */
export const getCodePlanItem = async (levelMark) => {
    let url = APIUrl + 'SetParam/GetCodePlanItem';
    let form = new FormData();
    form.append('LEVEL_MARK', levelMark);
    form.append('DEL_FLG', false);
    let response = await api.Post(url, form, new Headers(), false);
    return response;
}

/**
 * 取得建設項目類別&建設類別
 * @returns 
 */
export const getBuildKindList = async () => {
    let responses = await Promise.all([
        // 取得建設項目類別            
        GetSetParam('BUILD_KIND_TYPE', '', true),
        // 取得建設類別            
        GetSetParam('COM_PLANKIND', '', true),
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    result = dataConvert(result, "");

    return result;
}

/**
 * 資料轉換
 * @param {*} data 
 * @returns 
 */
const dataConvert = (data) => {
    return data.map(item =>
        item.map(x => {
            const result = { label: x.SET_VALUE, value: x.SET_TYPE }
            return result;
        })
    );
}

/**
 * 取得計畫基本資料(含計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關)
 * @param {*} projectNo 列管編號
 * @param {*} projAdjId 調整流水號
 * @returns 
 */
export const getProjectBasicFill = async (projectNo, projAdjId) => {
    let url = APIUrl + 'Project/GetProjectBasicFill';
    let response = null;
    if (projAdjId) {
        url = APIUrl + 'ProjectAdjust/GetProjectBasicAdj';
        let form = new FormData();
        form.append('PROJECT_NO', projectNo);
        form.append('PROJ_ADJ_ID', projAdjId);
        response = await api.Post(url, form, new Headers(), false);
    }
    else {
        response = await api.Post(url, JSON.stringify(projectNo ?? ""), null, false);
    }
    let result = 0;
    if (response.ok) {
        result = await response.json();
    }
    const { BUDGET_HOLD_ORGAN_C } = result.ProjectBasic

    // 代辦機關是否使用
    result.ProjectBasic.BUDGET_HOLD_IS_ENABLE = IsNullOrEmpty(BUDGET_HOLD_ORGAN_C) ? false : true

    if (result.ProjectBudgetSourceG != null && result.ProjectBudgetSourceG.length > 0) {
        result.ProjectBudgetSourceG.forEach(x => {
            x.disabled = IsNullOrEmpty(x.PLAN_ITEM_C);
        });
    }

    return result;
}

/**
 * 儲存計畫基本資料(含計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關)
 * @param {*} data 
 * @param {*} isAdj 存到哪裡的資料，false: 主檔，true: 調整檔
 * @returns 
 */
export const saveProjectBasicAdd = async (data, isAdj) => {
    let result = null;
    let url = APIUrl + 'Project/SaveProjectBasicAdd';
    if (isAdj) {
        url = APIUrl + 'ProjectAdjust/SetProjectBasicAdj';
    }
    let response = await api.Post(url, JSON.stringify(data), null, false);
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

//#region 存檔驗證
Yup.setLocale({
    //設定必填欄位顯示的訊息
    mixed: {
        required: "此欄位必填",
    }
});

/**
 * 欄位驗證
 */
export const validateBasicField = Yup.object().shape({
    PROJECT_NAME: Yup.string().nullable()
        .when('PROJECT_NO', { is: (PROJECT_NO) => IsNullOrEmpty(PROJECT_NO), then: Yup.string().required() }),
    PROJECT_YEAR: Yup.string().required(),
    EXEC_ORGAN_C: Yup.string().nullable()
        .when('PROJECT_NO', { is: (PROJECT_NO) => IsNullOrEmpty(PROJECT_NO), then: Yup.string().required() }),
    EXEC_UNDERTAKER_C: Yup.string().nullable()
        .when('PROJECT_NO', { is: (PROJECT_NO) => IsNullOrEmpty(PROJECT_NO), then: Yup.string().required() }),
    BUDGET_HOLD_ORGAN_C: Yup.string().nullable()
        .when(['BUDGET_HOLD_IS_ENABLE'],
            {
                is: (BUDGET_HOLD_IS_ENABLE) => BUDGET_HOLD_IS_ENABLE,
                then: Yup.string().required()
            }),
    BUDGET_HOLD_UNDERTAKER_C: Yup.string().nullable()
        .when(['BUDGET_HOLD_ORGAN_C'],
            {
                is: (BUDGET_HOLD_ORGAN_C) =>
                    !IsNullOrEmpty(BUDGET_HOLD_ORGAN_C),
                then: Yup.string().required()
            }),
    TOWN_M: Yup.string().nullable()
        .when(['TOWN_C'], {
            is: (TOWN_C) =>
                TOWN_C == "H01",
            then: Yup.string().required()
        }),
    ALL_JOB: Yup.string().nullable().max(1000, '字數限制1000字以內'),
    PROJECT_BENEFIT: Yup.string().nullable().max(1000, '字數限制1000字以內'),
    MEMO: Yup.string().nullable().max(500, '字數限制500字以內'),
    PROJECT_LOCATION: Yup.string().nullable().max(200, '字數限制200字以內'),
});

// GRID資料驗證
export const validataSourceGField = Yup.object().shape({
    PLAN_YEAR: Yup.string().required(),
    BUDGET_CLASS: Yup.string().required().nullable(),
    PLAN_ITEM_C: Yup.string().nullable()
        .test(
            'PLAN_ITEM_C',
            '此欄位必填',
            function (item) {
                if (this.parent.BUDGET_CLASS === "1") {
                    return !IsNullOrEmpty(item)
                } else {
                    return true
                }
            }
        ),
    BUDGET_CENTRAL: Yup.number().nullable(),
    PLAN_ITEM_L: Yup.string().nullable(),
    BUDGET_LOCAL: Yup.number().nullable(),
    //核定函檔案
    FILE: Yup.array().nullable()
        .test(
            'BuildKindValid',
            '為前瞻預算，請上傳檔案',
            function (item) {
                if (this.parent.BUDGET_CLASS === "1") {
                    return !IsNullOrEmpty(item) && item.find(x => x.editType != 3)
                } else {
                    return true
                }
            }
        )
});
//#endregion 

export const undertakerType = {
    //主管機關
    MASTER: 1,

    //執行機關
    EXEC: 2,

    //協辦機關
    ASSISTANT: 3,

    //代辦機關
    BUDGET_HOLD: 4
}
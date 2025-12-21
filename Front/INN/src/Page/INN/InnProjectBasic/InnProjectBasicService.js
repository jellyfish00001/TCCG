import { GetSetParam, getPlanYearList, getOrganList, getCodeTownByCityId, getInnPropsalType } from "../../../Basic/CommonService";
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
        // 取得主要提案類別           
        getInnPropsalType((new Date().getFullYear() - 1911).toString(), true),
        // 取得機關
        getOrganList(),
    ])
    //responses轉Json
    let responsesJson = responses.map(res => res.json());
    let result = await (Promise.all(responsesJson));

    // 機關
    result[1].unshift({ text: "請選擇機關", value: "" });
    // 辦理地點
    result[0].unshift({ text: "請選擇", value: "" });
    return result;
}

// 檔案初始資料
export const initFiles = {
    PROJECT_NO: "",
    FILE_KIND: "", // SET_PARAM.SET_ITEM='FILE_KIND'
    EditFiles: [],
};

/**
 * #region 存檔驗證
 */
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
    INN_PLAN_NAME: Yup.string().nullable().required("此欄為必填")
        .when('INN_PLAN_NO', { is: (INN_PLAN_NO) => IsNullOrEmpty(INN_PLAN_NO), then: Yup.string().required() }),
    SPONSOR_TYPE: Yup.string().nullable().required("此欄為必填"),
    GROUP: Yup.string().nullable().required("此欄為必填"),
    REJECT_YN: Yup.string().nullable().required("此欄為必填"),
    PROPOSAL_TYPE: Yup.string().nullable().required("此欄為必填"),
    SPREAD_IDEA_YN:Yup.string().nullable().required("此欄為必填"),
    ORIGINATE_YN:Yup.string().nullable().required("此欄為必填"),
    SPONSOR_ORG:Yup.string().nullable().required("此欄為必填"),
    SPONSOR_NAME:Yup.string().nullable().required("此欄為必填"),
    INN_DESCRIPTION: Yup.string().nullable().required("此欄為必填").max(500, '字數限制500字以內'),
    EXPECT_BENEFIT: Yup.string().nullable().required("此欄為必填").max(500, '字數限制500字以內'),
    IDEA_CONTENT: Yup.string().nullable().required("此欄為必填").max(500, '字數限制500字以內'),
});



/**
 * 取得創新提案基本資料
 * @param {*} projectNo 列管編號
 * @param {*} projAdjId 調整流水號
 * @returns 
 */
export const getInnProjectBasic = async (projectNo) => {
    let url = APIUrl + 'InnProject/GetInnBasic';
    let response = null;
    response = await api.Post(url, JSON.stringify(projectNo ?? ""), null, false);
    let result = 0;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存資料資料
 * @param {*} data 
 * @returns 
 */
export const saveInnBasic = async (data) => {
    let result = null;
    let url = APIUrl + 'InnProject/SaveInnBasic';

    let response = await api.Post(url, JSON.stringify(data), null, false);
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

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




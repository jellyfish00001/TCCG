import { api } from '../../../Basic/ApiFetch';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware'

const setParamUrl = getGlobalServerConfig().backEndUrl.get() + 'SetParam/';
const eFormSetUrl = getGlobalServerConfig().backEndUrl.get() + 'eFormSet';
const flowSetUrl = getGlobalServerConfig().backEndUrl.get() + 'FlowSet';

//取得表單設定清單
const getFormSet = async (filterField) => {
    let url = eFormSetUrl + '/GeteForm'
    let response = await api.Post(url, JSON.stringify(filterField))
    let formSet = []
    if (response.ok)
        formSet = AddNoColumn(await response.json());

    return formSet;
}

//新增表單設定
const insertFormSet = async (formSetData) => {
    let response = await api.Post(eFormSetUrl, JSON.stringify(formSetData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//修改表單設定
const updateFormSet = async (formSetData) => {
    let response = await api.Put(eFormSetUrl, JSON.stringify(formSetData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//刪除表單設定
const deleteFormSet = async (formId) => {
    let url = eFormSetUrl + '/' + formId
    let response = await api.Delete(url)

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//取得表單類型下拉選單的data
const getFormType = async (defaultValue) => {
    let url = setParamUrl + 'eFormType'
    let response = await api.Read(url)

    let formType = []
    if (response.ok) {
        formType = await response.json();
        formType.unshift({ SET_TYPE: "", SET_VALUE: defaultValue })
    }

    return formType;
}

//取得表單使用流程下拉選單的data
const getMapFlow = async () => {
    let response = await api.Read(flowSetUrl)
    let mapFlow = []
    if (response.ok) {
        mapFlow = await response.json()
        mapFlow.unshift({ FLOW_ID: "", FLOW_NAME: "請選擇...", ENABLE_FLG: true })
        mapFlow = mapFlow.filter(x => x.ENABLE_FLG === true)
    }

    return mapFlow;
}

//取得表單設定byFormId
const getFormSetByFormId = async (formId) => {
    let formSet = {
        FORM_TYPE: "",
        FORM_ID: "",
        FORM_NAME: "",
        EFFECTIVE_DATE: new Date(),
        EXPIRE_DATE: new Date(),
        MAP_FLOW: "",
        MAP_ORG: [],
        MEMO: ""
    }

    const [MAP_FLOW, MAP_ORG] = await Promise.all([
        //取得表單的對應流程byFormId
        getMapFlowByFormId(formId),
        //取得表單的對應使用單位byFormId
        getMapOrgsByFormId(formId),
    ]);

    let url = eFormSetUrl + '/' + formId
    let response = await api.Read(url)
    if (response.ok) {
        let data = await response.json()
        formSet = {
            FORM_TYPE: data.FORM_TYPE,
            FORM_ID: data.FORM_ID,
            FORM_NAME: data.FORM_NAME,
            EFFECTIVE_DATE: new Date(data.EFFECTIVE_DATE),
            EXPIRE_DATE: new Date(data.EXPIRE_DATE),
            MAP_FLOW: MAP_FLOW,
            MAP_ORG: MAP_ORG,
            MEMO: data.MEMO
        }
    }

    return formSet
}

//取得表單的對應流程byFormId
const getMapFlowByFormId = async (formId) => {
    let url = eFormSetUrl + '/ReadMapFlows/' + formId
    let response = await api.Read(url)

    let mapFlow = ""
    if (response.ok)
        mapFlow = await response.text()

    return mapFlow;
}

//取得表單的對應使用單位byFormId
const getMapOrgsByFormId = async (formId) => {
    let url = eFormSetUrl + '/ReadMapOrgs/' + formId
    let response = await api.Read(url)

    let mapOrgs = []
    if (response.ok)
        mapOrgs = (await response.json()).map(item => {
            const result = { ORG_DISPLAY: item.Text, ORG_ID: item.Value }
            return result;
        })

    return mapOrgs;
}

const FormSetService = {
    getFormSet: getFormSet,
    insertFormSet: insertFormSet,
    updateFormSet: updateFormSet,
    deleteFormSet: deleteFormSet,
    getFormType: getFormType,
    getMapFlow: getMapFlow,
    getFormSetByFormId: getFormSetByFormId,

}

export default FormSetService;
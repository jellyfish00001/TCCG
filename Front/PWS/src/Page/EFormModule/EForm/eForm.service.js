import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { AddNoColumn, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'


const eFormUrl = getGlobalServerConfig().backEndUrl.get() + 'eForm';

//取得表單下拉選單data
const getFormDataForDDL = async () => {
    let url = eFormUrl + '/ReadForms'
    let response = await api.Get(url)
    let formData = []
    if (response.ok) {
        formData = await response.json()
        if (!IsNullOrEmpty(formData)) {
            formData = formData.map(item => {
                const result = { FORM_ID: item.FORM_ID, FORM_NAME: item.FORM_ID + "-" + item.FORM_NAME }
                return result;
            })
        }
    }

    return formData
}

//取得表單清單
const getForm = async (formId) => {
    let url = eFormUrl + '?formId=' + formId;
    let response = await api.Get(url)
    let formData = []
    if (response.ok) {
        formData = AddNoColumn(await response.json())
    }

    return formData
}

//新增表單
const insertForm = async (formData) => {
    let response = await api.Post(eFormUrl, JSON.stringify(formData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//修改表單
const updateForm = async (formData) => {
    let response = await api.Put(eFormUrl, JSON.stringify(formData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

// 刪除表單
const deleteForm = async (fillId) => {
    let url = eFormUrl + '/' + fillId
    let response = await api.Delete(url)

    return {
        ok: response.ok,
        result: await response.json()
    };
}


//取得表單填寫的資料
const getFormFieldData = async (fillId) => {
    let url = eFormUrl + '/ReadDataFill?fillId=' + fillId
    let response = await api.Get(url)
    let formFiledData = ""
    if (response.ok) {
        formFiledData = await response.text();
    }

    return formFiledData
}


//取得表單使用流程下拉選單的data
const getMapFlow = async (fillId) => {
    let formSet = {
        ddlData: [],
        ddlValue: ""
    }
    //預設下拉選單value(取得表單設定時所設定的流程ID)
    let flowId = await getFormFlowId(fillId)
    let url = getGlobalServerConfig().backEndUrl.get() + 'FlowSet'
    let response = await api.Get(url)
    if (response.ok) {
        formSet = {
            ddlData: (await response.json()).filter(x => x.ENABLE_FLG === true),
            ddlValue: flowId
        }
    }

    return formSet
}

//取得表單設定時所設定的流程ID
const getFormFlowId = async (fillId) => {
    let url = eFormUrl + '/GetFlowId?fillId=' + fillId
    let response = await api.Get(url)
    let flowId = ""
    if (response.ok) {
        flowId = await response.text();
    }

    return flowId
}

//啟動簽核流程
const sendFlow = async (data) => {
    let url = eFormUrl + '/UpdateFlow'
    let response = await api.Post(url, JSON.stringify(data))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

const FormService = {
    getFormDataForDDL: getFormDataForDDL,
    getForm: getForm,
    insertForm: insertForm,
    updateForm: updateForm,
    deleteForm: deleteForm,
    getFormFieldData: getFormFieldData,
    getMapFlow: getMapFlow,
    sendFlow: sendFlow
}

export default FormService;
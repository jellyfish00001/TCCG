import { api } from '../../../Basic/ApiFetch';
import { AddNoColumn, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware'


let flowSetUrl = getGlobalServerConfig().backEndUrl.get() + 'FlowSet';

// 取得流程清單grid
const getFlowSetData = async () => {
    let response = await api.Get(flowSetUrl)
    let flowSetData = []
    if (response.ok) {
        flowSetData = AddNoColumn(await response.json());
    }
    return flowSetData
}

// 取得流程主檔
const getFlowByFlowId = async (flowId) => {
    let url = flowSetUrl + '/' + flowId
    let response = await api.Get(url)
    let flowSetData = []
    if (response.ok) {
        flowSetData = await response.json();
    }
    return flowSetData
}

// 取得流程關卡明細
const getFlowDetailByFlowId = async (flowId) => {
    let url = getGlobalServerConfig().backEndUrl.get() + 'FlowSetDetail/' + flowId
    let response = await api.Get(url)
    let flowSetData = [];
    if (response.ok) {
        flowSetData = await response.json();
    }
    return gridData(flowSetData);
}

// 重新組Grid顯示的資料
function gridData(items) {
    return items.map(item => {
        const result = {
            SET_ODR: item.SET_ODR,
            SET_ORG_ID: item.SET_ORG_ID,
            SET_ROLE_ID: item.SET_ROLE_ID,
            SET_USER_ID: item.SET_USER_ID,
            SET_FLOW_ID: item.SET_FLOW_ID,
            FLOW_STAGE_NAME: item.SET_OPTION.FLOW_STAGE_NAME,
            MAIL: item.SET_OPTION.MAIL,
            SET_DECISION: item.SET_DECISION,
            SIGNATURE: item.SET_OPTION.SIGNATURE,
            CERTIFICATE: item.SET_OPTION.CERTIFICATE,
            SEALED: item.SET_OPTION.SEALED
        }
        return result;
    }).filter(f => f !== undefined);
}

// 取得下拉選單資料
const getDDLDataList = async (url, defaultItem) => {
    let response = await api.Get(getGlobalServerConfig().backEndUrl.get() + url)
    let ddlDatas = [];
    if (response.ok) {
        ddlDatas = await response.json();
    }
    ddlDatas.unshift(defaultItem);
    return ddlDatas;
}

// 新增流程
const createFlow = async (flowData) => {
    let response = await api.Post((flowSetUrl), JSON.stringify(flowData))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}

//修改功能
const updateFlow = async (flowData) => {
    let response = await api.Put((flowSetUrl), JSON.stringify(flowData))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}

// 將grid資料轉換為存檔的格式
function detailData(items, flowId) {
    items = AddNoColumn(items);
    return items.map(item => {
        const result = {
            FLOW_ID: flowId,
            SET_ODR: item.NO,
            SET_ORG_ID: IsNullOrEmpty(item.SET_ORG_ID) ? "" : item.SET_ORG_ID,
            SET_ROLE_ID: IsNullOrEmpty(item.SET_ROLE_ID) ? "" : item.SET_ROLE_ID,
            SET_USER_ID: IsNullOrEmpty(item.SET_USER_ID) ? "" : item.SET_USER_ID,
            SET_FLOW_ID: IsNullOrEmpty(item.SET_FLOW_ID) ? "" : item.SET_FLOW_ID,
            SET_DECISION: IsNullOrEmpty(item.SET_DECISION) ? false : item.SET_DECISION,
            SET_OPTION: {
                MAIL: IsNullOrEmpty(item.MAIL) ? false : item.MAIL,
                SIGNATURE: IsNullOrEmpty(item.SIGNATURE) ? false : item.SIGNATURE,
                CERTIFICATE: IsNullOrEmpty(item.CERTIFICATE) ? false : item.CERTIFICATE,
                SEALED: IsNullOrEmpty(item.SEALED) ? false : item.SEALED
            }
        }
        return result;
    }).filter(f => f !== undefined);
}

//修改功能
const updateFlowDetail = async (flowId, flowDetailData) => {
    let response = await api.Put((getGlobalServerConfig().backEndUrl.get() + 'FlowSetDetail'), JSON.stringify({
        FLOW_ID: flowId,
        SET_STAGE: detailData(flowDetailData, flowId)
    }))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}

//刪除功能
const deleteFlowSetData = async (flowId) => {
    let url = flowSetUrl + '/' + flowId
    let response = await api.Delete(url)

    return {
        ok: response.ok,
        result: await response.json()
    }
}

const FlowSetService = {
    getFlowSetData: getFlowSetData,
    getFlowByFlowId: getFlowByFlowId,
    getFlowDetailByFlowId: getFlowDetailByFlowId,
    createFlow: createFlow,
    updateFlow: updateFlow,
    updateFlowDetail: updateFlowDetail,
    deleteFlowSetData: deleteFlowSetData,
    getDDLDataList: getDDLDataList
}

export default FlowSetService;

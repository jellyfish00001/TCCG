import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { AddNoColumn, IsNullOrEmpty } from '../../../Basic/SDOExtension';

let setParamItemUrl = ServerConfig.backEndUrl + 'SetParamItem';
let setParamUrl = ServerConfig.backEndUrl + 'SetParam';


// 取得參數主檔
const getParamItemData = async () => {
    let response = await api.Get(setParamItemUrl)
    let paramItemData = []
    if (response.ok) {
        paramItemData = AddNoColumn(await response.json());
    }
    return paramItemData
}

// 取得參數資料
const getParamData = async (setItem) => {
    let url = setParamUrl +'/'+ setItem
    let response = await api.Get(url)
    let paramData = [];
    if (response.ok) {
        paramData = AddNoColumn(await response.json());
    }
    return paramData;
}

// 取得參數類別
const getParamItemBySetItem = async (setItem) => {
    let url = setParamItemUrl + '/' + setItem
    let response = await api.Get(url)
    let paramItemData = []
    if (response.ok) {
        paramItemData = await response.json();
    }
    return paramItemData
}

// 取得參數類別
const getParamBySetType = async (setItem,setType) => {
    let url = setParamUrl + '/' + setItem+ '/' + setType
    let response = await api.Get(url)
    let paramData = []
    if (response.ok) {
        paramData = await response.json();
    }
    return paramData
}

// 新增參數
const createParamItem = async (paramItemData) => {
    let response = await api.Post((setParamItemUrl), JSON.stringify(paramItemData))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}

//修改參數
const updateParamItem = async (paramItemData) => {
    let response = await api.Put((setParamItemUrl), JSON.stringify(paramItemData))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}



// 新增參數
const createParam = async (paramData) => {
    let response = await api.Post((setParamUrl), JSON.stringify(paramData))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}

//修改參數
const updateParam = async (paramData) => {
    let response = await api.Put((setParamUrl), JSON.stringify(paramData))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}

const setParamService = {
    getParamItemData: getParamItemData,
    getParamData:getParamData,
    getParamItemBySetItem:getParamItemBySetItem,
    createParamItem:createParamItem,
    updateParamItem:updateParamItem,
    getParamBySetType:getParamBySetType,
    createParam:createParam,
    updateParam:updateParam
}

export default setParamService;

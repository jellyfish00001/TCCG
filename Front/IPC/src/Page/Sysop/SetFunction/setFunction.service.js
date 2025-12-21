import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { AddNoColumn } from '../../../Basic/SDOExtension';

let setFunctionUrl = ServerConfig.backEndUrl + 'SetFunction'

// 取得功能清單grid
const getReadByGroup = async (searchData) => {
    let url = setFunctionUrl + '/ReadByGroup?';
    url += 'PARENT_ID=' + searchData.parentId;
    url += '&FUNCTION_ID=' + searchData.functionId;
    url += '&FUNCTION_NAME=' + searchData.functionName;
    let response = await api.Get(url)
    let readByGroup = []
    if (response.ok) {
        readByGroup = AddNoColumn(await response.json());
    }
    return readByGroup
}

// 取得功能清單grid
const getFunctionDDLList = async () => {
    let url = setFunctionUrl + '/ReadByFunctionRoot?';
    let response = await api.Get(url)
    let readByGroup = []
    if (response.ok) {
        readByGroup = await response.json();
    }

    return listData(readByGroup)
}

//取得單筆公告bySId
const getSetFunctionByFunctionId = async (functionId) => {
    let functionData = {};
    let url = setFunctionUrl + '/' + functionId
    let response = await api.Get(url)
    if (response.ok) {
        functionData = await response.json()
    }
    return functionData
}

// 重新組下拉選單顯示的資料
const listData = (items) => {
    return items.map(item => {
        const result = { text: item.FUNCTION_NAME, value: item.FUNCTION_ID }
        return result;
    }).filter(f => f !== undefined).sort((a, b) => a.sortId - b.sortId);
}

// 取得排序後資料ID
function sortListData(items) {
    return items.map(item => {
        const result = item.FUNCTION_ID;
        return result;
    }).filter(f => f !== undefined).sort((a, b) => a.sortId - b.sortId);
}

// 功能排序存檔
const setSortorder = async (readByGroupData) => {
    let url = setFunctionUrl + '/SetSortorder?';

    let response = await api.Put((url), JSON.stringify(sortListData(readByGroupData)))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}

// 新增功能
const createFunction = async (functionData,functionLevel) => {
    if(functionLevel==="root")
    {
        functionData.PARENT_ID="";
    }
    let response = await api.Post((setFunctionUrl), JSON.stringify(functionData))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}

//修改功能
const updateFunction = async (functionData,functionLevel) => {
    if(functionLevel==="root")
    {
        functionData.PARENT_ID="";
    }
    let response = await api.Put((setFunctionUrl), JSON.stringify(functionData))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}


//刪除功能
const deleteFunction = async (functionId) => {
    let url = setFunctionUrl + '/' + functionId
    let response = await api.Delete(url)

    return {
        ok: response.ok,
        result: await response.json()
    }
}

const SetFunctionService = {
    getReadByGroup: getReadByGroup,
    getFunctionDDLList: getFunctionDDLList,
    setSortorder: setSortorder,
    deleteFunction: deleteFunction,
    getSetFunctionByFunctionId: getSetFunctionByFunctionId,
    createFunction:createFunction,
    updateFunction:updateFunction
}

export default SetFunctionService;

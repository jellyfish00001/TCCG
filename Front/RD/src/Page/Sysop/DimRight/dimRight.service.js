import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'

const dimRightUrl = getGlobalServerConfig().backEndUrl.get() + 'DimRight';
const setFunctionUrl = getGlobalServerConfig().backEndUrl.get() + 'SetFunction';

//取得權限清單
const getDimRight = async () => {
    let response = await api.Read(dimRightUrl)
    let right = []
    if (response.ok)
        right = AddNoColumn(await response.json());

    return right;
}

//刪除權限
const deleteDimRight = async (rightId) => {
    let url = dimRightUrl + '/' + rightId
    let response = await api.Delete(url)

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//新增權限
const insertDimRight = async (rightData) => {
    let response = await api.Post(dimRightUrl, JSON.stringify(rightData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//修改權限
const updateDimRight = async (rightData) => {
    let response = await api.Put(dimRightUrl, JSON.stringify(rightData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//取得功能列表
const getFunction = async () => {
    let response = await api.Read(setFunctionUrl)
    let functionData = []
    if (response.ok)
        functionData = dualListData(await response.json());

    return functionData;
}

//取得權限清單ByRight_ID
const getDimRightByRightId = async (rightId) => {
    let url = dimRightUrl + '/' + rightId
    let response = await api.Read(url)
    let right = {
        RIGHT_ID: "",
        RIGHT_NAME: "",
        FUNCTIONS: []
    }
    //取得功能列表ByRight_ID
    let functions = await getFunctionByRightId(rightId);
    if (response.ok) {
        let data = await response.json()
        right = {
            RIGHT_ID: data.RIGHT_ID,
            RIGHT_NAME: data.RIGHT_NAME,
            FUNCTIONS: functions
        }
    }

    return right
}

//取得功能列表ByRight_ID
const getFunctionByRightId = async (rightId) => {
    let url = setFunctionUrl + '/ReadByRight/' + rightId
    let response = await api.Read(url)
    let functionData = []
    if (response.ok)
        functionData = dualListDataGetValue(await response.json());

    return functionData;
}

//重新組DualListBox顯示的資料
const dualListData = (items) => {
    //需重新排序
    return items.map(item => {
        const result = { label: item.FUNCTION_ID + '-' + item.FUNCTION_NAME, value: item.FUNCTION_ID }
        return result;
    })
}

//DualListBox只取得value
const dualListDataGetValue = (responseData) => {
    let data = [];
    responseData.map(item => {
        data.push(item.FUNCTION_ID)
        return data;
    });
    return data;
}

const DimRightService = {
    getDimRight: getDimRight,
    deleteDimRight: deleteDimRight,
    getFunction: getFunction,
    getDimRightByRightId: getDimRightByRightId,
    insertDimRight: insertDimRight,
    updateDimRight: updateDimRight
}

export default DimRightService;
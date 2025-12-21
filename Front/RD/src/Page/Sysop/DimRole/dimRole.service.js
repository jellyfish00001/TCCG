import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'

const dimRoleUrl = getGlobalServerConfig().backEndUrl.get() + 'DimRole';
const dimRightUrl = getGlobalServerConfig().backEndUrl.get() + 'DimRight';

//取得角色清單
const getDimRole = async () => {
    let response = await api.Get(dimRoleUrl);
    let role = []
    if (response.ok) {
        role = AddNoColumn(await response.json())
    }

    return role
}

//新增角色
const insertDimRole = async (roleData) => {
    let response = await api.Post(dimRoleUrl, JSON.stringify(roleData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//修改角色
const updateDimRole = async (roleData) => {
    let response = await api.Put(dimRoleUrl, JSON.stringify(roleData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//刪除角色 
const deleteDimRole = async (roleId) => {
    let url = dimRoleUrl + '/' + roleId
    let response = await api.Delete(url)

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//取得權利清單
const getDimRight = async () => {
    let response = await api.Get(dimRightUrl)
    let right = []
    if (response.ok) {
        right = dualListData(await response.json())
    }

    return right
}

//取得角色清單ByROLE_ID
const getDimRoleByRoleId = async (roleId) => {
    let url = dimRoleUrl + '/' + roleId
    let response = await api.Get(url)
    let role = {
        ROLE_ID: "",
        ROLE_NAME: "",
        RIGHTS: []
    }
    //取得功能列表ByRight_ID
    let rights = await getDimRightByRoleId(roleId);
    if (response.ok) {
        let data = await response.json()
        role = {
            ROLE_ID: data.ROLE_ID,
            ROLE_NAME: data.ROLE_NAME,
            RIGHTS: rights
        }
    }

    return role
}

//取得權限清單ByROLE_ID
const getDimRightByRoleId = async (roleId) => {
    let url = dimRightUrl + '/ReadByRole/' + roleId
    let response = await api.Get(url)
    let rights = []
    if (response.ok) {
        rights = dualListDataGetValue(await response.json())
    }

    return rights
}

//重新組DualListBox顯示的資料
const dualListData = (items) => {
    return items.map(item => {
        const result = { label: item.RIGHT_ID + '-' + item.RIGHT_NAME, value: item.RIGHT_ID }
        return result;
    });
}
//DualListBox只取得value
const dualListDataGetValue = (responseData) => {
    let data = [];
    responseData.map(item => {
        data.push(item.RIGHT_ID)
        return data;
    });

    return data;
}

const DimRoleService = {
    getDimRole: getDimRole,
    insertDimRole: insertDimRole,
    updateDimRole: updateDimRole,
    deleteDimRole: deleteDimRole,
    getDimRight: getDimRight,
    getDimRoleByRoleId: getDimRoleByRoleId,
    getDimRightByRoleId: getDimRightByRoleId
}

export default DimRoleService;
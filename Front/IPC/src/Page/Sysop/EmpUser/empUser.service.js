import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { AddNoColumn } from '../../../Basic/SDOExtension';

const empUserUrl = ServerConfig.backEndUrl + 'EmpUser';
const dimRoleUrl = ServerConfig.backEndUrl + 'DimRole';
const empOrgUrl = ServerConfig.backEndUrl + 'EmpOrg';
const loginUrl = ServerConfig.backEndUrl + 'Login';

//取得使用者清單
const getEmpUser = async (data) => {
    let url = empUserUrl + "/Read"
    let response = await api.Post(url, JSON.stringify(data))
    let user = []
    if (response.ok) {
        user = AddNoColumn(await response.json())
    }

    return user
}

//新增使用者
const insertEmpUser = async (userData) => {
    //判斷是否更動密碼
    userData = checkPD(userData)
    let response = await api.Post(empUserUrl, JSON.stringify(userData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//修改使用者 
const updateEmpUser = async (userData) => {
    //判斷是否更動密碼
    userData = checkPD(userData)
    let response = await api.Put(empUserUrl, JSON.stringify(userData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//刪除使用者
const deleteEmpUser = async (userId, delReason) => {
    let url = empUserUrl + '/' + userId
    let requestForm = new FormData();
    requestForm.append('del_reason', delReason);
    let response = await api.Delete(url, requestForm, new Headers(), false)
    //let response = await api.Delete(url)
    return {
        ok: response.ok,
        result: await response.json()
    };
}

//取得使用者的log紀錄
const getLoginLog = async (userId) => {
    let url = loginUrl + "/GetLoginLog";
    let response = await api.Post(url, JSON.stringify(userId));

    let LoginLogListData = [];
    if (response.ok) {
        LoginLogListData = await response.json();
    }
    return LoginLogListData;
}

//取得使用者清單ByUserId
const getEmpUserByUserId = async (userId) => {
    let url = empUserUrl + '/' + userId
    let response = await api.Get(url)
    let user = {
        USER_ID: "",
        USER_NAME: "",
        USER_EMAIL: "",
        USER_PD: "",
        CONFIRM_USER_PD: "",
        ROLES: [],
        USER_TEL: "",
        ORG_ID: "",
        IS_SYSOPPMO: false,
        ORG_NAME: "",
        USER_TITLE: ""
    }
    //取得角色列表ByUserId
    let roles = await getDimRoleByUserId(userId);
    if (response.ok) {
        let data = await response.json()
        user = {
            USER_ID: data.USER_ID,
            USER_NAME: data.USER_NAME,
            USER_EMAIL: data.USER_EMAIL,
            USER_PD: "********", //修改時，不查出密碼,
            CONFIRM_USER_PD: "********",
            ROLES: roles,
            USER_TEL: data.USER_TEL,
            ORG_ID: data.ORG_ID,
            IS_SYSOPPMO: false,
            ORG_NAME: data.ORG_NAME,
            USER_TITLE: data.USER_TITLE
        }
    }
    return user
}

//取得角色列表ByUserId
const getDimRoleByUserId = async (userId) => {
    let url = dimRoleUrl + '/ReadByUser/' + userId
    let response = await api.Get(url)
    let role = []
    if (response.ok) {
        role = dualListDataGetValue(await response.json())
    }

    return role
}

//取得角色列表
const getDimRole = async () => {
    let url = dimRoleUrl + '/GetMgrRoles';
    let response = await api.Post(url);
    let role = [];
    if (response.ok) {
        role = dualListData(await response.json());
    }

    return role;
}

// 重新組DualListBox顯示的資料
const dualListData = (items) => {
    return items.map(item => {
        const result = { label: item.ROLE_ID + '-' + item.ROLE_NAME, value: item.ROLE_ID, selected: false }
        return result;
    });
}

// DualListBox只取得value
const dualListDataGetValue = (responseData) => {
    let data = [];
    responseData.map(item => {
        data.push(item.ROLE_ID)
        return data;
    });
    return data;
}

//判斷是否更動密碼
const checkPD = (data) => {
    if (data.USER_PD == "********" && data.CONFIRM_USER_PD == "********") {
        data.CONFIRM_USER_PD = ''
        data.USER_PD = ''
    }
    return data
}

const getOrg = async (orgId) => {
    let response = await api.Get(ServerConfig.backEndUrl + 'EmpOrg/GetParentOrg/' + orgId);
    let result = [];
    if (response.ok) {
        let data = await response.json();
        result = [{ ORG_ID: data.ORG_ID, ORG_DISPLAY: data.ORG_DISPLAY }]
    }
    return result;
}

const getOrgs = async () => {
    let url = ServerConfig.backEndUrl + 'EmpOrg/GetAllOrgs'
    let response = await api.Get(url)
    let data = await response.json()
    return data
}

const getOrgDDLData = async () => {
    let url = empOrgUrl + '/GetOrgList';
    let response = await api.Get(url, null, null, false);
    let result = [];
    let data = [];
    if (response.ok) {
        result = await response.json();
        result.map(item => { data.push({ text: item['ORG_NAME'], value: item['ORG_ID'] }) });
    }
    if (result.length > 1) {
        data.unshift({ value: "", text: "請選擇" });
    }
    return data;
}

const EmpUserService = {
    getEmpUser: getEmpUser,
    insertEmpUser: insertEmpUser,
    updateEmpUser: updateEmpUser,
    deleteEmpUser: deleteEmpUser,
    getDimRole: getDimRole,
    getEmpUserByUserId: getEmpUserByUserId,
    getOrg: getOrg,
    getOrgs: getOrgs,
    getOrgDDLData: getOrgDDLData,
    getLoginLog: getLoginLog
}

export default EmpUserService;
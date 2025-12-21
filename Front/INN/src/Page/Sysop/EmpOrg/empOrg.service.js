import { ServerConfig } from '../../../Basic/BasicData';
import { api } from '../../../Basic/ApiFetch';


const EmpOrgService = () => {
    //取得使用者清單
    const getEmpUsers = async () => {
        let response = await api.Get(ServerConfig.backEndUrl + 'EmpOrg/GetEmpUsers');
        let data = [];
        if (response.ok) {
            data = await response.json();
        }
        return data;
    }
    //取得單位的使用者清單
    const getEmpOrgUsers = async (orgId) => {
        let response = await api.Get(ServerConfig.backEndUrl + 'EmpOrg/GetOrgUsers/' + orgId);
        let data = [];
        if (response.ok) {
            data = await response.json();
        }
        return data;
    }
    //取得上層機關
    const getParentOrg = async (parentId) => {
        let response = await api.Get(ServerConfig.backEndUrl + 'EmpOrg/GetParentOrg/' + parentId);
        let data = [];
        if (response.ok) {
            data = await response.json();
        }
        return data;
    }
    //取得單位資料 by parentOrgId
    const getEmpOrg = async (parentOrgId = "") => {
        let response = await api.Get(ServerConfig.backEndUrl + 'EmpOrg/' + parentOrgId);
        let data = [];
        if (response.ok) {
            data = await response.json();
        }
        return data;
    }
    //執行刪除
    const deleteEmpOrgs = async empOrgs => {
        let response = await api.Delete(ServerConfig.backEndUrl + 'EmpOrg/' + empOrgs);
        return response;
    }
    //更新修改組織
    const updateEmpOrg = async data => {
        return await api.Put(ServerConfig.backEndUrl + 'EmpOrg', JSON.stringify(data));
    }
    //新增組織
    const insertEmpOrg = async data => {
        return await api.Post(ServerConfig.backEndUrl + 'EmpOrg', JSON.stringify(data));
    }

    return {
        getEmpUsers: getEmpUsers,
        getEmpOrgUsers: getEmpOrgUsers,
        getParentOrg: getParentOrg,
        getEmpOrg: getEmpOrg,
        deleteEmpOrgs: deleteEmpOrgs,
        updateEmpOrg: updateEmpOrg,
        insertEmpOrg: insertEmpOrg
    };
}

export default EmpOrgService;
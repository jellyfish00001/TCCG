import { ServerConfig, GetBasicData } from "../../../Basic/BasicData";
import { api } from "../../../Basic/ApiFetch";
import { AddNoColumn } from "../../../Basic/SDOExtension";


const EmpAgentService = () => {

    const getEmpAgent = async () => {
        let url = ServerConfig.backEndUrl + 'EmpAgent';
        let response = await api.Get(url);
        let data = [];
        if (response.ok)
            data = AddNoColumn(await response.json());
        return data;
    }

    const confirmDelete = async (sid) => {
        let url = ServerConfig.backEndUrl + 'EmpAgent/' + sid;
        let response = await api.Delete(url);
        return await response.json();
    }

    const getOrgUser = async () => {
        let data = {
            orgUsers: [],
            orgUsersLoaded: false
        }

        let url = ServerConfig.backEndUrl + 'EmpUser/ReadByOrg/' + await GetBasicData("orgId");
        let response = await api.Get(url);
        if (response.ok) {
            let result = await response.json();
            let currentUserId = await GetBasicData("userId");
            result = result.filter(user => user.USER_ID !== currentUserId);
            result.unshift({ USER_NAME: '請選擇...', USER_ID: '' });
            data = {
                orgUsers: result,
                orgUsersLoaded: true
            }
        }

        return data;
    }

    const createEmpAgent = async (data) => {
        let url = ServerConfig.backEndUrl + 'EmpAgent';
        let response = await api.Post(url, JSON.stringify(data));
        return response;
    }

    return {
        getEmpAgent: getEmpAgent,
        confirmDelete: confirmDelete,
        getOrgUser: getOrgUser,
        createEmpAgent: createEmpAgent
    }
}

export default EmpAgentService
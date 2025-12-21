import { api } from "../../../Basic/ApiFetch";
import { AddNoColumn } from "../../../Basic/SDOExtension";
import { getGlobalServerConfig } from '../../../Route/RootMiddleware'

const FlowRoleService = () => {
    const getFlowRoleData = async () => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'FlowRole';
        let response = await api.Get(url);
        let data = [];
        if (response.ok) {
            data = AddNoColumn(await response.json());
        }
        return data;
    }

    const deleteFlowRoleData = async (roleId) => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'FlowRole/' + roleId;
        return await api.Delete(url);
    }


    const getFlowRoleUserByRoleId = async (roleId) => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'FlowRole/ReadUsers/' + roleId;
        let response = await api.Get(url);
        return await response.json();
    }

    const getFlowRoleByRoleId = async (roleId) => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'FlowRole/' + roleId
        let response = await api.Get(url);
        return await response.json();
    }

    const createFlowRole = async (body) => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'FlowRole';
        let response = await api.Post(url, JSON.stringify(body));
        return response;
    }

    const updateFlowRole = async (body) => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'FlowRole';
        let response = await api.Put(url, JSON.stringify(body));
        return response;
    }

    return {
        getFlowRoleData: getFlowRoleData,
        deleteFlowRoleData: deleteFlowRoleData,
        getFlowRoleUserByRoleId: getFlowRoleUserByRoleId,
        getFlowRoleByRoleId: getFlowRoleByRoleId,
        createFlowRole: createFlowRole,
        updateFlowRole: updateFlowRole
    }
}

export default FlowRoleService;
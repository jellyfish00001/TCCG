import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'

const MailRoleService = () => {
    const mailRoleUrl = getGlobalServerConfig().backEndUrl.get() + 'MailRole/';
    const empOrgUrl = getGlobalServerConfig().backEndUrl.get() + 'EmpOrg/';

    const loadMailRole = async () => {
        let response = await api.Read(mailRoleUrl);
        let mailRoles = []
        if (response.ok)
            mailRoles = AddNoColumn(await response.json());
        
        return mailRoles;
    }

    const loadRoleById = async id => {
        let response = await api.Read(mailRoleUrl + id);
        let mailRole = {
            ROLE_ID: '',
            ROLE_NAME: '',
            DEL_FLG: false,
            USERS: []
        };
        
        if(response.ok){
            let role = await response.json();
            mailRole = {
                ...mailRole,
                ROLE_ID: role.ROLE_ID,
                ROLE_NAME: role.ROLE_NAME,
                DEL_FLG: role.DEL_FLG,

            }
        }

        return mailRole;
    } 

    const loadRoleUsersById = async id => {
        let url = mailRoleUrl + 'ReadUsers/' + id;
        let response = await api.Read(url);
        let user =[]

        if(response.ok)
            user = await response.json();
        
        return user;
    }

    const loadUserByOrgId = async (orgId) => {
        let url = empOrgUrl + 'GetOrgUsers/' + orgId;
        let response = await api.Read(url);
        let user =[]

        if(response.ok)
            user = await response.json();

        return user;
    }

    const loadMultiOrgs = async orgId => {
        let response = 
            await api.Post(
                empOrgUrl + 'GetMultiOrgs', 
                JSON.stringify(orgId)    
            )
            
        let orgData = [];
        if(response.ok)
            orgData = await response.json();
        
        return orgData;
    }

    const insertMailRole = async mailRole => {
        let response = await api.Insert(mailRoleUrl, JSON.stringify(mailRole));
        return {
            ok: response.ok,
            result: await response.json()
        }
    }

    const updateMailRole = async mailRole => {
        let response = await api.Update(mailRoleUrl, JSON.stringify(mailRole));
        return {
            ok: response.ok,
            result: await response.json()
        }
    }

    const deleteMailRole = async roleId => {
        let response = await api.Delete(mailRoleUrl + roleId);
        return {
            ok: response.ok,
            result: await response.json()
        }
    }

    return{
        loadMailRole: loadMailRole,
        loadRoleById: loadRoleById,
        loadRoleUsersById: loadRoleUsersById,
        loadUserByOrgId: loadUserByOrgId,
        loadMultiOrgs: loadMultiOrgs,
        insertMailRole: insertMailRole,
        updateMailRole: updateMailRole,
        deleteMailRole: deleteMailRole,
    }
}


export default MailRoleService;
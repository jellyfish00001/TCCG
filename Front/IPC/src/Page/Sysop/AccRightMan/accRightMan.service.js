import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware'

let accRightManUrl = getGlobalServerConfig().backEndUrl.get() + 'ScPolicy'

// 取得帳號權限管理
const getAccRightMan = async (policyId, policyCompId) => {
    let url = accRightManUrl + "/GetPolicy";
    let formData = new FormData();
    formData.append('policyId', policyId);
    formData.append('policyCompId', policyCompId);
    let response = await api.Post(url, formData, new Headers(), false);
    let accRightMan = []
    if (response.ok) {
        accRightMan = await response.json()
    }
    return accRightMan
}

// 帳號權限管理存檔
const updateAccRightMan = async (policyData) => {
    let response = await api.Put((accRightManUrl), JSON.stringify(policyData))
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

const AccRightManService = {
    getAccRightMan: getAccRightMan,
    updateAccRightMan: updateAccRightMan
}

export default AccRightManService;

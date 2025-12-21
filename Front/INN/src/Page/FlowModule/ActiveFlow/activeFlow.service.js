import { api } from "../../../Basic/ApiFetch";
import { AddNoColumn } from "../../../Basic/SDOExtension";
import { getGlobalServerConfig } from '../../../Route/RootMiddleware'

const ActiveFlowService = () => {

    // read activeFlow list
    const readActiveFlow = async flowCode => {
        let response = await api.Read(getGlobalServerConfig().backEndUrl.get() + 'ActiveFlow/' + flowCode);
        if (response.ok)
            return AddNoColumn(await response.json());
        else
            return response;
    }

    return {
        readActiveFlow: readActiveFlow
    }
}

export default ActiveFlowService;
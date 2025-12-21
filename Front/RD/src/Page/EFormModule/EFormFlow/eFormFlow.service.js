import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'


const EformFlowService = () => {
    const eformFlowUrl = getGlobalServerConfig().backEndUrl.get() + 'eForm/';
    const activeFlowUrl = getGlobalServerConfig().backEndUrl.get() + 'ActiveFlow/';

    const loadFlowList = async () => {
        let response = await api.Read(eformFlowUrl + 'ReadFlowList');
        let flowList = null;
        if (response.ok) {
            flowList = AddNoColumn(await response.json());
        }

        return flowList;
    }

    const setFlowAccept = async data => {
        let response = await api.Post(activeFlowUrl + 'SetFlowAccept', JSON.stringify(data));
        let rtnResult = await response.json();

        return rtnResult;
    }

    const setFlowReject = async data => {
        let response = await api.Post(activeFlowUrl + 'SetFlowReject', JSON.stringify(data));
        let rtnResult = await response.json();

        return rtnResult;
    }

    const setSubFlowActive = async data => {
        let response = await api.Post(activeFlowUrl + 'SetSubFlowActive', JSON.stringify(data));
        let rtnResult = await response.json();

        return rtnResult;
    }

    return (
        {
            loadFlowList: loadFlowList,
            setFlowAccept: setFlowAccept,
            setFlowReject: setFlowReject,
            setSubFlowActive: setSubFlowActive,
        }
    )
}

export default EformFlowService;
import { getGlobalServerConfig } from '../../../Route/RootMiddleware'
const { api } = require("../../../Basic/ApiFetch");
const { AddNoColumn } = require("../../../Basic/SDOExtension");


const FlowDetailService = () => {

    /* 讀取DimRole By UserId */
    const getDimRole = async userId => {
        let response = await api.Get(getGlobalServerConfig().backEndUrl.get() + 'DimRole/ReadByUser/' + userId);
        let data = [];
        if (response.ok) {
            data = await response.json();
        }
        return data;
    }

    /* 讀取流程資料 */
    const loadFlowData = async flowCode => {
        let response = await api.Get(getGlobalServerConfig().backEndUrl.get() + 'ActiveFlow/GetDetail/' + flowCode);
        let data = [];
        if (response.ok) {
            data = AddNoColumn(await response.json());
        }
        return data;
    }

    /* 讀取分會流程資料 */
    const loadSubflowData = async (flowCode, signedOdr) => {
        let response = await api.Get(getGlobalServerConfig().backEndUrl.get() + 'ActiveFlow/GetSubDetail/' + flowCode + '/' + signedOdr);
        let data = [];
        if (response.ok) {
            try {
                //回傳值type = string[]
                let dataStr = await response.json();
                for (let str of dataStr) {
                    //string parse to Json & 加入No.欄位
                    let d = AddNoColumn(await JSON.parse(str));
                    data.push(d);
                }
            }
            catch (e) {
                console.log("ErrorMessage" + e);
            }
        }
        return data;
    }

    /* 取消流程 */
    const deleteFlow = async flowCode => {
        let response = await api.Get(getGlobalServerConfig().backEndUrl.get() + 'ActiveFlow/CancelFlow/' + flowCode);
        let data = [];
        if (response.ok) {
            data = await response.json();
        }
        return data;
    }

    /* 重設流程 */
    const resetFlow = async flowCode => {
        let response = await api.Get(getGlobalServerConfig().backEndUrl.get() + 'ActiveFlow/ResetFlow/' + flowCode);
        let data = [];
        if (response.ok) {
            data = await response.json();
        }
        return data;
    }

    /* 取消分會流程 */
    const deleteSubflow = async subflowCode => {
        let response = await api.Get(getGlobalServerConfig().backEndUrl.get() + 'ActiveFlow/CancelSubflow/' + subflowCode);
        let data = await response.json();
        return data;
    }

    return {
        loadFlowData: loadFlowData,
        loadSubflowData: loadSubflowData,
        getDimRole: getDimRole,
        deleteFlow: deleteFlow,
        resetFlow: resetFlow,
        deleteSubflow: deleteSubflow
    }
}

export default FlowDetailService;
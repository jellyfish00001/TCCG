import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得實地查證
 * @param {*} projectNo 
 * @returns 
 */
const getProjectFactFinding = async (projectNo) => {
    let url = APIUrl + 'ProjectExecute/GetProjectFactFinding';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存實地查證
 * @param {*} model
 * @returns 
 */
const saveProjectFactFinding = async (models) => {
    let url = APIUrl + 'ProjectExecute/SaveProjectFactFinding';
    let response = await api.Post(url, JSON.stringify(models), null, false);
    let result = null;
    if (response.ok) {
        result = response.json();
    }
    return result;
}

const ProjectFillFieldService = {
    getProjectFactFinding: getProjectFactFinding,
    saveProjectFactFinding: saveProjectFactFinding,
}
export default ProjectFillFieldService;
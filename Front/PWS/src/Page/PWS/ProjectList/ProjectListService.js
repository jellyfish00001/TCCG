import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得計畫列表
 * @param {*} data 
 * @returns 
 */
export const getProjectList = async (data) => {
    let url = APIUrl + 'ListExec/GetPWSProjectList';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 刪除計畫
 * @param {*} PLANNO 
 * @returns 
 */
export const deleteProjectList = async (PLANNO) => {
    let url = APIUrl + 'ListExec/DeleteProjectList';
    let response = await api.Post(url, JSON.stringify(PLANNO), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 複製計畫
 * @param {*} PLANNO 
 * @returns 
 */
export const copyProject = async (PLANNO) => {
    let url = APIUrl + 'PlanBasicA/CopyProject';
    let response = await api.Post(url, JSON.stringify(PLANNO), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 查詢機關是否截止
 * @param {*} OU_ID 
 * @returns 
 */
export const getOrgDeadline = async (OU_ID) => {
    let url = APIUrl + 'ListExec/GetOrgDeadline';
    let response = await api.Post(url, JSON.stringify(OU_ID), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}


import { api } from '../../../Basic/ApiFetch';
import { GetSetParam } from '../../../Basic/CommonService';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 取得計畫檔案資料
 * @param {*} projectNo 
 * @param {*} fileUpSource 
 * @returns 
 */
const getProjectAttachment = async (projectNo, fileUpSource) => {
    let url = APIUrl + 'ProjectCommon/GetProjectAttachment';
    let data = {
        PROJECT_NO: projectNo,
        FILE_UP_SOURCE: fileUpSource
    };
    let response = await api.Post(url, JSON.stringify(data), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 取得非相關檔案上傳的檔案資料
 * @param {*} projectNo 
 * @returns 
 */
const getProjectOtherAttachment = async (projectNo) => {
    let url = APIUrl + 'ProjectCommon/GetProjectOtherAttachmentList';
    let response = await api.Post(url, JSON.stringify(projectNo), null, false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

/**
 * 儲存計畫檔案資料
 * @param {*} models 
 * @returns 
 */
const saveProjectAttachment = async (models) => {
    let url = APIUrl + 'ProjectCommon/SaveProjectAttachment';
    let response = await api.Post(url, JSON.stringify(models), null, false);
    let result = null;
    if (response) {
        result = response.json();
    }
    return result;
}

/**
 * 取得計畫檔案類型
 * @returns 
 */
const getFileKindDropDown = async () => {
    let result = await GetSetParam("FILE_KIND");
    result = result.filter(x => x.SET_TYPE == "01" || x.SET_TYPE == "03" ||
        x.SET_TYPE == "04" || x.SET_TYPE == "14" || x.SET_TYPE == "15" || x.SET_TYPE == "20");
    result.unshift({ SET_TYPE: "", SET_VALUE: "請選擇" });
    return result;
}

// 欄位驗證
const validateField = Yup.object().shape({
    FILE_KIND: Yup.string().required("此欄位為必填"),
    FILE_MEMO: Yup.string().required("此欄位為必填").nullable(),
});

const ProjectFillFileUpService = {
    getProjectAttachment: getProjectAttachment,
    getProjectOtherAttachment: getProjectOtherAttachment,
    saveProjectAttachment: saveProjectAttachment,
    getFileKindDropDown: getFileKindDropDown,
    validateField: validateField,
}

export default ProjectFillFileUpService;
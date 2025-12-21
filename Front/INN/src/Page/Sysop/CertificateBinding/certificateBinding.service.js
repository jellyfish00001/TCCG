import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'

const tokenUrl = getGlobalServerConfig().backEndUrl.get() + 'Token';
const empUserUrl = getGlobalServerConfig().backEndUrl.get() + 'EmpUser'

//讀取Certificate Grid資料
const loadCertificate = async () => {
    let url = tokenUrl + '/GetTokenByType/H'
    let response = await api.Read(url);
    let certificates = []

    if (response.ok)
        certificates = AddNoColumn(await response.json());
    
    return certificates;
}

//讀取使用者下拉選單資料
const loadUserList = async () => {
    let response = await api.Read(empUserUrl);
    let userListData = {
        data: [], //下拉選單資料
        dataOri: [], //使用者資料 用於過濾並給下拉選單顯示
        loaded: false, //下拉選單loading狀態
    }
    
    if (response.ok){
        let data = await response.json();
        userListData.dataOri = data;
        userListData.data = data;
        userListData.loaded = true;
    }

    return userListData;
}

//讀取Certificate資料
const loadCertificateById = async id => {
    let url = tokenUrl + '/' + id;
    let response = await api.Read(url);
    let token = {
        TOKEN_ID: null,
        TOKEN: null,
        TOKEN_TYPE: 'H',
        USER_ID: null,
        USER_TYPE: 'USR',
        EXPIRE_DATE: new Date(),
        DEL_FLG: false,
    };

    if(response.ok){ 
        let data = await response.json();
        token = {
            TOKEN_ID: data.TOKEN_ID,
            TOKEN: data.TOKEN,
            TOKEN_TYPE: data.TOKEN_TYPE,
            USER_ID: data.USER_ID,
            USER_TYPE: data.USER_TYPE,
            EXPIRE_DATE: data.EXPIRE_DATE,
            DEL_FLG: data.DEL_FLG,
        }
    }

    return token;
}

//新增Certificate
const InsertCertificate = async certificate => {
    let response = await api.Insert(tokenUrl,JSON.stringify(certificate));
    return {
        ok: response.ok,
        result: await response.json()
    };
}

//修改Certificate
const UpdateCertificate = async certificate => {
    let response = await api.Update(tokenUrl,JSON.stringify(certificate));
    return {
        ok: response.ok,
        result: await response.json()
    };
}

const certificateBindingService = {
    loadCertificate: loadCertificate,
    loadUserList: loadUserList,
    loadCertificateById: loadCertificateById,
    InsertCertificate: InsertCertificate,
    UpdateCertificate: UpdateCertificate
}

export default certificateBindingService;
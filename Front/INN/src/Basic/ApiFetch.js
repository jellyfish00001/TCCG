import { GetHistory } from './BasicData';
import Cookies from 'universal-cookie';
import CacheLoader from './CacheLoader';
import { IsNullOrEmpty, SetMaskOnOff } from './SDOExtension';
import { showGlobalMessageBox, getGlobalServerConfig, setGlobalCountDown } from '../Route/RootMiddleware';

const queue = []
let handlerWorking = false;

export const makeRequest = (url, method, headers = null, body = null) => {
    return new Request(url, {
        method: method,
        headers: headers ? headers : new Headers(),
        body: body
    })
}

/**
 * 利用url及headers組出轉介所需的表頭
 * @param {*} url 要從轉介api導過去的實際位置
 * @param {*} headers 
 * @returns 轉介api所需表頭
 */
const getBridgeHeader = (url, headers) => {
    let Bridge_headers = new Headers();

    Bridge_headers.append('RouteUrl', url);
    if (headers && headers.has('Content-Type'))
        Bridge_headers.append('Content-Type', headers.get('Content-Type'));

    return Bridge_headers;
}

let Bridge_URL = getGlobalServerConfig().Bridge_backEndUrl.get();
const Get = (url, body = null, headers = null, mask = true) => {
    return apiFetch(makeRequest(url, 'GET', headers, body), headers != null, mask);
}

const Post = (url, body = null, headers = null, mask = true) => {
    return apiFetch(makeRequest(url, 'POST', headers, body), headers != null, mask);
}

const Put = (url, body = null, headers = null, mask) => {
    return apiFetch(makeRequest(url, 'PUT', headers, body), headers != null, mask);
}

const Delete = (url, body = null, headers = null, mask) => {
    return apiFetch(makeRequest(url, 'DELETE', headers, body), headers != null, mask);
}

const BGet = (url, body = null, headers = null, mask = true) => {
    return apiFetch(makeRequest(Bridge_URL, 'GET', getBridgeHeader(url, headers), body), headers != null, mask);
}

const BPost = (url, body = null, headers = null, mask = true) => {
    return apiFetch(makeRequest(Bridge_URL, 'POST', getBridgeHeader(url, headers), body), headers != null, mask);
}

const BPut = (url, body = null, headers = null, mask) => {
    return apiFetch(makeRequest(Bridge_URL, 'PUT', getBridgeHeader(url, headers), body), headers != null, mask);
}

const BDelete = (url, body = null, headers = null, mask) => {
    return apiFetch(makeRequest(Bridge_URL, 'DELETE', getBridgeHeader(url, headers), body), headers != null, mask);
}

export const api = {
    Read: Get,
    Get: Get,
    Insert: Post,
    Post: Post,
    Update: Put,
    Put: Put,
    Delete: Delete,
    Fetch: apiFetch,
}
/*
  bridge Apiset
*/
export const apiBridge = {
    Read: BGet,
    Get: BGet,
    Insert: BPost,
    Post: BPost,
    Update: BPut,
    Put: BPut,
    Delete: BDelete
}

/**
 * @function apiFetch
 * @async
 * @param {any} request Request內容
 * @param {boolean} [customContentType] 是否客製ContentType，false則自動設為json
 * @param {boolean} [mask] 是否使用遮罩
 * @returns {Promise<any>} Response結果
 * @description 傳入Request內容呼叫Api
 */
export async function apiFetch(request, customContentType = false, mask = true) {
    let response;
    if (mask)//顯示遮罩
        await SetMaskOnOff(true);

    

    //執行fetch
    response = await fetching(request, customContentType).then(handleErrors)
        .catch(
            error => {
                showGlobalMessageBox("發生伺服器端錯誤，請聯繫管理員。");
            }
        );
    if (response && response.ok) {

        setGlobalCountDown(getGlobalServerConfig().cacheExpire.get());
        //resolve
    }
    else {

        console.warn(response);
        //reject
    }
    if (mask)//關閉遮罩
        await SetMaskOnOff(false);
    return response
}


/**
 * @function addFetch
 * @async
 * @param {any} request Request內容
 * @param {boolean} [customContentType] 是否客製ContentType，false則自動設為json
 * @param {boolean} [mask] 是否使用遮罩
 * @returns {Promise<any>} Response結果
 * @description 將Request加入Queue並回傳Promise
 */
async function addFetch(request, customContentType, mask) {
    //製作Promise
    let promise = new Promise((resolve, reject) => {
        let requestItem = {
            request: request,
            customContentType: customContentType,
            mask: mask,
            resolve: resolve,//保存Promise的resolve,reject供handler改變Promise狀態
            reject: reject,
        }
        //將各項參數加入Queue中
        queue.push(requestItem);
    });

    //啟動Handler
    if (!handlerWorking)
        fetchQueueHandler();

    return promise;
}

//處理Request
async function fetchQueueHandler() {
    handlerWorking = true;
    //執行至Queue為空
    while (queue.length > 0) {
        //從列隊開頭取出(先進先出)
        let requestItem = queue.shift();
        if (requestItem.mask)//顯示遮罩
            SetMaskOnOff(true);
        //執行fetch
        let response = await fetching(requestItem.request, requestItem.customContentType);
        if (response.ok) {
            //resolve
            requestItem.resolve(response);
        }
        else {
            if (response.status === 401 && !IsNullOrEmpty(GetHistory())) {
                CacheLoader().DeleteCache();
                GetHistory().push('/');
            }
            //reject
            requestItem.reject(response);
        }
        //關閉遮罩
        SetMaskOnOff(false);
    }
    handlerWorking = false;
}

//發送fetch
/**
 * @function addFetch
 * @async
 * @param {*} request 
 * @param {boolean} customContentType 是否使用自定義ContentType
 * @returns 
 * @description 傳入Request內容呼叫Api
 */
export async function fetching(request, customContentType) {
    //讀取Cookie時間
    await CheckCacheSetting();

    //讀取驗證token
    await LoadCache();

    //設定驗證header
    if (request.headers.has('authorization'))
        request.headers.set('authorization', getGlobalServerConfig().BasicData.token.get());
    else
        request.headers.append('authorization', getGlobalServerConfig().BasicData.token.get());

    //設定contentType
    if (!customContentType)//若不客製則設定
    {
        request.headers.set('Content-Type', 'application/json');
    }

    //設定CacheToken供後端同步Cache資料
    let cookie = new Cookies();
    let cacheToken = cookie.get('cacheToken')
    if (!IsNullOrEmpty(cacheToken))
        request.headers.set('CacheToken', cacheToken);

    //fetch
    let response = await fetch(request)
        .then(handleErrors)
        .catch(
            error => {
                showGlobalMessageBox("發生伺服器端錯誤，請聯繫管理員。");
            }
        );



    //更新驗證token
    if (response && !IsNullOrEmpty(response.headers.get('authorization'))) {
        getGlobalServerConfig().BasicData.token.set(response.headers.get('authorization'));

    }

    //更新cookies有效期限
    if (response && !IsNullOrEmpty(response.headers.get('cacheToken'))) {
        CacheLoader().SetCache(response.headers.get('cacheToken'));
    }

    return response
}

export const handleErrors = (response) => {
    if (response && response.status === 401) {
        CacheLoader().DeleteCache();
        showGlobalMessageBox("登入資訊不存在或已過期，請重新登入!", () => { GetHistory().push('/'); });
        return response;
    }
    if (!response.ok) {
        if (response.status === 401) {
            CacheLoader().DeleteCache();
            GetHistory().push('/');
        }
        throw new Error(response.status);
    };
    return response;
}

async function CheckCacheSetting() {
    // if (!ServerConfig.cacheSettingLoaded)
    if (!getGlobalServerConfig().cacheSettingLoaded.get())
        await CacheLoader().LoadCacheExpire();
}

async function LoadCache() {
    if (IsNullOrEmpty(getGlobalServerConfig().BasicData.token.get()) && CacheLoader().HasCache())
        await CacheLoader().LoadCache();
}
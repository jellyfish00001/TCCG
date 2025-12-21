import { getCookieSetOptions } from './BasicData';
import Cookies from 'universal-cookie';
import { IsNullOrEmpty } from './SDOExtension';
import { showGlobalMessageBox, getGlobalServerConfig } from '../Route/RootMiddleware';
import { handleErrors } from './ApiFetch';

const CacheLoader = () => {
    const HasCache = () => {
        let cookies = new Cookies().getAll();
        return cookies.hasOwnProperty('loginCache');
    }

    //讀取並寫入Basic.tokne
    //return:讀取是否成功
    const LoadCache = async () => {

        if (!HasCache())
            return false

        let headers = new Headers();
        headers.set("loginCache", new Cookies().get('loginCache'));
        headers.append('content-type', 'application/json')
        let request = new Request(getGlobalServerConfig().backEndUrl.get() + 'Login', {
            headers: headers,
            credentials: 'include'
        })

        let response = await fetch(request);
        if (!response.ok)
            return false;

        if (!IsNullOrEmpty(response.headers.get('loginCache'))) {
            SetCache(response.headers.get('loginCache'));
        }

        let loginToken = await response.text();

        if (loginToken === '')
            return false;
        getGlobalServerConfig().BasicData.token.set(loginToken);

        return true;
    }

    /**
     * 讀取Cache過期時間
     * @returns 
     */
    const LoadCacheExpire = async () => {
        let headers = new Headers();
        headers.append('content-type', 'application/json')
        let request = new Request(getGlobalServerConfig().backEndUrl.get() + 'SystemInfo/GetCacheExpireTime', {
            headers: headers
        })
        let response = await fetch(request)
            .then(handleErrors)
            .catch(
                error => {
                    showGlobalMessageBox("發生伺服器端錯誤，請聯繫管理員。");
                }
            );

        if (!response || !response.ok)
            return false;

        let cacheExpire = await response.text();

        if (cacheExpire === '')
            return false;

        getGlobalServerConfig().cacheSettingLoaded.set(true);
        getGlobalServerConfig().cacheExpire.set(parseInt(cacheExpire));
    }

    /**
     * 設定Cache
     * @param {*} loginCache
     */
    const SetCache = (loginCache) => {
        let expires = new Date(Date.now() + (getGlobalServerConfig().cacheExpire.get() * 1000))
        localStorage.setItem('timeOut', expires);

        new Cookies().set('loginCache', loginCache, getCookieSetOptions(process.env.REACT_APP_COOKIE_PATH, expires));
    }

    /**
     * 取得登入狀態
     * @returns 
     */
    const GetCache = () => {
        return !HasCache() ? '' : new Cookies().get('loginCache');
    }

    /**
     * 刪除Cache
     */
    const DeleteCache = async () => {
        const loginCache = new Cookies().get('loginCache');

        if (loginCache) {
            let url = getGlobalServerConfig().backEndUrl.get() + 'login/DelCache';
            // request options
            const options = {
                method: 'POST',
                credentials: 'include',
                body: JSON.stringify(loginCache),
                headers: {
                    'Content-Type': 'application/json',
                }
            }

            localStorage.removeItem('timeOut');
            new Cookies().set('loginCache', '', getCookieSetOptions(process.env.REACT_APP_COOKIE_PATH, new Date(Date.now() - 1)));

            // send POST request
            await fetch(url, options)
        }
    }

    return {
        HasCache: HasCache,
        LoadCache: LoadCache,
        LoadCacheExpire: LoadCacheExpire,
        DeleteCache: DeleteCache,
        SetCache: SetCache,
        GetCache: GetCache
    }
}

export default CacheLoader; 
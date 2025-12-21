import { getCookieSetOptions } from './BasicData';
import Cookies from 'universal-cookie';
import { IsNullOrEmpty } from './SDOExtension';
import { showGlobalMessageBox, getGlobalServerConfig } from '../Route/RootMiddleware';
import { handleErrors } from './ApiFetch';

const CacheLoader = () => {
    const HasCache = () => {
        let cookies = new Cookies().getAll();
        return cookies.hasOwnProperty('cacheToken');
    }

    //讀取並寫入Basic.tokne
    //return:讀取是否成功
    const LoadCache = async () => {

        if (!HasCache())
            return false


        let headers = new Headers();
        headers.set("cacheToken", new Cookies().get('cacheToken'));
        headers.append('content-type', 'application/json')
        let request = new Request(getGlobalServerConfig().backEndUrl.get() + 'Login', {
            headers: headers
        })


        let response = await fetch(request);
        if (!response.ok)
            return false;

        if (!IsNullOrEmpty(response.headers.get('cacheToken'))) {
            SetCache(response.headers.get('cacheToken'));
        }

        let loginToken = await response.text();

        if (loginToken === '')
            return false;
        getGlobalServerConfig().BasicData.token.set(loginToken);

        return true;
    }
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

    const SetCache = (cacheToken) => {

        let expires = new Date(Date.now() + (getGlobalServerConfig().cacheExpire.get() * 1000))
        localStorage.setItem('expires', expires);

        // @ts-ignore
        new Cookies().set('cacheToken', cacheToken, getCookieSetOptions(process.env.REACT_APP_COOKIE_PATH, expires));
    }

    const GetCache = () => {
        return !HasCache() ? '' : new Cookies().get('cacheToken');
    }

    const DeleteCache = async () => {

        let cacheToken = new Cookies().get('cacheToken');
        if (cacheToken) {
            let url = getGlobalServerConfig().backEndUrl.get() + 'login/DelCache';
            // request options
            const options = {
                method: 'POST',
                body: JSON.stringify(new Cookies().get('cacheToken')),
                headers: {
                    'Content-Type': 'application/json',
                }
            }

            localStorage.removeItem('expires');
            new Cookies().set('cacheToken', '', getCookieSetOptions(process.env.REACT_APP_COOKIE_PATH, new Date(Date.now() - 1)));

            // send POST request
            await fetch(url, options)
        }

    }

    /**
     * 更新cookies有效期限
     */
    const RefreshCacheExpire = async () => {
        let cache = await GetCache();
        if (!IsNullOrEmpty(cache)) {
            //更新cookies有效期限
            SetCache(cache)
        }
    }

    return {
        HasCache: HasCache,
        LoadCache: LoadCache,
        LoadCacheExpire: LoadCacheExpire,
        SetCache: SetCache,
        GetCache: GetCache,
        DeleteCache: DeleteCache,
        RefreshCacheExpire: RefreshCacheExpire
    }
}

export default CacheLoader;
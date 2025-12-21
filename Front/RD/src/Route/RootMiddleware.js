/**
 * @callback requestCallback
 */
import { GlobalMessageBox, messageSettings } from '../Components/Dialogs/GlobalMessageBox';
import { GlobalConfirmBox, ConfirmSettings } from '../Components/Dialogs/GlobalConfirmBox';
import { createState } from '@hookstate/core';
import { getThemeConfig } from '../Basic/ThemeConfig';
import Cookies from 'universal-cookie';
import { getCookieSetOptions } from '../Basic/BasicData';
const ServerConfig = {
    Me: "PMO",
    backEndUrl: process.env.REACT_APP_API_URL,
    Bridge_backEndUrl: process.env.REACT_APP_Bridge_API_URL,
    /**@type {boolean} */
    cacheSettingLoaded: false,
    /**@type {number} */
    cacheExpire: 0,
    /**@type {number} */
    countDown: 0,
    /**@type {any} */
    BasicData: {
        token: '',
        userId: '',
        userName: '',
        orgId: '',
        orgName: '',
        email: '',
        userType: '',
        roles: [],
        allRoles: []
    },
    defaultPageSize: 20
}

const _globalState = {
    globalMessageBoxSettings: messageSettings,
    globalConfirmBoxSettings: ConfirmSettings,
    globalServerConfig: ServerConfig
};

/**
 * 利用hookstate套件管理globaleState 
*/
export const globalState = createState(_globalState);

/**
 * @function showGlobalMessageBox
 * @param {string} message 訊息內容
 * @param {requestCallback} [callback] 按下確認後回呼的事件
 * @description 利用globalState顯示全域訊息框
 * @example
 * <caption>使用方式1:</caption>
 * let message="訊息";
 * showGlobalMessageBox(message)
 * @example
 * <caption>使用方式2:</caption>
 * let message="訊息";
 * let callback=()=>{};
 * showGlobalMessageBox(message)
 * @see globalState 參閱globalState
 * @author Peter.Pang <peter_pang@gss.com.tw>
 */
export const showGlobalMessageBox = (message, callback) => {
    globalState.globalMessageBoxSettings.visible.set(true);
    globalState.globalMessageBoxSettings.message.set(message);
    //每次進入都先清除原function
    if (typeof (callback) == "function")
        globalState.globalMessageBoxSettings.onOkAction.set(() => callback);
    else
        globalState.globalMessageBoxSettings.onOkAction.set(() => { return () => { } });//利用空的function覆蓋

}

/**
 * @function showGlobalConfirmBox
 * @param {string} message 訊息內容
 * @param {requestCallback|Promise<any>} [OkActionCallback] 按下確認後回呼的事件
 * @param {requestCallback} [OkCancelCallback] 按下取消後回呼的事件
 * @description 利用globalState顯示全域確認框
 * @example
 * <caption>使用方式1:</caption>
 * let message="訊息";
 * showGlobalConfirmBox(message)
 * @example
 * <caption>使用方式2:</caption>
 * let message="訊息";
 * let OkActionCallback=()=>{};
 * let OkCancelCallback=()=>{};
 * showGlobalConfirmBox(message,OkActionCallback,OkCancelCallback)
 * @see globalState 參閱globalState
 * @author Peter.Pang <peter_pang@gss.com.tw>
 */
export const showGlobalConfirmBox = (message, OkActionCallback, OkCancelCallback) => {
    globalState.globalConfirmBoxSettings.visible.set(true);
    globalState.globalConfirmBoxSettings.message.set(message);
    if (typeof (OkActionCallback) == "function")
        globalState.globalConfirmBoxSettings.onOkAction.set(() => OkActionCallback);
    if (typeof (OkCancelCallback) == "function")
        globalState.globalConfirmBoxSettings.onCancelAction.set(() => OkCancelCallback);
}


export const setGlobalCountDown = (c) => {
    globalState.globalServerConfig.countDown.set(c);
}


export const getGlobalServerConfig = () => {
    return globalState.globalServerConfig;
}

export const resetGlobalServerConfig = () => {
    globalState.globalServerConfig.cacheSettingLoaded.set(false);
    globalState.globalServerConfig.BasicData.set({
        token: '',
        userId: '',
        userName: '',
        orgId: '',
        orgName: '',
        email: '',
        userType: '',
        roles: [],
        allRoles: []
    });
}

/**
 * 切換網站樣式
 * @param {string} themeName 
 */
export const setTheme = (themeName) => {
    let themeConfig = getThemeConfig();

    themeConfig.forEach(t => {
        document.documentElement.style.setProperty(`--selected-${t}`, `var(--${themeName}-${t})`);
    });
    SetThemeCookie(themeName);
}

export const getThemeCookie = () => {
    let cookies = new Cookies().getAll();
    if (cookies.hasOwnProperty('theme'))
        return new Cookies().get('theme');

    return null;
}

const SetThemeCookie = (themeName) => {

    let cookieExpireDate = new Date();
    cookieExpireDate.setDate(cookieExpireDate.getDate() + 30);
    new Cookies().set('theme', themeName, getCookieSetOptions(process.env.REACT_APP_COOKIE_PATH, cookieExpireDate));
}

export { GlobalMessageBox, GlobalConfirmBox }
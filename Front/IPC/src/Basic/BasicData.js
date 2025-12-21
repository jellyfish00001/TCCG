//*****共用的基本資料*****

/**
 * @typedef  {{
 *          token:string,
 *          userId:string,
 *          orgId:string,
 *          email:string
 *       }} BasicData
 * */

import React, { createContext, useState } from 'react';
import { IsNullOrEmpty } from './SDOExtension';
import { api } from './ApiFetch';
import CacheLoader from '../Basic/CacheLoader';
import { getGlobalServerConfig } from '../Route/RootMiddleware';
import menuIcon01 from '../Images/menu_icon01.png';
import menuIcon02 from '../Images/menu_icon02.png';
import menuIcon03 from '../Images/menu_icon03.png';
import menuIcon04 from '../Images/menu_icon04.png';
import menuIcon05 from '../Images/menu_icon05.png';
import menuIcon06 from '../Images/menu_icon06.png';

/**@type {*} */
let basicData = getGlobalServerConfig().BasicData;

/**
 * @function GetBasicData
 * @returns {Promise<object>}
 * @description 根據Key取得BasicData對應的資料
 */
export const GetBasicData = async (key) => {
    if (IsNullOrEmpty(basicData[key].get())) {
        await loadBasicData();
    }
    return basicData[key].get();
}

/**
 * @function loadBasicData
 * @description 透過api取得BasicData的值
 */
export const loadBasicData = async () => {
    let response = await api.Get(getGlobalServerConfig().backEndUrl.get() + 'Login/LoadBasic', null, null, false);
    if (response.ok) {
        let result = await response.json();
        basicData.userId.set(result.USER_ID);
        basicData.orgId.set(result.ORG_ID);
        basicData.email.set(result.EMAIL);
        basicData.roles.set(result.ROLES);
        basicData.userType.set(result.USR_TYPE);
        basicData.allRoles.set(result.AllROLES);
    }
}

/**
 * @description 供全域使用的參數
 */
export const ServerConfig = {
    // @ts-ignore
    backEndUrl: process.env.REACT_APP_API_URL,
    /**@type {boolean} */
    cacheSettingLoaded: false,
    /**@type {number} */
    cacheExpire: 0,
    pagesize: 20
}

const Route = {
    history: null
}

export const SetHistory = history => {
    Route.history = history
}

export const GetHistory = () => {
    return Route.history;
}

export const Pageable = {
    buttonCount: 5,
    info: true,
    /**@type {object} */
    type: 'numeric',
    // 若需客製page下拉選單，請提供array，反之:設定為true
    pageSizes: [5, 10, 20, 50, 100],//true,
    previousNext: true
}

export const defaultFormat = 'tYY/MM/DD'

export const defaultSignInStatusSettings = {
    signInStatus: false,
    setSignInStatus: () => { },
}

export const SignInStatusContext = createContext(defaultSignInStatusSettings);
export const SignInStatusProvider = props => {
    const [signInStatus, setVisible] = useState(defaultSignInStatusSettings.signInStatus);

    const setSignInStatus = async () => {
        setVisible(await CacheLoader().LoadCache());
    };

    return (
        <SignInStatusContext.Provider value={{ signInStatus, setSignInStatus }}>
            {props.children}
        </SignInStatusContext.Provider>
    )
}

//#region 是否有頁首頁尾
export const defaultHeaderFooterStatusSettings = {
    headerFooterStatus: false,
    setHeaderFooterStatus: (e) => { },
}
export const HeaderFooterContext = createContext(defaultHeaderFooterStatusSettings);
export const HeaderFooterStatusProvider = props => {
    const [headerFooterStatus, setVisible] = useState(defaultHeaderFooterStatusSettings.headerFooterStatus);

    const setHeaderFooterStatus = async (value) => {
        setVisible(value);
    };

    return (
        <HeaderFooterContext.Provider value={{ headerFooterStatus: headerFooterStatus, setHeaderFooterStatus: setHeaderFooterStatus }}>
            {props.children}
        </HeaderFooterContext.Provider>
    )
}
//#endregion 

export const getCookieSetOptions = (path, expires) => {
    let cookieOptions = {
        path: path,
        expires: expires,
        secure: process.env.REACT_APP_COOKIE_SECURE === 'true' ? true : false
    }
    // new Date(Date.now() - 1)

    return cookieOptions;
}

/**
 * menuIcons清單
 * @returns 
 */
export const menuIcons =
{
    "PLAN": menuIcon01,      // 計畫填報
    "REVIEW": menuIcon02,    // 計畫審查
    "SEARCH": menuIcon03,    // 查詢
    "DECISION": menuIcon04,  // 決策分析
    "SETTING": menuIcon05,   // 設定
    "DASHBOARD": menuIcon06, // 儀表板
};

/**
 * True or False
 */
export const withOrWithoutData = [
    {
        label: "無",
        value: false
    },
    {
        label: "有",
        value: true
    }
]
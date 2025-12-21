/**
 * @typedef {{
 *          logic:Object,
 *          filters:Array.<{
 *                          field:string,
 *                          operator:string|function,
 *                          value?: any,
 *                          ignoreCase?:boolean
 *                        }>
 *       }} filter
 */

import React from 'react';
import logo from '../Images/logo.png';
import logo2 from '../Images/logo2.png';
import { useTranslation } from 'react-i18next';
import { api } from '../Basic/ApiFetch';
import CacheLoader from '../Basic/CacheLoader';
import { HeaderFooterContext, GetHistory, SignInStatusContext } from '../Basic/BasicData';
import { IsNullOrEmpty } from '../Basic/SDOExtension';
import { showGlobalConfirmBox, getGlobalServerConfig, globalState, resetGlobalServerConfig } from '../Route/RootMiddleware';
import { Countdown } from './CountDown';
import { Button } from '@progress/kendo-react-buttons';

const Head = (props) => {
    const { signInStatus, setSignInStatus } = React.useContext(SignInStatusContext);
    const { headerFooterStatus, setHeaderFooterStatus } = React.useContext(HeaderFooterContext);
    const [userAgentDDLList, setUserAgentDDLList] = React.useState({
        data: [], //下拉選單資料
        dataOri: [] //使用者資料 用於過濾並給下拉選單顯示
    })

    const [userAgent, setUserAgent] = React.useState(null)

    const { t } = useTranslation();
    const languageDDLList = [
        { text: "繁體中文", value: "zh-TW" },
        { text: "English", value: "en" }
    ];
    const RootHistory = GetHistory();

    /**
     * @function isSignOut
     * @description 呼叫showConfirmBox來確認是否登出
     */
    const isSignOut = () => {
        showGlobalConfirmBox("確定要登出系統嗎？", signOut);
    }

    React.useEffect(() => {
        if (signInStatus && !headerFooterStatus) {
            getUserAgent()
        }
    }, [signInStatus])

    /**
     * @function getUserAgent
     * @description 取得登入者還可以代理那些user
     */
    const getUserAgent = async () => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'EmpUser/ReadUserAgent';
        let response = await api.Get(url, null, null, false);
        if (response && response.ok) {
            let data = await response.json()
            setUserAgentDDLList({
                ...userAgentDDLList,
                data: data,
                dataOri: data
            });

            setUserAgent(data[0]);
            getGlobalServerConfig().BasicData.userName.set(data[0].USER_NAME);
            getGlobalServerConfig().BasicData.userId.set(data[0].USER_ID);
            getGlobalServerConfig().BasicData.orgId.set(data[0].ORG_ID);
            getGlobalServerConfig().BasicData.orgName.set(data[0].ORG_NAME);
            getGlobalServerConfig().BasicData.email.set(data[0].USER_EMAIL);
            getGlobalServerConfig().BasicData.userType.set(data[0].UsrType);
        }
    }

    // 登出
    const signOut = async () => {
        // 清除BasicData
        resetGlobalServerConfig();
        // 清除快取
        await CacheLoader().DeleteCache();
        // 導頁導到登入頁面
        if (IsNullOrEmpty(RootHistory)) {
            window.location.href = process.env.PUBLIC_URL
        }
        else {
            RootHistory.push('/');
        }
        // 紀錄目前登入狀態
        setSignInStatus();
    }

    const logoClick = () => {
        RootHistory
            ? RootHistory.location.pathname === "/SystemSelector"
                ? RootHistory.push('/SystemSelector')
                : RootHistory.push('/Home')
            : window.location.reload();
    }

    // 有登入時載入props
    const logoButtonProp = CacheLoader().HasCache() ? {
        className: "logoButton",
        onClick: logoClick
    } : {}

    return (
        <header style={headerFooterStatus ? { display: "none" } : {}}>
            {signInStatus && !headerFooterStatus &&
                <>
                    <span {...logoButtonProp}>
                        <img
                            title={t('common.title')}
                            alt="Background"
                            src={window.location.pathname.includes("/SystemSelector") || window.location.pathname.includes("/UserGuide") ? logo : logo2}
                        />
                    </span>
                    <ul className="userInfo">
                        <li>
                            {
                                userAgent &&
                                <div>
                                    {`${userAgent.UnitOuName}(${userAgent.OuName})：${userAgent.USER_NAME}`}
                                </div>
                            }
                        </li>
                        <li>
                            <Countdown ServerConfig={globalState.globalServerConfig} signOut={signOut} />
                        </li>
                        <li>
                            {/* <Button type="button" onClick={isSignOut}>登出</Button> */}
                        </li>
                    </ul>
                </>
            }
        </header>
    );
}
export default Head;
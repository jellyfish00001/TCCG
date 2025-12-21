//@ts-check
/**
 * @callback requestCallback
 */
import React, { useState, useEffect, Suspense } from 'react';
import { BrowserRouter, Route, Switch } from "react-router-dom";
import Login from '../HomePage/Login';
import Head from '../HomePage/Head';
import Foot from '../HomePage/Foot';
import HomeRoute from './HomeRoute';
import HomeRoutePanel from './HomeRoutePanel';

import ProjectChapterRoutePanel from '../Page/IPC/ProjectChapter/ProjectChapterRoutePanel';
import SystemSelector from '../HomePage/SystemSelector';
import ProjectPrint from '../Page/IPC/ProjectPrint/ProjectPrintMain';
import ProjectAdjustContent from '../Page/IPC/ProjectAdjustContent/ProjectAdjustContentMain';

import { MessageBox } from '../Components/Dialogs/MessageBox';
import { GlobalMessageBox, GlobalConfirmBox, globalState, setTheme, getThemeCookie } from './RootMiddleware'

import { ConfirmBox } from '../Components/Dialogs/ConfirmBox';
import Overlay from '../Components/Overlay/Overlay';
import ErrorBoundary from '../Basic/ErrorBoundary';
import { SignInStatusProvider, HeaderFooterContext } from '../Basic/BasicData';

/* kendo globalization tool */
import { LocalizationProvider, IntlProvider, load, loadMessages } from '@progress/kendo-react-intl';

/* cldr 語言包 */
import likelySubtags from 'cldr-core/supplemental/likelySubtags.json';
import currencyData from 'cldr-core/supplemental/currencyData.json';
import weekData from 'cldr-core/supplemental/weekData.json';

/* zh語系 語言包 */
import zhNumbers from 'cldr-numbers-full/main/zh-Hant/numbers.json';
import zhLocalCurrency from 'cldr-numbers-full/main/zh-Hant/currencies.json';
import zhCaGregorian from 'cldr-dates-full/main/zh-Hant/ca-gregorian.json';
import zhDateFields from 'cldr-dates-full/main/zh-Hant/dateFields.json';
import zhTimeZoneNames from 'cldr-dates-full/main/zh-Hant/timeZoneNames.json';

// kendo DOM 多國語言
import messages from '../i18n/kendoMessages';
// 多國語系i18N
import '../i18n/i18n.js';
import { useTranslation } from 'react-i18next';
import { Button } from '@progress/kendo-react-buttons';
import SSOLogin from '../HomePage/SSOLogin';
import SSOCloud from '../HomePage/SSOCloud';
import ResetCountDown from '../HomePage/ResetCountDown';
import { ChapterPageIndexProvider } from '../Page/IPC/ProjectChapter/ChapterPageIndexProvider';
import UserGuide from '../HomePage/UserGuide';

/**
 * Root元件
 *
 * @component
 * @example
 */
const RootRoute = (props) => {

    const { headerFooterStatus, setHeaderFooterStatus } = React.useContext(HeaderFooterContext);
    const [loading, setLoading] = useState(false);
    const { i18n } = useTranslation();
    const [i18nLanguage, seti18nLanguage] = useState("zh-TW");
    const [lang, setLang] = useState("zh-Hant");
    const [visible, setVisible] = useState(true);

    // 載入基本語言包
    // useEffect(() => {
    //載入必要的語言包 (預設已有en-US)
    load(
        likelySubtags,
        currencyData,
        weekData,
        zhLocalCurrency,
        zhNumbers,
        zhCaGregorian,
        zhDateFields,
        zhTimeZoneNames
    );
    //載入kendo元件語言包
    loadMessages(messages.zh, 'zh-Hant');
    // }, [])

    // 切換語系
    useEffect(() => {
        const loadCache = async () => {
            i18n.changeLanguage(i18nLanguage);
            setLang(i18nLanguage === 'zh-TW' ? 'zh-Hant' : i18nLanguage);
        }
        if (getThemeCookie() != null) {
            setTheme(getThemeCookie());
        } else {
            setTheme('blue');
        }

        loadCache();
    }, [i18nLanguage]);

    useEffect(() => {
        let href = window.location.href;
        setHeaderFooterStatus(href.includes("ProjectChapter") || href.includes("ProjectPrint") || href.includes("AdjustContent"));
        setLoading(true);
    }, [])

    const openMenu = () => {
        setVisible(!visible);
    }

    return (
        <LocalizationProvider language={lang}>
            <IntlProvider locale={lang}>
                <SignInStatusProvider>
                    <ChapterPageIndexProvider>
                        <Head value={i18nLanguage} setValue={(value) => seti18nLanguage(value)} />
                        <ConfirmBox />
                        <MessageBox />
                        <GlobalMessageBox stateConfig={globalState.globalMessageBoxSettings} />
                        <GlobalConfirmBox stateConfig={globalState.globalConfirmBoxSettings} />
                        <Overlay />
                        <ErrorBoundary>
                            <BrowserRouter basename={process.env.PUBLIC_URL}>
                                <Switch>
                                    <Route exact path="/" component={Login} />
                                    <Route exact path="/SystemSelector" component={SystemSelector} />
                                    <Route exact path="/SSOLogin" component={SSOLogin} />
                                    <Route exact path="/SSOCloud" component={SSOCloud} />
                                    <Route exact path="/ProjectPrint" component={ProjectPrint} />
                                    <Route exact path="/AdjustContent" component={ProjectAdjustContent} />
                                    <Route exact path="/ResetCountDown" component={ResetCountDown} />
                                    <Route exact path="/UserGuide" component={UserGuide} />
                                    {/* 確認加載後再決定Menu */}
                                    {loading &&
                                        <div id="container" className={headerFooterStatus ? "navOpen full-screen" : "navOpen"}  >
                                            <nav className={visible ? "" : "close-menu"}>
                                                <div className={visible ? "" : "close-menu"}>
                                                    <Button icon="menu" onClick={openMenu} />
                                                </div>
                                                {
                                                    headerFooterStatus
                                                        ? <ProjectChapterRoutePanel visible={visible} />
                                                        : <HomeRoutePanel visible={visible} />
                                                }
                                            </nav>
                                            <Suspense fallback={<Overlay />}>
                                                <Route path="/" render={() => <HomeRoute />} />
                                            </Suspense>
                                        </div>
                                    }
                                </Switch>
                            </BrowserRouter>
                        </ErrorBoundary>
                        <Foot />
                    </ChapterPageIndexProvider>
                </SignInStatusProvider>
            </IntlProvider>
        </LocalizationProvider>
    );
}

export default RootRoute;
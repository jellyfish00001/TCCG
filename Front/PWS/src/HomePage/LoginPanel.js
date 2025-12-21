import React, { useEffect, useContext, useState, useRef } from 'react';
import * as Yup from 'yup';
import '../Css/Login.css';
import { Formik } from 'formik';
import { GetBasicData, HeaderFooterContext, SetHistory, SignInStatusContext } from '../Basic/BasicData';
import { api } from '../Basic/ApiFetch';
import CacheLoader from '../Basic/CacheLoader';
import { IsNullOrEmpty, openPage } from '../Basic/SDOExtension';
import TextInput from '../Components/Input/TextInput';
import { Captcha } from '../Components/Captcha/Captcha';
import { showGlobalMessageBox, getGlobalServerConfig, resetGlobalServerConfig } from '../Route/RootMiddleware';
import { WindowResizehook } from '../Hook/useWindowResize';
import { Button } from '@progress/kendo-react-buttons';
import { Window } from '@progress/kendo-react-dialogs';

import logoLogin from '../Images/logo_login.png';
import iconUnlock from '../Images/fa-unlock-alt.png';
import iconQuestion from '../Images/icon_question_h.png';

import ResetPW from './ResetPW';
import ForgotPW from './ForgotPW';
import { GetScLink, GetSingleSetParam } from '../Basic/CommonService';

const LoginPanel = (props) => {
    const initCaptchaEncode = { CaptchaEncode: '', }
    const { setSignInStatus, signInStatus } = useContext(SignInStatusContext);
    const { headerFooterStatus, setHeaderFooterStatus } = React.useContext(HeaderFooterContext);
    const [passwordVisible, setPasswordVisible] = useState(true);
    const [captchaTxt, setCaptcha] = useState(initCaptchaEncode);
    const [tempData, setTempData] = useState({});
    const loginCnt = useRef(0);
    const [canAccApply, setCanAccApply] = useState(true)

    const dimensions = WindowResizehook();

    // 密碼變更
    const [isShowResetPW, setIsShowResetPW] = useState(false);
    // 忘記密碼
    const [isShowForgotPW, setIsShowForgotPW] = useState(false);

    useEffect(() => {
        checkAccApply();
        cleanCache()

        getCaptcha()

        // 恢復預設值
        setHeaderFooterStatus(false);

        localStorage.removeItem('IsOpenRCD');

        // 避免home頁登出後，window.name還停留在原本的
        window.name = "";
    }, [])

    const checkAccApply = async () => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'Login/CheckAccApply';
        let res = await api.Get(url, null, false, false);
        if (res.ok) {
            setCanAccApply(await res.json())
        }
    }

    /**
     * 清除快取
     */
    const cleanCache = async () => {
        if (!IsNullOrEmpty(await CacheLoader().GetCache())) {
            // 清除快取
            await CacheLoader().DeleteCache();
        }
        // 紀錄目前登入狀態
        setSignInStatus();
    }

    /**
     * 檢查快取，若存在直接進行導頁
     */
    const cacheLoaderCheck = async () => {
        SetHistory(props.history);
        let cacheLoader = CacheLoader();
        let loadResult = await cacheLoader.LoadCache();
        if (loadResult) {
            // 判斷此登入者是否是議員 如果是導頁議會案件；如果不是澤進入口頁
            let userType = await GetBasicData("userType");
            if (userType === "4") {
                const isNewVersion = (await GetSingleSetParam("SystemConfig", "TrackoIsNewVersion")).SET_VALUE;
                let url = "";
                if (isNewVersion === "Y") {
                    let setParam = await GetSingleSetParam("SystemConfig", "TrackoParliamentUrl");
                    url = `${setParam.SET_VALUE}/PTMSSSO/TYCGLoginSSO?`;
                    url += `uid=${await GetBasicData("userId")}`;
                }
                else {
                    url = await GetScLink("OLDRIS", "PA");
                    url += '&AI=PA';
                }
                window.location.href = url;
            }
            else {
                setSignInStatus();
                props.history.push('/Home');
            }
        }
        else if (loginCnt.current === 0) {
            loginCheck(tempData);
            loginCnt.current = loginCnt.current + 1;
        }
        else {
            console.log("LoginError")
        }
    }

    //登入驗證
    const loginCheck = async (data) => {
        if (IsNullOrEmpty(data.USER_PD) || IsNullOrEmpty(data.Captcha)) {
            return;
        }

        setTempData(data);
        data.CaptchaEncoded = captchaTxt.CaptchaEncode;
        let url = getGlobalServerConfig().backEndUrl.get() + 'Login';
        let response = await api.Post(url, JSON.stringify(data));
        if (response.ok) {
            let result = await response.json();
            if (!result.success) {
                showGlobalMessageBox(result.message);
                return;
            }

            // 判斷須變更密碼
            if (result.data === "Y") {
                // 開啟密碼變更視窗
                setIsShowResetPW(true);
                //清除BasicData
                resetGlobalServerConfig();
                //清除快取
                await CacheLoader().DeleteCache();
            }
            else {
                cacheLoaderCheck();
            }
        }
    }

    /**
     * 獲取驗證碼
     * @param {*} e 
     */
    const getCaptcha = async (e) => {
        if (e) {
            e.preventDefault();
        }
        let url = getGlobalServerConfig().backEndUrl.get() + `Login/LoadCapcha`;
        let captchaData = { ...initCaptchaEncode };

        try {
            let response = await api.Get(url, null, null, false);
            if (response.ok) {
                captchaData = await response.json();
                // 接到request data後資料更新參數
                setCaptcha(captchaData);
            }
        }
        catch (ex) {
            console.log(ex);
        }
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        USER_ID: Yup.string()
            .required('請輸入帳號'),
        USER_PD: Yup.string()
            .required('請輸入密碼'),
        Captcha: Yup.string()
            .required('請輸入驗證碼')
    });

    // 連結SC網址
    const onClickScLink = async (dominName, apId) => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'Login/GetScLink/' + dominName + '/' + apId;
        let response = await api.Get(url, null, new Headers(), false);
        let data = {};
        if (response.ok) {
            data = await response.json();
        }

        let link = data.message.replaceAll("&amp;", "&");
        openPage(link, `${dominName}_${apId}`);
    }

    return (
        <div className='loginPanel'>
            <div className='loginFrame'>
                <img src={logoLogin} className='logoLogin' alt='' />
                <Formik
                    initialValues={{
                        USER_ID: "",
                        USER_PD: "",
                        Captcha: "",
                        CaptchaEncoded: ""
                    }}
                    validationSchema={validateField}
                    onSubmit={(data) => loginCheck(data)}
                >
                    {props => {
                        const {
                            values,
                            errors,
                            handleBlur,
                            handleSubmit,
                            handleChange,
                            submitForm
                        } = props;
                        return (
                            <form onSubmit={handleSubmit}>
                                <table style={{ border: "none" }}>
                                    <tbody>
                                        <tr>
                                            <th>使用者帳號</th>
                                            <td>
                                                <TextInput
                                                    onChange={handleChange}
                                                    value={values.USER_ID}
                                                    name="USER_ID"
                                                    onBlur={handleBlur}
                                                    error={errors.USER_ID}
                                                    onKeyDown={(e) => {
                                                        if (e.key === 'enter') {
                                                            submitForm();
                                                        }
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>使用者密碼</th>
                                            <td>
                                                <i
                                                    className={`fa ${passwordVisible ? "fa-eye-slash" : "fa-eye"} pull-right`}
                                                    onClick={() => setPasswordVisible(!passwordVisible)}
                                                    style={{ marginTop: "5px" }}
                                                ></i>
                                                <TextInput
                                                    onChange={handleChange}
                                                    value={values.USER_PD}
                                                    name="USER_PD"
                                                    type={passwordVisible ? "password" : "text"}
                                                    onBlur={handleBlur}
                                                    error={errors.USER_PD}
                                                    onKeyDown={(e) => {
                                                        if (e.key === 'enter') {
                                                            submitForm();
                                                        }
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>驗證碼</th>
                                            <td>
                                                <TextInput
                                                    onChange={handleChange}
                                                    value={values.Captcha}
                                                    name="Captcha"
                                                    onBlur={handleBlur}
                                                    error={errors.Captcha}
                                                    onKeyDown={(e) => {
                                                        if (e.key === 'enter') {
                                                            submitForm();
                                                        }
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colSpan={4}>
                                                {
                                                    captchaTxt.Img &&
                                                    <Captcha
                                                        Img={captchaTxt.Img}
                                                        GetCaptcha={getCaptcha}
                                                    />
                                                }
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colSpan={2} >
                                                <Button type="submit">登入系統</Button>
                                            </td>
                                        </tr>
                                        {canAccApply &&
                                            <tr>
                                                <td colSpan={2} >
                                                    <Button type="button" onClick={() => onClickScLink('RISSCnet', 'SC32_AAFU')}>帳號申請</Button>
                                                </td>
                                            </tr>}

                                        <tr>
                                            <td colSpan={2} >
                                                <ul className="loginLink">
                                                    <li>
                                                        <a href="/" onClick={(e) => {
                                                            e.preventDefault();
                                                            setIsShowResetPW(true);
                                                        }}>
                                                            <img src={iconUnlock} className='iconUnlock' alt='' />
                                                            密碼變更
                                                        </a>
                                                    </li>
                                                    <li>
                                                        <a href="/" onClick={(e) => {
                                                            e.preventDefault();
                                                            setIsShowForgotPW(true);
                                                        }}>
                                                            <img src={iconQuestion} className='iconQuestion' alt='' />
                                                            忘記密碼
                                                        </a>
                                                    </li>
                                                </ul>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </form>
                        );
                    }}
                </Formik>

                {
                    isShowResetPW &&
                    <div className="fullscreen window-fullscreen">
                        <Window
                            title={"密碼變更"}
                            onClose={() => setIsShowResetPW(false)}
                            width={dimensions.width * 0.7}
                            height={dimensions.height * 0.85}
                            draggable={false}
                            resizable={false}
                            modal={true}
                        >
                            <ResetPW
                                closeWindow={() => setIsShowResetPW(false)}
                            />
                        </Window>
                    </div>
                }

                {
                    isShowForgotPW &&
                    <div className="fullscreen window-fullscreen">
                        <Window
                            title={"忘記密碼"}
                            onClose={() => setIsShowForgotPW(false)}
                            width={dimensions.width * 0.45}
                            height={dimensions.height * 0.55}
                            draggable={false}
                            resizable={false}
                            modal={true}
                        >
                            <ForgotPW
                                closeWindow={() => setIsShowForgotPW(false)}
                            />
                        </Window>
                    </div>
                }
            </div>
        </div>
    );
}

export default LoginPanel;
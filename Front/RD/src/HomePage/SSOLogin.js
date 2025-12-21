import React from "react";
import { api } from "../Basic/ApiFetch";
import { getGlobalServerConfig, showGlobalMessageBox } from "../Route/RootMiddleware";
import { setPrintInfo } from '../Basic/CommonService';
import { IsNullOrEmpty } from "../Basic/SDOExtension";

const SSOLogin = (props) => {

    const ssoLoginCheck = async () => {
        const errMsg = "使用者資訊有誤，請洽系統管理員";
        const search = require('query-string').parse(props.location.search);

        if (IsNullOrEmpty(search.Token)) {
            showGlobalMessageBox(errMsg);
            return;
        }

        // Tracko 呼叫計畫預覽列印 參數
        if (search.Type === "1" && (IsNullOrEmpty(search.ProjectNo) || IsNullOrEmpty(search.ProjectName))) {
            showGlobalMessageBox(errMsg);
            return;
        }

        let url = getGlobalServerConfig().backEndUrl.get() + 'Login/SSOLogin';
        let response = await api.Post(url, JSON.stringify(search.Token));
        let result = await response.json();
        if (!result.success) {
            showGlobalMessageBox(errMsg);
            return;
        }

        switch (search.Type ?? "") {
            case "1": // 計畫預覽列印
                openProjectPrint(search.ProjectNo, search.ProjectName);
                break;
            default:
                props.history.push('/Home');
                break;
        }
    }

    // 開啟計畫預覽列印
    const openProjectPrint = async (projectNo, projectName) => {
        setPrintInfo(false, false, projectNo, projectName, true);
        props.history.push('/ProjectPrint');
    }

    React.useEffect(() => {
        ssoLoginCheck()
    }, [])

    return (
        <>
        </>
    )
}

export default SSOLogin;
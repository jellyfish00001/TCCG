import React from "react";
import { api } from "../Basic/ApiFetch";
import { getGlobalServerConfig, showGlobalMessageBox } from "../Route/RootMiddleware";
import { IsNullOrEmpty } from "../Basic/SDOExtension";

const SSOCloud = (props) => {

    React.useEffect(() => {
        ssoLoginCheck();
    }, [])

    const ssoLoginCheck = async () => {
        const errMsg = "使用者資訊有誤，請洽系統管理員";
        const search = require('query-string').parse(props.location.search);

        if (IsNullOrEmpty(search.sessionId) || IsNullOrEmpty(search.cn)) {
            showGlobalMessageBox(errMsg, () => window.close());
            return;
        }

        let url = getGlobalServerConfig().backEndUrl.get() + 'Login/SSOCloud';
        let form = new FormData();
        form.append('sessionId', search.sessionId);
        form.append('userId', search.cn);
        let response = await api.Post(url, form, new Headers(), false);
        let result = await response.json();
        if (!result.success) {
            showGlobalMessageBox(errMsg, () => window.close());
            return;
        }

        if (IsNullOrEmpty(search.queryType)) {
            props.history.push('/SystemSelector');
        }
        else {
            props.history.push('/Home/UnitingQuery/UnitingQueryMain', { queryType: search.queryType });
        }
    }

    return (
        <>
        </>
    )
}

export default SSOCloud;
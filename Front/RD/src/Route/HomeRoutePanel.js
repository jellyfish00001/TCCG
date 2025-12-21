import React, { useEffect, useState, useContext } from 'react';
import { api } from '../Basic/ApiFetch';
import { SetHistory, SignInStatusContext } from '../Basic/BasicData';
import { IsNullOrEmpty, setMenu } from '../Basic/SDOExtension';
import { PanelBar, PanelBarUtils } from '@progress/kendo-react-layout';
import { Reveal } from '@progress/kendo-react-animation';
import 'bootstrap/dist/css/bootstrap.min.css';
import { getGlobalServerConfig } from '../Route/RootMiddleware';
import { withRouter } from 'react-router-dom';

const HomeRoutePanel = (props) => {
    const { visible } = props;
    const { signInStatus, setSignInStatus } = useContext(SignInStatusContext);
    const [items, setItems] = useState([]);

    const getFunctionList = async () => {
        try {
            let response = await api.Get(getGlobalServerConfig().backEndUrl.get() + 'SetFunction/ReadFunctionList?apid=RD2');
            let responseData = await response.json();
            /*接到request data後要做的事情*/
            // 設定Menu清單
            responseData = setMenu(responseData);
            setItems(responseData);
            if (!signInStatus)
                setSignInStatus();
        }
        catch (e) {
            console.log(e);
        }
    }

    const onSelect = (event) => {

        if (!IsNullOrEmpty(event.target.props.functionUrl) && props.history.location.pathname !== event.target.props.functionUrl)
            props.history.push(event.target.props.functionUrl);
        if (!signInStatus)
            setSignInStatus();
    }

    useEffect(() => {
        SetHistory(props.history);
        getFunctionList();

    }, []);

    const components = PanelBarUtils.mapItemsToComponents(items);

    return (
        <>
            <Reveal
                //動畫長短設定
                transitionExitDuration={500}
                transitionEnterDuration={500}
                direction={"horizontal"}
            >
                {visible ? <PanelBar expandMode={"single"} onSelect={onSelect} children={components} style={{ width: '200px' }} /> : null}
            </Reveal>
        </>
    );
}

export default withRouter(HomeRoutePanel);
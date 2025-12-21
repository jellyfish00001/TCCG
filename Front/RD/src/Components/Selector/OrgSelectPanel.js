import React, { useEffect, useState } from 'react';
import { Button } from '@progress/kendo-react-buttons'
import { PanelBar, PanelBarUtils } from '@progress/kendo-react-layout';
import { Reveal } from '@progress/kendo-react-animation';
import 'bootstrap/dist/css/bootstrap.min.css';
import { withRouter } from 'react-router-dom';

const OrgSelectPanel = (props) => {
    let { data, title, style } = props;
    const [items, setItems] = useState([]);
    const [visible, setVisible] = useState(true);

    const openMenu = () => {
        setVisible(!visible);
    }

    // 回傳選取之機關代碼
    const onSelect = (event) => {
        props.onSelect(event.target.props.OU_ID);
    }

    useEffect(() => {
        if (data && data.length > 0) {
            setItems([...data])
        }
    }, [data])

    const components = PanelBarUtils.mapItemsToComponents(items);

    return (
        <div className={visible ? "orgPanelBg" : "orgPanelBgClose"} style={visible ? style : null}>
            <div>
                <Button icon="menu" onClick={openMenu} ></Button>
                {visible && title && <span>{title}</span>}
            </div>
            <Reveal
                //動畫長短設定
                transitionExitDuration={500}
                transitionEnterDuration={500}
                direction={"horizontal"}
            >
                {visible && <PanelBar expandMode={"single"} onSelect={onSelect} children={components} style={{ width: '200px' }} />}
            </Reveal>
        </div>
    );
}

export default withRouter(OrgSelectPanel);
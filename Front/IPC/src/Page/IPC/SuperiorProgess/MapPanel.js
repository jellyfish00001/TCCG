import React from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { Button } from '@progress/kendo-react-buttons';
import { Reveal } from '@progress/kendo-react-animation';
import { withRouter } from 'react-router-dom';
import map from '../../../Images/map.jpg';
import pin from '../../../Images/pin-large.png';

const MapPanel = (props) => {
    const { mapData, setFilterTownName } = props;

    const [visible, setVisible] = React.useState(true);
    const [data, setData] = React.useState(mapData);

    // icon click 事件
    const mapItemClick = (townName) => {
        setData([...data.filter(x => x.name === townName)]);
        setFilterTownName(townName);
    }

    // 查全市 click 事件
    const onShowAll = () => {
        setData([...mapData]);
        setFilterTownName("");
    }

    return (
        <div
            className={visible ? "orgPanelBg" : "orgPanelBgClose"}
            style={{ backgroundColor: "transparent", width: visible ? "50%" : "2%" }}
        >
            <div>
                <Button
                    icon="menu"
                    onClick={() => setVisible(!visible)}
                    style={{ backgroundColor: visible ? "var(--blue-fnbuttons-bg)" : "" }}
                ></Button>
                <Button
                    onClick={() => onShowAll()}
                    style={{ marginLeft: "10px", fontSize: "18px", backgroundColor: "var(--blue-fnbuttons-bg)", display: visible ? "" : "none" }}
                >查全市</Button>
            </div>
            <Reveal
                //動畫長短設定
                transitionExitDuration={500}
                transitionEnterDuration={500}
                direction={"horizontal"}
            >
                {
                    visible &&
                    <div style={{ position: 'relative' }}>
                        <img src={map} alt="" />
                        {
                            data.map((x) => (
                                <div
                                    style={{ position: 'absolute', top: `${x.top}`, right: `${x.right}` }}
                                    onClick={() => mapItemClick(x.name)}
                                >
                                    <img src={pin} alt="" style={{ width: '50px' }} />
                                </div>
                            ))
                        }
                    </div>
                }
            </Reveal>
        </div>
    );
}

export default withRouter(MapPanel);
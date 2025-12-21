import React from 'react';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { TabStrip, TabStripTab } from '@progress/kendo-react-layout';
import { Button } from '@progress/kendo-react-buttons';
import '../../../Css/Map.css';
import MapTab from './MapTab';
import ListTab from './ListTab';

const MapGrid = (props) => {
    const { gridData, filterTownName } = props;

    const [selected, setSelected] = React.useState(0);
    const [listData, setListData] = React.useState([]);
    const [filterObj, setFilterObj] = React.useState({ TownName: "", StageType: "", OffsetType: "" });

    React.useEffect(() => {
        setFilterObj({ ...filterObj, TownName: filterTownName, StageType: "", OffsetType: "" });
    }, [filterTownName])

    React.useEffect(() => {
        let data = gridData;
        if (!IsNullOrEmpty(filterObj.TownName)) {
            data = data.filter(x => x.TOWN_NAME === filterObj.TownName);
        }

        switch (filterObj.StageType) {
            case "N": // 規劃中
                data = data.filter(x => IsNullOrEmpty(x.ENGNEER_STAGE));
                break;
            case "A": // 施工中
            case "B": // 已完工
                data = data.filter(x => x.ENGNEER_STAGE === filterObj.StageType);
                break;
            default: // 總覽
                break;
        }

        switch (filterObj.OffsetType) {
            case "A": // 進度符合
                data = data.filter(x => x.PRG_OFFSET >= 0);
                break;
            case "B": // 落後<5%
                data = data.filter(x => 0 > x.PRG_OFFSET && x.PRG_OFFSET > -5);
                break;
            case "C": // 落後>=5%
                data = data.filter(x => x.PRG_OFFSET <= -5);
                break;
            default:
                break;
        }
        setListData([...data]);
    }, [filterObj])

    // 總覽 規劃中 施工中 已完工 className
    const btnStageClass = (state) => {
        return filterObj.StageType === state ? "btnStageClickState" : "";
    }

    // 進度符合 落後 className
    const btnOffsetClass = (type) => {
        return filterObj.OffsetType === type ? "btnOffsetClickState" : "";
    }

    // 總覽 規劃中 施工中 已完工 click 事件
    const stageClick = (stageType) => {
        setFilterObj({ ...filterObj, StageType: stageType });
    }

    // 進度符合 落後 click 事件
    const offsetClick = (offsetType) => {
        setFilterObj({ ...filterObj, OffsetType: offsetType });
    }

    return (
        <div style={{ width: '100%' }}>
            <div style={{ display: 'flex' }}>
                <div className='fn-buttons' style={{ width: '50%' }}>
                    <Button
                        className={btnStageClass("")}
                        onClick={() => stageClick("")}
                    ><i className='fa fa-clone'></i>總覽</Button>
                    <Button
                        className={btnStageClass("N")}
                        iconClass='mapIcon refresh'
                        onClick={() => stageClick("N")}
                    >規劃中</Button>
                    <Button
                        className={btnStageClass("A")}
                        iconClass='mapIcon cog'
                        onClick={() => stageClick("A")}
                    >施工中</Button>
                    <Button
                        className={btnStageClass("B")}
                        iconClass='mapIcon check'
                        onClick={() => stageClick("B")}
                    >已完工</Button>
                    <h2 style={{ margin: "10px" }}>區域：{IsNullOrEmpty(filterObj.TownName) ? "全市" : filterObj.TownName}</h2>
                </div>
                <div style={{ width: '50%', textAlign: 'right', paddingRight: '10px' }}>
                    <div style={{ paddingTop: '5px' }}>
                        <Button
                            className={btnOffsetClass("A")}
                            style={{ backgroundColor: '#3F8618', width: '95px' }}
                            onClick={() => offsetClick("A")}
                        >進度符合</Button>
                    </div>
                    <div style={{ paddingTop: '5px' }}>
                        <Button
                            className={btnOffsetClass("B")}
                            style={{ backgroundColor: '#E6972A', marginTop: '5px', width: '95px' }}
                            onClick={() => offsetClick("B")}
                        >{'落後<5%'}</Button>
                    </div>
                    <div style={{ paddingTop: '5px' }}>
                        <Button
                            className={btnOffsetClass("C")}
                            style={{ backgroundColor: '#C4432B', marginTop: '5px', width: '95px' }}
                            onClick={() => offsetClick("C")}
                        >{'落後>=5%'}</Button>
                    </div>
                </div>
            </div>

            <div style={{ marginTop: '10px' }}>
                <TabStrip
                    selected={selected}
                    onSelect={(e) => setSelected(e.selected)}
                >
                    <TabStripTab title='地圖模式'>
                        <MapTab listData={listData} />
                    </TabStripTab>
                    <TabStripTab title='清單模式'>
                        <ListTab listData={listData} />
                    </TabStripTab>
                </TabStrip>
            </div>
        </div>
    )
}

export default MapGrid;
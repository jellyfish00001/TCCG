import React from 'react';
import { PageContainer } from '../../../Basic/PageContainer';
import { CheckIsHANDRole } from '../../../Basic/CommonService';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import { Button } from '@progress/kendo-react-buttons';
import { Window } from '@progress/kendo-react-dialogs';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem } from '@progress/kendo-react-charts';
import 'hammerjs';
import map from '../../../Images/map.jpg';
import '../../../Css/Map.css';

import Service from './SuperiorRegionService';
import RegionQueryForm from './RegionQueryForm';
import PlanListGrid from './PlanListGrid';

const SuperiorRegionMain = (props) => {

    const tempFormData = {
        MASTER_DEPT: "", // 主管機關
        EXEC_DEPT: "", // 執行機關
        ENGNEER_STAGE: "", // 工程階段
        IS_HAND_ROLE: false, // 是否只有主辦權限
    };
    const [formData, setFormData] = React.useState(tempFormData);

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState({
        MASTER_ORGAN: [], // 主管機關
        EXEC_ORGAN: [], // 執行機關
    });
    // 長條圖資料
    const [chartData, setChartData] = React.useState({ Categories: [], Data: [] });
    // 地圖資料
    const [mapData, setMapData] = React.useState([]);
    // 顯示計畫清單
    const [planList, setPlanList] = React.useState({ visible: false, name: "", TOWN_C: "" });
    const dimensions = WindowResizehook();
    // 是否顯示內容
    const [isVisible, setIsVisible] = React.useState(false);
    // Formik innerRef
    const formRef = React.useRef(null);

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = async () => {
        await getAllDropDowns();
        await getData(formData);
    }

    // 取得下拉清單
    const getAllDropDowns = async () => {
        SetMaskOnOff(true);
        const checkIsHANDRole = await CheckIsHANDRole();
        const data = await Service.getOrganData(checkIsHANDRole);
        setDdlData({ ...ddlData, MASTER_ORGAN: data, EXEC_ORGAN: data });

        let model = formData;
        model.IS_HAND_ROLE = checkIsHANDRole;
        setFormData({ ...model });
        SetMaskOnOff(false);
    }

    // 查詢
    const onsubmit = async () => {
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    }

    // 取資料
    const getData = async (data) => {
        SetMaskOnOff(true);
        setIsVisible(false);
        setFormData({ ...data });
        const rdata = await Service.getRegion(data);
        setChartData({ ...Service.getChartData(rdata) });
        setMapData([...Service.getMapData(rdata)]);
        setIsVisible(true);
        SetMaskOnOff(false);
    }

    // 清除
    const onClear = () => {
        if (formRef.current) {
            formRef.current.handleReset();
            setIsVisible(false);
            setFormData({ ...tempFormData });
        }
    }

    // 長條圖 狀體 click 事件
    const chartItemClick = (dataItem) => {
        const items = dataItem.category.split("_");
        setPlanList({ ...planList, visible: true, name: items[1], TOWN_C: items[0] });
    }

    return (
        <PageContainer
            toolbar={
                <>
                    <h3 className='k-dialog-titlebar'>決策分析 – 區域統計分析</h3>
                    <Button className='k-button-lighten' onClick={() => onsubmit()}>查詢</Button>
                    <Button className='k-button-lighten' onClick={() => onClear()}>清除</Button>
                    <Button className='k-button-lighten' onClick={() => window.print()}>列印</Button>
                </>
            }
        >
            <RegionQueryForm
                ddlData={ddlData}
                formData={formData}
                formRef={formRef}
                getData={getData}
            />

            {
                isVisible &&
                <div style={{ display: 'flex', overflow: 'auto', height: 'Calc(100% - 75px)' }}>
                    <div style={{ position: 'relative' }}>
                        <img src={map} alt='' height='500px' />
                        {
                            mapData.map((x) => (
                                <div className='regionImgLabel' style={{ top: `${x.top}`, right: `${x.right}` }}>{x.cnt}</div>
                            ))
                        }
                    </div>

                    <Chart
                        style={{ height: chartData.Categories.length * 30, minHeight: 100, width: '100%' }}
                        onSeriesClick={(dataItem) => chartItemClick(dataItem)}
                    >
                        <ChartLegend position='top' orientation='horizontal' />
                        <ChartCategoryAxis>
                            <ChartCategoryAxisItem categories={chartData.Categories} labels={{
                                font: '30px',
                                margin: { right: 10 },
                                content: (e) => { return e.value.split('_')[1] }
                            }} />
                        </ChartCategoryAxis>
                        <ChartSeries>
                            {
                                chartData.Data.map((item) => (
                                    <ChartSeriesItem
                                        type='bar'
                                        data={item.data}
                                        name={item.name}
                                        color={item.color}
                                        labels={{
                                            visible: true,
                                            position: 'center',
                                            background: 'transparent',
                                            content: (e) => { return e.value > 0 ? e.value : "" },
                                            font: "bold 16px Arial, sans-serif"
                                        }}
                                        stack={true}
                                    />
                                ))
                            }
                        </ChartSeries>
                    </Chart>
                </div>
            }

            {
                planList.visible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title={`區域統計分析 - ${planList.name}`}
                        onClose={() => {
                            setPlanList({ ...planList, visible: false, name: "", TOWN_C: "" });
                        }}
                        width={dimensions.width * 0.9}
                        height={dimensions.height * 0.9}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <PlanListGrid
                            mainFormData={formData}
                            TOWN_C={planList.TOWN_C}
                        />
                    </Window>
                </div>
            }
        </PageContainer>
    )
}

export default SuperiorRegionMain;
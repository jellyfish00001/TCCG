import React from 'react';
import GotoTopBtn from '../../../Components/Utils/GotoTopBtn';
import { GetHistory } from '../../../Basic/BasicData';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import { Window } from '@progress/kendo-react-dialogs';
import { Button } from '@progress/kendo-react-buttons';

import Service from './SuperiorAnalysisService';
import StatCntPieChart from './Chart/StatCntPieChart';
import DelayPieChart from './Chart/DelayPieChart';
import BehindItemChart from './Chart/BehindItemChart';
import BarChart from './Chart/BarChart';
import PlanListGrid from './PlanListGrid';
import QueryConditions from './QueryConditions';

const SuperiorAnalysisDetailMain = ({
    location: { state },
    location: {
        state: { categoryName, mainFormData } = {
            categoryName: "", mainFormData: {
                ...mainFormData,

                MAIN_TYPE: "", // 主頁點擊的型態
                MAIN_VALUE: "", // 主頁點擊的值
                IS_GET_MATCH: null, // 是否取得符合 Null:無 True:符合 False:落後
            }
        }
    }
}) => {

    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => GetHistory().push('/Home/SuperiorAnalysis/SuperiorAnalysisMain'));
    }

    const [formData, setFormData] = React.useState({
        ...mainFormData,
        DETAIL_TYPE: "", // 次頁點擊的型態
        DETAIL_VALUE: "", // 次頁點擊的值
        DELAY_KIND: "", // 落後類別
        DELAY_SUBCLASS_C: "" // 落後次類別代碼
    })

    // 圓餅圖
    const [statCntpieData, setStatCntPieData] = React.useState([]);
    // 落後圓餅圖
    const [delayPieData, setDelayPieData] = React.useState([]);
    // 建設分布
    const [buildData, setBuildData] = React.useState({ Categories: [], Data: [] });
    // 機關分布
    const [orgData, setOrgData] = React.useState({ Categories: [], Data: [] });
    // 地區分布
    const [areaData, setAreaData] = React.useState({ Categories: [], Data: [] });
    // 落後項目
    const [delayItemData, setDelayItemData] = React.useState({ Categories: [], Data: [] });

    // 顯示計畫清單
    const [planList, setPlanList] = React.useState({ visible: false, name: "" });

    const dimensions = WindowResizehook();

    React.useEffect(() => {
        if (state) {
            getData();
        }
    }, [])

    // 取資料
    const getData = async () => {
        if (formData.CHART_TYPE !== 3) {
            const pieData = await Service.getPieChartStat(formData);
            setStatCntPieData([...pieData]);

            const item = pieData.find(x => x.category === "符合");
            setFormData({ ...formData, IS_GET_MATCH: item !== undefined && item.value > 0 });
        }
    }

    // 回上頁
    const onBack = () => {
        GetHistory().push('/Home/SuperiorAnalysis/SuperiorAnalysisMain', {
            mainFormData: {
                ...mainFormData,
                MAIN_TYPE: "",
                MAIN_VALUE: "",
                IS_GET_MATCH: null
            }
        });
    }

    // 如果 isGetMatch 是 true 查符合資料; false 查落後資料
    React.useEffect(() => {
        if (state && formData.IS_GET_MATCH !== null) {
            getChartStat();
        }
    }, [formData.IS_GET_MATCH])

    // 取得明細資料
    const getChartStat = async () => {
        const data = await Service.getAnalysisDetail(formData);
        const isGetMatch = formData.IS_GET_MATCH;
        setBuildData({ ...Service.getDetailChartStat(data.StatBuilds, isGetMatch, 0) });
        setOrgData({ ...Service.getDetailChartStat(data.StatMasterOrgans, isGetMatch, 1) });
        setAreaData({ ...Service.getDetailChartStat(data.StatTowns, isGetMatch, 2) });
        setDelayPieData([...Service.getDelayPieData(data.StatCntBehind)]);
        setDelayItemData({ ...Service.getDelayItemData(data.BehindItems) });
    }

    // 長條圖 狀體 click 事件
    const chartItemClick = (categoryId, categoryName, chartType) => {
        setFormData({
            ...formData,
            DETAIL_TYPE: chartType,
            DETAIL_VALUE: categoryId,
            DELAY_KIND: "",
            DELAY_SUBCLASS_C: ""
        });
        setPlanList({ ...planList, visible: true, name: categoryName });
    }

    // 落後項目 狀體 click 事件
    const behindItemChartItemClick = (categoryId, categoryName) => {
        setFormData({
            ...formData,
            DETAIL_TYPE: "",
            DETAIL_VALUE: "",
            DELAY_KIND: "",
            DELAY_SUBCLASS_C: categoryId
        });
        setPlanList({ ...planList, visible: true, name: categoryName });
    }

    // 落後圓餅圖 click 事件
    const behindPieChartItemClick = (categoryId, categoryName) => {
        setFormData({
            ...formData,
            DETAIL_TYPE: "",
            DETAIL_VALUE: "",
            DELAY_KIND: categoryId,
            DELAY_SUBCLASS_C: ""
        });
        setPlanList({ ...planList, visible: true, name: categoryName });
    }

    return (
        <>
            <div className="SuperiorAnalysisDetail" style={{ overflow: "auto", height: "100%" }}>
                <h3 className="k-dialog-titlebar">決策分析 – 重大建設分析 – {categoryName}</h3>

                <div className="fn-buttons">
                    <Button title="回上頁" className="k-button-lighten" onClick={() => onBack()}>回上頁</Button>
                    <Button className='k-button-lighten' onClick={() => window.print()}>列印</Button>
                </div>

                {
                    formData.CHART_TYPE !== 3 &&
                    <>
                        <QueryConditions data={formData} />
                        <h3>{categoryName} – 圓餅圖</h3>
                        <div style={{ display: "flex" }}>
                            <div style={{ width: "50%" }}>
                                {/* 圓餅圖 */}
                                <StatCntPieChart
                                    pieData={statCntpieData}
                                    ChartItemClick={(e) => setFormData({ ...formData, IS_GET_MATCH: e })}
                                />
                            </div>
                            <div style={{ width: "50%" }}>
                                {/* 落後圓餅圖 */}
                                {
                                    formData.IS_GET_MATCH === false &&
                                    <DelayPieChart
                                        pieData={delayPieData}
                                        chartItemClick={behindPieChartItemClick}
                                    />
                                }
                            </div>
                        </div>
                    </>
                }

                <h2>{formData.IS_GET_MATCH === true ? "符合進度" : formData.IS_GET_MATCH === false ? "落後進度" : ""}</h2>

                {
                    formData.IS_GET_MATCH === false &&
                    <>
                        <BehindItemChart
                            title={`${categoryName} – 落後項目（單位：件）`}
                            model={delayItemData}
                            chartItemClick={behindItemChartItemClick}
                        />
                    </>
                }

                {
                    buildData.Categories.length > 0 &&
                    <BarChart
                        title={`${categoryName} – 建設分布（單位：件）`}
                        model={buildData}
                        chartItemClick={chartItemClick}
                    />
                }

                {
                    orgData.Categories.length > 0 &&
                    <BarChart
                        title={`${categoryName} – 主管機關分布（單位：件）`}
                        model={orgData}
                        chartItemClick={chartItemClick}
                    />
                }

                {
                    areaData.Categories.length > 0 &&
                    <BarChart
                        title={`${categoryName} – 地區分布（單位：件）`}
                        model={areaData}
                        chartItemClick={chartItemClick}
                    />
                }
            </div>
            <GotoTopBtn targetClass="SuperiorAnalysisDetail" />

            {
                planList.visible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title={`${categoryName} - ${planList.name}`}
                        onClose={() => {
                            setPlanList({ ...planList, visible: false, name: "" });
                            setFormData({ ...formData, DETAIL_TYPE: "", DETAIL_VALUE: "" });
                        }}
                        width={dimensions.width * 0.9}
                        height={dimensions.height * 0.9}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <PlanListGrid
                            mainFormData={formData}
                        />
                    </Window>
                </div>
            }
        </>
    )
}

export default SuperiorAnalysisDetailMain;
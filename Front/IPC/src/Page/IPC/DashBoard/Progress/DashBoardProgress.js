import React, { useEffect, useRef, useState } from 'react';
import { PageContainer } from '../../../../Basic/PageContainer';
import { CheckIsHANDRole, getOrganList } from '../../../../Basic/CommonService';
import { SetMaskOnOff, setMenu } from '../../../../Basic/SDOExtension';
import { WindowResizehook } from '../../../../Hook/useWindowResize';
import { Button } from '@progress/kendo-react-buttons';
import { Window } from '@progress/kendo-react-dialogs';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem } from '@progress/kendo-react-charts';
import 'hammerjs';
import map from '../../../../Images/map.jpg';
import delayPng from '../../../../Images/dashBoardDelay.png'
import buildKindPng from '../../../../Images/buildKind.png'
import inProgress from '../../../../Images/icon_inprogress.png'
import setttings from '../../../../Images/icon_settings.png'
import wrench from '../../../../Images/icon_wrench.png'
import ontime from '../../../../Images/ontime.png'
import beOnTime from '../../../../Images/beOnTime.png'
import '../../../../Css/Map.css';
import style from '../../../../Css/custom/DashBoard.module.css';
import { getEngProgress, getFillCompleteCycle, getMapData, getProgressMapTownPosition } from '../DashBoardService';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';
import { getStatisticsRPT } from '../DashBoardService';

// 重大工程進度 Component
const DashBoardProgress = (props) => {
    // 地圖資料
    const dimensions = WindowResizehook();

    // 執行機關選單資料
    const [orgDdlData, setOrgDdlData] = useState([])
    // 所選執行機關
    const [orgId, setOrgId] = useState("")
    // 所選行政區
    const [town, setTown] = useState({
        townId: "",
        townName: "全市"
    })
    // 統計年
    const [DAB_YEAR_YYY, setDAB_YEAR_YYY] = useState("");
    // 統計年月
    const year = useRef("");
    const month = useRef("");

    // 件數統計資料
    const [progressData, setProgressData] = useState({
        STAGE_E1_NUM: 0,
        STAGE_E2_NUM: 0,
        STAGE_E3_NUM: 0,
        STAGE_E1_DELAY_NUM: 0,
        STAGE_E2_DELAY_NUM: 0,
        STAGE_E3_DELAY_NUM: 0,
        LastTwoYearsFinishCnt: 0,
        EstimateFinishCnt: 0,
        ActualFinishCnt: 0
    })

    // 工程進度顯示資料
    const progressViewData = [
        { icon: setttings, title: "尚未開工案件數", item1: progressData.STAGE_E1_NUM, item2: progressData.STAGE_E1_DELAY_NUM },
        { icon: inProgress, title: "施工中案件數", item1: progressData.STAGE_E2_NUM, item2: progressData.STAGE_E2_DELAY_NUM },
        { icon: wrench, title: "近一年已完工案件數", item1: progressData.STAGE_E3_NUM, item2: progressData.STAGE_E3_DELAY_NUM },
        { icon: beOnTime, title: `${DAB_YEAR_YYY - 1}年度至今已完工案件數`, item1: progressData.LastTwoYearsFinishCnt },
        { icon: ontime, title: `${DAB_YEAR_YYY}當年度應完工案件數`, item1: progressData.EstimateFinishCnt, item2: progressData.ActualFinishCnt },
    ]

    /**
     * 取得下拉清單資料
     */
    const loadDdlData = async () => {
        // 取得機關清單資料
        let orgDdlData = [];
        let res = await getOrganList();
        if (res.ok)
            orgDdlData = await res.json();

        orgDdlData.unshift({ text: "桃園市政府", value: "" });
        setOrgDdlData(orgDdlData);
        setOrgId(orgDdlData[0].value);
    }

    /**
     * 取得進度資料
     */
    const loadProgressData = async () => {
        SetMaskOnOff(true);
        const { DAB_YEAR_YYY, DAB_MONTH } = await getFillCompleteCycle();
        // 統計年月
        setDAB_YEAR_YYY(DAB_YEAR_YYY);
        year.current = DAB_YEAR_YYY;
        month.current = DAB_MONTH;
        // 取得進度資料
        const result = await getEngProgress({
            EXEC_ORGAN_C: orgId,
            TOWN_C: town.townId,
            DAB_YEAR_YYY: DAB_YEAR_YYY,
            DAB_MONTH: DAB_MONTH
        });

        if (result) {
            setProgressData(result);
        }
        SetMaskOnOff(false);
    }

    useEffect(() => {
        loadProgressData();
    }, [orgId, town.townId])

    useEffect(() => {
        loadDdlData();
    }, [])

    return (
        <PageContainer
            style={{
                overflow: 'auto', margin: '0 auto', width: '100%', maxWidth: '1100px'
            }}
            toolbar={
                <>
                    {/* 機關下拉清單 篩選條件 */}
                    <DropDownListWithValue
                        data={orgDdlData}
                        textField={"text"}
                        dataItemKey={"value"}
                        value={orgId}
                        onChange={(e) => {
                            setOrgId(e.target.value);
                        }}
                    />
                </>
            }
        >
            <div style={{ display: 'flex', height: '100%' }} className={style.executionContainer}>
                <div style={{ position: 'relative' }}>
                    <div className={style.chartTitle}>
                        各行政區工程進度
                    </div>
                    <div><button className={style.dashBoardBtn}
                        onClick={() => { setTown({ townId: "", townName: "全市" }) }}>看全市</button></div>
                    <img src={map} alt='' height='500px' />
                    {
                        getProgressMapTownPosition.map((x) => {
                            let buttonStyle = { top: `${x.top}`, right: `${x.right}` };
                            // 桃園區點選判定範圍與其他縣市不一樣
                            if (x.townId == "H02") {
                                buttonStyle = {
                                    ...buttonStyle, width: "30px", height: "32px"
                                };
                            }
                            return (
                                <div className={style.townHiddenButton}
                                    style={buttonStyle}
                                    onClick={() => {
                                        setTown({
                                            townId: x.townId,
                                            townName: x.name
                                        })
                                    }}>
                                </div>)
                        })
                    }
                </div>

                <div style={{ margin: '0px 100px' }}>
                    <div className={style.titleBar}>
                        <span style={{ fontSize: '1.6rem' }}>{town.townName}</span><span style={{ fontSize: '1.2rem' }}>工程進度</span>
                    </div>
                    {
                        progressViewData.map((item) =>
                            <div>
                                <div className={style.board}>
                                    <div style={{ height: "50px", "width": "80px" }}>
                                        <img src={item.icon} className={style.icon} />
                                    </div>
                                    <span className={style.description}>
                                        <big >{item.title}</big> <br />
                                        <span className={style.highlights}>{item.item1}</span>件
                                        {item.hasOwnProperty('item2') && <>
                                            &nbsp;|&nbsp;落後
                                            <span className={style.highlights}>{item.item2}</span>件
                                        </>}
                                    </span>
                                </div>
                            </div>
                        )}
                </div>
                <div style={{
                    marginRight: '50px',
                    position: 'absolute',  // 使用絕對定位
                    right: '0',            // 將容器靠視窗的右邊
                    display: 'flex',       // 使用 Flexbox
                    flexDirection: 'column', // 設定為垂直排列
                }}>
                    <span
                        onClick={() => getStatisticsRPT(year.current, month.current, 11)}
                        className={style.reportLink}
                    >
                        各行政區工程進度(簡表)
                    </span>
                    <span
                        onClick={() => getStatisticsRPT(year.current, month.current, 12)}
                        className={style.reportLink}
                    >
                        各行政區工程進度(詳表)
                    </span>
                </div>
            </div>
        </PageContainer >
    )
}

export default DashBoardProgress;
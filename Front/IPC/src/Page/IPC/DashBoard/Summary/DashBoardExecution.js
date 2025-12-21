import React, { useEffect, useState, useRef } from 'react';
import { PageContainer } from '../../../../Basic/PageContainer';
import { GetSetParam, getOrganList } from '../../../../Basic/CommonService';
import { SetMaskOnOff } from '../../../../Basic/SDOExtension';
import 'hammerjs';
import map from '../../../../Images/map.jpg';
import '../../../../Css/Map.css';
import style from '../../../../Css/custom/DashBoard.module.css';
import { Button } from '@progress/kendo-react-buttons';
import { buildKindColorScheme, formatBudget, getDashBoardSummary, getFillCompleteCycle, getMapData, getOrgProjectSummary, selectedColor, unselectedColor, getStatisticsRPT, exportDashboardRPT } from '../DashBoardService';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';
import DelayLineChart from './DelayLineChart';
import DelayRankCharts from './DelayRankChart';
import TownDelaySection from './TownDelaySection';
import StatisticsService from '../../Statistics/StatisticsService'
import DelayListWindow from './DelayListWindow';
import DelayProjectListWindow from './ProjectDelayListWindow';
import CollapseBoardCard from '../../../../Components/BoardCard/CollapseBoardCard';
import '../../../../Css/ProjectPrint.css';

const DashBoardExecution = (props) => {

    // 行政區統計資料
    const [mapData, setMapData] = useState([]);
    // 建設類別統計資料
    const [buildKindData, setBuildKindData] = useState([]);
    // 建設類別代碼、icon資料
    const buildKindSettings = useRef([])
    // 件數、經費統計資料
    const budgetCountData = useRef([])
    // 執行機關下拉清單資料
    const [orgDdlData, setOrgDdlData] = useState([]);
    // 是否為 「執行中列管情形」頁面
    const [isProgressPage, setIsProgressPage] = useState(window.location.href.includes('Execution'));

    // 儀表統計資料 (非機關統計資料)
    const [dashBoardSummary, setDashBoardSummary] = useState({
        AvgDelayRate: 0,             // 全府落後比率
        LastYearAvgDelayRate: 0,     // 前一年全府平均落後比率
        DelayRankData: {
            Count: [],  // 落後件數排名
            Rate: [],   // 落後比率排名
            Summary: [] // 落後綜合排名
        }
    })
    // 執行機關代碼
    const [orgId, setOrgId] = useState("");
    const [orgName, setOrgName] = useState("")

    // 機關統計資料
    const [orgSummary, setOrgSummary] = useState({
        ProjectCount: 0,            // 列管件數
        ProjectCountDiff: 0,        // 列管件數較上月差異
        BudgetTotal: 0,             // 總經費
        BudgetTotalDiff: 0,         // 總經費較上月差異
        DelayCount: 0,              // 落後件數
        DelayCountDiff: 0,          // 落後件數計上月差異
        E1DelayCount: 0,            // 規劃中落後件數
        E2DelayCount: 0,            // 施工中落後件數
        E3DelayCount: 0,            // 驗收中落後件數
        DelayRate: 0,               // 落後比率
        DelayRateDiff: 0,           // 落後比率差異
        DelayOver3MonthsRate: 0,     // 連續落後3個月以上案件比率
        DelayOver3MonthsRateDiff: 0, // 連續落後3個月以上案件比率差異
        AffectedSubsidyNum: 0        // 可能影響補助款件數
    })

    // 落後比率折線圖資料
    const [delayRateChartData, setDelayRateChartData] = useState([])
    // 行政區落後統計資料
    const [townDelayData, settownDelayData] = useState([]);
    // 統計類別:件數(count)/經費(budget)
    const [statType, setStatType] = useState("count")
    // 統計年月
    const [statCycle, setStatCycle] = useState({
        DAB_YEAR_YYY: "",
        DAB_MONTH: ""
    })
    // 落後件數清單window
    const [delayListVisible, setDelayListVisible] = useState(false)
    // 落後案件清單window
    const [delayProjectListVisible, setDelayProjectListVisible] = useState(false)
    // window名稱
    const [windowName, setWindowName] = useState("")
    // 落後情形
    const [type, setType] = useState("")
    // 圖表資料
    const chartData = useRef([])

    /**
     * 取得頁面資料
     */
    const loadData = async () => {
        SetMaskOnOff(true);

        // 取得統計年月
        let statCycle = await getFillCompleteCycle();
        // statCycle.current = { DAB_YEAR_YYY: statCycle.DAB_YEAR_YYY, DAB_MONTH: statCycle.DAB_MONTH };
        setStatCycle({ DAB_YEAR_YYY: statCycle.DAB_YEAR_YYY, DAB_MONTH: statCycle.DAB_MONTH })

        // 取得機關清單資料
        let orgDdlData = [];
        let res = await getOrganList();
        if (res.ok)
            orgDdlData = await res.json();

        orgDdlData.unshift({ text: "桃園市政府", value: "" });
        setOrgDdlData(orgDdlData);
        setOrgId(orgDdlData[0].value);
        // 建設類別種類資料
        buildKindSettings.current = await GetSetParam("COM_PLANKIND");

        // 取儀錶板所有資料
        let result = await getDashBoardSummary({
            DAB_YEAR_YYY: statCycle.DAB_YEAR_YYY,
            DAB_MONTH: statCycle.DAB_MONTH,
            EXEC_ORGAN_C: orgId,
            IS_IN_PROGRESS_DATA: isProgressPage
        });

        if (result) {
            handleBudgetCountData(result.BudgetCountData);
            budgetCountData.current = result.BudgetCountData;
            setDashBoardSummary({
                ...result,
                DelayRankData: {
                    Count: sortAndTopN(result.OrgDelayRankData, "F1", false, 5),
                    Rate: sortAndTopN(result.OrgDelayRankData, "F2", false, 5),
                    Summary: sortAndTopN(result.OrgDelayRankData, "DELAY_F1F2", false, 5),
                }
            });
        }
    }

    /**
     * 處理行政區及建設類別統計資料
     * @param {*} data 
     */
    const handleBudgetCountData = async (data) => {
        data = data.map(x => {
            // 找出建設類別對應icon
            let buildKindIcon = buildKindSettings.current.find(item => item.SET_TYPE == x.SET_TYPE)?.MEMO;
            return {
                ...x,
                value: statType == "count" ? x.TOTAL_NUM : formatBudget(x.BUDGET_TOTAL),
                icon: buildKindIcon
            }
        });
        // 行政區資料
        let townData = data.filter(x => x.DAB_KIND == "B");
        setMapData([...getMapData(townData)]);

        // 取統計結果前10建設類別資料
        let buildKindData = sortAndTopN(data.filter(x => x.DAB_KIND == "C"), "value", true, 10);
        setBuildKindData(buildKindData);
    }

    /**
     * 排序並取得前N筆資料
     * @param {*} objArray 物件陣列
     * @param {*} propName 排序屬性名稱
     * @param {*} isRaising 是否降冪排序
     * @param {*} fetchNum 前N筆資料
     * @returns 
     */
    const sortAndTopN = (objArray, propName, isDescending, fetchNum) => {
        if (objArray && objArray.length == 0)
            return objArray;

        return objArray.sort((a, b) => isDescending ?
            parseFloat(b[propName]) - parseFloat(a[propName])
            : parseFloat(a[propName]) - parseFloat(b[propName]))
            .slice(0, fetchNum);
    }

    /**
     * 取得機關統計資料
     */
    const getOrgSummary = async () => {
        SetMaskOnOff(true);

        // 取得統計年月
        let statCycle = await getFillCompleteCycle();

        let orgSummary = await getOrgProjectSummary({
            DAB_YEAR_YYY: statCycle.DAB_YEAR_YYY,
            DAB_MONTH: statCycle.DAB_MONTH,
            EXEC_ORGAN_C: orgId,
            IS_IN_PROGRESS_DATA: isProgressPage
        })

        if (orgSummary) {
            // 各機關近一年落後比率
            setDelayRateChartData(orgSummary.AnnualOrgDelayRateData)

            let categoryDelayData = orgSummary.ProjectCategoryDelayData;
            setOrgSummary({
                ...orgSummary,
                E1DelayCount: categoryDelayData.filter(x => x.PROJECT_CATEGORY == "E1").length,
                E2DelayCount: categoryDelayData.filter(x => x.PROJECT_CATEGORY == "E2").length,
                E3DelayCount: categoryDelayData.filter(x => x.PROJECT_CATEGORY == "E3").length
            })
            // 行政區落後資料
            settownDelayData(orgSummary.TownDelayData)
        }

        SetMaskOnOff(false);
    }

    /**
     * 回傳差異組合文字
     * @param {*} val 
     * @returns 
     */
    const formatDiff = (val) => {
        if (val >= 0)
            return `增加${val}`;
        else
            return `減少${val}`;
    }

    /**
     * 加總物件陣列中指定屬性
     * @param {*} arr 
     * @param {*} prop 
     * @returns 
     */
    const sumArrObjProp = (arr, prop) => {
        return arr.reduce(function (a, b) {
            return a + b[prop];
        }, 0);
    }

    useEffect(() => {
        loadData();
    }, [])

    useEffect(() => {
        getOrgSummary();
    }, [orgId])

    useEffect(() => {
        handleBudgetCountData(budgetCountData.current)
    }, [statType])

    /**
     * 開啟清單視窗
     * @param {*} title 
     * @param {*} state 
     */
    const Openwindow = (title, state) => {
        setWindowName(title);
        setType(state);
        setDelayProjectListVisible(true);
    }

    return (
        <PageContainer
            style={{ overflow: 'auto' }}
            toolbar={
                <>
                    {/* 機關下拉清單 篩選條件 */}
                    {
                        isProgressPage &&
                        <DropDownListWithValue
                            data={orgDdlData}
                            textField={"text"}
                            dataItemKey={"value"}
                            value={orgId}
                            onChange={(e) => {
                                setOrgId(e.target.value);
                                setOrgName(e.target.text);
                            }}
                        />
                    }
                </>
            }
        >
            <div className={style.titleBar} style={{ display: 'flex', alignItems: 'center' }}>
                <div style={{ display: 'inline-flex' }}>
                    <div className={style.period}>{`${statCycle.DAB_YEAR_YYY}/${statCycle.DAB_MONTH}`}</div>
                    <div className={style.description}>列管
                        <span
                            className={`${style.highlights} ${style['cursor-pointer']}`}
                        >
                            {orgSummary.ProjectCount}
                        </span> 件
                        <br />
                        (較上月{formatDiff(orgSummary.ProjectCountDiff)}件)
                    </div>
                </div>
                {/* seperator */}
                <div className={style.seperator}></div>
                <div >
                    <div className={style.description}>
                        總經費 <span className={style.highlights}>{orgSummary.BudgetTotal}</span> 億元
                        <br />
                        (較上月{formatDiff(orgSummary.BudgetTotalDiff)}億元)
                    </div>
                </div>
                <div style={{
                    marginLeft: 'auto'
                }}>
                    <span
                        onClick={() => getStatisticsRPT(statCycle.DAB_YEAR_YYY, statCycle.DAB_MONTH, 1, "", false)}
                        className={style.reportLink}
                        style={{ backgroundColor: '#409BCA' }}
                    >
                        列管情形報表
                    </span>
                </div>
            </div>
            {/* 件數 / 經費切換按鈕 */}
            <div style={{ marginLeft: '140px' }}>
                <button className={style.toggleButton} style={{
                    backgroundColor: statType == "count" ? selectedColor : unselectedColor
                }}
                    onClick={() => setStatType('count')}>件數</button>
                <button className={style.toggleButton} style={{
                    backgroundColor: statType == "budget" ? selectedColor : unselectedColor
                }}
                    onClick={() => setStatType('budget')}>經費</button>
            </div>
            <div style={{ display: 'flex', height: '1000px', width: '1470px', marginLeft: '140px' }}>
                <div style={{ position: 'relative' }}>
                    <div className={style.chartTitle}>
                        {statType == 'count' ? '各行政區列管數(件)' : '各行政區列管經費(億元)'}
                    </div>
                    <img src={map} alt='' height='500px' />
                    {
                        mapData.map((x) => (
                            <div className={style.regionImgLabel}
                                style={{ top: `${x.top}`, right: `${x.right}` }}>
                                {x.value}</div>
                        ))
                    }
                </div>
                <div style={{ marginLeft: '100px' }} >
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                        <div>
                            <div className={style.chartTitle}>{statType == 'count' ? '建設類別列管件數前10名' : '建設類別列管經費數前10名'}</div>
                            <div className={style.gridContainer}>

                                {buildKindData.map((x, idx) => {
                                    return (
                                        <div className={style.gridBox}
                                            style={{ backgroundColor: `${buildKindColorScheme[idx]}` }}>
                                            <i className={`fa ${x.icon} fa-2x`} />
                                            <span style={{ fontSize: "0.8rem", padding: "2px 0" }}>{x.SET_VALUE}</span>
                                            <div style={{ backgroundColor: "#fff", height: "1px", width: "100%" }}></div>
                                            <b>{parseFloat(x.value).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</b>
                                        </div>)
                                })}
                            </div>
                        </div>
                        <div style={{ marginLeft: '60px', marginTop: '250px' }}>
                            <span
                                onClick={() => exportDashboardRPT(chartData.current)}
                                className={style.reportLink}
                            >
                                落後比率圖表
                            </span>
                        </div>
                    </div>
                    {/* 折線圖 */}
                    <DelayLineChart data={delayRateChartData} orgId={orgId} orgName={orgName} chartData={chartData} />
                </div>
            </div>
            {/* 落後件數、落後比率統計區塊 */}
            <div className={style.titleBar} style={{ height: '100px', display: 'flex', alignItems: 'center' }}>
                <div style={{ display: "inline-flex", minWidth: "290px" }}>
                    <div className={style.description} style={{ marginTop: '10px' }}> 落後
                        <span
                            className={`${style.highlights} ${style['cursor-pointer']}`}
                            onClick={() => setDelayListVisible(true)}
                            title="點選查看清單"
                        >
                            {orgSummary.DelayCount}
                        </span> 件
                        <br />
                        (較上月{formatDiff(orgSummary.DelayCountDiff)}件)
                    </div>
                    <div>
                        <ul>
                            <li> 規劃落後{orgSummary.E1DelayCount}件</li>
                            <li> 施工落後{orgSummary.E2DelayCount}件</li>
                            <li> 驗收落後{orgSummary.E3DelayCount}件</li>
                        </ul>
                    </div>
                </div>
                {/* seperator */}
                <div className={style.seperator}></div>
                <div style={{ display: "inline-flex", minWidth: "600px" }}>
                    <div className={style.description} style={{ marginTop: '10px' }}>
                        落後比率 <span className={style.highlights} >{orgSummary.DelayRate}</span> %
                        <br />
                        (較上月{formatDiff(orgSummary.DelayRateDiff)}%)
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'center' }}>
                        <ul>
                            <li> 全府落後比率{dashBoardSummary.AvgDelayRate}%</li>
                            <li>
                                連續落後3個月以上案件比率{orgSummary.DelayOver3MonthsRate}%
                                (較上月{formatDiff(orgSummary.DelayOver3MonthsRateDiff)}%)
                            </li>
                            <li> 較{new Date().getFullYear() - 1912}年度平均落後比率{dashBoardSummary.LastYearAvgDelayRate}%
                                ：{formatDiff((orgSummary.DelayRate - dashBoardSummary.LastYearAvgDelayRate).toFixed(2))}%
                            </li>
                        </ul>
                    </div>
                </div>
                <div style={{ marginLeft: 'auto' }}>
                    <span
                        onClick={() => getStatisticsRPT(statCycle.DAB_YEAR_YYY, statCycle.DAB_MONTH, 2, "", false)}
                        className={style.reportLink}
                        style={{ backgroundColor: '#409BCA' }}
                    >
                        落後情形報表
                    </span>
                </div>
            </div>
            {/* 落後統計區塊 */}
            <div className={style.titleBar} style={{ height: '200px' }}>
                <div className={style.delayContainer}>
                    <div className={style.inner}>
                        <b>工進落後</b>
                        <ul>
                            <li>未達10%：
                                <span
                                    style={{ color: "red" }}
                                    className={`${style.highlights} ${style['cursor-pointer']}`}
                                    onClick={() => Openwindow("進度落後(未達10%)", "EngDelayBelow10Cnt")}
                                    title="點選查看清單"
                                >
                                    {sumArrObjProp(townDelayData, "EngDelayBelow10Cnt")}</span> 件</li>
                            <li>10%以上，未達20%：
                                <span
                                    style={{ color: "red" }}
                                    className={`${style.highlights} ${style['cursor-pointer']}`}
                                    onClick={() => Openwindow("進度落後(10%以上，未達20%)", "EngDelayBelow20Cnt")}
                                    title="點選查看清單"
                                >
                                    {sumArrObjProp(townDelayData, "EngDelayBelow20Cnt")}</span> 件</li>
                            <li>20%以上：
                                <span
                                    style={{ color: "red" }}
                                    className={`${style.highlights} ${style['cursor-pointer']}`}
                                    onClick={() => Openwindow("進度落後(20%以上)", "EngDelayOver20Cnt")}
                                    title="點選查看清單"
                                >
                                    {sumArrObjProp(townDelayData, "EngDelayOver20Cnt")}</span>件</li>
                        </ul>
                    </div>

                    <div className={style.seperator}></div>
                    <div className={style.inner}>
                        <b>檢核點落後</b>
                        <ul>
                            <li>未達3個月：
                                <span
                                    style={{ color: "red" }}
                                    className={`${style.highlights} ${style['cursor-pointer']}`}
                                    onClick={() => Openwindow("檢核點落後(未達3個月)", "ChkPtDelayBelow3MonthsCnt")}
                                    title="點選查看清單"
                                >
                                    {sumArrObjProp(townDelayData, "ChkPtDelayBelow3MonthsCnt")}
                                </span>件</li>
                            <li>三個月以上，未達6個月：
                                <span
                                    style={{ color: "red" }}
                                    className={`${style.highlights} ${style['cursor-pointer']}`}
                                    onClick={() => Openwindow("檢核點落後(三個月以上，未達6個月)", "ChkPtDelayBelow6MonthsCnt")}
                                    title="點選查看清單"
                                >
                                    {sumArrObjProp(townDelayData, "ChkPtDelayBelow6MonthsCnt")}
                                </span>件</li>
                            <li>6個月以上：
                                <span
                                    style={{ color: "red" }}
                                    className={`${style.highlights} ${style['cursor-pointer']}`}
                                    onClick={() => Openwindow("檢核點落後(6個月以上)", "ChkPtDelayOver6MonthsCnt")}
                                    title="點選查看清單"
                                >
                                    {sumArrObjProp(townDelayData, "ChkPtDelayOver6MonthsCnt")}
                                </span>件</li>
                        </ul>
                    </div>

                    <div className={style.seperator}></div>
                    <div className={style.inner}>
                        <b>可能影響補助款</b>
                        <ul>
                            <li>
                                <span
                                    style={{ color: "red" }}
                                    className={`${style.highlights} ${style['cursor-pointer']}`}
                                    onClick={() => Openwindow("可能影響補助款", "AffectedSubsidyNum")}
                                    title="點選查看清單"
                                >
                                    {orgSummary.AffectedSubsidyNum}
                                </span>件</li>
                        </ul>
                    </div>
                </div>
            </div>
            {/* 行政區落後統計區塊 */}
            <TownDelaySection
                data={orgSummary.TownDelayData}
                DAB_YEAR_YYY={statCycle.DAB_YEAR_YYY}
                DAB_MONTH={statCycle.DAB_MONTH}
            />
            <div style={{
                width: '1600px', display: 'flex', alignItems: 'center', flexDirection: 'column' // 垂直排列
            }}>
                <div style={{ marginLeft: 'auto', marginRight: '20px' }}>
                    <span
                        onClick={() => getStatisticsRPT(statCycle.DAB_YEAR_YYY, statCycle.DAB_MONTH, 4, "Statistics", false)}
                        className={style.reportLink}
                    >
                        落後排名報表
                    </span>
                </div>
                <div style={{ marginLeft: 'auto', marginTop: '10px', marginRight: '20px' }}>
                    <span
                        onClick={() => getStatisticsRPT(statCycle.DAB_YEAR_YYY, statCycle.DAB_MONTH, 5, "Delay", false)}
                        className={style.reportLink}
                    >
                        落後案件清單
                    </span>
                </div>
            </div>
            {/* 落後統計資料區塊 */}
            <DelayRankCharts data={dashBoardSummary.DelayRankData} />
            {/* 落後件數清單 */}
            <DelayListWindow
                visible={delayListVisible}
                onClose={() => { setDelayListVisible(false); }}
                DAB_YEAR_YYY={statCycle.DAB_YEAR_YYY}
                // DAB_MONTH={statCycle.DAB_MONTH}
                DAB_MONTH={"05"}
            />
            {/* 落後各案件清單(進度/檢核點/可能影響補助款) */}
            <DelayProjectListWindow
                visible={delayProjectListVisible}
                onClose={() => { setDelayProjectListVisible(false); }}
                DAB_YEAR_YYY={statCycle.DAB_YEAR_YYY}
                DAB_MONTH={statCycle.DAB_MONTH}
                name={windowName}
                Type={type}
            />
        </PageContainer >
    )
}

export default DashBoardExecution;
import React, { useEffect, useState, useRef } from 'react';
import 'hammerjs';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';
import TrendChart from './TrendChart';
import { PageContainer } from '../../../../Basic/PageContainer';
import { SetMaskOnOff } from '../../../../Basic/SDOExtension';
import { getOrganList } from '../../../../Basic/CommonService';
import { formatBudget, getPastYearsData, exportDashboardRPT } from '../DashBoardService';
import style from '../../../../Css/custom/DashBoard.module.css';

// 歷年列管情形 Main Component
const DashBoardHistoryMain = (props) => {
    // 所選執行機關
    const [orgId, setOrgId] = useState("")
    // 機關下拉清單資料
    const [orgDdlData, setOrgDdlData] = useState([])
    // 機關下拉清單資料
    const [categories, setCategories] = useState([])
    // 報表資料
    const [chartData, setChartData] = useState({
        count: [],
        budget: [],
        delayRate: []
    })
    // 報表資料
    const chartDataRef = useRef(null);

    /**
     * 取得機關資料
     */
    const loadOrgData = async () => {
        // 取得機關清單資料
        let orgDdlData = [];
        let res = await getOrganList();
        if (res.ok)
            orgDdlData = await res.json();
        // 加入桃園市政府
        orgDdlData.unshift({ text: "桃園市政府", value: "" });

        setOrgDdlData(orgDdlData);
        setOrgId(orgDdlData && orgDdlData.length > 0 ?
            orgDdlData[0].value : "")
    }

    /**
     * 取得歷年執行情形資料
     */
    const loadPastYearsData = async (orgId) => {
        let result = await getPastYearsData(orgId);
        if (result && result.length) {
            setCategories(result.map(x => { return `${x.DAB_YEAR_YYY}年` }));
            setChartData({
                count: result.map(x => x.TOTAL_NUM),
                budget: result.map(x => x.TOTAL_EXS),
                delayRate: result.map(x => x.DELAY_RATE)
            });
        }

        // 更新 chartDataRef
        chartDataRef.current = {
            STATISTICS_ID: 17,  // 報表代碼
            STATISTICS_NAME: `歷年列管圖表`,
            LineChartModels: [
                {
                    ChartRow: result.map(x => `${x.DAB_YEAR_YYY}年`),
                    ColumnName: "歷年案件數",
                    ChartColumn: "(單位：件)",
                    ChartValue1: result.map(x => x.TOTAL_NUM),
                    ChartType: "column"
                },
                {
                    ChartRow: result.map(x => `${x.DAB_YEAR_YYY}年`),
                    ColumnName: "歷年總經費",
                    ChartColumn: "(單位：億元)",
                    ChartValue1: result.map(x => parseFloat(formatBudget(x.TOTAL_EXS))),
                    ChartType: "column"
                },
                {
                    ChartRow: result.map(x => `${x.DAB_YEAR_YYY}年`),
                    ColumnName: "歷年平均落後比率",
                    ChartColumn: "(單位：%)",
                    ColumnStart: 0,
                    ColumnEnd: 100,
                    ChartValue1: result.map(x => x.DELAY_RATE),
                    ChartType: "line"
                }
            ],
        };
    }

    useEffect(() => {
        SetMaskOnOff(true);
        loadOrgData();
    }, [])

    useEffect(() => {
        SetMaskOnOff(true);
        loadPastYearsData(orgId);
        SetMaskOnOff(false);
    }, [orgId])


    return (
        <PageContainer style={{ overflow: 'auto' }}>
            {/* 機關下拉清單 篩選條件 */}
            <div style={{ display: 'flex', alignItems: 'center' }}>
                <DropDownListWithValue
                    data={orgDdlData}
                    textField={"text"}
                    dataItemKey={"value"}
                    value={orgId}
                    onChange={(e) => {
                        setOrgId(e.target.value);
                    }}
                />
                <div style={{
                    marginTop: '5px',
                    marginRight: '50px',
                    position: 'absolute',  // 使用絕對定位
                    right: '0',            // 將容器靠視窗的右邊
                }}>
                    <span
                        onClick={() => { exportDashboardRPT(chartDataRef.current) }}
                        className={style.reportLink}
                    >
                        歷年列管圖表
                    </span>
                </div>
            </div>
            <TrendChart chartTitle={"歷年立案件數趨勢(單位：件)"}
                chartType={'column'}
                categories={categories}
                data={chartData.count} />
            <TrendChart chartTitle={"歷年總經費趨勢(單位：億元)"}
                chartType={'column'}
                categories={categories}
                data={chartData.budget.map(x => { return formatBudget(x) })} />
            <TrendChart chartTitle={"歷年平均落後比率趨勢(單位：%)"}
                chartType={'line'}
                categories={categories}
                data={chartData.delayRate} />
        </PageContainer>
    );

}
export default DashBoardHistoryMain;
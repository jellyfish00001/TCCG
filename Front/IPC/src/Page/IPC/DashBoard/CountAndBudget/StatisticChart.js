import React, { useEffect, useRef, useState } from 'react';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem, ChartValueAxis, ChartValueAxisItem, ChartTitle, ChartCategoryAxisTitle } from '@progress/kendo-react-charts';
import 'hammerjs';
import style from '../../../../Css/custom/DashBoard.module.css';
import { formatBudget, getDashBoardCountAndBudget, getFillCompleteCycle, exportDashboardRPT } from '../DashBoardService';
import { SetMaskOnOff } from '../../../../Basic/SDOExtension';
import { getStatisticsRPT } from './../DashBoardService';

// 件數及經費情形 圖表 Component
const StatisticChart = (props) => {
    const { dabKind, type, projectStatusType, setProjectStatusType } = props
    // 統計項目
    const [categories, setCategories] = useState({
        count: [],
        budget: []
    })
    // 加载狀態
    const isLoading = useRef(true);

    // 件數/經費資料
    const [chartData, setChartData] = useState({
        count: [],
        budget: []
    })
    // 選取顏色
    const selectedColor = 'rgb(39, 133, 158)';
    const unselectedColor = 'rgb(98, 182, 183)';
    // 統計年月
    const year = useRef("");
    const month = useRef("");
    // 報表資料
    const chartDataRef = useRef(null);

    // 取得統計資料
    const loadData = async () => {
        SetMaskOnOff(true);
        let { DAB_YEAR_YYY, DAB_MONTH } = await getFillCompleteCycle();
        let result = await getDashBoardCountAndBudget({
            DAB_YEAR_YYY,
            DAB_MONTH,
            DAB_KIND: dabKind,
            ProjectStatusType: projectStatusType,
        });

        // 設定年月(方便報表查詢)
        year.current = DAB_YEAR_YYY;
        month.current = DAB_MONTH;

        if (result && result.length > 0) {
            const sortedCountData = [...result].sort((a, b) => b.TOTAL_NUM - a.TOTAL_NUM);
            const sortedBudgetData = [...result].sort((a, b) => b.BUDGET_TOTAL - a.BUDGET_TOTAL);

            setCategories({
                count: sortedCountData.map(x => x.SET_VALUE),
                budget: sortedBudgetData.map(x => x.SET_VALUE),
            });
            setChartData({
                count: sortedCountData.map(x => x.TOTAL_NUM),
                budget: sortedBudgetData.map(x => formatBudget(x.BUDGET_TOTAL)),
            });

            // 更新 chartDataRef
            chartDataRef.current = {
                STATISTICS_ID: 16,  // 新的報表代碼
                STATISTICS_NAME: `${type} 件數及經費報表`,  // 報表名稱
                LineChartModels: [
                    {
                        ChartRow: sortedCountData.map(x => x.SET_VALUE),
                        ColumnName: `${type}列管件數`,
                        ChartColumn: `(單位：件)`,
                        ColumnStart: 0,
                        // 取最大值做為圖表結束值
                        ColumnEnd: Math.ceil(Math.max(...sortedCountData.map(x => x.TOTAL_NUM))) + 1,
                        ChartValue1: sortedCountData.map(x => x.TOTAL_NUM),
                    },
                    {
                        ChartRow: sortedBudgetData.map(x => x.SET_VALUE),
                        ColumnName: `${type}列管經費`,
                        ChartColumn: `(單位：億元)`,
                        ColumnStart: 0,
                        // 取最大值做為圖表結束值
                        ColumnEnd: Math.ceil(Math.max(...sortedBudgetData.map(x => formatBudget(x.BUDGET_TOTAL)))) + 1,
                        ChartValue1: sortedBudgetData.map(x => formatBudget(x.BUDGET_TOTAL)),
                    },
                ],
            };
        }
        isLoading.current = false;
        SetMaskOnOff(false);
    };

    useEffect(() => {
        setCategories({});
        setChartData({});
        loadData();
    }, [])

    useEffect(() => {
        setCategories({});
        setChartData({});
        loadData();
    }, [projectStatusType])

    return (
        <>
            <div className={style.toggleButtonContainer} >
                <button className={style.toggleButton}
                    style={{ backgroundColor: projectStatusType == "1" ? selectedColor : unselectedColor }}
                    onClick={() => setProjectStatusType("1")}
                >當年度列管</button>
                <button className={style.toggleButton}
                    style={{ backgroundColor: projectStatusType == "2" ? selectedColor : unselectedColor }}
                    onClick={() => setProjectStatusType("2")}
                >執行中</button>
            </div>
            <div style={{
                marginTop: '5px',
                display: 'flex',       // 使用 Flexbox
                flexDirection: 'column', // 設定為垂直排列
                alignItems: 'flex-end'  // 讓內部項目向右對齊
            }}>
                {projectStatusType == '1' &&
                    <span
                        onClick={() => getStatisticsRPT(year.current, month.current, 1)}
                        className={style.reportLink}
                    >
                        列管情形報表
                    </span>
                }
                {projectStatusType == '2' &&
                    <span
                        onClick={() => getStatisticsRPT(year.current, month.current, 1)}
                        className={style.reportLink}
                    >
                        列管情形報表
                    </span>
                }<span
                    onClick={() => { exportDashboardRPT(chartDataRef.current) }}
                    className={style.reportLink}
                >
                    列管情形圖表
                </span>
            </div>
            {!isLoading.current && (<>
                {/* 件數橫條圖 */}
                <Chart style={{ height: '800px' }}>
                    <ChartTitle text={`${type}列管件數(單位：件)`}
                        align='left' />
                    <ChartCategoryAxis>
                        <ChartCategoryAxisItem categories={categories.count}>
                        </ChartCategoryAxisItem>
                    </ChartCategoryAxis>
                    <ChartSeries>
                        <ChartSeriesItem type="bar" gap={2} spacing={0.25} data={chartData.count}
                            labels={{
                                visible: true,
                                position: 'outsideEnd',
                                content: (e) => { return e.value > 0 ? e.value : "" },
                                font: "bold 16px Arial, sans-serif",
                                background: 'none',
                            }} />
                    </ChartSeries>
                </Chart>
                {/* 經費橫條圖 */}
                <Chart style={{ height: '800px' }}>
                    <ChartTitle text={`${type}列管經費情形(單位：億元)`}
                        align='left' />
                    <ChartCategoryAxis>
                        <ChartCategoryAxisItem categories={categories.budget}>
                        </ChartCategoryAxisItem>
                    </ChartCategoryAxis>
                    <ChartSeries>
                        <ChartSeriesItem type="bar" color={'navy'} gap={2} spacing={0.25} data={chartData.budget}
                            labels={{
                                visible: true,
                                position: 'outsideEnd',
                                content: (e) => { return e.value > 0 ? e.value : "" },
                                font: "bold 16px Arial, sans-serif",
                                background: 'none'
                            }}
                        />
                    </ChartSeries>
                </Chart>
            </>
            )}
        </>
    );
}
export default StatisticChart;
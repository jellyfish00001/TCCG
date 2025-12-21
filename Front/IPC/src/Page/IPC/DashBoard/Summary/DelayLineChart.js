import React, { useEffect, useState, useRef } from 'react';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem, ChartValueAxis, ChartValueAxisItem } from '@progress/kendo-react-charts';
import 'hammerjs';
import style from '../../../../Css/custom/DashBoard.module.css';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { formatBudget, getFillCompleteCycle } from '../DashBoardService';

// 重要儀錶板 - 近一年落後比率折線圖
const DelayLineChart = (props) => {
    const { data, orgId, orgName, chartData } = props;

    // 全府落後資料
    const [values, setValues] = useState([]);

    // 圖表資料
    const [chartValues, setChartValues] = useState({
        all: [], // 全府資料
        orgData: [] // 機關資料
    });

    // 年月項目
    const categories = useRef([]);

    /**
     * 產生近一年 年月項目
     */
    const generateCategories = async () => {
        let { DAB_YEAR_YYY, DAB_MONTH } = await getFillCompleteCycle();

        let date = toDateTime(DAB_YEAR_YYY, DAB_MONTH);
        date.setFullYear(date.getFullYear() - 1);
        for (let i = 0; i < 12; i++) {
            date.setMonth(date.getMonth() + 1);
            categories.current.push(
                `${date.getFullYear() - 1911}年${(date.getMonth() + 1).toString().padStart(2, "0")}月`
            );
        }
    };

    /**
     * YYYMM轉為日期格式
     * @param {*} tYY 
     * @param {*} month 
     */
    const toDateTime = (tYY, month) => {
        return new Date(Number(tYY) + 1911, Number(month) - 1);
    };

    /**
     * 處理raw Data
     */
    const dataProcessing = () => {
        let avgDelayRates = [];
        let orgAvgDelayRates = [];
        // 計算每月平均落後比率
        categories.current.map(YM => {
            // 全府月落後比率資料
            let monthlyDelayRates = data.filter(item => `${item.DAB_YEAR_YYY}年${item.DAB_MONTH}月` === YM)
                ?.map(a => a.DELAY_RATE);
            // 全府平均落後比率
            avgDelayRates.push(countAvgDelayRate(monthlyDelayRates));

            // 計算 機關 每月平均落後比率
            if (!IsNullOrEmpty(orgId)) {
                let orgMonthlyDelayRates = data.filter(item =>
                    `${item.DAB_YEAR_YYY}年${item.DAB_MONTH}月` === YM && item.SET_TYPE === orgId)
                    ?.map(a => a.DELAY_RATE);
                orgAvgDelayRates.push(countAvgDelayRate(orgMonthlyDelayRates));
            }
        });
        setChartValues({
            all: avgDelayRates,
            orgData: orgAvgDelayRates
        });
    };

    /**
     * 計算平均落後比率
     * @param {Array} monthlyDelayRates 
     */
    const countAvgDelayRate = (monthlyDelayRates) => {
        return monthlyDelayRates && monthlyDelayRates.length > 0 ?
            (monthlyDelayRates.reduce((a, b) => a + b, 0) / monthlyDelayRates.length).toFixed(2)
            : 0;
    };

    /**
     * 自定義圖表項目
     * @param {*} values 
     * @param {*} name 
     * @returns 
     */
    const CustomChartSeriesItem = (values, name) => {
        return (
            <ChartSeriesItem
                type="line"
                data={values}
                name={name}
                labels={{
                    visible: true,
                    position: 'top',
                    content: (e) => { return e.value > 0 ? e.value : "" },
                    font: "bold 16px Arial, sans-serif"
                }}
            />
        );
    };

    // 資料處理
    useEffect(() => {
        if (data && data.length > 0)
            dataProcessing();
        else {
            setChartValues({
                all: new Array(12).fill(0),
                orgData: new Array(12).fill(0)
            });
        }
    }, [data]);

    // 產生年月項目
    useEffect(() => {
        if (categories.current.length === 0)
            generateCategories();
    }, []);

    // 打印最終數據
    useEffect(() => {
        if (categories.current.length > 0 && chartValues.all.length > 0) {
            // 整理圖表數據
            chartData.current = {
                STATISTICS_ID: 15,  // 報表代碼
                STATISTICS_NAME: "近一年落後比率",   // 報表名稱
                LineChartModels: [
                    {
                        ChartRow: categories.current, // 下方列值
                        ChartColumn: "百分比", // 右方欄
                        ColumnName: "近一年落後比率", // 欄位名稱
                        ColumnStart: 0, // 欄位起始值
                        ColumnEnd: 100, // 欄位結束值
                        ChartValue1: chartValues.all.map(value => parseFloat(value)) // 全府落後比率，轉為小數 圖表值
                    }
                ]
            };
        }
    }, [categories.current, chartValues]);

    return (
        <div>
            <div className={style.chartTitle}>近一年落後比率</div>
            <Chart style={{ width: '770px' }}>
                <ChartLegend position="bottom" orientation="horizontal" />
                <ChartCategoryAxis>
                    <ChartCategoryAxisItem categories={categories.current} />
                </ChartCategoryAxis>
                <ChartValueAxis>
                    <ChartValueAxisItem
                        labels={{
                            content: (e) => `${e.value}%`
                        }}
                        max={100}
                    />
                </ChartValueAxis>
                <ChartSeries>
                    {CustomChartSeriesItem(chartValues.all, "全府落後比率")}
                    {!IsNullOrEmpty(orgId) &&
                        CustomChartSeriesItem(chartValues.orgData, `${orgName}落後比率`)}
                </ChartSeries>
            </Chart>
        </div>
    );
};

export default DelayLineChart;

import React, { useEffect, useState, useRef } from 'react';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem } from '@progress/kendo-react-charts';
import 'hammerjs';
import style from '../../../../Css/custom/DashBoard.module.css';
import { formatBudget, getStatisticsRPT } from '../DashBoardService';

// 重要儀錶板 - 各區落後情形 行政區落後
const TownDelaySection = (props) => {
    const { data, DAB_YEAR_YYY, DAB_MONTH } = props

    /**
     * 處理raw Data
     */
    const dataProcessing = () => {
        // 機關落後件數總計
        let orgDelayCnt = data.reduce(function (a, b) {
            return a + sumDelayCnt(b);
        }, 0);

        // 計算各行政區落後比率
        data.forEach(x => {
            x.delayPercentage = orgDelayCnt == 0 ? 0 : (sumDelayCnt(x) / orgDelayCnt * 100).toFixed(2);
        });
    }

    /**
     * 加總個比率落後件數
     * @param {*} item 
     * @returns 
     */
    const sumDelayCnt = (item) => {
        return item.EngDelayBelow10Cnt + item.EngDelayBelow20Cnt + item.EngDelayOver20Cnt
    }

    /**
     * 落後比率對應背景色
     * @param {*} percentage 
     * @returns 
     */
    const delayDelayCardBackgroundColor = (percentage) => {
        if (percentage < 10) {
            return "rgb(254,210,209)";
        } else if (percentage >= 10 && percentage < 20) {
            return "rgb(255,151,150)";
        } else {
            return "rgb(255,81,82)";
        }
    }

    useEffect(() => {
        if (data && data.length > 0)
            dataProcessing();
    }, [data])

    return (
        <div>
            <div style={{ display: 'flex', alignItems: 'center', width: '1610px' }}>

                <div className={style.chartTitle}>各區落後情形</div>
                <div style={{ marginLeft: 'auto', marginRight: '15px' }}>
                    <span
                        onClick={() => getStatisticsRPT(DAB_YEAR_YYY, DAB_MONTH, 3)}
                        className={style.reportLink}
                    >
                        各區落後報表
                    </span>
                </div>
            </div>
            <div>淡色-落後件數比率未達10%：深色-落後件數比率10%以上，未達20%：最深色：落後件數比率20%以上</div>

            <div className={style.townDelayContainer}>
                {data && data.map(x => {
                    let bg = delayDelayCardBackgroundColor(x.delayPercentage);
                    return (
                        <div className={style.townDelayCard}
                            style={{ backgroundColor: bg }}>
                            {/* 地區名稱 */}
                            <div className={style.townTitle}>{x.TOWNNAME}</div>
                            {/* 中空部分 */}
                            <div className={style.townInnerBox}>
                                <p>
                                    ※工進落後：
                                    <ul style={{ listStyle: "none", padding: 0, margin: 0 }}>
                                        <li>未達10%：<span>{x.EngDelayBelow10Cnt}</span> 件</li>
                                        <li>10%以上，未達20%：<span>{x.EngDelayBelow20Cnt}</span>件</li>
                                        <li>20%以上：<span>{x.EngDelayOver20Cnt}</span>件</li>
                                    </ul>
                                    <br />
                                    ※檢核點落後：
                                    <ul>
                                        <li>未達3個月：<span>{x.ChkPtDelayBelow3MonthsCnt}</span>件</li>
                                        <li>3個月以上，未達6個月：<span>{x.ChkPtDelayBelow6MonthsCnt}</span>件</li>
                                        <li>6個月以上：<span>{x.ChkPtDelayOver6MonthsCnt}</span>件</li>
                                    </ul>
                                </p>

                            </div>
                        </div>
                    )
                })}
            </div>
        </div >
    )
}

export default TownDelaySection;
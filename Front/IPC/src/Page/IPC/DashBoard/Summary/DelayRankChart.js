import React, { useEffect, useState, useRef } from 'react';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem } from '@progress/kendo-react-charts';
import 'hammerjs';
import style from '../../../../Css/custom/DashBoard.module.css';
import '../../../../Css/Site.css'
import { formatBudget } from '../DashBoardService';

// 重要儀錶板 - 落後排名Component
const DelayRankCharts = (props) => {
    const { data } = props

    /**
     * 取得排序機關Jsx
     * @param {*} type 統計類別 
     * @param {*} dataItem 資料
     * @returns 
     */
    const getOrgRankListJsx = (type, data) => {
        // 落後件數排名
        if (type == "Count") {
            return data.map((item, idx) => {
                const { SET_VALUE: orgName, DELAY_NUM } = item;
                return (
                    <div className={style.listContainer}>
                        <div className={style.textSquare}
                            style={{ backgroundColor: 'rgb(178, 203, 127)' }}>
                            {idx + 1}
                        </div>
                        <div className={style.rightPart}>
                            <div className={style.textLocation} > {orgName}</div>
                            <div className={style.textSquare} style={{ backgroundColor: "rgb(155,187,89)" }}>
                                {DELAY_NUM}
                            </div>
                            <div>件</div>
                        </div>
                    </div >
                )
            })
            // 落後比率排名
        } else if (type == "Rate") {
            return data.map((item, idx) => {
                const { SET_VALUE: orgName, DELAY_RATE } = item;
                return (
                    <div className={style.listContainer}>
                        <div className={style.textSquare}
                            style={{ backgroundColor: 'rgb(169, 193, 223)' }}>
                            {idx + 1}
                        </div>
                        <div className={style.rightPart}>
                            <div className={style.textLocation}> {orgName}</div>
                            <div class={style.progressBar}>
                                <span style={{ width: `${DELAY_RATE}%` }}>
                                    {Number.isInteger(DELAY_RATE) ? `${DELAY_RATE}%` : `${parseFloat(DELAY_RATE).toFixed(1)}%`}
                                </span>
                            </div>
                        </div>
                    </div >
                )
            })
            // 落後綜合排名
        } else if (type == "Summary") {
            return data.map((item, idx) => {
                const { SET_VALUE: orgName } = item;
                return (
                    <div className={style.listContainer}>
                        <div className={style.textSquare}
                            style={{ backgroundColor: 'rgb(179,162,199)' }}>
                            {idx + 1}
                        </div>
                        <div className={style.rightPart}>
                            <div className={style.textLocation3}> {orgName}</div>
                        </div>
                    </div >
                )
            })
        }
    }

    return (
        <div className={style.delayRankChartsContainer}>
            {/* 落後件數排名 */}
            <div className={style.delayRankChartBox}>
                <div className={style.title}>
                    <b>落後件數排名</b>
                </div>
                <div className={style.listContainer}>
                    <span>&ensp;排名</span>
                    <span>執行機關</span>
                    <span>件數&ensp;</span>
                </div>
                <div className={style.seperator} style={{ backgroundColor: "rgb(155,187,89)" }} />
                {/* 機關清單 */}
                {getOrgRankListJsx('Count', data.Count)}
            </div>
            {/* 落後比率排名 */}
            <div className={style.delayRankChartBox}>
                <div className={style.title}>
                    <b>落後比率排名</b>
                </div>
                <div className={style.listContainer}>
                    <span>&ensp;排名</span>
                    <span>執行機關</span>
                    <span>比率&ensp;</span>
                </div>
                <div className={style.seperator} style={{ backgroundColor: "rgb(149, 179, 215)" }} />
                {getOrgRankListJsx('Rate', data.Rate)}
            </div>
            {/* 落後綜合排序 */}
            <div className={style.delayRankChartBox}
            >
                <div className={style.title}>
                    <b>落後綜合排名</b><br />
                    <span style={{ fontSize: "0.9rem" }}>落後件數排序+落後比率排序</span>
                </div>
                <div className={style.listContainer}>
                    <span>&ensp;排名</span>
                    <span style={{ marginRight: '80px' }}>執行機關</span>
                </div>
                <div className={style.seperator} style={{ backgroundColor: "rgb(179,162,199)" }} />
                {getOrgRankListJsx('Summary', data.Summary)}
            </div>
        </div >
    )
}

export default DelayRankCharts;
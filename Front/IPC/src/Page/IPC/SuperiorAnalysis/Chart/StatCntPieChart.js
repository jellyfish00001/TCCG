import React from 'react';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem } from '@progress/kendo-react-charts';
import 'hammerjs';

const StatCntPieChart = (props) => {
    const { pieData, ChartItemClick } = props;

    return (
        <Chart onSeriesClick={(dataItem) => {
            ChartItemClick(dataItem.category === "符合")
        }}>
            <ChartLegend position="right" orientation="vertical" labels={{
                content: (e) => { return e.text + '：' + e.value }
            }} />
            <ChartSeries>
                <ChartSeriesItem
                    type="pie"
                    data={pieData}
                    field="value"
                    categoryField="category"
                    labels={{
                        visible: true,
                        position: "center",
                        color: "black",
                        background: "transparent",
                        content: (e) => { return `${e.category}:${e.value}` },
                        font: "bold 16px Arial, sans-serif"
                    }}
                    color="color"
                />
            </ChartSeries>
        </Chart>
    )
}

export default StatCntPieChart;
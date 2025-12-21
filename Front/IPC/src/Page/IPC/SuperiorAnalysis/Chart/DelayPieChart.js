import React from 'react';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem, ChartTitle } from '@progress/kendo-react-charts';
import 'hammerjs';

const DelayPieChart = (props) => {
    const { pieData, chartItemClick } = props;

    return (
        <Chart onSeriesClick={(dataItem) => {
            const items = dataItem.category.split("_")
            chartItemClick(items[0], items[1])
        }}>
            <ChartTitle text="落後類型" font='bold 24px Arial, sans-serif' />
            <ChartLegend position="right" orientation="vertical" labels={{
                content: (e) => { return e.text.split("_")[1] + '：' + e.value }
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
                        content: (e) => { return `${e.category.split("_")[1]}:${e.value}` },
                        font: "bold 16px Arial, sans-serif"
                    }}
                    color="color"
                />
            </ChartSeries>
        </Chart>
    )
}

export default DelayPieChart;
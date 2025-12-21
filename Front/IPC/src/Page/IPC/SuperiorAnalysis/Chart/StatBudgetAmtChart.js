import React from 'react';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem, ChartValueAxis, ChartValueAxisItem } from '@progress/kendo-react-charts';
import 'hammerjs';

const StatBudgetAmtChart = (props) => {
    const { model, ChartItemClick } = props;

    return (
        <Chart
            style={{ height: model.Categories.length * 30, minHeight: 100 }}
            onSeriesClick={(dataItem) => {
                const val = dataItem.category.split('_')
                ChartItemClick(val[1], val[0])
            }}
        >
            <ChartLegend position="top" orientation="horizontal" />
            <ChartCategoryAxis>
                <ChartCategoryAxisItem categories={model.Categories} labels={{
                    font: "30px",
                    margin: { right: 10 },
                    content: (e) => { return e.value.split('_')[0] }
                }} />
            </ChartCategoryAxis>
            <ChartSeries>
                {
                    model.Data.map((item) => (
                        <ChartSeriesItem
                            type="bar"
                            data={item.data}
                            name={item.name}
                            color={item.color}
                            labels={{
                                visible: true,
                                background: "transparent",
                                content: (e) => {
                                    return e.value > 0
                                        ? (parseInt(parseFloat(e.value) / 1000)).toLocaleString()
                                        : ""
                                },
                                font: "bold 16px Arial, sans-serif"
                            }}
                            stack={true}
                        />
                    ))
                }
            </ChartSeries>
            <ChartValueAxis>
                <ChartValueAxisItem labels={{
                    font: "30px",
                    content: (e) => {
                        return e.value > 0
                            ? (parseInt(parseFloat(e.value) / 1000)).toLocaleString()
                            : ""
                    }
                }} />
            </ChartValueAxis>
        </Chart>
    )
}

export default StatBudgetAmtChart;
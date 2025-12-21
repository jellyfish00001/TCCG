import React from 'react';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem } from '@progress/kendo-react-charts';
import 'hammerjs';

const BehindItemChart = (props) => {
    const {
        title,
        model = { Categories: [], Data: [] },
        chartItemClick
    } = props;

    return (
        <>
            {
                !IsNullOrEmpty(title) &&
                <h3>{title}</h3>
            }

            <Chart
                style={{ height: model.Categories.length * 30, minHeight: 100 }}
                onSeriesClick={(dataItem) => {
                    const items = dataItem.category.split("_")
                    chartItemClick(items[0], items[1])
                }}
            >
                <ChartLegend position="top" orientation="horizontal" visible={false} />
                <ChartCategoryAxis>
                    <ChartCategoryAxisItem categories={model.Categories} labels={{
                        font: "30px",
                        margin: { right: 10 },
                        content: (e) => { return e.value.split('_')[1] }
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
                                    content: (e) => { return e.value > 0 ? e.value : "" },
                                    font: "bold 16px Arial, sans-serif"
                                }}
                                stack={true}
                            />
                        ))
                    }
                </ChartSeries>
            </Chart>
        </>
    )
}

export default BehindItemChart;
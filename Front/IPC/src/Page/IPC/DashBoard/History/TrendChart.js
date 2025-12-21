import React from 'react';
import { Chart, ChartSeries, ChartLegend, ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem, ChartValueAxis, ChartValueAxisItem, ChartTitle, ChartCategoryAxisTitle } from '@progress/kendo-react-charts';
import 'hammerjs';


const TrendChart = ({ chartTitle, chartType, categories, data }) => {

    return (
        <>
            <div style={{ fontWeight: 'bold', marginTop: '30px' }}>{chartTitle}</div>
            <Chart>
                <ChartCategoryAxis>
                    <ChartCategoryAxisItem categories={categories}>
                    </ChartCategoryAxisItem>
                </ChartCategoryAxis>
                <ChartSeries>
                    <ChartSeriesItem
                        type={chartType}
                        color={'lightBlue'}
                        gap={2}
                        spacing={0.25}
                        data={data}
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
    );

}
export default TrendChart;
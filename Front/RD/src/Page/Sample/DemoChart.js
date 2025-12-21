import React, { useState } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Slide } from '@progress/kendo-react-animation';
import { Dialog } from '@progress/kendo-react-dialogs';
import { Chart, ChartTitle, ChartSeries, ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem } from '@progress/kendo-react-charts';

const DemoChart = () => {
    const [slideVisible, setSlideVisible] = useState(true)
    const [chartVisible, setChartVisible] = useState(false)

    const exampleCode = `
    //需import Component
    import { Chart, ChartTitle, ChartSeries, 
             ChartSeriesItem, ChartCategoryAxis, ChartCategoryAxisItem } from '@progress/kendo-react-charts'; 
    
    //data 
    const categories = ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六'];
    const data = [
        {
            name: "觀光客",
            data: [111, 13, 32, 21, 33, 12, 71]
        },
        {
            name: "當地人",
            data: [9, 54, 15, 13, 23, 66, 7]
        }
    ];
             
    <Chart>
        <ChartTitle text="來高雄市觀光人數" />                   //設置圖表標題
        <ChartCategoryAxis>
            <ChartCategoryAxisItem categories={categories} />   //有哪些分類
        </ChartCategoryAxis>
        <ChartSeries>
            {data.map((item) => (               //多個數據，可使用map將data塞入
                <ChartSeriesItem
                    type="bar"                  //圖表類型 ex:bar, line, pie, ...等
                    tooltip={{ visible: true }} //數據提示工具
                    data={item.data}            //數據
                    name={item.name}            //顯示的文字 
                />
            ))}
        </ChartSeries>
    </Chart>`;

    const categories = ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六'];
    const data = [
        {
            name: "觀光客",
            data: [111, 13, 32, 21, 33, 12, 71]
        },
        {
            name: "當地人",
            data: [9, 54, 15, 13, 23, 66, 7]
        }
    ];

    return (
        <div className="fnForm">
            <div>
                <Button icon="menu" onClick={() => setSlideVisible(!slideVisible)} style={{ margin: "0 10px", color: "black", background: "none", border: "none" }}></Button>
            </div>
            <div>
                <Slide>
                    {
                        slideVisible &&
                        <div>
                            <ul>
                                <li><a href="https://www.telerik.com/kendo-react-ui/components/charts/" target="_blank" rel="noopener noreferrer">KendoReact Charts Overview</a></li>
                                <li>可參考實作範例的資源使用狀況(ResourceUsage/Query)</li>
                                <Button onClick={() => setChartVisible(!chartVisible)}>Demo</Button>
                            </ul>
                        </div>
                    }
                </Slide>
                {chartVisible && <Dialog onClose={() => setChartVisible(false)} title={"圖表範例"}
                    width='70%' height='70%' >
                    <Chart>
                        <ChartTitle text="來高雄市觀光人數" />
                        <ChartCategoryAxis>
                            <ChartCategoryAxisItem categories={categories} />
                        </ChartCategoryAxis>
                        <ChartSeries>
                            {data.map((item) => (
                                <ChartSeriesItem
                                    type="bar"
                                    tooltip={{ visible: true }}
                                    data={item.data}
                                    name={item.name}
                                />
                            ))}
                        </ChartSeries>
                    </Chart>
                </Dialog>}
                <pre>
                    <i>
                        {exampleCode}
                    </i>
                </pre>
            </div>
        </div >

    );
}

export default DemoChart;

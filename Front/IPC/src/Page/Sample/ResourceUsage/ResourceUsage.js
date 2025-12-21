import React, { useState, useEffect, useRef } from 'react';
import { ServerConfig } from '../../../Basic/BasicData';
import { api } from '../../../Basic/ApiFetch';
import { Button } from '@progress/kendo-react-buttons';
import {
    Chart,
    ChartTitle,
    ChartSeries,
    ChartSeriesItem,
    ChartCategoryAxis,
    ChartCategoryAxisItem
} from '@progress/kendo-react-charts';
import './Style.css';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'

function useMount(callBack) {
    useEffect(callBack, []) //傳入空陣列，該effect只在componentDidMount執行
}

function Query() {
    const timerId = useRef(0);
    const [performance, setPerformance] = useState([]);
    let loadData = async mask => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'Performance';
        let request = new Request(url);
        let response = await api.Fetch(request, false, mask);
        if (response.ok) {
            setPerformance(await response.json());
        }
    }

    useMount(() => {
        ///componentDidMount
        loadData(true);
        // @ts-ignore
        timerId.current = setInterval(() => {
            loadData(false);
        }, 10000)

        return () => {
            ///componentDidUnMount
            if (timerId.current !== 0) {
                clearInterval(timerId.current);
            }
        }
    });

    return (
        <div className="fnForm">
            <div className="fn-buttons">
                <Button onClick={() => loadData(true)} >查詢</Button>
            </div>
            <div>
                <Chart>
                    <ChartTitle text="資源使用狀況" />
                    <ChartCategoryAxis>
                        <ChartCategoryAxisItem
                            categories={performance.map(x => x.CategoryName + ' ' + x.Unit)}
                        />
                    </ChartCategoryAxis>
                    <ChartSeries>
                        <ChartSeriesItem
                            type="bar"
                            data={performance}
                            field="Value"
                            tooltip={{ visible: true }}
                        />
                    </ChartSeries>
                </Chart>
            </div>

            <h3 style={{ marginTop: '15px' }}>System.Diagnostics</h3>

            <h4>PerformanceCounter(string categoryName, string counterName, string instanceName)</h4>
            <pre>
                <em>
                    categoryName =&gt; 監視目標的種類名稱, ex. "Processor", "Memory", "PhysicalDisk"{'\n'}
                counterName  =&gt; 監視目標計數器的名稱, ex. "% Processor Time", "% Disk Time"{'\n'}
                instanceName =&gt; 監視目標計數器的執行個體名稱, 單一執行個體則不用instanceName, ex. "_Total", {'\n'}
                    {'\n'}
                取得效能計數器的值{'\n'}
                    float performanceCounter.nextValue(){'\n'}
                (Note: 有些計數器初始值為 0，需要先取nextValue，接著等至少一秒之後再取nextValue){'\n'}
                </em>
            </pre>
            <h4>PerformanceCounterCategory(string categoryName)</h4>
            <pre>
                <em>
                    categoryName =&gt; 監視目標的種類名稱, ex. "Processor"代表CPU{'\n'}
                    {'\n'}
                取得instanceName{'\n'}
                    performanceCounterCategory.GetInstanceNames(){'\n'}
                    return type = string[]{'\n'}
                </em>
            </pre>
            <h4>DriveInfo</h4>
            <pre>
                <em>
                    {'using System.IO;\n'}
                    {'\n'}
                    {'取得該機器所有Drive\n'}
                    {'DriveInfo[] drives = DriveInfo.GetDrives();\n'}
                    {'\n'}
                    {'string DriveInfo.Name //取得該Drive名稱 ex:C:\\\\\n'}
                    {'long DriveInfo.TotalSize //取得該Drive所有空間 ex: 1073741824\n'}
                    {'long DriveInfo.TotalFreeSpace //取得該Drive所有可用空間 ex: 1073741824\n'}
                </em>
            </pre>
        </div>
    )
}

export default Query;
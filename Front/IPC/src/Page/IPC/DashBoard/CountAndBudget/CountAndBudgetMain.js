import React, { useState, useRef } from 'react';
import { PageContainer } from '../../../../Basic/PageContainer';
import { TabStrip, TabStripTab } from "@progress/kendo-react-layout";
import StatisticChart from './StatisticChart';

// 件數及經費情形 Main Component
const DashBoardCountAndBudgetMain = () => {

    // Tab選擇
    const [selected, setSelected] = useState(0);
    // 專案狀態
    const [projectStatusType, setProjectStatusType] = useState("1")
    /**
     * Tab切換事件
     * @param {*} e 
     */
    const handleSelect = (e) => {
        setSelected(e.selected);
    };

    return (
        <PageContainer style={{ overflow: 'auto' }}>
            <TabStrip selected={selected} onSelect={handleSelect} className="tab-content">
                <TabStripTab title="執行機關">
                    <StatisticChart dabKind={"A"}
                        type={"執行機關"}
                        projectStatusType={projectStatusType}
                        setProjectStatusType={setProjectStatusType}
                    />
                </TabStripTab>
                <TabStripTab title="行政區">
                    <StatisticChart dabKind={"B"}
                        type={"行政區"}
                        projectStatusType={projectStatusType}
                        setProjectStatusType={setProjectStatusType}
                    />
                </TabStripTab>
                <TabStripTab title="建設類別">
                    <StatisticChart dabKind={"C"}
                        type={"建設類別"}
                        projectStatusType={projectStatusType}
                        setProjectStatusType={setProjectStatusType}
                    />
                </TabStripTab>
            </TabStrip>
        </PageContainer >
    );

}
export default DashBoardCountAndBudgetMain;
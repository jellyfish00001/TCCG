import React from 'react';
import { PageContainer } from '../../../Basic/PageContainer';
import { TabStrip, TabStripTab } from "@progress/kendo-react-layout";
import { CodeCheckPointGrid } from './CodeCheckpointGrid';
import SpecialCodeGrid from './SpecialCodeGrid';
import DelayClassGrid from './DelayClassGrid';
import CodePlanItemGrid from './CodePlanItemGrid';
import WorkingDay from './WorkingDay';

const SetCode = () => {

    const [selected, setSelected] = React.useState(0);

    const handleSelect = (e) => {
        setSelected(e.selected);
    };

    return (
        <PageContainer>
            <h3 className="k-dialog-titlebar">代碼維護</h3>
            <TabStrip selected={selected} onSelect={handleSelect} className="tab-content">
                <TabStripTab title="執行方式">
                    <CodeCheckPointGrid />
                </TabStripTab>
                <TabStripTab title="落後項目">
                    <DelayClassGrid />
                </TabStripTab>
                <TabStripTab title="特殊加註">
                    <SpecialCodeGrid />
                </TabStripTab>
                <TabStripTab title="本府預算來源">
                    <CodePlanItemGrid levelMark={"1"} />
                </TabStripTab>
                <TabStripTab title="中央預算來源">
                    <CodePlanItemGrid levelMark={"2"} />
                </TabStripTab>
                <TabStripTab title="工作日">
                    <WorkingDay />
                </TabStripTab>
            </TabStrip>
        </PageContainer >
    );

}
export default SetCode;
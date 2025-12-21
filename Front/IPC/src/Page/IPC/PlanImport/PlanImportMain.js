import React from 'react';
import { PageContainer } from '../../../Basic/PageContainer';
import { TabStrip, TabStripTab } from "@progress/kendo-react-layout";
import BasicDataImport from './BasicDataImport';
import PWSSDPlanImport from './PWSSDPlanImport';

const PlanImportMain = () => {

    const [selected, setSelected] = React.useState(0);

    const handleSelect = (e) => {
        setSelected(e.selected);
    };

    return (
        <PageContainer style={{ height: "100%" }}>
            <h3 className="k-dialog-titlebar">{"設定-計畫匯入"}</h3>
            <TabStrip selected={selected} onSelect={handleSelect} >
                <TabStripTab title="先期計畫匯入">
                    <PWSSDPlanImport />
                </TabStripTab>
                <TabStripTab title="基本資料匯入">
                    <BasicDataImport />
                </TabStripTab>
            </TabStrip>
        </PageContainer>
    );

}
export default PlanImportMain;
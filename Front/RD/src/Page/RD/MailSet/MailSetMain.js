import React from 'react';
import { PageContainer } from '../../../Basic/PageContainer';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { TabStrip, TabStripTab } from "@progress/kendo-react-layout";
import MailSetGrid from "./MailSetGrid";
import { MailSetService } from './MailSetService';

const MailSetMain = () => {
    const [selected, setSelected] = React.useState(0);
    const [gridData, setGridData] = React.useState([]);
    const handleSelect = (e) => {
        setSelected(e.selected);
    };

    /**
     * 載入郵件範本
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let mailData = await MailSetService.getMailTemplate();
        setGridData(mailData);
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, [])

    return (
        <PageContainer
            toolbar={<>
                <h3 className="k-dialog-titlebar">郵件設定</h3>
            </>}
        >
            <TabStrip selected={selected} onSelect={handleSelect} className="tab-content">
                <TabStripTab title="計畫填報通知">
                    <MailSetGrid data={gridData.filter(x => x.MAIL_TYPE == "A")} loadData={loadData} />
                </TabStripTab>
            </TabStrip>
        </PageContainer >
    );
}
export default MailSetMain;
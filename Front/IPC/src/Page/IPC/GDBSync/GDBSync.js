import React from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import TextInput from '../../../Components/Input/TextInput';
import { syncData } from './GDBSyncService';

const GDBSync = () => {
    const [projectNos, setProjectNos] = React.useState("");

    /**
     * 存檔
     */
    const save = async () => {
        SetMaskOnOff(true);
        let result = await syncData(projectNos);
        SetMaskOnOff(false);
        if (result.success) {
            setProjectNos("");
            showGlobalMessageBox(result.message);
        }
    }

    return (
        <PageContainer toolbar={
            <>
                <h3 className="k-dialog-titlebar">{"工程標案同步"}</h3>
                <Button type="button" title="確定執行" onClick={save} disabled={IsNullOrEmpty(projectNos)}>確定執行</Button>
            </>
        }>
            <form>
                <table>
                    <tr>
                        <th>
                            <CommonTooltip title={"計畫編號"} content={"多筆請用空白隔開"} />
                        </th>
                        <td>
                            <TextInput
                                rows={10}
                                name="PROJECT_NO"
                                style={{ width: "100%" }}
                                value={projectNos}
                                onChange={(e) => {
                                    setProjectNos(e.value);
                                }}
                            />
                        </td>
                    </tr>
                </table>
            </form>
        </PageContainer>
    )
}
export default GDBSync;
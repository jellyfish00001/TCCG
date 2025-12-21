import React, { useEffect, useState } from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { getCurrentCycleData } from '../../../Basic/CommonService';
import { generateMonthlyDAB, getFillCompleteCycle } from './DashBoardService';

const GenerateMonthlyDAB = () => {
    const [projectNos, setProjectNos] = React.useState("");

    const [projectFillCycle, setProjectFillCycle] = useState({
        year: "",
        month: ""
    })

    // 產出
    const execute = async () => {
        SetMaskOnOff(true);
        let result = await generateMonthlyDAB(projectFillCycle.year, projectFillCycle.month);
        SetMaskOnOff(false);
        if (result.success) {
            showGlobalMessageBox(result.message);
        }
    }

    // 取得當期週期資料
    const loadFillCycleData = async () => {
        const { DAB_YEAR_YYY, DAB_MONTH } = await getFillCompleteCycle();
        setProjectFillCycle({ year: DAB_YEAR_YYY, month: DAB_MONTH });
    }

    useEffect(() => {
        loadFillCycleData();
    }, [])


    return (
        <PageContainer toolbar={
            <>
                <h3 className="k-dialog-titlebar">{"產生儀錶板資料"}</h3>
                <Button type="button" title="產生" onClick={execute} disabled={IsNullOrEmpty(projectFillCycle.year)}>確定執行</Button>
            </>
        }>
            <form>
                <table>
                    <tr>
                        <th>
                            填報週期
                        </th>
                        <td>
                            {projectFillCycle.year}年{projectFillCycle.month}月
                        </td>
                    </tr>
                </table>
            </form>
        </PageContainer>
    )
}
export default GenerateMonthlyDAB;
import React from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { PageContainer } from "../../../Basic/PageContainer";
import { GetHistory } from '../../../Basic/BasicData';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { CheckIsRDECRole } from '../../../Basic/CommonService';

import { GetReportsList } from './ReportsService';

/**
 * 報表列印
 * @return {*} 
 */
const ReportsList = () => {
    // Grid 資料
    const [gridData, setGridData] = React.useState([]);
    // 是否為管考
    const [isRDECRole, setIsRDECRole] = React.useState(false);

    const loadData = async () => {
        SetMaskOnOff(true);
        // 檢查登入者是否有管考權限
        const isRDECRole = await CheckIsRDECRole();
        let data = GetReportsList();
        setIsRDECRole(isRDECRole);
        if (!isRDECRole) {
            data = data.filter(x => x.ID <= 8);
        }
        setGridData(data);
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, []);

    // 跳頁至查詢頁面
    const nameCell = (prop) => {
        return (
            <td>
                <a onClick={() => {
                    GetHistory().push('/Home/ReportsQuery', {
                        isRDECRole: isRDECRole,     // 是否為管考
                        id: prop.dataItem.ID,      // 報表編號
                        name: prop.dataItem.NAME  // 報表名稱
                    });
                }}>{prop.dataItem.NAME}</a>
            </td>
        )
    }

    return (
        <>
            <PageContainer>
                <h3 className="k-dialog-titlebar">統計報表</h3>
                <Grid
                    style={{
                        height: '100%'
                    }}
                    data={gridData}
                    resizable={true}>
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn field="ID" title="報表" width="50px" cell={(prop) => {
                        return <td style={{ textAlign: "center" }}>{prop.dataItem.ID}</td>
                    }} />
                    <GridColumn field="NAME" title="報表名稱" cell={nameCell} />
                    <GridColumn field="MEMO" title="報表說明" />
                </Grid>
            </PageContainer>
        </>
    )
}
export default ReportsList;

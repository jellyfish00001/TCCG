import React, { useState, useEffect } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { PageContainer } from "../../../Basic/PageContainer";
import { GetHistory } from '../../../Basic/BasicData';
import ReportService from './ReportService';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { GetBasicData } from "../../../Basic/BasicData";

// 查詢-統計報表
const ReportMain = () => {
    const [gridData, setGridData] = useState([]);

    /**
     * 取報表列表
     * @returns 
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        // 取報表列表
        let data = ReportService.getStatisticsList();
        let roles = await GetBasicData('allRoles');
        let dataID = [];
        let gridData = [];
        if(roles.length > 0)
        switch (roles[0].ROLE_ID) {
            // 機關承辦
            case 'HAND_USER_ROL_PWS':
                dataID = data.filter(item => [1, 2, 3, 7,8, 9].includes(item.ID));
                gridData = dataID.filter(item => item.RPT_ID !== 'RPTProjectPolicyPublicBudgetReview' && item.RPT_ID !== 'RPTProjectPolicyFundBudgetReview');
                break;
            // 機關主管(研考)
            case 'ORG_RDEC_ROL_PWS':
                dataID = data.filter(item => [1, 2, 3, 7, 8, 9].includes(item.ID));
                gridData = dataID.filter(item => item.RPT_ID !== 'RPTProjectPolicyPublicBudgetReview' && item.RPT_ID !== 'RPTProjectPolicyFundBudgetReview');
                break;
            // 專案小組管理
            case 'RDEC_MGR_ROL_PWS':
                dataID = data;
                gridData = dataID.filter(item => item.RPT_ID !== 'RPTPolicyPublicBudgetReview' && item.RPT_ID !== 'RPTPolicyFundBudgetReview');
                break;
            default:
                dataID = data;
                gridData = dataID.filter(item => item.RPT_ID !== 'RPTPolicyPublicBudgetReview' && item.RPT_ID !== 'RPTPolicyFundBudgetReview');
                break;
        }
        setGridData(gridData);
        SetMaskOnOff(false);
    }

    // 初始畫面資料
    useEffect(() => {
        loadData();
    }, []);

    // 跳頁至查詢頁面
    const nameCell = (prop) => {
        return (
            <td>
                <a onClick={() => {
                    GetHistory().push('/Home/ReportQuery', {
                        id: prop.dataItem.ID,
                        name: prop.dataItem.NAME,
                        rpt_id:prop.dataItem.RPT_ID
                    });
                }}>{prop.dataItem.NAME}</a>
            </td>
        )
    }

    return (
        <>
            <PageContainer>
                <h3 className="k-dialog-titlebar">報表列印</h3>
                <Grid
                    style={{
                        height: '100%'
                    }}
                    data={gridData}
                    resizable={true}>
                    <GridNoRecords>無資料</GridNoRecords>
                    
                    <GridColumn field="ID" title="表" width="50px" cell={(prop) => {
                        return <td style={{ textAlign: "center" }}>{prop.dataItem.ID}</td>
                    }} />
                    <GridColumn field="NAME" title="報表名稱" cell={nameCell} />
                    <GridColumn field="MEMO" title="說明" />
                </Grid>
            </PageContainer>
        </>
    )
}
export default ReportMain;
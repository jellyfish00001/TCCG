import React, { useState, useEffect, useRef } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Pageable } from '../../../Basic/BasicData';
import CheckBoxList from '../../../Components/Input/CheckBoxList';

export const PWSSDPlanImportGrid = ({ data, planCheck }) => {

    const [gridData, setGridData] = useState([]);
    const [paging, setPaging] = useState({ skip: 0, take: 20 })

    const checkedPlan = useRef([]);

    // checkBox Cell
    const checkBoxCell = props => {
        if (!props.dataItem.IsImport) {
            return (
                <CheckBoxList
                    group='CheckBoxList'
                    valueField='id'
                    data={[
                        { id: props.dataItem.PlanId, },
                    ]}
                    onChange={(e) => {
                        SetChecked(e);
                    }}
                    style={{ 'textAlign': 'center' }}
                />
            )
        } else {
            return (
                <td></td>
            )
        }
    }

    //將勾選的資料放進checkedPlan
    const SetChecked = (e) => {
        if (e.value === true)
            checkedPlan.current.push(e.dataItem.id);
        else {
            checkedPlan.current.map((list, index) => {
                if (list === e.dataItem.id) {
                    checkedPlan.current.splice(index, 1);
                }
            });
        }
        // call back 選取之PlanId
        planCheck(checkedPlan.current);
    }

    //匯入狀態cell
    const isImportCell = props => {
        let isImport = props.dataItem.IsImport;
        let color = isImport ? 'blue' : 'red';
        let text = isImport ? '已匯入' : '未匯入';
        return (
            <td style={{ 'color': color }}>{text}</td>
        )
    }

    // 設定外部傳入Grid查詢結果
    useEffect(() => {
        setGridData([...data]);
    }, [data])

    return (
        <Grid
            style={{
                height: 'calc(100vh - 320px)',
                overflow: 'auto',
            }}
            data={gridData.slice(paging.skip, paging.take + paging.skip)}
            skip={paging.skip}
            take={paging.take}
            total={gridData.length}
            pageable={Pageable}
            resizable={true}
            onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}>

            <GridNoRecords>無資料</GridNoRecords>
            <GridColumn field="PlanId" width="0px" />
            <GridColumn field='isChecked' title="口" width={"40px"} cell={checkBoxCell} />
            <GridColumn field="PlanYear" width={"70px"} title="年度" />
            <GridColumn field="PlanName" title="計畫名稱" width="300px" />
            <GridColumn field="OU_NAME" title="主管機關" />
            <GridColumn field="ExecUnitName" title="提報單位" />
            <GridColumn field="ExecOrgName" title="執行機關" />
            <GridColumn field="PlanDate" title="計畫期程" />
            <GridColumn field="SendStatus" title="審核狀態" />
            <GridColumn field="IsImport" title="匯入狀態"
                cell={isImportCell} />
        </Grid>
    );
}
export default PWSSDPlanImportGrid
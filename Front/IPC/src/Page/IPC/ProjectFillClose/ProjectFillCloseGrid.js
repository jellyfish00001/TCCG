import React, { useState, useEffect, useRef } from 'react';
import Table from '../../../Css/custom/Table.module.css';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import { formatNumber } from '@telerik/kendo-intl';

export const ProjectFillCloseAuditGrid = (
    { data, gridChange, isRDEC, isAudit }) => {

    const [gridData, setGridData] = useState([]);

    const numericTextInputCell = props => {
        return (
            <NumericTextInputCell
                {...props}
                Numberformat={'n0'}
                required={true}
                // 結案審核主辦不可編輯
                editable={!isAudit || isRDEC}
                onCellInputBlur={(data) => {
                    data.TOTAL_ACTUAL_COMP = data.ACTUAL_PAY + data.UNPAY;
                    gridChange([{ ...data }])
                }}
                AlwaysEdit={true}
            />
        )
    }

    // 設定外部傳入Grid查詢結果
    useEffect(() => {
        setGridData([...data]);
        gridChange([...data]);
    }, [data])

    return (
        <Grid
            style={{
                textAlign: "center",
                height: '100%',
                overflow: 'auto',
            }}
            data={gridData}
            resizable={true}
        >

            <GridNoRecords>無資料</GridNoRecords>

            <GridColumn field="DATA_DATE_SHOW" title="月份" className={Table.textAlign_center} />
            <GridColumn field="TOTAL_BUDGET"
                title="計畫總經費(元)"
                cell={(prop) => {
                    return <td className={Table.textAlign_right}>
                        {formatNumber(prop.dataItem.TOTAL_BUDGET, "n0")}
                    </td>
                }}
            />
            <GridColumn field="TOTAL_ACTUAL_COMP"
                title={<span>累計實際<br />完成金額(元)</span>}
                cell={(prop) => {
                    return <td className={Table.textAlign_right}>
                        {formatNumber(prop.dataItem.TOTAL_ACTUAL_COMP, "n0")}
                    </td>
                }}
            />
            <GridColumn
                field='ACTUAL_PAY'
                title={<span>累計各項<br />經費支用(元)</span>}
                cell={numericTextInputCell}
            />
            <GridColumn
                field="UNPAY"
                title="應付未付數(元)"
                cell={numericTextInputCell}
            />
            <GridColumn
                field='BALANCE'
                title="結餘數(元)"
                cell={numericTextInputCell}
            />
            <GridColumn
                field='MDF_DATE'
                title="填報日期"
                className={Table.textAlign_center}
            />
        </Grid>
    );
}
export default ProjectFillCloseAuditGrid
import React, { useState, useEffect } from 'react';
import { Grid, GridNoRecords, GridColumn } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';

const SubflowDetail = (props) => {
    /* useState */
    const [row, setRow] = useState([]);

    /* useEffect */
    useEffect(() => {
        const subflows = props.dataItem.detail;
        if (subflows.length > 0) {
            let row = [];
            for (let i = 0; i < subflows.length; i++) {
                row.push(
                    <Grid
                        style={{
                            width: '100%',
                            height: '100%',
                            overflow: 'auto'
                        }}
                        data={subflows[i].slice(0, 20)}
                        total={subflows[i].length}
                        skip={0}
                        scrollable="scrollable"
                        resizable={true}
                        pageSize={20}
                        pageable={{
                            info: true,
                            type: 'numeric',
                            pageSizes: true,
                            previousNext: true
                        }}
                        rowRender={props.customRowRender}
                        key={'flowDetail_' + i}
                    >
                        <GridNoRecords> </GridNoRecords>
                        <GridColumn title=" " field="NO" width={25} />
                        <GridColumn cell={toolCellStyle} width={40} />
                        <GridColumn title="分會流程代碼" field="flow_code" />
                        <GridColumn title="分會流程關卡" cell={props.customCellStyle} />
                        <GridColumn title="分會簽核狀態" field="status_name" />
                        <GridColumn title="分會簽核時間" field="mdf_date" cell={props.formatDate} width={150} />
                    </Grid>
                )
            }
            setRow(row);
        }
    }, [props]);

    //刪除subflow
    const toolCellStyle = (data) => {
        return (
            (!data.dataItem.has_signed && data.dataItem.status === 1 && props.isAdmin) ?
                <td>
                    <Button icon="delete" onClick={() => {
                        props.deleteSubflow(data.dataItem.flow_code);
                    }}></Button>
                </td>
                :
                <td></td>
        );
    }

    return (
        <div>{row}</div>
    );
}

export default SubflowDetail;
import React, { useState, useEffect, useRef } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import { Pageable } from '../../../../Basic/BasicData';
import { saveProjectMapPCC } from '../ProjectFillCkptComService';
import { showGlobalMessageBox } from '../../../../Route/RootMiddleware';

// 關聯工程會標案計畫Grid
export const AssociatePCCGrid = ({ data, onClose, PROJECT_NO, reloadData, startWorkDate }) => {

    const [gridData, setGridData] = useState([]);
    const [paging, setPaging] = useState({ skip: 0, take: 20 })

    const associatePCCCell = props => {
        const uid = props.dataItem.plnprj_uid;
        return (
            <td>
                <div className='fn-buttons' style={{ 'display': "flex", justifyContent: 'center' }}>
                    <Button type='button' style={{ background: "#02466d", color: "#fff", padding: "2px 5px" }} onClick={() => { associatePCC(uid) }}>關聯</Button>
                </div>
            </td>
        )
    }

    // 關聯工程會
    const associatePCC = async (uid) => {
        let result = await saveProjectMapPCC(PROJECT_NO, uid, startWorkDate);
        if (result.success) {
            showGlobalMessageBox(result.message, () => { reloadData(true); onClose() })
        } else {
            showGlobalMessageBox(result.message);
        }
    }

    // 設定外部傳入Grid查詢結果
    useEffect(() => {
        if (data && data.length > 0)
            setGridData([...data]);
    }, [data])

    return (
        <Grid
            style={{
                textAlign: "center",
                height: 'calc(100% - 145px)',
                overflow: 'auto',
            }}
            data={gridData.slice(paging.skip, paging.take + paging.skip)}
            resizable={true}
            skip={paging.skip}
            take={paging.take}
            pageable={Pageable}
            total={gridData.length}
            onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
        >
            <GridNoRecords>無資料</GridNoRecords>
            <GridColumn field="plnprj_uid" width="0px" />
            <GridColumn cell={associatePCCCell} width="100px" title="關聯" />
            <GridColumn field="plnprj_id"
                title="標案編號"
            />
            <GridColumn field="plnprj_name"
                title="標案名稱"
            />
            <GridColumn field="execorg_name"
                title="執行機關"
            />
        </Grid>
    );
}
export default AssociatePCCGrid
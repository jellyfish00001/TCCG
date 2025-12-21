import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import EformFlowService from './eFormFlow.service';
import EFormFlowAudit from './EFormFlow-Audit/EFormFlowAudit';
import FlowDetail from '../../FlowModule/FlowDetail/FlowDetail';

const EFormFlow = () => {
    /* use Context */
    const { showConfirmBox } = useContext(ConfirmBoxContext);

    /* use State */
    const [flowList, setFlowList] = useState([]);
    const [queryDetailVisible, setQueryDetailVisible] = useState(false);
    const [mdfWindowVisible, setMdfWindowVisible] = useState(false);
    const [flowId, setFlowId] = useState("");
    const [flowCode, setFlowCode] = useState("");
    const [subflowCode, setSubflowCode] = useState("");
    const [isSubflow, setIsSubflow] = useState(false);
    const [count, setCount] = useState(0);
    const [page, setPage] = useState({
        take: 20,
        skip: 0,
    })
    const service = EformFlowService();

    /* use Effect */
    useEffect(() => {
        loadGridData();
    }, [])

    /* load grid data */
    const loadGridData = async () => {
        let list = await service.loadFlowList();
        setFlowList(list);
        setCount(list.length);
    }

    /* Grid換頁控制 */
    const pageChange = (event) => {
        setPage({
            skip: event.page.skip,
            take: event.page.take
        });
    }

    /* 簽核按鈕style */
    const toolCellStyle = (props) => {
        return (
            <td>
                {!props.dataItem.HAS_SUBFLOW &&
                    <Button icon="track-changes-accept" onClick={() => {
                        setFlowId(props.dataItem.FLOW_ID);
                        setFlowCode(props.dataItem.FLOW_CODE);
                        setIsSubflow(props.dataItem.IS_SUBFLOW);
                        setSubflowCode(props.dataItem.SUB_FLOW_CODE);
                        showConfirmBox("確定要進行簽核作業嗎？", () => { setMdfWindowVisible(true); })
                    }}></Button>
                }
                <Button icon="grid-layout" onClick={() => {
                    setFlowCode(props.dataItem.FLOW_CODE);
                    setQueryDetailVisible(true);
                }}></Button>
            </td>
        );
    }

    /* 簽核日數外觀客製 */
    const expireDayCellStyle = (props) => {
        let color = "black";
        if (props.dataItem.EXPIRE_DAY <= 0) color = "red";
        return (<td style={{ color: color }}>{props.dataItem.EXPIRE_DAY < 0 ? 0 : props.dataItem.EXPIRE_DAY}</td>);
    }

    /* 關閉編修畫面 */
    const closeMdfWindow = () => {
        setFlowId('');
        setFlowCode('');
        setSubflowCode('');
        setIsSubflow(false);
        setMdfWindowVisible(false);
    }

    /* 完成編修 */
    const finishMdf = () => {
        closeMdfWindow();
        loadGridData();
    }

    return (
        <div className="fnForm">
            <div>
                <div className="fn-buttons">
                    <Button onClick={() => { loadGridData(); }}>查詢</Button>
                </div>
            </div>
            <Grid
                className="expand"
                data={flowList.slice(page.skip, page.skip + page.take)}
                total={count}
                skip={page.skip}
                scrollable="scrollable"
                pageSize={page.take}
                pageable={{
                    info: true,
                    type: 'numeric',
                    previousNext: true
                }}
                onPageChange={pageChange}
            >
                <GridNoRecords> </GridNoRecords>
                <GridColumn title=" " field="NO" width='40px' />
                <GridColumn cell={toolCellStyle} width='80px' />
                <GridColumn title="表單類型" field="FORM_TYPENAME"></GridColumn>
                <GridColumn title="表單代碼" field="FILL_ID"></GridColumn>
                <GridColumn title="剩餘簽核天數" field="EXPIRE_DAY" cell={expireDayCellStyle}></GridColumn>
            </Grid>
            {/* 彈出流程明細頁面 */}
            {queryDetailVisible && <FlowDetail
                flowCode={flowCode}
                isAdmin={false}
                onClose={() => {
                    setFlowCode('');
                    setQueryDetailVisible(false);
                }}
            />}
            {/* 彈出修改視窗 */}
            {mdfWindowVisible && <EFormFlowAudit
                flowCode={flowCode}
                subflowCode={subflowCode}
                flowId={flowId}
                isSubflow={isSubflow}
                onClose={closeMdfWindow}
                onFinish={finishMdf}
            />}
        </div>
    );
}

export default EFormFlow;
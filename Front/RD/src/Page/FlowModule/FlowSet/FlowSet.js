import React, { useState, useEffect, useContext } from 'react';
import { Button, } from '@progress/kendo-react-buttons'
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import AddMdf from './FlowSet-AddMdf/FlowSetAddMdf'
import DetailAddMdf from './FlowSet-DetailAddMdf/FlowSetDetailAddMdf'
import { PageContainer } from '../../../Basic/PageContainer';
import { Tooltip } from '@progress/kendo-react-tooltip';
import FlowSetService from './flowSet.service'
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import { Pageable } from '../../../Basic/BasicData';
const Query = () => {
    const [flowSetData, setFlowSetData] = useState([]);
    const [paging, setPaging] = useState({ skip: 0, take: 10 })
    const [isAddMdf, setIsAddMdf] = useState({ visible: false, flowId: "", title: "" })
    const [isDetailAddMdf, setIsDetailAddMdf] = useState({ visible: false, stageData: [], title: "" })
    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const { showMessage } = useContext(MessageBoxContext);

    // 取得api資料
    const getFlowSetData = async () => {
        setFlowSetData(await FlowSetService.getFlowSetData());
    }

    //刪除功能 
    const isDelte = (flowId) => {
        showConfirmBox("進行刪除作業，確定嗎？", () => { deleteFlowData(flowId) })
    }

    const deleteFlowData = async (flowId) => {
        let response = await FlowSetService.deleteFlowSetData(flowId);
        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    getFlowSetData()
                }
            }
        })
    }

    //刪除功能 
    const isDetailDelte = (flowId, setOdr, details) => {
        showConfirmBox("進行刪除作業，確定嗎？", () => { deleteFlowDetailData(flowId, setOdr, details) })
    }

    const deleteFlowDetailData = async (flowId, setOdr, details) => {
        let datas = details.filter(detail => detail.SET_ODR !== setOdr);
        //新增or修改 功能
        let response = await FlowSetService.updateFlowDetail(flowId, datas);

        showMessage(response.result, {
            onOkAction: () => {
                if (response.ok) {
                    getFlowSetData();
                }
            }
        })
    }

    // 取得對應的關卡資料
    const getDetailData = async (event) => {
        event.dataItem.expanded = event.value;
        let flowId = event.dataItem.FLOW_ID;
        // 取得資料
        let detailStageData = await FlowSetService.getFlowDetailByFlowId(flowId);

        let data = flowSetData.slice();
        // 找出對應的主檔資料
        let index = data.findIndex(d => d.FLOW_ID === flowId);
        // 加入關卡資料至grid
        data[index].details = detailStageData;
        setFlowSetData(data);
    }

    function iconCell(props) {
        const value = props.dataItem[props.field];
        const icon = value ?
            'k-icon k-i-check-circle k-i-checkmark-circle'
            : 'k-icon k-i-close-circle k-i-x-circle';

        return (
            <td style={{ textAlign: "center" }}>
                <span className={icon} style={{ color: value ? 'green' : 'red' }}>  </span>
            </td>
        );
    }

    const DetailComponent = (props) => {
        const data = props.dataItem.details;
        if (data) {
            return (
                <Grid data={data}>
                    <GridColumn width={70} cell={(props) =>
                        <td>
                            <Button icon="edit" look="bare" title={"修改"} onClick={() => setIsDetailAddMdf({ visible: true, flowId: props.dataItem.FLOW_ID, stageData: props.dataItem, title: "修改" })} />
                            <Button icon="close" look="bare" title={"刪除"} onClick={() => isDetailDelte(props.dataItem.FLOW_ID, props.dataItem.SET_ODR, data)} />
                        </td>} />
                    <GridColumn width="300px" field="FLOW_STAGE_NAME" title="關卡名稱" />
                    <GridColumn field="MAIL" title="MAIL通知" cell={iconCell} />
                    <GridColumn field="SET_DECISION" title="是否決行" cell={iconCell} />
                    <GridColumn field="SIGNATURE" title="是否簽章" cell={iconCell} />
                    <GridColumn field="CERTIFICATE" title="檢查憑證" cell={iconCell} />
                    <GridColumn field="SEALED" title="是否封存" cell={iconCell} />
                </Grid>
            );
        }
        return (
            <div style={{ height: "50px", width: '100%' }}>
                <div style={{ position: 'absolute', width: '100%' }}>
                    <div className="k-loading-image" />
                </div>
            </div>
        );
    }

    useEffect(() => {
        //載入時取得流程清單
        getFlowSetData();
    }, []);

    const toolbar = [
        <Button onClick={getFlowSetData} >查詢</Button>,
        <Button onClick={() => setIsAddMdf({ visible: true, flowId: "", title: "新增" })}>新增</Button>
    ]

    return (
        <PageContainer
            toolbar={toolbar}
        >
            <Tooltip openDelay={10} position="bottom" anchorElement="target">
                <Grid
                    style={{
                        height: '100%',
                        overflow: 'auto'
                    }}
                    data={flowSetData.slice(paging.skip, paging.take + paging.skip)}
                    detail={DetailComponent}
                    total={flowSetData.length}
                    skip={paging.skip}
                    take={paging.take}
                    pageable={Pageable}
                    onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
                    expandField="expanded"
                    onExpandChange={getDetailData}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn width="30px" field="NO" title="No" />
                    <GridColumn width={110} cell={(props) =>
                        <td>
                            <Button icon="edit" look="bare" title={"修改"} onClick={() => setIsAddMdf({ visible: true, flowId: props.dataItem.FLOW_ID, title: "修改" })} />
                            <Button icon="close" look="bare" title={"刪除"} onClick={() => isDelte(props.dataItem.FLOW_ID)} />
                            <Button icon="track-changes-enable" look="bare" title={"新增關卡"} onClick={() => setIsDetailAddMdf({ visible: true, flowId: props.dataItem.FLOW_ID, stageData: "", title: "新增" })} />
                        </td>} />
                    <GridColumn field="FLOW_NAME" title="流程名稱" />
                    <GridColumn field="MEMO" title="流程說明" />
                    <GridColumn field="ENABLE_FLG" title="是否啟用" cell={(props) =>
                        <td >
                            {props.dataItem[props.field] ? "啟用" : "停用"}
                        </td>} />
                    <GridColumn field="FORM_COUNT" title="表單數" />
                </Grid>
            </Tooltip>

            {/* 新增or修改視窗 */}
            {isAddMdf.visible && <AddMdf
                title={isAddMdf.title}
                closeWindow={() => setIsAddMdf({ visible: false, flowId: "", title: "" })}
                flowId={isAddMdf.flowId}
                refreshGrid={getFlowSetData}
            />}

            {isDetailAddMdf.visible && <DetailAddMdf
                title={isDetailAddMdf.title}
                closeWindow={() => setIsDetailAddMdf({ visible: false, flowId: "", stageData: "", title: "" })}
                flowId={isDetailAddMdf.flowId}
                stageData={isDetailAddMdf.stageData}
                refreshGrid={getFlowSetData}
            />}
        </PageContainer>
    );

}
export default Query;


import React, { useState, useReducer, useEffect, useContext } from 'react';
import WindowBox from '../../../Components/Dialogs/WindowBox';
import { Grid, GridNoRecords, GridColumn } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import FlowDetailService from './flowDetail.service';
import { GetBasicData } from '../../../Basic/BasicData';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import { IsNullOrEmpty, FormatDate } from '../../../Basic/SDOExtension';
import SubflowDetail from './SubflowDetail';

const FlowDetail = props => {
    /* useContext */
    const { showMessage } = useContext(MessageBoxContext);
    const { showConfirmBox } = useContext(ConfirmBoxContext);

    /* useState */
    const [data, setData] = useState([]);
    const [count, setCount] = useState(0);
    const [page, setPage] = useState({ take: 20, skip: 0 });
    const [isFlowSysRole, setIsFlowSysRole] = useState(false);
    const [ignore, forceUpdate] = useReducer(x => x + 1, 0);
    const service = FlowDetailService();

    /* useEffect */
    /* 載入grid資料*/
    useEffect(() => {
        getFlowSysRole();
        loadFlowData();
    }, []);

    /* function */
    const loadFlowData = async () => {
        let data = await service.loadFlowData(props.flowCode);
        setData(data);
        setCount(data.length);
    }

    /* 判斷是否為流程管理角色 */
    const getFlowSysRole = async () => {
        let userId = await GetBasicData("userId");
        let dimRole = await service.getDimRole(userId);
        for (let role of dimRole) {
            if (role.ROLE_ID === 'FlowSysRole') {
                setIsFlowSysRole(true);
                break;
            }
        }
    }

    /* Row已簽核顯示灰色 */
    const customRowRender = (tr, props) => {
        let color = props.dataItem.has_signed ? "lightgray" : "black";
        let className = props.dataItem.NO % 2 ? "k-master-row" : "k-master-row k-alt";
        return (
            <tr className={className} style={{ color: color }} >{props.children}</tr>
        );
    }

    /* 自訂grid展開按鈕圖示 */
    const expandCellStyle = props => {
        let icon = "k-icon k-i-arrow-60-right"
        if (props.dataItem.expanded) icon = "k-icon k-i-collapse-se"
        return props.dataItem.has_subflow ?
            <td><Button icon={icon} onClick={() => { expandChange(props) }}></Button></td> :
            <td></td>
    }

    /* subflow展開 */
    const expandChange = async event => {
        if (event.dataItem.has_subflow) {
            event.dataItem.expanded = !event.dataItem.expanded;
            if (event.dataItem.expanded) {
                event.dataItem.detail = await service.loadSubflowData(event.dataItem.flow_code, event.dataItem.base_odr);
            }
            forceUpdate();
        }
    }

    /* 功能工具列設定 */
    const toolCellStyle = (data) => {
        return (
            (
                !data.dataItem.has_signed &&
                data.dataItem.status === 1 &&
                props.isAdmin &&
                isFlowSysRole
            ) ?
                <td>
                    <Button icon="delete" onClick={() => {
                        deleteFlow(data.dataItem.flow_code);
                    }}></Button>
                    <Button icon="reset" onClick={() => {
                        resetFlow(data.dataItem.flow_code);
                    }}></Button>
                </td>
                :
                <td></td>
        );
    }

    /* 取消流程 */
    const deleteFlow = async (flowCode) => {
        showConfirmBox("確定要取消流程嗎？", async () => {
            showResult(await service.deleteFlow(flowCode), '取消流程');
        });
    }

    /* 重設流程 */
    const resetFlow = async (flowCode) => {
        showConfirmBox("確定要重設流程嗎？", async () => {
            showResult(await service.resetFlow(flowCode), '重設流程');
        });
    }

    /* 取消分會流程 */
    const deleteSubflow = async (subflowCode) => {
        showConfirmBox("確定要取消分會流程嗎？", async () => {
            showResult(await service.deleteSubflow(subflowCode), '取消分會流程');
        });
    }

    /* 顯示結果 */
    const showResult = (response, resultMessage) => {
        if (response.message) {
            showMessage(response.message);
        }
        else {
            showMessage(resultMessage + (response.success ? '成功' : '失敗'), {
                onOkAction: () => { loadFlowData(); }
            })
        }
    }

    /* 流程關卡欄位合併 */
    const formatFlow = (props) => {
        let data = props.dataItem;
        return <td>
            {!IsNullOrEmpty(data.org_name) ? data.org_name + ' ' :
                (!IsNullOrEmpty(data.org_id) ? data.org_id + ' ' : '')}
            {!IsNullOrEmpty(data.role_name) ? '(' + data.role_name + ') ' :
                (!IsNullOrEmpty(data.role_id) ? '(' + data.role_id + ') ' : '')}
            {data.user_name ?? (data.user_id ?? '')}
        </td>
    }

    /* date format */
    const formatDate = (props) => {
        let data = props.dataItem;
        return <td>{data.has_signed ? FormatDate(props.dataItem.mdf_date, "tYY/MM/DD HH:mm:ss") : ''}</td>
    }

    /* grid換頁處理 */
    const onPageChange = (event) => {
        setPage({
            take: event.page.take,
            skip: event.page.skip
        })
    }

    return (
        <WindowBox
            title={'流程日誌-流程關卡'}
            onClose={props.onClose}
        >
            <Grid
                style={{
                    width: '100%',
                    height: '100%',
                    overflow: 'auto'
                }}
                data={data.slice(page.skip, page.skip + page.take)}
                pageSize={page.take}
                skip={page.skip}
                total={count}
                resizable={true}
                scrollable="scrollable"
                pageable={{
                    info: true,
                    type: 'numeric',
                    pageSizes: true,
                    previousNext: true
                }}
                onPageChange={onPageChange}
                detail={data =>
                    <SubflowDetail
                        {...data}
                        formatDate={formatDate}
                        customCellStyle={formatFlow}
                        customRowRender={customRowRender}
                        isAdmin={props.isAdmin && isFlowSysRole}
                        onEdit={loadFlowData}
                        deleteSubflow={deleteSubflow}
                    />
                }
                expandField="expanded"
                rowRender={customRowRender}
            >
                <GridNoRecords> </GridNoRecords>
                <GridColumn title=" " cell={expandCellStyle} width={30} />
                <GridColumn title=" " field="NO" width={30} />
                <GridColumn cell={toolCellStyle} width={80} />
                <GridColumn title="流程代碼" field="flow_code" />
                <GridColumn title="流程關卡" cell={formatFlow} />
                <GridColumn title="簽核狀態" field="status_name" />
                <GridColumn title="簽核時間" field="mdf_date" width={150} cell={formatDate} />
            </Grid>
        </WindowBox>
    );
}

export default FlowDetail;
import React, { useState, useContext, useEffect } from 'react';
import { Button, } from '@progress/kendo-react-buttons'
import { Grid, GridColumn } from '@progress/kendo-react-grid';
import AddMdf from './FlowRole-AddMdf/FlowRoleAddMdf'
import FlowRoleService from './flowRole.service';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';

const FlowRole = () => {
    const { showMessage } = useContext(MessageBoxContext);
    const { showConfirmBox } = useContext(ConfirmBoxContext);

    const [flowRoleData, setFlowRoleData] = useState([]); // grid資料
    const [skip, setSkip] = useState(0);
    const [take, setTake] = useState(10);
    const [pageable, setPageable] = useState({
        buttonCount: 5,
        info: true,
        type: 'numeric',
        pageSizes: true,
        previousNext: true
    });
    const [addMdfVisible, setAddMdfVisible] = useState({
        dataItem: undefined,
        visible: false
    }); //是否顯示新增修改視窗, param

    const service = FlowRoleService();

    const customItemCell = (props) => {
        return (
            <td>
                <Button icon="edit" look="bare" title={"修改"} onClick={() => { openOrCloseFlowRoleWindow(props.dataItem) }} />
                <Button icon="delete" look="bare" title={"刪除"} onClick={() => { openCheckDialog(props.dataItem) }} />
            </td>
        );
    }

    useEffect(() => {
        getFlowRoleData();
    }, [])

    // 取得流程角色資料
    const getFlowRoleData = async () => {
        setFlowRoleData(await service.getFlowRoleData())
    }

    const openCheckDialog = (dataItem) => {
        showConfirmBox('進行刪除作業，確定嗎？', () => {
            deleteFlowRoleData(dataItem.ROLE_ID);
        })
    }

    // 開啟or關閉新增修改視窗
    const openOrCloseFlowRoleWindow = (dataItem) => {
        setAddMdfVisible({
            visible: !addMdfVisible.visible,
            dataItem: dataItem ?? {}
        });
    }

    //完成新增修改 更新grid
    const onFinishHandler = () => {
        openOrCloseFlowRoleWindow();
        getFlowRoleData();
    }

    // 刪除流程角色
    const deleteFlowRoleData = async (roleId) => {
        let response = await service.deleteFlowRoleData(roleId);
        let responseData = await response.json();
        //顯示結果視窗
        showMessage(responseData.message, {
            onOkAction: () => { getFlowRoleData() }
        })
    }

    const pageChange = (event) => {
        setSkip(event.page.skip);
        setTake(event.page.take);
    }

    return (
        <div className="fnForm">
            <div className="fn-buttons">
                <Button value="Query" onClick={getFlowRoleData} >查詢</Button>
                <Button onClick={() => openOrCloseFlowRoleWindow()}>新增</Button>
            </div>
            <Grid
                className="expand"
                data={flowRoleData.slice(skip, take + skip)}
                skip={skip}
                take={take}
                total={flowRoleData.length}
                pageable={pageable}
                onPageChange={pageChange}
            >
                <GridColumn width="30px" field="NO" title="No" />
                <GridColumn cell={customItemCell} width={70} />
                <GridColumn field="ROLE_ID" title="角色代碼" />
                <GridColumn field="ROLE_NAME" title="角色名稱" />
                <GridColumn field="USER_COUNT" title="帳號數" />
            </Grid>

            {/* 新增or修改視窗 */}
            {addMdfVisible.visible && <AddMdf
                roleId={addMdfVisible.dataItem.ROLE_ID}
                onClose={openOrCloseFlowRoleWindow}
                onFinish={onFinishHandler}
            />}
        </div>
    );
}

export default FlowRole;
import React, { useState, useEffect, useContext, useCallback } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import {  Pageable } from '../../../Basic/BasicData';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import AddMdf from './MailRole-AddMdf/MailRoleAddMdf';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox'
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import { PageContainer } from '../../../Basic/PageContainer';
import MailRoleService from './mailRole.service';

const EditCell = (showAddMdf, showDelete) => {
    return props => {
        return(
            <td>
                <Button icon='edit' look='bare' onClick={() => showAddMdf(props.dataItem.ROLE_ID)}></Button>
                <Button icon='close' look='bare' onClick={() => showDelete(props.dataItem.ROLE_ID)}></Button>
            </td>
        )
    }
}

const Query = props => {
    const service = MailRoleService();
    const [gridWidth, setGridWidth] = useState(0);
    const gridRef = useCallback(grid => {
        if (grid !== null)
            setGridWidth(grid.element.offsetWidth);
    }, []);
    const [addMdfState, setAddMdfState] = useState({
        visible: false,
        roleId: null
    })
    const [searchResult, setSearchResults] = useState({
        mailRoles:[]
    })
    const { showMessage } = useContext(MessageBoxContext);
    const { showConfirmBox } = useContext(ConfirmBoxContext);

    const setWidth = percentage => {
        return Math.round(gridWidth * (percentage / 100));
    }

    const loadData = async () => {
        setSearchResults({
            mailRoles: await service.loadMailRole()
        })
    }

    const deleteRole = async roleId => {
        if(!IsNullOrEmpty(roleId)){
            let response = await service.deleteMailRole(roleId);
            showMessage(response.result.message);
            return response.ok;
        }
        return false
    }

    const showAddMdf = (roleId = null) => {
        setAddMdfState({
            visible:true,
            roleId: roleId,
        })
    }
    const showDelete = roleId => {
        showConfirmBox(
            '進行刪除作業，確定嗎?',
            async () => {
                if(await deleteRole(roleId))
                    loadData();
            }
        )
    }

    useEffect(() => {
        if(!addMdfState.visible)
            loadData();
    },[addMdfState.visible])

    useEffect(() => {
        loadData();
    },[])

    return(
        <PageContainer
            toolbar={[
                <Button key='searchButton' onClick={() => loadData()}>查詢</Button>,
                <Button key='createButton' onClick={() => showAddMdf()}>新增</Button>
            ]}
        >
            <div>
                <Grid
                    ref={gridRef}
                    style={{ height: '700px' }}
                    data={searchResult.mailRoles}
                    total={searchResult.mailRoles.length}
                    pageSize={10}
                    pageable={Pageable}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn width={setWidth(3)} field="NO" title="No"/>
                    <GridColumn width={setWidth(7)} title="" cell={EditCell(showAddMdf, showDelete)} />
                    <GridColumn field="ROLE_ID" title="角色代碼"  />
                    <GridColumn field="ROLE_NAME" title="角色名稱"  />
                    <GridColumn field="USER_COUNT" title="帳號數"  />
                </Grid>
            </div>
            <AddMdf 
                visible={addMdfState.visible} 
                roleId={addMdfState.roleId} 
                onClose={() => {
                    setAddMdfState({
                        visible:false,
                        roleId: null,
                    });
                }}
            />
        </PageContainer>
    )
}

export default Query;
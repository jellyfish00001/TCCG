import React, { useState, useEffect, useContext } from 'react';
import DimRoleService from '../DimRole/dimRole.service'
import { Pageable } from '../../../Basic/BasicData';
import { PageContainer } from '../../../Basic/PageContainer';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import { Grid, GridColumn } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import AddMdf from './DimRole-AddMdf/DimRoleAddMdf'

const Query = () => {
    const [paging, setPaging] = useState({ skip: 0, take: 10 })
    const [roleData, setRoleData] = useState([])//角色資料
    const [isAddMdf, setIsAddMdf] = useState({ visible: false, roleId: "", title: "" })

    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        //取得角色清單
        getDimRole()
    }, []);

    //取得角色清單
    const getDimRole = async () => {
        setRoleData(await DimRoleService.getDimRole())
    }

    const isDelte = (roleId) => {
        showConfirmBox("進行刪除作業，確定嗎？", () => { deleteDimRole(roleId) })
    }

    //刪除角色 
    const deleteDimRole = async (roleId) => {
        let response = await DimRoleService.deleteDimRole(roleId)
        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    getDimRole()
                }
            }
        })
    }

    const toolbar = [
        <Button onClick={getDimRole}>查詢</Button>,
        <Button onClick={() => setIsAddMdf({ visible: true, roleId: "", title: "新增" })}>新增</Button>
    ]

    return (
        <PageContainer
            toolbar={toolbar}
        >
            <Grid
                style={{
                    height: '100%',
                    overflow: 'auto'
                }}
                data={roleData.slice(paging.skip, paging.take + paging.skip)}
                total={roleData.length}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
            >
                <GridColumn field="NO" title="No" width={30} />
                <GridColumn width={70} cell={(props) =>
                    <td>
                        <Button icon="edit" look="bare" name="update" onClick={() => setIsAddMdf({ visible: true, roleId: props.dataItem.ROLE_ID, title: "修改" })} />
                        <Button icon="close" look="bare" onClick={() => isDelte(props.dataItem.ROLE_ID)} />
                    </td>
                } />
                <GridColumn field="ROLE_ID" title="角色代碼" />
                <GridColumn field="ROLE_NAME" title="角色名稱" />
            </Grid>

            {/* 新增or修改視窗 */}
            {isAddMdf.visible && <AddMdf
                title={isAddMdf.title}
                closeWindow={() => setIsAddMdf({ visible: false, roleId: "", title: "" })}
                roleId={isAddMdf.roleId}
                refreshGrid={getDimRole}
            />}
        </PageContainer>
    );
}

export default Query;


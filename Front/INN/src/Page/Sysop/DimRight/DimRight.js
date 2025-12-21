import React, { useState, useEffect, useContext } from 'react';
import DimRightService from '../DimRight/dimRight.service';
import { Pageable } from '../../../Basic/BasicData';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn } from '@progress/kendo-react-grid';
import { PageContainer } from '../../../Basic/PageContainer';
import AddMdf from './DimRight-AddMdf/DimRightAddMdf'
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';

const Query = () => {
    const [rightData, setRightData] = useState([])//權利資料
    const [paging, setPaging] = useState({ skip: 0, take: 10 })
    const [isAddMdf, setIsAddMdf] = useState({ visible: false, rightId: "", title: "" })

    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        //取得權限清單
        getDimRight()
    }, []);

    //取得權限清單
    const getDimRight = async () => {
        setRightData(await DimRightService.getDimRight())
    }

    const isDelte = (rightId) => {
        showConfirmBox("進行刪除作業，確定嗎？", () => { deleteDimRight(rightId) })
    }

    //刪除權限
    const deleteDimRight = async (rightId) => {
        let response = await DimRightService.deleteDimRight(rightId)
        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    getDimRight()
                }
            }
        })
    }

    const toolbar = [
        <Button onClick={getDimRight}>查詢</Button>,
        <Button onClick={() => setIsAddMdf({ visible: true, rightId: '', title: "新增" })}>新增</Button>
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
                data={rightData.slice(paging.skip, paging.take + paging.skip)}
                total={rightData.length}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
            >
                <GridColumn field="NO" title="No" width={30} />
                <GridColumn width={70} cell={(props) =>
                    <td>
                        <Button icon="edit" look="bare" title={"修改"} onClick={() => setIsAddMdf({ visible: true, rightId: props.dataItem.RIGHT_ID, title: "修改" })} />
                        <Button icon="close" look="bare" title={"刪除"} onClick={() => isDelte(props.dataItem.RIGHT_ID)} />
                    </td>
                } />
                <GridColumn field="RIGHT_ID" title="權限代碼" />
                <GridColumn field="RIGHT_NAME" title="權限名稱" />
            </Grid>

            {/* 新增or修改視窗 */}
            {isAddMdf.visible && <AddMdf
                title={isAddMdf.title}
                closeWindow={() => setIsAddMdf({ visible: false, rightId: '', title: '' })}
                rightId={isAddMdf.rightId}
                refreshGrid={getDimRight}
            />}
        </PageContainer>
    );
}

export default Query;

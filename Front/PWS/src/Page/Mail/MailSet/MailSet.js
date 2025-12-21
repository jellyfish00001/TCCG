import React, { useState, useEffect, useContext, useCallback } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Pageable } from '../../../Basic/BasicData';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import AddMdf from './MailSet-AddMdf/MailSetAddMdf';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import MailSetService from './mailSet.service';

const service = MailSetService();

const EditCell = (openCreate, showDelete) => {
    return props => {
        return (
            <td>
                <Button icon='edit' look='bare' onClick={() => openCreate(false, props.dataItem.MAIL_ID)}></Button>
                <Button icon='close' look='bare' onClick={() => showDelete(props.dataItem.MAIL_ID)}></Button>
            </td>
        )
    }
}

const Query = props => {
    const { showMessage } = useContext(MessageBoxContext);
    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const [gridWidth, setGridWidth] = useState(0);
    const [paging, setPaging] = useState({ skip: 0, take: 10 })
    const gridRef = useCallback(grid => {
        if (grid !== null)
            setGridWidth(grid.element.offsetWidth);
    }, []);
    const [searchResult, setSearchResults] = useState({
        mailSet: []
    })
    const [createState, setCreateState] = useState({
        visible: false,
        isCreate: true,
        mailId: '',
    })

    const setWidth = percentage => {
        return Math.round(gridWidth * (percentage / 100));
    }

    const loadData = async () => setSearchResults({ ...searchResult, mailSet: await service.loadMailSet() })

    const openCreate = (isCreate, modifyMailId) => {
        setCreateState({
            visible: true,
            isCreate: isCreate,
            mailId: modifyMailId,
        })
    }

    const closeCreate = async () => {
        setCreateState({
            visible: false,
            isCreate: true,
            mailId: '',
        })

        loadData();
    }

    const deleteMailSet = async mailId => {
        if (IsNullOrEmpty(mailId))
            return false;

        let response = await service.deleteMailSet(mailId);
        showMessage(response.result.message)
        return response.ok;
    }

    const showDelete = mailId => {
        showConfirmBox('進行刪除作業，確定嗎?',
            async () => {
                if (await deleteMailSet(mailId))
                    loadData();
            }
        )
    }

    useEffect(() => {
        loadData();
    }, [])

    return (
        <>
            <div className="fn-buttons">
                <Button onClick={() => loadData()}>查詢</Button>
                <Button onClick={() => openCreate(true, '')}>新增</Button>
            </div>
            <Grid
                ref={gridRef}
                style={{
                    // 動態計算Grid的高度
                    height: (searchResult.mailSet.slice(paging.skip, paging.take + paging.skip)).length === 0 ?
                        '100%'
                        :
                        36 * (searchResult.mailSet.slice(paging.skip, paging.take + paging.skip)).length + 62.45,
                    overflow: 'auto'
                }}
                data={searchResult.mailSet.slice(paging.skip, paging.take + paging.skip)}
                total={searchResult.mailSet.length}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
            >
                <GridNoRecords> </GridNoRecords>
                <GridColumn width={setWidth(2)} field="NO" title="No" />
                <GridColumn width={setWidth(5)} title="" cell={EditCell(openCreate, showDelete)} />
                <GridColumn field="MAIL_NAME" title="範本名稱" />
                <GridColumn field="MAIL_SUBJECT" title="範本主旨" />
            </Grid>
            <AddMdf
                visible={createState.visible}
                mailId={createState.mailId}
                isCreate={createState.isCreate}
                onClose={closeCreate} />
        </>
    )
}

export default Query;
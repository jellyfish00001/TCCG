import React, { useState, useEffect, useContext, useCallback } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Pageable } from '../../../Basic/BasicData';
import { Button } from '@progress/kendo-react-buttons';
import 'bootstrap/dist/css/bootstrap.min.css';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import AddMdf from './MailQueue-AddMdf/MailQueueAddMdf';
import MailQueueService from './mailQueue.service';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import { PageContainer } from '../../../Basic/PageContainer';

const service = MailQueueService();

const EditCell = (openCreate, showDelete) => {
    return props => {
        return (
            <td>
                <Button icon={props.dataItem.SEND_FLG !== 1 ? 'edit' : 'search'} look='bare' onClick={() => openCreate(props.dataItem.QUEUE_ID)}></Button>
                {props.dataItem.SEND_FLG !== 1 && //未寄出
                    <Button icon='close' look='bare' onClick={() => showDelete(props.dataItem.QUEUE_ID)}></Button>
                }
            </td>
        )
    }
}

const mailAddressFromCell = props => {
    return (
        <td>
            {props.dataItem[props.field].MAIL_NAME + '(' + props.dataItem[props.field].MAIL_ADDR + ')'}
        </td>
    )
}

const sendFlgCell = props => {
    return (
        <td>
            {props.dataItem[props.field] === 1 ? '已' : '未'}寄出
        </td>
    )
}

const DateCell = props => {
    let value = props.dataItem[props.field];
    let date = new Date(value);
    return (
        //yyyy/MM/dd HH:mm:ss
        <td>{date.getFullYear()}/{date.getMonth() + 1}/{date.getDate()} {date.getHours()}:{date.getMinutes()}:{date.getSeconds()}</td>
    )
}

const sendDateCell = props => {
    let value = props.dataItem[props.field];
    let date = new Date(value);
    let sendFlg = props.dataItem.SEND_FLG === 1
    let display = sendFlg ?
        date.getFullYear() + '/' + (date.getMonth() + 1) + '/' + date.getDate() + ' ' + date.getHours() + ':' + date.getMinutes() + ':' + date.getSeconds()
        : ''
    return (
        //yyyy/MM/dd HH:mm:ss
        <td>{display}</td>
    )
}

const Query = props => {
    const { showMessage } = useContext(MessageBoxContext);
    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const [gridWidth, setGridWidth] = useState(0);
    const gridRef = useCallback(grid => {
        if (grid !== null)
            setGridWidth(grid.element.offsetWidth);
    }, []);
    const [searchResult, setSearchResults] = useState({
        mailQueue: []
    })
    const [addMdfState, setAddMdfState] = useState({
        visible: false,
        queueId: '',
    })

    const setWidth = percentage => {
        return Math.round(gridWidth * (percentage / 100));
    }

    const loadMailQueue = async () => setSearchResults({ mailQueue: await service.loadMailQueue() })

    const openCreate = queueId => {
        setAddMdfState({
            visible: true,
            queueId: queueId,
        })
    }

    const closeCreate = () => {
        setAddMdfState({
            visible: false,
            queueId: '',
        })
    }

    const deleteQueue = async queueId => {
        if (IsNullOrEmpty(queueId))
            return false;

        let response = await service.deleteQueue(queueId);
        showMessage(response.result.message)
        return response.ok;
    }

    const showDelete = queueId => {
        showConfirmBox('進行刪除作業，確定嗎?',
            async () => {
                if (await deleteQueue(queueId))
                    loadMailQueue();
            }
        )
    }

    useEffect(() => {
        if (!addMdfState.visible)
            loadMailQueue();
    }, [addMdfState.visible])

    useEffect(() => {
        loadMailQueue();
    }, [])

    return (
        <PageContainer
            toolbar={
                <>
                    <Button onClick={() => loadMailQueue()}>查詢</Button>
                    <Button onClick={() => openCreate('')}>新增</Button>
                </>
            }
        >
            <div>
                <Grid
                    ref={gridRef}
                    style={{ height: '700px' }}
                    data={searchResult.mailQueue}
                    total={searchResult.mailQueue.length}
                    pageSize={10}
                    pageable={Pageable}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn width={setWidth(2)} field="NO" title="No" />
                    <GridColumn width={setWidth(5)} title="" cell={EditCell(openCreate, showDelete)} />
                    <GridColumn field="MAIL_ADDRESS_FROM" title="寄件者" cell={mailAddressFromCell} />
                    <GridColumn field="MAIL_SUBJECT" title="郵件主旨" />
                    <GridColumn field="CRT_DATE" title="建立日期" cell={DateCell} />
                    <GridColumn field="SEND_FLG" title="寄出狀態" cell={sendFlgCell} />
                    <GridColumn field="SEND_TIME" title="寄出時間" cell={sendDateCell} />
                </Grid>
            </div>
            <AddMdf
                visible={addMdfState.visible}
                queueId={addMdfState.queueId}
                onClose={closeCreate} />
        </PageContainer>
    )
}

export default Query;
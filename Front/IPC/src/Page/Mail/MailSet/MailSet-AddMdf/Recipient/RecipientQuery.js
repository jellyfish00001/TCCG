import React, { useState, useEffect, useContext, useCallback } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Pageable } from '../../../../../Basic/BasicData';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty } from '../../../../../Basic/SDOExtension';
import AddMdf from './RecipientAddMdf';
import MailSetService from '../../mailSet.service';
import { ConfirmBoxContext } from '../../../../../Components/Dialogs/ConfirmBox';
import { MessageBoxContext } from '../../../../../Components/Dialogs/MessageBox';
import { PageContainer } from '../../../../../Basic/PageContainer';

const service = MailSetService();

//功能按鈕
const EditCell = (openCreate, showDelete) => {
    return props => {
        return (
            <td>
                <Button icon='edit' look='bare' onClick={() => openCreate(false, props.dataItem.REFERENCE)}></Button>
                <Button icon='close' look='bare' onClick={() => showDelete(props.dataItem.REFERENCE)}></Button>
            </td>
        )
    }
}

//郵寄對象template
const RecipientTitleCell = props => {
    let kind = props.dataItem.RECIPIENT_KIND;
    return (
        <td>
            <span
                style={{
                    color: 'white',
                    padding: '5px',
                    borderRadius: '5px',
                    marginRight: '5px',
                    backgroundColor: kind === 'ADDR' ? '#FF5722' : '#4CAF50'
                }}
            >
                {kind === 'ADDR' ? '電子郵件' : '郵件角色'}
            </span>
            {props.dataItem.RECIPIENT_TITLE}
        </td>
    )
}

const Query = props => {
    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const { showMessage } = useContext(MessageBoxContext);
    const [gridWidth, setGridWidth] = useState(0);
    const gridRef = useCallback(grid => {
        if (grid !== null)
            setGridWidth(grid.element.offsetWidth);
    }, []);
    const [recipients, setRecipients] = useState([]);
    const [displayData, setDisplayData] = useState([]);
    const [mailType, setMailType] = useState({ data: [], loaded: false });
    const [mailRole, setMailRole] = useState({ data: [], loaded: false });
    const [modifyState, setModifyState] = useState({
        visible: false,
        mailId: '',
        recipient: {
            MAIL_ID: '',
            MAIL_TITLE: '',
            MAIL_ROLE: '',
            MAIL_ADDRESS: '',
            MAIL_TYPE: '',
        },
        isCreate: true,
        editIndex: -1,
        recipients: [],
    });

    const setWidth = percentage => {
        return Math.round(gridWidth * (percentage / 100));
    }

    const loadRecipient = async () => setRecipients(await service.loadRecipientById(props.mailId))

    const loadMailType = async () => setMailType({ data: await service.loadMailType(), loaded: true });

    const loadMailRole = async () => setMailRole({ data: await service.loadMailRole(), loaded: true });

    const openCreate = (isCreate, currentRecipient) => {
        setModifyState({
            visible: true,
            mailId: props.mailId,
            isCreate: isCreate,
            recipient: isCreate ? modifyState.recipient : currentRecipient,
            editIndex: isCreate ? -1 : recipients.findIndex(x => x === currentRecipient),
            recipients: recipients,
        })
    }

    const closeCreate = async (submit, newRecipients = null) => {
        setModifyState({
            visible: false,
            mailId: '',
            recipient: {
                MAIL_ID: '',
                MAIL_TITLE: '',
                MAIL_ROLE: '',
                MAIL_ADDRESS: '',
                MAIL_TYPE: '',
            },
            isCreate: true,
            editIndex: -1,
            recipients: [],
        });
        setRecipients(submit && newRecipients !== null ? newRecipients : recipients)
    }

    const deleteRecipient = deleteIndex => {
        let newRecipients = [...recipients];
        newRecipients.splice(deleteIndex, 1);
        setRecipients(newRecipients);
    }

    const showDelete = deleteIndex => {
        showConfirmBox(
            '進行刪除作業，確定嗎?',
            () => {
                deleteRecipient(deleteIndex);
            }
        )
    }

    const submit = async () => {
        let response = await service.updateRecipients(props.mailId, recipients);
        showMessage(response.result.message);
    }

    useEffect(() => {
        let newDisplayData = [];
        recipients.forEach(recipient => {
            newDisplayData.push(
                {
                    RECIPIENT_TYPE: mailType.data.find(mailType => mailType.SET_TYPE === recipient.MAIL_TYPE)?.SET_VALUE,
                    RECIPIENT_TITLE: IsNullOrEmpty(recipient.MAIL_ROLE) ?
                        recipient.MAIL_TITLE + '(' + recipient.MAIL_ADDRESS + ')' :
                        mailRole.data.find(role => role.ROLE_ID === recipient.MAIL_ROLE)?.ROLE_NAME,
                    RECIPIENT_KIND: IsNullOrEmpty(recipient.MAIL_ROLE) ? 'ADDR' : 'ROLE',
                    REFERENCE: recipient
                }
            );
        });

        setDisplayData(newDisplayData);
    }, [recipients])

    useEffect(() => {
        if (props.visible) {
            loadRecipient();
        }
        else {
            setRecipients([]);
            setModifyState({
                visible: false,
                mailId: '',
                recipient: {
                    MAIL_ID: '',
                    MAIL_TITLE: '',
                    MAIL_ROLE: '',
                    MAIL_ADDRESS: '',
                    MAIL_TYPE: '',
                },
                isCreate: true,
                editIndex: -1,
                recipients: [],
            });
        }
    }, [props.visible, props.mailId])

    useEffect(() => {
        const init = async () => {
            await loadMailType();
            await loadMailRole();
            await loadRecipient();
        }

        init();
    }, []);

    return (
        <PageContainer
            toolbar={
                <>
                    <Button onClick={submit}>存檔</Button>
                    <Button onClick={() => loadRecipient()}>重設</Button>
                    <Button onClick={() => openCreate(true, '')}>新增</Button>
                </>
            }
        >
            <div>
                <Grid
                    ref={gridRef}
                    style={{ height: '700px' }}
                    data={displayData}
                    total={displayData.length}
                    pageSize={10}
                    pageable={Pageable}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn width={setWidth(7)} title="" cell={EditCell(openCreate, showDelete)} />
                    <GridColumn field="RECIPIENT_TYPE" title="寄件類型" />
                    <GridColumn field="RECIPIENT_TITLE" title="寄件對象" cell={RecipientTitleCell} />
                </Grid>
            </div>
            <AddMdf
                visible={modifyState.visible}
                isCreate={modifyState.isCreate}
                mailId={modifyState.mailId}
                editIndex={modifyState.editIndex}
                recipients={modifyState.recipients}
                currentRecipient={modifyState.recipient}
                onClose={closeCreate} />
        </PageContainer>
    )
}

export default Query;
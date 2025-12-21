import React, { useState, useEffect } from 'react'
import { IsNullOrEmpty } from '../../../../../Basic/SDOExtension';
import MailSetService from '../../mailSet.service';
import { Button } from '@progress/kendo-react-buttons';
import { Label, Error } from '@progress/kendo-react-labels';
import { DropDownList } from '@progress/kendo-react-dropdowns'
import { Input, RadioButton } from '@progress/kendo-react-inputs';
import WindowBox from '../../../../../Components/Dialogs/WindowBox';
import { PageContainer } from '../../../../../Basic/PageContainer';

const service = MailSetService();

const AddMdf = props => {
    const [recipient, setRecipient] = useState({
        MAIL_ID: '',
        MAIL_TITLE: '',
        MAIL_ROLE: '',
        MAIL_ADDRESS: '',
        MAIL_TYPE: '',
    });
    const [mailType, setMailType] = useState({
        data: [],
        loaded: false
    });
    const [mailRole, setMailRole] = useState({
        data: [],
        loaded: false
    });
    const [validateSet, setValidateSet] = useState({
        mailTypeErrorVisible: false,
        mailRoleErrorVisible: false,
        mailTitleErrorVisible: false,
        mailAddressErrorVisible: false,
        mailAddressErrorMessage: ''
    });
    const [recipientKind, setRecipientKind] = useState('ADDR');

    const loadMailType = async () => setMailType({ data: await service.loadMailType(), loaded: true });

    const loadMailRole = async () => setMailRole({ data: await service.loadMailRole(), loaded: true });

    const validate = () => {
        let result = true;
        let validateSet = {
            mailTypeErrorVisible: false,
            mailRoleErrorVisible: false,
            mailTitleErrorVisible: false,
            mailAddressErrorVisible: false,
            mailAddressErrorMessage: ''
        }

        if (IsNullOrEmpty(recipient.MAIL_TYPE)) {
            result = false;
            validateSet.mailTypeErrorVisible = true;
        }


        if (recipientKind === 'ROLE' && IsNullOrEmpty(recipient.MAIL_ROLE)) {
            result = false;
            validateSet.mailRoleErrorVisible = true;
        }

        if (recipientKind === 'ADDR' && IsNullOrEmpty(recipient.MAIL_TITLE)) {
            result = false;
            validateSet.mailTitleErrorVisible = true;
        }

        if (recipientKind === 'ADDR') {
            if (IsNullOrEmpty(recipient.MAIL_ADDRESS)) {
                result = false;
                validateSet.mailAddressErrorVisible = true;
                validateSet.mailAddressErrorMessage = '請輸入郵件地址'
            }
            else {
                const emailRegex = new RegExp(/\S+@\S+\.\S+/); //ex:123@aaa.bbb
                if (!emailRegex.test(recipient.MAIL_ADDRESS)) {
                    result = false;
                    validateSet.mailAddressErrorVisible = true;
                    validateSet.mailAddressErrorMessage = '郵件地址格式錯誤'
                }
            }
        }

        setValidateSet(validateSet);

        return result;
    }

    const submit = () => {
        if (!validate()) {
            return;
        }

        let recipients = [...props.recipients];

        if (props.isCreate)
            recipients.push(recipient);
        else {
            recipients[props.editIndex] = recipient
        }

        props.onClose(true, recipients);
    }

    const restore = () => {
        setRecipient(props.currentRecipient);
        setValidateSet({
            mailTypeErrorVisible: false,
            mailRoleErrorVisible: false,
            mailTitleErrorVisible: false,
            mailAddressErrorVisible: false,
            mailAddressErrorMessage: ''
        });
        setRecipientKind(props.isCreate ? 'ADDR' : IsNullOrEmpty(props.currentRecipient.MAIL_ROLE) ? 'ADDR' : 'ROLE')
    }

    useEffect(() => {
        if (props.visible) {
            if (props.isCreate) {
                setRecipient({
                    ...recipient,
                    MAIL_ID: props.mailId,
                })
            }
            else {
                setRecipient(props.currentRecipient);
                setRecipientKind(IsNullOrEmpty(props.currentRecipient.MAIL_ROLE) ? 'ADDR' : 'ROLE')
            }
        }
        else {
            setRecipient({
                MAIL_ID: '',
                MAIL_TITLE: '',
                MAIL_ROLE: '',
                MAIL_ADDRESS: '',
                MAIL_TYPE: '',
            });
            setRecipientKind('ADDR')
        }
    }, [props.visible, props.isCreate, props.currentRecipient])

    useEffect(() => {
        loadMailType();
        loadMailRole();
    }, [])

    return (
        <div>
            {props.visible &&
                <WindowBox title={props.isCreate ? '新增' : '修改'} onClose={() => props.onClose(false)} width='70' height='50'>
                    <PageContainer
                        toolbar={
                            <>
                                <Button onClick={submit}>存檔</Button>
                                <Button onClick={restore}>重設</Button>
                            </>
                        }
                    >
                        <table style={{ width: '100%' }}>
                            <tbody>
                                <tr>
                                    <th>
                                        <Label>郵寄類型</Label>
                                    </th>
                                    <td>
                                        <DropDownList
                                            data={mailType.data}
                                            textField="SET_VALUE"
                                            dataItemKey="SET_TYPE"
                                            onChange={e => setRecipient({ ...recipient, MAIL_TYPE: e.value.SET_TYPE })}
                                            loading={!mailType.loaded}
                                            value={mailType.data.find(user => user.SET_TYPE === recipient.MAIL_TYPE)}
                                        />
                                        {validateSet.mailTypeErrorVisible && <Error>請選擇郵寄類型</Error>}
                                    </td>
                                </tr>
                                <tr>
                                    <th>
                                        <RadioButton
                                            value="ADDR"
                                            checked={recipientKind === 'ADDR'}
                                            onChange={e => setRecipientKind(e.value)} />
                                        <Label>電子郵件</Label>
                                    </th>
                                    <td>
                                        <Input
                                            value={recipient.MAIL_TITLE}
                                            onChange={e => setRecipient({ ...recipient, MAIL_TITLE: e.value })}
                                            label='郵件抬頭'
                                            disabled={recipientKind !== 'ADDR'}
                                        />
                                        {validateSet.mailTitleErrorVisible && <Error>請輸入郵件抬頭</Error>}
                                    </td>
                                    <td>
                                        <Input
                                            value={recipient.MAIL_ADDRESS}
                                            onChange={e => setRecipient({ ...recipient, MAIL_ADDRESS: e.value })}
                                            label='郵件地址'
                                            disabled={recipientKind !== 'ADDR'}
                                        />
                                        {validateSet.mailAddressErrorVisible && <Error>{validateSet.mailAddressErrorMessage}</Error>}
                                    </td>
                                </tr>
                                <tr>
                                    <th>
                                        <RadioButton
                                            value="ROLE"
                                            checked={recipientKind === 'ROLE'}
                                            onChange={e => setRecipientKind(e.value)} />
                                        <Label>郵寄角色</Label>
                                    </th>
                                    <td>
                                        <DropDownList
                                            data={mailRole.data}
                                            textField="ROLE_NAME"
                                            dataItemKey="ROLE_ID"
                                            onChange={e => setRecipient({ ...recipient, MAIL_ROLE: e.value.ROLE_ID })}
                                            loading={!mailRole.loaded}
                                            value={mailRole.data.find(role => role.ROLE_ID === recipient.MAIL_ROLE)}
                                            disabled={recipientKind !== 'ROLE'}
                                        />
                                        {validateSet.mailRoleErrorVisible && <Error>請選擇郵寄角色</Error>}
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </PageContainer>
                </WindowBox>
            }
        </div>
    )
}

export default AddMdf;
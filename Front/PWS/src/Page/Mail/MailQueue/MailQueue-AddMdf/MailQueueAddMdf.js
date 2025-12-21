import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { DropDownList } from '@progress/kendo-react-dropdowns';
import { Label } from '@progress/kendo-react-labels';
import MailQueueService from '../mailQueue.service';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { PageContainer } from '../../../../Basic/PageContainer';
import useVisible from '../../../../Hook/useVisible';
import useInVisible from '../../../../Hook/useInVisible';
import useMergeState from '../../../../Hook/useMergeState';
import TextInput from '../../../../Components/Input/TextInput'
import TextEditor from '../../../../Components/Editor/TextEditor'
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

const service = MailQueueService();

const AddMdf = props => {
    const { showMessage } = useContext(MessageBoxContext);
    const [mailQueue, setMailQueue] = useMergeState({
        QUEUE_ID: '',
        MAIL_ADDRESS_FROM: {
            MAIL_NAME: '',
            MAIL_ADDR: ''
        },
        MAIL_ADDRESS_TO: [{
            MAIL_NAME: '',
            MAIL_ADDR: ''
        }],
        MAIL_SUBJECT: '',
        MAIL_CONTENT: '',
    });
    const [onlyView, setOnlyView] = useState(false);
    const [mailSet, setMailSet] = useState({
        data: [],
        loaded: false,
    });
    const validationSchema = Yup.object().shape({
        MAIL_ADDRESS_FROM: Yup.object().shape({
            MAIL_NAME: Yup.string().required('寄件者抬頭為必填'),
            MAIL_ADDR: Yup.string().email('寄件者地址需為電子郵件').required('寄件者地址為必填')
        }),
        MAIL_ADDRESS_TO:Yup.array(Yup.object().shape({
            MAIL_NAME: Yup.string().required('寄件者抬頭為必填'),
            MAIL_ADDR: Yup.string().email('寄件者地址需為電子郵件').required('寄件者地址為必填')
        })),
        MAIL_SUBJECT: Yup.string().required('郵件主旨為必填'),
    })

    const loadMailQueueById = async queueId => {
        let result = await service.loadMailQueueById(queueId);
        if (result) {
            setMailQueue({
                QUEUE_ID: result.QUEUE_ID,
                MAIL_ADDRESS_FROM: result.MAIL_ADDRESS_FROM,
                MAIL_ADDRESS_TO: result.MAIL_ADDRESS_TO,
                MAIL_SUBJECT: result.MAIL_SUBJECT,
                MAIL_CONTENT: result.MAIL_CONTENT,
            })

            setOnlyView(result.SEND_FLG === 1)//已寄出不得編輯
        }
    }

    const loadMailSet = async () => setMailSet({ data: await service.loadMailSet(), loaded: true })

    const submit = async mailQueue => {
        let response = await (IsNullOrEmpty(props.queueId) ? service.insertQueue(mailQueue):service.updateQueue(mailQueue));

        showMessage(
            response.result.message,
            {
                onOkAction: () => {
                    if(response.ok)
                        props.onClose();
                }
            }
        )
    }

    useVisible(() => {
        if(!IsNullOrEmpty(props.queueId))
            loadMailQueueById(props.queueId);
    }, props.visible)

    useInVisible(() => {
        setMailQueue({
            QUEUE_ID: '',
            MAIL_ADDRESS_FROM: {
                MAIL_NAME: '',
                MAIL_ADDR: ''
            },
            MAIL_ADDRESS_TO: [{
                MAIL_NAME: '',
                MAIL_ADDR: ''
            }],
            MAIL_SUBJECT: '',
            MAIL_CONTENT: '',
        });

        setOnlyView(false);
    }, props.visible)

    useEffect(() => {
        loadMailSet();
    }, [])

    return(
        <div>
            {props.visible &&
                <WindowBox title={IsNullOrEmpty(props.queueId) ? '新增' : '修改'} onClose={props.onClose} width='70' height='80'>
                    <Formik
                        initialValues={mailQueue}
                        validationSchema={validationSchema}
                        onSubmit={data => submit(data)}
                        enableReinitialize={true}
                    >
                        {
                            /**
                             * @param {object} props
                             */
                            props => (
                                <Form>
                                    <PageContainer
                                        toolbar={
                                            <>
                                                <Button type="submit" disabled={onlyView}>存檔</Button>
                                                <Button type="reset" disabled={onlyView}>重設</Button>
                                            </>
                                        }
                                    >
                                        <table style={{ width: '100%' }}>
                                            <tbody>
                                                <tr>
                                                    <th></th>
                                                    <td>
                                                        <DropDownList
                                                            data={mailSet.data}
                                                            loading={!mailSet.loaded}
                                                            textField="MAIL_NAME"
                                                            dataItemKey="MAIL_ID"
                                                            onChange={e => {
                                                                let mailId = e.value.MAIL_ID;
                                                                const setTemplate = async mailId => {
                                                                    if(IsNullOrEmpty(mailId)){
                                                                        return;
                                                                    }
                                                        
                                                                    const template = await service.loadMailTemplate(mailId);
                                                                    props.setValues({
                                                                        ...props.values,
                                                                        ...template
                                                                    })
                                                                }

                                                                setTemplate(mailId);
                                                            }}
                                                            defaultItem={{ MAIL_NAME: '選擇範本...' }}
                                                            disabled={onlyView}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>
                                                        <Label>寄件者</Label>
                                                    </th>
                                                    <td>
                                                        <TextInput
                                                            name='MAIL_ADDRESS_FROM.MAIL_NAME'
                                                            value={props.values.MAIL_ADDRESS_FROM.MAIL_NAME}
                                                            readOnly={onlyView}
                                                            onChange={props.handleChange}
                                                            onBlur={props.handleBlur}
                                                            error={props.errors?.MAIL_ADDRESS_FROM?.MAIL_NAME}
                                                            label={'寄件者抬頭'}
                                                        />
                                                    </td>
                                                    <td>
                                                        <TextInput
                                                            name='MAIL_ADDRESS_FROM.MAIL_ADDR'
                                                            value={props.values.MAIL_ADDRESS_FROM.MAIL_ADDR}
                                                            readOnly={onlyView}
                                                            onChange={props.handleChange}
                                                            onBlur={props.handleBlur}
                                                            error={props.errors?.MAIL_ADDRESS_FROM?.MAIL_ADDR}
                                                            label={'寄件者地址'}
                                                        />
                                                    </td>
                                                    <td>

                                                    </td>
                                                    <th>
                                                        <Label>收件者</Label>
                                                    </th>
                                                    <td>
                                                        <TextInput
                                                            name='MAIL_ADDRESS_TO[0].MAIL_NAME'
                                                            value={props.values.MAIL_ADDRESS_TO[0].MAIL_NAME}
                                                            readOnly={onlyView}
                                                            onChange={props.handleChange}
                                                            onBlur={props.handleBlur}
                                                            error={
                                                                props.errors?.MAIL_ADDRESS_TO ? props.errors.MAIL_ADDRESS_TO[0].MAIL_NAME : ''}
                                                            label={'寄件者抬頭'}
                                                        />
                                                    </td>
                                                    <td>
                                                        <TextInput
                                                            name='MAIL_ADDRESS_TO[0].MAIL_ADDR'
                                                            value={props.values.MAIL_ADDRESS_TO[0].MAIL_ADDR}
                                                            readOnly={onlyView}
                                                            onChange={props.handleChange}
                                                            onBlur={props.handleBlur}
                                                            error={
                                                                props.errors?.MAIL_ADDRESS_TO ? props.errors.MAIL_ADDRESS_TO[0].MAIL_ADDR : ''}
                                                            label={'寄件者地址'}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>
                                                        <Label>郵件主旨</Label>
                                                    </th>
                                                    <td colSpan={7}>
                                                        <TextInput
                                                            name='MAIL_SUBJECT'
                                                            style={{ width: '100%' }}
                                                            value={props.values.MAIL_SUBJECT}
                                                            readOnly={onlyView}
                                                            onChange={props.handleChange}
                                                            onBlur={props.handleBlur}
                                                            error={props.errors?.MAIL_SUBJECT}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>
                                                        <Label>郵件內容</Label>
                                                    </th>
                                                    <td colSpan={7}>
                                                        <TextEditor
                                                            name='MAIL_CONTENT'
                                                            value={props.values.MAIL_CONTENT}
                                                            onChange={e => {
                                                                if(onlyView)
                                                                    return;

                                                                props.setValues({
                                                                    ...props.values,
                                                                    MAIL_CONTENT: e.html
                                                                })
                                                            }}
                                                            onBlur={props.handleBlur}
                                                        />
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </PageContainer>
                                </Form>
                            )
                        }
                    </Formik>
                </WindowBox>
            }
        </div>
    )
}

export default AddMdf;
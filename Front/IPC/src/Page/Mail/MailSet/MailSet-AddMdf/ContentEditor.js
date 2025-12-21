import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Label } from '@progress/kendo-react-labels';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { Editor, EditorTools } from '@progress/kendo-react-editor';
import MailSetService from '../mailSet.service';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { PageContainer } from '../../../../Basic/PageContainer';
import * as Yup from 'yup';
import { Formik } from 'formik';
import TextInput from '../../../../Components/Input/TextInput';

const {
    Bold, Italic, Underline, Strikethrough,
    ForeColor,
    AlignLeft, AlignCenter, AlignRight, AlignJustify,
    Indent, Outdent, OrderedList, UnorderedList,
} = EditorTools;

const ContentEditor = props => {
    const service = MailSetService();
    const { showMessage } = useContext(MessageBoxContext);
    const [mailSet, setMailSet] = useState({
        MAIL_ID: '',
        MAIL_NAME: '',
        MAIL_SUBJECT: '',
        MAIL_CONTENT: '',
    })

    const loadMailById = async mailId => {
        setMailSet(await service.loadMailSetById(mailId));
        props.setHasCreated(true, mailId);
    }

    const submit = async (data) => {

        let response = await (IsNullOrEmpty(props.mailId) ? service.insertMailSet(data) : service.updateMailSet(data));

        showMessage(response.result.message);

        props.setHasCreated(response.ok, data.MAIL_ID);
    }

    useEffect(() => {
        if (props.visible && !IsNullOrEmpty(props.mailId)) {
            loadMailById(props.mailId);
        }

        if (!props.visible) {
            setMailSet({
                MAIL_ID: '',
                MAIL_NAME: '',
                MAIL_SUBJECT: '',
                MAIL_CONTENT: '',
            });
        }
    }, [props.visible, props.mailId])

    const validateField = Yup.object().shape({
        MAIL_ID: Yup.string().required("範本編號必填"),
        MAIL_NAME: Yup.string().required("範本名稱"),
        MAIL_SUBJECT: Yup.string().required("郵件主旨"),
    })

    const mailId = props.mailId;
    return (
        <div>
            <Formik
                initialValues={mailSet}
                validationSchema={validateField}
                onSubmit={(data) => submit(data)}
                //允許重複賦予初始值
                enableReinitialize
            >
                {props => {
                    const {
                        values,
                        errors,
                        handleBlur,
                        handleSubmit,
                        handleReset,
                        handleChange,
                        setValues
                    } = props;
                    return (
                        <form onSubmit={handleSubmit} onReset={handleReset}>
                            <PageContainer
                                toolbar={
                                    <>
                                        <Button type="submit">存檔</Button>
                                        <Button type="reset">重設</Button>
                                    </>
                                }
                            >
                                <table style={{ width: "100%" }}>
                                    <tbody>
                                        <tr>
                                            <th>
                                                <Label>範本編號</Label>
                                            </th>
                                            <td>
                                                <TextInput
                                                    name="MAIL_ID"
                                                    value={values.MAIL_ID}
                                                    error={errors.MAIL_ID}
                                                    onChange={handleChange}
                                                    onBlur={handleBlur}
                                                    readOnly={!IsNullOrEmpty(mailId)}
                                                    disabled={!IsNullOrEmpty(mailId)}
                                                />
                                            </td>
                                            <th>
                                                <Label>範本名稱</Label>
                                            </th>
                                            <td>
                                                <TextInput
                                                    name="MAIL_NAME"
                                                    value={values.MAIL_NAME}
                                                    error={errors.MAIL_NAME}
                                                    onChange={handleChange}
                                                    onBlur={handleBlur}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                <Label>郵件主旨</Label>
                                            </th>
                                            <td colSpan={3}>
                                                <TextInput
                                                    name="MAIL_SUBJECT"
                                                    style={{ width: '100%' }}
                                                    value={values.MAIL_SUBJECT}
                                                    error={errors.MAIL_SUBJECT}
                                                    onChange={handleChange}
                                                    onBlur={handleBlur}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                <Label>郵件內容</Label>
                                            </th>
                                            <td colSpan={3}>
                                                <Editor
                                                    tools={[
                                                        [Bold, Italic, Underline, Strikethrough],
                                                        ForeColor,
                                                        [AlignLeft, AlignCenter, AlignRight, AlignJustify],
                                                        [Indent, Outdent],
                                                        [OrderedList, UnorderedList]
                                                    ]}
                                                    value={values.MAIL_CONTENT}
                                                    onChange={e => setValues({ ...values, MAIL_CONTENT: e.html })}
                                                />
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                                <div style={{ color: 'red', width:990}}>
                                    <ol>
                                        <li>變數請以"<b>$</b>"符號包住，並以大寫處理，例：$USER_TITLE$</li>
                                    </ol>
                                </div>
                            </PageContainer>
                        </form>
                    )
                }}
            </Formik>
        </div>
    )
}

export default ContentEditor;
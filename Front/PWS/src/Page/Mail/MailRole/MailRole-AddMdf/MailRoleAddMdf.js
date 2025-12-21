import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { Label } from '@progress/kendo-react-labels';
import { PageContainer } from '../../../../Basic/PageContainer';
import MailRoleService from '../mailRole.service';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../../../../Components/Input/TextInput';
import OrgUserSelector from '../../../../Components/Selector/OrgUserSelector';

const AddMdf = props => {
    const { showMessage } = useContext(MessageBoxContext);
    const service = MailRoleService();
    const [isAdd, setIsAdd] = useState(true);
    const [mailRole, setMailRole] = useState({
        ROLE_ID: '',
        ROLE_NAME: '',
        DEL_FLG: false,
        USERS: []
    });
    const [mailRoleUsers, setMailRoleUsers] = useState([]);

    const loadRoleData = async roleId => {
        const mailRole = await service.loadRoleById(roleId);
        const users = await service.loadRoleUsersById(roleId)

        setMailRole({
            ...mailRole,
            USERS: users.map(user => user.USER_ID),//設定預設值
        })

        setMailRoleUsers(users);
    }

    const submit = async mailRole => {
        let response = await (isAdd ? service.insertMailRole(mailRole) : service.updateMailRole(mailRole));

        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok)
                    props.onClose();
            }
        })
    }

    useEffect(() => {
        if (props.visible && !IsNullOrEmpty(props.roleId)) {
            loadRoleData(props.roleId);//讀取ROLE資料
        }

        if (!props.visible) {
            setIsAdd(true);
            setMailRole({
                ROLE_ID: '',
                ROLE_NAME: '',
                DEL_FLG: false,
                USERS: []
            });
            setMailRoleUsers([])
        }
    }, [props.visible, props.roleId])

    useEffect(() => {
        setIsAdd(IsNullOrEmpty(props.roleId));
    }, [props.roleId])

    //欄位驗證
    const validateField = Yup.object().shape({
        ROLE_ID: Yup.string().required("角色代碼為必填"),
        ROLE_NAME: Yup.string().required("角色名稱為必填")
    })

    return (
        <div>
            {props.visible &&
                <WindowBox title={props.isCreate ? '新增' : '修改'} onClose={props.onClose} width='70' height='80'>
                    <Formik
                        initialValues={mailRole}
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
                                        toolbar={<Button type="submit">存檔</Button>}
                                    >
                                        <table>
                                            <tbody>
                                                <tr>
                                                    <th><Label>角色代碼:</Label></th>
                                                    <td>
                                                        <TextInput
                                                            name="ROLE_ID"
                                                            value={values.ROLE_ID}
                                                            error={errors.ROLE_ID}
                                                            onChange={handleChange}
                                                            onBlur={handleBlur}
                                                            readOnly={!isAdd}
                                                            disabled={!isAdd}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th><Label>角色名稱:</Label></th>
                                                    <td>
                                                        <TextInput
                                                            name="ROLE_NAME"
                                                            value={values.ROLE_NAME}
                                                            error={errors.ROLE_NAME}
                                                            onChange={handleChange}
                                                            onBlur={handleBlur}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th><Label>使用者:</Label></th>
                                                    <td>
                                                        <OrgUserSelector
                                                            name="USERS"
                                                            value={values.USERS}
                                                            users={mailRoleUsers}
                                                            setValue={data => {
                                                                setValues({
                                                                    ...values,
                                                                    USERS: data
                                                                })
                                                            }}
                                                        />
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </PageContainer>
                                </form>
                            )
                        }}
                    </Formik>
                </WindowBox>
            }
        </div>
    )
}

export default AddMdf;
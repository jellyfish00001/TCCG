import React, { useState, useEffect, useContext } from 'react';
import { Formik } from 'formik';
import * as Yup from 'yup';
import { Button } from '@progress/kendo-react-buttons';
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import TextInput from '../../../../Components/Input/TextInput';
import OrgUserSelector from '../../../../Components/Selector/OrgUserSelector';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import FlowRoleService from '../flowRole.service';

const FlowRoleAddMdf = (props) => {
    const { showMessage } = useContext(MessageBoxContext);

    const [flowRoleData, setFlowRoleData] = useState({
        ROLE_ID: '',
        ROLE_NAME: '',
        DEL_FLG: false,
        USER_IDS: []
    });
    const [flowRoleUsers, setFlowRoleUsers] = useState([]);
    const service = FlowRoleService();

    useEffect(() => {
        if (isUpdate) {
            loadData();
        }
    }, [])

    const loadData = async () => {
        let users = await service.getFlowRoleUserByRoleId(props.roleId);
        let flowRole = await service.getFlowRoleByRoleId(props.roleId);
        setFlowRoleUsers(users.map(user => {
            return { USER_ID: user.USER_ID, USER_NAME: user.USER_NAME }
        }));
        setFlowRoleData({
            ROLE_ID: flowRole.ROLE_ID,
            ROLE_NAME: flowRole.ROLE_NAME,
            DEL_FLG: flowRole.DEL_FLG,
            USER_IDS: users.map(user => user.USER_ID)
        });
    }
    //欄位驗證
    const validateField = Yup.object().shape({
        ROLE_ID: Yup.string().required('角色代碼必填'),
        ROLE_NAME: Yup.string().required('角色名稱必填'),
    });

    //存檔
    const submit = async (data) => {
        let response = isUpdate ?
            await service.updateFlowRole(data) : await service.createFlowRole(data);
        let responseData = await response.json();
        showMessage(responseData.message, {
            onOkAction: () => {
                if (response.ok) {
                    props.onFinish();
                }
            }
        });
    }

    const isUpdate = props.roleId ? true : false;

    return (
        <WindowBox title={isUpdate ? '修改' : '新增'} onClose={props.onClose}>
            <Formik
                initialValues={flowRoleData}
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
                            <div className="fn-buttons">
                                <Button type="submit">存檔</Button>
                                <Button type="reset">重設</Button>
                            </div>
                            <table><tbody>
                                <tr>
                                    <th>角色代碼</th>
                                    <td>
                                        <TextInput
                                            name="ROLE_ID"
                                            value={values.ROLE_ID}
                                            error={errors.ROLE_ID}
                                            onChange={handleChange}
                                            onBlur={handleBlur}
                                            readOnly={isUpdate}
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th>角色名稱</th>
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
                                    <th>使用者</th>
                                    <td>
                                        <OrgUserSelector
                                            name="USER_IDS"
                                            value={values.USER_IDS}
                                            users={flowRoleUsers}
                                            setValue={data => {
                                                setValues({
                                                    ...values,
                                                    USER_IDS: data
                                                });
                                            }}
                                        />
                                    </td>
                                </tr>
                            </tbody></table>
                        </form>
                    )
                }}
            </Formik>
        </WindowBox>
    );
}

export default FlowRoleAddMdf;
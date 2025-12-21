import React, { useState, useEffect, useContext, useRef } from 'react';
import DimRoleService from '../dimRole.service';
import { Button } from '@progress/kendo-react-buttons';
import DualListBox from 'react-dual-listbox';
import 'react-dual-listbox/lib/react-dual-listbox.css';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../../../../Components/Input/TextInput'

const AddMdf = (props) => {
    const IsAdd = IsNullOrEmpty(props.roleId) ? true : false
    const [roleData, setRoleData] = useState({
        ROLE_ID: "",
        ROLE_NAME: "",
        RIGHTS: []
    })
    const [rightData, setRightData] = useState([]) //權利清單

    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        getDimRight()
        //取得要修改的資料
        if (!IsAdd) {
            //取得角色清單ByRoleId
            getDimRoleByRoleId(props.roleId)
        }
    }, []);

    //取得權利清單
    const getDimRight = async () => {
        setRightData(await DimRoleService.getDimRight())
    }

    //取得角色清單ByROLE_ID
    const getDimRoleByRoleId = async (roleId) => {
        let data = await DimRoleService.getDimRoleByRoleId(roleId)

        setRoleData(data)
    }

    //存檔
    const submit = async (data) => {
        let response;
        //新增or修改 權限
        if (IsAdd) {
            response = await DimRoleService.insertDimRole(data)
        } else {
            response = await DimRoleService.updateDimRole(data)
        }

        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    props.closeWindow();
                    props.refreshGrid();
                }
            }
        })
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        ROLE_ID: Yup.string()
            .required('角色代碼必填'),
        ROLE_NAME: Yup.string()
            .required('角色名稱必填')
    });

    return (
        <WindowBox title={props.title} onClose={props.closeWindow}>
            <Formik
                initialValues={roleData}
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
                            <table>
                                <tbody>
                                    <tr>
                                        <th >角色代碼</th>
                                        <td>
                                            <TextInput
                                                name="ROLE_ID"
                                                value={values.ROLE_ID}
                                                readOnly={!IsAdd}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.ROLE_ID}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>角色名稱</th>
                                        <td>
                                            <TextInput
                                               name="ROLE_NAME"
                                               value={values.ROLE_NAME}
                                               onChange={handleChange}
                                               onBlur={handleBlur}
                                               error={errors.ROLE_NAME}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>權限列表</th>
                                        <td >
                                            <DualListBox
                                                options={rightData}
                                                selected={values.RIGHTS}
                                                onChange={(e) => setValues({ ...values, RIGHTS: e })}
                                            />
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </WindowBox>
    );
}

export default AddMdf;


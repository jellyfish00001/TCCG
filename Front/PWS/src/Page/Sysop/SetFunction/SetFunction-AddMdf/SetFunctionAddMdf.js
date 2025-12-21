import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Input } from '@progress/kendo-react-inputs';
import '@progress/kendo-theme-default/dist/all.css';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import SetFunctionService from '../setFunction.service'
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../../../../Components/Input/TextInput'
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { useTranslation } from 'react-i18next';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';

const AddMdf = (props) => {
    const IsAdd = IsNullOrEmpty(props.functionId) ? true : false
    const [functionResult, setFunctionResult] = useState({
        PARENT_ID: props.functionDefaultDDL,
        FUNCTION_ID: props.functionId,
        FUNCTION_NAME: "",
        FUNCTION_URL: "",
        FUNCTION_CONTROLLER:""
    });
    const functionLevel = props.functionLevel;
    const [functionDDLList, setFunctionDDLList] = useState([]);
    const { showMessage } = useContext(MessageBoxContext);
    const { t } = useTranslation();
    const getFunctionDDLList = async () => {
        setFunctionDDLList(await SetFunctionService.getFunctionDDLList());
    }

    // 取得功能
    const getSetFunctionByFunctionId = async () => {
        /**
         * @type {object}
         */
        let functionData = await SetFunctionService.getSetFunctionByFunctionId(props.functionId);
        setFunctionResult(functionData);
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        FUNCTION_ID: Yup.string()
            .required(t('sysop.setFunction.setFunction08')),
        FUNCTION_NAME: Yup.string()
            .required(t('sysop.setFunction.setFunction09'))
    });

    // 存檔
    const save = async (data) => {
        //新增or修改 功能
        let response;
        if (IsAdd) {
            response = await SetFunctionService.createFunction(data, functionLevel);
        } else {
            response = await SetFunctionService.updateFunction(data, functionLevel);
        }

        showMessage(response.result, {
            onOkAction: () => {
                if (response.ok) {
                    props.closeWindow();
                    props.refreshGrid();
                }
            }
        })
    }

    useEffect(() => {
        //載入下拉選單資料和編修資料
        getFunctionDDLList();
        if (!IsAdd) {
            //取得功能資料
            getSetFunctionByFunctionId();
        }

    }, []);


    return (
        <WindowBox
            width={60}
            height={80}
            title={props.title}
            onClose={props.closeWindow}
        >
            <Formik
                initialValues={functionResult}
                validationSchema={validateField}
                onSubmit={(data) => save(data)}
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
                                <Button type="submit">{t('common.button.save')}</Button>
                                <Button type="reset">{t('common.button.reset')}</Button>
                            </div>
                            <table >
                                <tbody>
                                    <tr style={functionLevel ? { display: 'none' } : {}}>
                                        <th>{t('sysop.setFunction.setFunction03')}</th>
                                        <td>
                                            <DropDownListWithValue
                                                name="PARENT_ID"
                                                data={functionDDLList}
                                                textField="text"
                                                dataItemKey="value"
                                                value={values.PARENT_ID}
                                                disabled={!IsAdd}
                                                onChange={(e) => setValues({ ...values, PARENT_ID: e.target.value })} />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th >{t('sysop.setFunction.setFunction04')}</th>
                                        <td>
                                            <TextInput
                                                name="FUNCTION_ID"
                                                value={values.FUNCTION_ID}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.FUNCTION_ID} />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th scope="col">{t('sysop.setFunction.setFunction05')}</th>
                                        <td>
                                            <TextInput
                                                name="FUNCTION_NAME"
                                                value={values.FUNCTION_NAME}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.FUNCTION_NAME} />
                                        </td>
                                    </tr>
                                    <tr style={functionLevel ? { display: 'none' } : {}}>
                                        <th>{t('sysop.setFunction.setFunction07')}</th>
                                        <td>
                                            <Input
                                                type="text"
                                                name="FUNCTION_URL"
                                                value={values.FUNCTION_URL}
                                                onChange={handleChange} />
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

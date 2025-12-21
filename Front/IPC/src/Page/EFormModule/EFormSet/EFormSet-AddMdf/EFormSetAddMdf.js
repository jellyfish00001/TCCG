import React, { useState, useEffect, useContext } from 'react';
import FormSetService from '../eFormSet.service'
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty, GetOrgValue, FormatDate } from '../../../../Basic/SDOExtension';
import TwDatePicker from '../../../../Components/DateInputs/TwDatePicker'
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../../../../Components/Input/TextInput'
import OrgSelect from '../../../../Components/Selector/Orgselector'
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';

const AddMdf = (props) => {
    const IsAdd = IsNullOrEmpty(props.formId) ? true : false
    const [formSetData, setFormSetData] = useState({
        FORM_TYPE: "",
        FORM_ID: "",
        FORM_NAME: "",
        EFFECTIVE_DATE: new Date(),
        EXPIRE_DATE: new Date(),
        MAP_FLOW: "",
        MAP_ORG: [],
        MEMO: ""
    })
    const [formTypeData, setFormTypeData] = useState([])//表單類型下拉選單data
    const [mapFlowData, setMapFlowData] = useState([])//表單使用流程下拉選單data

    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        //取得表單類型下拉選單的data
        getFormType()
        //取得表單使用流程下拉選單的data
        getMapFlow()
        if (!IsAdd) {
            //取得表單設定byFormId
            getFormSetByFormId(props.formId)
        }
    }, []);

    //取得表單類型下拉選單的data
    const getFormType = async () => {
        setFormTypeData(await FormSetService.getFormType("請選擇..."))
    }

    //取得表單使用流程下拉選單的data
    const getMapFlow = async () => {
        setMapFlowData(await FormSetService.getMapFlow())
    }

    //取得表單設定byFormId
    const getFormSetByFormId = async (fromId) => {
        let data = await FormSetService.getFormSetByFormId(fromId)
        setFormSetData(data)
    }

    //存檔
    const submit = async (data) => {
        //資料處理
        let formSetData = { ...data }
        //取得Org object value
        formSetData.MAP_ORG = GetOrgValue(formSetData.MAP_ORG)
        formSetData.EFFECTIVE_DATE = FormatDate(formSetData.EFFECTIVE_DATE, 'YYYY-MM-DD')
        formSetData.EXPIRE_DATE = FormatDate(formSetData.EXPIRE_DATE, 'YYYY-MM-DD')
        //新增or修改 表單設定
        let response;
        if (IsAdd) {
            response = await FormSetService.insertFormSet(formSetData)
        } else {
            response = await FormSetService.updateFormSet(formSetData)
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
        FORM_TYPE: Yup.string()
            .required('表單類型必填'),
        FORM_ID: Yup.string()
            .required('表單代碼必填'),
        FORM_NAME: Yup.string()
            .required('表單名稱必填'),
        EFFECTIVE_DATE: Yup.date()
            .required('起始日期必填').nullable(),
        EXPIRE_DATE: Yup.date()
            .required('到期日期必填').nullable()
            .min(Yup.ref('EFFECTIVE_DATE'), "到期日期不可早於起始日期"),
        MAP_FLOW: Yup.string()
            .required('表單使用流程'),
        MAP_ORG: Yup.array()
            .required('表單應用單位')
    });

    return (
        <WindowBox title={props.title} onClose={props.closeWindow}>
            <Formik
                initialValues={formSetData}
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
                                        <th>表單類型</th>
                                        <td>
                                            <DropDownListWithValue
                                                name="FORM_TYPE"
                                                data={formTypeData}
                                                textField="SET_VALUE" //表單類型名稱
                                                dataItemKey="SET_TYPE" //表單類型代碼
                                                value={values.FORM_TYPE}
                                                onChange={(e) => setValues({ ...values, FORM_TYPE: e.target.value })}
                                                onBlur={handleBlur}
                                                error={errors.FORM_TYPE}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>表單代碼</th>
                                        <td>
                                            <TextInput
                                                name="FORM_ID"
                                                value={values.FORM_ID}
                                                readOnly={!IsAdd}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.FORM_ID}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>表單名稱</th>
                                        <td >
                                            <TextInput
                                                name="FORM_NAME"
                                                value={values.FORM_NAME}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.FORM_NAME}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>有效日期</th>
                                        <td>
                                            <tr>
                                                <td>
                                                    <TwDatePicker
                                                        name="EFFECTIVE_DATE"
                                                        format={"yyy/MM/dd"}
                                                        onChange={handleChange}
                                                        value={values.EFFECTIVE_DATE}
                                                        error={errors.EFFECTIVE_DATE}
                                                    />
                                                </td>
                                                <td>~</td>
                                                <td>
                                                    <TwDatePicker
                                                        name="EXPIRE_DATE"
                                                        format={"yyy/MM/dd"}
                                                        onChange={handleChange}
                                                        value={values.EXPIRE_DATE}
                                                        error={errors.EXPIRE_DATE}
                                                    />
                                                </td>
                                            </tr>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>表單使用流程</th>
                                        <td >
                                            <DropDownListWithValue
                                                name="MAP_FLOW"
                                                data={mapFlowData}
                                                textField="FLOW_NAME" //流程名稱  
                                                dataItemKey="FLOW_ID" //流程代碼
                                                value={values.MAP_FLOW}
                                                onChange={(e) => setValues({ ...values, MAP_FLOW: e.target.value })}
                                                onBlur={handleBlur}
                                                error={errors.MAP_FLOW}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>表單應用單位</th>
                                        <td>
                                            <OrgSelect
                                                name="MAP_ORG"
                                                multiple={true}  //多選
                                                value={values.MAP_ORG}
                                                setValue={(data) => setValues({ ...values, MAP_ORG: data })} // data 為object ，再"送出"的時候再取得value
                                                onBlur={handleBlur}
                                                error={errors.MAP_ORG}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>備註</th>
                                        <td >
                                            <textarea
                                                className="k-textarea"
                                                name="MEMO"
                                                value={values.MEMO}
                                                onChange={handleChange}
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
import React, { useState, useEffect, useContext } from 'react';
import FormService from '../eForm.service';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../../Components/Dialogs/ConfirmBox';
import { Formik } from 'formik';
import * as Yup from 'yup';
import { Error } from '@progress/kendo-react-labels';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';

export const AddMdf = (props) => {
    const IsAdd = IsNullOrEmpty(props.fillId) ? true : false
    const [formData, setFormData] = useState({
        //FORM_ID來源:修改=> 來自Grid所選取的表單填寫資料，新增=>來自下拉選單 
        FORM_ID: props.formId ? props.formId : props.formIdForDdl,
        FILL_ID: props.fillId,
        FILL_DATA: ""//填寫表單資料
    })

    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        if (!IsAdd) {
            //取得表單填寫的資料
            getFormFieldData(props.fillId)
        }
    }, []);

    //取得表單填寫的資料
    const getFormFieldData = async (fillId) => {
        let data = await FormService.getFormFieldData(fillId)

        setFormData({ ...formData, FILL_DATA: data })
    }

    //存檔
    const submit = async (data) => {
        let response;
        //新增or修改 表單
        if (IsAdd) {
            response = await FormService.insertForm(data)
        } else {
            response = await FormService.updateForm(data)
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
        FILL_DATA: Yup.string()
            .required('必填')
    });

    return (
        <WindowBox title={props.title} onClose={props.closeWindow} >
            <Formik
                initialValues={formData}
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
                                        <th>填寫表單資料</th>
                                        <td >
                                            <textarea
                                                className="k-textarea"
                                                name="FILL_DATA"
                                                value={values.FILL_DATA}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                            />
                                            {<Error>{errors.FILL_DATA}</Error>}
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


//送出流程
export const SendFlow = (props) => {
    //ddlData:表單使用流程下拉選單data，ddlValue:下拉選單目前選到的value(同時也是查詢條件)，預設下拉選單data的第一筆
    const [flowData, setFlowData] = useState({ ddlData: [], ddlValue: "" });

    const { showConfirmBox } = useContext(ConfirmBoxContext)
    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        //取得表單使用流程下拉選單的data
        getMapFlow()
    }, []);

    //取得表單使用流程下拉選單的data
    const getMapFlow = async () => {
        setFlowData(await FormService.getMapFlow(props.fillId))
    }

    const isSendFlow = () => {
        showConfirmBox("送出流程作業，確定嗎？", () => sendFlow())
    }

    //啟動簽核流程
    const sendFlow = async (flowId) => {
        //啟動簽核流程所需要的欄位
        let data = {
            FLOW_ID: flowId,
            FILL_ID: props.fillId
        }
        let response = await FormService.sendFlow(data)
        showMessage(response.result.ERR_MSG, {
            onOkAction: () => {
                if (response.ok) {
                    props.closeWindow()
                    props.refreshGrid()
                }
            }
        })
    }

    return (
        <WindowBox title={"表單填寫-送出流程"} onClose={props.closeWindow} >
            <Formik
                initialValues={flowData}
                onSubmit={(data) => sendFlow(data.ddlValue)}
                //允許重複賦予初始值
                enableReinitialize
            >
                {props => {
                    const {
                        values,
                        handleSubmit,
                        setValues
                    } = props;
                    return (
                        <form onSubmit={handleSubmit}>
                            <div className="fn-buttons">
                                <Button type="submit">確定</Button>
                            </div>
                            <table>
                                <tbody>
                                    <tr>
                                        <th>表單使用流程</th>
                                        <td >
                                            <DropDownListWithValue
                                                data={values.ddlData}
                                                textField="FLOW_NAME" //流程名稱  
                                                dataItemKey="FLOW_ID" //流程代碼
                                                value={values.ddlValue}
                                                onChange={(e) => setValues({ ...values, ddlValue: e.target.value })}
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


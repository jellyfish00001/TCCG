import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import '@progress/kendo-theme-default/dist/all.css';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import TwDatePicker from '../../../../Components/DateInputs/TwDatePicker'
import FlowSetService from '../flowSet.service'
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../../../../Components/Input/TextInput'
import WindowBox from '../../../../Components/Dialogs/WindowBox';

const AddMdf = (props) => {
    const IsAdd = IsNullOrEmpty(props.flowId) ? true : false
    // 流程主檔 SET_STAGE：流程關卡明細
    const [flowResult, setFlowResult] = useState({
        FLOW_ID: props.flowId,
        FLOW_NAME: "",
        MEMO: "",
        EFFECTIVE_DATE: "",
        EXPIRE_DATE: "",
    });

    const { showMessage } = useContext(MessageBoxContext);

    // 取得功能
    const getFlowByFlowId = async () => {
        let functionData = await FlowSetService.getFlowByFlowId(props.flowId);
        setFlowResult({
            ...functionData,
            EFFECTIVE_DATE: new Date(functionData.EFFECTIVE_DATE),
            EXPIRE_DATE: new Date(functionData.EXPIRE_DATE),
        });
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        FLOW_ID: Yup.string()
            .required('流程代碼必填'),
        FLOW_NAME: Yup.string()
            .required('流程名稱必填'),
        EFFECTIVE_DATE: Yup.date()
            .required('起始日期必填').nullable(),
        EXPIRE_DATE: Yup.date()
            .required('到期日期必填').nullable()
            .min(Yup.ref('EFFECTIVE_DATE'), "到期日期不可早於起始日期")
    });

    // 存檔
    const save = async (data) => {
        //新增or修改 功能
        let response;
        if (IsAdd) {
            response = await FlowSetService.createFlow(data);
        } else {
            response = await FlowSetService.updateFlow(data);
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
        if (!IsAdd) {
            //取得流程
            getFlowByFlowId();
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
                initialValues={flowResult}
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
                                <Button type="submit">存檔</Button>
                                <Button type="reset">重設</Button>
                            </div>
                            <table >
                                <tbody>
                                    <tr>
                                        <th >流程代碼</th>
                                        <td>
                                            <TextInput
                                                name="FLOW_ID"
                                                value={values.FLOW_ID}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.FLOW_ID} />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th scope="col">流程名稱</th>
                                        <td>
                                            <TextInput
                                                name="FLOW_NAME"
                                                value={values.FLOW_NAME}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.FLOW_NAME} />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th scope="col">流程說明</th>
                                        <td colspan="3">
                                            <textarea
                                                className="k-textarea"
                                                name="MEMO"
                                                value={values.MEMO}
                                                onChange={(e) => setValues({ ...values, MEMO: e.target.value })} />
                                        </td>
                                    </tr>
                                    <tr >
                                        <th>
                                            查詢日期
                                        </th>
                                        <td >
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
                                                <td>
                                                    ~
                                                </td>
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
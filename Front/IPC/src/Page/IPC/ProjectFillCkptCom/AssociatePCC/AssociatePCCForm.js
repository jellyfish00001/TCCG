import React, { useState, useRef } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Formik } from "formik";
import TextInput from '../../../../Components/Input/TextInput';
import { Tooltip } from '@progress/kendo-react-tooltip';
import { showGlobalMessageBox } from '../../../../Route/RootMiddleware';


// 關聯工程會標案查詢Form
export const AssociatePCCForm = ({ query }) => {

    // Form 資料
    const [formData, setFormData] = useState({
        PCC_PROJECT_NO: "",
        PCC_PROJECT_NAME: "",
        PCC_EXEC_ORG_NAME: ""
    });
    const formRef = useRef("");

    // 查詢關聯工程會資料
    const onSubmit = (data) => {
        const validObj = checkIsValid(data);
        if (validObj.valid) {
            query(data)
        } else {
            showGlobalMessageBox(validObj.msg);
        }
    }

    // 送出查詢條件前檢查
    const checkIsValid = (data) => {
        // 檢查至少輸入一項查詢條件
        if (data.PCC_PROJECT_NO.replace(/ /g, '') == '' &&
            data.PCC_PROJECT_NAME.replace(/ /g, '') == '' &&
            data.PCC_EXEC_ORG_NAME.replace(/ /g, '') == '')
            return { valid: false, msg: '查詢條件至少輸入一項' };

        if (data.PCC_PROJECT_NO.replace(/ /g, '') != '' && data.PCC_PROJECT_NO.length < 3)
            return { valid: false, msg: '標案編號至少輸入3碼以上' }

        return { valid: true, msg: '' }
    }

    return (
        <Formik
            initialValues={formData}
            onSubmit={(data) => onSubmit(data)}
            enableReinitialize={true}
            innerRef={formRef}
        >
            {props => {
                const {
                    handleChange,
                    handleSubmit,
                    values,
                } = props;
                return (
                    <form onSubmit={handleSubmit}>

                        <Tooltip
                            content={() => <span style={{ fontSize: '15px' }}>{"至少輸入一項條件，若輸入標案編號則最少3碼以上"}</span>}
                            openDelay={10}
                            position={"right"}
                            anchorElement="target"
                        >
                            <Button className='k-button' type='submit' >查詢</Button>
                            <span title={'查詢'} style={{ marginLeft: '5px', verticalAlign: 'middle' }} className="fa-exclamation-circle_Yellow">
                            </span>
                        </Tooltip>

                        <table>
                            <tbody>
                                <tr>
                                    <th>標案編號</th>
                                    <td>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.PCC_PROJECT_NO}
                                            name="PCC_PROJECT_NO"
                                            style={{ width: '90%' }}
                                        />
                                    </td>
                                    <th>標案名稱</th>
                                    <td>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.PCC_PROJECT_NAME}
                                            name="PCC_PROJECT_NAME"
                                            style={{ width: '90%' }}
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th>執行機關</th>
                                    <td colSpan={3}>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.PCC_EXEC_ORG_NAME}
                                            name="PCC_EXEC_ORG_NAME"
                                            style={{ width: '50%' }}
                                        />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </form>
                )
            }}
        </Formik>
    );
}
export default AssociatePCCForm
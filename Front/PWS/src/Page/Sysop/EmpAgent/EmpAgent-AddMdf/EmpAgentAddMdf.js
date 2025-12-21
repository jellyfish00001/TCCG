import React, { useEffect, useState, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { GetBasicData } from '../../../../Basic/BasicData';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import TwDatePicker from '../../../../Components/DateInputs/TwDatePicker';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import EmpAgentService from '../empAgent.service';
import { Formik } from 'formik';
import * as Yup from 'yup';
import 'bootstrap/dist/css/bootstrap.min.css';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';

const AddMdf = (props) => {
    const { showMessage } = useContext(MessageBoxContext);

    const [agent, setAgent] = useState({
        AGENT_ID: '',
        AGENT_FROM: new Date(Date.now()),//今天
        AGENT_TO: new Date(Date.now() + 60 * 60 * 24 * 1000)//明天
    });

    const [orgUser, setOrgUser] = useState({
        orgUsers: [],
        orgUsersLoaded: false
    });

    const service = EmpAgentService();

    useEffect(() => {
        loadOrgUser();
    }, []);

    const loadOrgUser = async () => {
        setOrgUser(await service.getOrgUser());
    }

    const submit = async (body) => {
        let data = {
            AGENT_ID: body.AGENT_ID,
            USER_ID: await GetBasicData("userId"),
            AGENT_FROM: body.AGENT_FROM.toISOString().slice(0, 10),
            AGENT_TO: body.AGENT_TO.toISOString().slice(0, 10)
        };
        let response = await service.createEmpAgent(data);
        let result = await response.json();
        showMessage(result.message, {
            onOkAction: () => {
                if (response.ok) {
                    props.onClose();
                }
            }
        });
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        AGENT_ID: Yup.string()
            .required('設定代理者帳號為必填'),
        AGENT_FROM: Yup.date()
            .required('起始日期必填').nullable(),
        AGENT_TO: Yup.date()
            .required('到期日期必填').nullable()
            .min(Yup.ref('AGENT_FROM'), "到期日期不可早於起始日期")
    });

    return (
        <WindowBox title={props.isCreate ? '新增' : '修改'} onClose={props.onClose}>
            <Formik
                initialValues={agent}
                validationSchema={validateField}
                onSubmit={(data) => submit(data)}
                //允許重複賦予初始值
                enableReinitialize
            >
                {props => {
                    const {
                        values,
                        errors,
                        handleSubmit,
                        handleReset,
                        handleChange,
                        handleBlur,
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
                                        <th>設定代理者帳號</th>
                                        <td>
                                            <DropDownListWithValue
                                                name="AGENT_ID"
                                                data={orgUser.orgUsers}
                                                loading={!orgUser.orgUsersLoaded}
                                                textField="USER_NAME"
                                                dataItemKey="USER_ID"
                                                value={values.AGENT_ID}
                                                onChange={(e) => setValues({ ...values, AGENT_ID: e.target.value })}
                                                onBlur={props.handleBlur('AGENT_ID')}
                                                error={errors.AGENT_ID}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>代理開始日期</th>
                                        <td>
                                            <TwDatePicker
                                                name="AGENT_FROM"
                                                value={values.AGENT_FROM}
                                                error={errors.AGENT_FROM}
                                                max={IsNullOrEmpty(values.AGENT_TO) ? new Date(Date.now()) : values.AGENT_TO}
                                                onChange={handleChange}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>代理結束日期</th>
                                        <td>
                                            <TwDatePicker
                                                name="AGENT_TO"
                                                value={values.AGENT_TO}
                                                error={errors.AGENT_TO}
                                                min={IsNullOrEmpty(values.AGENT_FROM) ? new Date(Date.now()) : values.AGENT_FROM}
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
import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import '@progress/kendo-theme-default/dist/all.css';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import FlowSetService from '../flowSet.service'
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { Formik } from 'formik';
import * as Yup from 'yup';
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { Checkbox, RadioButton } from '@progress/kendo-react-inputs';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';

const AddMdf = (props) => {
    // 流程關卡明細
    const IsAdd = IsNullOrEmpty(props.stageData) ? true : false
    //const flowId =
    const [stage, setStage] = useState({
        SET_ODR: props.stageData.SET_ODR,
        STAGE_TYPE: "role",
        SET_ORG_ID: "",
        SET_ROLE_ID: "",
        SET_USER_ID: "",
        SET_FLOW_ID: "",
        MAIL: false,
        SET_DECISION: false,
        SIGNATURE: false,
        CERTIFICATE: false,
        SEALED: false
    });
    // 各下拉選單
    const [DDLDatas, setDDLDatas] = useState({
        orgList: [],
        roleList: [],
        userList: [],
        flowList: []
    });
    const { showMessage } = useContext(MessageBoxContext);
    // 取得api資料
    const getDDLDataList = async () => {
        let orgList = await FlowSetService.getDDLDataList('EmpOrg/GetOrgs', { ORG_NAME: '請選擇', ORG_ID: "" });
        let roleList = await FlowSetService.getDDLDataList('FlowRole', { ROLE_NAME: '請選擇', ROLE_ID: "" });
        let userList = await FlowSetService.getDDLDataList('EmpUser', { USER_NAME: '請選擇', USER_ID: "" });
        let flowList = await FlowSetService.getDDLDataList('FlowSet', { FLOW_NAME: '請選擇', FLOW_ID: "" });

        setDDLDatas({
            orgList: orgList,
            roleList: roleList,
            userList: userList,
            flowList: flowList
        });
    }

    // 單選事件處理
    const radioButtonChange = (e) => {
        const value = e.value;
        let setOrgId = stage.SET_ORG_ID;
        let setRoleId = stage.SET_ROLE_ID;
        let setUserId = stage.SET_USER_ID;
        let setFlowId = stage.SET_FLOW_ID;
        // 選則其中一個單選後 其他下拉選單清空
        if (value === "role") {
            setUserId = "";
            setFlowId = "";
        }
        else if (value === "user") {
            setOrgId = "";
            setRoleId = "";
            setFlowId = "";
        } else if (value === "flow") {
            setOrgId = "";
            setRoleId = "";
            setUserId = "";
        }


        setStage(prevState => ({
            ...prevState,
            STAGE_TYPE: value,
            SET_ORG_ID: setOrgId,
            SET_ROLE_ID: setRoleId,
            SET_USER_ID: setUserId,
            SET_FLOW_ID: setFlowId
        }));
    }

    // 流程明細關卡存進grid中
    const save = async (stageData) => {
        // 取得全部關卡資料
        let detailStageDatas = await FlowSetService.getFlowDetailByFlowId(props.flowId);

        if (IsAdd) {
            let nextOdr = IsNullOrEmpty(detailStageDatas) ? 1 : detailStageDatas[detailStageDatas.length - 1].SET_ODR + 1;
            detailStageDatas = [
                ...detailStageDatas,
                {
                    ...stageData,
                    SET_ODR: nextOdr
                }
            ]
        }
        else {
            let objIndex = detailStageDatas.findIndex((obj => obj.SET_ODR === props.stageData.SET_ODR));

            detailStageDatas[objIndex] = {
                ...stageData
            }
        }
        //新增or修改 功能
        let response = await FlowSetService.updateFlowDetail(props.flowId, detailStageDatas);

        showMessage(response.result, {
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
        STAGE_TYPE: Yup.string(),
        SET_ROLE_ID: Yup.string().when(['STAGE_TYPE'], (STAGE_TYPE, schema) => {
            return STAGE_TYPE === "role" ? schema.required('角色選單必填') : schema;
        }),
        SET_USER_ID: Yup.string().when(['STAGE_TYPE'], (STAGE_TYPE, schema) => {
            return STAGE_TYPE === "user" ? schema.required('角色選單必填') : schema;
        }),
        SET_FLOW_ID: Yup.string().when(['STAGE_TYPE'], (STAGE_TYPE, schema) => {
            return STAGE_TYPE === "flow" ? schema.required('流程選單必填') : schema;
        })

    });

    useEffect(() => {
        //載入時取得下拉選單
        getDDLDataList();

        if (!IsAdd) {
            // 判斷關卡名稱確定單選資料
            let stageType = "";
            if (!IsNullOrEmpty(props.stageData.SET_ORG_ID) || !IsNullOrEmpty(props.stageData.SET_ROLE_ID)) {
                stageType = "role";
            }
            else if (!IsNullOrEmpty(props.stageData.SET_USER_ID)) {
                stageType = "user";
            }
            else if (!IsNullOrEmpty(props.stageData.SET_FLOW_ID)) {
                stageType = "flow";
            }

            setStage({ ...props.stageData, STAGE_TYPE: stageType })
        }

    }, []);


    return (
        <WindowBox
            width={40}
            height={30}
            title={props.title}
            onClose={props.closeWindow}
        >
            <Formik
                initialValues={stage}
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
                        setValues
                    } = props;
                    return (
                        <form onSubmit={handleSubmit} onReset={handleReset}>
                            <div className="fn-buttons">
                                <Button type="submit">存檔</Button>
                                <Button type="reset">重設</Button>
                            </div>
                            <tbody>
                                <tr>
                                    <td>
                                        <RadioButton
                                            name="STAGE_TYPE"
                                            value="role"
                                            checked={values.STAGE_TYPE === "role"}
                                            onChange={(e) => radioButtonChange(e)}
                                            onBlur={handleBlur}
                                            error={errors.STAGE_TYPE}
                                            label="角色" />
                                    </td>
                                    <td>
                                        <DropDownListWithValue
                                            name="SET_ORG_ID"
                                            data={DDLDatas.orgList}
                                            textField="ORG_NAME"
                                            dataItemKey="ORG_ID"
                                            value={values.SET_ORG_ID}
                                            onChange={(e) => setValues({ ...values, SET_ORG_ID: e.target.value })}
                                            onBlur={handleBlur}
                                            error={errors.SET_ORG_ID}
                                            width="300px"
                                            disabled={!(values.STAGE_TYPE === "role")} />
                                        <DropDownListWithValue
                                            name="SET_ROLE_ID"
                                            data={DDLDatas.roleList}
                                            textField="ROLE_NAME"
                                            dataItemKey="ROLE_ID"
                                            value={values.SET_ROLE_ID}
                                            disabled={!(values.STAGE_TYPE === "role")}
                                            onChange={(e) => setValues({ ...values, SET_ROLE_ID: e.target.value })}
                                            onBlur={handleBlur}
                                            error={errors.SET_ROLE_ID}
                                            width="300px" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <RadioButton
                                            name="STAGE_TYPE"
                                            value="user"
                                            checked={values.STAGE_TYPE === "user"}
                                            onChange={(e) => radioButtonChange(e)}
                                            onBlur={handleBlur}
                                            error={errors.STAGE_TYPE}
                                            label="使用者" />
                                    </td>
                                    <td>
                                        <DropDownListWithValue
                                            name="SET_USER_ID"
                                            data={DDLDatas.userList}
                                            textField="USER_NAME"
                                            dataItemKey="USER_ID"
                                            value={values.SET_USER_ID}
                                            disabled={!(values.STAGE_TYPE === "user")}
                                            onChange={(e) => setValues({ ...values, SET_USER_ID: e.target.value })}
                                            onBlur={handleBlur}
                                            error={errors.SET_USER_ID} />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <RadioButton
                                            name="STAGE_TYPE"
                                            value="flow"
                                            checked={values.STAGE_TYPE === "flow"}
                                            onChange={(e) => radioButtonChange(e)}
                                            onBlur={handleBlur}
                                            error={errors.STAGE_TYPE}
                                            label="流程" />
                                    </td>
                                    <td>
                                        <DropDownListWithValue
                                            name="SET_FLOW_ID"
                                            data={DDLDatas.flowList}
                                            textField="FLOW_NAME"
                                            dataItemKey="FLOW_ID"
                                            value={values.SET_FLOW_ID}
                                            disabled={!(values.STAGE_TYPE === "flow")}
                                            onChange={(e) => setValues({ ...values, SET_FLOW_ID: e.target.value })}
                                            onBlur={handleBlur}
                                            error={errors.SET_FLOW_ID} />
                                    </td>

                                </tr>
                                <tr>
                                    <td>關卡設定</td>
                                    <td>
                                        <Checkbox
                                            name="MAIL"
                                            checked={values.MAIL}
                                            onChange={(e) => setValues({ ...values, MAIL: e.value })}
                                            label={'Mail通知'} />
                                    &nbsp;&nbsp;&nbsp;&nbsp;
                                     <Checkbox
                                            name="SET_DECISION"
                                            checked={values.SET_DECISION}
                                            onChange={(e) => setValues({ ...values, SET_DECISION: e.value })}
                                            label={'是否決行'} />
                                    &nbsp;&nbsp;&nbsp;&nbsp;
                                    <Checkbox
                                            name="SIGNATURE"
                                            checked={values.SIGNATURE}
                                            onChange={(e) => setValues({ ...values, SIGNATURE: e.value })}
                                            label={'是否簽章'} />
                                    &nbsp;&nbsp;&nbsp;&nbsp;
                                    <Checkbox
                                            name="CERTIFICATE"
                                            checked={values.CERTIFICATE}
                                            onChange={(e) => setValues({ ...values, CERTIFICATE: e.value })}
                                            label={'檢查憑證'} />
                                    &nbsp;&nbsp;&nbsp;&nbsp;
                                    <Checkbox
                                            name="SEALED"
                                            checked={values.SEALED}
                                            onChange={(e) => setValues({ ...values, SEALED: e.value })}
                                            label={'是否封存'} />
                                    </td>
                                </tr>
                            </tbody>
                        </form>
                    );
                }}
            </Formik>
        </WindowBox>
    );
}
export default AddMdf;
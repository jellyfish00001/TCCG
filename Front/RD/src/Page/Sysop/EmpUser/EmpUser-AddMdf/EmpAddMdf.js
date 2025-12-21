import React, { useState, useRef, useEffect } from 'react';
import EmpUserService from '../empUser.service'
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty, telRegex } from '../../../../Basic/SDOExtension';
import { Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../../../../Components/Input/TextInput';
import { DualListBox as DListBox } from '../../../../Components/ListBox/DualListBox';
import { GetHistory } from '../../../../Basic/BasicData';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';
import EmpDel from './EmpDel';
import { showGlobalMessageBox } from '../../../../Route/RootMiddleware';

const EmpAddMdf = ({
    location: { state },
    location: {
        state: { userId, orgId, orgName, func } = { userId: "", orgId: "", orgName: "", func: "" }
    }
}) => {

    // 是否是新增
    let IsAdd = IsNullOrEmpty(userId) ? true : false;
    // 是否是專案辦公室
    let isSysopPMO = !IsNullOrEmpty(orgId) ? true : false;
    // 回上一頁網址
    let backurl = IsNullOrEmpty(func) ? '/Home/Sysop/EmpUser/Query' : '/Home/Sysop/SysopPMO/Edit';

    // 停用window的控制
    const [isDel, setIsDel] = useState({ visible: false, userId: "", title: "" });
    // 組織
    const [org, setOrg] = useState([]);
    //角色列表 
    const [roleData, setRoleData] = useState([]);
    // 使用者資料
    const [userData, setUserData] = useState({
        USER_ID: "",
        USER_NAME: "",

        USER_EMAIL: "",
        USER_TEL: "",
        USER_TITLE: "",
        USER_PD: "",
        CONFIRM_USER_PD: "",
        ROLES: isSysopPMO ? ["PMOMgr"] : [],
        ORG_ID: isSysopPMO ? orgId : "",
        ORG_NAME: isSysopPMO ? orgName : "",
        IS_SYSOPPMO: isSysopPMO
    })
    // 專案辦公室角色文字
    const [pmoRole, setPmoRole] = useState("");

    useEffect(() => {
        //取得組織清單
        getOrgData()

        //取得要修改的資料
        if (!IsAdd) {
            //取得使用者清單ByUserId
            getEmpUserByUserId()
        }
    }, []);

    //初次載入或roleData改變時
    useEffect(() => {
        //取得角色列表
        if (roleData.length <= 0) {
            getDimRole()
        }
    }, [roleData]);


    //取得角色列表
    const getDimRole = async () => {
        let data = await EmpUserService.getDimRole();
        setRoleData(data);

        if (isSysopPMO) {
            let role = data.find(x => x.value === userData.ROLES[0]);
            if (role !== undefined) {
                setPmoRole(role.label);
            }
        }
    }

    //取得使用者清單ByUserId
    const getEmpUserByUserId = async () => {
        let data = await EmpUserService.getEmpUserByUserId(userId)
        setUserData(data)
    }

    // 組織下拉選單
    const getOrgData = async () => {
        if (!isSysopPMO) {
            let orgData = await EmpUserService.getOrgDDLData();
            setOrg([...orgData]);
            setUserData({ ...userData, ORG_ID: orgData[0].value });
        }
    }

    //存檔
    const submit = async (data) => {
        let response;
        //新增or修改 使用者
        if (IsAdd) {
            response = await EmpUserService.insertEmpUser(data)
        } else {
            response = await EmpUserService.updateEmpUser(data)
        }

        if (response.success) {
            showGlobalMessageBox(response.result.message)
        } else {
            showGlobalMessageBox(response.result.message)
        }
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        USER_ID: Yup.string()
            .required('帳號必填'),
        USER_NAME: Yup.string()
            .required('姓名必填'),
        USER_EMAIL: Yup.string()
            .email("電子郵件 欄位不是有效的電子郵件地址。"),
        USER_TEL: Yup.string()
            .required("此欄位為必填").max(50).matches(telRegex, { message: '請輸入正確的電話格式!' }),
        ORG_ID: Yup.string()
            .required('組織必選'),
        USER_TITLE: Yup.string()
            .required("職稱必填"),
        USER_PD: Yup.string()
            .required("密碼必填"),
        CONFIRM_USER_PD: Yup.string().equals([Yup.ref('USER_PD')], "'確認密碼' 和 '密碼' 不相符。").required("確認密碼必填")
    });

    return (
        <>
            <h3 className="k-dialog-titlebar">系統管理&gt;使用者管理&gt;{IsAdd ? "新增使用者" : "編輯使用者"}</h3>
            <Formik
                initialValues={userData}
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
                                <Button onClick={(e) => {
                                    let data = func ? { orgId: orgId } : {};
                                    GetHistory().push(backurl, data);
                                }}>回上一頁</Button>
                                <Button type="submit">存檔</Button>
                                <Button type="reset">清除</Button>
                                {
                                    !IsNullOrEmpty(userId) &&
                                    <Button type="button" onClick={() => setIsDel({ visible: true, userId: userId, title: "停用帳號" })}>停用帳號</Button>
                                }
                            </div>
                            <table width='100%'>
                                <tbody>
                                    <tr>
                                        <th>隸屬機關</th>
                                        <td colSpan={3}>
                                            {
                                                isSysopPMO
                                                    ?
                                                    <TextInput
                                                        value={orgName}
                                                        disabled={true}
                                                    />
                                                    :
                                                    <DropDownListWithValue
                                                        name="ORG_ID"
                                                        data={org}
                                                        value={values.ORG_ID}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        onChange={(e) => {
                                                            setValues({ ...values, ORG_ID: e.target.value });
                                                        }}
                                                        error={errors["ORG_ID"]}
                                                    />
                                            }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <span style={{ color: "red" }}>*</span>
                                            使用者帳號
                                            <br />
                                            <span style={{ color: "red" }}>(請區分大小寫)</span>
                                        </th>
                                        <td>
                                            <TextInput
                                                name="USER_ID"
                                                value={values.USER_ID}
                                                readOnly={!IsAdd}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.USER_ID}
                                            />
                                        </td>

                                        <th>
                                            <span style={{ color: "red" }}>*</span>
                                            姓名
                                        </th>
                                        <td>
                                            <TextInput
                                                name="USER_NAME"
                                                value={values.USER_NAME}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.USER_NAME}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <span style={{ color: "red" }}>*</span>
                                            密碼
                                            <br />
                                            <span style={{ color: "red" }}>(請區分大小寫)</span>
                                        </th>

                                        <td >
                                            <TextInput
                                                name="USER_PD"
                                                value={values.USER_PD}
                                                onChange={handleChange}
                                                error={errors.USER_PD}
                                                type="password"
                                            />
                                        </td>

                                        <th>確認密碼</th>
                                        <td >
                                            <TextInput
                                                name="CONFIRM_USER_PD"
                                                value={values.CONFIRM_USER_PD}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.CONFIRM_USER_PD}
                                                type="password"
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <span style={{ color: "red" }}>*</span>
                                            職稱
                                        </th>
                                        <td>
                                            <TextInput
                                                name="USER_TITLE"
                                                value={values.USER_TITLE}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.USER_TITLE}
                                            />
                                        </td>

                                        <th>電子郵件</th>
                                        <td>
                                            <TextInput
                                                name="USER_EMAIL"
                                                value={values.USER_EMAIL}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.USER_EMAIL}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <span style={{ color: "red" }}>*</span>
                                            電話
                                        </th>
                                        <td>
                                            <TextInput
                                                name="USER_TEL"
                                                value={values.USER_TEL}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.USER_TEL}
                                            />
                                        </td>

                                        <th></th>
                                        <td></td>
                                    </tr>
                                    {
                                        // 從專案辦公室過來(props.orgId有值) 不顯示角色列表
                                        !isSysopPMO
                                            ?
                                            <tr>
                                                <th>角色列表</th>
                                                <td colSpan={3}>
                                                    {
                                                        roleData.length > 0 &&
                                                        <DListBox
                                                            selectableData={roleData}
                                                            selectedDataValue={values.ROLES}
                                                            onChange={(e) => { setValues({ ...values, ROLES: e }); }}
                                                        />
                                                    }
                                                </td>
                                            </tr>
                                            :
                                            <tr>
                                                <th>角色列表</th>
                                                <td colSpan={3}>{pmoRole}</td>
                                            </tr>
                                    }
                                </tbody>
                            </table>
                        </form>
                    );
                }}
            </Formik>
            {isDel.visible && <EmpDel
                title={isDel.title}
                closeWindow={() => setIsDel({ visible: false, userId: "", title: "" })}
                userId={isDel.userId}
                visible={isDel.visible}
            />}
        </>
    );
}

export default EmpAddMdf;
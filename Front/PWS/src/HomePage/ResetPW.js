import React from 'react';
import * as Yup from 'yup';
import { Formik } from 'formik';
import { api } from '../Basic/ApiFetch';
import { showGlobalMessageBox, getGlobalServerConfig } from '../Route/RootMiddleware';
import TextInput from '../Components/Input/TextInput';
import { Button } from '@progress/kendo-react-buttons';
import { SetMaskOnOff } from '../Basic/SDOExtension';

const ResetPW = (props) => {

    const [passMinLen, setPassMinLen] = React.useState(0);
    const [policyData, setPolicyData] = React.useState([]);
    const [originalUserPdVisible, setOriginalUserPdVisible] = React.useState(true);
    const [userPDVisible, setUserPDVisible] = React.useState(true);
    const [confirmUserPDVisible, setConfirmUserPDVisible] = React.useState(true);

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = async () => {
        SetMaskOnOff(true);
        await getSCPolicy();
        SetMaskOnOff(false);
    }

    // 取得SC密碼規則
    const getSCPolicy = async () => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'Login/GetSCPolicy';
        let response = await api.Post(url);
        let result = await response.json();

        setPassMinLen(result.PASS_MIN_LEN);
        getPolicyData(result);
    }

    // 取得規則
    const getPolicyData = (policy) => {
        let data = [];
        if (policy.PASS_NO_SAME_ID_NAME === "Y") {
            data.push("不可與帳號或使用者名稱相同");
        }
        if (policy.PASS_MIX_CHAR_NUM === "Y") {
            data.push("4項至少需符合3項(大寫英文字母、小寫英文字母、數字、特殊符號)");
        }
        if (policy.PASS_NO_SPEC_CHAR === "Y") {
            data.push("不可含空白或特殊字元，只能用文字 A-Z 或數字 1-9 組成");
        }
        if (policy.PASS_AT_LEAST_SPECIAL_CHARS !== "0") {
            data.push("至少含" + policy.PASS_AT_LEAST_SPECIAL_CHARS + "個特殊字元");
        }
        if (policy.PASS_NO_SAME_2 === "Y") {
            data.push("相鄰二字元不可相同");
        }
        if (policy.PASS_NO_CONT_3 === "Y") {
            data.push("相鄰三字元不可為連續升冪或降冪");
        }
        if (policy.PASS_NO_SAME_PAST_TIMES !== 0) {
            data.push("不可與最近" + policy.PASS_NO_SAME_PAST_TIMES + "次內重複");
        }
        if (policy.PASS_CHANGE_IN_DAYS !== 0) {
            data.push("每" + policy.PASS_CHANGE_IN_DAYS + "天需變更密碼");
        }

        setPolicyData([...data]);
    }

    // 確認
    const submit = async (data) => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'Login/ResetPW';
        let response = await api.Post(url, JSON.stringify(data));
        let result = await response.json();
        if (result.success) {
            showGlobalMessageBox(result.message, () => {
                props.closeWindow();
            });
        }
        else {
            showGlobalMessageBox(result.message);
        }
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        USER_ID: Yup.string()
            .required('請輸入帳號'),
        ORIGINAL_USER_PD: Yup.string()
            .required('請輸入原密碼'),
        USER_PD: Yup.string()
            .required('請輸入新密碼'),
        CONFIRM_USER_NEW_PD: Yup.string()
            .when("USER_PD", {
                is: undefined,
                otherwise: Yup.string()
                    .required('請輸入確認新密碼')
                    .equals([Yup.ref('USER_PD')], "'確認新密碼' 和 '新密碼' 不相符。")
            })
    });

    return (
        <Formik
            initialValues={{
                USER_ID: "",
                ORIGINAL_USER_PD: "",
                USER_PD: "",
                CONFIRM_USER_NEW_PD: ""
            }}
            validationSchema={validateField}
            onSubmit={(data) => submit(data)}
        >
            {prop => {
                const {
                    values,
                    errors,
                    handleSubmit,
                    handleChange,
                    setValues
                } = prop;
                return (
                    <form onSubmit={handleSubmit}>
                        <div style={{ textAlign: 'left' }}>
                            <span>本系統同仁使用帳號，通行碼登入系統時，應盡到善良使用者的責任，以帳號來源區分如下：</span>
                            <br />
                            <span>一、由公務雲登入(依循公務雲帳號管理原則)</span>
                            <br />
                            <span>二、由研考資訊系統登入</span>
                            <br />
                            <span style={{ marginLeft: '30px' }}>(一)遵守密碼長度原則{passMinLen}碼以上</span>
                            <br />
                            <span style={{ marginLeft: '30px' }}>(二)以下依據條件：</span>
                            <br />
                            {
                                policyData.map(x => {
                                    return <span style={{ marginLeft: '60px' }}>{x}<br /></span>
                                })
                            }
                        </div>

                        <table style={{ border: 'none' }}>
                            <tbody>
                                <tr>
                                    <th>帳號</th>
                                    <td>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.USER_ID}
                                            name="USER_ID"
                                            error={errors.USER_ID}
                                        />
                                    </td>
                                </tr>

                                <tr>
                                    <th>原密碼</th>
                                    <td>
                                        <i
                                            className={`fa ${originalUserPdVisible ? "fa-eye-slash" : "fa-eye"} pull-right`}
                                            onClick={() => setOriginalUserPdVisible(!originalUserPdVisible)}
                                            style={{ marginTop: "5px" }}
                                        ></i>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.ORIGINAL_USER_PD}
                                            name="ORIGINAL_USER_PD"
                                            error={errors.ORIGINAL_USER_PD}
                                            type={originalUserPdVisible ? "password" : "text"}
                                        />
                                    </td>
                                </tr>

                                <tr>
                                    <th>新密碼</th>
                                    <td>
                                        <i
                                            className={`fa ${userPDVisible ? "fa-eye-slash" : "fa-eye"} pull-right`}
                                            onClick={() => setUserPDVisible(!userPDVisible)}
                                            style={{ marginTop: "5px" }}
                                        ></i>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.USER_PD}
                                            name="USER_PD"
                                            error={errors.USER_PD}
                                            type={userPDVisible ? "password" : "text"}
                                        />
                                    </td>
                                </tr>

                                <tr>
                                    <th>確認新密碼</th>
                                    <td>
                                        <i
                                            className={`fa ${confirmUserPDVisible ? "fa-eye-slash" : "fa-eye"} pull-right`}
                                            onClick={() => setConfirmUserPDVisible(!confirmUserPDVisible)}
                                            style={{ marginTop: "5px" }}
                                        ></i>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.CONFIRM_USER_NEW_PD}
                                            name="CONFIRM_USER_NEW_PD"
                                            error={errors.CONFIRM_USER_NEW_PD}
                                            type={confirmUserPDVisible ? "password" : "text"}
                                        />
                                    </td>
                                </tr>

                                <tr>
                                    <th></th>
                                    <td>
                                        <div style={{ display: "flex" }}>
                                            <Button type="submit" style={{ width: '20%' }}>確認</Button>
                                            <Button type="button" style={{ width: '20%', marginLeft: '5px' }} onClick={() => {
                                                setValues({
                                                    ...values,
                                                    USER_ID: "",
                                                    ORIGINAL_USER_PD: "",
                                                    USER_PD: "",
                                                    CONFIRM_USER_NEW_PD: ""
                                                })
                                            }}>清除</Button>
                                            <Button type="button" style={{ width: '20%', marginLeft: '5px' }} onClick={() => props.closeWindow()}>取消</Button>
                                        </div>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </form>
                );
            }}
        </Formik>
    )
}

export default ResetPW;
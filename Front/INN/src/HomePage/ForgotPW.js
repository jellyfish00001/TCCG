import React from 'react';
import * as Yup from 'yup';
import { Formik } from 'formik';
import { api } from '../Basic/ApiFetch';
import { showGlobalMessageBox, getGlobalServerConfig } from '../Route/RootMiddleware';
import TextInput from '../Components/Input/TextInput';
import { Button } from '@progress/kendo-react-buttons';

const ForgotPW = (props) => {
    // 確認
    const submit = async (data) => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'Login/ForgotPW';
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
        USER_NAME: Yup.string()
            .required('請輸入姓名'),
        USER_EMAIL: Yup.string()
            .required('請輸入電子郵件位址')
    });

    return (
        <Formik
            initialValues={{
                USER_ID: "",
                USER_NAME: "",
                USER_EMAIL: ""
            }}
            validationSchema={validateField}
            onSubmit={(data) => submit(data)}
        >
            {prop => {
                const {
                    values,
                    errors,
                    handleBlur,
                    handleSubmit,
                    handleChange,
                    setValues
                } = prop;
                return (
                    <form onSubmit={handleSubmit}>
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
                                    <th>姓名</th>
                                    <td>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.USER_NAME}
                                            name="USER_NAME"
                                            error={errors.USER_NAME}
                                        />
                                    </td>
                                </tr>

                                <tr>
                                    <th>電子郵件位址</th>
                                    <td>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.USER_EMAIL}
                                            name="USER_EMAIL"
                                            error={errors.USER_EMAIL}
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
                                                    USER_NAME: "",
                                                    USER_EMAIL: ""
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

export default ForgotPW;
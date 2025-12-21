//@ts-check
import React, { useContext } from 'react';
import RVWResetService from './ResetPD.Service'
import { Button } from '@progress/kendo-react-buttons';
import { Tooltip } from '@progress/kendo-react-tooltip';
import { PageContainer } from '../../Basic/PageContainer';
import { MessageBoxContext } from '../../Components/Dialogs/MessageBox';
import TextInput from '../../Components/Input/TextInput'
import * as Yup from 'yup';
import { Formik } from 'formik';
import { none } from '@hookstate/core';
import { globalState } from '../../Route/RootMiddleware';
const Main = () => {
    
    //欄位驗證
    const validateField = Yup.object().shape({
        USER_PD: Yup.string()
            .required('請輸入原密碼'),
        USER_NEWPD: Yup.string()
            .required('請輸入新密碼'),
        CONFIRM_USER_PD: Yup.string()
            .required('請輸入確認密碼')
            .equals([Yup.ref('USER_NEWPD')], "'確認密碼' 和 '密碼' 不相符。")
    });

    const submit = async (data) => {
        let Data = {
            USER_PD: (data.USER_PD),
            USER_NEWPD:(data.USER_NEWPD),
            Sys:globalState.globalServerConfig.Me.get()
        }
        let response = await RVWResetService.ResetPW(Data);
        showMessage(response.result, {
            onOkAction: () => {
                if (response.ok) {
                }
            }
        })
    }
    const Features = [
        <h3 className="k-dialog-titlebar">管理作業&gt;變更密碼</h3>
    ]
  
   
    const { showMessage } = useContext(MessageBoxContext);
   
    
    return (
            <Tooltip openDelay={10} position="bottom" anchorElement="target">
                <PageContainer toolbar={Features}>
                <Formik
                    initialValues={{
                        USER_PD:"",
                        USER_NEWPD:"",
                        CONFIRM_USER_PD:"",
                    }}
                    validationSchema={validateField}
                    onSubmit={(data) => submit(data)}
                >
                    {props => {
                        const {
                            values,
                            errors,
                            handleBlur,
                            handleSubmit,
                            handleChange
                        } = props;
                        return (
                            <form onSubmit={handleSubmit}>
                                <table style={{ border:"none"}}>
                                    <tbody>
                                    <tr>
                                        <th colSpan={2} style={{textAlign:'center'}}>密碼變更</th>
                                    </tr>
									<tr>
                                            <th>原密碼</th>
                                            <td>
                                                <TextInput
                                                    onChange={handleChange}
                                                    value={values.USER_PD}
                                                    Maxlength={10}
                                                    name="USER_PD"
                                                    type="password"
                                                    onBlur={handleBlur}
                                                    error={errors.USER_PD}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>新密碼</th>
                                            <td>
                                                <TextInput
                                                    onChange={handleChange}
                                                    value={values.USER_NEWPD}
                                                    name="USER_NEWPD"
                                                    Maxlength={10}
                                                    type="password"
                                                    onBlur={handleBlur}
                                                    error={errors.USER_NEWPD}
                                                />
                                            </td>
                                        </tr>
                                           
										<tr>
											<th>確認密碼</th>
											<td>
												<TextInput
													onChange={handleChange}
													value={values.CONFIRM_USER_PD}
                                                    name="CONFIRM_USER_PD"
                                                    Maxlength={10}
                                                    type="password"
													onBlur={handleBlur}
													error={errors.CONFIRM_USER_PD}
												/>
											</td>
										</tr>
                                        <tr style={{textAlign:'right'}}>
                                            <td colSpan={2} className="fn-buttons">
                                            <Button type="submit">確認重置</Button>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </form>
                            
                        );
                    }}
                </Formik>
                
                </PageContainer>
            </Tooltip>
    );
}
export default Main;
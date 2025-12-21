import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Label } from '@progress/kendo-react-labels';
import { RadioButton } from '@progress/kendo-react-inputs';
import { filterBy } from '@progress/kendo-data-query';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import WindowBox from '../../../../Components/Dialogs/WindowBox'
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import certificateBindingService from '../certificateBinding.service';
import { Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../../../../Components/Input/TextInput';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';


export const AddMdf = props => {
    const [isAdd, setIsAdd] = useState(true);
    const [certificate, setCertificate] = useState({
        TOKEN_ID: null,
        TOKEN: null,
        TOKEN_TYPE: 'H',
        USER_ID: null,
        USER_TYPE: 'USR',
        EXPIRE_DATE: new Date(),
        DEL_FLG: false,
    })
    const [userList, setUserList] = useState({
        data: [], //下拉選單資料
        dataOri: [], //使用者資料 用於過濾並給下拉選單顯示
        loaded: false, //下拉選單loading狀態
    })

    const { showMessage } = useContext(MessageBoxContext);


    //讀取使用者下拉選單資料
    const loadUserList = async () => {
        setUserList(await certificateBindingService.loadUserList());
    }

    //讀取Token資料
    const loadTokenById = async (id) => {
        setCertificate(await certificateBindingService.loadCertificateById(id));
    }

    //送出
    const submit = async (data) => {
        let response;
        if (isAdd) {
            response = await certificateBindingService.InsertCertificate(data)
        }
        else {
            response = await certificateBindingService.UpdateCertificate(data)
        }

        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok)
                    props.onClose();
            }
        })
    }

    const userIdOnFilterChange = (e) => {
        let filter = {
            logic: 'or',
            filters: [
                { field: 'USER_ID', operator: 'contains', value: e.filter.value, ignoreCase: true },
                { field: 'USER_NAME', operator: 'contains', value: e.filter.value, ignoreCase: true },
            ]
        }

        setUserList({
            ...userList,
            data: filterBy(userList.dataOri, filter)
        })
    }

    useEffect(() => {
        //顯示
        //if (props.visible) {
        //讀取TOKEN資料
        if (!IsNullOrEmpty(props.tokenId))
            loadTokenById(props.tokenId);
        //設定IsAdd(是否為新增)
        setIsAdd(IsNullOrEmpty(props.tokenId));
        //}
        //關閉
        //else {
        //各狀態初始化
        //     setCertificate({
        //         TOKEN_ID: null,
        //         TOKEN: null,
        //         TOKEN_TYPE: 'H',
        //         USER_ID: null,
        //         USER_TYPE: 'USR',
        //         EXPIRE_DATE: new Date(),
        //         DEL_FLG: false,
        //     });
        // }
    }, [props.tokenId])

    useEffect(() => {
        loadUserList();
    }, [])

    const validateField = Yup.object().shape({
        TOKEN: Yup.string().required("憑證代碼為必填").nullable(),
        USER_ID: Yup.string().required("使用者為必填").nullable()
    })

    return (
        <WindowBox title={isAdd ? '新增' : '修改'} onClose={props.onClose} width='70' height='80'>
            <Formik
                initialValues={certificate}
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
                        handleChange,
                        handleBlur,
                        setValues
                    } = props;
                    return (
                        <form onSubmit={handleSubmit}>
                            <Button type="submit">存檔</Button>
                            <table>
                                <tbody>
                                    <tr>
                                        <th><Label>憑證序號:</Label></th>
                                        <td>
                                            <TextInput
                                                id="certificateBindingAddMdfToken"
                                                name="TOKEN"
                                                value={values.TOKEN}
                                                error={errors.TOKEN}
                                                onBlur={handleBlur}
                                                onChange={handleChange}
                                                readOnly={!isAdd}
                                                disabled={!isAdd}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th><Label>使用者:</Label></th>
                                        <td>
                                            <DropDownListWithValue
                                                name="USER_ID"
                                                data={userList.data}
                                                textField="USER_NAME"
                                                dataItemKey="USER_ID"
                                                onChange={e => setValues({ ...values, USER_ID: e.target.value })}
                                                onBlur={handleBlur}
                                                filterable={true}
                                                onFilterChange={userIdOnFilterChange}
                                                loading={!userList.loaded}
                                                value={values.USER_ID}
                                                error={errors.USER_ID}
                                                readOnly={!isAdd}
                                                disabled={!isAdd}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th><Label>啟用狀態:</Label></th>
                                        <td>
                                            <RadioButton name="delFlg" value={false} checked={!certificate.DEL_FLG} label="啟用" onChange={e => setCertificate({ ...certificate, DEL_FLG: e.value })} />
                                            <RadioButton name="delFlg" value={true} checked={certificate.DEL_FLG} label="停用" onChange={e => setCertificate({ ...certificate, DEL_FLG: e.value })} />
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </form>
                    );
                }
                }
            </Formik>
        </WindowBox>
    )
}
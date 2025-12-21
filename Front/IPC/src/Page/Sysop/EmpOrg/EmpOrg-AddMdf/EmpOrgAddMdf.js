import React, { useContext, useState, useEffect } from 'react';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import TextInput from '../../../../Components/Input/TextInput'
import DualListBox from 'react-dual-listbox';
import { Button } from '@progress/kendo-react-buttons';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import EmpOrgService from '../empOrg.service';
import { Formik } from 'formik';
import * as Yup from 'yup';
import Orgselector from '../../../../Components/Selector/Orgselector';

const EmpOrgAddMdf = (props) => {
    /*** use Context ***/
    const { showMessage } = useContext(MessageBoxContext);
    /*** use State ***/
    const [empOrgData, setEmpOrgData] = useState({
        orgId: '',
        orgName: '',
        parentOrg: [],
        empOrgUsers: []

    })
    const [empUsers, setEmpUsers] = useState([]);
    const type = props.type;
    const service = EmpOrgService();

    /*** use Effect ***/
    useEffect(() => {
        loadData();
    }, []);

    useEffect(() => {
        //console.log(empOrgData);
    }, [empOrgData])

    /*** function ***/
    //載入組織資料
    const loadData = async () => {
        let empUsers = await getEmpUsers();
        if (type === 'PUT') {
            let parent = await getParentOrg();
            let empOrgUsers = await getEmpOrgUsers();
            if (!IsNullOrEmpty(empOrgUsers)) empUsers.push(...empOrgUsers);
            setEmpOrgData({
                orgId: props.value.ORG_ID,
                orgName: props.value.ORG_NAME,
                parentOrg: parent,
                empOrgUsers: castToDualListItem(empOrgUsers)
            });
        }
        setEmpUsers(empUsers);
    };

    //取得使用者清單
    const getEmpUsers = async () => {
        return await service.getEmpUsers();
    }

    //取得單位的使用者清單
    const getEmpOrgUsers = async () => {
        return await service.getEmpOrgUsers(props.value.ORG_ID);
    }

    //取得上層機關
    const getParentOrg = async () => {
        let parent = await service.getParentOrg(props.value.PARENT_ID);
        return IsNullOrEmpty(parent) ? [] : [parent];
    }

    //json object 轉換成 dual list資料格式
    const castToDualListItem = (items) => {
        if (items.length > 0)
            return items.map(item => { return item.USER_ID; });
        return [];
    }

    const submit = async (data) => {
        //getObjectValue有問題        
        let obj = {
            ORG_ID: data.orgId,
            ORG_NAME: data.orgName,
            PARENT_ID: data.parentOrg[0].ORG_ID,
            USERS: data.empOrgUsers
        }

        let response;
        if (type === "PUT") {
            response = await service.updateEmpOrg(obj);
        }
        else if (type === "POST") {
            response = await service.insertEmpOrg(obj);
        }
        else {
            return;
        }
        let responseData = await response.json();
        //更新提示視窗訊息, 顯示
        showMessage(responseData.message, {
            onOkAction: () => {
                if (response.ok)
                    props.onFinish();
            }
        });
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        orgId: Yup.string()
            .required('組織代碼必填'),
        orgName: Yup.string()
            .required('組織名稱必填')
    });

    return (
        <WindowBox title={type === 'POST' ? "組織-新增" : "組織-修改"} onClose={props.onClose} >
            <Formik
                initialValues={empOrgData}
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
                            {/* tool button */}
                            < div className="fn-buttons" >
                                <Button type="submit">存檔</Button>
                                <Button type="reset">重設</Button>
                            </div>
                            {/* 輸入介面 */}
                            <table>
                                <tbody>
                                    <tr>
                                        <th>組織代碼</th>
                                        <td>
                                            <TextInput
                                                name="orgId"
                                                readOnly={type === "PUT"}
                                                value={values.orgId}
                                                error={errors.orgId}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>組織名稱</th>
                                        <td>
                                            <TextInput
                                                name="orgName"
                                                value={values.orgName}
                                                error={errors.orgName}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>上層組織</th>
                                        <td>
                                            <Orgselector
                                                name="parentOrg"
                                                value={values.parentOrg}
                                                //多選
                                                multiple={false}
                                                setValue={(data) => {
                                                    setValues({ ...values, parentOrg: data })
                                                }}
                                            />
                                        </td>
                                    </tr>
                                    {/* <tr>
                                        <th>所屬使用者</th>
                                        <td>
                                            <DualListBox
                                                //候選清單
                                                options={
                                                    empUsers.map(item => {
                                                        return { label: item.USER_ID + '-' + item.USER_NAME, value: item.USER_ID };
                                                    })
                                                }
                                                //已選擇清單
                                                selected={values.empOrgUsers}
                                                onChange={(e) => setValues({ ...values, empOrgUsers: e })}
                                            />
                                        </td>
                                    </tr> */}
                                </tbody>
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </WindowBox >
    );
}

export default EmpOrgAddMdf;
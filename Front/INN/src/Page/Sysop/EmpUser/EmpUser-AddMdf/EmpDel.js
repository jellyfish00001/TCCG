import React, { useState, useEffect, useContext, useRef } from 'react';
import EmpUserService from '../empUser.service'
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { PageContainer } from '../../../../Basic/PageContainer';
import '../../../../Css/custom/Table.css';
import { FormatDate } from "../../../../Basic/SDOExtension";
import { GetHistory } from '../../../../Basic/BasicData';
import { TextArea } from "@progress/kendo-react-inputs/dist/npm/textarea/TextArea";
import { Error } from '@progress/kendo-react-labels';

const EmpUserDel = (props) => {
    let userId = props.userId;
    let visible = props.visible;
    let nowDateTime = FormatDate(new Date(), "tYY/MM/DD HH:mm:ss");
    const { showMessage } = useContext(MessageBoxContext);

    const _userId = useRef(userId);
    // 使用者資料
    const [userData, setUserData] = useState({
        ORG_NAME:"",
        USER_NAME:"",
        USER_ID:"",
        DEL_REASON:""
    })

    useEffect(()=>{
        getEmpUserByUserId(_userId.current);
    }, []);

    // userId查詢使用者資料
    const getEmpUserByUserId = async (userId) => {
        let data = await EmpUserService.getEmpUserByUserId(userId);
        setUserData(data);
    }

    // 使用者帳號停用
    const deleteEmpUser = async (userId, delReason) => {
        let response = await EmpUserService.deleteEmpUser(userId, delReason)
        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    GetHistory().push('/Home/Sysop/EmpUser/Query');
                }
            }
        })
    }

    return (
        <>
            {visible && <WindowBox
                width={50}
                height={50}
                title='停用帳號'
                onClose={props.closeWindow}
            >
                <PageContainer
                    toolbar={
                        <Button title="確定停用" disabled={IsNullOrEmpty(userData.DEL_REASON)} onClick={()=> deleteEmpUser(_userId.current, userData.DEL_REASON)}>確定停用</Button>
                    }
                >
                    {userData && 
                        <table className="gridTable" width='100%'>
                            <tbody>
                                <tr>
                                    <th style={{height:"40px", textAlign:'center'}}>單位</th>
                                    <td>{userData.ORG_NAME}</td>
                                    <th style={{height:"40px", textAlign:'center'}}>姓名</th>
                                    <td>{userData.USER_NAME}</td>
                                </tr>
                                <tr>
                                    <th style={{height:"40px", textAlign:'center'}}>使用者帳號</th>
                                    <td>{userData.USER_ID}</td>
                                    <th style={{height:"40px", textAlign:'center'}}>現在時間</th>
                                    <td>{nowDateTime}</td>
                                </tr>
                                <tr>
                                    <th style={{height:"40px", textAlign:'center'}}>停用原因</th>
                                    <td colSpan={3}>
                                        <TextArea
                                            maxLength={200}
                                            value={userData.DEL_REASON}
                                            style={{ width: "100%" }}
                                            name="DEL_REASON"
                                            rows={4}
                                            onChange={(e) => {
                                                setUserData({
                                                    ...userData,
                                                    DEL_REASON: e.target.element.current.value
                                                });
                                            }}
                                        />
                                        <Error>{IsNullOrEmpty(userData.DEL_REASON) ? "填寫停用原因" : ''}</Error>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    }
                </PageContainer>
            </WindowBox>}
        </>
    );
}

export default EmpUserDel;
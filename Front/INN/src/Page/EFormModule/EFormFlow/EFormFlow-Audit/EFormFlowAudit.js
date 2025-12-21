import React, { useState, useContext, useEffect, useReducer } from "react";
import { Dialog, DialogActionsBar } from "@progress/kendo-react-dialogs";
import { Button } from "@progress/kendo-react-buttons";
import { Error } from '@progress/kendo-react-labels';
import WindowBox from "../../../../Components/Dialogs/WindowBox";
import { IsNullOrEmpty } from "../../../../Basic/SDOExtension"
import { MessageBoxContext } from "../../../../Components/Dialogs/MessageBox";
import OrgUserSelector from '../../../../Components/Selector/OrgUserSelector';
import EformFlowService from "../eFormFlow.service";

const EFormFlowAudit = (props) => {
    /* use Context */
    const { showMessage } = useContext(MessageBoxContext);

    /* use State */
    const [memoInputVisible, setMemoInputVisible] = useState(false);
    const [memoRequiredVisible, setMemoRequiredVisible] = useState(false);
    const [subflowActiveVisible, setSubflowActiveVisible] = useState(false);
    const [title, setTitle] = useState('');
    const [memo, setMemo] = useState('');
    const [selectedEmpUsers, setSelectedEmpUsers] = useState([]);
    const [rtnResult, setRtnResult] = useState(null);
    const service = EformFlowService();
    const [ignore, forceUpdate] = useReducer(x => x + 1, 0);

    /* use Effect */
    useEffect(() => {
        if (rtnResult !== null) {
            if (rtnResult.success) {
                showMessage(title + '成功', {
                    onOkAction: () => {
                        setMemoInputVisible(false);
                        setSubflowActiveVisible(false);

                        props.onFinish();
                    }
                });
            }
            else {
                showMessage(rtnResult.message);
            }
        }
    }, [rtnResult])

    /* memo輸入 */
    const onMemoChange = event => {
        setMemo(event.target.value);
    }

    /* 驗證memo輸入 */
    const validate = () => {
        let valid = IsNullOrEmpty(memo);
        setMemoRequiredVisible(valid);
        return !valid;
    }

    const onAudit = async () => {
        if (validate()) {
            let data = {
                memo: memo,
                flow_code: props.isSubflow ? props.subflowCode : props.flowCode,
                is_subflow: props.isSubflow,
            };
            let result = {
                message: title + '失敗'
            };
            if (title === '簽核退回') {
                result = await service.setFlowReject(data);
                setRtnResult(result);
            }
            else {
                /*簽核同意 or 提供意見(分會)*/
                result = await service.setFlowAccept(data);
                setRtnResult(result);
            }
        }
    }

    const onSubflowActive = async () => {
        if (selectedEmpUsers.length === 0) return;
        let data = {
            flow_code: props.flowCode,
            flow_id: props.flowId,
            sub_users: selectedEmpUsers.join(",")
        };
        let result = await service.setSubFlowActive(data);
        setRtnResult(result);
    }

    /* 彈出memo視窗 */
    const onOpenMemo = (title) => {
        setTitle(title);
        setMemoInputVisible(true);
    }


    return (
        <WindowBox
            title="表單簽核-修改"
            onClose={props.onClose}
        >
            {props.isSubflow ?
                <div className="fnForm">
                    <div className="fn-buttons">
                        <Button onClick={() => {
                            onOpenMemo('提供意見');
                        }}>提供意見</Button>
                    </div>
                </div> :
                <div className="fnForm">
                    <div className="fn-buttons">
                        <Button onClick={() => {
                            onOpenMemo('簽核同意');
                        }}>同意</Button>
                        <Button onClick={() => {
                            onOpenMemo('簽核退回');
                        }}>退回</Button>
                        <label style={{ color: 'lightgray', fontSize: '20px' }}>|</label>
                        <Button onClick={() => {
                            setTitle('分會啟動');
                            setSubflowActiveVisible(true);
                        }}>分會</Button>
                    </div>
                </div>
            }
            {/* Memo input Dialog */}
            {memoInputVisible && <Dialog
                title={title}
                onClose={() => { setMemoInputVisible(false) }}
                width="35%"
                height="45%"
            >
                <textarea className="light-textarea" onChange={onMemoChange} onBlur={() => { validate() }}></textarea>
                {/* 驗證提示訊息 */}
                {memoRequiredVisible && <Error>請填寫簽核意見</Error>}
                <DialogActionsBar>
                    <Button onClick={() => { onAudit() }}>確認</Button>
                    <Button onClick={() => { setMemoInputVisible(false) }}>取消</Button>
                </DialogActionsBar>
            </Dialog>}
            {/* 分會選擇視窗 */}
            {subflowActiveVisible && <Dialog
                onClose={() => { setSubflowActiveVisible(false); }}
                width="35%"
                height="63%"
            >
                <OrgUserSelector
                    value={selectedEmpUsers}
                    setValue={data => {
                        setSelectedEmpUsers(data);
                        forceUpdate();
                    }}
                />
                <DialogActionsBar>
                    <Button onClick={() => { onSubflowActive() }}>確認</Button>
                    <Button onClick={() => { setSubflowActiveVisible(false); }}>取消</Button>
                </DialogActionsBar>
            </Dialog>}
        </WindowBox >
    )
}

export default EFormFlowAudit;
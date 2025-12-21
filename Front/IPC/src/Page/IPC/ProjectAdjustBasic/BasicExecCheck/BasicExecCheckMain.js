import React, { useState, useEffect } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import CollapseBoardCard from '../../../../Components/BoardCard/CollapseBoardCard';
import { getAdjustChk, SendExecAdjust } from './BasicExecCheckService';
import { SetMaskOnOff, closeAndbackToParentWindow } from '../../../../Basic/SDOExtension';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../../Route/RootMiddleware';

const BasicExecCheckMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            /**傳入的參數 */
            state: {
                awKind,
                projectNo,
                projAdjId
            } = {
                awKind: null,
                projectNo: null,
                projAdjId: null,
            }
        }
    } = props;

    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => props.history.push('/Home'));
    }

    // 檢核結果
    const [checkResult, setCheckResult] = useState({
        success: null,
        message: "",
        data: {}
    });

    useEffect(() => {
        loadData();
    }, []);

    /**
     * 載入送審檢核結果
     */
    const loadData = async () => {
        let result = await getAdjustChk(projectNo, projAdjId);
        setCheckResult(result);
    }

    /**
     * 送出申請
     */
    const sendApply = async () => {
        showGlobalConfirmBox("確認送出基本資料調整至主管審查?", async () => {
            SetMaskOnOff(true);
            let requestData = {
                PROJ_ADJ_ID: projAdjId,
                PROJECT_NO: projectNo,
                AW_KIND: awKind
            }
            let saveResult = await SendExecAdjust(requestData);
            SetMaskOnOff(false);

            if (saveResult.success) {
                showGlobalMessageBox(saveResult.message, () => {
                    closeAndbackToParentWindow(window);
                });
            }
        });
    }

    // 顯示檢核結果
    const displayResult = () => {
        return (
            <>
                <div>計畫名稱：{checkResult.data.PROJECT_NAME}</div>
                {!checkResult.data.errList ?
                    <>
                        <span style={{ color: 'blue' }}>【正確】</span>
                        <Button type="button" onClick={sendApply}>送出申請</Button>
                    </>
                    :
                    <>
                        <div style={{ color: 'red' }}>【錯誤】</div>
                        <Grid
                            style={{
                                height: '100%',
                                overflow: 'auto',
                            }}
                            resizable={true}
                            data={checkResult.data.errList}
                        >
                            <GridNoRecords>無資料</GridNoRecords>
                            <GridColumn title="錯誤章節" field="errTitle" cell={(p) =>
                                <td>
                                    <a onClick={() => {
                                        props.history.push(p.dataItem.SOURCE_PATH, props.location.state);
                                    }}>{p.dataItem.errTitle}</a>
                                </td>} />
                            <GridColumn title="錯誤內容" field="errDesc" className='error-msg' />
                        </Grid>
                    </>
                }
            </>
        )
    }
    return (
        <CollapseBoardCard title="3.基本資料調整送審" >
            {checkResult.success && checkResult.data ? displayResult() : checkResult.message}
        </CollapseBoardCard>
    )
}

export default BasicExecCheckMain;
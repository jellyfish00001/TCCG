import React, { useState, useEffect } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import CollapseBoardCard from '../../../../Components/BoardCard/CollapseBoardCard';
import { initFiles, getAdjustChk, sendExecAdjust, saveScheAttach, exportRPT } from './ScheduleExecCheckService';
import { downProjectAttachment } from '../../../../Basic/CommonService';
import { closeAndbackToParentWindow, IsNullOrEmpty, SetMaskOnOff } from '../../../../Basic/SDOExtension';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../../Route/RootMiddleware';
import { TempFileUploader } from '../../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../../Components/Upload/TempFileUploadService';
import { getGlobalServerConfig } from '../../../../Route/RootMiddleware';
import CommonTooltip from '../../../../Components/Tooltip/CommonTooltip';
import CacheLoader from '../../../../Basic/CacheLoader';

const ScheduleExecCheckMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            /**傳入的參數 */
            state: {
                awKind,
                projectNo,
                projAdjId,
                isShowBtn,
            } = {
                awKind: null,
                projectNo: null,
                projAdjId: null,
                isShowBtn: true,
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

    // ------------檔案上傳---------------
    const [approvalFiles, setApprovalFiles] = useState([]); // 准簽
    const [files, setFiles] = useState([]); // 已核章申請表
    // 檔案上傳異動相關資訊
    const editedApprovalFiles = React.useRef([]); // 准簽
    const editedFiles = React.useRef([]); // 已核章申請表
    // 紀錄資料資否有異動
    const [isDataChange, setIsDataChange] = React.useState(false);
    // 下載檔案
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId);
    }

    // 上傳所需參數 (准簽)
    let paramApproval = {
        files: approvalFiles,
        editedFiles: editedApprovalFiles,
        setFiles: setApprovalFiles,
        setIsDataChange,
        multiple: false,
        downFile
    }
    // 上傳所需參數 (已核章申請表)
    let paramFiles = {
        files: files,
        editedFiles: editedFiles,
        setFiles: setFiles,
        setIsDataChange,
        multiple: false,
        downFile
    }
    // 暫存檔上傳Service
    const tempFileUploadService1 = TempFileUploadService(paramApproval); // 准簽
    const tempFileUploadService2 = TempFileUploadService(paramFiles); // 已核章申請表

    useEffect(() => {
        loadData();
    }, []);

    /**
     * 載入送審檢核結果
     */
    const loadData = async () => {
        let result = await getAdjustChk(projectNo, projAdjId);
        let responseData = result.data;
        if (responseData.approvalFiles) {
            let fileData = fileList(responseData.approvalFiles, "IDENTITY_FIELD");
            setApprovalFiles(fileData);
        }
        if (responseData.files) {
            let fileData = fileList(responseData.files, "IDENTITY_FIELD");
            setFiles(fileData);
        }
        setCheckResult(result);
    }

    /**
     * 送出申請
     */
    const sendApply = async () => {
        // 驗證
        if (IsNullOrEmpty(approvalFiles)) {
            showGlobalMessageBox("請上傳府簽（含智發會及相關機關會簽意見）或機關簽");
            return;
        }
        if (IsNullOrEmpty(files)) {
            showGlobalMessageBox("請上傳已核章申請表");
            return;
        }

        // 送出
        showGlobalConfirmBox("確認送出期程調整至主管審查?", async () => {
            SetMaskOnOff(true);
            let requestData = {
                PROJ_ADJ_ID: projAdjId,
                PROJECT_NO: projectNo,
                AW_KIND: awKind,
                Files2: { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "10", EditFiles: editedApprovalFiles.current },
                Files: { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "11", EditFiles: editedFiles.current }
            }
            let saveResult = await sendExecAdjust(requestData);
            SetMaskOnOff(false);

            if (saveResult.success) {
                showGlobalMessageBox(saveResult.message, () => {
                    closeAndbackToParentWindow(window);
                });
            }
            else {
                showGlobalMessageBox(saveResult.message);
            }
        });
    }

    /**
     * 存檔
     */
    const save = async () => {
        // 驗證
        if (IsNullOrEmpty(approvalFiles)) {
            showGlobalMessageBox("請上傳府簽（含智發會及相關機關會簽意見）或機關簽");
            return;
        }
        if (IsNullOrEmpty(files)) {
            showGlobalMessageBox("請上傳已核章申請表");
            return;
        }

        // 存檔
        let requestData = {
            PROJ_ADJ_ID: projAdjId,
            Files2: { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "10", EditFiles: editedApprovalFiles.current },
            Files: { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "11", EditFiles: editedFiles.current }
        };
        let saveResult = await saveScheAttach(requestData);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                editedApprovalFiles.current = [];
                editedFiles.current = [];
                window.location.reload();
            });
        }
        else {
            showGlobalMessageBox(saveResult.message);
        }
    }

    /**
     * 取消
     */
    const reset = async () => {
        editedApprovalFiles.current = [];
        editedFiles.current = [];
        loadData();
    }

    // 顯示檢核結果
    const displayResult = () => {
        return (
            <>
                <div>計畫名稱：{checkResult.data.PROJECT_NAME}</div>
                <div>
                    {checkResult.data.SCHE_TYPE &&
                        <>
                            <span style={{ marginRight: '10px' }}>調整類別：{checkResult.data.SCHE_TYPE == 'Y' ? '總期程調整' : '分月調整'}</span>
                            {isShowBtn && <Button type="button" onClick={() => exportRPT(projectNo, projAdjId)} >匯出申請表</Button>}
                        </>
                    }
                </div>

                {/* 若無 errList 顯示正確，有則顯示錯誤 */}
                {!checkResult.data.errList ?
                    <>
                        <div>
                            (1) A級列管案件涉及總期程調整者，請匯出申請表，專簽府一層核定後（先會辦智發會），將府簽公文及已核章之申請表，分別上傳系統，再按「送出申請」。<br />
                            (2) 其餘類型之期程調整，請匯出申請表，簽請機關首長核章後，將機關簽公文及已核章之申請表，分別上傳系統，再按「送出申請」。
                        </div>
                        <span style={{ color: 'blue' }}>【正確】</span>
                        {isShowBtn && <span className='fn-buttons'>
                            <Button title='送出申請' onClick={sendApply} className="k-button-lighten">送出申請</Button>
                        </span>}
                        <form>
                            <table>
                                <tr>
                                    <th>
                                        <CommonTooltip title={"府簽（含智發會及相關機關會簽意見）或機關簽"} content={
                                            "佐證資料已於「期程調整事由」上傳，本處無須上傳。"}
                                        />
                                    </th>
                                    <td>
                                        <TempFileUploader
                                            {...tempFileUploadService1.uploaderParam}
                                            saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                            saveHeaders={{
                                                // @ts-ignore
                                                'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                // 'CacheToken': CacheLoader().GetCache(),
                                            }}
                                            files={approvalFiles}
                                            setFiles={setApprovalFiles}
                                            multiple={false}
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th><CommonTooltip title={"已核章申請表"} content={
                                        <>
                                            可上傳檔案格式如下<br />
                                            jpg, jpeg, bmp, png,<br />
                                            mpg, doc, docx, ppt,<br />
                                            pptx, pdf, xls, xlsx,<br />
                                            odt, ods, odp, odg
                                        </>}
                                    /></th>
                                    <td>
                                        <TempFileUploader
                                            {...tempFileUploadService2.uploaderParam}
                                            saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                            saveHeaders={{
                                                // @ts-ignore
                                                'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                // 'CacheToken': CacheLoader().GetCache(),
                                            }}
                                            files={files}
                                            setFiles={setFiles}
                                            multiple={false}
                                        />
                                    </td>
                                </tr>
                            </table>
                        </form>
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
                            <GridColumn title="錯誤內容" field="errDesc" className="error-msg" />
                        </Grid>
                    </>
                }
            </>
        )
    }

    return (
        <CollapseBoardCard button={
            // 若檢核結果為錯誤，不顯示按鈕
            checkResult.success && checkResult.data && !checkResult.data.errList && isShowBtn &&
            <>
                <Button title="存檔" onClick={save} >存檔</Button>
                <Button title="取消" className="k-button-lighten" onClick={reset}>取消</Button>
            </>
        } title="3.期程調整送審" isFirstArea={true}>
            {checkResult.success && checkResult.data ? displayResult() : checkResult.message}
        </CollapseBoardCard>
    )
}

export default ScheduleExecCheckMain;
import React, { useState, useEffect, useRef } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import Table from '../../../Css/custom/Table.module.css';
import { getProjectFillClose, saveProjectFillClose } from './ProjectFillCloseService'
import { closeAndbackToParentWindow, FormatDate, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { getGlobalServerConfig, showGlobalMessageBox, showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import ProjectFillCloseAuditGrid from './ProjectFillCloseGrid';
import CacheLoader from '../../../Basic/CacheLoader';
import RadioBoxList from '../../../Components/Input/RadioBoxList';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { fileList, TempFileUploadService } from '../../../Components/Upload/TempFileUploadService';
import { downProjectAttachment, openProjectPrint } from '../../../Basic/CommonService';
import TextAreaInput from '../../../Components/Input/TextAreaInput';

export const ProjectFillCloseTable = (props) => {
    let { projectNo, cycleData, isAudit, isRDEC, showSomeBtn, state } = props

    // 經費資料
    const [budgetData, setBudgetData] = useState([]);
    const budgetDataForSave = useRef([])
    // FormData
    const [formData, setFormData] = useState({});
    // 上傳檔案資料
    const [files, setFiles] = useState([]);
    // 檔案上傳異動相關資訊
    const editedFiles = React.useRef([]);
    // for 檔案上傳元件使用
    const [isDataChange, setIsDataChange] = useState(false);
    // 計算經費資料
    const [currentExpenseData, setCurrentExpenseData] = useState({
        currentExpense: 0,
        currBudgetExeRate: 0
    })
    // 可否存檔
    const [canSave, setCanSave] = useState(true);

    // 經費支用情形Grid異動
    const gridChange = (data) => {
        budgetDataForSave.current = [...data]
        if (data && data.length > 0) {
            const expense = data[0].ACTUAL_PAY + data[0].UNPAY + data[0].BALANCE;
            const totalBudget = data[0].TOTAL_BUDGET;
            setCurrentExpenseData({
                currentExpense: expense,
                currBudgetExeRate: totalBudget > 0 ? (expense / totalBudget) * 100 : 0
            })
        }
    }

    let param = {
        files: files,
        editedFiles,
        setFiles,
        setIsDataChange,
        multiple: true,
        downFile: downProjectAttachment
    };
    // 暫存檔上傳Service
    const tempFileUploadService = TempFileUploadService(param);

    // 取得資料
    const loadData = async () => {
        SetMaskOnOff(true);
        editedFiles.current = [];
        let result = await getProjectFillClose(projectNo);
        if (result) {
            // FormData
            setFormData({ ...result });
            // 經費支用情形
            let budgetData = [{
                PAYMENT_ID: result.PAYMENT_ID,
                DATA_DATE_SHOW: result.MDF_DATE == null ? cycleData : FormatDate(result.MDF_DATE, 'tYY_MM'),
                TOTAL_BUDGET: result.TOTAL_BUDGET,
                TOTAL_ACTUAL_COMP: result.TOTAL_ACTUAL_COMP,
                ACTUAL_PAY: result.ACTUAL_PAY,
                UNPAY: result.UNPAY,
                BALANCE: result.BALANCE,
                MDF_DATE: result.MDF_DATE == null ? '' : FormatDate(result.MDF_DATE, 'tYY/MM/DD')
            }];
            setBudgetData([...budgetData])
            budgetDataForSave.current = [...budgetData];

            // 計算經費資料
            const expense = result.ACTUAL_PAY + result.UNPAY + result.BALANCE;
            setCurrentExpenseData({
                currentExpense: expense,
                currBudgetExeRate: (expense / result.TOTAL_BUDGET) * 100
            })
            // 設定檔案資料
            if (result.ProjAttachments.length > 0) {
                setFiles(fileList(result.ProjAttachments, 'IDENTITY_FIELD', 'FILE_NAME'))
            }
            // 設定可否存檔
            setCanSave(result.CanSave);
        }
        SetMaskOnOff(false);
    }

    // 存檔
    const save = async (isSubmit) => {

        const validObj = await checkDataValid();

        if (validObj.valid) {
            // 組成儲存Model
            let requestData = { ...formData };
            requestData.DATA_DATE_SHOW = FormatDate(new Date(), 'tYY_MM');
            requestData.EditFiles = editedFiles.current;
            requestData.TOTAL_ACTUAL_COMP = budgetDataForSave.current[0].TOTAL_ACTUAL_COMP ?? 0;
            requestData.ACTUAL_PAY = budgetDataForSave.current[0].ACTUAL_PAY ?? 0;
            requestData.UNPAY = budgetDataForSave.current[0].UNPAY ?? 0;
            requestData.BALANCE = budgetDataForSave.current[0].BALANCE ?? 0;
            requestData.REVIEW_COMMENTS = formData.REVIEW_COMMENTS;
            requestData.REVIEW_RESULT = formData.REVIEW_RESULT;
            requestData.LOG_STATUS = formData.REVIEW_RESULT == "Y" ? '7' : formData.REVIEW_RESULT == "R" ? '6' : '8';
            requestData.isAudit = isAudit;
            requestData.isSubmit = isSubmit;
            let result = await saveProjectFillClose(requestData)
            if (result.success) {
                showGlobalMessageBox(result.message, () => {
                    // 存檔後續動作判斷 
                    if (isAudit) {// 結案審核
                        // 確認送出，關閉章節頁，並導回來源列表頁
                        if (isSubmit) {
                            closeAndbackToParentWindow(window);
                        } else { // 存檔後reload頁面資料
                            loadData();
                        }
                    } else {// 結案資料，刷新章節表
                        window.location.reload();
                    }
                });
            }
            else {
                showGlobalMessageBox(result.message);
            }
        } else {
            showGlobalMessageBox(validObj.message)
        }
    }

    // 存檔檢核
    const checkDataValid = async () => {
        // 結案審核 - 檢核是否有審核結果、佐證資料
        if (isAudit) {
            const rvwResult = formData.REVIEW_RESULT;
            if (rvwResult !== 'Y' && rvwResult !== 'R' && rvwResult !== 'N') {
                return { valid: false, message: '請選擇審核結果' }
            }
        }

        if (files.length === 0 && editedFiles.current.length === 0) {
            return { valid: false, message: '請上傳佐證資料' }
        }

        return { valid: true }
    }

    // 取消
    const cancel = async () => {
        loadData();
    }

    // 預覽列印
    const preview = async () => {
        openProjectPrint(state);
    }

    useEffect(() => {
        loadData();
    }, [])

    return (
        <CollapseBoardCard
            button={
                <div className='fn-buttons'>
                    {/* 結案審核按鈕 */}
                    {
                        isAudit &&
                        <>
                            <Button title="存檔" onClick={() => { save(false) }} >存檔</Button>
                            <Button className='k-button-lighten' onClick={cancel}  >取消</Button>
                            <Button title="確認送出" onClick={() => {
                                showGlobalConfirmBox('請確認是否確認送出？', () => { save(true) })
                            }}>確認送出</Button>
                        </>
                    }
                    {/* 結案資料按鈕 */}
                    {
                        (!isAudit && showSomeBtn && canSave) &&
                        <>
                            <Button title="存檔" onClick={() => { save(false) }} >存檔</Button>
                            <Button className='k-button-lighten' onClick={cancel}  >取消</Button>
                        </>
                    }
                    <Button className='k-button-lighten' onClick={preview}  >預覽列印</Button>
                </div>
            }
            title={isAudit ? "結案審核" : "結案資料"} isFirstArea={true}
        >

            <form>
                <table className={Table.fullWidth}>
                    <tbody>
                        <tr>
                            <th rowSpan={2}>經費支用情形</th>
                            <td>
                                <ProjectFillCloseAuditGrid
                                    data={budgetData}
                                    gridChange={gridChange}
                                    isRDEC={isRDEC}
                                    isAudit={isAudit}
                                />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                {!isAudit &&
                                    <span>
                                        1.累計實際完成金額：為實際已完成施做，不論是否已估驗計價之金額。 <br />
                                        2.累計各項經費支用：為係指已完成估驗計價，且廠商已領取之累計金額，包含工程預付款及估驗計價保留款等 (代辦案件不含機關間之撥付金額)。 <br />
                                        3.應付未付數：係指已施作及完成估驗之應付未付廠商款項(代辦案件不含機關間之撥付金額)。 <br />
                                        4.節餘數：係指於本年度辦理發包或工程節餘將於年度結束辦理繳庫，不再發包運用金額(代辦案件不含機關間之撥付金額)。<br />
                                    </span>
                                }
                                累計各項經費支用(元)+應付未付數(元)+結餘數(元)：

                                {Number(
                                    parseFloat(currentExpenseData.currentExpense).toFixed(3)
                                ).toLocaleString("en")}元<br />
                                目前累計經費執行率：{currentExpenseData.currBudgetExeRate.toFixed(2)}%
                            </td>
                        </tr>
                        <tr>
                            <th>
                                <CommonTooltip title={"佐證資料"}
                                    content={
                                        <>
                                            請填報經費支用情形，並上傳結案證明資料，如正驗合格紀錄等。<br />
                                            可上傳檔案格式如下<br />
                                            jpg, jpeg, bmp, png,
                                            mpg, doc, docx, ppt,<br />
                                            pptx, pdf, xls, xlsx,
                                            odt, ods, odp, odg
                                        </>}
                                // withoutRedStar={!isAudit}
                                />
                            </th>
                            <td>
                                <TempFileUploader
                                    {...tempFileUploadService.uploaderParam}
                                    saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                    saveHeaders={{
                                        'authorization': getGlobalServerConfig().BasicData.token.get(),
                                        // 'CacheToken': CacheLoader().GetCache(),
                                    }}
                                    files={files}
                                    setFiles={setFiles}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th>結案審核意見</th>
                            <td>
                                {
                                    (formData.ProjLogs !== undefined && formData.ProjLogs !== null) &&
                                    formData.ProjLogs.map((item, i) =>
                                        <>
                                            <span>{i + 1}.</span>
                                            <span>{FormatDate(item.LOG_DATE)} </span>
                                            <span>{item.LOG_STATUS}</span>
                                            <span>{IsNullOrEmpty(item.MEMO) ? "" : ":"}</span>
                                            <span>{item.MEMO}</span>
                                            <span>。</span>
                                            <br />
                                        </>
                                    )
                                }
                            </td>
                        </tr>
                        {isAudit &&
                            <>
                                <tr>
                                    <th>管考意見</th>
                                    {/* 管考意見只有擁有管考權限才可編輯 */}
                                    {isRDEC ?
                                        <td>
                                            <TextAreaInput
                                                rows={2}
                                                max={500}
                                                maxLength={500}
                                                name="REVIEW_COMMENTS"
                                                defaultValue={formData.REVIEW_COMMENTS}
                                                onBlur={(e) => {
                                                    setFormData({ ...formData, REVIEW_COMMENTS: e.target.element.current.value })
                                                }}
                                            />
                                        </td>
                                        :
                                        <td>{formData.REVIEW_COMMENTS}</td>
                                    }
                                </tr>
                                <tr>
                                    {/* 結案審核的審核結果為必填 */}
                                    <th className={isAudit ? 'addRedStar' : ''}>
                                        審核結果
                                    </th>
                                    <td>
                                        <RadioBoxList
                                            group='REVIEW_RESULT'
                                            valueField='value'
                                            textField='text'
                                            style={{ margin: '0' }}
                                            data={[
                                                { text: '審核通過', value: 'Y', checked: formData.REVIEW_RESULT == 'Y', disabled: !isRDEC },
                                                { text: '退回補正', value: 'R', checked: formData.REVIEW_RESULT == 'R', disabled: !isRDEC },
                                                { text: '審核未通過', value: 'N', checked: formData.REVIEW_RESULT == 'N', disabled: !isRDEC },
                                            ]}
                                            onChange={(e) => setFormData({ ...formData, REVIEW_RESULT: e.value })}
                                        />
                                    </td>
                                </tr>
                            </>
                        }
                    </tbody>
                </table>
            </form>

        </CollapseBoardCard>
    );
}
export default ProjectFillCloseTable
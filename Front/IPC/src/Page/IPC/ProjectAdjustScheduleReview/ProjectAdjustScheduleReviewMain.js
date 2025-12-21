import React, { useState, useEffect, useRef } from 'react';
import utils from '../../../Css/custom/Utils.module.css';
import classnames from 'classnames/bind';
import { getGlobalServerConfig, showGlobalMessageBox, showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import TextAreaInput from '../../../Components/Input/TextAreaInput';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import { TempFileUploadService, fileList } from '../../../Components/Upload/TempFileUploadService';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { FormatDate, IsNullOrEmpty, SetMaskOnOff, closeAndbackToParentWindow } from '../../../Basic/SDOExtension';
import { downProjectAttachment } from "../../../Basic/CommonService";
import CacheLoader from '../../../Basic/CacheLoader';
import { Button } from '@progress/kendo-react-buttons';
import { RadioGroup } from '@progress/kendo-react-inputs';
import { Error } from '@progress/kendo-react-labels';
import { openAdjustContent } from '../ProjectAdjustContent/ProjectAdjustContentService';
import { initData, initFiles, getPageData, saveAuditReview, getZip, validateField, exportRPT } from './ProjectAdjustScheduleReviewService';
import { ProjectScheduleWindow } from '../ProjectScheduleRPT/ProjectScheduleWindow';

const ProjectAdjustScheduleReviewMain = (props) => {
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

    // 初始表單資料
    const initFormData = useRef({ ...initData, AW_KIND: awKind });
    const initApprovalFiles = useRef([]);
    // 表單編輯資料
    const [editedFormData, setEditedFormData] = useState({ ...initData, AW_KIND: awKind });

    // 申請項目文字
    const [awKindText, setAwKindText] = useState("");
    // 調整原因清單
    const [adjustReasons, setAdjustReasons] = useState([]);
    const [errors, setErrors] = useState({});
    const [files, setFiles] = useState([]); // 佐證資料
    // 資料是否載入完成
    const [loadComplete, setloadComplete] = useState(false);

    // 進度甘特圖window reference
    const scheduleWindowRef = useRef();

    // ---------上傳檔案(智發會准簽)-------------
    const [approvalFiles, setApprovalFiles] = useState([]);
    // 檔案上傳異動相關資訊
    const editedApprovalFiles = useRef([]);
    // 紀錄資料資否有異動
    const [isDataChange, setIsDataChange] = useState(false);
    // 下載檔案
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId);
    }
    // 上傳所需參數
    let paramFiles = {
        files: approvalFiles,
        editedFiles: editedApprovalFiles,
        setFiles: setApprovalFiles,
        setIsDataChange,
        multiple: true,
        downFile
    }
    // 暫存檔上傳Service
    const tempFileUploadService = TempFileUploadService(paramFiles);

    useEffect(() => {
        loadData();
    }, []);

    // 取資料
    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await getPageData(projAdjId);
        if (result.length === 3) {
            initFormData.current = result[0];
            setEditedFormData(result[0]);
            setAwKindText(result[1].SET_VALUE);
            setAdjustReasons(result[2]);
            // 設置檔案
            // 佐證資料
            let proveFileList = [];
            // 智發會准簽
            let approvalFileList = result[0].Files.filter(item => {
                if (!(item.FILE_KIND === "21" || item.FILE_KIND === "24")) {
                    proveFileList.push(item);
                    return false;
                }
                return true;
            });
            setFiles(proveFileList);
            let approvalFileData = fileList(approvalFileList, "IDENTITY_FIELD");
            initApprovalFiles.current = fileList(approvalFileList, "IDENTITY_FIELD");
            setApprovalFiles(approvalFileData);
        }
        setloadComplete(true);
        SetMaskOnOff(false);
    }

    /**
     * 存檔
     * @param {*} isSend 0:存檔、1:確認送出 
     */
    const save = async (isSend = 0) => {
        // 驗證
        let isValid = await validate();

        if (!isValid) {
            showGlobalMessageBox("有必填欄位未填寫，請確認資料填妥後重新嘗試！");
        }
        else {
            let data = {
                ...editedFormData,
                Files: [{ ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "24" }],
                IS_SEND: isSend
            };
            if (isSend === 1) {
                showGlobalConfirmBox("請確認是否送出審核結果？", () => saveData(data));
            }
            else {
                saveData(data);
            }
        }
    }

    const saveData = async (data) => {
        SetMaskOnOff(true);
        data.Files[0].EditFiles = editedApprovalFiles.current;
        let saveResult = await saveAuditReview(data);
        SetMaskOnOff(false);

        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                // 確認送出
                if (data.IS_SEND === 1) {
                    closeAndbackToParentWindow(window);
                }
                // 存檔
                else {
                    editedApprovalFiles.current = [];
                    window.location.reload();
                }
            });
        }
        else {
            showGlobalMessageBox(saveResult.message);
        }
    }

    /**
     * 取消
     */
    const reset = () => {
        setEditedFormData(initFormData.current);
        setApprovalFiles(initApprovalFiles.current);
    }

    /**
     * 下載zip檔
     */
    const downloadZip = async () => {
        await getZip(projectNo, projAdjId);
    }

    /**
     * 調整內容
     */
    const getAdjustContent = () => {
        openAdjustContent({ ...state, logId: editedFormData.LOG_ID });
    }

    /**
     * formik內的datepicker onChange事件使用
     * @param {*} e event
     * @param {*} setValues formik setValues 方法 
     * @param {*} values formik values 物件
     */
    const onFormDatePickerChange = (e, setValues, values) => {
        let newObj = {};
        newObj[e.target.name] = FormatDate(e.target.value, 'YYYY-MM-DD');
        setValues({ ...values, ...newObj });
    }

    //驗證
    const validate = async () => {
        let data = {};
        let isValid = false;
        await validateField.validate(editedFormData, { abortEarly: false })
            .then((v) => {
                if (v) {
                    setErrors({});
                    isValid = true;
                }
            }).catch(function (err) {
                err.inner.map(e => data[e.path] = e.message);
                setErrors({ ...data });

            });
        return isValid;
    }

    /**
     * 顯示調整原因
     * @returns 
     */
    const displayReasons = () => {
        if (IsNullOrEmpty(adjustReasons) || IsNullOrEmpty(editedFormData.Reasons)) {
            return null;
        }
        return (
            editedFormData.Reasons.map(reason => {
                let text = adjustReasons.find(item => item.SET_TYPE === reason.SET_TYPE).SET_VALUE;
                return (
                    <li key={reason.SET_TYPE} style={{ display: 'flex', alignItems: 'center' }}>
                        {reason.SET_TYPE === '99'
                            ? text + "：" + editedFormData.OTHER_REASON
                            : text
                        }
                    </li>
                )
            })
        )
    }

    /**
     * 顯示佐證資料
     * @returns 
     */
    const displayFiles = () => {
        if (IsNullOrEmpty(files)) {
            return null;
        }
        return (
            <>
                <div className="fn-buttons">
                    <Button title="下載全部" type='button' onClick={() => downloadZip()} >下載全部</Button>
                </div>
                {
                    files.map(file => {
                        return (
                            <li key={file.IDENTITY_FIELD} style={{ display: 'flex', alignItems: 'center' }}>
                                <div style={{ display: 'flex', alignItems: 'center' }}>
                                    <span onClick={() => downFile(file.IDENTITY_FIELD)}
                                        className={classnames(utils.underline, "k-link")}
                                    >
                                        {file.NAME}：{file.FILE_NAME}
                                    </span>
                                </div>
                            </li>
                        )
                    })
                }
            </>
        )
    }

    return (
        <CollapseBoardCard button={
            <>
                <Button title="存檔" type='button' onClick={() => save()} >存檔</Button>
                <Button title="取消" className="k-button-lighten" onClick={reset}>取消</Button>
            </>
        } title="期程調整審核" isFirstArea={true}>
            <div className="fn-buttons">
                <Button title="確認送出" type='button' onClick={() => save(1)} >確認送出</Button>
                <Button title="調整內容" type='button' onClick={getAdjustContent} className="k-button-lighten">調整內容</Button>
                <Button type='button' className='k-button-lighten' onClick={() => { scheduleWindowRef.current.open() }}>進度甘特圖</Button>
            </div>
            <form>
                <table>
                    <tbody>
                        <tr>
                            <th>申請項目</th>
                            <td>
                                {awKindText}
                                <span style={{ marginLeft: '10px' }}>
                                    <Button title="匯出申請表" type='button' onClick={() => exportRPT(projectNo, projAdjId)} >匯出申請表</Button>
                                </span>
                            </td>
                        </tr>
                        <tr>
                            <th>執行機關</th>
                            <td>
                                {editedFormData.EXEC_UNIT}
                            </td>
                        </tr>
                        <tr>
                            <th>計畫名稱</th>
                            <td>
                                {editedFormData.PROJECT_NAME}
                            </td>
                        </tr>
                        <tr>
                            <th>調整原因</th>
                            <td>
                                {displayReasons()}
                            </td>
                        </tr>
                        <tr>
                            <th>計畫調整原因說明</th>
                            <td>
                                <TextAreaWrapInput value={editedFormData.ADJUST_REASON} />
                            </td>
                        </tr>
                        <tr>
                            <th>佐證資料</th>
                            <td>
                                {displayFiles()}
                            </td>
                        </tr>
                        <tr>
                            <th className="addRedStar">核准日期</th>
                            <td>
                                <TwDatePicker
                                    name="APPRV_DATE"
                                    format={"yyy/MM/dd"}
                                    onChange={(e) => {
                                        onFormDatePickerChange(e, setEditedFormData, editedFormData);
                                    }}
                                    value={editedFormData.APPRV_DATE == null ? null : new Date(editedFormData.APPRV_DATE)}
                                />
                                {<Error>{errors["APPRV_DATE"]}</Error>}
                            </td>
                        </tr>
                        <tr>
                            <th>
                                <CommonTooltip
                                    title={"智發會准簽"}
                                    withoutRedStar={true}
                                    content={
                                        <>
                                            單一檔案上限請勿超過20mb<br />
                                            可上傳檔案格式如下<br />
                                            jpg, jpeg, bmp, png,<br />
                                            mpg, doc, docx, ppt,<br />
                                            pptx, pdf, xls, xlsx,<br />
                                            odt, ods, odp, odg
                                        </>}
                                />
                            </th>
                            <td>
                                <TempFileUploader
                                    {...tempFileUploadService.uploaderParam}
                                    saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                    saveHeaders={{
                                        // @ts-ignore
                                        'authorization': getGlobalServerConfig().BasicData.token.get(),
                                        // 'CacheToken': CacheLoader().GetCache(),
                                    }}
                                    files={approvalFiles}
                                    setFiles={setApprovalFiles}
                                    multiple={true}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th><CommonTooltip title={"是否於期限內提出"} content={"是否於期限內提出"} /></th>
                            <td>
                                <RadioGroup
                                    name="IS_DELAY_APPLY"
                                    value={editedFormData.IS_DELAY_APPLY == null ? "" : editedFormData.IS_DELAY_APPLY}
                                    onChange={(e) => {
                                        if (e.value === 0) {
                                            setEditedFormData({
                                                ...editedFormData,
                                                IS_DELAY_APPLY: e.value,
                                                DELAY_COMMENTS: null
                                            })
                                        }
                                        else {
                                            setEditedFormData({
                                                ...editedFormData,
                                                IS_DELAY_APPLY: e.value
                                            })
                                        }
                                    }}
                                    layout={"horizontal"}
                                    data={[
                                        { label: "是", value: 0 },
                                        { label: "否", value: 1 },
                                    ]}
                                />
                                <Error>{errors["IS_DELAY_APPLY"]}</Error>
                            </td>
                        </tr>
                        <tr>
                            <th>管考意見</th>
                            <td>
                                <TextAreaInput
                                    rows={3}
                                    max={500}
                                    maxLength={500}
                                    name="REVIEW_COMMENTS"
                                    defaultValue={editedFormData.REVIEW_COMMENTS == null ? '' : editedFormData.REVIEW_COMMENTS}
                                    style={{ width: "100%" }}
                                    onBlur={(e) => {
                                        setEditedFormData({
                                            ...editedFormData,
                                            REVIEW_COMMENTS: e.target.element.current.value
                                        })
                                    }}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th className="addRedStar">管考審核結果</th>
                            <td>
                                <RadioGroup
                                    name="REVIEW_RESULT"
                                    value={editedFormData.REVIEW_RESULT == null ? "" : editedFormData.REVIEW_RESULT}
                                    onChange={(e) => {
                                        setEditedFormData({
                                            ...editedFormData,
                                            REVIEW_RESULT: e.value
                                        })
                                    }}
                                    layout={"horizontal"}
                                    data={[
                                        { label: "審核通過", value: 'Y' },
                                        { label: "審核未通過", value: 'N' },
                                        { label: "退回修正", value: 'R' },
                                    ]}
                                />
                                <Error>{errors["REVIEW_RESULT"]}</Error>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </form>
            {loadComplete &&
                <ProjectScheduleWindow
                    projectNo={projectNo}
                    ref={scheduleWindowRef}
                    checkedItems={["3", "4", "5"]}
                    projectAwStatus={editedFormData.PROJECT_AW_STATUS}
                />
            }

        </CollapseBoardCard>
    )
}

export default ProjectAdjustScheduleReviewMain;
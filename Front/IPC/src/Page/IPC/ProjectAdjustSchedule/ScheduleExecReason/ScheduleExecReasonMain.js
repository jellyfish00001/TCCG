import React, { useState, useEffect } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Checkbox } from '@progress/kendo-react-inputs';
import { Error } from '@progress/kendo-react-labels';
import { IsNullOrEmpty, SetMaskOnOff } from '../../../../Basic/SDOExtension';
import { RadioGroup } from '@progress/kendo-react-inputs';
import TextInput from '../../../../Components/Input/TextInput';
import TextAreaInput from '../../../../Components/Input/TextAreaInput';
import TextAreaWrapInput from '../../../../Components/Input/TextAreaWrapInput';
import { showGlobalMessageBox } from '../../../../Route/RootMiddleware';
import { initData, initFiles, getPageData, saveExecReason, removeTempFile } from './ScheduleExecReasonService';
import { getReviewResult } from '../../ProjectAdjustListExec/ProjectAdjustListExecService';
import CommonTooltip from '../../../../Components/Tooltip/CommonTooltip';
import { getGlobalServerConfig } from '../../../../Route/RootMiddleware';
import { TempFileUploader } from '../../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../../Components/Upload/TempFileUploadService';
import CollapseBoardCard from '../../../../Components/BoardCard/CollapseBoardCard';
import { downProjectAttachment } from '../../../../Basic/CommonService';
import CacheLoader from '../../../../Basic/CacheLoader';
import { formatNumber } from '@telerik/kendo-intl';
import { Formik } from "formik";
import * as Yup from 'yup';

const ScheduleExecReasonMain = (props) => {
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
                isShowBtn
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

    const [formData, setFormData] = useState({
        ...initData,
        PROJ_ADJ_ID: projAdjId, // 計畫調整編號
        PROJECT_NO: projectNo, // 計畫列管編號
        AW_KIND: awKind,  // 申請項目
    });

    // 申請項目
    const [awKindText, setAwKindText] = useState("");
    // 調整原因
    const [adjBasicKind, setAdjBasicKind] = useState([]);

    const formRef = React.useRef(null);

    const loadTimes = React.useRef(0);
    // 進度甘特圖window reference
    const scheduleWindowRef = React.useRef();

    // ------------檔案上傳---------------
    const [assessFiles, setAssessFiles] = useState([]); // 上傳中央同意展延期程文件
    const [files, setFiles] = useState([]); // 佐證資料
    // 檔案上傳異動相關資訊
    const editedAsssessFiles = React.useRef([]); // 上傳中央同意展延期程文件
    const editedFiles = React.useRef([]); // 佐證資料
    // 紀錄資料資否有異動
    const [isDataChange, setIsDataChange] = React.useState(false);
    // 下載檔案
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId);
    }

    // 上傳所需參數 (上傳中央同意展延期程文件)
    let paramAssess = {
        files: assessFiles,
        editedFiles: editedAsssessFiles,
        setFiles: setAssessFiles,
        setIsDataChange,
        multiple: true,
        downFile
    }
    // 上傳所需參數 (佐證資料)
    let paramFiles = {
        files: files,
        editedFiles: editedFiles,
        setFiles: setFiles,
        setIsDataChange,
        multiple: true,
        downFile
    }
    // 暫存檔上傳Service
    const tempFileUploadService1 = TempFileUploadService(paramAssess); // 上傳核定函
    const tempFileUploadService2 = TempFileUploadService(paramFiles); // 佐證資料

    useEffect(() => {
        loadData();
    }, []);

    // 取資料
    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await getPageData(projAdjId);

        if (result.length === 3) {
            // 設置申請項目文字
            setAwKindText(result[0].SET_VALUE);
            // 設置調整原因
            setAdjBasicKind(result[1]);
            // 設置檔案
            let assessFileData = fileList(result[2].Files2, "IDENTITY_FIELD");
            let fileData = fileList(result[2].Files, "IDENTITY_FIELD");
            setAssessFiles(assessFileData);
            setFiles(fileData);
            // 設置表單資料
            let newData = {
                ...formData,
                ...result[2],
                Files2: { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "22" },
                Files: { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "12" },
                loadTimes: loadTimes.current
            };
            loadTimes.current = loadTimes.current + 1;
            setFormData(newData);
        }
        SetMaskOnOff(false);
    }

    // 存檔
    const save = async (data) => {
        // 驗證
        if (IsNullOrEmpty(data.Reasons)) {
            showGlobalMessageBox("調整原因必填");
            return;
        }
        if (IsNullOrEmpty(files)) {
            showGlobalMessageBox("請上傳佐證資料");
            return;
        }
        // 儲存
        SetMaskOnOff(true);
        data.NO_OD_REASON = editedAsssessFiles.current.filter(x => x.EditType === 1).length > 0 ? "" : data.NO_OD_REASON;
        data.Files2.EditFiles = editedAsssessFiles.current;
        data.Files.EditFiles = editedFiles.current;
        let saveResult = await saveExecReason(data);
        SetMaskOnOff(false);

        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                editedAsssessFiles.current = [];
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
        editedAsssessFiles.current = [];
        editedFiles.current = [];
        loadData();
    }

    // 刪除FTP上的暫存檔
    const removeTempFilesOnFTP = async (uid) => {
        let result = await removeTempFile(uid);
        if (!result.success) {
            showGlobalMessageBox(result.message);
        }
    }

    /**
     * 刪除檔案 (當「有無獲得中央補助款填寫」填寫無，需清掉原本的檔案)
     * @param {*} removeFiles useState 的 files
     * @param {*} setRemoveFiles useState 的 setFiles
     * @param {*} editedRemoveFiles useRef 存資料庫的editedFiles
     */
    const removeFiles = (removeFiles, setRemoveFiles, editedRemoveFiles) => {
        removeFiles.map(file => {
            if (file.isUploaded) {
                let editFile = {
                    EditType: 2, // 刪除
                    FileId: file.FileId,
                    FileName: file.name,
                    Extension: file.extension
                };
                editedRemoveFiles.current.push(editFile);
                setRemoveFiles(removeFiles.filter(f => f !== file));
            } else {
                // 刪除FTP上的暫存檔
                removeTempFilesOnFTP(file.uid);
                // 將此檔案從files中移除
                setRemoveFiles(removeFiles.filter(f => f !== file));
                // 將此檔案從異動檔案清單移除
                editedRemoveFiles.current = editedRemoveFiles.current.filter(editedFile => editedFile.Uid !== file.uid);
            }
        })
    }

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = async () => {
        if (formRef.current) {
            let isValid = await validateField.isValid(formRef.current.values);
            if (!isValid) {
                showGlobalMessageBox("有必填未填寫");
            }
            formRef.current.handleSubmit();
        }
    }

    // 顯示 checkbox 清單
    const displayCheckBoxList = (values, setValues, errors) => {
        return (
            adjBasicKind.map(data =>
                <div style={{ whiteSpace: 'nowrap' }}>
                    <Checkbox
                        name={'Reasons_' + data.SET_TYPE}
                        value={data.SET_TYPE}
                        label={data.SET_VALUE}
                        checked={values.Reasons.find(item => item.SET_TYPE === data.SET_TYPE) ? true : false}
                        onChange={e => {
                            let adjReasonArr = [...values.Reasons];
                            let adjReasonItem = { SET_ITEM: 'ADJUST_REASON' };
                            let otherReason = values.OTHER_REASON;
                            // 若有無勾選其他，需清掉輸入框內容
                            if (e.target.element.value === '99') {
                                otherReason = e.value ? values.OTHER_REASON : null;
                            }
                            if (e.value) {
                                adjReasonItem = { ...adjReasonItem, SET_TYPE: e.target.element.value };
                                adjReasonArr.push(adjReasonItem);
                            }
                            else {
                                adjReasonArr = values.Reasons.filter(val => val.SET_TYPE !== e.target.element.value);
                            }
                            setValues({ ...values, Reasons: adjReasonArr, OTHER_REASON: otherReason });
                        }}
                    />
                    &nbsp;
                    {// 其他 需有文字框
                        data.SET_TYPE === "99" &&
                        <TextInput
                            name="OTHER_REASON"
                            style={{ width: "90%" }}
                            value={values.OTHER_REASON == null ? '' : values.OTHER_REASON}
                            maxLength={200}
                            error={errors.OTHER_REASON}
                            onChange={(e) => {
                                setValues({ ...values, OTHER_REASON: e.target.value });
                            }}
                        />
                    }
                </div>

            )
        )
    }

    // 欄位驗證
    const validateField = Yup.object().shape({
        PROJECT_NAME: Yup.string().required("此欄位為必填"),
        OTHER_REASON: Yup.string().nullable().test(
            {
                message: "此欄位必填",
                test: function (value) {
                    return this.parent.Reasons.find(v => v.SET_TYPE === "99") && IsNullOrEmpty(value) ? false : true;
                }
            }),
        TENDER_PROJ: Yup.string().nullable().required("此欄位為必填"),
        TENDER_DESIGN: Yup.string().nullable().required("此欄位為必填"),
        TENDER_SUPV: Yup.string().nullable().required("此欄位為必填"),
        TENDER_CONST: Yup.string().nullable().required("此欄位為必填"),
        IS_BUDGET_CENTRAL: Yup.number().nullable().required("此欄位為必填"),
        NO_OD_REASON: Yup.string().nullable().test(
            {
                message: "此欄位必填",
                test: function (value) {
                    // 有無獲得中央補助款填寫有，且上傳核定函 若未上傳，則此欄位必填
                    return this.parent.IS_BUDGET_CENTRAL === 1 && IsNullOrEmpty(assessFiles) && IsNullOrEmpty(value) ? false : true;
                }
            }),
        IS_EFFECT_BUDGET: Yup.number().nullable().test(
            {
                message: "此欄位必填",
                test: function (value) {
                    // 「有無獲得中央補助款」填寫有，則此欄位必填
                    return this.parent.IS_BUDGET_CENTRAL === 1 && IsNullOrEmpty(value) ? false : true;
                }
            }),
        EFFECT_BUDGET_MEMO: Yup.string().nullable().test(
            {
                message: "此欄位必填",
                test: function (value) {
                    // 「有無獲得中央補助款」填寫有，且「本次調整有無影響補助經費請領」填寫有，則此欄位必填
                    return this.parent.IS_BUDGET_CENTRAL === 1 && this.parent.IS_EFFECT_BUDGET === 1 && IsNullOrEmpty(value) ? false : true;
                }
            }),
        CUR_EXECUTION: Yup.string().nullable().required("此欄位為必填"),
        ADJUST_REASON: Yup.string().nullable().required("此欄位為必填"),
    });

    return (
        <CollapseBoardCard button={
            isShowBtn && <>
                <Button title="存檔" onClick={handleSubmit} >存檔</Button>
                <Button title="取消" className="k-button-lighten" onClick={reset}>取消</Button>
            </>
        } title="1.期程調整事由" isFirstArea={true}>
            <Formik
                innerRef={formRef}
                initialValues={formData}
                validationSchema={validateField}
                onSubmit={(data) => save(data)}
                enableReinitialize //允許重複賦予初始值
            >
                {prop => {
                    const {
                        values,
                        errors,
                        handleBlur,
                        handleChange,
                        setValues
                    } = prop;
                    return (
                        <form>
                            <table>
                                <tbody>
                                    <tr>
                                        <th>計畫名稱</th>
                                        <td colSpan={3}>{values.PROJECT_NAME}</td>
                                    </tr>
                                    <tr>
                                        <th>特殊加註</th>
                                        <td colSpan={3}>
                                            {values.SPEC_NOTE}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>申請項目</th>
                                        <td colSpan={3}>{awKindText}</td>
                                    </tr>
                                    <tr>
                                        <th>總計畫經費</th>
                                        <td colSpan={3}>總預算經費：{formatNumber(values.BUDGET, "##,#")}元，發包金額：{formatNumber(values.PROCUREMENT_AMT, "##,#")}元，決標金額：{formatNumber(values.TENDER_AWARDING_AMT, "##,#")}元</td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">承包廠商–專案管理</th>
                                        <td>
                                            <TextInput
                                                name="TENDER_PROJ"
                                                value={values.TENDER_PROJ == null ? "" : values.TENDER_PROJ}
                                                error={errors.TENDER_PROJ}
                                                maxLength={200}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                        <th className="addRedStar">承包廠商–設計單位</th>
                                        <td>
                                            <TextInput
                                                name="TENDER_DESIGN"
                                                value={values.TENDER_DESIGN == null ? "" : values.TENDER_DESIGN}
                                                error={errors.TENDER_DESIGN}
                                                maxLength={200}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">承包廠商–監造單位</th>
                                        <td>
                                            <TextInput
                                                name="TENDER_SUPV"
                                                value={values.TENDER_SUPV == null ? "" : values.TENDER_SUPV}
                                                error={errors.TENDER_SUPV}
                                                maxLength={200}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                        <th className="addRedStar">承包廠商–施工單位</th>
                                        <td>
                                            <TextInput
                                                name="TENDER_CONST"
                                                value={values.TENDER_CONST == null ? "" : values.TENDER_CONST}
                                                error={errors.TENDER_CONST}
                                                maxLength={200}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">有無獲得中央補助款</th>
                                        <td colSpan={3}>
                                            <RadioGroup
                                                name="IS_BUDGET_CENTRAL"
                                                value={values.IS_BUDGET_CENTRAL}
                                                onChange={(e) => {
                                                    if (e.value === 0) {
                                                        // 清空欄位
                                                        setValues({
                                                            ...values,
                                                            IS_BUDGET_CENTRAL: e.value,
                                                            NO_OD_REASON: null,
                                                            IS_EFFECT_BUDGET: null,
                                                            EFFECT_BUDGET_MEMO: null
                                                        });
                                                        // 清空中央同意展延期程文件
                                                        removeFiles(assessFiles, setAssessFiles, editedAsssessFiles);
                                                    }
                                                    else {
                                                        setValues({ ...values, IS_BUDGET_CENTRAL: e.value });
                                                    }
                                                }}
                                                layout={"horizontal"}
                                                data={[
                                                    { label: "有", value: 1 },
                                                    { label: "無", value: 0 }
                                                ]}
                                            />
                                            <Error>{errors.IS_BUDGET_CENTRAL}</Error>
                                        </td>
                                    </tr>
                                    {values.IS_BUDGET_CENTRAL === 1 &&
                                        <>
                                            <tr>
                                                <th>
                                                    <CommonTooltip
                                                        title={"中央同意展延期程文件"}
                                                        content={"如無同意文件，請於下方欄位填寫原因。"}
                                                        withoutRedStar={true}
                                                    />
                                                </th>
                                                <td colSpan={3}>
                                                    <TempFileUploader
                                                        {...tempFileUploadService1.uploaderParam}
                                                        saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                                        saveHeaders={{
                                                            // @ts-ignore
                                                            'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                            // 'CacheToken': CacheLoader().GetCache(),
                                                        }}
                                                        files={assessFiles}
                                                        setFiles={setAssessFiles}
                                                        multiple={true}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>無中央同意展延期程文件原因</th>
                                                <td colSpan={3}>
                                                    <TextInput
                                                        name="NO_OD_REASON"
                                                        value={IsNullOrEmpty(values.NO_OD_REASON) || assessFiles.length > 0 ? "" : values.NO_OD_REASON}
                                                        error={errors.NO_OD_REASON}
                                                        maxLength={200}
                                                        onChange={(e) => {
                                                            setValues({ ...values, NO_OD_REASON: e.value });
                                                            if (!IsNullOrEmpty(e.value)) {
                                                                // 清空中央同意展延期程文件
                                                                removeFiles(assessFiles, setAssessFiles, editedAsssessFiles);
                                                            }
                                                        }}
                                                        onBlur={handleBlur}
                                                        style={{ width: "100%" }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    <CommonTooltip
                                                        title={<div style={{ display: 'inline-flex' }}>本次調整有無影響<br />補助經費之請領</div>}
                                                        content={"請詳實說明本次調整期程與補助款之關係，如經費請領情形，或已向中央展延期限之狀況等"}
                                                    />
                                                </th>
                                                <td colSpan={3}>
                                                    <RadioGroup
                                                        name="IS_EFFECT_BUDGET"
                                                        value={IsNullOrEmpty(values.IS_EFFECT_BUDGET) ? "" : values.IS_EFFECT_BUDGET}
                                                        onChange={(e) => {
                                                            if (e.value === 0) {
                                                                setValues({ ...values, IS_EFFECT_BUDGET: e.value, EFFECT_BUDGET_MEMO: null });
                                                            }
                                                            else {
                                                                setValues({ ...values, IS_EFFECT_BUDGET: e.value });
                                                            }
                                                        }}
                                                        layout={"horizontal"}
                                                        data={[
                                                            { label: "有", value: 1 },
                                                            { label: "無", value: 0 },
                                                        ]}
                                                    />
                                                    <Error>{errors.IS_EFFECT_BUDGET}</Error>
                                                    <TextInput
                                                        name="EFFECT_BUDGET_MEMO"
                                                        value={IsNullOrEmpty(values.EFFECT_BUDGET_MEMO) ? "" : values.EFFECT_BUDGET_MEMO}
                                                        error={errors.EFFECT_BUDGET_MEMO}
                                                        maxLength={200}
                                                        onChange={(e) => {
                                                            setValues({ ...values, EFFECT_BUDGET_MEMO: e.value, IS_EFFECT_BUDGET: 1 });
                                                        }}
                                                        onBlur={handleBlur}
                                                        style={{ width: "100%" }}
                                                    />
                                                </td>
                                            </tr>
                                        </>
                                    }
                                    <tr>
                                        <th className="addRedStar">目前執行情形</th>
                                        <td colSpan={3}>
                                            <TextInput
                                                name="CUR_EXECUTION"
                                                value={values.CUR_EXECUTION == null ? "" : values.CUR_EXECUTION}
                                                error={errors.CUR_EXECUTION}
                                                maxLength={500}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">調整原因</th>
                                        <td colSpan={3}>
                                            {displayCheckBoxList(values, setValues, errors)}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">計畫調整原因說明</th>
                                        <td colSpan={3}>
                                            <TextAreaInput
                                                rows={3}
                                                max={500}
                                                maxLength={500}
                                                name="ADJUST_REASON"
                                                defaultValue={values.ADJUST_REASON == null ? '' : values.ADJUST_REASON}
                                                style={{ width: "100%" }}
                                                onBlur={(e) => {
                                                    setValues({
                                                        ...values,
                                                        ADJUST_REASON: e.target.element.current.value
                                                    });
                                                }}
                                                error={errors.ADJUST_REASON}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <CommonTooltip title={"佐證資料"} content={"請依序上傳佐證資料檔案，檔名將一併呈現於申請表。"}
                                            /></th>
                                        <td colSpan={3}>
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
                                                multiple={true}
                                            />
                                        </td>
                                    </tr>
                                    {values.REVIEW_RESULT &&
                                        <>
                                            <tr>
                                                <th>審核結果</th>
                                                <td colSpan={3}>
                                                    {getReviewResult(values.REVIEW_RESULT)}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>管考意見</th>
                                                <td colSpan={3}>
                                                    <TextAreaWrapInput value={values.REVIEW_COMMENTS} />
                                                </td>
                                            </tr>
                                        </>
                                    }
                                </tbody>
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </CollapseBoardCard>
    )
}

export default ScheduleExecReasonMain;
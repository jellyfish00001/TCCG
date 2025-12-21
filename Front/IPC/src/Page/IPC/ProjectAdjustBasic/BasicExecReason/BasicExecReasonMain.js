import React, { useState, useEffect } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Checkbox } from '@progress/kendo-react-inputs';
import { IsNullOrEmpty, SetMaskOnOff } from '../../../../Basic/SDOExtension';
import TextInput from '../../../../Components/Input/TextInput';
import TextAreaInput from '../../../../Components/Input/TextAreaInput';
import TextAreaWrapInput from '../../../../Components/Input/TextAreaWrapInput';
import { showGlobalMessageBox } from '../../../../Route/RootMiddleware';
import { getPageData, saveExecReason, validateField } from './BasicExecReasonService';
import { getReviewResult } from '../../ProjectAdjustListExec/ProjectAdjustListExecService';
import CommonTooltip from '../../../../Components/Tooltip/CommonTooltip';
import { getGlobalServerConfig } from '../../../../Route/RootMiddleware';
import { TempFileUploader } from '../../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../../Components/Upload/TempFileUploadService';
import CollapseBoardCard from '../../../../Components/BoardCard/CollapseBoardCard';
import { downProjectAttachment } from '../../../../Basic/CommonService';
import CacheLoader from '../../../../Basic/CacheLoader';
import { Formik } from "formik";

const BasicExecReasonMain = (props) => {
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

    // 佐證資料
    const initFiles = {
        PROJECT_NO: projectNo,
        FILE_KIND: "09", // SET_PARAM.SET_ITEM='FILE_KIND'
        EditFiles: []
    };

    const [initData, setInitData] = useState({
        PROJ_ADJ_ID: projAdjId, // 計畫調整編號
        PROJECT_NO: projectNo, // 計畫列管編號
        PROJECT_NAME: "", // 計畫名稱
        AW_KIND: awKind, // 申請項目
        Reasons: [], // 調整原因
        OTHER_REASON: null, // 其他調整原因
        ADJUST_REASON: null, // 計畫調整原因說明
        REVIEW_COMMENTS: null, // 審查原因(退回補正用)
        REVIEW_RESULT: null // 審查結果(退回補正用)
    });

    // 申請項目
    const [awKindText, setAwKindText] = useState("");
    // 調整原因
    const [adjBasicKind, setAdjBasicKind] = useState([]);

    const formRef = React.useRef(null);

    const loadTimes = React.useRef(0);

    // ------------檔案上傳---------------
    const [files, setFiles] = useState([]);
    // 檔案上傳異動相關資訊
    const editedFiles = React.useRef([]);
    // 紀錄資料資否有異動
    const [isDataChange, setIsDataChange] = React.useState(false);
    // 下載檔案
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId);
    }

    // 上傳所需參數
    let param = {
        files,
        editedFiles,
        setFiles,
        setIsDataChange,
        multiple: true,
        downFile
    }
    // 暫存檔上傳Service
    const tempFileUploadService = TempFileUploadService(param);

    useEffect(() => {
        loadData();
    }, []);

    // 取資料
    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await getPageData(projAdjId);

        if (result.length == 3) {
            // 設置申請項目文字
            setAwKindText(result[0].SET_VALUE);
            // 設置調整原因
            setAdjBasicKind(result[1]);
            // 設置檔案
            let fileData = fileList(result[2].Files, "IDENTITY_FIELD");
            setFiles(fileData);
            // 設置表單資料
            let newData = {
                ...initData,
                ...result[2],
                Files: { ...initFiles },
                loadTimes: loadTimes.current
            };
            loadTimes.current = loadTimes.current + 1;
            setInitData(newData);
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

        // 儲存
        SetMaskOnOff(true);
        data.Files.EditFiles = editedFiles.current;
        let saveResult = await saveExecReason(data);
        SetMaskOnOff(false);

        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
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
        editedFiles.current = [];
        loadData();
    }

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
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
                        checked={values.Reasons.find(item => item.SET_TYPE == data.SET_TYPE) ? true : false}
                        onChange={e => {
                            let adjReasonArr = [...values.Reasons];
                            let adjReasonItem = {
                                PROJECT_NO: values.PROJECT_NO,
                                SET_ITEM: 'ADJUST_REASON'
                            };
                            let otherReason = values.OTHER_REASON;
                            // 若有無勾選其他，需清掉輸入框內容
                            if (e.target.element.value == '99') {
                                otherReason = e.value ? values.OTHER_REASON : null;
                            }
                            if (e.value) {
                                adjReasonItem = { ...adjReasonItem, SET_TYPE: e.target.element.value };
                                adjReasonArr.push(adjReasonItem);
                            }
                            else {
                                adjReasonArr = values.Reasons.filter(val => val.SET_TYPE != e.target.element.value);
                            }
                            setValues({ ...values, Reasons: adjReasonArr, OTHER_REASON: otherReason });
                        }}
                    />
                    &nbsp;
                    {// 其他 需有文字框
                        data.SET_TYPE == "99" &&
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

    return (
        <CollapseBoardCard button={
            <>
                <Button title="存檔" onClick={handleSubmit} >存檔</Button>
                <Button title="取消" className="k-button-lighten" onClick={reset}>取消</Button>
            </>
        } title="1.基本資料調整事由" isFirstArea={true} >
            <Formik
                innerRef={formRef}
                initialValues={initData}
                validationSchema={validateField}
                onSubmit={(data) => save(data)}
                enableReinitialize //允許重複賦予初始值
            >
                {prop => {
                    const {
                        values,
                        errors,
                        handleBlur,
                        handleSubmit,
                        handleChange,
                        setValues
                    } = prop;
                    return (
                        <form>
                            <table>
                                <tbody>
                                    <tr>
                                        <th>計畫名稱</th>
                                        <td>{values.PROJECT_NAME}</td>
                                    </tr>
                                    <tr>
                                        <th>申請項目</th>
                                        <td>
                                            {awKindText}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">調整原因</th>
                                        <td>
                                            {displayCheckBoxList(values, setValues, errors)}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">計畫調整原因說明</th>
                                        <td>
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
                                        <th>佐證資料</th>
                                        <td>
                                            <TempFileUploader
                                                {...tempFileUploadService.uploaderParam}
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
                                                <td>
                                                    {getReviewResult(values.REVIEW_RESULT)}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>管考意見</th>
                                                <td>
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

export default BasicExecReasonMain;
import React, { useState, useEffect } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Checkbox } from '@progress/kendo-react-inputs';
import { FormatDate, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import TextInput from '../../../Components/Input/TextInput';
import TextAreaInput from '../../../Components/Input/TextAreaInput';
import { DropDownWithFilter } from '../../../Components/Dropdowns/DropDownWithFilter';
import { showGlobalMessageBox, showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import { initData, initFiles, getWindowData, saveExecReason, validateField } from '../ProjectAdjustRevoke/ProjectAdjustRevokeService';
import { getProjectNameDropDown } from '../ProjectAdjustListExec/ProjectAdjustListExecService';
import { getReviewResult } from '../ProjectAdjustListExec/ProjectAdjustListExecService';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../Components/Upload/TempFileUploadService';
import { downProjectAttachment } from "../../../Basic/CommonService";
import CacheLoader from '../../../Basic/CacheLoader';
import { Formik } from "formik";
import { WindowResizehook } from '../../../Hook/useWindowResize';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';

const ProjectRevokeMain = (props) => {
    const { closeWindow, loadData, awKind, projAdjId } = props;

    const dimensions = WindowResizehook();

    // 表單資料
    const [formData, setFormData] = useState(initData);
    // 撤銷原因清單
    const [revokeReasonList, setRevokeReasonList] = useState([]);

    const [ddlData, setDdlData] = React.useState([]);

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
        loadWindowData();
    }, []);

    // 取資料
    const loadWindowData = async () => {
        SetMaskOnOff(true);
        const data = await getProjectNameDropDown("");
        setDdlData(data);
        let result = await getWindowData(projAdjId);
        // 若有傳入 projAdjId，表為修改(退回修正用)，有初始資料
        if (projAdjId != null && result.length == 2) {
            // 設置表單資料
            let newData = {
                ...formData,
                ...result[0],
                Files: { ...initFiles, PROJECT_NO: result[0].PROJECT_NO },
            }
            setFormData(newData);
            // 設置檔案
            let fileData = fileList(result[0].Files, "IDENTITY_FIELD");
            setFiles(fileData);
            // 設置撤銷原因清單
            setRevokeReasonList(result[1]);
        } else {
            setFormData({
                ...formData,
                AW_KIND: awKind.SET_TYPE,
            });
            setRevokeReasonList(result);
        }
        SetMaskOnOff(false);
    }

    // 存檔/送出
    const save = async (formData) => {
        // 驗證
        if (IsNullOrEmpty(formData.Reasons)) {
            showGlobalMessageBox("撤銷原因必填");
            return;
        }
        if (IsNullOrEmpty(editedFiles.current) && IsNullOrEmpty(files)) {
            showGlobalMessageBox("請上傳佐證資料");
            return;
        }

        let data = { ...formData };
        if (projAdjId == null) {
            // 處理資料
            data = {
                ...formData,
                Files: { ...formData.Files, PROJECT_NO: formData.PROJECT_NO }
            };
        }

        // 儲存
        showGlobalConfirmBox("請確認計畫名稱是否正確，送出後即進入撤銷審核狀態。", async () => {
            SetMaskOnOff(true);
            data.Files.EditFiles = editedFiles.current;
            let saveResult = await saveExecReason(data);
            SetMaskOnOff(false);

            if (saveResult.success) {
                showGlobalMessageBox(saveResult.message, () => {
                    loadData();
                    closeWindow();
                    setFiles([]);
                });
            }
            else {
                showGlobalMessageBox(saveResult.message);
            }
        });
    }

    // 顯示 checkbox 清單
    const displayCheckBoxList = (values, setValues, errors) => {
        return (
            revokeReasonList.map(data =>
                <div style={{ whiteSpace: 'nowrap' }}>
                    <Checkbox
                        name={'Reasons_' + data.SET_TYPE}
                        value={data.SET_TYPE}
                        label={data.SET_VALUE}
                        checked={values.Reasons.find(item => item.SET_TYPE == data.SET_TYPE) ? true : false}
                        onChange={e => {
                            let revokeReasonArr = [...values.Reasons];
                            let revokeReasonItem = { SET_ITEM: 'REVOKE_REASON' };
                            let otherReason = values.OTHER_REASON;
                            // 若有無勾選其他，需清掉輸入框內容
                            if (e.target.element.value == '99') {
                                otherReason = e.value ? values.OTHER_REASON : null;
                            }
                            if (e.value) {
                                revokeReasonItem = { ...revokeReasonItem, SET_TYPE: e.target.element.value };
                                revokeReasonArr.push(revokeReasonItem);
                            }
                            else {
                                revokeReasonArr = values.Reasons.filter(val => val.SET_TYPE != e.target.element.value);
                            }
                            setValues({ ...values, Reasons: revokeReasonArr, OTHER_REASON: otherReason });
                        }}
                    />
                    &nbsp;
                    {// 其他 需有文字框
                        data.SET_TYPE == "99" &&
                        <TextInput
                            name="OTHER_REASON"
                            style={{ width: dimensions.width * 0.28 }}
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

    return (
        <>
            <Formik
                initialValues={formData}
                validationSchema={validateField}
                onSubmit={(data) => save(data)}
                enableReinitialize //允許重複賦予初始值
            >
                {prop => {
                    const {
                        values,
                        errors,
                        handleSubmit,
                        setValues
                    } = prop;
                    return (
                        <form onSubmit={handleSubmit}>
                            <div className="fn-buttons">
                                <Button type="submit">送出申請</Button>
                            </div>
                            <h4>申請撤銷列管，應依本府重大建設計畫選項列管作業要點第9點規定，敘明理由及檢具事證，會辦智發會後，專簽本府一層核定，再按「送出申請」。</h4>
                            <table>
                                <tbody>
                                    <tr>
                                        <th className={projAdjId != null ? "" : "addRedStar"}>計畫名稱</th>
                                        <td>
                                            {projAdjId != null ?
                                                values.PROJECT_NAME
                                                :
                                                <DropDownWithFilter
                                                    ddlData={ddlData}
                                                    style={{ width: '100%' }}
                                                    value={values.PROJECT_NO}
                                                    textField={"PROJECT_NAME"}
                                                    keyField={"PROJECT_NO"}
                                                    typingLength={1}
                                                    onChange={(e) => {
                                                        setValues({
                                                            ...values,
                                                            PROJECT_NO: e.PROJECT_NO,
                                                            PROJECT_NAME: e.PROJECT_NAME,
                                                        });
                                                    }}
                                                    error={errors.PROJECT_NO}
                                                />
                                            }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>申請項目</th>
                                        <td>
                                            {awKind.SET_VALUE}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th><CommonTooltip title={"撤銷原因"} content={"撤銷原因"} /></th>
                                        <td>
                                            {displayCheckBoxList(values, setValues, errors)}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">計畫撤銷原因說明</th>
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
                                        <th className="addRedStar">核准日期</th>
                                        <td>
                                            <TwDatePicker
                                                name="APPRV_DATE"
                                                format={"yyy/MM/dd"}
                                                onChange={(e) => {
                                                    onFormDatePickerChange(e, setValues, values);
                                                }}
                                                value={values.APPRV_DATE == null ? null : new Date(values.APPRV_DATE)}
                                                error={errors.APPRV_DATE}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <CommonTooltip title={"撤銷准簽"} content={
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
        </>
    )
}

export default ProjectRevokeMain;
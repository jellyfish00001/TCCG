import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import {  SetMaskOnOff, IsNullOrEmpty } from "../../../Basic/SDOExtension";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import TextInput from '../../../Components/Input/TextInput';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import { WindowResizehook } from '../../../Hook/useWindowResize';
import WindowBox from '../../../Components/Dialogs/WindowBox';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { Window } from '@progress/kendo-react-dialogs';
/** --- 檔案上傳 --- **/
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../Components/Upload/TempFileUploadService';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import CacheLoader from '../../../Basic/CacheLoader';
import { downProjectAttachment, getSysYearMonth } from '../../../Basic/CommonService';
/** ------ **/
import { SaveRDResPolicyIndex, validationSchema, initFiles } from './ProjectExecManageService';

/**
 * 研究發展作業系統-委託研究計劃-執行情形管理
 */
const ProjectExecManageWindow = (props) => {
    const dimensions = WindowResizehook();
    // 外部傳進來的資料
    const { ddlData, windowData, setWindowVisible, isEdit } = props;
    //表單異動資料
    const formRef = useRef();
    //表單資料
    const [formData, setFormData] = useState({});

    /***  檔案上傳 ***/
    const [file, setFile] = useState([]);
    // 檔案上傳異動相關資訊
    const editedFile = useRef([]);
    // 紀錄資料資否有異動
    const [isDataChange, setIsDataChange] = useState(false);
    // 下載檔案
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId);
    }
    // 成果上傳所需參數
    let paramFiles = {
        files: file,
        editedFiles: editedFile,
        setFiles: setFile,
        setIsDataChange,
        multiple: false,
        downFile
    }
    // 暫存檔上傳Service
    const tempFileUploadService = TempFileUploadService(paramFiles);

    useEffect(() => {
        if(windowData){
            let fileArr = []
            if(!IsNullOrEmpty(windowData["FILE"])){
                let File = windowData["FILE"];
                fileArr.push(File)
                let FileData = fileList(fileArr, "IDENTITY_FIELD");
                initFiles.current = fileList(fileArr, "IDENTITY_FIELD");
                setFile(FileData);
            }
            let winDataWithFile = {
                ...windowData,
                FILE: { ...initFiles, PROJECT_NO: windowData.PLAN_NO, FILE_KIND: "03" },
            }
            setFormData(winDataWithFile);
            SetMaskOnOff(false);
        }
        
    }, [windowData]);

    /**
     * 存檔
     * @param {*} event
     */
    const saveChanges = (event) => {
        event.preventDefault();
        showGlobalConfirmBox("請確認是否存檔", () => { saveChangesEvent() }
        )
    }
    /**
     * 存檔送出資料計畫事件
     */
    const saveChangesEvent = async (isAudit = false) => {
        SetMaskOnOff(true);
        // 送出表單資料
        let data = formRef.current.values;
        // 把檔案塞進 FILE 欄位
        data.FILE.EditFiles = editedFile.current;
        // 若是送審，再加上審核資料
        if(isAudit){
            let item = {
                SUB_NO: windowData.SEQ,         // 執行情形流水號
                MAIN_NO: windowData.PLAN_NO,    // 計畫編號
                PLAN_REVIEW_TYPE: "A2",         // 審查類別
                IS_SEND: 1,                     // 是否確認送出（送出審核）
                AUDIT_YEAR: getSysYearMonth()[0],   // 系統民國年
                AUDIT_MONTH: getSysYearMonth()[1],  // 系統民國月
            }
            data = {
                ...data,
                AUDIT: item
            }
        }
        let result = await SaveRDResPolicyIndex(data);
        if(result.success){
            showGlobalMessageBox(result.message, () => {
                // 畫面 reload
                window.location.reload()
            })
        }
        SetMaskOnOff(false);
    }
    /**
     * 表單取消
     * @param {*} event
     */
    const cancel = (event) => {
        event.preventDefault();
        if (formRef.current) {
            // 重置表單為初始值
            formRef.current.resetForm({ values: formData });
        }
    };
    /**
     * 送出審核
     */
    const audit = () => {
        showGlobalConfirmBox("請確認是否送出審核" ,() => { saveChangesEvent(true) })
    };
        return (
            <Window
                initialWidth={dimensions.width * .7}
                initialHeight={dimensions.height * .9}
                onClose={() => { setWindowVisible() }}
                title={"執行情形填報"}
                draggable={true}
                resizable={true}
                modal={true}
            >
                    <Formik
                        initialValues={formData}
                        onSubmit={(data) => audit(data)}
                        enableReinitialize={true}
                        innerRef={formRef}
                        validationSchema={validationSchema}
                    >
                        {props => {
                            const {
                                values,
                                errors,
                                handleChange,
                                handleSubmit,
                                setValues
                            } = props;
                            return (
                                <form>
                                    <div className="fn-buttons">
                                    {
                                    /* 點編輯打開 Window 才能有功能按鈕 */
                                    isEdit
                                    &&
                                    <>
                                    <Button title="存檔" onClick={saveChanges}>存檔</Button>
                                    <Button title="送出審核" onClick={handleSubmit}>送出審核</Button>
                                    <Button title="取消" onClick={cancel}>取消</Button>
                                    </>
                                    }
                                    </div>
                                    <table>
                                            <tr>
                                                <th>
                                                    計畫編號
                                                </th>
                                                <td colSpan={3}>
                                                    {windowData.PLAN_NO}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    計畫名稱
                                                </th>
                                                <td >
                                                    {windowData.PLAN_NAME}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    年度
                                                </th>
                                                <td >
                                                    {values.PLAN_YEAR}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    季度
                                                </th>
                                                <td >
                                                    {values.PLAN_QQ}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    委託單位
                                                </th>
                                                <td >
                                                    {values.ENTRUST_UNIT_NAME}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    研究主持人
                                                </th>
                                                <td >
                                                    {values.RESEARCH_NAME}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    評核項目
                                                </th>
                                                <td >
                                                    {values.POLICY_INDEX_DESC}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                    執行進度
                                                </th>
                                                <td>
                                                    <DropDownListWithValue
                                                        name="PROGRESS_TYPE"
                                                        data={ddlData}
                                                        textField={"SET_VALUE"}
                                                        dataItemKey={"SET_TYPE"}
                                                        value={values.PROGRESS_TYPE}
                                                        onChange={(e) => { setValues({ ...values, PROGRESS_TYPE: e.target.value }) }}
                                                        error={errors.PROGRESS_TYPE}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                    執行情形簡述
                                                </th>
                                                <td >
                                                    <TextInput
                                                        name="EXECUTION_DESC"
                                                        value={values.EXECUTION_DESC}
                                                        onChange={handleChange}
                                                        style={{ width: '100%' }}
                                                        error={errors.EXECUTION_DESC}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    落後原因
                                                </th>
                                                <td>
                                                <PureHtmlTextAreaInput
                                                    rows={5}
                                                    name='BEHIND_REASON'
                                                    value={values.BEHIND_REASON}
                                                    error={errors.BEHIND_REASON}
                                                    onChange={handleChange}
                                                    style={{ width: "100%" }}
                                                />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    解決對策
                                                </th>
                                                <td>
                                                <PureHtmlTextAreaInput
                                                    rows={5}
                                                    name='SOLUTIONS'
                                                    value={values.SOLUTIONS}
                                                    error={errors.SOLUTIONS}
                                                    onChange={handleChange}
                                                    style={{ width: "100%" }}
                                                />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>附件上傳</th>
                                                <td>
                                                    <TempFileUploader
                                                        {...tempFileUploadService.uploaderParam}
                                                        saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                                        saveHeaders={{
                                                            // @ts-ignore
                                                            'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                            'CacheToken': CacheLoader().GetCache(),
                                                        }}
                                                        files={file}
                                                        setFiles={setFile}
                                                        multiple={false}
                                                    />
                                                </td>
                                            </tr>
                                    </table>
                                </form>
                            )
                        }}
                    </Formik>
                    
        </Window>
        )
        }
        export default ProjectExecManageWindow;
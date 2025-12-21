import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { SetMaskOnOff, IsNullOrEmpty } from "../../../Basic/SDOExtension";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { Button } from '@progress/kendo-react-buttons';
import TextInput from '../../../Components/Input/TextInput';
import WindowBox from '../../../Components/Dialogs/WindowBox';
import { Window } from '@progress/kendo-react-dialogs';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
/** --- 檔案上傳 --- **/
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../Components/Upload/TempFileUploadService';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import CacheLoader from '../../../Basic/CacheLoader';
import { downProjectAttachment, getSysYearMonth } from '../../../Basic/CommonService';
/** ------ **/
import { ProjectBasicGrid } from "../ProjectBasic/ProjectBasicGrid"
import { validataPolicyField, validationSchema, SaveRDResPolicyIndex, initFiles } from "./ProjectExtensionService"

/**
 * 研究發展作業系統-委託研究計劃-執行情形管理
 * 執行情形管理
 */
const ProjectExtensionWindow = (props) => {
    // 外部傳進的資料
    const { ddlData, windowData, setWindowVisible } = props;
    // 是否可以使用功能鍵
    const [isDisable, setIsDisable] = useState(true);
    //表單異動資料
    const formRef = useRef();
    //表單資料
    const [formData, setFormData] = useState({});
    // 紀錄評核指標(多Grid)異動資料
    const editedGridData = React.useRef([]);

    /*********************  檔案上傳 *********************/
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

    // 表單初始化資料
    useEffect(() => {
        if(windowData){
            SetMaskOnOff(true);
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
     * 驗證評核指標欄位
     * @return {*} 
     */
    const checkPolicyIsValid = async () => {
        let notValids = [];
        // 有異動的評核指標目前資料長度
        let dataLength = editedGridData.current.length;
        if (dataLength > 0) {
            for (let index = 0; index < dataLength; index++) {
                const item = editedGridData.current[index];
                if (item.editType !== 3) {
                    let isValid = await validataPolicyField.isValid(item);
                    if (!isValid) {
                        notValids.push(isValid);
                    }
                }
            }
            if (notValids.length > 0) {
                showGlobalMessageBox('評核指標有必填欄位未填');
                return false;
            }
        }
        // 有異動的評核指標無資料，以及原始資料的評核指標也無資料
        if(dataLength === 0 && windowData.policyIndex.length === 0){
            showGlobalMessageBox('評核指標未填');
            return false;
        }
        return true;
    }
    /**
     * 送出前處理資料
     */
    const processDataBeforeSubmit = () => {
        let data = formRef.current.values;
        // 將民國年字串轉成西元年
        const EXTP_LANEND_YEAR = parseInt(data.EXTP_LANEND_YEAR, 10) + 1911;
        const EXTP_LANEND_MONTH = data.EXTP_LANEND_MONTH.toString().padStart(2, '0')
        const dateStr = EXTP_LANEND_YEAR.toString() + "-" + EXTP_LANEND_MONTH + "-01"
        // 預期完成日期
        data.EXTP_LANEND_DATE = dateStr;
        // 評核指標
        data.policyIndex = editedGridData.current;
        return data;
    }
    /**
     * 確認存檔
     * @param {*} data
     */
    const saveChanges = async (event) => {
        event.preventDefault();
        showGlobalConfirmBox("請確認是否存檔", () => { saveChangesEvent() }
        )
    }
    /**
     * 存檔送出資料計畫事件
     * @param {object} data
     */
    const saveChangesEvent = async (isAudit = false) => {
        SetMaskOnOff(true);
        // 送出前處理資料
        let data = processDataBeforeSubmit();
        // 若是送審，再加上審核資料
        if(isAudit){
            // 審查類別PLAN_REVIEW_TYPE：基本資料A1、執行情形填報A2、結案成果填報A3、續列管一年內參採情形A4、展延申請A5
            let item = {
                PLAN_REVIEW_TYPE: "A5",             // 審查類別
                MAIN_NO: windowData.PLAN_NO,        // 計畫編號
                SUB_NO: windowData.EXTENSION_ID,    // 展延流水號
                IS_SEND: 1,                         // 是否確認送出（送出審核）
                AUDIT_YEAR: getSysYearMonth()[0],   // 系統民國年
                AUDIT_MONTH: getSysYearMonth()[1],  // 系統民國月
            }
            data = {
                ...data,
                AUDIT: item
            }
        }
        // 儲存執行情形明細資料
        let result = await SaveRDResPolicyIndex(data);
        if(result.success){
            showGlobalMessageBox(result.message, () =>
            {   // 畫面 reload
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
        if (formRef.current) {
            formRef.current.validateForm().then(errors => {
                    // 檢查表單是否有錯誤
                    if (Object.keys(errors).length === 0 && checkPolicyIsValid()) {
                        showGlobalConfirmBox("請確認是否送出審核" ,
                        () => {
                            saveChangesEvent(true);
                        })
                    }else{
                        showGlobalMessageBox("有欄位尚未填寫");
                    }
                });
            }
    };
    return (
        <WindowBox
            width={80}
            height={75}
            onClose={() => { setWindowVisible() }}
            title={"展延申請"}
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
                                {
                                    isDisable
                                    ?
                                    <div className="fn-buttons">
                                        <Button title="存檔" onClick={saveChanges}>存檔</Button>
                                        <Button title="送審" onClick={handleSubmit}>送審</Button>
                                        <Button title="取消" onClick={cancel}>取消</Button>
                                    </div>
                                    :
                                    null
                                }
                                <table>
                                        <tr>
                                            <th>
                                                申請展延編號
                                            </th>
                                            <td colSpan={3}>
                                                {values.EXTENSION_NO}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                展延原因
                                            </th>
                                            <td>
                                                <TextInput
                                                    name="EXT_REASON"
                                                    value={values.EXT_REASON}
                                                    onChange={handleChange}
                                                    style={{ width: '100%' }}
                                                    error={errors.EXT_REASON}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>展延附件上傳</th>
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
                                        <tr>
                                            <th>
                                                計畫編號
                                            </th>
                                            <td colSpan={3}>
                                                {values.PLAN_NO}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                研究名稱
                                            </th>
                                            <td >
                                                {values.PLAN_NAME}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                計畫期程
                                            </th>
                                            <td >
                                                {values.ORG_PLAN_DATE}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                調整計畫期程
                                            </th>
                                            <td>
                                                <div style={{ display: "flex", alignItems: "center" }}>
                                                    <DropDownListWithValue
                                                        name="EXTP_LANEND_YEAR"
                                                        textField="text"
                                                        dataItemKey="value"
                                                        data={ddlData.PLAN_YEAR}
                                                        value={values.EXTP_LANEND_YEAR}
                                                        style={{ width: "150px" }}
                                                        onChange={(e) => { setValues({ ...values, EXTP_LANEND_YEAR: e.target.value }) }}
                                                        error={errors.EXTP_LANEND_YEAR} />
                                                    <span style={{ margin: "5px" }}>年</span>
                                                    <DropDownListWithValue
                                                        name="EXTP_LANEND_MONTH"
                                                        textField="text"
                                                        dataItemKey="value"
                                                        data={ddlData.PLAN_MONTH}
                                                        value={values.EXTP_LANEND_MONTH}
                                                        style={{ width: "150px" }}
                                                        onChange={(e) => { setValues({ ...values, EXTP_LANEND_MONTH: e.target.value }) }}
                                                        error={errors.EXTP_LANEND_MONTH} />
                                                    <span style={{ margin: "5px" }}>月</span>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                評核指標
                                            </th>
                                            <td >
                                                <ProjectBasicGrid
                                                    policyIndex={formData.policyIndex}
                                                    editedGridData={editedGridData}
                                                    isExtension={true}
                                                />
                                            </td>
                                        </tr>
                                        
                                </table>
                            </form>
                        )
                    }}
                </Formik>
                
    </WindowBox>
    )
    }
    export default ProjectExtensionWindow;
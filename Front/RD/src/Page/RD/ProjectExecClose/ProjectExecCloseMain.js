import React, { useState, useRef, useEffect } from "react";
import { Formik} from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { GetSetParam, downProjectAttachment, getSysYearMonth } from '../../../Basic/CommonService';
import { IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
/** --- 檔案上傳 --- **/
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../Components/Upload/TempFileUploadService';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import CacheLoader from '../../../Basic/CacheLoader';
/** ---------------- **/
import { ConvertTwToAd, validationSchema, GetRDResSituation, SaveRDResSituation, initFiles } from './ProjectExecCloseService';
import { SaveRDAudit } from "../ProjectAudit/ProjectAuditService"

/**
 * 研究發展作業系統-委託研究計劃-結案一年之內參採情形&結案成果填報 共用畫面
 */
const ProjectExceClose = (props) => { 
    const {
        location: {
            state: {
                PLAN_NO,
                PLAN_NAME,
            }
        }
    } = props;
    // 研究情形下拉選單
    const [ddlData, setDDLData] = useState({
        SITUATION_TYPE: []
    });
    // 是否隱藏功能鍵
    const [isFuncDisable, setIsFuncDisable] = useState(true);
    //表單異動資料
    const formRef = useRef();
    // 初始檔案資料
    const initFile = useRef([]);
    // Form Data
    const [formData, setFormData] = useState({});
    // 因為共用畫面，要判斷當前畫面是否為續列管一年參採情形
    const [situaContinue, setSituaContinue] = useState(false);
    // 檔案上傳
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
    /**
     * 表單取消
     */
    const cancel = () => {
        SetMaskOnOff(true);
        if (formRef.current) {
            // 重置表單為初始值
            formRef.current.resetForm({ values: formData });
        }
        SetMaskOnOff(false);
    };
    /**
     * 透過計畫編號取得 續列管一年內參採情形資料/結案成果填報資料
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let data;
        let actionName;
        // 抓取當前 URL
        let href = window.location.href;
        // 當前 URL 是否包含 ProjectSituaContinue (續列管一年內參採情形)
        let isContains = href.includes("ProjectSituaContinue")
        setSituaContinue(isContains);
        // 判斷現在要撈「續列管一年內參採情形」還是「結案成果填報」資料
        if(isContains){
            // 續列管一年內參採情形
            actionName = 'GetRDResSituaContinue';
        }else{
            // 結案成果填報
            actionName = 'GetRDResSituation';
        }
        data = await GetRDResSituation(PLAN_NO, actionName);
        // 續列管一年內參採情形
        if(isContains){
            // 清空結案日期
            data["CLOSING_DATE"] = null;
            /* 以下情況可使用功能鍵 */
            // 1. 結案成果填報為審核通過(3)，以及續列管一年內參採情形未有資料(null)
            // 2. 結案成果填報為審核通過(3)，以及續列管一年內參採情形為待審核(1)
            // 3. 結案成果填報為審核通過(3)，以及續列管一年內參採情形為審核退回(4)
            if(data.SITUATION_STATUS === '3' && (IsNullOrEmpty(data.SITUACONTINUE_STATUS) || data.SITUACONTINUE_STATUS === '1'|| data.SITUACONTINUE_STATUS === '4')){
                setIsFuncDisable(false);
            }
        }
        // 結案成果填報
        else{
            /* 以下情況可使用功能鍵 */
            // 1. 評核指標不可無資料，且評核指標與評核指標審核通過(3)數量一樣，結案成果填報審核狀態為未送審(1)
            // 2. 評核指標不可無資料，且評核指標與評核指標審核通過(3)數量一樣，結案成果填報審核狀態為審核退回(4)
            // 3. 評核指標不可無資料，且評核指標與評核指標審核通過(3)數量一樣，結案成果填報尚未有審核狀態(null)
            if((data.RD_RES_POLICY_INDEX_COUNT !== 0 && data.RD_RES_POLICY_INDEX_COUNT === data.STATUS_3_COUNT) && (IsNullOrEmpty(data.SITUATION_STATUS) || data.SITUATION_STATUS === '1' || data.SITUATION_STATUS === '4')){
                setIsFuncDisable(false);
            }
        }
        // 參採情形下拉選單
        let situationDropDownList = await GetSetParam("RDSituationType");
        situationDropDownList.unshift({SET_VALUE:"請選擇", SET_TYPE:""});
        setDDLData({
            SITUATION_TYPE: [...situationDropDownList]
        });

        let fileArr = []
        if(!IsNullOrEmpty(data["FILE"])){
            let File = data["FILE"];
            fileArr.push(File)
            let FileData = fileList(fileArr, "IDENTITY_FIELD");
            initFile.current = fileList(fileArr, "IDENTITY_FIELD");
            setFile(FileData);
        }
        
        let newData = {
            ...data,
            FILE: { ...initFiles, PROJECT_NO: PLAN_NO, FILE_KIND: "02" },
        }
        setFormData(newData)
        SetMaskOnOff(false);
    }
    useEffect(() => {
        setFormData({});
        loadData();
    }, []);
    
    /**
     * 表單送出存檔
     * @param {*} event
     */
    const save = (event) => {
        event.preventDefault();
        showGlobalConfirmBox("請確認是否執行動作",
                () => { saveChangesEvent(formRef.current.values) }
            )
    };
    /**
     * 表單送出存檔事件
     * @param {*} data
     */
    const saveChangesEvent = async (data, isAudit = false) => {
        SetMaskOnOff(true);
        let result;
        let actionName;
        if(situaContinue){
            actionName = 'SaveRDResSituaContinue'
        }else{
            actionName = 'SaveRDResSituation'
        }
        // 非續列管一年內參採情形（結案成果填報）才要處理結案日期轉西元年
        if(!situaContinue){
            // 結案日期是民國格式，轉西元
            data.CLOSING_DATE = ConvertTwToAd(data.CLOSING_DATE);
        }
        // 計畫編號
        data.PLAN_NO = PLAN_NO;
        // 把檔案塞進 FILE 欄位
        data.FILE.EditFiles = editedFile.current;
        // 若是送審，再加上審核資料
        if(isAudit){
            // 審查類別PLAN_REVIEW_TYPE：基本資料A1、執行情形填報A2、結案成果填報A3、續列管一年內參採情形A4、展延申請A5
            let item = {
                PLAN_REVIEW_TYPE: situaContinue ? "A4" : "A3",  // 審查類別
                MAIN_NO: PLAN_NO,                   // 計畫編號
                IS_SEND: 1,                         // 是否確認送出（送出審核）
                AUDIT_YEAR: getSysYearMonth()[0],   // 系統民國年
                AUDIT_MONTH: getSysYearMonth()[1],  // 系統民國月
            }
            data = {
                ...data,
                AUDIT: item
            }
        }
        result = await SaveRDResSituation(data, actionName)
        if(result != null){
            showGlobalMessageBox(result.message, () => {window.location.reload()})
        }
        SetMaskOnOff(false);
    }
    /**
     * 送出審核
     */
    const audit = (event) => {
        event.preventDefault();
        if (formRef.current) {
            formRef.current.validateForm().then(errors => {
                    // 檢查表單是否有錯誤
                    if (Object.keys(errors).length === 0) {
                        showGlobalConfirmBox("請確認是否送出審核" ,
                        () => {
                            saveChangesEvent(formRef.current.values, true);
                        })
                    }else{
                        showGlobalMessageBox("有欄位尚未填寫");
                    }
                });
            }
        
    };
    return (
        <PageContainer>
            <CollapseBoardCard
            button={
                <>
                {
                    isFuncDisable
                    ?
                    null
                    :
                    <>
                        <Button title="存檔" className="k-button-lighten" onClick={save}>存檔</Button>
                        <Button title="送出審核" className="k-button-lighten" onClick={audit}>送出審核</Button>
                        <Button title="取消" className="k-button-lighten" onClick={cancel}>取消</Button>
                    </>
                }
                </>
            }
            title="結案情形"
            isFirstArea={true}
            >     
                <Formik
                    initialValues={formData}
                    validationSchema={validationSchema}
                    onSubmit={(data) => audit(data)}
                    enableReinitialize={true}
                    innerRef={formRef}
                    //字段改變不驗證
                    validateOnChange={false}
                >
                    {props => {
                        const {
                            values,
                            errors,
                            handleChange,
                            setValues
                        } = props;
                        return (
                            <form>
                                <table>
                                    <tr>
                                        <th>
                                            計畫編號
                                        </th>
                                        <td>
                                            {PLAN_NO}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            計畫名稱
                                        </th>
                                        <td >
                                            {PLAN_NAME}
                                        </td>
                                    </tr>
                                    
                                    {situaContinue
                                        ? // 是續列管一年內參採情形，顯示以下元件
                                        <>
                                            <tr>
                                                <th>
                                                    結案初步評估採行情形
                                                </th>
                                                <td>
                                                    {values.SITUATION_TYPE}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    採行情形簡述
                                                </th>
                                                <td >
                                                    {values.SITUATION_DESC}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                    續列管一年參採情形
                                                </th>
                                                <td>
                                                    <DropDownListWithValue
                                                        name="CONTINUE_SITUAITON_TYPE"
                                                        data={ddlData.SITUATION_TYPE}
                                                        textField={"SET_VALUE"}
                                                        dataItemKey={"SET_TYPE"}
                                                        value={values.CONTINUE_SITUAITON_TYPE}
                                                        onChange={(e) => { setValues({ ...values, CONTINUE_SITUAITON_TYPE: e.target.value }) }}
                                                        error={errors.CONTINUE_SITUAITON_TYPE}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                    續列管一年參採情形簡述
                                                </th>
                                                <td>
                                                <PureHtmlTextAreaInput
                                                    rows={5}
                                                    name='CONTINUE_SITUAITON_DESC'
                                                    value={values.CONTINUE_SITUAITON_DESC}
                                                    onChange={handleChange}
                                                    error={errors.CONTINUE_SITUAITON_DESC}
                                                />
                                                </td>
                                            </tr>
                                        </>
                                        : // 非續列管一年內參採情形，是結案成果填報，顯示以下元件
                                        <>
                                            <tr>
                                                <th className="addRedStar">
                                                    研究建議處理情形
                                                </th>
                                                <td>
                                                    <DropDownListWithValue
                                                        name="SITUATION_TYPE"
                                                        data={ddlData.SITUATION_TYPE}
                                                        textField={"SET_VALUE"}
                                                        dataItemKey={"SET_TYPE"}
                                                        value={values.SITUATION_TYPE}
                                                        onChange={(e) => { setValues({ ...values, SITUATION_TYPE: e.target.value }) }}
                                                        error={errors.SITUATION_TYPE}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                    採行情形簡述
                                                </th>
                                                <td>
                                                    <PureHtmlTextAreaInput
                                                        rows={5}
                                                        name='SITUATION_DESC'
                                                        value={values.SITUATION_DESC}
                                                        onChange={handleChange}
                                                        error={errors.SITUATION_DESC}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    附件上傳
                                                </th>
                                                <td >
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
                                                    結案日期
                                                </th>
                                                <td >
                                                    {values.CLOSING_DATE}
                                                </td>
                                            </tr>
                                        </>
                                    }
                                </table>
                            </form>
                        )
                    }}
                </Formik>
            </CollapseBoardCard>
        </PageContainer>
    )
    }
    export default ProjectExceClose;
import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { SetMaskOnOff, IsNullOrEmpty } from "../../../Basic/SDOExtension";
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import TextInput from '../../../Components/Input/TextInput';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import NumericTextInput from '../../../Components/Input/NumericTextInput';
import { GetStorageData, SetStorageData, getPlanYearList, getMonthList } from '../../../Basic/CommonService';
/** --- 檔案上傳 --- **/
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../Components/Upload/TempFileUploadService';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import CacheLoader from '../../../Basic/CacheLoader';
import { downProjectAttachment, getSysYearMonth } from '../../../Basic/CommonService';
/** ------ **/
import { ProjectBasicGrid } from "./ProjectBasicGrid"
import { SaveRDBasicStatus } from "../ProjectMain/ProjectMainService";
import { getPlanByNo, savePlan, initFiles, validataPolicyField, validationSchema } from "./ProjectBasicService";

/**
 * 研究發展作業系統-委託研究計劃-計畫基本資料
 */
const ProjectBasicMain = (props) => {
    const {
        location: {
            state: {
                PLAN_NO,
                funRole
            }
        }
    } = props;
    // 初始資料
    const [initData, setInitData] = useState({});
    // 是否隱藏功能鍵
    const [isFuncDisable, setIsFuncDisable] = useState(true);
    // 評核指標是否要顯示必填訊息
    const [isPolicyIndexError, setIsPolicyIndexError] = useState(false);
    // 經費來源是否要顯示必填訊息
    const [isMoneyError, setIsMoneyError] = useState(false);
    // 下拉選單資料
    const [ddlData, setDDLData] = React.useState({
        PLAN_YEAR: [], // 計畫年度
        PLAN_MONTH: [], // 計畫月份
    });
    // 紀錄評核指標(多Grid)異動資料
    const editedGridData = React.useRef([]);
    //表單資料
    const formRef = useRef();
    /*********  檔案上傳 *********/
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
     * 頁面載入資料
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        // 透過計畫編號撈計畫明細
        let project = await getPlanByNo(PLAN_NO);
        // 如果狀態是未送審，或是審核退回，啟用功能鍵
        if(project.RESEARCH_STATUS === '1' || project.RESEARCH_STATUS === '4'){
            setIsFuncDisable(false);
        }
        // 重新更新 session 資料，加上計畫序號 PLAN_ID
        let storageData = GetStorageData();
        storageData.PLAN_ID = project.PLAN_ID;
        SetStorageData(storageData);
        props.location.state = storageData;
        /** 載入下拉選單資料（年度、月份） **/
        // 年分
        let yearDropDownList = await getPlanYearList(false, 20, "A");
        yearDropDownList.unshift({text:"請選擇", value:""});
        // 月份
        let monthDropDownList = await getMonthList();
        monthDropDownList.unshift({text:"請選擇", value:""});
        setDDLData({
            PLAN_YEAR: [...yearDropDownList],
            PLAN_MONTH: [...monthDropDownList]
        });
        // 設置檔案
        if(!IsNullOrEmpty(project["FILE"])){
            let fileArr = []
            let File = project["FILE"];
            fileArr.push(File)
            let FileData = fileList(fileArr, "IDENTITY_FIELD");
            initFiles.current = fileList(fileArr, "IDENTITY_FIELD");
            setFile(FileData);
        }
        let projectWithFile = {
            ...project,
            FILE: { ...initFiles, PROJECT_NO: PLAN_NO, FILE_KIND: "01" },
        }
        setInitData(projectWithFile);
        SetMaskOnOff(false);
    }
    useEffect(() => {
        loadData();
    }, []);
    /**
     * 解除鎖定
     */
    const unlock = () => {
        SetMaskOnOff(true);
        let planArr = [PLAN_NO]
        showGlobalConfirmBox("請確認是否解除鎖定計畫",
            async () => { 
                    let result = await SaveRDBasicStatus(planArr, "L2");
                    showGlobalMessageBox(result.message)
                    }
        )
        SetMaskOnOff(false);
    };
    /**
     * 表單取消
     */
    const cancel = () => {
        SetMaskOnOff(true);
        if (formRef.current) {
            // 重置表單為初始值
            formRef.current.resetForm({ values: initData });
        }
        SetMaskOnOff(false);
    };
    /**
     * 驗證多 Grid 評核指標欄位
     * @return {*} 
     */
    const checkPolicyIsValid = () => {
        let notValids = [];
        // 有異動的評核指標目前資料長度
        let dataLength = editedGridData.current.length;
        if (dataLength > 0) {
            for (let index = 0; index < dataLength; index++) {
                const item = editedGridData.current[index];
                if (item.editType !== 3) {
                    let isValid = validataPolicyField.isValid(item);
                    if (!isValid) {
                        notValids.push(isValid);
                    }
                }
            }
            if (notValids.length > 0) {
                return false;
            }
        }
        // 有異動的評核指標無資料，以及原始資料的評核指標也無資料
        if(dataLength === 0 && initData.policyIndex.length === 0){
            return false;
        }
        return true;
    }
    /**
     * 驗證預算欄位是否至少有填一項
     * @return {*} 
     */
    const checkMoneyIsValid = () => {
        // 目前表單資料
        let data = formRef.current.values;
        // 公務預算、基金預算、中央預算、其他預算至少有一欄位要非 0
        if (data.PUBLIC_MONEY !== 0 || data.FUND_MONEY !== 0 || data.CENTER_MONEY !== 0 || data.OTHER_MONEY !== 0) {
            return true;
        }else{
            setIsMoneyError(true);
            return false;
        }
    }
    /**
     * 送出前處理資料
     */
    const processDataBeforeSubmit = () => {
        let data = formRef.current.values;
        let PLAN_START_YEAR;
        let PLAN_START_DATE = null;
        let PLAN_END_YEAR;
        let PLAN_END_DATE = null;
        // 將民國年字串轉成西元年，起年跟月有一個未填則不處理
        if(!IsNullOrEmpty(data.PLAN_START_YEAR) && !IsNullOrEmpty(data.PLAN_START_MONTH)){
            PLAN_START_YEAR = parseInt(data.PLAN_START_YEAR, 10) + 1911;
            PLAN_START_DATE = PLAN_START_YEAR + "-" + data.PLAN_START_MONTH + "-01"
        }
        // 將民國年字串轉成西元年，訖年跟月有一個未填則不處理
        if(!IsNullOrEmpty(data.PLAN_END_YEAR) && !IsNullOrEmpty(data.PLAN_END_MONTH)){
            PLAN_END_YEAR = parseInt(data.PLAN_END_YEAR, 10) + 1911;
            PLAN_END_DATE = PLAN_END_YEAR + "-" + data.PLAN_END_MONTH + "-01";
        }
        // 把檔案塞進 FILE 欄位
        data.FILE.EditFiles = editedFile.current;
        data.PLAN_START_DATE = PLAN_START_DATE;
        data.PLAN_END_DATE = PLAN_END_DATE;
        // 送出資料加上評核指標
        data.policyIndex = editedGridData.current;
        return data;
    }
    /**
     * 存檔送出儲存資料計畫
     * @param {object} data 表單資料
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
        // 送出前處理資料
        let data = processDataBeforeSubmit();
        // 若是送審，再加上審核資料
        if(isAudit){
            let item = {
                PLAN_REVIEW_TYPE: "A1",             // 審查類別
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
        // 存檔送出儲存資料計畫
        let result = await savePlan(data);
        if(result.success){
            showGlobalMessageBox(result.message, () => {
                // 重新更新 session 資料
                let result = GetStorageData();
                // 計畫名稱可能有改變，須更新
                result.PLAN_NAME = data.PLAN_NAME;
                SetStorageData(result);
                props.location.state = result;
                // 畫面 reload
                window.location.reload();
            })
        }
        SetMaskOnOff(false);
    }
    /**
     * 送出審核 submit，利用 Ref 把 Formik 的 submit 功能拉出來，以提供外部按鈕呼叫
     */
    const handleSubmit = () => {
        formRef.current.validateForm().then(errors => {
            // 檢查表單瑱入內容、多 Grid 評核指標欄位是否有錯誤，以及預算欄位是否至少有一項非 0
            if (Object.keys(errors).length === 0 && checkPolicyIsValid() && checkMoneyIsValid()) {
                // 皆無錯誤，送出表單
                formRef.current.handleSubmit(); 
            }else{
                setIsPolicyIndexError(true);
                showGlobalMessageBox("有欄位尚未填寫，或填寫格式錯誤");
            }
        });
    }
    /**
     * 送出審核
     */
    const audit = () => {
        showGlobalConfirmBox("請確認是否送出審核", () => { saveChangesEvent(true) })
    };
    return (
        <PageContainer style={{ overflow: "auto", height: "100%" }}>
            <CollapseBoardCard
                button={
                    <>
                    {
                        isFuncDisable
                        ?
                        null
                        :
                        <>
                            <Button title="存檔" type="button" onClick={saveChanges}>存檔</Button>
                            { /* 管考才可以使用解除鎖定功能 */
                            funRole === 2
                            ?
                            <Button title="解除鎖定" className="k-button-lighten" onClick={unlock}>解除鎖定</Button>
                            :
                            null
                            }
                            <Button title="送出審核" className="k-button-lighten" onClick={handleSubmit}>送出審核</Button>
                            <Button title="取消" className="k-button-lighten" onClick={cancel}>取消</Button>
                        </>
                    }
                    </>
                }
                title={"計畫基本資料"}
                isFirstArea={true}
            >     
                <Formik
                    initialValues={initData}
                    validationSchema={validationSchema}
                    onSubmit={(data) => { audit(data)}}
                    enableReinitialize={true} // 允許重複賦予初始值，要外部傳入 initialValues 更新資料
                    innerRef={formRef}
                >
                    {props => {
                        const {
                            values,
                            errors,
                            handleChange,
                            handleBlur,
                            setValues
                        } = props;
                        return (
                            <form>
                                <table>
                                    <tr>
                                        <th colSpan="2" >
                                            計畫編號
                                        </th>
                                        <td colSpan="3">
                                            {values.PLAN_NO}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">
                                            研究名稱
                                        </th>
                                        <td colSpan="3" >
                                            <TextInput
                                                name="PLAN_NAME"
                                                value={values.PLAN_NAME}
                                                style={{ width: "100%" }}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.PLAN_NAME}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" >
                                            委託機關
                                        </th>
                                        <td colSpan="3">
                                            {values.EXEC_ORG_NAME}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">
                                            受託單位
                                        </th>
                                        <td colSpan="3">
                                            <TextInput
                                                name="ENTRUST_UNIT_NAME"
                                                value={values.ENTRUST_UNIT_NAME}
                                                style={{ width: "100%" }}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.ENTRUST_UNIT_NAME}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">
                                            研究主持人
                                        </th>
                                        <td colSpan="3">
                                            <TextInput
                                                name="RESEARCH_NAME"
                                                value={values.RESEARCH_NAME}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.RESEARCH_NAME}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">
                                            計畫期程
                                        </th>
                                        <td colSpan="3">
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                                <DropDownListWithValue
                                                    name="PLAN_START_YEAR"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={ddlData.PLAN_YEAR}
                                                    value={values.PLAN_START_YEAR}
                                                    style={{ width: "140px" }}
                                                    onChange={(e) => { setValues({ ...values, PLAN_START_YEAR: e.target.value }) }}
                                                    error={errors.PLAN_START_YEAR} />
                                                <span style={{ margin: "5px" }}>年</span>
                                                <DropDownListWithValue
                                                    name="PLAN_START_MONTH"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={ddlData.PLAN_MONTH}
                                                    value={values.PLAN_START_MONTH}
                                                    style={{ width: "140px" }}
                                                    onChange={(e) => { setValues({ ...values, PLAN_START_MONTH: e.target.value }) }}
                                                    error={errors.PLAN_START_MONTH} />
                                                <span style={{ margin: "5px" }}>月 ~ </span>
                                                <DropDownListWithValue
                                                    name="PLAN_END_YEAR"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={ddlData.PLAN_YEAR}
                                                    value={values.PLAN_END_YEAR}
                                                    style={{ width: "140px" }}
                                                    onChange={(e) => { setValues({ ...values, PLAN_END_YEAR: e.target.value }) }}
                                                    error={errors.PLAN_END_YEAR} />
                                                <span style={{ PLAN_END_YEAR: "5px" }}>年</span>
                                                <DropDownListWithValue
                                                    name="PLAN_END_MONTH"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={ddlData.PLAN_MONTH}
                                                    value={values.PLAN_END_MONTH}
                                                    style={{ width: "140px" }}
                                                    onChange={(e) => { setValues({ ...values, PLAN_END_MONTH: e.target.value }) }}
                                                    error={errors.PLAN_END_MONTH} />
                                                <span style={{ margin: "5px" }}>月</span>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th rowspan="5" className="addRedStar">
                                            經費來源
                                            {
                                                isMoneyError
                                                &&
                                                <>
                                                <br />
                                                <span style={{color:"red", fontSize: "12px", fontStyle: "italic"}}>
                                                    經費來源至少填一項
                                                </span>
                                                </>
                                            }
                                        </th>
                                    </tr>
                                    <tr>
                                        <th style={{'borderTop':'none'}}>
                                            公務預算
                                        </th>
                                        <td colSpan="3" >
                                            <NumericTextInput
                                                name="PUBLIC_MONEY"
                                                value={values.PUBLIC_MONEY}
                                                onChange={(e) => { setValues({ ...values, PUBLIC_MONEY: IsNullOrEmpty(e.target.value) ? 0 : e.target.value })}}
                                                max={1000000000000000}
                                                min={0}
                                                WithFormik={true}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            基金預算
                                        </th>
                                        <td colSpan="3">
                                            <NumericTextInput
                                                name="FUND_MONEY"
                                                value={values.FUND_MONEY}
                                                onChange={(e) => { setValues({ ...values, FUND_MONEY: IsNullOrEmpty(e.target.value) ? 0 : e.target.value })}}
                                                max={1000000000000000}
                                                min={0}
                                                WithFormik={true}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            中央預算
                                        </th>
                                        <td colSpan="3" >
                                            <NumericTextInput
                                                name="CENTER_MONEY"
                                                value={values.CENTER_MONEY}
                                                onChange={(e) => { setValues({ ...values, CENTER_MONEY: IsNullOrEmpty(e.target.value) ? 0 : e.target.value }) }}
                                                max={1000000000000000}
                                                min={0}
                                                WithFormik={true}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>其他</th>
                                        <td width="350px">
                                            <NumericTextInput
                                                name="OTHER_MONEY"
                                                value={values.OTHER_MONEY}
                                                onChange={(e) => { setValues({ ...values, OTHER_MONEY: IsNullOrEmpty(e.target.value) ? 0 : e.target.value }) }}
                                                max={1000000000000000}
                                                min={0}
                                                WithFormik={true}
                                            />
                                        </td>
                                        <th>來源說明</th>
                                        <td colSpan="3">
                                            <TextInput
                                                name="OTHER_DESC"
                                                value={values.OTHER_DESC}
                                                style={{ width: "100%" }}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.OTHER_DESC}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">評核指標</th>
                                        <td colSpan="3">
                                            <ProjectBasicGrid
                                                policyIndex={initData.policyIndex}
                                                editedGridData={editedGridData}
                                                isExtension={false}
                                                isPolicyIndexError={isPolicyIndexError}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">研究原因及目的</th>
                                        <td colSpan="3">
                                            <PureHtmlTextAreaInput
                                                rows={5}
                                                name='PLAN_CAUSE'
                                                value={values.PLAN_CAUSE}
                                                onChange={handleChange}
                                                error={errors.PLAN_CAUSE}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">計畫項目內容</th>
                                        <td colSpan="3">
                                            <PureHtmlTextAreaInput
                                                rows={5}
                                                name='PLAN_CONTENT'
                                                value={values.PLAN_CONTENT}
                                                onChange={handleChange}
                                                error={errors.PLAN_CONTENT}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">預期研究成果</th>
                                        <td colSpan="3">
                                            <PureHtmlTextAreaInput
                                                rows={5}
                                                name='PLAN_EXPECTED'
                                                value={values.PLAN_EXPECTED}
                                                onChange={handleChange}
                                                error={errors.PLAN_EXPECTED}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">承辦人</th>
                                        <td colSpan="3">
                                            <TextInput
                                                name="CONTACT_NAME"
                                                value={values.CONTACT_NAME}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.CONTACT_NAME}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">電話/分機</th>
                                        <td colSpan="3">
                                            <TextInput
                                                name="CONTACT_TEL"
                                                value={values.CONTACT_TEL}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.CONTACT_TEL}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2" className="addRedStar">承辦人Email</th>
                                        <td colSpan="3">
                                            <TextInput
                                                name="CONTACT_EMAIL"
                                                value={values.CONTACT_EMAIL}
                                                style={{ width: "50%" }}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.CONTACT_EMAIL}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2">代理人Eamil</th>
                                        <td colSpan="3">
                                            <TextInput
                                                name="ASSIGNE_EMAIL"
                                                value={values.ASSIGNE_EMAIL}
                                                style={{ width: "50%" }}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.ASSIGNE_EMAIL}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="2">附件上傳</th>
                                        <td colSpan="3">
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
            </CollapseBoardCard>
        </PageContainer>
    )
    }
    export default ProjectBasicMain;
        


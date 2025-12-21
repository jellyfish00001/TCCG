import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import TextInput from '../../../Components/Input/TextInput';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput';
import { Checkbox, RadioButton } from '@progress/kendo-react-inputs';
import NumericTextInput from '../../../Components/Input/NumericTextInput';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import CheckPointGrid from './CheckPointGrid';
import ProjectFundGrid from './ProjectFundGrid';
import AddNewPlanWindow from './AddNewPlanWindow';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { downProjectAttachment } from '../../../Basic/CommonService';
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import { SavePWSSDPLANMAIN, getData, loadCheckPoint, initFiles, validationSchema } from "./AddNewPlanService";
import { AddNoColumn } from '../../../Basic/SDOExtension';
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { fileList, TempFileUploadService } from '../../../Components/Upload/TempFileUploadService';
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import CacheLoader from '../../../Basic/CacheLoader';
import { GetBasicData } from "../../../Basic/BasicData";
import { SendBackProject } from "../GpReviewProject/GpReviewProjectService";
import { getOrgDeadline } from "../ProjectList/ProjectListService";

const AddNewPlanMain = (props) => {
    // 外部傳入
    const { 
        location: { 
            state: {
                projectNo,
                projectIsSend,
            }
        }
     } = props;

    // 退回按鈕判斷
    const [sendBack, setSendBack] = useState(false);
    // 從SQL來的檢查點資料
    const checkPointFromSql = useRef([0]);
    // 檢核點資料
    const [gridData, setGridData] = useState([]);
    // 表單異動資料
    const [windowVisible, setWindowVisible] = useState(false);
    // 表單傳入資料追蹤
    const [gridChange, setGridChange] = useState([]);
    // 跨年度經費資料追蹤
    const editedGridData = useRef([]);
    // 是否跨年度計畫
    const [isCrossYear, setIsCrossYear] = useState(false);
    // 檢核點資料追蹤
    const editedCheckPointData = useRef([]);
    // 年度下拉選單
    const [yearOptions, setYearOptions] = useState([]);
    // 月份下拉選單
    const [monthOptions, setmonthOptions] = useState([]);
    // 基金下拉選單
    const [fundOptions, setFundOptions] = useState([]);
    // 機關下拉選單
    const [organOptions, setOrganOptions] = useState([]);
    // 核定狀態
    const [APPROVEDYN, setAPPROVEDYN] = useState("Y");
    // 表單資料
    const [formData, setFormData] = useState({});
    // 表單資料
    const formRef = useRef();
    // 畫面數字總和
    const [totalMoney, setTotalMoney] = useState(0);
    const toatalMoneyRef = useRef();
    // 執行類別代碼黨資料
    const [cpKindDropdown, setCpKindDropdown] = useState([]);
    // 執行類別值
    const [cpKind, setCpKind] = useState('')
    // 重點工作規劃及工作期程下拉資料
    const [checkpointDropDown, setCheckpointDropDown] = useState([]);
    // 重點工作規劃及工作期程下拉值
    const [checkpoint, setCheckpoint] = useState('')
    // 原始重點工作規劃及工作期程
    const orgCheckpoint = useRef('');
    // 紀錄異動資料
    const [isDataChange, setIsDataChange] = useState(false);
    // 下載檔案
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId);
    }
    // 為每個上傳元件建立文件狀態和異動資料追蹤
    const [files1, setFiles1] = useState([]);
    const editedFiles1 = useRef([]);

    const [files2, setFiles2] = useState([]);
    const editedFiles2 = useRef([]);

    const [files3, setFiles3] = useState([]);
    const editedFiles3 = useRef([]);

    // 配置每個上傳服務
    const tempFileUploadService1 = TempFileUploadService({   
        files: files1,
        editedFiles: editedFiles1,
        setFiles: setFiles1,
        setIsDataChange,
        multiple: false,
        downFile
    });
    const tempFileUploadService2 = TempFileUploadService({   
        files: files2,
        editedFiles: editedFiles2,
        setFiles: setFiles2,
        setIsDataChange,
        multiple: false, 
        downFile 
    });
    const tempFileUploadService3 = TempFileUploadService({ 
        files: files3, 
        editedFiles: editedFiles3, 
        setFiles: setFiles3, 
        setIsDataChange, 
        multiple: false, 
        downFile 
    });

    /**
     * 下拉選單和畫面資料
     @returns 
     */
    const loadFromData = async () => {
        SetMaskOnOff(true);
        const data = await getData(projectNo);
        // 年
        setYearOptions(data.yearOptions);
        // 月
        setmonthOptions(data.monthOptions);
        //是否跨年度
        setIsCrossYear(data.isCross);
        // 基金
        setFundOptions(data.fundOptions);
        // 機關
        setOrganOptions(data.organOptions);
        // 執行類別
        setCpKindDropdown(data.cpKindDropdown);
        // 檔案
        if (data.formData.Files && data.formData.Files.length > 0) {
            if (data.formData.Files[0]) {
                setFiles1(fileList([data.formData.Files[0]], 'IDENTITY_FIELD', 'FILE_NAME'));
            }
            if (data.formData.Files[1]) {
                setFiles2(fileList([data.formData.Files[1]], 'IDENTITY_FIELD', 'FILE_NAME'));
            }
            if (data.formData.Files[2]) {
                setFiles3(fileList([data.formData.Files[2]], 'IDENTITY_FIELD', 'FILE_NAME'));
            }
        }
        // 執行類別
        setCpKind(data.formData.CP_KIND);
        // 原始重點工作規劃及工作期程
        orgCheckpoint.current = data.formData.RUNWAY_C;
        // 重點工作規劃及工作期程
        setCheckpoint(data.formData.RUNWAY_C);
        // 檢核點
        checkPointFromSql.current = AddNoColumn(data.formData.CusCheckpointModels);
        // 已核定或未核定
        let isAPPROVEDYN = data.formData.APPROVEDYN == null ? "Y" : data.formData.APPROVEDYN;
        setAPPROVEDYN(isAPPROVEDYN);
        // 計畫跨年度經費
        setGridChange(data.formData.PlanCrossAMTA);
        // 畫面經費計算
        setTotalMoney(data.formData.thisYearMoney);
        // 表單資料
        setFormData(data.formData);
        SetMaskOnOff(false);
    }

    /**
     * 取得執行方式下拉選單
     * @param {*} cpKind 執行類別
     * @returns 
     */
    const loadCheckpointDropDown = async (cpKind) => {
        SetMaskOnOff(true);
        let checkpointData = await loadCheckPoint(cpKind, true);
        if (checkpointData.length > 0) {
            setCheckpointDropDown([...checkpointData]);
        }

        SetMaskOnOff(false);
    }
    
    // 判斷是否為跨年度計畫
    useEffect(() => {
        if (formData.startYear && formData.endYear) {
            setIsCrossYear(formData.startYear !== formData.endYear);
            // 若為跨年度計畫，則將異動資料清空，並將她標為刪除
            if (!isCrossYear) {
                editedGridData.current = gridChange.map(item => ({ ...item, editType: 3 }));
                setGridChange([]);
            }
        }
    }, [formData.startYear, formData.endYear]);

    // 執行類別異動，更新重點工作規劃及工作期程下拉清單資料
    useEffect(() => {
        if (cpKind !== "") {
            loadCheckpointDropDown(cpKind)
        }
    }, [cpKind]);

    // 初始計劃資訊
    useEffect(() => {
        // 取得表單資料
        loadFromData();
        // 退回按鈕權限
        CanSendBack();
    }, []);

    /**
     * 變動主管機關
     * @param {*} PUBLICMONEY 
     * @param {*} CENTERMONEY 
     * @param {*} OTHERMONEY 
     */
    const changeOuId = async (PUBLICMONEY, CENTERMONEY, OTHERMONEY) => {
        // 有其他預算主管機關為自己機關
        if(PUBLICMONEY + CENTERMONEY + OTHERMONEY > 0){
            let OrgId = await GetBasicData('orgId')
            if(formData.OU_ID == OrgId){
                handleDropdownChange('OU_ID', OrgId);
            }else{
                handleDropdownChange('OU_ID', formData.OU_ID);
            }
        }
        // 只剩基金預算，則主管機關跟隨基金機關
        else if(formData.FUNDMONEY > 0 && formData.FUNDNO != null){
            let fund = fundOptions.filter(option => option.value == formData.FUNDNO)
            if(fund.length == 0) return;
            handleDropdownChange('OU_ID', fund[0].param);
        }
    }

    // 機關變動
    useEffect(() => {
        changeOuId(formData.PUBLICMONEY, formData.CENTERMONEY, formData.OTHERMONEY);
    }, [formData.PUBLICMONEY, formData.CENTERMONEY, formData.OTHERMONEY]);

    /**
     * 利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
     * @returns 
     */
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    /**
     * 檢查日期檢查
     * @returns 
     */
    const checkpointVaild = (checkdate, planDate) => {
        let isWork = checkdate.filter(item => item.CHECKITEM_NAME == "開工");
        let ok = true;
        let message = "";
        if (isWork.length == 1) {
            // 檢查日期是否在開工日期之前
            ok = isWork.ESTIMATED_ENDDATE >= planDate;
            message = "計畫結束日期需在開工日期之後";
            return {ok, message};
        }
        // 取得最大日期
        const maxDate = checkdate.reduce((a, b) => {
            const dateA = new Date(a.ESTIMATED_ENDDATE);
            const dateB = new Date(b.ESTIMATED_ENDDATE);
            return dateA > dateB ? dateA : dateB;
        });
        // 檢查最大日期是否在計畫結束日期之前
        ok = maxDate.ESTIMATED_ENDDATE >= planDate;
        message = "檢核點日期需在計畫結束日期之前";
        return {ok, message};
        
    }

     /**
     * 存檔
     * @param {*} data  //表單資料
     * @returns 
     */
    const save = async (data) => {
        SetMaskOnOff(true);
        // 日期要取最後一天所以+1天在SQL-1秒
        const startMonth = data.startMonth == 12 ? 1 : data.startMonth + 1;
        const endMonth = data.endMonth == 12 ? 1 : data.endMonth + 1;
        // 將民國年字串轉換成西元年
        const startYear = startMonth == 1 ? parseInt(data.startYear, 10) + 1912 : parseInt(data.startYear, 10) + 1911;
        const endYear = endMonth == 1 ? parseInt(data.endYear, 10) + 1912 : parseInt(data.endYear, 10) + 1911;
        // 判斷屬於公務預算或基金預算
        const budgetType = (data.PUBLICMONEY + data.CENTERMONEY + data.OTHERMONEY) > 0 ? "1" : "2";
        // 重新組合model可以用的資料
        const setdata = {
            ...data,
            PLANNO: projectNo,
            BUDGETTYPE: budgetType,
            APPROVEDYN: APPROVEDYN,
            RUNWAY_C: data.RUNWAY_C == "" ? null : data.RUNWAY_C,
            OLD_RUNWAY_C: orgCheckpoint.current == "" ? null : orgCheckpoint.current,
            // 日期要取最後一天所以+1天在SQL-1秒
            PLANSTARTDATE: startYear + "-" + startMonth + "-01",
            PLANENDDATE: endYear + "-" + endMonth + "-01",
            PLANTOTMONEY: toatalMoneyRef.current,
        };
        const Files = [
            { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "01" },
            { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "02" },
            { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "03"}
        ];
        Files[0].EditFiles = editedFiles1.current;
        Files[1].EditFiles = editedFiles2.current;
        Files[2].EditFiles = editedFiles3.current;
        const PlanCrossAMTA = editedGridData.current;
        // 檢核點日疑檢查
        if(gridData !== null && gridData.length !== 0){
            let isSave = checkpointVaild(gridData,new Date(setdata.PLANENDDATE) )
            if (isSave.ok) {
                showGlobalMessageBox(isSave.message, () => { SetMaskOnOff(false); });
                return;
            }
        }
        // 將計畫編號傳入檢核點資料
        const checkpointdata = editedCheckPointData.current.map(item => ({ ...item, PROJECT_NO: projectNo }));
        // 去除沒有填寫的空資料
        const CusCheckpointModels = checkpointdata.filter(item => item.CHECKITEM_NAME !== "");
        const savedata = {
            ...setdata,
            PlanCrossAMTA,
            CusCheckpointModels,
            Files
        }
        // 存檔
        let saveResult = await SavePWSSDPLANMAIN(savedata);
        SetMaskOnOff(false);
        // 成功訊息
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => { window.location.reload(); });
        }
    }

     /**
     * 表單取消
     * @returns 
     */
    const cancel = () => {
        loadFromData();
    };

    /**
     * 退回按鈕開起
     */
    const CanSendBack = async () => {
        let allRoles = await GetBasicData('allRoles');
        // 判斷腳色
        if (allRoles.length > 0) {
            let Roles = allRoles.some(item => item.ROLE_ID == 'ORG_RDEC_ROL_PWS' || item.ROLE_ID == 'AREA_RDEC_ROL_PWS');
            setSendBack(Roles);
        }
        // 取機關截止 是否能新增計畫
        let OrgId = await GetBasicData('orgId');
        let result = await getOrgDeadline(OrgId);
        if(result && !formData.IS_SEND_ONTIME)
        {
            setSendBack(false);
        }
    }

    /**
     * 退回
     */
    const Back = async () => {
        let result = await SendBackProject(projectNo);
        // 成功訊息
        if (result.success) {
            showGlobalMessageBox(result.message, () => { window.location.reload(); });
        }
    }

    /**
     * 通用的下拉選單 onChange 處理函數
     * @param {*} name 欄位名稱
     * @param {*} value 值  
     * @returns 
     */
    const handleDropdownChange = (name, value, values) => {
        setFormData(values => ({
            ...values, [name]: value
        }));
        if(name == "CP_KIND"){
            setCpKind(value);
        }
    };

    /**
     * Checkbox改變處理函數
     * @param {*} fieldName 欄位名稱
     * @param {*} isChecked 是否勾選
     * @param {*} setValues 設定欄位值
     * @returns 
     */
    const handleCheckboxChange = (fieldName, isChecked, values) => {
        const updatedValues = {
            ...values,
            [fieldName]: isChecked ? 1 : 0
        };
        setFormData(updatedValues);
    };

    /**
     * RadioButton改變處理函數
     * @param {*} fieldName 
     * @param {*} value 
     * @param {*} values 
     */
    const handleRadioButtonChange = (fieldName, value, values) => {
        const updatedValues = {
            ...values,
            [fieldName]: value
        };
        // 若計畫性質為延續性計畫，則清空是否屬市長政策或指示項目
        if (fieldName == "PLANDATETYPE") {
            if (value == "3" || value == "2") {
                updatedValues.PLANORGINYN = "";
            }
        }
        setFormData(updatedValues);
    }

    /**
     * 數字輸入框做計算
     * @param {*} fieldName 欄位名稱
     * @returns 
     */
    const handleBudgetInputBlur = (fieldName, values, e) => {
        const updatedValue = e.target.value ? e.target.value : 0;
        // 更新後的欄位值
        const updatedValues = {
            ...values,
            [fieldName]: updatedValue
        };
        // 計算總和
        const totalSum = ['PUBLICMONEY', 'FUNDMONEY', 'CENTERMONEY', 'OTHERMONEY']
            // SUM後面這部分是為了處理未填寫的欄位(第二個參數是累加的初始值)
            .reduce((sum, field) => sum + (updatedValues[field] || 0), 0);
        setTotalMoney(totalSum);
        setFormData(updatedValues);
    };

    return (
        <PageContainer
            style={{ overflow: "auto", height: "100%" }}
        >
            <CollapseBoardCard
                button={
                    <>
                        { !projectIsSend &&
                            <>
                            <Button title="存檔" onClick={handleSubmit} style={{ marginLeft: '5px' }} >存檔</Button>
                            <Button title="取消" className="k-button-lighten" onClick={cancel} style={{ marginLeft: '10px' }}>取消</Button>
                            </>
                        }
                        { sendBack &&
                            <Button title="退回" className="k-button-lighten" onClick={Back} style={{ marginLeft: '10px' }}>退回</Button>
                        }
                    </>
                }
                title="計畫執行內容 "
                isFirstArea={true}
            >
                <Formik
                    initialValues={{ ...formData }}
                    validationSchema={validationSchema}
                    onSubmit={(data) => save(data)}
                    enableReinitialize={true}
                    innerRef={formRef}
                >
                    {props => {
                        const {
                            values,
                            errors,
                            handleChange,
                            handleBlur,
                            setFieldValue,
                            setValues
                        } = props;
                        return (
                            <form>
                                <table>
                                    <tr>
                                        <th colSpan="3" className="addRedStar">
                                            計畫名稱
                                        </th>
                                        <td colSpan="3" >
                                            <TextInput
                                                name="PLANNAME"
                                                value={values.PLANNAME}
                                                onChange={handleChange}
                                                style={{ width: "50%" }}
                                                maxLength={100}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" className="addRedStar">
                                            主管機關
                                        </th>
                                        <td colSpan="3">
                                            {/* 區公所要選擇一級機關 */}
                                            {values.ORGOUNAME && values.ORGOUNAME.endsWith("區公所") ? (
                                                <DropDownListWithValue
                                                    name="OU_ID"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={organOptions}
                                                    value={values.OU_ID}
                                                    onChange={(e) => {handleDropdownChange('OU_ID', e.target.value, values);}}
                                                />
                                            ) : (
                                                <DropDownListWithValue
                                                name="OU_ID"
                                                textField="text"
                                                dataItemKey="value"
                                                data={organOptions}
                                                value={values.OU_ID}
                                                onChange={(e) => { handleDropdownChange('OU_ID', e.target.value, values);}}
                                                disabled={true}
                                            />
                                            )}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3">
                                            提報單位(機關)
                                        </th>
                                        <td colSpan="3">
                                            {values.UNITOUNAME}({values.ORGOUNAME})
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" className="addRedStar" style={{ 'borderTop': 'none' }}>
                                            計畫期程
                                        </th>
                                        <td colSpan="3">
                                            <div style={{ display: "flex", alignItems: "center" }}>
                                                <DropDownListWithValue
                                                    name="startYear"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={yearOptions}
                                                    value={values.startYear}
                                                    onChange={(e) => handleDropdownChange('startYear', e.target.value, values)}
                                                    error={errors.startYear}
                                                    style={{ width: "100%" }}
                                                />
                                                <span style={{ margin: "5px" }}>年</span>
                                                <DropDownListWithValue
                                                    name="startMonth"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={monthOptions}
                                                    value={values.startMonth}
                                                    onChange={(e) => handleDropdownChange('startMonth', e.target.value, values)}
                                                    error={errors.startMonth}
                                                    style={{ width: "100%" }}
                                                />
                                                <span style={{ margin: "5px" }}>月 ~ </span>
                                                <DropDownListWithValue
                                                    name="endYear"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={yearOptions}
                                                    value={values.endYear}
                                                    onChange={(e) => handleDropdownChange('endYear', e.target.value, values)}
                                                    error={errors.endYear}
                                                    style={{ width: "100%" }}
                                                />
                                                <span style={{ margin: "5px" }}>年</span>
                                                <DropDownListWithValue
                                                    name="endMonth"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={monthOptions}
                                                    value={values.endMonth}
                                                    onChange={(e) => handleDropdownChange('endMonth', e.target.value, values)}
                                                    error={errors.endMonth}
                                                    style={{ width: "100%" }}
                                                />
                                                <span style={{ margin: "5px" }}>月</span>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" className="addRedStar">
                                            是否規劃辦理性別影響評估
                                        </th>
                                        <td colSpan={3} >
                                            <RadioButton
                                                name="GENDER_ANALYST_YN"
                                                value={1}
                                                label="是(請附性別影響評估表)"
                                                checked={values.GENDER_ANALYST_YN === 1}
                                                onChange={() => handleRadioButtonChange("GENDER_ANALYST_YN", 1, values)}
                                            />
                                            <br/>
                                            <RadioButton
                                                name="GENDER_ANALYST_YN"
                                                value={0}
                                                label="否"
                                                checked={values.GENDER_ANALYST_YN === 0}
                                                onChange={() => handleRadioButtonChange("GENDER_ANALYST_YN", 0, values)}
                                            />
                                            <br/>
                                            <TempFileUploader 
                                                {...tempFileUploadService1.uploaderParam} 
                                                saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                                saveHeaders={{
                                                    'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                    'CacheToken': CacheLoader().GetCache(),
                                                }}
                                                files={files1}
                                                setFiles={setFiles1}
                                                multiple={false}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" className="addRedStar">
                                            計畫性質
                                        </th>
                                        <td colSpan={3}>
                                            <RadioButton
                                                name="PLANDATETYPE"
                                                value="3"
                                                label="延續性計畫(既有例行單一年度計畫)"
                                                checked={values.PLANDATETYPE === "3"}
                                                onChange={() => {handleRadioButtonChange("PLANDATETYPE", "3", values);}}
                                            />
                                            <div></div>
                                            <RadioButton
                                                name="PLANDATETYPE"
                                                value="2"
                                                label="延續性計畫(前一年度已編列預算之跨年度計畫)"
                                                checked={values.PLANDATETYPE === "2"}
                                                onChange={() => {handleRadioButtonChange("PLANDATETYPE", "2", values);}}
                                            />
                                            <div></div>
                                            <RadioButton
                                                name="PLANDATETYPE"
                                                value="1"
                                                label="新興計畫(以前年度未提報之計畫)"
                                                checked={values.PLANDATETYPE === "1"}
                                                onChange={() => handleRadioButtonChange("PLANDATETYPE", "1", values)}
                                            />
                                            {values.PLANDATETYPE == '1' &&
                                                <div style={{ marginLeft: '16px' }}>
                                                    <span>－ 是否屬市長政策或指示項目</span>
                                                    <br/>
                                                    <RadioButton
                                                        name="PLANORGINYN"
                                                        value="Y"
                                                        label="是(請附相關簽陳或會議紀錄)"
                                                        checked={values.PLANORGINYN === "Y"}
                                                        onChange={() => handleRadioButtonChange("PLANORGINYN", "Y", values)}
                                                    />
                                                    <RadioButton
                                                        name="PLANORGINYN"
                                                        value="N"
                                                        label="否"
                                                        checked={values.PLANORGINYN === "N"}
                                                        onChange={() => handleRadioButtonChange("PLANORGINYN", "N", values)}
                                                    />
                                                    <br/>
                                                    <TempFileUploader {...tempFileUploadService2.uploaderParam} 
                                                        saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                                        saveHeaders={{
                                                            'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                            'CacheToken': CacheLoader().GetCache(),
                                                        }}
                                                        files={files2}
                                                        setFiles={setFiles2}
                                                        multiple={false}
                                                    />
                                                </div>
                                            }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" className="addRedStar">
                                            <span>是否須檢附成本效益分析報告</span>
                                        </th>
                                        <td colSpan={3}>
                                            <RadioButton
                                                name="ATTA_COST_YN"
                                                value={1}
                                                label="是(請檢附成本效益分析報告)"
                                                checked={values.ATTA_COST_YN === 1}
                                                onChange={() => handleRadioButtonChange("ATTA_COST_YN", 1, values)}
                                            />
                                            <br/>
                                            <RadioButton
                                                name="ATTA_COST_YN"
                                                value={0}
                                                label="否"
                                                checked={values.ATTA_COST_YN === 0}
                                                onChange={() => handleRadioButtonChange("ATTA_COST_YN", 0, values)}
                                            />
                                            <br/>
                                            <TempFileUploader {...tempFileUploadService3.uploaderParam} 
                                                saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                                saveHeaders={{
                                                    'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                    'CacheToken': CacheLoader().GetCache(),
                                                }}
                                                files={files3}
                                                setFiles={setFiles3}
                                                multiple={false}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                            <th rowspan="8" className="addRedStar" style={{ width: '5%' }} >
                                                <div>{values.PLANYEAR}</div>
                                                <div>需求數</div>
                                                <div>(千元)</div>
                                            </th>
                                        </tr>
                                        <tr >
                                            <th rowspan="3" style={{'borderTop':'none'}}>
                                                本府預算
                                            </th>
                                        </tr>
                                        <tr>
                                            <th style={{ width: '5%' }}>
                                                <div>公務預算</div>
                                                <div>(市預算)</div>
                                            </th>
                                            <td colSpan="3">
                                            <div style={{ display: "flex", alignItems: "center"}}>
                                                <NumericTextInput
                                                    name="PUBLICMONEY"
                                                    min={0}
                                                    inputType={'text'}
                                                    value={values.PUBLICMONEY}
                                                    onChange={handleChange}
                                                    onBlur={(e) => handleBudgetInputBlur("PUBLICMONEY", props.values, e)}
                                                    WithFormik={true}
                                                />
                                                <span>千元</span>
                                            </div>
                                            </td>

                                        </tr>
                                        <tr>
                                            <th>
                                                基金預算
                                            </th>
                                                <td>
                                                <div style={{ display: "flex", alignItems: "center"}}>
                                                <NumericTextInput
                                                    name="FUNDMONEY"
                                                    min={0}
                                                    format={'n0'}
                                                    value={values.FUNDMONEY}
                                                    onChange={handleChange}
                                                    onBlur={(e) => handleBudgetInputBlur("FUNDMONEY", props.values, e)}
                                                    WithFormik={true}
                                                />
                                                <span>千元</span>
                                            </div>
                                                </td>
                                                
                                            <th >
                                                基金名稱
                                            </th>
                                                <td >
                                                <DropDownListWithValue
                                                name="FUNDNO"
                                                textField="text"
                                                dataItemKey="value"
                                                data={fundOptions}
                                                value={values.FUNDNO}
                                                onChange={(e) => { 
                                                    handleDropdownChange('FUNDNO', e.target.value, values); 
                                                    // 計畫屬於基金預算時，主管機關跟隨基金機關
                                                    if (values.PUBLICMONEY + values.CENTERMONEY + values.OTHERMONEY < 1) {
                                                        handleDropdownChange('OU_ID', e.value.param, values);
                                                    }
                                                }}
                                                />
                                                </td>
                                        </tr>
                                        <tr >
                                            <th rowspan="3" style={{'borderTop':'none'}}>
                                                中央預算
                                            </th>
                                        </tr>
                                        <tr>
                                            <th rowSpan={2}>
                                            </th>
                                            <td colSpan="3">
                                            <RadioButton
                                                name="APPROVEDYN"
                                                value="Y"
                                                label="已核定"
                                                checked={APPROVEDYN === "Y"}
                                                onChange={() => {
                                                    setAPPROVEDYN("Y");
                                                    handleRadioButtonChange("CENTERMONEY", 0, values); 
                                                    handleRadioButtonChange("APPROVEDNUMBER", "", values); 
                                                    handleRadioButtonChange("APPLYAPPROVEDYN", "", values);
                                                }}
                                            />
                                            <RadioButton
                                                name="APPROVEDYN"
                                                value="N"
                                                label="未核定"
                                                checked={APPROVEDYN === "N"}
                                                onChange={() => {
                                                    setAPPROVEDYN("N");
                                                    handleRadioButtonChange("CENTERMONEY", 0, values); 
                                                    handleRadioButtonChange("APPROVEDNUMBER", "", values); 
                                                }}
                                            />
                                            </td>
                                        </tr>
                                        {APPROVEDYN === "Y" && (
                                            <tr>
                                                <td>
                                                    <div style={{ display: "flex", alignItems: "center", width:"100%" }}>
                                                        <NumericTextInput
                                                            name="CENTERMONEY"
                                                            min={0}
                                                            format={'n0'}
                                                            value={values.CENTERMONEY}
                                                            onChange={handleChange}
                                                            onBlur={(e) => handleBudgetInputBlur("CENTERMONEY", props.values, e)}
                                                            WithFormik={true}
                                                        />
                                                        <span>千元</span>
                                                    </div>
                                                </td>
                                                <th >
                                                    核定文號
                                                </th>
                                                <td >
                                                    <TextInput
                                                        name="APPROVEDNUMBER"
                                                        value={values.APPROVEDNUMBER}
                                                        onChange={handleChange}
                                                        error={errors.APPROVEDNUMBER}
                                                        maxLength={50}
                                                    />
                                                </td>
                                            </tr>
                                        )}
                                        {APPROVEDYN === "N" && (
                                            <>
                                            <td colSpan={4}>
                                            <div style={{ display: "flex", alignItems: "center" }}>
                                                <NumericTextInput
                                                    name="CENTERMONEY"
                                                    min={0}
                                                    format={'n0'}
                                                    value={values.CENTERMONEY}
                                                    onChange={handleChange}
                                                    onBlur={(e) => handleBudgetInputBlur("CENTERMONEY", props.values, e)}
                                                    WithFormik={true}
                                                />
                                                <span style={{ margin: "5px" }}>千元</span>
                                                <RadioButton
                                                    name="APPLYAPPROVEDYN"
                                                    value="Y"
                                                    label="已報請核定"
                                                    checked={values.APPLYAPPROVEDYN === "Y"}
                                                    onChange={() => {
                                                        handleRadioButtonChange("APPLYAPPROVEDYN", "Y", values);
                                                    }}
                                                />
                                                <RadioButton
                                                    name="APPLYAPPROVEDYN"
                                                    value="N"
                                                    label="未報請核定"
                                                    checked={values.APPLYAPPROVEDYN === "N"}
                                                    onChange={() => {
                                                        handleRadioButtonChange("APPLYAPPROVEDYN", "N", values);
                                                    }}
                                                />
                                                </div>
                                            </td>
                                            </>
                                        )}
                                        <tr>
                                            <th colSpan={2}>
                                            其他
                                            </th>
                                                <td>
                                                <div style={{ display: "flex", alignItems: "center"}}>
                                                <NumericTextInput
                                                    name="OTHERMONEY"
                                                    min={0}
                                                    inputType={'text'}
                                                    value={values.OTHERMONEY}
                                                    onChange={handleChange}
                                                    onBlur={(e) => handleBudgetInputBlur("OTHERMONEY", props.values, e)}
                                                    WithFormik={true}
                                                />
                                                <span>千元</span>
                                            </div>
                                                </td>
                                            <th >
                                            來源說明
                                            </th>
                                                <td >
                                                <TextInput
                                                    name="OTHERDESC"
                                                    value={values.OTHERDESC}
                                                    onChange={handleChange}
                                                    style={{ width: "100%" }}
                                                    maxLength={100}
                                                />
                                                </td>
                                        </tr>
                                    <tr>
                                        <th colSpan="3" className="addRedStar">
                                            計畫總經費(千元):
                                        </th>
                                        <td colSpan="3" >
                                            <div className='fn-buttons'>
                                                <Button 
                                                    type='button' 
                                                    title="跨年度經費設定" 
                                                    disabled={ !isCrossYear }
                                                    onClick={() => {
                                                        setWindowVisible(true);
                                                    }}>
                                                    跨年度經費設定
                                                </Button>
                                            </div>
                                            <ProjectFundGrid
                                                totalMoney={totalMoney}
                                                toatalMoneyRef={toatalMoneyRef}
                                                PLANYEAR={values.PLANYEAR}
                                                gridChange={gridChange}
                                                isCrossYear={isCrossYear}
                                            />
                                            {windowVisible &&
                                                <AddNewPlanWindow
                                                    closeWindow={() => { setWindowVisible(false); }}
                                                    yearOptions={yearOptions}
                                                    PLANNO={projectNo}
                                                    gridChange={gridChange}
                                                    setGridChange={setGridChange}
                                                    editedGridData={editedGridData}
                                                />
                                            }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3">
                                            計畫內容(可複選)
                                        </th>
                                        <td colSpan="3">
                                            <Checkbox
                                                name="PLANCONTENTENGINE"
                                                label="涉及建築裝修工程"
                                                checked={values.PLANCONTENTENGINE === 1}
                                                onChange={(e) => handleCheckboxChange('PLANCONTENTENGINE', e.value, values)}
                                            />
                                            <div></div>
                                            <Checkbox
                                                name="PLANCONTENTLAND"
                                                label="涉及用地取得 "
                                                checked={values.PLANCONTENTLAND === 1}
                                                onChange={(e) => handleCheckboxChange('PLANCONTENTLAND', e.value, values)}
                                            />
                                            <div></div>
                                            <Checkbox
                                                name="PLANCONTENTINFO"
                                                label="涉及資訊費用(含購置資訊設備或軟體、開發應用系統、既有系統擴充或調整、既有軟體維護費、電路費等) "
                                                checked={values.PLANCONTENTINFO === 1}
                                                onChange={(e) => handleCheckboxChange('PLANCONTENTINFO', e.value, values)}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" >
                                            <CommonTooltip 
                                                title={(
                                                    <span>
                                                        說明計畫之
                                                        <br/>&nbsp;&nbsp;必要性、亮點及效益
                                                    </span>
                                                )}
                                                content="請儘量量化呈現"
                                            />
                                        </th>
                                        <td colSpan="3">
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='EXPLAINNECESSITY'
                                                value={values.EXPLAINNECESSITY}
                                                onChange={handleChange}
                                                onBlur={(e) => {
                                                    setFormData({ ...values, EXPLAINNECESSITY: e.target.value })
                                                }}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3">
                                            <CommonTooltip 
                                                title={(
                                                <span>
                                                    說明計畫之
                                                    <br/>&nbsp;&nbsp;基本資料及執行方式
                                                </span>
                                                )}
                                                content="工程類計畫應說明地點、規模大小、位屬之都市計畫、土地使用區分；
                                                         建築工程應另說明建蔽率、容積率、總樓地板面積、地上、地下樓層數及用途等" 
                                            />
                                        </th>
                                        <td colSpan="3">
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='EXPLANBASICINFO'
                                                value={values.EXPLANBASICINFO}
                                                onChange={handleChange}
                                                onBlur={(e) => {
                                                    setFormData({ ...values, EXPLANBASICINFO: e.target.value })
                                                }}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" >
                                            <CommonTooltip 
                                                title={(
                                                    <span>
                                                        既有例行計畫
                                                        <br/>&nbsp;&nbsp;應說明業務之精進作法
                                                    </span>
                                                    )}
                                                content="若提報年度需求經費較前一年度增加，需說明經費成長幅度及增加原因"
                                            />
                                        </th>
                                        <td colSpan="3" >
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='EXPLANIMPROVE'
                                                value={values.EXPLANIMPROVE}
                                                onChange={handleChange}
                                                onBlur={(e) => {
                                                    setFormData({ ...values, EXPLANIMPROVE: e.target.value })
                                                }}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" >
                                            <CommonTooltip 
                                                title={(
                                                    <span>
                                                        說明計畫完成後
                                                        <br/>&nbsp;&nbsp;是否產生後續維護費用
                                                    </span>
                                                )}
                                                content="如館舍維護費、系統維護費等"
                                            />
                                        </th>
                                        <td colSpan="3">
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='EXPLANFUND'
                                                value={values.EXPLANFUND}
                                                onChange={handleChange}
                                                onBlur={(e) => {
                                                    setFormData({ ...values, EXPLANFUND: e.target.value })
                                                }}
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" >
                                            <CommonTooltip 
                                                title="重點工作規劃及工作期程" 
                                                content="1.請先選擇該計畫之「執行方式」。
                                                         2.各檢核點預定完成日期需為遞增。
                                                         3.最後檢核點預定完成日期不得超過「計畫期程」－截止年月。
                                                        "
                                            />
                                        </th>
                                        <td colSpan="3" style={{'display': 'flex', 'grid-gap': '10px'}}>
                                            <DropDownListWithValue
                                                name="CP_KIND"
                                                textField={"SET_VALUE"}
                                                dataItemKey={"SET_TYPE"}
                                                data={cpKindDropdown ? cpKindDropdown : '1'}
                                                value={cpKind}
                                                onChange={(e) => {
                                                    setCheckpoint('');
                                                    handleDropdownChange('RUNWAY_C', '', values);
                                                    handleDropdownChange('CP_KIND', e.target.value, values);
                                                }}
                                            />
                                            <DropDownListWithValue
                                                name="RUNWAY_C"
                                                textField={"CHECKPOINT_CLASS"}
                                                dataItemKey={"CHECKPOINT_CLASS_ID"}
                                                data={checkpointDropDown}
                                                value={checkpoint ? checkpoint : ''}
                                                onChange={(e) => {
                                                    setCheckpoint(e.target.value);
                                                    handleDropdownChange('RUNWAY_C', e.target.value, values);
                                                }}
                                                itemRender={(li, item) => {
                                                    if (item.dataItem.DEL_FLG) {
                                                        return <></>
                                                    }
                                                    else {
                                                        return React.cloneElement(li, li.props, <span>{li.props.children}</span>)
                                                    }
                                                }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3">

                                        </th>
                                        <td colSpan="3" >
                                            <CheckPointGrid
                                                checkpoint={checkpoint}
                                                editedCheckPointData={editedCheckPointData}
                                                orgCheckpoint={orgCheckpoint}
                                                checkPointFromSql={checkPointFromSql}
                                                setGridData={setGridData}
                                                gridData={gridData}
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
export default AddNewPlanMain;


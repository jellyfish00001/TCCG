import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import TextInput from '../../../Components/Input/TextInput';
import { Checkbox, RadioButton } from '@progress/kendo-react-inputs';
import NumericTextInput from '../../../Components/Input/NumericTextInput';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import CAddNewPlanGrid from './CAddNewPlanGrid';
import CAddNewPlanWindow from './CAddNewPlanWindow';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import { SavePlanBasicB, validationSchema, initFiles, getData } from "./CAddNewPlanService";
import { downProjectAttachment } from "../../../Basic/CommonService";
import { fileList, TempFileUploadService } from '../../../Components/Upload/TempFileUploadService';
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import { GetBasicData } from "../../../Basic/BasicData";
import CacheLoader from '../../../Basic/CacheLoader';
import { SendBackProject } from "../GpReviewProject/GpReviewProjectService";
import { getOrgDeadline } from "../ProjectList/ProjectListService";

const CAddNewPlanMain = (props) => {    
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
    // 表單傳入資料追蹤
    const [gridChange, setGridChange] = useState([]);
    //表單資料
    const formRef = useRef();
    //表單異動資料
    const [windowVisible, setWindowVisible] = useState(false);
    // 跨年度經費資料追蹤
    const editedGridData = useRef([]);
    // 是否跨年度計畫
    const [isCrossYear, setIsCrossYear] = useState(false);
    //年度下拉選單
    const [yearOptions, setYearOptions] = useState([]);
    //月份下拉選單
    const [monthOptions, setmonthOptions] = useState([]);
    // 基金下拉選單
    const [fundOptions, setFundOptions] = useState([]);
    // 機關下拉選單
    const [organOptions, setOrganOptions] = useState([]);
    //核定狀態
    const [APPROVEDYN, setAPPROVEDYN] = useState('Y');
    //表單資料
    const [formData, setFormData] = useState({});
    // 畫面數字總和
    const [totalMoney, setTotalMoney] = useState(0);
    const toatalMoneyRef = useRef();
    // 下載檔案
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId);
    }
    // 檔案上傳元件使用
    const [isDataChange, setIsDataChange] = useState(false);
    // 為每個上傳元件建立文件狀態和異動資料追蹤
    const [files1, setFiles1] = useState([]);
    const editedFiles1 = useRef([]);

    const [files2, setFiles2] = useState([]);
    const editedFiles2 = useRef([]);

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

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    // 下拉選單資料
    const loadData = async () => {
        SetMaskOnOff(true);
        let data = await getData(projectNo);
        // 年度下拉選單
        setYearOptions(data.Year);
        // 月份下拉選單
        setmonthOptions(data.Month);
        // 取得基金下拉選單
        setFundOptions(data.FundList);
        // 取得機關下拉選單
        setOrganOptions(data.OrganList);
        // 檔案資料
        if (data.FromData.Files && data.FromData.Files.length > 0) {
            if (data.FromData.Files[0]) {
                setFiles1(fileList([data.FromData.Files[0]], 'IDENTITY_FIELD', 'FILE_NAME'));
            }
            if (data.FromData.Files[1]) {
                setFiles2(fileList([data.FromData.Files[1]], 'IDENTITY_FIELD', 'FILE_NAME'));
            }
        }
        // 已核定或未核定
        let isAPPROVEDYN = data.FromData.APPROVEDYN == null ? "Y" : data.FromData.APPROVEDYN;
        setAPPROVEDYN(isAPPROVEDYN);
        // 計畫跨年度經費
        setGridChange(data.FromData.PlanCrossAMTB);
        // 畫面經費計算
        setTotalMoney(data.FromData.thisYearMoney);
        // 是否為跨年
        setIsCrossYear(data.FromData.PLANDATETYPE == "2");
        // 表單資料
        setFormData(data.FromData);
        SetMaskOnOff(false);
    }

    // 改變為非跨年度計畫
    useEffect(() => {
        if (isCrossYear) {
            editedGridData.current = gridChange.map(item => ({ ...item, editType: 3 }));
            setGridChange([]);
        }
    }, [isCrossYear]);

    // 取得下拉選單
    useEffect(() => {
        // 取得表單資料
        loadData();
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
                handleDropdownChange('OU_ID', OrgId, formData);
            }else{
                handleDropdownChange('OU_ID', formData.OU_ID, formData);
            }
        }
        // 只剩基金預算，則主管機關跟隨基金機關
        else if(formData.FUNDMONEY > 0 && formData.FUNDNO != null){
            let fund = fundOptions.filter(option => option.value == formData.FUNDNO)
            if(fund.length == 0) return;
            handleDropdownChange('OU_ID', fund[0].param, formData);
        }
    }

    // 機關變動
    useEffect(() => {
        changeOuId(formData.PUBLICMONEY, formData.CENTERMONEY, formData.OTHERMONEY);
    }, [formData.PUBLICMONEY, formData.CENTERMONEY, formData.OTHERMONEY]);
    
    // 存檔
    const save = async (data) => {
        SetMaskOnOff(true);
        // 日期要取最後一天所以+1天在SQL-1秒
        const startMonth = data.startMonth == 12 ? 1 : data.startMonth + 1;
        const endMonth = data.endMonth == 12 ? 1 : data.endMonth + 1;
        const bidMonth = data.bidMonth == 12 ? 1 : data.bidMonth + 1;
        const cenMonth = data.cenMonth == 12 ? 1 : data.cenMonth + 1;
        const lastMonth = data.lastMonth == 12 ? 1 : data.lastMonth + 1;
        const finishMonth = data.finishMonth == 12 ? 1 : data.finishMonth + 1;
        // 將民國年字串轉換成西元年
        const startYear = startMonth == 1 ? parseInt(data.startYear, 10) + 1912 : parseInt(data.startYear, 10) + 1911;
        const endYear = endMonth == 1 ? parseInt(data.endYear, 10) + 1912 : parseInt(data.endYear, 10) + 1911;
        const bidYear = bidMonth == 1 ? parseInt(data.bidYear, 10) + 1912 : parseInt(data.bidYear, 10) + 1911;
        const cenYear = cenMonth == 1 ? parseInt(data.cenYear, 10) + 1912 : parseInt(data.cenYear, 10) + 1911;
        const lastYear = lastMonth == 1 ? parseInt(data.lastYear, 10) + 1912 : parseInt(data.lastYear, 10) + 1911;
        const finishYear = finishMonth == 1 ? parseInt(data.finishYear, 10) + 1912 : parseInt(data.finishYear, 10) + 1911;
        // 判斷屬於公務預算或基金預算
        const budgetType = (data.PUBLICMONEY + data.CENTERMONEY + data.OTHERMONEY) > 0 ? "1" : "2";
        // 檔案資料
        const Files = [
            { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "04" },
            { ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "05" }
        ];
        Files[0].EditFiles = editedFiles1.current;
        Files[1].EditFiles = editedFiles2.current;
        // 重新組合資料
        const setdata = {
            ...data, 
            PLANNO: data.PLANNO,
            // 日期要取最後一天所以+1天在SQL-1秒
            PLANSTARTDATE: startYear + "-" + startMonth + "-01",
            PLANENDDATE: endYear + "-" + endMonth + "-01",
            AWARDYM: bidYear + "-" + bidMonth + "-01",
            MIDREPORTYM: cenYear + "-" + cenMonth + "-01",
            FINAKREPORTYM: lastYear + "-" + lastMonth + "-01",
            CLOSEYM: finishYear + "-" + finishMonth + "-01",
            APPROVEDYN: APPROVEDYN,
            BUDGETTYPE: budgetType,
            PLANTOTMONEY: toatalMoneyRef.current,
        };
        const PlanCrossAMTB = editedGridData.current;
        const savedata = {
            ...setdata,
            PlanCrossAMTB,
            Files,
        };
        // 存檔
        let saveResult = await SavePlanBasicB(savedata);
        SetMaskOnOff(false);
        // 成功訊息
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                window.location.reload();
            });
        }
    }

     /**
     * 表單取消
     * @param {*}  
     * @returns 
     */
     const cancel = () => {
        loadData();
    };

    /**
     * 退回按鈕開起
     */
    const CanSendBack = async () => {
        let allRoles = await GetBasicData('allRoles');
        // 
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
     * Checkbox改變處理函數
     * @param {*} fieldName 欄位名稱
     * @param {*} isChecked 是否勾選
     * @param {*} setValues 設定欄位值
     * @returns 
     */
       const handleCheckboxChange = (fieldName, isChecked, values) => {
        const updatedValues = {
            ...values,
            [fieldName]: isChecked ? "Y" : "N"
        };
        setFormData(updatedValues);
    };

    // 通用的下拉選單 onChange 處理函數
    const handleDropdownChange = (name, value, values) => {
        setFormData({
            ...values,[name]: value
        });
    };

    /**
     * 數字輸入框做計算
     * @param {*} fieldName 欄位名稱
     * @param {*} values 表單資料
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
                    {!projectIsSend &&
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
            title="計畫執行內容"
            isFirstArea={true}
            >     
                <Formik
                    initialValues={{...formData}}
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
                            setFieldValue,
                            setValues
                        } = props;
                        return (
                            <form>
                                <table>
                                    <tr>
                                        <th colSpan="3">
                                            計畫編號
                                        </th>
                                        <td colSpan="3">
                                            {values.PLANNO}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" className="addRedStar">
                                            委託機關
                                        </th>
                                        <td colSpan="3">
                                            <DropDownListWithValue
                                                name="OU_ID"
                                                textField="text"
                                                dataItemKey="value"
                                                data={organOptions}
                                                value={values.OU_ID}
                                                onChange={(e) => handleDropdownChange('OU_ID', e.target.value, values)}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" className="addRedStar">
                                            計畫性值
                                        </th>
                                        <td colSpan="3">
                                            <soan>是否符合政府採購法第7條第3項所稱研究發展類之勞務委託案?</soan>
                                            <div>
                                            <RadioButton
                                                name="LABORYN"
                                                value="Y"
                                                label="是"
                                                checked={values.LABORYN === "Y"}
                                                onChange={() => setFieldValue('LABORYN', 'Y')}
                                            />
                                            <RadioButton
                                                name="LABORYN"
                                                value="N"
                                                label="否, 非本府委託研究計畫認定範圍"
                                                checked={values.LABORYN === "N"}
                                                onChange={() => setFieldValue("LABORYN", "N")}
                                            />
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" className="addRedStar">
                                            計畫名稱
                                        </th>
                                        <td colSpan="3">
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
                                        <th rowspan="6" className="addRedStar">
                                            計畫期程
                                        </th>
                                    </tr>
                                    <tr>
                                        <th colSpan={2} style={{'borderTop':'none'}}>
                                            計畫期間
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
                                        <th colSpan={2}>
                                            決標
                                        </th>
                                        <td colSpan="3">
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="bidYear"
                                                textField="text"
                                                dataItemKey="value"
                                                data={yearOptions}
                                                value={values.bidYear}
                                                onChange={(e) => handleDropdownChange('bidYear', e.target.value, values)}
                                                style={{ width: "100%" }}
                                            />
                                            <span style={{ margin: "5px" }}>年</span>
                                            <DropDownListWithValue
                                                name="bidMonth"
                                                textField="text"
                                                dataItemKey="value"
                                                data={monthOptions}
                                                value={values.bidMonth}
                                                onChange={(e) => handleDropdownChange('bidMonth', e.target.value, values)}
                                                style={{ width: "100%" }}
                                            />
                                            <span style={{ margin: "5px" }}>月</span>
                                        </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan={2}>
                                            期中報告
                                        </th>
                                        <td colSpan="3">
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="cenYear"
                                                textField="text"
                                                dataItemKey="value"
                                                data={yearOptions}
                                                value={values.cenYear}
                                                onChange={(e) => handleDropdownChange('cenYear', e.target.value, values)}
                                                style={{ width: "100%" }}
                                            />
                                            <span style={{ margin: "5px" }}>年</span>
                                            <DropDownListWithValue
                                                name="cenMonth"
                                                textField="text"
                                                dataItemKey="value"
                                                data={monthOptions}
                                                value={values.cenMonth}
                                                onChange={(e) => handleDropdownChange('cenMonth', e.target.value, values)}
                                                style={{ width: "100%" }}
                                            />
                                            <span style={{ margin: "5px" }}>月</span>
                                        </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan={2}>
                                            期末報告
                                        </th>
                                        <td colSpan="3">
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="lastYear"
                                                textField="text"
                                                dataItemKey="value"
                                                data={yearOptions}
                                                value={values.lastYear}
                                                onChange={(e) => handleDropdownChange('lastYear', e.target.value, values)}
                                                style={{ width: "100%" }}
                                            />
                                            <span style={{ margin: "5px" }}>年</span>
                                            <DropDownListWithValue
                                                name="lastMonth"
                                                textField="text"
                                                dataItemKey="value"
                                                data={monthOptions}
                                                value={values.lastMonth}
                                                onChange={(e) => handleDropdownChange('lastMonth', e.target.value, values)}
                                                style={{ width: "100%" }}
                                            />
                                            <span style={{ margin: "5px" }}>月</span>
                                        </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan={2}>
                                            結案
                                        </th>
                                        <td colSpan="3">
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="finishYear"
                                                textField="text"
                                                dataItemKey="value"
                                                data={yearOptions}
                                                value={values.finishYear}
                                                onChange={(e) => handleDropdownChange('finishYear', e.target.value, values)}
                                                style={{ width: "100%" }}
                                            />
                                            <span style={{ margin: "5px" }}>年</span>
                                            <DropDownListWithValue
                                                name="finishMonth"
                                                textField="text"
                                                dataItemKey="value"
                                                data={monthOptions}
                                                value={values.finishMonth}
                                                onChange={(e) => handleDropdownChange('finishMonth', e.target.value, values)}
                                                style={{ width: "100%" }}
                                            />
                                            <span style={{ margin: "5px" }}>月</span>
                                        </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3">
                                            計畫性值
                                        </th>
                                        <td colSpan="3">
                                        <div>
                                        <RadioButton
                                            name="PLANDATETYPE"
                                            value="1"
                                            label="單一年度計畫"
                                            checked={values.PLANDATETYPE === "1"}
                                            onChange={() => {setIsCrossYear(false);setFieldValue("PLANDATETYPE", "1"); }}
                                        />
                                        <RadioButton
                                            name="PLANDATETYPE"
                                            value="2"
                                            label="跨年度計畫"
                                            checked={values.PLANDATETYPE === "2"}
                                            onChange={() => {setIsCrossYear(true);setFieldValue("PLANDATETYPE", "2"); }}
                                        />
                                        </div>
                                        <Checkbox
                                            name="PLANORGINYN"
                                            value="Y"
                                            label="市長政策或指示項目(請檢附相關簽陳或會議紀錄)"
                                            checked={values.PLANORGINYN === "Y"}
                                            onChange={(e) => {
                                                handleCheckboxChange('PLANORGINYN', e.value, values);
                                            }}
                                        />
                                        </td>
                                    </tr>                                
                                    <tr>
                                        <th colSpan="3">
                                        <CommonTooltip withoutRedStar={true} title={"附加檔案"} content={"單一檔案 , 容量上限10MB"} />
                                        </th>
                                        <td colSpan={3} >
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
                                        <th rowspan="9" className="addRedStar" style={{ width: '5%' }} >
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
                                            onChange={(e) => { // 計畫屬於基金預算時，主管機關跟隨基金機關
                                                if (values.PUBLICMONEY + values.CENTERMONEY + values.OTHERMONEY < 1) {
                                                    setFormData({...values, OU_ID: e.value.param, FUNDNO: e.target.value });
                                                };
                                                setValues({ ...values, FUNDNO: e.target.value });
                                            }}
                                            />
                                            </td>
                                    </tr>
                                    <tr >
                                        <th rowspan="4" style={{'borderTop':'none'}}>
                                            中央預算
                                        </th>
                                    </tr>
                                    <tr>

                                            <th>
                                        <CommonTooltip withoutRedStar={true} title={"附加檔案"} content={"單一檔案 , 容量上限10MB"} />
                                        </th>
                                        <td colSpan={3} >
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
                                        </td>
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
                                                // 清空已報請核定
                                                setFieldValue("APPLYAPPROVEDYN", "");
                                            }}
                                        />
                                        <RadioButton
                                            name="APPROVEDYN"
                                            value="N"
                                            label="未核定"
                                            checked={APPROVEDYN === "N"}
                                            onChange={() => {
                                                setAPPROVEDYN("N");
                                                // 清空核定文號
                                                handleChange({ target: { name: 'APPROVEDNUMBER', value: '' } });
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
                                                    setFieldValue("APPLYAPPROVEDYN", "Y");
                                                }}
                                            />
                                            <RadioButton
                                                name="APPLYAPPROVEDYN"
                                                value="N"
                                                label="未報請核定"
                                                checked={values.APPLYAPPROVEDYN === "N"}
                                                onChange={() => {
                                                    setFieldValue("APPLYAPPROVEDYN", "N");
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
                                                {values.PLANDATETYPE === "2" && 
                                                    <Button type='button' title="跨年度經費設定" 
                                                        onClick={() => {
                                                            setWindowVisible(true);
                                                        }}>
                                                        跨年度經費設定
                                                    </Button>
                                                }
                                            </div>
                                            <CAddNewPlanGrid
                                                totalMoney={totalMoney}
                                                toatalMoneyRef={toatalMoneyRef}
                                                PLANYEAR={values.PLANYEAR}
                                                gridChange={gridChange}
                                                isCrossYear={isCrossYear}
                                            />
                                            {windowVisible && 
                                                <CAddNewPlanWindow
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
                                        <th colSpan="3" className="addRedStar">
                                            研究原因及目的
                                        </th>
                                        <td colSpan="3">
                                            <TextInput
                                                name="PLANCAUSE"
                                                value={values.PLANCAUSE}
                                                onChange={handleChange}
                                                style={{ width: "100%" }}
                                                maxLength={2000}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th colSpan="3" className="addRedStar">
                                            預期研究成果
                                        </th>
                                        <td colSpan="3">
                                            <TextInput
                                                name="PLANEXPECTED"
                                                value={values.PLANEXPECTED}
                                                onChange={handleChange}
                                                style={{ width: "100%" }}
                                                maxLength={2000}
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
        export default CAddNewPlanMain;


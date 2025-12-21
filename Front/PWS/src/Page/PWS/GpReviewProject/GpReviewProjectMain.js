import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Formik } from 'formik';
import TextAreaInput from '../../../Components/Input/TextAreaInput';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import { closeAndbackToParentWindow, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { SavePlanReview, SendBackProject, getLoadData } from "./GpReviewProjectService";
import { showGlobalConfirmBox, showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { GpReviewProjectWindow } from "./GpReviewProjectWindow";
import { setPrintInfo, openProjectPrint } from '../../../Basic/CommonService';

const CGpReviewProjectMain = (props) => {
    // 外部傳入
    const { 
        location: { 
            state: {
                projectNo,
            }
        }
     } = props;
    
    const [initialFormData, setInitialFormData] = useState({});
    // 表單異動資料
    const [windowVisible, setWindowVisible] = useState(false);
    // 關聯性下拉選單
    const [relevance, setRelevance] = useState([]) 
    // 執行績效下拉選單
    const [exePerformance, setExePerformance] = useState([]) 
    // 預算審查下拉選單
    const [bougetReview, setBougetReview] = useState([]) 
    // 顯示送審按鈕
    const [showSendButton, setShowSendButton] = useState(false);
    // 可否存檔
    const [canSave, setCanSave] = useState(false);
    // 表單資料
    const [formData, setFormData] = useState({});
    const formRef = useRef(null);
    // 取關聯重大編號,名稱
    const [ipcPlanNo, setIpcPlanNo] = useState("");
    const [ipcPlanName, setIpcPlanName] = useState("");
    // 區分送出和審核
    const [actionType, setActionType] = useState(null);
    // 小組意見
    const [adViewDesc, setAdViewDesc] = useState("");
    const adViewDescRef = useRef(null);
    // 是否需要顯示關聯性視窗
    const [showDropDone, setShowDropDone] = useState(false);
    // 計畫總金額
    const totalRef = useRef(null);

    // 按鈕點擊處理函數
    const handleSaveClick = () => {
        setActionType("save");
        handleSubmit();
    };

    const handleReviewClick = () => {
        setActionType("review");
        handleSubmit();
    };
    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    };

    /**
     * 載入表單資料
     * @returns
     */
    const loadFormData = async () => {
        SetMaskOnOff( true );
        let data = await getLoadData(projectNo);
        // 是否需要顯示關聯性視窗
        setShowDropDone(data.fromData.PLANDATETYPE == "1" ? true : false);
        // 是否可以送審
        setShowSendButton(data.fromData.IS_SEND);
        // 可否存檔
        setCanSave(data.fromData.AUDIT_STATUS);
        // 評核類別
        setBougetReview(data.bougetReview);
        // 關聯性
        setRelevance(data.relevance);
        // 執行績效
        setExePerformance(data.exePerformance);
        // 計畫總金額
        totalRef.current = data.fromData.PLANTOTMONEY;
        // 關聯重大No
        setIpcPlanNo(data.fromData.IPC_PROJECTNO);
        // 小組意見
        setAdViewDesc(data.fromData.ADVIEWDESC);
        // 小組意見預設表
        adViewDescRef.current = data.AuditTemplateModels;
        // 表單資料
        setFormData(data.fromData);
        // 暫存初始值
        setInitialFormData(JSON.parse(JSON.stringify(data.fromData)));
        SetMaskOnOff( false );
    }

    // 載入表單資料
    useEffect(() => {
        loadFormData();
    }, []);

    /**
     * 發送稽催填報
     * @param {*} data 
     */
    const saveChanges = async (data) => {
        SetMaskOnOff( true );
        let saveData = {
            ...data,
            PLANNO: projectNo,
            PUBLIC1: data.gridData[0].PublicBudget,
            FUND1: data.gridData[0].FundBudget,
            PUBLIC2: data.gridData[1].PublicBudget,
            FUND2: data.gridData[1].FundBudget,
            ADVIEWDESC: adViewDesc,
            IPC_PROJECTNO: ipcPlanNo,
        };
        // 審核
        if (actionType === "review") {
            // 計算grid總金額
            const totalAmount = data.gridData.reduce((sum, row) => sum + row.PublicBudget + row.FundBudget, 0);
            // 判斷金額總和是否等於計劃的總金額
            if (totalAmount !== totalRef.current) {
                SetMaskOnOff(false);
                showGlobalMessageBox("經費需求與計畫總金額不符");
                return;
            }
            // 審核要傳送狀態更改
            saveData = {
                ...data,
                AUDIT_STATUS: 1 
            };
        } 
        // 存檔
        let saveResult = await SavePlanReview(saveData);
        SetMaskOnOff(false);
        // 成功訊息
        if (saveResult.success) {
            if (actionType === "review") {
                showGlobalMessageBox("審核成功");
            }else{
                showGlobalMessageBox(saveResult.message);
            }
        }
        SetMaskOnOff( false );
    }

    /**
     * 下拉選單篩選小組意見
     * @param {*} formValues
     */
    const filterAdViewDesc = (formValues) => {
        // 新興計畫'1'才需要關聯性和執行績效
        if (formValues.PLANDATETYPE == '1') {
            // 確保所有下拉框都有值
            if (formValues.SUB_PLANDATETYPE && formValues.PLAN_REF && formValues.EXE_PERFORMANCE) {
                // 篩選小組意見
                const filteredResults = adViewDescRef.current.filter(item =>
                    item.SUB_PLANDATETYPE == formValues.SUB_PLANDATETYPE &&
                    item.PLAN_REF == formValues.PLAN_REF &&
                    item.EXE_PERFORMANCE == formValues.EXE_PERFORMANCE);

                if (filteredResults.length == 1) {
                    showGlobalConfirmBox('是否增加小組意見?',() => setAdViewDesc(filteredResults[0].TEMPLATE));
                } else {
                    setAdViewDesc(""); 
                }
            }
        }
        else{
            if(formValues.SUB_PLANDATETYPE){
                const filteredResults = adViewDescRef.current.filter(item =>
                    item.SUB_PLANDATETYPE == formValues.SUB_PLANDATETYPE ); 
                if (filteredResults.length == 1) {
                    showGlobalConfirmBox('是否增加小組意見?',() => setAdViewDesc(filteredResults[0].TEMPLATE));
                } else {
                    setAdViewDesc(""); 
                }
            }
        }
    };
    
    // 預覽列印
    const preview = () => {
        let data = {
            projectNo: ipcPlanNo,
            projectName: ipcPlanName
        }
        openProjectPrint(data);
    }
    
    /**
     * 計畫退回
     * @returns
     */
    const backSend = async () => {
        let result = await SendBackProject(projectNo);
        // 成功訊息
        if (result.success) {
            showGlobalMessageBox(result.message, () => { window.location.reload(); });
        }
    }

    /**
     * 取消
     * @returns
     */
    const clean = () => {
        // 使用初始表單資料來重設表單狀態
        setFormData({...initialFormData});
        setAdViewDesc(initialFormData.ADVIEWDESC);
    
        // 重置Formik表單
        formRef.current.resetForm({
            values: JSON.parse(JSON.stringify(initialFormData)),
        });
    };
    

    /**
     * 改變輸入框
     */
    const onCellInputChange = () => {
    }

    /**
     * 數字輸入框
     * @param {*} props 
     * @returns 
     */
    const numerCell = (props) => {
        return (
            <NumericTextInputCell
                {...props}
                format={'n0'}
                editable={true}
                onCellInputChange={onCellInputChange}
                AlwaysEdit={true}
                min={0}
            />
        );
    };

    return (
        <>
            <PageContainer style={{ overflow: "auto", height: "100%" }}>
                <CollapseBoardCard
                    button={
                        <>
                            <Button title="存檔" onClick={handleSaveClick} disabled={canSave}>存檔</Button>
                            <Button title="取消" className="k-button-lighten" onClick={clean}>取消</Button>
                            <Button title="已審核" className="k-button-lighten" onClick={handleReviewClick} disabled={!showSendButton}>審核</Button>
                            <Button title="退回" className="k-button-lighten" onClick={(backSend)} disabled={!showSendButton}>退回</Button>
                            
                        </>
                    }
                    title="小組審核作業"
                    isFirstArea={true}
                > 
                <Formik
                    initialValues={formData}
                    onSubmit={(data) => saveChanges(data)}
                    enableReinitialize={true}
                    innerRef={formRef}
                    //字段改變不驗證
                    validateOnChange={false}
                >
                    {props => {
                        const {
                            values,
                            setValues
                        } = props;
                        return (
                            <form>
                                <table>
                                    <tr>
                                        <th>
                                            年度需求數（千元）
                                        </th>
                                        <td colSpan={3}>
                                            公務預算: {values.publicMoney}千元&nbsp;&nbsp;&nbsp;基金預算: {values.FundMoney}千元
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            計畫狀態
                                        </th>
                                        <td style={{width:"30%"}}>
                                            {values.IS_SEND == true ? "已送出" : "未送出"}
                                        </td>
                                        <th>
                                            執行類別
                                        </th>
                                        <td>
                                            { values.PLANKIND === "1" ? "重大施政" : "委託研究" }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            建議核列金額
                                        </th>
                                        <td colSpan={3}>
                                        <Grid
                                            style={{ overflow: 'auto', height: '100%' }}
                                            resizable={true}
                                            data={values.gridData}
                                        >
                                            <GridNoRecords>無資料</GridNoRecords>
                                            <GridColumn field="Number" title="優先序" width="100" />
                                            <GridColumn field="Check" title=" " width="100" />
                                            <GridColumn field="PublicBudget" title="公務預算(千元)" cell={numerCell}/>
                                            <GridColumn field="FundBudget" title="基金預算(千元)" cell={numerCell}/>
                                        </Grid>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            關聯重大
                                        </th>
                                        <td colSpan={3}>
                                            {ipcPlanNo}
                                            <Button type='button' title="關聯" onClick={() => {setWindowVisible(true);}} style={{ marginRight: "10px", marginLeft: "10px" }}>關聯</Button>
                                            { ipcPlanNo !== null &&
                                                <Button type='button' title="檢視" onClick={preview} style={{ marginRight: "10px", marginLeft: "10px" }}>檢視</Button>
                                            }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            評核類別
                                        </th>
                                        <td colSpan={3}>
                                            <DropDownListWithValue
                                                name="SUB_PLANDATETYPE"
                                                textField="SET_VALUE"
                                                dataItemKey="SET_TYPE"
                                                data={bougetReview}
                                                value={values.SUB_PLANDATETYPE}
                                                onChange={(e) => { 
                                                    const updatedValues = { ...values, SUB_PLANDATETYPE: e.target.value };
                                                    setValues(updatedValues);
                                                    filterAdViewDesc(updatedValues);
                                                }}
                                            />
                                        </td>
                                    </tr>
                                    { showDropDone && 
                                        <tr>
                                            <th>
                                                關聯性
                                            </th>
                                            <td >
                                                <DropDownListWithValue
                                                    name="PLAN_REF"
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    data={relevance}
                                                    value={values.PLAN_REF}
                                                    onChange={(e) => { 
                                                        const updatedValues = { ...values, PLAN_REF: e.target.value };
                                                        setValues(updatedValues);
                                                        filterAdViewDesc(updatedValues);
                                                    }}
                                                />
                                            </td>
                                            <th>
                                                執行績效
                                            </th>
                                            <td >
                                                <DropDownListWithValue
                                                    name="EXE_PERFORMANCE"
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    data={exePerformance}
                                                    value={values.EXE_PERFORMANCE}
                                                    onChange={(e) => { 
                                                        const updatedValues = { ...values, EXE_PERFORMANCE: e.target.value };
                                                        setValues(updatedValues);
                                                        filterAdViewDesc(updatedValues);
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                    }
                                    <tr>
                                        <th>
                                            專案小組審查意見
                                        </th>
                                        <td colSpan={3}>
                                            <TextAreaInput
                                                name="ADVIEWDESC"
                                                rows={5}
                                                maxlength={2000}
                                                style={{ width: "100%" }}
                                                defaultValue={adViewDesc}
                                                onBlur={(e) => {
                                                    setAdViewDesc(e.target.element.current.value);
                                                }}
                                            />
                                        </td>
                                    </tr>
                                </table>
                                {windowVisible &&
                                    <GpReviewProjectWindow 
                                        closeWindow={() => { setWindowVisible(false); }}
                                        setIpcPlanNo={setIpcPlanNo}
                                        setIpcPlanName={setIpcPlanName}
                                    />
                                }
                            </form>
                        )
                    }}

                </Formik>
                </CollapseBoardCard>
            </PageContainer>
        </>
    );
}
export default CGpReviewProjectMain;
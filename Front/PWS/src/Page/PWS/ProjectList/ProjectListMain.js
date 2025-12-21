import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import OrgSelectPanel from '../../../Components/Selector/OrgSelectPanel';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Formik } from 'formik';
import TextInput from '../../../Components/Input/TextInput';
import { ExportGrid } from '../../../Basic/Download';
import AddProjectWindow from './AddProjectWindow';
import { openProjectChapter } from "../ProjectChapter/ProjectChapterService";
import { getProjectList, deleteProjectList, copyProject, getOrgDeadline } from "./ProjectListService";
import { ProjectListGrid } from "./ProjectListGrid";
import { IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { getPlanYearList, GetSetParam } from "../../../Basic/CommonService";
import { GetBasicData } from "../../../Basic/BasicData";
import { showGlobalConfirmBox } from "../../../Route/RootMiddleware";
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { exportRPT } from "../ReportList/ReportService";

const ProjectListMain = (props) => {
    //是否能新增計畫
    const [canAddPlan, setCanAddPlan] = useState(false);
    //是否能刪除計畫
    const [canDeletePlan, setCanDeletePlan] = useState(false);
    // grid資料
    const [selectResultData, setSelectResultData] = useState([]);
    // 下拉資料
    const [planKindDropDown, setPlanKindDropDown] = useState([]);
    // 控制windowBox的開啟
    const [windowVisible, setWindowVisible] = useState(false);
    // 查詢是否開啟
    const [showFilterForm, setShowFilterForm] = useState(false);
    // 匯出Grid資料
    const GridDataForExport = useRef({ props: {}, Columns: [] })
    // 表單紀錄
    const formRef = useRef(null);
    // 下拉選單年度
    const [YearOptions, setYearOptions] = useState([]);
    // 勾選資料
    const checkdata = useRef([]);
    // 組織樹資料
    const [treeData, setTreeData] = useState([]);
    // 表單初始資料
    const initialValues = ({
        PLANYEAR: (new Date().getFullYear() - 1911 + 1).toString(),
        PLANNO: "",
        PLANKIND: "",
        PLANNAME: "",
        ORGOUNAME: "",
        UNITOUNAME: ""
    });
    // 表單資料
    const [formData, setFormData] = useState({...initialValues});

    /**
     * 腳色權限判斷
     * @returns
     */
    const PlanPermissions = async () => {
        // 取機關截止 是否能新增計畫
        let OrgId = await GetBasicData('orgId');
        let result = await getOrgDeadline(OrgId);
        setCanAddPlan(result);

        // 取帳號腳色 是否能刪除計畫
        let allRoles = await GetBasicData('allRoles');
        if (allRoles.length > 0) {
            let Roles = allRoles.some(item => item.ROLE_ID == 'ORG_RDEC_ROL_PWS' || item.ROLE_ID == 'AREA_RDEC_ROL_PWS');
            setCanDeletePlan(Roles);
        }
    }

    /**
     * 取下拉選單資料
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        // 年度
        let Year = await getPlanYearList(false, 20, "A");
        setYearOptions(Year);
        // 取得計畫類別
        let cpKindData = await GetSetParam('PLAN_KIND', '');
        if (cpKindData.length > 0) {
            setPlanKindDropDown([ { SET_VALUE: "請選擇", SET_TYPE: "" }, ...cpKindData]);
        }
        SetMaskOnOff(false);
    };

    
    /**
     * 查詢
     * @param {*} data 
     * @param {*} isResetOrgTree 
     */
    const select = async (data, isResetOrgTree = true) => {
        SetMaskOnOff(true);
        let OrgId = await GetBasicData('orgId');
        data = { ...data, CREATEORGOUID: OrgId };
        let result = await getProjectList(data);
        setSelectResultData(result);
        if(isResetOrgTree){        
            let treeData = genOrgTreeData(result);
            setTreeData(treeData);
        }
        SetMaskOnOff(false);
    }

    /**
     * 產生組織樹資料
     * @param {*} data 
     * @returns 
     */
    const genOrgTreeData = (data) => {
        // 取得計劃清單機關清單
        let projOrgPanelDataList = data.filter(proj => !IsNullOrEmpty(proj.OU_ID))
            .map(proj => {
                let projCnt = data.filter(x => x.OU_ID === proj.OU_ID).length;
                return {
                    OU_ID: proj.OU_ID,
                    title: proj.OU_NAME + '(' + projCnt + ')',
                    order: proj.ORDER
                }
            });
        // 去除重複的組織數據
        let distinctData = projOrgPanelDataList.filter((value, index, self) =>
            index === self.findIndex((t) => (
                t.OU_ID === value.OU_ID
            ))
        );
        // 排序（如果有需要）
        distinctData.sort((a, b) => a.order - b.order);
        // 在清單前面新增一個總項
        distinctData.unshift({ OU_ID: "", title: `桃園市政府(${data.length})` });
        return distinctData;
    };


    /**
     * 組織樹查詢
     * @param {*} ouId 
     */
    const onSelect = async (data) => {
        let ouId = data.OU_ID;
        // 根據所選組織ID更新查詢條件
        let updatedQueryCondition = {
            ...formRef.current.values,
            OU_ID: ouId
        };
        // 重新查詢並更新 gridData
        await select(updatedQueryCondition, false);
    };

    // 畫面資料判斷
    useEffect(() => {
        // 取得腳色權限
        PlanPermissions();
        // 取畫面選單資料
        loadData();
        // 初始化查詢
        select(formData);
    }, []);

    /**
     * 刪除
     */
    const deletePlan = async() => {
        // 判斷是否有選取資料
        if (checkdata.current.length == 0) {
            showGlobalMessageBox("請選擇一筆資料進行刪除", () => { });
        }
        else {
            showGlobalConfirmBox("確定刪除所選計畫?", async () => {
                // 判斷是否有已送出計畫
                let isRemove = checkdata.current.some(item => item.IS_SEND == true);
                if( !isRemove ){
                    let PLANNO = checkdata.current.map(item => item.PLANNO);
                    let result = await deleteProjectList(PLANNO);
                    // 刪除成功訊息
                    if (result.success) {
                        showGlobalMessageBox(result.message, () => { checkdata.current = []; window.location.reload(); });
                    }
                }
                else{
                    showGlobalMessageBox("包含已送出計畫不可刪除", () => { });
                }
            });
        }
    }

    /**
     * 複製計畫
     * @param {*} planNo
     */
    const copyPlan = async (planNo) => {
        if (planNo.length == 1) {
            showGlobalConfirmBox("確定複製所選計畫?", async () => {
                let result = await copyProject(planNo[0].PLANNO);
                // 複製成功訊息
                if (result.success) {
                    showGlobalMessageBox(result.message, () => { checkdata.current = []; openProjectChapter(result.data, 0); window.location.reload(); });
                }
            });
        }
        else if (planNo.length == 0){
            showGlobalMessageBox("請選擇一筆資料進行複製", () => { });
        }
        else if (planNo.length > 1){
            showGlobalMessageBox("一次只能複製一筆資料", () => { });
        }
    }

    /**
     * 計畫資料報表匯出
     * @param {*} checkdata
     */
    const ReportExport = async (checkdata) => {
        if (checkdata.length > 0) {
            // 確認是否為同一類別計畫
            const isSamePLANKIND = checkdata.every(item => item.PLANKIND == checkdata[0].PLANKIND);
            if (isSamePLANKIND) {
            // 取得計畫編號
            let PlanNoList = checkdata.map(item => item.PLANNO);
            // 匯出重大或委託
            if(checkdata[0].PLANKIND == 1){
            let data =  {
                RPT_ID : "RPTProjectPolicyReview",
                STATISTICS_NAME : "桃園市政府113先期計畫重大施政計畫先期審查表",
                PlanNoList : PlanNoList

            };
            await exportRPT(data);
            }
            else{
            let data =  {
                RPT_ID : "RPTProjectEntList",
                STATISTICS_NAME : "桃園市政府113先期計畫先期審查",
                PlanNoList : PlanNoList

            };
            await exportRPT(data);
            }
            } else {
                showGlobalMessageBox("請選擇同一類別資料匯出", () => { });
            }
        }
    }

    /**
     * 清除表單
     */
    const clean = () => {
        if (formRef.current) {
            formRef.current.resetForm();
        }
    }

    /**
     * 切換篩選表單
     */
    const toggleFilterForm = () => {
        setShowFilterForm(!showFilterForm);
    };

    /**
     * 利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
     */
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    };

    return (
        <>
            <h3 className="k-dialog-titlebar">先期計畫登入</h3>
            <div style={{ 'display': 'flex' }}>
                <OrgSelectPanel
                    data={treeData}
                    onSelect={onSelect}
                    title={'主管機關'}
                    style={{ overflow: "auto" }}
                />
                <PageContainer
                    toolbar={
                        <>
                            <div style={{ display: 'flex', justifyContent: 'start' }}>
                                <Button title="篩選計畫" className="k-button-lighten" onClick={toggleFilterForm}>篩選計畫</Button>
                                {!canAddPlan &&
                                    <Button title="新增計畫" className="k-button-lighten"onClick={() => {setWindowVisible(true);}}> 新增計畫</Button>
                                }
                                {canDeletePlan &&( 
                                    <Button title="刪除計畫" className="k-button-lighten" onClick={deletePlan}>刪除計畫</Button>
                                    )}
                                <Button title="複製計畫" className="k-button-lighten"onClick={() => {copyPlan(checkdata.current);}}> 複製計畫</Button>
                                <Button title="匯出Excal" className="k-button-lighten" onClick={() => ExportGrid(GridDataForExport.current, "xlsx", "先期計畫登入")}>匯出Excal</Button>
                                <Button title="匯出Ods" className="k-button-lighten" onClick={() => ExportGrid(GridDataForExport.current, "ods", "先期計畫登入")}>匯出Ods</Button>
                                <Button title="匯出計畫資料" className="k-button-lighten" onClick={() => {ReportExport(checkdata.current);}}>匯出計畫資料</Button>
                            </div>
                            {showFilterForm && (
                            <div style={{ display: 'flex', justifyContent: 'start' }}>
                                <Button title="查詢" className="k-button-lighten" onClick={handleSubmit}>查詢</Button>
                                <Button title="取消" className="k-button-lighten" onClick={clean}>取消</Button>
                            </div>
                            )}
                        </>
                    }
                    style={{ height: "calc(100vh - 129px)" }}
                >
                <div style={{ display: showFilterForm ? 'block' : 'none' }}>
                    <Formik
                        initialValues={formData}
                        onSubmit={(data) => select(data)}
                        enableReinitialize={true}
                        innerRef={formRef}
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
                                                年度
                                            </th>
                                            <td>
                                                <DropDownListWithValue
                                                    name="PLANYEAR"
                                                    data={YearOptions}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.PLANYEAR}
                                                    onChange={(e) => { setValues({ ...values, PLANYEAR: e.target.value }) }}
                                                />
                                            </td>
                                            <th>
                                                編號
                                            </th>
                                            <td>
                                                <TextInput
                                                    name="PLANNO"
                                                    value={values.PLANNO}
                                                    onChange={handleChange}
                                                    style={{ width: "50%" }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                計畫類別
                                            </th>
                                            <td>
                                            <DropDownListWithValue
                                                    data={planKindDropDown}
                                                    textField={"SET_VALUE"}
                                                    dataItemKey={"SET_TYPE"}
                                                    value={values.PLANKIND}
                                                    onChange={(e) => {
                                                        {setValues({ ...values, PLANKIND: e.target.value })};
                                                    }}
                                                />
                                            </td>
                                            <th>
                                                計畫名稱
                                            </th>
                                            <td>
                                                <TextInput
                                                    name="PLANNAME"
                                                    value={values.PLANNAME}
                                                    onChange={handleChange}
                                                    style={{ width: "50%" }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                提報機關
                                            </th>
                                            <td>
                                                <TextInput
                                                    name="ORGOUNAME"
                                                    value={values.ORGOUNAME}
                                                    onChange={handleChange}
                                                    style={{ width: "50%" }}
                                                />
                                            </td>
                                            <th>
                                                提報單位
                                            </th>
                                            <td>
                                                <TextInput
                                                    name="UNITOUNAME"
                                                    value={values.UNITOUNAME}
                                                    onChange={handleChange}
                                                    style={{ width: "50%" }}
                                                />
                                            </td>
                                        </tr>
                                    </table>
                                </form>
                            )
                        }}
                    </Formik>
                    </div>
                    <ProjectListGrid
                        selectResultData={selectResultData}
                        GridDataForExport={GridDataForExport}
                        checkdata={checkdata}
                    />
                    {windowVisible &&
                        <AddProjectWindow
                            closeWindow={() => { setWindowVisible(false); }}
                        />
                    }
                </PageContainer>
            </div>
        </>
    );
}
export default ProjectListMain;
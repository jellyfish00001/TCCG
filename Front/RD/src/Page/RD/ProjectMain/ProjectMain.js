import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { Button } from '@progress/kendo-react-buttons';
import  OrgSelectPanel  from '../../../Components/Selector/OrgSelectPanel';
import CheckBoxList from '../../../Components/Input/CheckBoxList';
import TextInput from "../../../Components/Input/TextInput";
import { Pageable } from '../../../Basic/BasicData';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { Formik } from "formik";
import { DropDownListWithValue } from "../../../Components/Dropdowns/DropDownListWithValue";
import { FormatDate, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { getPlanYearList} from "../../../Basic/CommonService";
import { orderBy } from "@progress/kendo-data-query";
import { GetBasicData } from '../../../Basic/BasicData';

import ProjectAddWindow from './ProjectAddWindow';
import { getQueryData, GetRDProjectManage, SaveRDBasicStatus, genOrgTreeData, openProjectChapter } from "./ProjectMainService";


/**
 * 研究發展作業系統-委託研究計畫管理以及委託研究計畫提報 共用畫面
 */
const ProjectManageMain = (props) => { 
    // grid data
    const [gridData, setGridData] = useState([]);
    // grid 分頁
    const [paging, setPaging] = React.useState({ skip: 0, take: 20 });
    // grid 排序
    const [sort, setSort] = React.useState([{ field: "", dir: "" }]);
    // 登入者是否具備管考權限(管考角色)
    const [isRDEC, setIsRDEC] = React.useState(false);
    // 年度下拉選單資料
    const [ddlData, setDDLData] = React.useState({
        PLAN_YEAR: []
    });
    // 已選取之計畫
    const checkedPlan = useRef([]);
    // 組織樹資料
    const [planOrgData, setPlanOrgData] = useState([])
    // 表單資料
    const formRef = React.useRef(null);

    // 查詢條件初始值，清除(reset)的時候使用
    const [initQueryData, setInitQueryData] = React.useState();
    // 查詢條件
    const queryData = React.useRef({});
    // 查詢條件開關控制
    const [queryConditionVisible, setQueryConditionVisible] = useState(false);
    // 查詢 button 文字
    const [showHideText, setShowHideText] = useState("篩選計畫");
    // 隱藏、顯示查詢條件 Event
    const showHideQueryCondition = () => {
        setQueryConditionVisible(!queryConditionVisible)
        queryConditionVisible ? setShowHideText("篩選計畫") : setShowHideText("隱藏篩選計畫");
    }

    /***** 計畫提報/登錄使用 *****/
    // 建立計畫 window，預設關閉 false
    const [windowVisible, setWindowVisible] = useState(false)

    /**
     * 存檔（新增/提報計畫）
     * @param {string} planNo
     * @param {string} planName
     */
    const saveEvent = async (planNo, planName) => {
        // 關閉 window
        setWindowVisible(false);
        let item = {
            PLAN_NO: planNo,
            PLAN_NAME: planName,
            funRole: isRDEC ? 2 : 0 // 依主辦(0)面向（主辦才能提報）
        }
        // 開啟章節表
        openProjectChapter(item);
    }

    /**
     *  頁面載入資料（初始、查詢、執行機關）
     *  @param {object} data 撈取條件
     *  @param {bool} isInitial 是否第一次初始化撈資料
     *  @param {bool} isResetOrgTree 是否重新產生組織樹
     *  @returns 
     */
    const loadData = async (data, isInitial = false, isResetOrgTree = false) => {
        SetMaskOnOff(true);
        // 檢查目前路徑是否為計畫管理(ProjectManage)
        let href = window.location.href;
        let isProjectManage = href.includes("ProjectManage");
        // 路徑為計畫管理(ProjectManage)，權限就是管考
        setIsRDEC(isProjectManage);

        // 有管考權限可以看全部機關的資料，非管考權限只能看同自己機關的資料
        if(!isProjectManage){
            // 取得登入者的機關 ID
            let orgId = await GetBasicData("orgId");
            data.OU_ID = orgId
        }

        // 是否第一次載入資料
        if(isInitial){
            // 載入年度下拉選單資料，往前 10 年
            let yearDropDownList = await getPlanYearList();
            yearDropDownList.unshift({text:"請選擇", value:""});
            setDDLData({
                PLAN_YEAR: [...yearDropDownList]
            });
            
        }
        // 將撈取條件更新回查詢條件 Ref
        queryData.current = data;
        // 取得委託研究計畫清單
        let projectList = await GetRDProjectManage(data);
        setGridData(projectList);
        // 設置 grid 頁碼回第一頁
        setPaging({ ...paging, skip: 0 });
        // 是否要重新產生組織樹
        if(isResetOrgTree){
            setPlanOrgData(genOrgTreeData([...projectList]));
        }
        SetMaskOnOff(false);
    }

    /**
     * 執行機關選擇事件 callback
     * @param {string} ouId 機關代號
     */
    const selectOrg = async (ouId) => {
        // 目前查詢條件再加上機關條件
        let queryDataWithOrg = {
            ...queryData.current,
            OU_ID: ouId
        };
        // 更新回去目前查詢條件
        queryData.current = queryDataWithOrg;
        // 撈 Grid 資料
        await loadData(queryDataWithOrg);
    }
    useEffect(() => {
        // 帶入查詢條件初始值
        let initData = getQueryData();
        setInitQueryData(initData);
        loadData(initData, true, true);
    }, [])
    /**
     * 查詢送出
     * @param {*} data
     */
    const onSubmit = async (data) => {
        loadData(data, false, true);
    }
    /**
     * 清除
     * @param {*} data
     */
    const handleReset = async () => {
        SetMaskOnOff(true);
        if (formRef.current) {
            formRef.current.resetForm({ values: initQueryData })
        }
        SetMaskOnOff(false);
    }
    /**
     * 刪除及撤銷多筆計畫
     * @param {char} execKind D 刪除 R 撤銷
     */
    const DeleteRevokePlans = (execKind) => {
        if (checkedPlan.current.length > 0) {
            showGlobalConfirmBox("請確認是否執行動作",
                () => { DeleteRevokeEvent(execKind) }
            )
        } else {
            showGlobalMessageBox('請選擇欲執行動作計畫');
        }
    };
    /**
     * 刪除及撤銷多筆計畫 event
     * @param {char} execKind D 刪除 R 撤銷
     */
    const DeleteRevokeEvent = async (execKind) => {
        SetMaskOnOff(true);
        let result = await SaveRDBasicStatus(checkedPlan.current, execKind);
        if(result != null){
            showGlobalMessageBox(result.message,
                () => {
                    window.location.reload();
                })
            }
        SetMaskOnOff(false);
    };
    /**
     * 編修欄位功能鍵
     * @param {*} props
     * @return {React.Component} 編輯按鈕 
     */
    const EditCell = (props) => {
        return (
            <CommandCell>
                <Button title={"編輯"} icon='edit' look='default' onClick={() => {
                    // 存入 Storage Data 資料物件
                    let item = {
                        PLAN_NO:props.dataItem.PLAN_NO,
                        PLAN_NAME:props.dataItem.PLAN_NAME,
                        funRole:  isRDEC ? 2 : 0// 依管考(2)面向
                    }
                    openProjectChapter(item);
                }}
                />
            </CommandCell>
        );
    };
    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };
    /**
     * 刪除欄位功能鍵
     */
    const DeleteCell = (props) => {
        return (
            <>
            {
                isRDEC
                &&
                <CommandCell>
                    <CheckBoxList
                        group='CheckBoxList'
                        valueField='id'
                        data={[
                            { PLAN_NO: props.dataItem.PLAN_NO, },
                        ]}
                        onChange={(e) => {
                            SetChecked(e);
                        }}
                    />
                </CommandCell>
            }
            </>
        );
    };
    /**
     * 將勾選的資料放進checkedPlan
     */
    const SetChecked = (e) => {
        if (e.value === true){
            checkedPlan.current.push(e.dataItem.PLAN_NO);
        }
        else {
            checkedPlan.current.map((list, index) => {
                if (list === e.dataItem.PLAN_NO) {
                    checkedPlan.current.splice(index, 1);
                }
            });
        }
    }

    return (
        <>
            <h3 className="k-dialog-titlebar">
                {isRDEC ? "計畫管理" : "計畫登錄"}
            </h3>
            <div style={{ 'display': 'flex' }}>
                {isRDEC // 路徑是計畫管理，才看得到執行機關的面板
                    ?
                    <OrgSelectPanel
                        data={planOrgData}
                        onSelect={selectOrg}
                        title={'執行機關'}
                        style={{ overflow: "auto" }}
                    />
                    :
                    null
                }
                <PageContainer
                    toolbar={
                        <>
                            {/* 不管是管考還是主辦，都可以使用篩選計畫功能 */}
                            <Button title="隱藏篩選計畫" className="k-button-lighten" onClick={showHideQueryCondition}>{showHideText}</Button>
                            {isRDEC
                                ? // 路徑是計畫管理，才看得到功能鍵
                                <>
                                    <Button title="發送稽催填報" className="k-button-lighten">發送稽催填報</Button>
                                    <Button title="發送稽催執行情形填報" className="k-button-lighten">發送稽催執行情形填報</Button>
                                    <Button title="刪除" className="k-button-lighten" onClick={() => DeleteRevokePlans('D')} >刪除</Button>
                                    <Button title="撤銷" className="k-button-lighten" onClick={() => DeleteRevokePlans('R')} >撤銷</Button>
                                </>
                                : // 路徑不是計畫管理，只看得到建立計畫
                                <>
                                <Button type='button' title="建立計畫基本資料" className="k-button-lighten"
                                    onClick={() => {setWindowVisible(true) }} >建立計畫基本資料
                                </Button>
                                </>
                            }
                        </>
                    }
                    style={{ overflow: "auto", height: "calc(100vh - 129px)" }}
                >
                <div style={{ display: queryConditionVisible ? 'inline' : 'none' }}>
                    <Formik
                        initialValues={queryData.current}
                        innerRef={formRef}
                        onSubmit={(data) => onSubmit(data)}
                        enableReinitialize // 允許重複賦予初始值，要外部傳入 initialValues 更新資料 
                    >
                    {prop => {
                        const {
                            values,
                            setValues,
                            handleChange,
                            handleSubmit
                        } = prop;
                        return (
                            <form onSubmit={handleSubmit} onReset={handleReset}>
                                <div className="fn-buttons">
                                    <Button className='k-button-lighten' type="submit">查詢</Button>
                                    <Button className='k-button-lighten' type="reset">清除</Button>
                                </div>
                                <table>
                                    <tr>
                                        <th>
                                            年度
                                        </th>
                                        <td>
                                            <DropDownListWithValue
                                                name="PLAN_YEAR"
                                                data={ddlData.PLAN_YEAR}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.PLAN_YEAR}
                                                onChange={(e) => { setValues({ ...values, PLAN_YEAR: e.target.value }) }}
                                            />
                                        </td>
                                        <th>計畫編號</th>
                                        <td>
                                            <div>
                                                <TextInput
                                                    name="PLAN_NO"
                                                    value={values.PLAN_NO}
                                                    style={{ width: "100%" }}
                                                    onChange={handleChange}
                                                />
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>計畫名稱</th>
                                        <td>
                                            <div>
                                                <TextInput
                                                    name="PLAN_NAME"
                                                    value={values.PLAN_NAME}
                                                    style={{ width: "100%" }}
                                                    onChange={handleChange}
                                                />
                                            </div>
                                        </td>
                                        <th>受託單位</th>
                                        <td>
                                            <div>
                                                <TextInput
                                                    name="ENTRUST_UNIT_NAME"
                                                    value={values.ENTRUST_UNIT_NAME}
                                                    style={{ width: "100%" }}
                                                    onChange={handleChange}
                                                />
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </form>
                        );
                    }}
                </Formik>
                </div>
                
                <Grid
                    data={gridData.slice(paging.skip, paging.take + paging.skip)}
                    style={{ overflow: 'auto', height: '100%'}}
                    resizable={true}
                    total={gridData.length}
                    skip={paging.skip}
                    take={paging.take}
                    pageable={Pageable}
                    sort={sort}
                    onSortChange={sortChange}
                    sortable={{ allowUnsort: true, mode: "single" }}
                    onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take})}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    {isRDEC && <GridColumn field="DELETE" title="刪除" cell={DeleteCell} width="50px" />}
                    <GridColumn field="EDIT" title="編修" cell={EditCell} width="50px" />
                    <GridColumn field="PLAN_NO" title="計畫編號" width="80px" />
                    <GridColumn field="PLAN_NAME" title="計畫名稱" width={isRDEC ? "" : ""}/>
                    <GridColumn field="PLAN_DATE" title="計畫期程" width="180px" />
                    <GridColumn field="CONTACT_NAME" title="承辦人" width="100px" />
                    {!isRDEC && <GridColumn field="EXEC_ORG_NAME" title="提報單位" width="100px" />}
                    {!isRDEC && <GridColumn field="CRT_USER_NAME" title="提報人員" width="70px" />}
                </Grid>
                <ProjectAddWindow
                        visible = {windowVisible}
                        onClose = {() => { setWindowVisible(false);}}
                        saveEvent = {(planNo, planName) => saveEvent(planNo, planName)}
                    />
            </PageContainer>
            </div>
        </>
    );
}
export default ProjectManageMain;
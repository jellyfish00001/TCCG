
import React, { useState, useRef } from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { ExportGrid } from '../../../Basic/Download';
import { IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { CommandCell } from '../../../Components/GridCell/CommandCell';
import CheckBoxList from '../../../Components/Input/CheckBoxList';
import OrgSelectPanel from '../../../Components/Selector/OrgSelectPanel';
import { Button } from "@progress/kendo-react-buttons";
import ProjectListtReviewQueryForm from './ProjectListtReviewQueryForm';
import ProjectListGrid from '../ProjectList/ProjectListGrid';
import { getProjectList, openProjectChapter, saveProjectCanceled } from "../ProjectList/ProjectListService";

const ProjectListtReviewMain = ({
    location: { state },
}) => {

    // 查詢條件開關控制
    const [queryConditionVisible, setQueryConditionVisible] = useState(false);
    // 查詢條件初始化次數(用來控制Form Component 初始化查詢條件)
    const [defaultQueryConditionCnt, setDefaultQueryConditionCnt] = useState(0);
    // 查詢button文字
    const [showHideText, setShowHideText] = useState("篩選計畫");
    // Grid資料
    const [gridData, setGridData] = useState([]);
    // 查詢條件資料
    const [queryCondition, setQueryCondition] = useState({});
    // 預測查詢條件
    const defaultQueryCondition = useRef({});
    // 匯出Grid資料
    const GridDataForExport = React.useRef({ props: {}, Columns: [] })
    // 已選取之計畫
    const checkedPlan = useRef([]);
    // 是否需要產生組織樹
    // 是否為初次載入
    const [isFirstLoad, setIsFirstLoad] = useState(true);
    // 機關計畫數
    const [projectOrgData, setProjectOrgData] = useState([])

    /**
     * 查詢資料異動
     * @param {{}} data 查詢條件
     * @param {boolean} isResetOrgTree 是否重新產生組織樹
     */
    const queryDataChange = async (data, isResetOrgTree = true) => {
        setQueryCondition({ ...data });
        let requestData = { ...data };
        SetMaskOnOff(true);
        let result = await getProjectList(requestData);
        if (isFirstLoad) {
            defaultQueryCondition.current = { ...data };
            setIsFirstLoad(false);
        }
        // 產生組織樹
        if (isResetOrgTree) {
            // 產生組織樹資料
            let projOrgPanelData = genOrgTreeData([...result]);
            setProjectOrgData([...projOrgPanelData])
        }
        setGridData(result)
        SetMaskOnOff(false)
    };

    // 產生組織樹資料
    const genOrgTreeData = (data) => {
        // 取得計畫列表機關清單
        let projOrgPanelDataList = data.filter(proj => !IsNullOrEmpty(proj.EXEC_ORGAN_C))
            .map(proj => {
                let projCnt = data.filter(x => x.EXEC_ORGAN_C === proj.EXEC_ORGAN_C).length;
                return {
                    OU_ID: proj.EXEC_ORGAN_C,
                    title: proj.EXEC_ORGAN_NAME + '(' + projCnt + ')',
                    order: proj.EXEC_ORGAN_ORDER
                }
            });
        // Distinct projOrgPanelDataList
        let distinctData = projOrgPanelDataList.filter((value, index, self) =>
            index === self.findIndex((t) => (
                t.OU_ID === value.OU_ID && t.OU_NAME === value.OU_NAME
            ))
        );
        distinctData.sort((a, b) => a.order - b.order);
        distinctData.unshift({ OU_ID: "", title: `桃園市政府(${data.length})` });
        return distinctData;
    }

    // 執行機關選擇事件callback
    const onSelect = async (ouId) => {
        let queryData = {
            ...queryCondition,
            EXEC_DEPT: ouId
        }

        // 清空已勾選計畫
        checkedPlan.current = [];
        await queryDataChange(queryData, false)
    }

    // 刪除計畫
    const deletePlan = async () => {
        if (checkedPlan.current.length > 0) {
            showGlobalConfirmBox("請確認是否刪除計畫，刪除後計畫無法查詢及恢復",
                () => { deleteEvent() }
            )
        } else {
            showGlobalMessageBox('請選擇欲刪除計畫');
        }

    }

    // 刪除事件
    const deleteEvent = async () => {
        let result = await saveProjectCanceled(checkedPlan.current);
        if (result != null && result.success) {
            showGlobalMessageBox(result.message, () => {
                // setIsFirstLoad(true);
                setDefaultQueryConditionCnt(defaultQueryConditionCnt + 1)
                queryDataChange(defaultQueryCondition.current)
            })
        }
    }

    // 匯出
    const exportPlan = async (extension) => {
        GridDataForExport.current.Columns = GridDataForExport.current.Columns.filter(x => x.title !== "審查" && x.title !== "釘選");
        ExportGrid(GridDataForExport.current, extension, '計畫清單');
    }

    //隱藏、顯示查詢條件 Event
    const showHideQueryCondition = () => {
        setQueryConditionVisible(!queryConditionVisible)
        queryConditionVisible ? setShowHideText("篩選計畫") : setShowHideText("隱藏篩選計畫");
    }

    //#region additional Grid Cell 
    // checkBox Cell
    const checkBoxCell = props => {
        return (
            <td style={{ 'textAlign': 'center' }}>
                <CheckBoxList
                    group='CheckBoxList'
                    valueField='id'
                    data={[
                        { id: props.dataItem.PROJECT_NO, },
                    ]}
                    onChange={(e) => {
                        SetChecked(e);
                    }}
                />
            </td>
        )
    }

    //將勾選的資料放進checkedPlan
    const SetChecked = (e) => {
        if (e.value === true)
            checkedPlan.current.push(e.dataItem.id);
        else {
            checkedPlan.current.map((list, index) => {
                if (list === e.dataItem.id) {
                    checkedPlan.current.splice(index, 1);
                }
            });
        }
    }

    // 審查按鈕Cell
    const reviewCell = (props) => {
        return (
            <CommandCell>
                <Button title={"審查"} icon='edit' look='default' onClick={() => {
                    // 依管考面向
                    openProjectChapter(props.dataItem, 2);
                }} />
            </CommandCell>
        )
    }

    // 額外Grid 命令欄位
    let additionsCmdCols = [
        { cell: checkBoxCell, title: '刪除' },
        { cell: reviewCell, title: '審查' }
    ]

    //#endregion

    return (
        <>
            <h3 className="k-dialog-titlebar">計畫審查-立案結案審查</h3>
            <div style={{ 'display': 'flex' }}>
                <OrgSelectPanel
                    data={projectOrgData}
                    onSelect={onSelect}
                    title={'執行機關'}
                    style={{ overflow: "auto" }}
                />

                <PageContainer
                    toolbar={
                        <>
                            <Button title="隱藏篩選計畫" className='k-button-lighten' onClick={showHideQueryCondition}>{showHideText}</Button>
                            <Button title="匯出Excel" className='k-button-lighten' disabled={gridData.length === 0} onClick={() => exportPlan('xlsx')}>匯出Excel</Button>
                            <Button title="匯出Ods" className='k-button-lighten' disabled={gridData.length === 0} onClick={() => { exportPlan('ods') }}>匯出Ods</Button>
                            <Button title="刪除計畫" className='k-button-lighten' onClick={deletePlan}>刪除計畫</Button>
                            <Button title="重新整理" className='k-button-lighten' onClick={() => { queryDataChange({ ...queryCondition, EXEC_ORGAN_C: "" }) }}>重新整理</Button>
                        </>
                    }
                    style={{ height: "calc(100vh - 129px)" }}
                >
                    <div style={{ display: queryConditionVisible ? 'inline' : 'none' }}>
                        <ProjectListtReviewQueryForm
                            visible={queryConditionVisible}
                            queryData={queryDataChange}
                            defaultQueryConditionCnt={defaultQueryConditionCnt}
                        />
                    </div>

                    <ProjectListGrid
                        data={gridData} // 資料顯示格式
                        GridDataForExport={GridDataForExport} // 匯出物件格式
                        additionalCmdCols={additionsCmdCols}
                        reloadGrid={() => { queryDataChange(queryCondition) }}
                        funRole={2} // 功能角色
                    />
                </PageContainer>
            </div>
        </>
    );
}

export default ProjectListtReviewMain;
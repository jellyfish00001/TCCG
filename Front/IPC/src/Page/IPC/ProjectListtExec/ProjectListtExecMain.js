
import React, { useEffect, useRef, useState } from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { ExportGrid } from '../../../Basic/Download';
import { GetBasicData } from '../../../Basic/BasicData';
import { CommandCell } from '../../../Components/GridCell/CommandCell';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { Button } from "@progress/kendo-react-buttons";
import ProjectAddWindow from './ProjectAddWindow';
import ProjectListtExecQueryForm from './ProjectListtExecQueryForm';
import ProjectListGrid from '../ProjectList/ProjectListGrid';
import { getProjectList, openProjectChapter } from "../ProjectList/ProjectListService";

const ProjectListtExecMain = () => {

    // 查詢條件開關控制
    const [queryConditionVisible, setQueryConditionVisible] = useState(false);
    // 查詢button文字
    const [showHideText, setShowHideText] = useState("篩選計畫");
    // Grid資料
    const [gridData, setGridData] = useState([]);
    // 查詢條件資料
    const [queryCondition, setQueryCondition] = useState({});
    // 匯出Grid資料
    const GridDataForExport = React.useRef({ props: {}, Columns: [] })
    // 是否具有管考權限
    const isRDEC = useRef(false)
    // 登入者機關
    const orgId = useRef('')

    const [projectAddVisible, setProjectAddVisible] = useState(false);

    // 查詢資料異動
    const queryDataChange = async (data) => {
        setQueryCondition({ ...data });
        let requestData = { ...data };
        SetMaskOnOff(true);
        let result = await getProjectList(requestData);
        setGridData(result)
        SetMaskOnOff(false)
    };

    // 匯出
    const exportPlan = async (extension) => {
        GridDataForExport.current.Columns = GridDataForExport.current.Columns.filter(x => x.title !== "編修" && x.title !== "釘選");
        ExportGrid(GridDataForExport.current, extension, '計畫清單');
    }

    //隱藏、顯示查詢條件 Event
    const showHideQueryCondition = () => {
        setQueryConditionVisible(!queryConditionVisible)
        queryConditionVisible ? setShowHideText("篩選計畫") : setShowHideText("隱藏篩選計畫");
    }

    // #region additional grid cmd cell
    /**
     * 定義指令欄 -編修按鈕
     * @param {*} props Grid傳入的參數
     * @returns CommandCell
     */
    const editCell = (props) => {
        const { EXEC_ORGAN_C, PROJECT_STATUS_C } = props.dataItem;
        if (isRDEC.current || (!isRDEC.current && EXEC_ORGAN_C === orgId.current)) {
            return (
                <CommandCell>
                    <Button
                        title={"編修"}
                        icon={!isRDEC.current && (PROJECT_STATUS_C === '7' || PROJECT_STATUS_C === '8') ? 'eye' : 'edit'}
                        look='default'
                        onClick={() => {
                            // 依主辦面向
                            openProjectChapter(props.dataItem, 0);
                        }} />
                </CommandCell>
            )
        } else {
            return (
                <td></td>
            )
        }

    }

    // 額外Grid 命令欄位
    let additionsCmdCols = [{ cell: editCell, title: '編修' }]
    // #endregion 

    const loadData = async () => {
        // // 檢查登入者是否有管考權限
        // let isRdec = await CheckIsRDECRole();
        // isRDEC.current = isRdec;
        // 登入者機關
        orgId.current = await GetBasicData("orgId");
    }

    useEffect(() => {
        loadData();
    }, [])

    const saveEvent = async (projectNo) => {
        // 刷新grid
        queryDataChange(queryCondition);
        let result = await getProjectList({ PROJECT_NO: projectNo });
        if (result.length === 1) {
            // 依主辦面向
            openProjectChapter(result[0], 0);
        } else {
            showGlobalMessageBox("章節表開啟有誤，請洽系統管理員");
        }

        // 關閉視窗
        setProjectAddVisible(false);
    }

    return (
        <PageContainer
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">計畫填報-資料登錄</h3>
                    <Button title="隱藏篩選計畫" className='k-button-lighten' onClick={showHideQueryCondition}>{showHideText}</Button>
                    <Button title="新增計畫" className='k-button-lighten' onClick={() => { setProjectAddVisible(true) }}>新增計畫</Button>
                    <Button title="匯出Excel" className='k-button-lighten' disabled={gridData.length === 0} onClick={() => exportPlan('xlsx')}>匯出Excel</Button>
                    <Button title="匯出Ods" className='k-button-lighten' disabled={gridData.length === 0} onClick={() => { exportPlan('ods') }}>匯出Ods</Button>
                    <Button title="重新整理" className='k-button-lighten' onClick={() => { queryDataChange(queryCondition) }}>重新整理</Button>
                </>
            }
        >
            <div style={{ display: queryConditionVisible ? 'inline' : 'none' }}>
                <ProjectListtExecQueryForm
                    visible={queryConditionVisible}
                    queryData={queryDataChange}
                />
            </div>

            <ProjectListGrid
                data={gridData} // 資料顯示格式
                GridDataForExport={GridDataForExport} // 匯出物件格式
                additionalCmdCols={additionsCmdCols}
                reloadGrid={() => {
                    queryDataChange(queryCondition)

                }}
                funRole={0} // 功能角色
            />

            <ProjectAddWindow
                visible={projectAddVisible}
                onClose={() => { setProjectAddVisible(false); queryDataChange(queryCondition) }}
                saveEvent={(projectNo) => saveEvent(projectNo)}
            />

        </PageContainer>
    );
}

export default ProjectListtExecMain;
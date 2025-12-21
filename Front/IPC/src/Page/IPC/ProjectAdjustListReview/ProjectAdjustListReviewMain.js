import React, { useState, useRef } from 'react';
import { Pageable } from '../../../Basic/BasicData';
import { PageContainer } from "../../../Basic/PageContainer";
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { Button } from "@progress/kendo-react-buttons";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { orderBy } from "@progress/kendo-data-query";
import { formatNumber } from '@telerik/kendo-intl';
import { ExportGrid } from '../../../Basic/Download';
import { getProjectList, genOrgTreeData } from './ProjectAdjustListReviewService';
import { openAdjustProjectChapter } from '../ProjectAdjustListExec/ProjectAdjustListExecService';
import ProjectLogListWindow from '../ProjectLogList/ProjectLogListWindow';
import OrgSelectPanel from '../../../Components/Selector/OrgSelectPanel';
import ProjectAdjustListtReviewQueryForm from './ProjectAdjustListReviewQueryForm';
import { FormatDate, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { openProjectPrint } from '../../../Basic/CommonService';

const ProjectAdjustListReviewMain = (props) => {
    // 組織樹資料
    const [orgTreeData, setOrgTreeData] = useState([]);
    // 查詢條件
    const queryCondition = useRef({});
    // 查詢條件開關控制
    const [queryConditionVisible, setQueryConditionVisible] = useState(false);
    // gridData
    const [gridData, setGridData] = useState([]);
    // grid分頁
    const [paging, setPaging] = useState({ skip: 0, take: 20 });
    // 匯出Grid資料
    const GridDataForExport = useRef({ props: {}, Columns: [] });
    // grid排序
    const [sort, setSort] = useState([{ field: "", dir: "", },]);
    // ---------異動紀錄------------
    // 異動紀錄visibility
    const [projLogListVisible, setProjLogListVisible] = useState(false);
    // 異動紀錄計畫編號
    const [projLogData, setProjLogData] = useState({
        projectNo: "",
        projectName: ""
    });

    /**
     * 取得資料
     * @param {*} queryData 篩選條件
     * @param {boolean} isResetOrgTree 是否重新產生組織樹
     */
    const loadData = async (queryData, isResetOrgTree = true) => {
        SetMaskOnOff(true);
        queryCondition.current = queryData;
        let result = await getProjectList(queryData);
        // 產生組織樹資料
        if (isResetOrgTree) {
            setOrgTreeData(genOrgTreeData([...result]));
        }
        setGridData(result);
        // 設置 grid 頁碼回第一頁
        setPaging({ ...paging, skip: 0 });
        SetMaskOnOff(false);
    }

    // 執行機關選擇事件callback
    const onSelect = async (ouId) => {
        let queryData = {
            ...queryCondition.current,
            EXEC_ORGAN_C: ouId
        };
        queryCondition.current = queryData;
        await loadData(queryData, false);
    }

    // 計畫名稱cell
    const projectNameCell = props => {
        return (
            <td style={{ minWidth: '200px' }}>
                <a onClick={() => { openProjectPrint(props.dataItem, "list", 2) }}>{props.dataItem.PROJECT_NAME}</a>
            </td>
        )
    }

    /**
     * 定義指令欄 -審核按鈕
     * @param {*} props Grid傳入的參數
     * @returns CommandCell
     */
    const _CommandCell = (props) => {
        // PROJECT_AW_STATUS = 'A02', 'B02', 'W02' 才可編修
        let projAwStatus = props.dataItem.PROJECT_AW_STATUS;
        let allowEditStatus = ['A02', 'B02', 'W02'];
        let allowEdit = allowEditStatus.findIndex(status => status == projAwStatus);
        if (allowEdit != -1) {
            return (
                <CommandCell>
                    <Button title={"審核"} icon='edit' look='default' onClick={async () => {
                        // 開啟新分頁
                        // AW_KIND = 'AW01' 開啟基本資料調整 /ProjectAdjustBasic/BasicAuditReason
                        // AW_KIND = 'AW02' 開啟期程調整 /ProjectAdjustSchedule/ScheduleAuditReason
                        let params = {
                            funRole: 2, // 依管考面向
                            ...props.dataItem,
                        }
                        openAdjustProjectChapter(params);
                    }} />
                </CommandCell>
            )
        }
        else {
            return <td></td>
        }
    }

    // 調整撤銷狀態cell
    const projAwStatusCell = (props) => {
        // PROJECT_AW_STATUS = 
        // ”調整不予同意(基本資料)”、”已調整(基本資料)”、” 調整不予同意(期程)”、”已調整(期程)”、”撤銷不予同意”、”已撤銷”
        // 才可開啟章節表預覽
        let projAwStatus = props.dataItem.PROJECT_AW_STATUS;
        let openChapterStatus = ['A04', 'A05', 'B04', 'B05', 'W04', 'W05', 'B07'];
        let openChapter = openChapterStatus.findIndex(status => status == projAwStatus);
        if (openChapter != -1) {
            return (
                <td>
                    <a onClick={() => {
                        // 開啟新分頁
                        // AW_KIND = 'AW01' 開啟基本資料調整審核 /ProjectAdjustBasicReview
                        // AW_KIND = 'AW02' 開啟期程調整 /ProjectAdjustSchedule/ScheduleExecReason
                        // AW_KIND = 'AW03' 開啟撤銷審核 /ProjectAdjustRevokeReview
                        let params = {
                            funRole: props.dataItem.AW_KIND == 'AW02' ? 0 : 2,
                            isShowBtn: false,
                            ...props.dataItem,
                        }
                        openAdjustProjectChapter(params);
                    }}>{props.dataItem.PROJECT_AW_STATUS_NAME}</a>
                </td>
            )
        }
        else {
            return <td>{props.dataItem.PROJECT_AW_STATUS_NAME}</td>
        }
    }

    // 匯出
    const exportPlan = async (extension) => {
        // @ts-ignore
        GridDataForExport.current.Columns = GridDataForExport.current.Columns.filter(x => x.title != "審查");
        ExportGrid(GridDataForExport.current, extension, '計畫清單');
    }

    // 異動紀錄cell
    const openLogListCell = prop => {
        const projectNo = prop.dataItem.PROJECT_NO
        const param = {
            projectNo: projectNo,
            projectName: prop.dataItem.PROJECT_NAME
        }
        return (
            <td style={{ 'textAlign': 'center' }}>
                <a onClick={() => { openLogList(param) }}>{projectNo}</a>
            </td>
        )
    }

    // 開啟異動紀錄window
    const openLogList = (param) => {
        setProjLogData({ ...param });
        setProjLogListVisible(true);
    }

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    return (
        <>
            <h3 className="k-dialog-titlebar">計畫審查-調整撤銷審查</h3>
            <div style={{ 'display': 'flex' }}>
                <OrgSelectPanel
                    data={orgTreeData}
                    onSelect={onSelect}
                    title={'執行機關'}
                    style={{ overflow: "auto" }}
                />
                <PageContainer
                    toolbar={
                        <>
                            <Button title={queryConditionVisible ? "隱藏篩選計畫" : "篩選計畫"}
                                className='k-button-lighten'
                                onClick={() => setQueryConditionVisible(!queryConditionVisible)}>
                                {queryConditionVisible ? "隱藏篩選計畫" : "篩選計畫"}
                            </Button>
                            <Button title="匯出Excel" className="k-button-lighten" disabled={gridData.length === 0} onClick={() => exportPlan('xlsx')} >匯出Excel</Button>
                            <Button title="匯出Ods" className="k-button-lighten" disabled={gridData.length === 0} onClick={() => { exportPlan('ods') }} >匯出Ods</Button>
                            <Button title="重新整理" onClick={() => loadData({ ...queryCondition.current, EXEC_ORGAN_C: "" })} className="k-button-lighten" >重新整理</Button>
                        </>
                    }
                    style={{ height: "calc(100vh - 129px)" }}
                >
                    <div style={{ display: queryConditionVisible ? 'inline' : 'none' }}>
                        <ProjectAdjustListtReviewQueryForm
                            loadData={loadData}
                        />
                    </div>

                    <Grid
                        data={gridData.slice(paging.skip, paging.take + paging.skip)}
                        exportData={gridData}
                        style={{
                            textAlign: "center",
                            height: "100%",
                            overflow: 'auto',
                        }}
                        ref={(e) => {
                            if (e != null) {
                                // @ts-ignore
                                GridDataForExport.current.Columns = e.columns;
                                GridDataForExport.current.props = e.props;
                            }
                        }}
                        // @ts-ignore
                        sort={sort}
                        onSortChange={sortChange}
                        sortable={{ allowUnsort: true, mode: "single" }}
                        total={gridData.length}
                        skip={paging.skip}
                        take={paging.take}
                        pageable={Pageable}
                        onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
                    >
                        <GridNoRecords>無資料</GridNoRecords>
                        <GridColumn cell={_CommandCell} title="審查" width="50px" />
                        <GridColumn field="PROJECT_AW_STATUS_NAME" title="調整撤銷狀態" width="150px" cell={projAwStatusCell} />
                        <GridColumn field="PROJECT_YEAR" title="年度" width="50px" cell={(props) =>
                            <td style={{ textAlign: "center" }}>{props.dataItem.PROJECT_YEAR}</td>} />
                        <GridColumn field="PROJECT_NO" title="計畫編號" width="100px" cell={openLogListCell} />
                        <GridColumn field="PROJECT_NAME" title="計畫名稱" cell={projectNameCell} />
                        <GridColumn field="BUDGET" title="計畫總經費" width="100px" cell={(props) =>
                            <td style={{ textAlign: "right" }}>{formatNumber(props.dataItem.BUDGET, "n0")}</td>} />
                        <GridColumn field="MASTER_ORG_NAME" title="主管機關" width="100px" />
                        <GridColumn field="EXEC_ORG_NAME" title="執行機關" width="100px" />
                        <GridColumn field="MDF_DATE" title="最後異動時間" width="110px" cell={(props) =>
                            <td style={{ textAlign: "center" }}>{FormatDate(props.dataItem.MDF_DATE)}</td>} />
                    </Grid>

                    {/* 異動紀錄 */}
                    <ProjectLogListWindow
                        visible={projLogListVisible}
                        projectData={projLogData}
                        onClose={() => { setProjLogListVisible(false) }}
                    />

                </PageContainer>
            </div>
        </>
    )
}

export default ProjectAdjustListReviewMain;
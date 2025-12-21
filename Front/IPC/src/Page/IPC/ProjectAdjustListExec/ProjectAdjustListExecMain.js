import React, { useState, useEffect, useRef } from 'react';
import { Pageable } from '../../../Basic/BasicData';
import { PageContainer } from "../../../Basic/PageContainer";
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { FormatDate, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { Button } from "@progress/kendo-react-buttons";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { orderBy } from "@progress/kendo-data-query";
import { Window } from '@progress/kendo-react-dialogs';
import { formatNumber } from '@telerik/kendo-intl';
import { projAwStatusDefaultList, getPageData, getProjectList, SaveExecCancel, openAdjustProjectChapter } from './ProjectAdjustListExecService';
import ProjectAdjustListExecQueryForm from './ProjectAdjustListExecQueryForm';
import ProjectRevokeMain from '../ProjectAdjustRevoke/ProjectAdjustRevokeMain';
import ChooseProject from './ChooseProject';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import ProjectLogListWindow from '../ProjectLogList/ProjectLogListWindow';
import { openProjectPrint } from '../../../Basic/CommonService';

const ProjectAdjustListExecMain = (props) => {
    // 是否為初次載入
    const isFirstLoad = useRef(true);
    // 查詢條件
    const queryCondition = useRef({});
    // 查詢條件開關控制
    const [queryConditionVisible, setQueryConditionVisible] = useState(false);

    const dimensions = WindowResizehook();
    // gridData
    const [gridData, setGridData] = useState([]);
    // grid分頁
    const [paging, setPaging] = React.useState({ skip: 0, take: 20 });
    // grid排序
    const [sort, setSort] = React.useState([{ field: "", dir: "" }]);

    // 申請項目
    const [awKindList, setAwKindList] = useState([]);
    // 計畫撤銷視窗顯示狀態
    const [revokeWindowVisible, setRevokeWindowVisible] = React.useState(false);
    // 計畫撤銷視窗所需 PROJ_ADJ_ID ，null 為新增，否則為修改 (退回修正用)
    const projAdjIdForRevoke = useRef(null);
    // 顯示申請視窗 (for 基本資料: AW01、期程調整: AW02)
    const [applyWindow, setApplyWindow] = React.useState('');

    // 異動紀錄visibility
    const [projLogListVisible, setProjLogListVisible] = useState(false);
    // 異動紀錄計畫編號
    const [projLogData, setProjLogData] = useState({
        projectNo: "",
        projectName: ""
    });

    /**
     * 取得頁面資料
     * @param {*} queryData 篩選條件
     */
    const loadData = async (queryData) => {
        SetMaskOnOff(true);
        queryCondition.current = queryData;
        let gridDataList = [];
        if (isFirstLoad.current) {
            let result = await getPageData({ ...queryData, PROJECT_AW_STATUS: projAwStatusDefaultList });
            if (result && result.length > 0) {
                setAwKindList(result[0]);
                gridDataList = result[1];
            }

            isFirstLoad.current = false;
        } else {
            gridDataList = await getProjectList(queryData);
        }
        setGridData(gridDataList);
        SetMaskOnOff(false);
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
     * 定義指令欄 -編修按鈕
     * @param {*} props Grid傳入的參數
     * @returns CommandCell
     */
    const _CommandCell = (props) => {
        // PROJECT_AW_STATUS = 'A01', 'A03', 'B01', 'B03','W01','W03' 才可編修
        let projAwStatus = props.dataItem.PROJECT_AW_STATUS;
        let allowEditStatus = ['A01', 'A03', 'B01', 'B03', 'W01', 'W03'];
        let allowEdit = allowEditStatus.findIndex(status => status == projAwStatus);
        if (allowEdit != -1) {
            return (
                <CommandCell>
                    <Button title={"編修"} icon='edit' look='default' onClick={async () => {
                        if (props.dataItem.AW_KIND == "AW03") {
                            projAdjIdForRevoke.current = props.dataItem.PROJ_ADJ_ID;
                            setRevokeWindowVisible(true);
                        }
                        else {
                            // 開啟新分頁
                            // AW_KIND = 'AW01' 開啟基本資料調整 /ProjectAdjustBasic/BasicExecReason
                            // AW_KIND = 'AW02' 開啟期程調整 /ProjectAdjustSchedule/ScheduleExecReason
                            let params = {
                                funRole: 0, // 依主辦面向
                                ...props.dataItem,
                            }
                            openAdjustProjectChapter(params);
                        }

                    }} />
                </CommandCell>
            )
        }
        else {
            return <td></td>
        }
    }

    /**
     * 定義指令欄 -取消申請按鈕
     * @param {*} props Grid傳入的參數
     * @returns CommandCell
     */
    const _CancelCommandCell = (props) => {
        // PROJECT_AW_STATUS = 'A01'、 'B01' 才可取消申請
        let projAwStatus = props.dataItem.PROJECT_AW_STATUS;
        let allowCancelStatus = ['A01', 'B01'];
        let allowCancel = allowCancelStatus.findIndex(status => status == projAwStatus);
        if (allowCancel != -1) {
            return (
                <CommandCell>
                    <Button title={"取消申請"} icon='edit' look='default' onClick={() => {
                        showGlobalConfirmBox("請確認是否取消申請，確認後不保留此次申請調整填報內容", async () => {
                            let saveResult = await SaveExecCancel(props.dataItem.PROJECT_NO, props.dataItem.PROJ_ADJ_ID, props.dataItem.AW_KIND);

                            if (saveResult.success) {
                                showGlobalMessageBox(saveResult.message, () => {
                                    loadData(queryCondition.current);
                                });
                            }
                        });
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
        // "期程調整通過"
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
        <PageContainer
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">計畫填報-調整撤銷</h3>
                    <Button title="基本資料調整" onClick={() => { setApplyWindow('AW01'); }} className="k-button-lighten">基本資料調整</Button>
                    <Button title="期程調整" onClick={() => { setApplyWindow('AW02'); }} className="k-button-lighten">期程調整</Button>
                    <Button title="計畫撤銷" onClick={() => { setRevokeWindowVisible(true); }} className="k-button-lighten">計畫撤銷</Button>
                    <Button title={queryConditionVisible ? "隱藏篩選計畫" : "篩選計畫"}
                        onClick={() => setQueryConditionVisible(!queryConditionVisible)}>
                        {queryConditionVisible ? "隱藏篩選計畫" : "篩選計畫"}
                    </Button>
                    <Button title="重新整理" onClick={() => loadData(queryCondition.current)}>重新整理</Button>
                </>
            }
        >
            {/* 篩選表單 */}
            <div style={{ display: queryConditionVisible ? 'inline' : 'none' }}>
                <ProjectAdjustListExecQueryForm
                    loadData={loadData}
                    paging={paging}
                    setPaging={setPaging}
                    visible={queryConditionVisible}
                />
            </div>

            <Grid
                data={gridData.slice(paging.skip, paging.take + paging.skip)}
                style={{
                    textAlign: "center",
                    height: '100%',
                    overflow: 'auto',
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
                <GridColumn cell={_CommandCell} title="編修" width="50px" />
                <GridColumn field="PROJECT_AW_STATUS_NAME" title="調整撤銷狀態" width="150px" cell={projAwStatusCell} />
                <GridColumn field="PROJECT_YEAR" title="年度" width="50px" cell={(props) =>
                    <td style={{ textAlign: "center" }}>{props.dataItem.PROJECT_YEAR}</td>} />
                <GridColumn field="PROJECT_NO" title="計畫編號" width="100px" cell={openLogListCell} />
                <GridColumn field="PROJECT_NAME" title="計畫名稱" cell={projectNameCell} />
                <GridColumn field="BUDGET" title="計畫總經費" width="100px" cell={(props) =>
                    <td style={{ textAlign: "right" }}>{formatNumber(props.dataItem.BUDGET, "n0")}</td>} />
                <GridColumn field="MASTER_ORG_NAME" title="主管機關" width="100px" />
                <GridColumn field="EXEC_ORG_NAME" title="執行機關" width="100px" />
                <GridColumn cell={_CancelCommandCell} title="取消申請" width="70px" />
                <GridColumn field="MDF_DATE" title="最後異動時間" width="110px" cell={(props) =>
                    <td style={{ textAlign: "center" }}>{FormatDate(props.dataItem.MDF_DATE)}</td>} />
            </Grid>

            {!IsNullOrEmpty(applyWindow) &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title={"選擇計畫"}
                        onClose={() => setApplyWindow('')}
                        width={dimensions.width * 0.5}
                        height={dimensions.height * 0.4}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <ChooseProject
                            closeWindow={() => setApplyWindow('')}
                            awKind={awKindList.find(item => item.SET_TYPE == applyWindow)}
                            loadListData={() => loadData(queryCondition.current)}
                        />
                    </Window>
                </div>
            }

            {revokeWindowVisible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title={"計畫撤銷事由"}
                        onClose={() => { projAdjIdForRevoke.current = null; setRevokeWindowVisible(false); }}
                        width={dimensions.width * 0.5}
                        height={dimensions.height * 0.9}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <ProjectRevokeMain
                            closeWindow={() => { projAdjIdForRevoke.current = null; setRevokeWindowVisible(false); }}
                            loadData={() => loadData(queryCondition.current)}
                            awKind={awKindList.find(item => item.SET_TYPE == "AW03")}
                            projAdjId={projAdjIdForRevoke.current}
                        />
                    </Window>
                </div>
            }

            {/* 異動紀錄 */}
            <ProjectLogListWindow
                visible={projLogListVisible}
                projectData={projLogData}
                onClose={() => { setProjLogListVisible(false) }}
            />

        </PageContainer>
    )
}

export default ProjectAdjustListExecMain;
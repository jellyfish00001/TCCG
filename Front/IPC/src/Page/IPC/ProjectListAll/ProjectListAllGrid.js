import React, { useEffect, useState } from 'react';
import '../../../Css/custom/Grid-Td-WordWrap.css';
import Table from '../../../Css/custom/Table.module.css';
import { openProjectPrint } from '../../../Basic/CommonService';
import { Pageable } from '../../../Basic/BasicData';
import { CommandCell } from '../../../Components/GridCell/CommandCell';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { formatNumber } from '@telerik/kendo-intl';
import { orderBy } from "@progress/kendo-data-query";
import { openProjectChapter } from '../ProjectList/ProjectListService';
import ProjectLogListWindow from '../ProjectLogList/ProjectLogListWindow';
import { openAdjustProjectChapter } from '../ProjectAdjustListExec/ProjectAdjustListExecService';

const ProjectListAllGrid = (props) => {
    let { listType, data } = props

    // gridData
    const [gridData, setGridData] = useState([]);
    // grid分頁
    const [paging, setPaging] = React.useState({ skip: 0, take: 20 });

    // 異動紀錄visibility
    const [projLogListVisible, setProjLogListVisible] = useState(false);
    // 異動紀錄計畫編號
    const [projLogData, setProjLogData] = useState({
        projectNo: "",
        projectName: ""
    });
    // grid排序
    const [sort, setSort] = React.useState([
        {
            field: "",
            dir: "",
        },
    ]);

    /**
     * 定義指令欄 -編修按鈕
     * @param {*} props Grid傳入的參數
     * @returns commendCell Jsx
     */
    const commandCell = (props) => {
        return (
            <CommandCell>
                <Button title={listType === 'Add' ? "編修" : "審查"} icon='edit' look='default' onClick={() => {
                    // 編修:依主辦面向 ; 審核:依管考面向
                    if (listType === "Add") {
                        openProjectChapter(props.dataItem, 0);
                    }
                    else {
                        if (props.dataItem.PROJECT_STATUS_C === "2" || props.dataItem.PROJECT_STATUS_C === "5") {
                            openProjectChapter(props.dataItem, 2);
                        }
                        else {
                            let params = {
                                funRole: 2, // 依管考面向
                                ...props.dataItem,
                            }
                            openAdjustProjectChapter(params);
                        }
                    }
                }} />
            </CommandCell>
        )
    }

    // 異動紀錄cell
    const openLogListCell = (prop) => {
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
     * 作業階段 or 審查作業
     * @param {*} prop 
     */
    const statusCell = (prop) => {
        if (listType === "Add" || prop.dataItem.PROJECT_STATUS_C === "2" || prop.dataItem.PROJECT_STATUS_C === "5") {
            return <td>{prop.dataItem.PROJECT_STATUS}</td>
        }
        else {
            return <td>{prop.dataItem.PROJECT_AW_STATUS}</td>
        }
    }

    /**
     * 開啟計畫預覽列印
     * @param {*} prop 
     * @returns 
     */
    const openProjectPrintCell = (prop) => {
        return (
            <td>
                <a onClick={() => { openProjectPrint(prop.dataItem, "list", listType === 'Add' ? 0 : 2) }}>
                    {prop.dataItem.PROJECT_NAME}
                </a>
            </td>
        )
    }

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    useEffect(() => {
        setGridData([...data]);
    }, [data])

    return (
        <>
            <h3 className="k-dialog-titlebar">{listType === 'Add' ? "計畫填報" : "計畫審查"}</h3>
            <Grid
                data={gridData.slice(paging.skip, paging.take + paging.skip)}
                style={{
                    height: '100%',
                    overflow: 'auto',
                }}
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
                <GridColumn cell={commandCell} title={listType === 'Add' ? "編修" : "審查"} width="50px" />
                <GridColumn cell={statusCell} title={listType === 'Add' ? "作業階段" : "審查作業"} width="100px" />
                <GridColumn field="PROJECT_NO" cell={openLogListCell} width='100px' title="計畫編號" />
                <GridColumn field="PROJECT_YEAR" className={Table.textAlign_center} width='70px' title="年度" />
                <GridColumn field="PROJECT_NAME" title="計畫名稱" cell={openProjectPrintCell} />
                <GridColumn width='100px' title="計畫總經費" cell={(prop) => (
                    <td className={Table.textAlign_right}>{formatNumber(prop.dataItem.BUDGET_TOTAL, "n0")}</td>
                )} />
                <GridColumn field="MASTER_ORGAN_NAME" title="主管機關" width='100px' />
                <GridColumn field="EXEC_ORGAN_NAME" title="執行機關" width='100px' />
            </Grid>

            <ProjectLogListWindow
                visible={projLogListVisible}
                projectData={projLogData}
                onClose={() => { setProjLogListVisible(false) }}
            />
        </>
    )
}

export default ProjectListAllGrid;
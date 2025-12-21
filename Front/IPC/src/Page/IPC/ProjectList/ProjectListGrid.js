import React, { useEffect, useState } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Pageable } from '../../../Basic/BasicData';
import { orderBy } from "@progress/kendo-data-query";
import '../../../Css/custom/Grid-Td-WordWrap.css';
import { updateProjectPisSelect } from './ProjectListService';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { formatNumber } from '@telerik/kendo-intl';
import Table from '../../../Css/custom/Table.module.css'
import ProjectLogListWindow from '../ProjectLogList/ProjectLogListWindow';
import { openProjectPrint } from '../../../Basic/CommonService';


// 資料登錄、計畫審查清單 共用Grid元件
const ProjectListGrid = (props) => {
    let { additionalCmdCols, data, GridDataForExport, reloadGrid, height, funRole } = props

    // gridData
    const [gridData, setGridData] = useState([]);
    // grid分頁
    const [paging, setPaging] = useState({ skip: 0, take: 20 });

    // grid排序
    const [sort, setSort] = useState([
        {
            field: "",
            dir: "",
        },
    ]);

    // 異動紀錄visibility
    const [projLogListVisible, setProjLogListVisible] = useState(false);
    // 異動紀錄計畫編號
    const [projLogData, setProjLogData] = useState({
        projectNo: "",
        projectName: ""
    });



    useEffect(() => {
        setGridData([...data])
        setPaging({...paging, skip: 0});
    }, [data])

    // 釘選狀態Cell
    const pisSelectCell = props => {
        return (
            <td className="text-center" style={{ 'cursor': 'pointer', 'width': '60px' }} onClick={() => { pisSelectProject(props.dataItem.PROJECT_NO, !props.dataItem.PIS_SELECT) }}>
                <i className={`fa ${props.dataItem.PIS_SELECT ? "fa-star_Yellow" : "fa-star-o_Yellow"}`}></i>
            </td>
        )
    }

    // 更新計畫訂選狀態
    const pisSelectProject = async (projectNo, pisSelectVal) => {
        let result = await updateProjectPisSelect(projectNo, pisSelectVal);
        if (result.success) {
            showGlobalMessageBox(result.message, () => { reloadGrid() })
        }
    }

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    // 傳入之功能Grid欄位
    let additionalCmdColsObj = additionalCmdCols ? additionalCmdCols.map(item =>
        <GridColumn cell={item.cell} title={item.title} width={item.width ? item.width : "50px"}
        />) : {}

    const projectNameCell = props => {
        return (
            <td style={{ minWidth: '200px' }}>
                <a onClick={() => { openProjectPrint(props.dataItem, "list", funRole) }}>{props.dataItem.PROJECT_NAME}</a>
            </td>
        )
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


    return (
        <>
            <Grid
                data={gridData.slice(paging.skip, paging.take + paging.skip)}
                exportData={gridData}
                style={{
                    textAlign: "center",
                    height: height ? height : '100%',
                    overflow: 'auto',
                }}
                sort={sort}
                onSortChange={sortChange}
                sortable={{ allowUnsort: true, mode: "single" }}
                ref={(e) => {
                    if (e != null) {
                        GridDataForExport.current.Columns = e.columns;
                        GridDataForExport.current.props = e.props;
                    }
                }}
                total={gridData.length}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
            >
                <GridNoRecords>無資料</GridNoRecords>
                {/* 傳入之功能Grid欄位 */}
                {additionalCmdCols && additionalCmdColsObj}
                <GridColumn field="PROJECT_STATUS" headerClassName={'breakSpacehHeader'} title="作業階段" width="85px" />
                <GridColumn field="PROJECT_YEAR" className={Table.textAlign_center} width="50px" title="年度" />
                <GridColumn field="PROJECT_NO" width="100px" cell={openLogListCell} title="計畫編號" />
                <GridColumn field="PROJECT_NAME" headerClassName={'breakSpacehHeader'} cell={projectNameCell} title="計畫名稱" />
                <GridColumn
                    cell={(prop) => {
                        return <td className={Table.textAlign_right}>{formatNumber(prop.dataItem.BUDGET_TOTAL, "n0")}</td>
                    }}
                    title="計畫總經費" width={"100px"} />
                <GridColumn field="MASTER_ORGAN_NAME" title="主管機關" width={"100px"} />
                <GridColumn field="EXEC_ORGAN_NAME" title="執行機關" width={"100px"} />
                <GridColumn field="PIS_SELECT" title="釘選" cell={pisSelectCell} width={'50px'} />
            </Grid>

            <ProjectLogListWindow
                visible={projLogListVisible}
                projectData={projLogData}
                onClose={() => { setProjLogListVisible(false) }}
            />
        </>
    )
}

export default ProjectListGrid;
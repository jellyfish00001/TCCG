import React from "react";
import service from "./SuperiorAnalysisService";
import { updateProjectPisSelect } from "../ProjectList/ProjectListService.js";
import { PageContainer } from "../../../Basic/PageContainer";
import { Pageable } from "../../../Basic/BasicData";
import { ExportGrid } from '../../../Basic/Download';
import { AddNoColumn } from "../../../Basic/SDOExtension";
import { openProjectPrint } from '../../../Basic/CommonService';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { Button } from "@progress/kendo-react-buttons";
import { Grid, GridColumn, GridNoRecords } from "@progress/kendo-react-grid";
import { orderBy } from "@progress/kendo-data-query";
import { formatNumber } from "@telerik/kendo-intl";

const PlanListGrid = (props) => {
    const { mainFormData } = props;

    // gridData
    const [gridData, setGridData] = React.useState([]);
    // grid分頁
    const [paging, setPaging] = React.useState({ skip: 0, take: 20 });
    // grid排序
    const [sort, setSort] = React.useState([{ field: "", dir: "" }]);
    // 匯出Grid資料
    const GridDataForExport = React.useRef({ props: {}, Columns: [] })

    React.useEffect(() => {
        getData();
    }, [])

    const getData = async () => {
        setGridData([...AddNoColumn(await service.getAnalyzePlan(mainFormData))]);
    }

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    // 匯出
    const exportRPT = async (extension) => {
        GridDataForExport.current.Columns = GridDataForExport.current.Columns.filter(x => x.title !== "項" && x.title !== "釘選");
        ExportGrid(GridDataForExport.current, extension, '決策分析計畫清單');
    }

    // 更新關心個案狀態
    const pisSelectProject = async (projectNo, pisSelectVal) => {
        let result = await updateProjectPisSelect(projectNo, pisSelectVal);
        if (result.success) {
            showGlobalMessageBox(result.message, () => { getData() })
        }
    }

    return (
        <PageContainer
            toolbar={
                <>
                    <Button title="匯出Excel" className="k-button-lighten" onClick={() => { exportRPT("excel") }} >匯出Excel</Button>
                    <Button title="匯出Ods" className="k-button-lighten" onClick={() => { exportRPT("ods") }}>匯出Ods</Button>
                </>
            }
        >
            <Grid
                data={gridData.slice(paging.skip, paging.take + paging.skip)}
                exportData={gridData}
                style={{
                    textAlign: "center",
                    height: "100%",
                    overflow: "auto",
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
                ref={(e) => {
                    if (e != null) {
                        // @ts-ignore
                        GridDataForExport.current.Columns = e.columns;
                        GridDataForExport.current.props = e.props;
                    }
                }}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="NO" title="項" width={50} className="text-center" />
                <GridColumn field="PROJECT_NO" title="計畫編號" width={150} />
                <GridColumn field="PROJECT_NAME" title="計畫名稱" cell={(item) => (
                    <td>
                        <a onClick={() => { openProjectPrint(item.dataItem, "list", 0) }}>{item.dataItem.PROJECT_NAME}</a>
                    </td>
                )} />
                <GridColumn field="PROJ_BUDGET" title="計畫總經費" width={120} cell={(item) => (
                    <td className="text-right">
                        {formatNumber(item.dataItem.PROJ_BUDGET, "N0")}
                    </td>
                )} />
                <GridColumn field="PRG_OFFSET_DESC" title="計畫進度" width={100} />
                <GridColumn field="MASTER_ORGAN_NAME" title="主管機關" width={100} />
                <GridColumn field="EXEC_ORGAN_NAME" title="執行機關" width={100} />
                <GridColumn title="釘選" width={50} cell={(item) => (
                    <td className="text-center" onClick={() => {
                        pisSelectProject(item.dataItem.PROJECT_NO, !item.dataItem.PIS_SELECT)
                    }}>
                        <i className={`fa ${item.dataItem.PIS_SELECT === true ? "fa-star_Yellow" : "fa-star-o_Yellow"}`}></i>
                    </td>
                )} />
            </Grid>
        </PageContainer>
    )
}

export default PlanListGrid;
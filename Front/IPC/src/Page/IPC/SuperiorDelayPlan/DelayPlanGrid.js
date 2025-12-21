import React from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { orderBy } from "@progress/kendo-data-query";
import { openProjectPrint } from '../../../Basic/CommonService';

const DelayPlanGrid = (props) => {
    const { gridData, setGridData, GridDataForExport, pisSelectProject } = props;

    // grid排序
    const [sort, setSort] = React.useState([{ field: "", dir: "" }]);

    // 切換排序
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    return (
        <Grid
            data={gridData}
            exportData={gridData}
            style={{
                textAlign: 'center',
                height: '100%',
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
        >
            <GridNoRecords>無資料</GridNoRecords>
            <GridColumn field="NO" title="項" width={50} cell={(item) => (
                <td className="text-center">{item.dataIndex + 1}</td>
            )} />
            <GridColumn field="PROJECT_NO" title="計畫編號" width={150} />
            <GridColumn field="PROJECT_NAME" title="計畫名稱" cell={(item) => (
                <td>
                    <a onClick={() => { openProjectPrint(item.dataItem, "list", 0) }}>{item.dataItem.PROJECT_NAME}</a>
                </td>
            )} />
            <GridColumn field="MASTER_ORGAN_NAME" title="主管機關" width={100} />
            <GridColumn field="EXEC_ORGAN_NAME" title="執行機關" width={100} />
            <GridColumn field="OFFSET_REMARK" title="落後原因" width={100} />
            <GridColumn title="釘選" width={50} cell={(item) => (
                <td className="text-center" onClick={() => {
                    pisSelectProject(item.dataItem.PROJECT_NO, !item.dataItem.PIS_SELECT)
                }}>
                    <i className={`fa ${item.dataItem.PIS_SELECT === true ? "fa-star_Yellow" : "fa-star-o_Yellow"}`}></i>
                </td>
            )} />
        </Grid>
    )

}

export default DelayPlanGrid;
import React from 'react';
import { openProjectPrint } from '../../../Basic/CommonService';
import { ExportGrid } from '../../../Basic/Download';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { formatNumber } from "@telerik/kendo-intl";
import { Button } from "@progress/kendo-react-buttons";

const ListTab = (props) => {
    const { listData } = props;

    // 匯出Grid資料
    const GridDataForExport = React.useRef({ props: {}, Columns: [] });

    // 匯出
    const exportRPT = async (extension) => {
        GridDataForExport.current.Columns = GridDataForExport.current.Columns.filter(x => x.title !== "序號");
        ExportGrid(GridDataForExport.current, extension, '決策分析計畫清單');
    }

    return (
        <div style={{ backgroundColor: '#E0E0E0', padding: '5px 10px' }}>
            <h5>基本資料</h5>
            <div className='fn-buttons'>
                <Button title="匯出Excel" className='k-button-lighten' onClick={() => { exportRPT("excel") }} >匯出Excel</Button>
            </div>
            <div style={{ height: '500px', width: '100%', display: 'flex', backgroundColor: 'white' }}>
                <Grid
                    data={listData}
                    exportData={listData}
                    style={{
                        textAlign: 'center',
                        height: '100%',
                        overflow: 'auto',
                    }}
                    sortable={{ allowUnsort: true, mode: "single" }}
                    ref={(e) => {
                        if (e != null) {
                            GridDataForExport.current.Columns = e.columns;
                            GridDataForExport.current.props = e.props;
                        }
                    }}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn field="NO" title="序號" width={50} cell={(item) => (
                        <td className="text-center">{item.dataIndex + 1}</td>
                    )} />
                    <GridColumn field="PROJECT_NAME" title="專案名稱" cell={(item) => (
                        <td>
                            <a onClick={() => openProjectPrint(item.dataItem, "list", 0)}>{item.dataItem.PROJECT_NAME}</a>
                        </td>
                    )} />
                    <GridColumn field="EXEC_ORGAN_NAME" title="執行機關" width={100} />
                    <GridColumn field="PROJ_BUDGET" title="總經費" width={120} cell={(item) => (
                        <td className="text-right">
                            {formatNumber(item.dataItem.PROJ_BUDGET, "N0")}
                        </td>
                    )} />
                </Grid>
            </div>
        </div>
    )
}
export default ListTab;
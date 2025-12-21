import React, { useState, useRef, useEffect } from "react";
import { Window } from '@progress/kendo-react-dialogs';
import { WindowResizehook } from '../../../../Hook/useWindowResize';
import { Button } from '@progress/kendo-react-buttons';
import { PageContainer } from "../../../../Basic/PageContainer";
import Table from '../../../../Css/custom/Table.module.css';
import { getDelayList } from '../DashBoardService';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { ExportGrid } from '../../../../Basic/Download';

const DelayListWindow = (props) => {
    let { visible, onClose, DAB_YEAR_YYY, DAB_MONTH } = props;
    // 視窗大小
    const dimensions = WindowResizehook();
    // Grid資料
    const [gridData, setGridData] = useState([]);
    // 匯出Grid資料
    const GridDataForExport = useRef({ props: {}, Columns: [] })

    /**
     * 取得資料
     */
    const loadData = async () => {
        const model = {
            DAB_YEAR_YYY: DAB_YEAR_YYY,
            DAB_MONTH: DAB_MONTH
        };
        const result = await getDelayList(model);
        setGridData(result);
    };

    // 初始化
    useEffect(() => {
        if (visible) {
            loadData();
        }
    }, [visible]);

    // 自定義格式化函數，將數字格式化為千分位
    const numberWithCommas = (value) => {
        if (value !== null && value !== undefined) {
            return value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        }
        return value;
    };

    return (
        <>
            {visible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title='落後案件清單'
                        onClose={() => { onClose() }}
                        initialWidth={dimensions.width > 700 ? 800 : dimensions.width * .7}
                        initialHeight={dimensions.height > 700 ? 500 : dimensions.height * .8}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <PageContainer
                            style={{
                                height: '100%',
                                overflow: 'auto',
                            }}
                            toolbar={
                                <div>
                                    <Button title="匯出" onClick={() => ExportGrid(GridDataForExport.current, "xlsx", "落後案件清單")}>匯出</Button>
                                </div>
                            }
                        >
                            <Grid
                                data={gridData}
                                exportData={gridData}
                                style={{
                                    height: '100%',
                                    overflow: 'auto',
                                }}
                                ref={(e) => {
                                    if (e != null) {
                                        GridDataForExport.current.Columns = e.columns;
                                        GridDataForExport.current.props = e.props;
                                    }
                                }}
                            >
                                <GridNoRecords>無資料</GridNoRecords>
                                <GridColumn field="NO" title="序號" width="50px" />
                                <GridColumn field="PROJECT_NAME" title="計畫名稱" />
                                <GridColumn field="EXEC_DEPT" title="執行機關" />
                                <GridColumn
                                    field="BUDGET_TOTAL"
                                    title="計畫總經費"
                                    className={Table.textAlign_right}
                                    cell={(props) => (
                                        <td className={Table.textAlign_right}>
                                            {numberWithCommas(props.dataItem[props.field])}
                                        </td>
                                    )}
                                />
                                <GridColumn field="DELAY_TYPE" title="落後類型" />
                            </Grid>
                        </PageContainer>
                    </Window>
                </div>
            }
        </>
    );
};

export default DelayListWindow;

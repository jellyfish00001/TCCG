import React, { useState, useRef, useEffect } from "react";
import { Window } from '@progress/kendo-react-dialogs';
import { WindowResizehook } from '../../../../Hook/useWindowResize';
import { Button } from '@progress/kendo-react-buttons';
import { PageContainer } from "../../../../Basic/PageContainer";
import Table from '../../../../Css/custom/Table.module.css';
import { GetProjectDelayList } from '../DashBoardService';
import StatisticsService from '../../Statistics/StatisticsService';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { ExportGrid } from '../../../../Basic/Download';
import { IsNullOrEmpty, SetMaskOnOff } from "../../../../Basic/SDOExtension";

const ProjectDelayListWindow = (props) => {
    let { visible, onClose, DAB_YEAR_YYY, DAB_MONTH, name, Type } = props;
    // 視窗大小
    const dimensions = WindowResizehook();
    // Grid資料
    const [gridData, setGridData] = useState([]);
    // 匯出Grid資料
    const GridDataForExport = useRef({ props: {}, Columns: [] })

    const loadData = async () => {
        SetMaskOnOff(true);
        const model = {
            DAB_YEAR_YYY: DAB_YEAR_YYY,
            DAB_MONTH: DAB_MONTH,
            Type: Type
        };
        const result = await GetProjectDelayList(model);
        setGridData(result);
        SetMaskOnOff(false);
    };

    useEffect(() => {
        setGridData([]);
        if (visible) {
            loadData();
        }
    }, [visible]);

    return (
        <>
            {visible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title={name}
                        onClose={() => { onClose() }}
                        initialWidth={dimensions.width > 700 ? 1200 : dimensions.width * .9}
                        initialHeight={dimensions.height > 700 ? 700 : dimensions.height * .8}
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
                                    <Button title="匯出" onClick={() => ExportGrid(GridDataForExport.current, "xlsx", name)}>匯出</Button>
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
                                <GridColumn field="BUDGET_TOTAL" title="計畫總經費" />
                                <GridColumn field="BUDGET_CENTRAL" title="中央補助" />
                                <GridColumn field="BUDGET_LOCAL" title="地方自籌款" />
                                <GridColumn field="CHECKITEM_STATUS" title="檢核點(預定/實際)" />
                                <GridColumn field="DelayDate" title="進度落後天數" />
                                <GridColumn field="IPC_PRG_STATUS" title="工程進度(預定/實際)" />
                                <GridColumn field="DELAY_TYPE" title="落後類型" />
                                <GridColumn field="DELAY_CAUSAL" title="落後原因" />
                            </Grid>
                        </PageContainer>
                    </Window>
                </div>
            }
        </>
    );
};

export default ProjectDelayListWindow;

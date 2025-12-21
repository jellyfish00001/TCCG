import React, { useEffect, useState, useRef } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Pageable } from '../../../Basic/BasicData';
import '../../../Css/custom/Grid-Td-WordWrap.css';
import { downcompareDiff, getProjectLogList } from './ProjectLogListService';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import { ExportGrid } from '../../../Basic/Download';
import { Window } from '@progress/kendo-react-dialogs';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { Checkbox } from '@progress/kendo-react-inputs';
import { openDiffCompare } from '../../../Basic/CommonService';
const ProjectLogListWindow = (props) => {
    const { visible, projectData: { projectNo, projectName }, onClose } = props

    // gridData
    const [gridData, setGridData] = useState([]);
    // grid分頁
    const [paging, setPaging] = React.useState({ skip: 0, take: 20 });

    const dimensions = WindowResizehook();

    const [isLoaded, setIsLoaded] = useState(false);

    const [checkedLog, setCheckedLog] = useState([]);

    // 匯出Grid資料
    const GridDataForExport = React.useRef({ props: {}, Columns: [] })

    const [htmlString, setHtmlString] = useState("");

    // 取得計畫異動紀錄資料
    const loadData = async () => {
        SetMaskOnOff(true)
        let result = await getProjectLogList(projectNo);

        if (result) {
            setGridData([...result]);
            setIsLoaded(true)
        }
        setCheckedLog([])
        SetMaskOnOff(false);
    }

    // checkBox Cell
    const checkBoxCell = props => {
        const { dataItem, dataItem: { LOG_STATUS_C } } = props;
        // 特定狀態才可執行差異比對
        // 立案送審/調整審核通過
        if (LOG_STATUS_C == '2' || LOG_STATUS_C == 'B07' || LOG_STATUS_C == 'A05' || LOG_STATUS_C == 'B05') {
            return (
                <td style={{ 'textAlign': 'center' }}>
                    <Checkbox
                        checked={checkedLog.map(x => x.LOG_ID).includes(dataItem.LOG_ID.toString())}
                        value={dataItem.LOG_ID.toString()}
                        onChange={(e) => {
                            SetChecked(e, dataItem);
                        }}
                    />
                </td>
            )
        } else {
            return (
                <td></td>
            )
        }
    }

    //將勾選的資料放進checkedPlan
    const SetChecked = (e, data) => {
        let dataItem = [...checkedLog];
        let value = e.target.element.value
        if (e.value === true) {
            dataItem.push({
                LOG_ID: value,
                LOG_STATUS_C: data.LOG_STATUS_C
            });
        }
        else {
            dataItem = dataItem.filter(x => x.LOG_ID != value)
        }
        setCheckedLog(dataItem)
    }

    // 匯出
    const exportPlan = async (extension) => {
        GridDataForExport.current.Columns = GridDataForExport.current.Columns.filter(x => x.field != "LOG_ID");
        ExportGrid(GridDataForExport.current, extension, '計畫異動清單')
    }




    /**
     * 開啟差異比對結果
     */
    const diffCompare = () => {
        if (checkedLog.length != 2) {
            showGlobalMessageBox("請勾選2筆項目。")
            return;
        } else if (checkedLog.map(x => x.LOG_STATUS_C == "2").length != 2) {
            showGlobalMessageBox("只能勾選 主辦送審。")
            return;
        }

        let data = {
            projectNo: projectNo,
            projectName: projectName,
            isDiffCompare: true,
            checkedLog: checkedLog.map(x => x.LOG_ID).sort((a, b) => { return a - b; })
        }
        openDiffCompare(data)
    }

    // 關閉視窗
    const closeWindow = () => {
        setGridData([])
        setIsLoaded(false);
        onClose();
    }

    useEffect(() => {
        if (visible)
            loadData();
    }, [visible])


    return (
        <>
            {visible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title={`${projectName}-作業階段異動歷程`}
                        onClose={() => { closeWindow() }}
                        initialWidth={dimensions.width > 700 ? 1000 : dimensions.width * .8}
                        initialHeight={dimensions.height > 700 ? 600 : dimensions.height * .9}
                        draggable={false}
                        resizable={false}
                        modal={true}>
                        {isLoaded &&
                            <>
                                <div className='fn-buttons'>
                                    <Button className='k-button-lighten' title="匯出Excel" disabled={gridData.length === 0} onClick={() => exportPlan('xlsx')}>匯出Excel</Button>
                                    <Button className='k-button-lighten' title="匯出Ods" disabled={gridData.length === 0} onClick={() => { exportPlan('ods') }}>匯出Ods</Button>
                                    <Button className='k-button-lighten' title="差異比對" disabled={gridData.length === 0} onClick={() => { diffCompare() }}>差異比對</Button>
                                </div>
                                <Grid
                                    data={gridData.slice(paging.skip, paging.take + paging.skip)}
                                    exportData={gridData}
                                    style={{
                                        height: 'calc(100% - 35px)',
                                        overflow: 'auto',
                                    }}
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
                                    <GridColumn field="LOG_ID" cell={checkBoxCell} title="勾選" width="50px" />
                                    <GridColumn field="LOG_DATE_FORMAT" title="執行時間" width="200px" />
                                    <GridColumn field="PROJECT_STAGE" title="作業階段" />
                                    <GridColumn field="MASTER_ORGAN_NAME" title="主管機關" />
                                    <GridColumn field="EXEC_ORGAN_NAME" title="執行機關" />
                                    <GridColumn field="MDF_ORG_NAME" title="異動機關" />
                                    <GridColumn field="LOG_USER" title="異動人員" />
                                    <GridColumn field="LOG_STATUS" title="異動狀態" />
                                </Grid>
                            </>
                        }
                    </Window>
                </div>
            }
        </>
    )
}

export default ProjectLogListWindow;
import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Window } from '@progress/kendo-react-dialogs';
import { CommandCell } from "../../../../Components/GridCell/CommandCell";
import ProjectFillAuditService from '../ProjectFillAuditService';
import ProjectAuditOpinionGridWindow from './ProjectAuditOpinionGridWindow';
import { SetMaskOnOff, handleEditedGridData, FormatDate } from '../../../../Basic/SDOExtension';
import { WindowResizehook } from '../../../../Hook/useWindowResize';
import TextAreaWrapInput from '../../../../Components/Input/TextAreaWrapInput';
import { showGlobalMessageBox } from "../../../../Route/RootMiddleware";

export const ProjectAuditOpinionGrid = (props) => {
    const { projectNo, data, ipcMemoDdlData, planYearDdlData, editedGridData, isRdecFun } = props;

    const dimensions = WindowResizehook();

    const [gridData, setGridData] = React.useState([]);
    // 傳入跳窗資料
    const [windowData, setWindowData] = React.useState({});
    //視窗狀態
    const [windowStatus, setWindowStatus] = React.useState(false);

    // 紀錄上個月日期
    let lastMonDate = new Date();
    lastMonDate.setMonth(lastMonDate.getMonth() - 1, 1);

    // 新增預設值
    const initData = {
        PROJECT_NO: projectNo,
        SEQ: 0,
        YEAR: FormatDate(new Date(), "tYY"),
        MONTH: FormatDate(lastMonDate, "MM"),
        AUDIT_OPINION: "",
        ComIPCMemoMappingData: [],
        editType: 1
    }

    // 載入管考審核意見
    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await ProjectFillAuditService.getProjectFillAudit(projectNo);
        setGridData(result.ProjectEngineeringAuditOpinion);
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        setGridData(data);
    }, [data])

    /**
     * 刪除欄位
     * @param {*} prop 
     * @returns 
     */
    const DelCommandCell = (prop) => {
        return (
            <CommandCell>
                <Button title={"刪除"} icon='close' look='default' onClick={() => {
                    remove(prop.dataItem);
                }} />
            </CommandCell>
        )
    }

    // 移除
    const remove = async (dataItem) => {
        //找出grid要刪除的列
        let index = dataItem.hiddenIndex ?
            gridData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex)
            :
            gridData.findIndex(record => record.SEQ === dataItem.SEQ);

        gridData.splice(index, 1);
        dataItem.editType = 3;
        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'SEQ');
        setGridData([...gridData]);
    }

    /**
     * 編輯欄位
     * @param {*} prop 
     * @returns 
     */
    const EditCommandCell = (prop) => {
        return (
            <CommandCell>
                <Button title={"編輯"} icon='edit' look='default' onClick={() => {
                    // 紀錄開啟視窗
                    setWindowData(prop.dataItem);
                    setWindowStatus(!windowStatus);
                }} />
            </CommandCell>
        )
    }

    /**
     * 期間
     * @param {*} prop 
     * @returns 
     */
    const DateCell = (prop) => {
        return (
            // YYY_MM
            <td style={{ textAlign: "center" }}>{prop.dataItem.YEAR}_{prop.dataItem.MONTH}</td>
        )
    }

    /**
     * 多行文字顯示欄位
     * @param {*} prop 
     * @returns 
     */
    const textAreaWrapCell = (prop) => {
        return (
            <td>
                <TextAreaWrapInput value={prop.dataItem[prop.field]} />
            </td>
        )
    }

    /**
     * 備註
     * @param {*} prop 
     * @returns 
     */
    const memoCell = (prop) => {
        let memoData = prop.dataItem.ComIPCMemoMappingData;
        return (
            <td>
                {memoData.map((x, index) => {
                    return (
                        <div>{index + 1}.{ipcMemoDdlData.find(y => y.SET_TYPE == x.SET_TYPE).SET_VALUE}</div>
                    )
                })}
            </td>
        )
    }

    /**
     * 通知
     * @param {*} prop 
     * @returns 
     */
    const noticeCell = (prop) => {
        return (
            <div style={{ textAlign: 'center', padding: '5px' }}>
                <Button title="通知主辦及機關窗口" onClick={async () => {
                    SetMaskOnOff(true);
                    let sendResult = await ProjectFillAuditService.sendProjectAuditOpinionMail(prop.dataItem);
                    SetMaskOnOff(false);
                    showGlobalMessageBox(sendResult.message);
                }}>
                    通知主辦及機關窗口
                </Button>
            </div>
        )
    }

    return (
        <>
            {isRdecFun &&
                <div className='fn-buttons'>
                    <Button type='button' title="新增" onClick={() => { setWindowData(initData); setWindowStatus(!windowStatus); }} >新增</Button>
                </div>
            }
            <Grid
                style={{
                    height: '100%',
                    overflow: 'auto',
                }}
                resizable={true}
                data={gridData}
            >
                <GridNoRecords>無資料</GridNoRecords>
                {isRdecFun && <GridColumn title="刪除" cell={DelCommandCell} width="50px" />}
                {isRdecFun && <GridColumn title="編輯" cell={EditCommandCell} width="50px" />}
                <GridColumn title="期間" cell={DateCell} width="100px" />
                <GridColumn field="AUDIT_OPINION" title="意見" cell={textAreaWrapCell} />
                <GridColumn title="備註" cell={memoCell} />
                {isRdecFun && <GridColumn title="通知" cell={noticeCell} />}
            </Grid>

            {windowStatus &&
                <>
                    <Window
                        onClose={() => setWindowStatus(false)}
                        width={dimensions.width * 0.6}
                        height={dimensions.height * 0.7}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <ProjectAuditOpinionGridWindow
                            loadData={loadData}
                            data={windowData}
                            gridData={gridData}
                            setGridData={setGridData}
                            ipcMemoDdlData={ipcMemoDdlData}
                            planYearDdlData={planYearDdlData}
                            editedGridData={editedGridData}
                            close={() => setWindowStatus(false)}
                        />
                    </Window>
                </>
            }
        </>
    )
}
export default ProjectAuditOpinionGrid;
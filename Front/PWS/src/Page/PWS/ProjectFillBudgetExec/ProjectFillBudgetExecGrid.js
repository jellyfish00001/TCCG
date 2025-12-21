import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { formatNumber } from '@telerik/kendo-intl';
import { IsNullOrEmpty, FormatDate, SetMaskOnOff } from '../../../Basic/SDOExtension';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';
import ProjectFillBudgetExecService from './ProjectFillBudgetExecService';
import { showGlobalMessageBox, showGlobalConfirmBox } from "../../../Route/RootMiddleware";

export const ProjectFillBudgetExecGrid = (props) => {
    const { gridData, setGridData, isRdecFun, ddlData, setDataItem, loadData } = props;
    const loadTimes = React.useRef(0);

    /**
     * 刪除欄位
     * @param {*} prop 
     * @returns 
     */
    const DelCommandCell = (prop) => {
        if (isRdecFun) {
            return (
                <CommandCell>
                    <Button title={"刪除"} icon='close' look='default' onClick={() => {
                        showGlobalConfirmBox("確認刪除", () => remove(prop.dataItem));
                    }} />
                </CommandCell>
            )
        }
        else {
            return (
                <td></td>
            )
        }
    }

    // 移除
    const remove = async (dataItem) => {
        //找出grid要刪除的列
        let index = gridData.findIndex(record => record.SEQ === dataItem.SEQ);
        gridData.splice(index, 1);
        dataItem.editType = 3;
        setGridData([...gridData]);
        SetMaskOnOff(true);
        let saveResult = await ProjectFillBudgetExecService.saveProjectFillBudgetExec(dataItem);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                loadData();
            });
        }
        else {
            showGlobalMessageBox(saveResult.message);
        }
    }

    /**
     * 編輯欄位
     * @param {*} prop 
     * @returns 
     */
    const EditCommandCell = (prop) => {
        if (isRdecFun) {
            return (
                <CommandCell>
                    <Button title={"編輯"} icon='edit' look='default' onClick={() => {
                        //帶資料
                        setDataItem({ ...prop.dataItem, loadTimes: loadTimes.current });
                        loadTimes.current = loadTimes.current + 1;
                    }} />
                </CommandCell>
            )
        }
        else {
            return (
                <td></td>
            )
        }
    }

    /**
     * 期間
     * @param {*} prop 
     * @returns 
     */
    const periodCell = (prop) => {
        return (
            <td style={{ wordWrap: "break-word" }}>
                {prop.dataItem.EXEC_YEAR}_{prop.dataItem.EXEC_MONTH}
            </td>
        )
    }

    /**
     * 原因欄位
     * @param {*} prop 
     * @returns 
     */
    const failedCell = (prop) => {
        let type = prop.dataItem.FailedMappingData.map(x => x.SET_TYPE);
        let data = ddlData.IPCBGTEXECFAILED.filter(x => {
            if (type.includes(x.SET_TYPE)) {
                return x;
            }
        })
        return (
            <td>
                {data.map(x => {
                    return (
                        <div>{x.SET_VALUE}</div>
                    )
                })}
            </td>
        )
    }

    /**
     * 責任歸屬欄位
     * @param {*} prop 
     * @returns 
     */
    const failedDutyCell = (prop) => {
        let type = prop.dataItem.FailedDutyMappingData.map(x => x.SET_TYPE);
        let data = ddlData.IPCBGTEXECFAILEDDUTY.filter(x => {
            if (type.includes(x.SET_TYPE)) {
                return x;
            }
        })
        return (
            <td>
                {data.map(x => {
                    return (
                        <div>{x.SET_VALUE}</div>
                    )
                })}
            </td>
        )
    }

    /**
     * 數字格式
     * @param {*} prop 
     * @returns 
     */
    const formatNumberCell = (prop) => {
        let format = prop.field == "GT_EXEC_RATE" || prop.field == "YEAR_EXEC_RATE" ? "n2" : "n0";
        if (prop.field == "bc") {
            prop.dataItem.bc = prop.dataItem.GT_ACT_BUDGET + prop.dataItem.GT_AP;
        }
        return (
            <td style={{ textAlign: "right" }}>{formatNumber(prop.dataItem[prop.field], format)}</td>
        )
    }
    // 日期格式化
    const formatDate = (prop) => {
        if (IsNullOrEmpty(prop.dataItem[prop.field])) {
            return (
                <td></td>
            )
        }
        else {
            return (
                <td style={{ wordWrap: "break-word" }}>{FormatDate(prop.dataItem[prop.field], "tYY/MM/DD")}</td>
            )
        }
    }

    /**
     * 多行文字顯示欄位
     * @param {*} prop 
     * @returns 
     */
    const textAreaWrapCell = (prop) => {
        return (
            <td style={{ wordWrap: "break-word" }}>
                <TextAreaWrapInput value={prop.dataItem[prop.field]} />
            </td>
        )
    }

    return (
        <Grid
            style={{
                height: '100%',
                overflow: 'auto',
            }}
            resizable={true}
            data={gridData}
        >
            <GridNoRecords>無資料</GridNoRecords>
            {isRdecFun
                ?
                <GridColumn>
                    <GridColumn title={<>刪<br />除</>} cell={DelCommandCell} width="30px" />
                    <GridColumn title={<>編<br />輯</>} cell={EditCommandCell} width="30px" />
                    <GridColumn title={<>期<br />間</>} cell={periodCell} width="35px" />
                </GridColumn>
                :
                <GridColumn>
                    <GridColumn title={<>期<br />間</>} cell={periodCell} width="35px" />
                </GridColumn>
            }
            <GridColumn title="累計執行情形">
                <GridColumn field="GT_EXPANDED_BUDGET" title={<>累計<br />預定<br />支用<br />(元)<br />(a)</>} cell={formatNumberCell} />
                <GridColumn field="bc" title={<>累計<br />實際<br />完成<br />金額<br />(元)<br />(b+c)</>} cell={formatNumberCell} />
                <GridColumn field="GT_ACT_BUDGET" title={<>累計<br />各項<br />經費<br />支用<br />(元)<br />(b)</>} cell={formatNumberCell} />
                <GridColumn field="GT_AP" title={<>應付<br />未付<br />數<br />(元)<br />(c)</>} cell={formatNumberCell} />
                <GridColumn field="GT_BALANCE" title={<>結餘數<br />(元)<br />(d)</>} cell={formatNumberCell} />
                <GridColumn field="GT_TOTAL" title={<>累計<br />實際<br />支用<br />數<br />(元)<br />(e)</>} cell={formatNumberCell} />
                <GridColumn field="GT_EXEC_RATE" title={<>累計<br />執行<br />率(%)<br />(e/a)</>} cell={formatNumberCell} width="60px" />
            </GridColumn>
            <GridColumn title="本年度執行情形">
                <GridColumn field="YEAR_BUDGET_EXPANDED" title={<>本年度<br />可支用<br />預算數<br />(元)<br />(j)</>} cell={formatNumberCell} />
                <GridColumn field="YEAR_BUDGET_ALLOCATED" title={<>本年度<br />預算<br />分配數<br />(元)<br />(k)</>} cell={formatNumberCell} />
                <GridColumn field="YEAR_EXEC_BUDGET" title={<>本年度<br />預算<br />執行數<br />(元)<br />(l)</>} cell={formatNumberCell} />
                <GridColumn field="YEAR_EXEC_RATE" title={<>本年度<br />執行率<br />(%)<br />(l/k)</>} width="60px" cell={formatNumberCell} />
                <GridColumn title={<>預算<br />執行率<br />未達<br />80%<br />原因</>} cell={failedCell} />
                <GridColumn field="" title={<>責任<br />歸屬<br /></>} cell={failedDutyCell} />
                <GridColumn field="EXEC_RATE_FAILED_NOTE" title={<>預算執行<br />率未達<br />80%說明</>} cell={textAreaWrapCell} />
                <GridColumn field="CRT_DATE" title={<>填報<br />日期</>} cell={formatDate} width="50px" />
            </GridColumn>
        </Grid>
    )
}
export default ProjectFillBudgetExecGrid;
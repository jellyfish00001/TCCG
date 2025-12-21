import { Button } from "@progress/kendo-react-buttons";
import { Grid, GridColumn, GridNoRecords } from "@progress/kendo-react-grid";
import React from "react";
import { FormatDate, IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { showGlobalConfirmBox } from "../../../Route/RootMiddleware";
import { deleteProjectDelayCausal, getProjectFillDelayList } from "./ProjectFillDelayService";

const ProjectFillDelayGrid = (props) => {

    const { projectNo, loadTableData, isRdecFun, showBtnByProjectStatus, state } = props;

    //#region 參數宣告
    const [gridData, setGridData] = React.useState([]);

    //#endregion

    /**
     * 修改欄位
     * @param {*} props 
     * @returns 
     */
    const editCell = props => {
        return (
            <CommandCell>
                <Button type="button" title="修改" icon="edit" look="default"
                    onClick={() => {
                        loadTableData(props.dataItem.SEQ);
                    }}
                />
            </CommandCell>
        )
    }

    /**
     * 刪除欄位
     * @param {*} props 
     * @returns 
     */
    const deleteCell = props => {
        return (
            <CommandCell>
                <Button type="button" title="刪除" icon="close" look="default"
                    onClick={() => {
                        showGlobalConfirmBox("是否要刪除該筆資料?",
                            () => {
                                deleteEvent(props.dataItem.SEQ)
                            })
                    }}
                />
            </CommandCell>
        )
    }

    const deleteEvent = async (seq) => {
        await deleteProjectDelayCausal(seq);
        loadTableData("");
    }

    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await getProjectFillDelayList(projectNo, state ? 1 : 0);
        setGridData(result);
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, [state])

    return (
        <Grid
            style={{
                height: '100%'
            }}
            data={gridData}
            resizable={true}
        >
            <GridNoRecords>無資料</GridNoRecords>
            {isRdecFun && showBtnByProjectStatus && <GridColumn title="編輯" cell={editCell} width="50px" />}
            {isRdecFun && showBtnByProjectStatus && <GridColumn title="刪除" cell={deleteCell} width="50px" />}
            <GridColumn title="年度" field="DATA_YEAR" width="50px" />
            <GridColumn title="月份" field="DATA_MONTH" width="50px" />
            <GridColumn title="落後類型" field="DELAY_KIND" width="70px" />
            <GridColumn title="落後類別" field="DELAY_CLASS_C" width="110px" />
            <GridColumn title="落後項目" field="DELAY_SUBCLASS_C" />
            <GridColumn title="責任歸屬" field="DELAY_RESPON" width="100px" />
            <GridColumn title="落後原因" field="DELAY_CAUSAL" />
            <GridColumn title="解決對策" field="SOLUTION" />
            <GridColumn title="須協調事項" field="COORDINATION" />
            <GridColumn title="改進完成期限" cell={(props) => (
                <td>
                    {
                        IsNullOrEmpty(props.dataItem["DEADLINES"])
                            ? ""
                            : FormatDate(props.dataItem["DEADLINES"], 'tYY-MM-DD')
                    }
                </td>
            )} width="110px" />
        </Grid>
    );

}

export default ProjectFillDelayGrid;
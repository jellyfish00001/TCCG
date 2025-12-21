import React from "react";
import Table from '../../../Css/custom/Table.module.css';
import { FormatDate, IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { CheckIsRDECRole } from "../../../Basic/CommonService";
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { Grid, GridColumn, GridNoRecords } from "@progress/kendo-react-grid";
import { Button } from "@progress/kendo-react-buttons";
import { getProjecFillExecuteList } from "./ProjectFillExecuteService";

const ProjectFillExecuteGrid = (props) => {
    const { projectNo, isEngineering, state, loadTableData } = props;

    const [gridData, setGridData] = React.useState([]);
    const [isLoad, setIsLoad] = React.useState(false);
    const isRdec = React.useRef(false);

    const loadData = async () => {
        SetMaskOnOff(true);
        isRdec.current = await CheckIsRDECRole();
        let result = await getProjecFillExecuteList(projectNo, state ? 1 : 0);
        setGridData(result);
        setIsLoad(true);
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, [state])

    /**
     * 修改欄位
     * @param {*} props 
     * @returns 
     */
    const editCell = (props) => {
        return (
            <CommandCell>
                <Button type="button" title="修改" icon="edit" look="default"
                    onClick={() => loadTableData(props.dataItem.SEQ)}
                />
            </CommandCell>
        )
    }

    return (
        // 避免欄位寬度跑掉
        isLoad &&
        <Grid
            style={{
                height: '100%'
            }}
            data={gridData}
        >
            <GridNoRecords>無資料</GridNoRecords>
            {isRdec.current && <GridColumn title="編輯" cell={editCell} width="50px" />}
            <GridColumn title="期間" cell={(props) => {
                return <td>{`${props.dataItem["YEAR"]}_${props.dataItem["MONTH"]}`}</td>
            }} width="70px" />
            {
                isEngineering
                    ? <GridColumn title="重大建設系統">
                        <GridColumn
                            title={<span>累計預定<br />施工進度</span>}
                            field="IPC_RES_PRG"
                            className={Table.textAlign_right}
                            width="100px" />
                        <GridColumn
                            title={<span>累計實際<br />施工進度</span>}
                            field="IPC_ACT_PRG"
                            className={Table.textAlign_right}
                            width="100px" />
                    </GridColumn>
                    : null
            }
            {
                isEngineering
                    ? <GridColumn title="標案管理系統" >
                        <GridColumn
                            title={<span>累計預定<br />施工進度</span>}
                            field="TEN_RES_PRG"
                            className={Table.textAlign_right}
                            width="100px" />
                        <GridColumn
                            title={<span>累計實際<br />施工進度</span>}
                            field="TEN_ACT_PRG"
                            className={Table.textAlign_right}
                            width="100px" />
                    </GridColumn>
                    : null
            }
            <GridColumn title="執行情況" field="EXECUTE_CONDITION" width={"calc(100% - 270px)"} />
            <GridColumn title="需協辦事項" field="ASSISTANT_ITEM" width="120px" />
            <GridColumn title={<span>送出日期<br />(逾期天數)</span>}
                cell={(props) => {
                    const { SEND_DATE, OVERDUE_DAY, DISREGARD } = props.dataItem
                    return (
                        <td>
                            {IsNullOrEmpty(SEND_DATE) ? null : FormatDate(SEND_DATE, 'tYY/MM/DD')}
                            {IsNullOrEmpty(SEND_DATE) ? null : <br />}
                            ({DISREGARD ? '不計算' : OVERDUE_DAY === 0 ? '無' : OVERDUE_DAY})
                        </td>
                    )
                }}
                field="DISPLAY_SEND_DATE" width="150px" />
        </Grid>
    )
}

export default ProjectFillExecuteGrid;


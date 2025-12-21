
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import React, { useRef } from 'react';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell';
import { handleEditedGridData, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { formatNumber } from '@telerik/kendo-intl';
import { GetBasicData } from '../../../Basic/BasicData';

const InnProjectProposalGrid = (props) => {

    const { } = props

    //#region 參數宣告

    const [gridData, setGridData] = React.useState([]);

    const orgId = useRef("");

    // 紀錄異動資料
    const editedGridData = React.useRef([]);

    // 觸發Grid驗證
    const [triggerValidation, setTriggerValidation] = React.useState(true);

    // 紀錄是否有新增資料
    const [newDatas, setNewDatas] = React.useState(false);


    // 文字輸入框Cell
    const textInputCell = props => {
        return (
            <TextInputCell
                {...props}
                required={true}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInputChange={onCellInputBlur}
                AlwaysEdit={true}
            />
        );

    }

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = async (item) => {
        handleEditedGridData(editedGridData, item);
    }


    // 新增
    const addNew = () => {
        //新增時寫入預設值
        if (gridData.length < 5) {
            const newRecord = {
                ORG_NAME: "",
                UNIT: "",
                JOB_TITLE: "",
                PLAN_MAN: ""

            }
            editedGridData.current.push(newRecord);
            setGridData([...gridData, newRecord]);
        }


    }


    // 載入資料
    const loadData = async () => {

    }

    React.useEffect(() => {
        loadData();
    }, [])

    /**
     * 刪除欄位
     * @param {*} prop 
     * @returns 
     */
    const DelCommandCell = (prop) => {
        return (
            <CommandCell>
                <Button title={"刪除"} icon='close' look='default' onClick={() => {
                    remove(prop.dataItem)
                }} />
            </CommandCell>
        )
    }
    // 移除
    const remove = (dataItem) => {
        let index = gridData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex)
        gridData.splice(index, 1);
        setGridData([...gridData]);
    }

    React.useEffect(() => {

    }, [])

    return (
        <>
            <div className="fn-buttons">
                <Button title="新增" type="button" onClick={addNew}>新增</Button>
            </div>

            <Grid
                style={{
                    height: '100%',
                    overflow: 'auto',
                }}
                resizable={true}
                data={gridData}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn title="刪除" cell={DelCommandCell} width="85%" />
                <GridColumn field="ORG_NAME" title="機關" cell={textInputCell} width="300%" />
                <GridColumn field="UNIT" title="所屬單位" cell={textInputCell} width="200%" />
                <GridColumn field="JOB_TITLE" title="職稱" cell={textInputCell} width="200%" />
                <GridColumn field="PLAN_MAN" title="姓名" cell={textInputCell} width="200%" />
            </Grid>

        </>
    );
}

export default InnProjectProposalGrid;
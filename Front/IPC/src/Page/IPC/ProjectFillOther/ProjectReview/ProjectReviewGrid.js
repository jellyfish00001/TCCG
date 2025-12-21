import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { handleEditedGridData } from '../../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../../Components/GridCell/CommandCell";
import { TextAreaCell } from "../../../../Components/GridCell/TextAreaCell";
import TwDatePicker from "../../../../Components/DateInputs/TwDatePicker";
import { RequiredHeaderCell } from '../../../../Components/GridCell/RequiredHeaderCell';

export const ProjectReviewGrid = (props) => {
    const { projectNo, gridData, setGridData, editedGridData } = props;

    // 新增
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            SEQ: 0,
            PROJECT_NO: projectNo,
            REVIEW_KIND: "04",
            IS_REVIEW: true,
            SEND_DATE: new Date(),
            REVIEW_DATE: null,
            OTH_RVWNAME: "",
            editType: 1
        }
        editedGridData.current.push(newRecord);
        setGridData([...gridData, newRecord]);
    }

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
        let index = dataItem.hiddenIndex ?
            gridData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex)
            :
            gridData.findIndex(record => record.SEQ === dataItem.SEQ);
        if (dataItem.hiddenIndex) {
            editedGridData.current.pop();
        }
        gridData.splice(index, 1);
        dataItem.editType = 3;
        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'SEQ');
        setGridData([...gridData]);
    }

    /**
     * 日期更改callback事件
     * @param {*} prop 
     * @param {*} value 
     */
    const onDateCellInputChange = async (prop, value) => {
        let item = prop.dataItem;
        if (item.editType !== 1) {
            item.editType = 2;
        }
        item[prop.field] = value;
        let index = item.hiddenIndex ?
            gridData.findIndex(record => record.hiddenIndex === item.hiddenIndex)
            :
            gridData.findIndex(record => record.SEQ === item.SEQ);
        gridData.splice(index, 1, item);

        setGridData([...gridData]);
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SEQ');
    }

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = async (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SEQ');
    }

    /**
     * 日期欄位
     * @param {*} prop 
     * @returns 
     */
    const twDatePickerCell = (prop) => {
        return (
            <td>
                <TwDatePicker
                    name={prop.field}
                    format={"yyy/MM/dd"}
                    onChange={(e) => onDateCellInputChange(prop, e.value)}
                    value={prop.dataItem[prop.field] == null ? null : new Date(prop.dataItem[prop.field])}
                    width="95%"
                />
            </td>
        )
    }

    const textAreaCell = (prop) => {
        return (
            <TextAreaCell
                {...prop}
                required={true}
                editable={true}
                maxlength={400}
                style={{ width: "100%" }}
                labelStyle={{ textAlign: "left" }}
                onCellInputBlur={onCellInputBlur}
                newDatas={true}
                IsAlwaysEdit={true}
            />
        )
    }

    return (
        <>
            <Button type='button' title="新增" onClick={addNew} >新增</Button>

            <Grid
                style={{
                    height: '100%',
                    overflow: 'auto',
                }}
                resizable={true}
                data={gridData}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn title="刪除" cell={DelCommandCell} width="50px" />
                <GridColumn field="OTH_RVWNAME" title="審查名稱" cell={textAreaCell} headerCell={RequiredHeaderCell} />
                <GridColumn field="SEND_DATE" title="送件日期" cell={twDatePickerCell} headerCell={RequiredHeaderCell} />
                <GridColumn field="REVIEW_DATE" title="核定日期" cell={twDatePickerCell} />
            </Grid>
        </>
    )
}
export default ProjectReviewGrid;
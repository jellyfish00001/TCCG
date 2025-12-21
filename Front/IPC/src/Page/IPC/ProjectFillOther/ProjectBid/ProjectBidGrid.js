import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { handleEditedGridData } from '../../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import TwDatePicker from "../../../../Components/DateInputs/TwDatePicker";
import { TextAreaCell } from "../../../../Components/GridCell/TextAreaCell";
import { CommandCell } from "../../../../Components/GridCell/CommandCell";
import { RequiredHeaderCell } from '../../../../Components/GridCell/RequiredHeaderCell';

export const ProjectBidGrid = (props) => {
    const { projectNo, bidKind, detailType, initData, projectBidDetailData, setProjectBidDetailData, editedGridData } = props;
    const [gridData, setGridData] = React.useState([]);

    React.useEffect(() => {
        setGridData([...initData]);
    }, [initData])

    // 新增
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: bidKind + "_" + detailType + "_" + (LasthiddenIndex + 1),
            SEQ: 0,
            PROJECT_NO: projectNo,
            BID_KIND: bidKind,
            DETAIL_TYPE: detailType,
            DETAIL_DATE: new Date(),
            DETAIL_REASON: "",
            editType: 1
        }
        editedGridData.current.push(newRecord);
        setGridData([...gridData, newRecord]);
        setProjectBidDetailData([...projectBidDetailData, newRecord]);
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
        let allIndex = dataItem.hiddenIndex ?
            projectBidDetailData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex)
            :
            projectBidDetailData.findIndex(record => record.SEQ === dataItem.SEQ);
        if (dataItem.hiddenIndex) {
            editedGridData.current.pop();
        }
        gridData.splice(index, 1);
        projectBidDetailData.splice(allIndex, 1);
        dataItem.editType = 3;
        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'SEQ');
        setGridData([...gridData]);
        setProjectBidDetailData([...projectBidDetailData]);
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
                    name={"DETAIL_DATE"}
                    format={"yyy/MM/dd"}
                    onChange={(e) => onDateCellInputChange(prop, e.value)}
                    value={prop.dataItem.DETAIL_DATE == null ? null : new Date(prop.dataItem.DETAIL_DATE)}
                    width="95%"
                />
            </td>
        )
    }

    /**
     * 多行輸入框
     * @param {*} prop 
     * @returns 
     */
    const textAreaCell = (prop) => {
        return (
            <TextAreaCell
                {...prop}
                required={true}
                editable={true}
                maxlength={500}
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
            <span style={{marginLeft: "10px"}}>{gridData.length}次</span>

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
                <GridColumn title="次" cell={(props) =>
                    <td style={{ textAlign: "center" }}>{props.dataIndex + 1}</td>} width="50px" />
                <GridColumn field="DETAIL_DATE" title={detailType == 0 ? "流標日期" : "廢標日期"} cell={twDatePickerCell} headerCell={RequiredHeaderCell} />
                <GridColumn field="DETAIL_REASON" title={detailType == 0 ? "流標原因" : "廢標原因"} cell={textAreaCell} headerCell={RequiredHeaderCell} />
            </Grid>
        </>
    )
}
export default ProjectBidGrid;
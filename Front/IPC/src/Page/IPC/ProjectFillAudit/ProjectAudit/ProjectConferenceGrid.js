import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { handleEditedGridData } from '../../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { DropDownListCell } from '../../../../Components/GridCell/DropDownListCell';
import TwDatePicker from "../../../../Components/DateInputs/TwDatePicker";
import { NumericTextInputCell } from '../../../../Components/GridCell/NumericTextInputCell';
import { CommandCell } from "../../../../Components/GridCell/CommandCell";
import ProjectFillAuditService from '../ProjectFillAuditService';
import { RequiredHeaderCell } from '../../../../Components/GridCell/RequiredHeaderCell';

// 列管會議
export const ProjectConferenceGrid = (props) => {
    const { projectNo, data, conferenceDdlData, editedGridData, isRdecFun } = props;
    const [gridData, setGridData] = React.useState([]);
    const [newDatas, setNewDatas] = React.useState(false);

    React.useEffect(() => {
        setGridData(data);
        editedGridData.current = [];
    }, [data])

    // 新增
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            PROJECT_NO: projectNo,
            IDENTITY_FIELD: 0,
            CONFERENCE_GENRE: "01",
            CONFERENCE_NUM: 0,
            CONFERENCE_TIME: new Date(),
            editType: 1,
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
            gridData.findIndex(record => record.IDENTITY_FIELD === dataItem.IDENTITY_FIELD);

        gridData.splice(index, 1);
        dataItem.editType = 3;
        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'IDENTITY_FIELD');
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
            gridData.findIndex(record => record.IDENTITY_FIELD === item.IDENTITY_FIELD);
        gridData.splice(index, 1, item);
        setGridData([...gridData]);
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'IDENTITY_FIELD');
    }

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = async (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'IDENTITY_FIELD');
        setNewDatas(!newDatas);
    }

    /**
     * 會議種類欄位
     * @param {*} prop 
     * @returns 
     */
    const conferenceGenreCell = prop => {
        return (
            <DropDownListCell
                {...prop}
                editable={true}
                ddlData={conferenceDdlData}
                textField={'SET_VALUE'}
                dataItemKey={'SET_TYPE'}
                AlwaysEdit={true}
                onCellDDLChange={onCellInputBlur}
            />
        )
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
                    name="CONFERENCE_TIME"
                    format={"yyy/MM/dd"}
                    onChange={(e) => onDateCellInputChange(prop, e.value)}
                    value={new Date(prop.dataItem.CONFERENCE_TIME)}
                    width="95%"
                />
            </td>
        )
    }

    /**
     * 數字輸入框
     * @param {*} prop 
     * @returns 
     */
    const numericCell = prop => {
        return (
            <NumericTextInputCell
                {...prop}
                Numberformat={'n0'}
                newDatas={true}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInputChange={onCellInputBlur}
                AlwaysEdit={true}
                customValidate={{ scheme: ProjectFillAuditService.validateConferenceField, triggerValidate: true }}
            />
        )
    }

    return (
        <>
            {isRdecFun && <Button type='button' title="新增" onClick={addNew}>新增</Button>}

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
                <GridColumn title="序號" cell={(props) =>
                    <td style={{ textAlign: "center" }}>{props.dataIndex + 1}</td>} width="50px" />
                <GridColumn field="CONFERENCE_GENRE" title="會議種類" cell={conferenceGenreCell} headerCell={RequiredHeaderCell} />
                <GridColumn field="CONFERENCE_TIME" title="會議時間" cell={twDatePickerCell} headerCell={RequiredHeaderCell} />
                <GridColumn field="CONFERENCE_NUM" title="會次" cell={numericCell} headerCell={RequiredHeaderCell} />
            </Grid>
        </>
    )
}
export default ProjectConferenceGrid;
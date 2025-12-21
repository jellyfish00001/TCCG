import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { handleEditedGridData } from '../../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { TextAreaCell } from "../../../../Components/GridCell/TextAreaCell";
import { CommandCell } from "../../../../Components/GridCell/CommandCell";
import { FormatDate } from "../../../../Basic/SDOExtension";
import { RequiredHeaderCell } from '../../../../Components/GridCell/RequiredHeaderCell';

// 列管會議
export const ProjectCloseDetailsGrid = (props) => {
    const {
        projectNo,
        type,
        data,
        projectCloseDetailsData,
        setProjectCloseDetailsData,
        editedGridData,
        setCnt,
        isRdecFun } = props;
    const [gridData, setGridData] = React.useState([]);

    React.useEffect(() => {
        setGridData([...data]);
    }, [data])

    // 計算grid列數
    React.useEffect(() => {
        if (setCnt) {
            setCnt(gridData.length);
        }
    }, [gridData])

    // 新增
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: type + "_" + (LasthiddenIndex + 1),
            PROJECT_NO: projectNo,
            SEQ: 0,
            CLOSE_DETAILS_TYPE: type,
            REF_DATE: new Date(),
            REF_MEMO: "",
            editType: 1,
        }
        editedGridData.current.push(newRecord);
        setGridData([...gridData, newRecord]);
        setProjectCloseDetailsData([...projectCloseDetailsData, newRecord]);
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
            projectCloseDetailsData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex)
            :
            projectCloseDetailsData.findIndex(record => record.SEQ === dataItem.SEQ);
        gridData.splice(index, 1);
        projectCloseDetailsData.splice(allIndex, 1);
        dataItem.editType = 3;
        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'SEQ');
        setGridData([...gridData]);
        setProjectCloseDetailsData([...projectCloseDetailsData]);
    }

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = async (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SEQ');
    }

    const formatDateCell = (prop) => {
        return (
            <td style={{ textAlign: "center" }}>
                {FormatDate(prop.dataItem.REF_DATE, "tYY/MM/DD")}
            </td>
        )
    }

    /**
     * 數字輸入框
     * @param {*} prop 
     * @returns 
     */
    const textAreaCell = prop => {
        return (
            <TextAreaCell
                {...prop}
                required={true}
                editable={true}
                maxlength={100}
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
                <GridColumn field="REF_DATE" title="日期" cell={formatDateCell} width="150px" />
                <GridColumn field="REF_MEMO" title="說明" cell={textAreaCell} headerCell={RequiredHeaderCell} />
            </Grid>
        </>
    )
}
export default ProjectCloseDetailsGrid;
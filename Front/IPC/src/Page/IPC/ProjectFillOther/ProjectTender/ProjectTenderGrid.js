import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { handleEditedGridData } from '../../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { RequiredHeaderCell } from '../../../../Components/GridCell/RequiredHeaderCell';
import { CommandCell } from "../../../../Components/GridCell/CommandCell";
import { TextInputCell } from '../../../../Components/GridCell/TextInputCell';
import { DropDownListCell } from '../../../../Components/GridCell/DropDownListCell';

export const ProjectTenderGrid = (props) => {
    const { projectNo, tenderKindDdlData, gridData, setGridData, editedGridData } = props;

    const [newDatas, setNewDatas] = React.useState(false);

    /**
     * 新增
     */
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            SEQ: 0,
            PROJECT_NO: projectNo,
            TENDER_KIND: "",
            TENDER_NAME: "",
            REG_NO: "",
            TENDER_ADDR: "",
            CONTACT_NAME: "",
            CONTACT_PHONE: "",
            CONTACT_EMAIL: "",
            editType: 1
        }
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
        //判定刪除的是否為新增的資料
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
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SEQ');
        setNewDatas(!newDatas);
    }

    /**
     * 供可切換欄位的input onChange callback的事件
     * @param {object} item
     */
    const onCellInputChange = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SEQ');
    }

    /**
     * 廠商類別欄位
     * @param {*} prop 
     * @returns 
     */
    const tenderKindCell = prop => {
        return (
            <DropDownListCell
                {...prop}
                editable={true}
                ddlData={tenderKindDdlData}
                textField={'SET_VALUE'}
                dataItemKey={'SET_TYPE'}
                AlwaysEdit={true}
                onCellDDLChange={onCellInputBlur}
            />
        )
    }

    /**
     * 文字輸入框欄位
     * @param {*} prop 
     * @returns 
     */
    const textInputCell = (prop) => {
        let max;
        let required = false;
        switch (prop.field) {
            case "TENDER_NAME":
                max = 50; required = true; break;
            case "REG_NO":
                max = 8; break;
            case "TENDER_ADDR":
                max = 500; break;
            case "CONTACT_NAME":
                max = 10; break;
            case "CONTACT_PHONE":
                max = 20; break;
            case "CONTACT_EMAIL":
                max = 200; break;
        }
        return (
            <TextInputCell
                {...prop}
                maxlength={max}
                required={required}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInputChange={onCellInputChange}
                AlwaysEdit={true}
                newDatas={newDatas}
            />
        )
    }

    return (
        <>
            <div className="fn-buttons">
                <Button title="新增" onClick={addNew} >新增</Button>
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
                <GridColumn title="刪除" cell={DelCommandCell} width="50px" />
                <GridColumn field="TENDER_KIND" title="廠商類別" cell={tenderKindCell} headerCell={RequiredHeaderCell} width="140px" />
                <GridColumn field="TENDER_NAME" title="廠商名稱" cell={textInputCell} headerCell={RequiredHeaderCell} />
                <GridColumn field="REG_NO" title="統編" cell={textInputCell} width="90px" />
                <GridColumn field="TENDER_ADDR" title="地址" cell={textInputCell} />
                <GridColumn field="CONTACT_NAME" title="聯絡人" cell={textInputCell} width="100px" />
                <GridColumn field="CONTACT_PHONE" title="聯絡人電話" cell={textInputCell} />
                <GridColumn field="CONTACT_EMAIL" title="聯絡人Email" cell={textInputCell} />
            </Grid>
        </>
    )
}
export default ProjectTenderGrid;
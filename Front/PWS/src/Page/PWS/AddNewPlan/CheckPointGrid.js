import React, { useState, useRef, useEffect } from "react";
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { handleEditedGridData } from '../../../Basic/SDOExtension';
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import { loadCusItem } from "./AddNewPlanService";
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';

export const CheckPointGrid = (props) => {
    const { checkpoint, editedCheckPointData, orgCheckpoint, checkPointFromSql, setGridData, gridData } = props;
    
    /**
     * 透過切換執行方式取得系統預設檢核點資料
     * @returns 
     */
    const loadGridData = async (checkpoint) => {
        SetMaskOnOff(true);
        if(checkpoint !== orgCheckpoint.current){
            let data = await loadCusItem(checkpoint);   
            // 讀出來的資料要轉成grid格式
            data = data.map((item) => {
                let gridItem = {
                    ...item,
                    hiddenIndex: item.NO,
                    SEQ: 0,
                    CHECKITEM_NAME: item.NAME,
                    CHECKITEM_SEQ: item.SEQ,
                    editType: 1,
                };
                handleEditedGridData(editedCheckPointData, gridItem, 'hiddenIndex', 'SEQ');
                return gridItem;
            });
            setGridData([...data]);
            console.log(data);
        }else{
            // 執行方式未改變 先清空紀錄 取原始資料 
            editedCheckPointData.current = [];
            setGridData([...checkPointFromSql.current]);
        }
        SetMaskOnOff(false);
    }
    

    // 先記錄改變下拉選單所得的新資料
    useEffect(() => {
        setGridData([]);
        // 若有選擇執行方式則取得系統預設檢核點資料
        if (checkpoint !== "") {
            loadGridData(checkpoint);
        }
    }, [checkpoint]);

    /**
     * 新增Grid資料行
     * @returns 
     */
    const addNew = () => {
        // 取得gridData內的最大值
        const LasthiddenIndex = Math.max(...gridData.map(item => item.NO), 0);
        // 新增時寫入預設值
        let newData = {
            NO: LasthiddenIndex + 1,
            hiddenIndex: LasthiddenIndex + 1,
            SEQ: 0,
            CHECKITEM_NAME: '',
            ESTIMATED_ENDDATE: new Date(),
            PROGRESS: 0,
            editType: 1
        }
        setGridData([...gridData, newData]);
        handleEditedGridData(editedCheckPointData, newData, 'hiddenIndex', 'SEQ');
    }

    /**
     * 移除指定Grid資料行
     * @param {*} dataItemToRemove //資料行
     * @returns 
     */
    const remove = (dataItemToRemove) => {
        //  篩選掉要刪除的資料
        const filteredData = gridData.filter(item => item !== dataItemToRemove);
        setGridData(filteredData);
        // 紀錄要刪除的資料
        dataItemToRemove.editType = 3;
        handleEditedGridData(editedCheckPointData, dataItemToRemove, 'hiddenIndex', 'SEQ');
    };
    
    /**
     * 刪除欄位
     * @param {*} props //資料行
     * @returns 
     */
    const DelCommandCell = (props) => {
        // 若資料來源為代碼維護設定則不得刪除
        if (props.dataItem.CHECKITEM_SEQ > 0 || props.dataItem.fromSettings) {
            return (
                <td></td>
            )
        } else {
            return (
                <td style={{ 'textAlign': 'center' }}>
                    <Button title={"刪除"} icon='close' look='default' type="button" onClick={() => {
                        remove(props.dataItem);
                    }} />
                </td>
            )
        }
    };

    /**
     * grid用input
     * @param {*} item //資料行
     * @returns 
     */
    const onCellInputBlur = async (item) => {
        handleEditedGridData(editedCheckPointData, item, 'hiddenIndex', 'SEQ');
    };

    /**
     * grid用日期
     * @param {*} prop //資料行
     * @param {*} value //日期
     * @returns 
     */
    const onDateCellInputChange = (prop, value) => {
        prop.dataItem[prop.field] = value;
        if(prop.dataItem.editType !== 1){
            prop.dataItem.editType = 2;
        }
        setGridData([...gridData]);
        handleEditedGridData(editedCheckPointData, prop.dataItem, 'hiddenIndex', 'SEQ');
    }

    /**
     * 輸入框
     * @param {*} props //資料行
     * @returns 
     */
    const textInputCell = (props) => {
        // 若為代碼維護設定則不可編輯
        if (props.dataItem.CHECKITEM_SEQ) {
            return (
                <td>{props.dataItem.CHECKITEM_NAME}</td>
            );
        }
        return (
            <TextInputCell
                {...props}
                required={true}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInchange={onCellInputBlur}
                AlwaysEdit={true}
            />
        );
    };

    // 數字輸入框Cell
    const numericInputCell = (props) => {
        // 若資料來源為代碼維護設定則不可編輯
        if (props.dataItem.CHECKITEM_SEQ) {
            return (
                <td>{props.dataItem.PROGRESS}</td>
            );
        } else {
            return (
                <NumericTextInputCell
                    {...props}
                    editable={true}
                    Numberformat={'n2'}
                    min={0}
                    max={100}
                    onCellInputBlur={onCellInputBlur}
                    AlwaysEdit={true}
                />
            )
        }
    }

    /**
     * grid用日期輸入框
     * @param {*} props //資料行
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

    return (
        <>
            <Button type='button' title="新增" onClick={addNew} >新增</Button>
            <Grid
                style={{ height: '100%', overflow: 'auto', }}
                resizable={true}
                data={gridData}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="DELETE" title="刪除" cell={DelCommandCell} width="50px" />
                <GridColumn field="CHECKITEM_NAME" title="檢核點" cell={textInputCell} />
                <GridColumn field="PROGRESS"
                    headerCell={RequiredHeaderCell}
                    title="管考進度%"
                    cell={numericInputCell}
                    width="120px"
                />
                <GridColumn field="ESTIMATED_ENDDATE" title="預定完成日期" cell={twDatePickerCell} />
            </Grid>
        </>
    )
}
export default CheckPointGrid;
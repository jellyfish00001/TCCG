import React, { useState, useRef, useEffect } from "react";
import { Formik, Form, Field } from 'formik';
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';

/**
 * 研究發展作業系統-委託研究計劃
 * 計畫基本資料的
 * 每季評核指標grid
 * 下拉選單寫死
 */

export const NewCheckGrid = (props) => {
    const { data } = props;
    const [gridData, setGridData] = useState([]);

    // 新增Grid資料行
    const addNew = () => {
        let newData = {
            CHECK_KPI: ''
        }
        setGridData([...gridData, newData]);
    }

    // 移除指定Grid資料行
    const remove = (dataItemToRemove) => {
        const filteredData = gridData.filter(item => item !== dataItemToRemove);
        setGridData(filteredData);
    };
    //刪除欄位
    const DelCommandCell = (props) => {
        return (
            <CommandCell>
                <Button title={"刪除"} icon='close' look='default' onClick={() => { remove(props.dataItem); }} 
                />
            </CommandCell>
        );
    };

    // grid用input
    const onCellInputBlur = async (item) => {
        
    };

    const onDateCellInputChange = (prop, value) => {
        prop.dataItem[prop.field] = value;
        setGridData([...gridData]);
    }
    
    // grid用input
    const textInputCell = (props) => {
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

    // grid用日期
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
                style={{height: '100%',overflow: 'auto',}}
                resizable={true}
                data={gridData}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="DELETE" title="刪除" cell={DelCommandCell} width="50px" />      
                <GridColumn field="CHECK_KPI" title="檢核點" cell={textInputCell} />
                <GridColumn field="FINISH_DATE" title="預定完成日期" cell={twDatePickerCell} />
            </Grid>
        </>
    )
    }
export default NewCheckGrid;
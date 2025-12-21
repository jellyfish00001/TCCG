import React, { useState, useRef, useEffect } from "react";
import { Formik, Form, Field } from 'formik';
import Table from '../../../Css/custom/Table.module.css';
import { Pageable } from '../../../Basic/BasicData';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell';
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { getPccmNames, getPccmXls, DownloadPccmXls } from '../../IPC/GDBXls/GDBXlsService';
import { GetHistory } from '../../../Basic/BasicData';
import { getCurrentCycleData, openProjectPrint, CheckIsRDECRole } from '../../../Basic/CommonService';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import TextInput from '../../../Components/Input/TextInput';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { TextAreaCell } from "../../../Components/GridCell/TextAreaCell";
import { handleEditedGridData, IsNullOrEmpty, SetMaskOnOff, FormatDate } from '../../../Basic/SDOExtension';

/**
 * 研究發展作業系統-委託研究計劃
 * 計畫基本資料的
 * 每季評核指標grid
 * 下拉選單寫死
 */

export const ProjectBasicGrid = (props) => {
    const { data } = props;
    const [gridData, setGridData] = useState([]);

    // 年度下拉選單
    const yearOptions = [
        { text: '110年', value: '110' },
        { text: '111年', value: '111' },
        { text: '112年', value: '112' }
    ];
    // 季別下拉選單
    const apartOptions = [
        { text: '第一季', value: '1' },
        { text: '第二季', value: '2' },
        { text: '第三季', value: '3' }
    ];

    // 新增Grid資料行
    const addNew = () => {
        let newData = {
            DETAIL_YY: '112',
            DETAIL_QQ: '1',
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
    
    // grid用input
    const textAreaCell = (props) => {
        return (
            <TextAreaCell
            {...props}
            required={true}
            editable={true}
            rows={3}
            style={{ width: "100%" }}
            onCellInputBlur={onCellInputBlur}
            newDatas={true}
            IsAlwaysEdit={true}
        />
        );
    };

    // grid共用下拉選單
    const DropDownCell = (props, options, itemKey) => {
        // 取方法
        const { gridDropDownChange } = props;
        // 選取改變
        const handleChange = (e) => {
            gridDropDownChange(itemKey, e.target.value, props.dataItem);
        };
        return (
            <DropDownListCell
                {...props}
                name={itemKey}
                ddlData={options}
                textField="text"
                dataItemKey="value"
                editable={true}
                AlwaysEdit={true}
                value={props.dataItem[itemKey]}
                onChange={handleChange}
            />
        );
    };

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
                <GridColumn field="DETAIL_YY" title="年度" cell={props => DropDownCell(props, yearOptions, "DETAIL_YY")} width="100px" />
                <GridColumn field="DETAIL_QQ" title="季別" cell={props => DropDownCell(props, apartOptions, "DETAIL_QQ")} width="100px" />        
                <GridColumn field="CHECK_KPI" title="評核指標" cell={textAreaCell} width="600px"/>
            </Grid>
        </>
    )
}
export default ProjectBasicGrid;
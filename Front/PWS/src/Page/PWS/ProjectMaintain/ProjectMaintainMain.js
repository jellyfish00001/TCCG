import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { getMaintainYear, saveMaintainYear } from "./ProjectMaintainService";
import { getYearList } from "./ProjectMaintainService";
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { SetMaskOnOff } from "../../../Basic/SDOExtension";

const ProjectMaintainMain = () => { 
    //資料庫年度
    const [year, setYear] = useState([]);  
    // 用於追踪編輯過的數據
    const editedGridData = useRef([]);
    // 初始化帶入資料
    useEffect(() => {
        MaintainYear();
    }, []);

    /**
     * 取得年度資料
     */
    const MaintainYear = async () => {
        SetMaskOnOff(true);
        let result = await getMaintainYear();
        setYear(result);
        SetMaskOnOff(false);
    };

    /**
     * 新增
     */
    const addNew = () => {
        // 取得最大年度做新增
        const maxYear = Math.max(...year.map(item => item.PLANYEAR));
        const newYear = (maxYear + 1).toString();
        const newItem = { PLANYEAR: newYear, editType: 1, newItemId: Date.now() };
        editedGridData.current.push(newItem);
        setYear([...year, newItem]);
    };

    /**
     * 刪除
     * @param {*} dataItemToRemove 
     */
    const remove = (dataItemToRemove) => {
        // 只允許刪除新增的行
        if (dataItemToRemove.editType === 1) { 
            const filteredData = year.filter(item => item !== dataItemToRemove);
            setYear(filteredData);
            editedGridData.current = editedGridData.current.filter(item => item.newItemId !== dataItemToRemove.newItemId);
        }
    };

    /**
     * 刪除按鈕
     * @param {*} props 
     * @returns 
     */
    const DelCommandCell = (props) => (
        // 只為新增的行顯示刪除按鈕
        props.dataItem.editType === 1 ? ( 
            <td>
                <Button title={"刪除"} icon='close' look='default' onClick={() => remove(props.dataItem)} />
            </td>
        ) : <td></td>
    );

    /**
     * 新增的年度欄位
     * @param {*} props 
     * @returns 
     */
    const renderDropDownListCell = (props) => (
        <td>
            {props.dataItem[props.field]}
        </td>
    );

    /**
     * 存檔
     */
    const save = async () => {
        SetMaskOnOff(true);
        let saveResult = await saveMaintainYear(editedGridData.current);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message);
            const updatedYear = year.map(item => ({ ...item, editType: 2 }));
            setYear(updatedYear);
        }
        // 清空追踪變更的列表
        editedGridData.current = [];
    };

    return (
        <PageContainer style={{ overflow: "auto", height: "100%" }}>
            <CollapseBoardCard
                button={
                    <Button title="存檔" className="k-button-lighten" onClick={save}>存檔</Button>
                }
                title="維護先期計畫年度"
                isFirstArea={true}
            > 
            <Button type='button' title="新增" onClick={addNew} >新增</Button>
            <Grid
                style={{ overflow: 'auto', height: '100%', width: '30%'}}
                resizable={true}
                data={year}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="Edit" title="刪除" cell={DelCommandCell} width="100"/>
                <GridColumn field="PLANYEAR" title="年度" cell={renderDropDownListCell} />
            </Grid>
            </CollapseBoardCard>
        </PageContainer>
    );
}

export default ProjectMaintainMain;

import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { TextInputCell } from "../../../Components/GridCell/TextInputCell";
import { Checkbox } from '@progress/kendo-react-inputs';
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import Table from '../../../Css/custom/Table.module.css';

export const ExecutionGrid = (props) => { 
     const { executionGridData, setExecutionGridData, exeGridRef } = props;
    // 無歷年預算執行率
    const [NOBUDGETYN, setNOBUDGETYN] = useState(false);
    // 前兩年執行率
    const [ratioAverage, setRatioAverage] = useState(0);
    const firstRef = useRef(true);
    
    /**
     * 取畫面資料
     * @returns 
     */
    const loadData = async () => {
        if(executionGridData.length > 0 && firstRef.current){
            // 無歷年預算執行率
            executionGridData[2].GROWRATIO = "-";
            setNOBUDGETYN(executionGridData[0].NOBUDGETYN);
            firstRef.current = false;
        }
    }

    /**
     * 輸入框輸入
     * @returns 
     */
    const onCellInputChange = () => {
        
    };

    /**
     * checkbox勾選事件
     * @param {*} event //點擊的資料
     * @returns 
     */
    const onCheckboxChange = (event) => {
        setNOBUDGETYN(event.value);
        const updatedData = executionGridData.map(row => ({ ...row, NOBUDGETYN: event.value ? 1 : 0 }));
        exeGridRef.current = updatedData;
        setExecutionGridData(updatedData);
        clean();
    };

    /**
     * 數字輸入框
     * @param {*} event //點擊的資料
     * @param {*} rowIndex //點擊的資料行數
     * @returns 
     */
    const onCellInputBlur = (event, rowIndex) => {
        let newData = [...executionGridData];
        // 更新目前行的數據
        newData[rowIndex] = event;
        // 計算並更新目前行和下一行（如果存在）的預算成長幅度%
        updateGrowthRate(newData, rowIndex);
        // 不是最後一行，則還需要更新下一行的數據
        if (rowIndex < newData.length - 1) {
            updateGrowthRate(newData, rowIndex + 1);
        }
        // 如果目前行不是第一行，則還需要更新前一行的數據
        if (rowIndex > 0) {
            updateGrowthRate(newData, rowIndex - 1);
        }
        // 計算 預算執行率%
        if (newData[rowIndex].PUBLICMONEY > 0) {
            const ratio = newData[rowIndex].EXECOUNT / newData[rowIndex].PUBLICMONEY * 100;;
            newData[rowIndex].RATIO = parseFloat(ratio.toFixed(1));
        } else {
            newData[rowIndex].RATIO = 0;
        }

        setExecutionGridData(newData);
    };
    
    /**
     * 更新預算成長幅度%的函數
     *  @param {*} data //點擊的資料
     * @param {*} index //點擊的資料行數
     * @returns 
     */
    const updateGrowthRate = (data, index) => {
        // 找到目前行的數據
        const currentYearData = data[index];
        // 找到前一年的數據(將年度字串轉換為數字，再減1)
        const previousYearData = data.find(item => item.EXEYEAR === currentYearData.EXEYEAR - 1);
        // 如果前一年的數據存在，且前一年的 PUBLICMONEY 大於 0，則計算成長幅度
        if (previousYearData && previousYearData.PUBLICMONEY > 0) {
            //計算成長幅度
            const growRate = (currentYearData.PUBLICMONEY - previousYearData.PUBLICMONEY) / previousYearData.PUBLICMONEY * 100;
            currentYearData.GROWRATIO = isNaN(growRate) ? 0 : parseFloat(growRate.toFixed(1));
        } else {
            //如果沒有前一年的數據或者前一年的 PUBLICMONEY 為 0，則成長幅度為 0
            currentYearData.GROWRATIO = 0;
            // 無歷年預算執行率
            executionGridData[2].GROWRATIO = "-";
        }
    };
    

    //取畫面資料
    useEffect(() => {
        loadData();
    }, [executionGridData]);

    // 計算前兩年執行率平均值
    useEffect(() => {
        if (executionGridData && executionGridData.length >= 2) {
            // 取得前兩筆資料的 RATIO 並計算平均值
            let total = executionGridData[1].RATIO + executionGridData[2].RATIO;
            let average = total / 2;
            setRatioAverage(average);
        } else {
            // 無資料時，平均值為0
            setRatioAverage(0); 
        }
    }, [executionGridData]);

    // 刪除
    const clean = () => {
        const resetData = exeGridRef.current.map(row => ({
            ...row,
            GROWRATIO: 0,
            RATIO: 0,
            PUBLICMONEY: null,
            EXECOUNT: null,
            EXEDESC: '' 
        }));
        // 無歷年預算執行率
        resetData[2].GROWRATIO = "-";
        setExecutionGridData(resetData);
    }



    /**
     * 數字輸入框
     * @param {*} props
     * @returns 
     */
    const numerCell = (props) => {
    return (
        <NumericTextInputCell
            {...props}
            editable={true}
            disabled={NOBUDGETYN}
            onCellInputBlur={(e) => onCellInputBlur(e, props.dataIndex)}
            onCellInputChange={(e) => onCellInputChange(e, props.dataIndex)}
            AlwaysEdit={true}
            min={0}
            allowNull={true}
        />
    )
    };

    /**
     * 輸入框
     * @param {*} props
     * @returns 
     */
    const textInputCell = (props) => {
        return (
            <TextInputCell
                {...props}
                editable={true}
                disabled={NOBUDGETYN}
                onCellInputBlur={onCellInputBlur}
                AlwaysEdit={true}
            />
        );
    };
    
    return (
        <>
            <PageContainer style={{ overflow: "auto", height: "100%" }}>

                <Grid
                    style={{ overflow: 'auto', height: '100%'}}
                    resizable={true}
                    data={executionGridData}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn field="EXEYEAR" title="年度" editable={false} width="50%" />
                    <GridColumn field="PUBLICMONEY" title="法定預算數(千元)" cell={numerCell} headerCell={props => {
                        return (
                            <CommonTooltip title={props.title} content={
                                <>
                                    包含公務預算及基金預算
                                </>} 
                                position="top"
                            />
                        )
                    }} />
                    <GridColumn field="GROWRATIO" title="預算成長幅度%" className={Table.textAlign_right} headerCell={RequiredHeaderCell} />
                    <GridColumn field="EXECOUNT" title="預算執行數(千元)" cell={numerCell} headerCell={RequiredHeaderCell} />
                    <GridColumn field="RATIO" title="預算執行率%" className={Table.textAlign_right}/>
                    <GridColumn field="EXEDESC" title="預算執行說明" cell={textInputCell} headerCell={props => {
                    return (
                        <CommonTooltip title={props.title} content={
                            <>
                                預算執行數請詳實查填，如執行率未達80%，請敘明原因
                            </>} 
                            position="top"
                        />
                    )
                }} />
                    
                </Grid>
                <div style={{ display: 'flex', alignItems: 'center' }}>
                    <Checkbox label={"無歷年預算執行率"} checked={NOBUDGETYN} onChange={onCheckboxChange} />
                    { !NOBUDGETYN && 
                        <span style={{ marginLeft: '54%' }}>前兩年執行率平均 {ratioAverage.toFixed(1)} </span>
                    }
                </div>
            </PageContainer>
        </>
    );
}
export default ExecutionGrid;
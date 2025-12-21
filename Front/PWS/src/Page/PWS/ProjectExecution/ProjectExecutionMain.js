import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { TextInputCell } from "../../../Components/GridCell/TextInputCell";
import { AddNoColumn } from "../../../Basic/SDOExtension";
import { Checkbox, TextArea } from '@progress/kendo-react-inputs';
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { saveExecution } from "./ProjectExecutionService";
import TextAreaInput from '../../../Components/Input/TextAreaInput';
import Table from '../../../Css/custom/Table.module.css';
import { GetExecution } from "./ProjectExecutionService";


const ProjectExecution = (props) => { 
    // 這裡假設grid資料從外部加載
    const { data } = props;
    const PLANYEAR = 114;   // 這裡假設從外部加載
    const PLANID = 128;     // 這裡假設從外部加載
    // 無歷年預算執行率說明
    const [noBudgetDesc, setNoBudgetDesc] = useState();
    // 無歷年預算執行率
    const [NOBUDGETYN, setNOBUDGETYN] = useState(false);
    // 假資料
    const [gridData, setGridData] = useState();

    //取畫面資料
    const loadData = async () => {
        let data = await GetExecution(PLANID, PLANYEAR);
        if(data[0].NOBUDGETYN){
            setNOBUDGETYN(data[0].NOBUDGETYN);
            setNoBudgetDesc(data[0].NOBUDGETDESC);
        }
        setGridData(data);
    }

    //取畫面資料
    useEffect(() => {
        loadData();
    }, []);
    // 刪除
    const clean = () => {
        const resetData = gridData.map(row => ({
            ...row,
            GROWRATIO: 0,
            RATIO: 0,
            PUBLICMONEY: null,
            EXECOUNT: null,
            EXEDESC: '' 
        }));
        setGridData(resetData);
        setNoBudgetDesc('');
    }
    //存檔驗證
    const validate = async() => {
        // 勾選擇檢查無歷年預算執行率說明是否填寫
        if (NOBUDGETYN && !noBudgetDesc.trim()) {
            showGlobalMessageBox("無歷年預算執行率時，下方說明為必填項目");
            return;
        }
        // 無勾選則判斷上面欄位是否填寫
        if(!NOBUDGETYN){
            // 檢查預算執行率大於 80 的行是否填寫了預算執行說明
            const isExplanationMissing = gridData.some(row => parseFloat(row.RATIO) > 80 && (!row.EXEDESC || row.EXEDESC.trim() === ''));
            if (isExplanationMissing) {
                showGlobalMessageBox("預算執行率大於 80 請填寫預算執行說明");
                return;
            }
            // 檢查所有行是否有填寫
            const isDataIncomplete = gridData.some(row => !row.PUBLICMONEY || !row.EXECOUNT);
            if (isDataIncomplete) {
                showGlobalMessageBox("請填寫完整金額");
                return;
            }
        }
        await save(gridData);
    }

    // 存檔
    const save = async (gridData) => {
        // 如果資料填寫完整，繼續存檔流程
        SetMaskOnOff(true);
        // 存檔前調整欄位
        const savedata = gridData.map(row => ({ 
            ...row, 
            PLANID: PLANID,
            NOBUDGETDESC: NOBUDGETYN ? noBudgetDesc : null,
            NOBUDGETYN: NOBUDGETYN ? 1 : 0
        }));
        let saveResult = await saveExecution(savedata);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message);
        }
    };

    // 輸入框輸入
    const onCellInputChange = (event, rowIndex) => {
        
    };
    //數字輸入框
    const onCellInputBlur = (event, rowIndex) => {
        let newData = [...gridData];

        // 更新目前行的數據
        newData[rowIndex] = event;
        // 計算並更新目前行和下一行（如果存在）的預算成長幅度%
        updateGrowthRate(newData, rowIndex);
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

        setGridData(newData);
    };
    
    // 更新預算成長幅度%的函數
    const updateGrowthRate = (data, index) => {
        // 找到目前行的數據
        const currentYearData = data[index];
        // 找到前一年的數據(將年度字串轉換為數字，再減1)
        const previousYearData = data.find(item => item.EXEYEAR === currentYearData.EXEYEAR - 1);
        // 如果前一年的數據存在，則計算成長幅度
        if (previousYearData && previousYearData.EXECOUNT > 0) {
            //計算成長幅度
            const growRate = (currentYearData.EXECOUNT - previousYearData.EXECOUNT) / previousYearData.EXECOUNT * 100;
            currentYearData.GROWRATIO = isNaN(growRate) ? 0 : parseFloat(growRate.toFixed(1));
        } else {
            currentYearData.GROWRATIO = 0;
        }
    };

    // checkbox勾選事件
    const onCheckboxChange = (event) => {
        setNOBUDGETYN(event.value);
        const updatedData = gridData.map(row => ({ ...row, NOBUDGETYN: event.value ? 1 : 0 }));
        setGridData(updatedData);
        clean();
    };

    //數字輸入框
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

    //輸入框
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
                <CollapseBoardCard
                    button={
                        <>
                            <Button title="存檔" className="k-button-lighten" onClick={validate} >存檔</Button>
                            <Button title="取消" className="k-button-lighten" onClick={clean} >取消</Button>
                        </>
                    }
                title="歷年執行情形"
                isFirstArea={true}
                > 
                <Grid
                    style={{ overflow: 'auto', height: '100%'}}
                    resizable={true}
                    data={gridData}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn field="EXEYEAR" title="年度" editable={false} width="50%" />
                    <GridColumn field="PUBLICMONEY" title="法定預算數(千元)包含公務及基金" cell={numerCell} headerCell={RequiredHeaderCell} width="300%" />
                    <GridColumn field="GROWRATIO" title="預算成長幅度%" className={Table.textAlign_right} headerCell={RequiredHeaderCell} />
                    <GridColumn field="EXECOUNT" title="預算執行數(千元)" cell={numerCell} headerCell={RequiredHeaderCell} />
                    <GridColumn field="RATIO" title="預算執行率%" className={Table.textAlign_right}/>
                    <GridColumn field="EXEDESC" title="預算執行說明" cell={textInputCell} />
                </Grid>
                <Checkbox label={"無歷年預算執行率"} checked={NOBUDGETYN} onChange={onCheckboxChange} />
                <TextArea
                    value={noBudgetDesc}
                    disabled={!NOBUDGETYN}
                    onChange={(e) => setNoBudgetDesc(e.value)}
                />
                </CollapseBoardCard>
            </PageContainer>
        </>
    );
}
export default ProjectExecution;
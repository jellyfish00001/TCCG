import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { Button } from '@progress/kendo-react-buttons';
import  OrgSelectPanel  from '../../../Components/Selector/OrgSelectPanel';
import { saveAs } from "@progress/kendo-drawing/pdf";
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { TextInputCell } from "../../../Components/GridCell/TextInputCell";
import { AddNoColumn } from "../../../Basic/SDOExtension";
import { Checkbox } from '@progress/kendo-react-inputs';

const CProjectExecution = (props) => { 
    // 這裡假設grid資料從外部加載
    const { data } = props;
  
    // 假資料
    const [fakedata, setFakedata] = useState([
        {
            Year: '109',
            Grow: '7.5',
            ExecutionRate: '30.5',
        },
        {
            Year: '108',
            Grow: '71.0',
            ExecutionRate: '65.4',
        },
    ]);
    const griddata = AddNoColumn(fakedata);
    // 刪除
    const clean = () => {

    }
    // 撤銷
    const save = () => {

    }
    // 輸入框輸入
    const onCellInputBlur = (event) => {
        
    };

    //輸入框
    const textInputCell = (props) => {
        return (
            <TextInputCell
            {...props}
            required={true}
            editable={true}
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
                            <Button title="存檔" className="k-button-lighten" onClick={save} >存檔</Button>
                            <Button title="取消" className="k-button-lighten" onClick={clean} >取消</Button>
                        </>
                    }
                title="歷年執行情形"
                isFirstArea={true}
                > 
                <Grid
                    style={{ overflow: 'auto', height: '100%'}}
                    resizable={true}
                    data={griddata}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn field="Year" title="年度" editable={false} width="50"/>
                    <GridColumn field="Budget" title="法定預算數(千元)包含公務及基金" cell={textInputCell} />
                    <GridColumn field="Grow" title="預算成長幅度%" />
                    <GridColumn field="BudgetNum" title="預算執行數(千元)" cell={textInputCell} />
                    <GridColumn field="ExecutionRate" title="預算執行率%" />
                    <GridColumn field="BudgetNote" title="預算執行說明" cell={textInputCell} />
                </Grid>
                </CollapseBoardCard>
            </PageContainer>
        </>
    );
}
export default CProjectExecution;
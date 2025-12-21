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
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';

const CFundingDetailsMain = (props) => { 
    // 這裡假設grid資料從外部加載
    const { data } = props;
    // 假資料
    const [fakedata, setFakedata] = useState([
        {
            CalMethod: '測試1',
            UnitPrice: 0,
            NumberOfItems: 0,
            TotalAmount: 0
        },
        {
            CalMethod: '測試2',
            UnitPrice: 0,
            NumberOfItems: 0,
            TotalAmount: 0
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
    const onCellInputBlur = (item) => {

    }
    //
    const onCellInputChange = (item) => {

    }
    
    // 取得下一個序號
    const getNextNo = () => {
        return (fakedata.length > 0 ? Math.max(...fakedata.map(data => parseInt(data.NO))) + 1 : 1).toString();
    };
    // 新增Grid資料行
    const addNew = () => {
        let newData = {
            NO: getNextNo(),
            ExpDetail: '',
            CalMethod: '',
            UnitPrice: 0,
            NumberOfItems: 0,
            TotalAmount: 0
        };
        setFakedata([...fakedata, newData]);
    };

    // 移除指定Grid資料行
    const remove = (dataItemToRemove) => {
        if (window.confirm("確定要刪除這條記錄嗎？")) {
            const filteredData = fakedata.filter(item => item !== dataItemToRemove);
            setFakedata(filteredData);
        }
    };


    //數字輸入框
    const numerCell = (props) => {
            const value = props.dataItem[props.field];
            const displayValue = (value === null || value === undefined) ? 0 : value;
        return (
            <NumericTextInputCell
                  {...props}
                  editable={true}
                  onCellInputBlur={onCellInputBlur}
                  onCellInputChange={onCellInputChange}
                  AlwaysEdit={true}
                  min={0}
              />
        );
    };

    //文字輸入框
    const textCell = (props) => {
        return (
            <TextInputCell
                  {...props}
                  editable={true}
                  onCellInputBlur={onCellInputBlur}
                  onCellInputChange={onCellInputChange}
                  AlwaysEdit={true}
              />
        );
    };

    //刪除欄位
    const DelCommandCell = (props) => {
        return (
            <CommandCell>
                <Button title={"刪除"} icon='close' look='default' onClick={() => { remove(props.dataItem); }} />
            </CommandCell>
        );
    };

    return (
        <>
            <PageContainer style={{ overflow: "auto", height: "100%" }}>
                <CollapseBoardCard
                    button={
                        <>
                            <Button title="撤銷" className="k-button-lighten" onClick={save} >存檔</Button>
                            <Button title="取消" className="k-button-lighten" onClick={clean} >取消</Button>
                        </>
                    }
                title="經費需求事項"
                isFirstArea={true}
                > 
                <div className='fn-buttons'>
                <Button type='button' title="新增" onClick={addNew} >新增</Button>
                </div>
                <Grid
                    style={{ overflow: 'auto', height: '100%'}}
                    resizable={true}
                    data={griddata}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn field="EDIT" title="編修" cell={DelCommandCell} width="100px" />
                    <GridColumn field="NO" title="序號" editable={false} width="50px"/>
                    <GridColumn field="ExpDetail" title="經費需求細項" cell={textCell} />
                    <GridColumn field="CalMethod" title="計算方式或說明" />
                    <GridColumn field="UnitPrice" title="單價(千元)" cell={numerCell} />
                    <GridColumn field="NumberOfItems" title="數項(千元)" cell={numerCell} />
                    <GridColumn field="TotalAmount" title="總計(千元)" cell={numerCell} />
                </Grid>
                </CollapseBoardCard>
            </PageContainer>
        </>
    );
}
export default CFundingDetailsMain;
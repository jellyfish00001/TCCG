import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { Button } from '@progress/kendo-react-buttons';
import { saveAs } from "@progress/kendo-drawing/pdf";
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { TextInputCell } from "../../../Components/GridCell/TextInputCell";
import { AddNoColumn } from "../../../Basic/SDOExtension";
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import { SaveDAMTB, GetDAMTB } from './FundingDetailService';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";

const FundingDetailsMain = (props) => { 
    // 這裡假設grid資料從外部加載
    const { data } = props;
    // 計畫ID(從外部傳入)
    const PLANID = 128;
    // 加入序號
    const [gridData, setGridData] = useState([]);
    // 取ID資料
    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await GetDAMTB(PLANID);
        setGridData(result);
        SetMaskOnOff(false);
    }

    // 取面資料
    useEffect(() => {
        loadData();
    }, []);
    // 取消
    const clean = () => {

    }
    // 撤銷
    const save = async() => {
        SetMaskOnOff(true);
        const DAMTBListModel = gridData.map(row => ({ 
            ...row, 
            FUNDID: row.NO,
            PLANID: PLANID,
        }));
        //將計畫ID一起傳入
        const dataTosave = {
            PLANID,
            DAMTBListModel
        }
        let saveResult = await SaveDAMTB(dataTosave);
        SetMaskOnOff(false);
        if (saveResult.success) {
          showGlobalMessageBox(saveResult.message);
        }
    }

    // 處理Grid資料行變更
    const onCellInputChange = (event) => {

    }

    // 新增Grid資料行
    const addNew = () => {
        const maxId = gridData.length > 0 ? Math.max(...gridData.map(item => item.NO)) : 0;
        let newData = {
            NO: maxId + 1,
            FUNDDESC: '',
            CALCULATIONDESC: '',
            PRICE: 0,
            AMOUNT: 0,
            FUNDTOT: 0
        };
        setGridData([...gridData, newData]);
    };

    // 移除指定Grid資料行
    const remove = (dataItemToRemove) => {
        const filteredData = gridData.filter(item => item.NO !== dataItemToRemove.NO);
        //重新計算序號
        const newGridData = filteredData.map((item, index) => ({ ...item, NO: index + 1 }));
        setGridData(newGridData);
    };


    //數字輸入框
    const numerCell = (props) => {
        return (
            <NumericTextInputCell
                  {...props}
                  format="n0"
                  editable={true}
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
                            <Button title="撤銷" className="k-button-lighten" type="button" onClick={save} >存檔</Button>
                            <Button title="取消" className="k-button-lighten" type="button" onClick={clean} >取消</Button>
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
                    data={gridData}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn field="EDIT" title="編修" cell={DelCommandCell} width="100px" />
                    <GridColumn field="NO" title="序號" editable={false} width="50px"/>
                    <GridColumn field="FUNDDESC" title="經費需求細項" cell={textCell} />
                    <GridColumn field="CALCULATIONDESC" title="計算方式或說明" />
                    <GridColumn field="PRICE" title="單價(千元)" cell={numerCell} />
                    <GridColumn field="AMOUNT" title="數項(千元)" cell={numerCell} />
                    <GridColumn field="FUNDTOT" title="總計(千元)" cell={numerCell} />
                </Grid>
                </CollapseBoardCard>
            </PageContainer>
        </>
    );
}
export default FundingDetailsMain;


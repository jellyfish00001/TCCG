import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { Button } from '@progress/kendo-react-buttons';
import { TextInputCell } from "../../../Components/GridCell/TextInputCell";
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import Table from '../../../Css/custom/Table.module.css';

export const FundingGrid = (props) => {
    const { fundingGridData, setFundingGridData } = props;

    /**
     * 處理Grid資料行變更
     * @returns 
     */
    const onCellInputChange = () => {

    }
    
    /**
     * 處理Grid資料行失焦變更
     * @param {*} e // 點擊的資料行
     * @returns 
     */
    const onCellInputBlur = (e) => {
        // 計算總計
        const total = e.PRICE * e.AMOUNT;
        const formattedTotal = total.toLocaleString(); 
        const updatedGridData = fundingGridData.map(item => {
            if (item.FUNDID == e.FUNDID) {
                return { ...item, FUNDTOT: formattedTotal };
            }
            return item;
        });
        setFundingGridData(updatedGridData);
    };

    /**
     * 新增Grid資料行
     * @returns 
     */
    const addNew = () => {
        const maxId = fundingGridData.length > 0 ? Math.max(...fundingGridData.map(item => item.FUNDID)) : 0;
        let newData = {
            FUNDID: maxId + 1,
            FUNDDESC: '',
            CALCULATIONDESC: '',
            PRICE: 0,
            AMOUNT: 0,
            FUNDTOT: 0
        };
        setFundingGridData([...fundingGridData, newData]);
    };

    // 移除指定Grid資料行
    const remove = (dataItemToRemove) => {
        const filteredData = fundingGridData.filter(item => item.FUNDID !== dataItemToRemove.FUNDID);
        //重新計算序號
        const newGridData = filteredData.map((item, index) => ({ ...item, FUNDID: index + 1 }));
        setFundingGridData(newGridData);
    };

    /**
     * 數字輸入框
     * @param {*} props
     * @returns 
     */
      const numerCell = (props) => {
        return (
            <NumericTextInputCell
                  {...props}
                  format="n0"
                  editable={true}
                  onCellInputChange={onCellInputChange}
                  onCellInputBlur={onCellInputBlur}
                  AlwaysEdit={true}
                  min={0}
              />
        );
    };

    /**
     * 文字輸入框
     * @param {*} props
     * @returns 
     */
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

    /**
     * 刪除欄位
     * @param {*} props
     * @returns 
     */
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
                <div className='fn-buttons'>
                <span>※經費需求細項填報說明：<br/>
                            1.請分項說明經費需求，並詳細說明計算方式，如填報資料不確實或過於簡略，將不建議核列。<br/>
                            2.經費需求細項之金額總計，須等於提報年度需求數金額。<br/>
                            3.含建築工程之計畫：請敘明總樓地板面積，並依「共同性費用編列標準表」編列，於計算方式欄位詳列算式；<br/>
                            &nbsp;&nbsp;&nbsp;非屬「共同性費用編列標準表」內之項目應分項詳列，經費總計欄應標明每坪單價，如：總計新臺幣○千元(約○千元/坪)。<br/>
                            4.含裝修工程之計畫：請依「共同性費用編列標準表」編列，於計算方式欄位詳列算式，設備費用應分項詳列，不得內含。<br/>
                            5.含用地取得之計畫：請說明用地費用計算方式(例：公告現值倍數或其他)及公有地是否排除取得費用。<br/>
                            6.含資訊費用之計畫：請說明購置資訊設備或軟體，開發應用系統，既有系統擴充或調整，既有軟硬體維護費，電路費等各項費用計算方式。 <br/>
                    </span>
                <Button type='button' title="新增" onClick={addNew} >新增</Button>
                </div>
                <Grid
                    style={{ overflow: 'auto', height: '100%'}}
                    resizable={true}
                    data={fundingGridData}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn field="EDIT" title="刪除" cell={DelCommandCell} width="100px" />
                    <GridColumn field="FUNDID" className={Table.textAlign_center} title="序號" editable={false} width="50px"/>
                    <GridColumn field="FUNDDESC" title="經費需求細項" cell={textCell} />
                    <GridColumn field="CALCULATIONDESC" title="計算方式或說明" cell={textCell}/>
                    <GridColumn field="PRICE" title="單價(千元)" cell={numerCell} />
                    <GridColumn field="AMOUNT" title="數量" cell={numerCell} />
                    <GridColumn field="FUNDTOT" className={Table.textAlign_right} title="總計(千元)" />
                </Grid>
            </PageContainer>
        </>
    );
}

export default FundingGrid;

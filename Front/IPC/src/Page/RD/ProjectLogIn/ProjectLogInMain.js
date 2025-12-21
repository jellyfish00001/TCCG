import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { Button } from '@progress/kendo-react-buttons';

/**
 * 研究發展作業系統-(主辦 才有提報單位、提報人員、"編修"的刪除功能)
 * 計畫登入
 * PPT P9 P10 
 * "編修"帶資料跳入計畫基本資料
 * fakeData: grid假資料測試
 */

const RDMain4 = (props) => { 
    // 這裡假設grid資料從外部加載
    const { data } = props;
    // 假資料
    const [fakedata, setFakedata] = useState([
        {
            PROJECT_NO: "110-0001",
            PROJECT_NAME: "測試專案一",
            REPORT_UNIT: "單位一",
            REPORT_PERSON: "人員甲",
        },
        {
            PROJECT_NO: "110-0002",
            PROJECT_NAME: "測試專案二",
            REPORT_UNIT: "單位二",
            REPORT_PERSON: "人員乙",
        },
        {
            PROJECT_NO: "110-0003",
            PROJECT_NAME: "測試專案三",
            REPORT_UNIT: "單位三",
            REPORT_PERSON: "人員丙",
        },
    ]);

    // 建立計畫基本資料
    const addNewPlan = () => {

    }

    // 移除指定Grid資料行
    const remove = (dataItemToRemove) => {
        if (window.confirm("確定要刪除這條記錄嗎？")) {
            const filteredData = fakedata.filter(item => item !== dataItemToRemove);
            setFakedata(filteredData);
        }
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
            <PageContainer style={{ overflow: "auto", height: "100%" }}>
                <CollapseBoardCard
                title="計畫登入"
                isFirstArea={true}
                >
                    <Button type='button' title="建立計畫基本資料" className="k-button-lighten" onClick={addNewPlan} >建立計畫基本資料</Button>
                    <Grid
                    style={{height: '100%',overflow: 'auto',}}
                    resizable={true}
                    data={fakedata}
                    >
                        <GridNoRecords>無資料</GridNoRecords>  
                        <GridColumn field="EDIT" title="編修" cell={DelCommandCell} width="50px"/>
                        <GridColumn field="PROJECT_NO" title="計畫編號" width="150px"/>
                        <GridColumn field="PROJECT_NAME" title="計畫名稱" />
                        <GridColumn field="REPORT_UNIT" title="提報單位" />
                        <GridColumn field="REPORT_PERSON" title="提報人員" />
                    </Grid>
                </CollapseBoardCard>
            </PageContainer>
        )
        }
        export default RDMain4;
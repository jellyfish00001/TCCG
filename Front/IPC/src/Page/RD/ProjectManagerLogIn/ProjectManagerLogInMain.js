import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { Button } from '@progress/kendo-react-buttons';
import  OrgSelectPanel  from '../../../Components/Selector/OrgSelectPanel';

/**
 * 研究發展作業系統-管理=委託研究計畫管理
 * 計畫登入
 * PPT P16
 * "編修"帶資料跳入計畫基本資料
 * fakeData: grid假資料測試
 * treeData: tree假資料測試
 */

const RDMain5 = (props) => { 
    // 這裡假設grid資料從外部加載
    const { data } = props;
    // 假資料
    const [fakedata, setFakedata] = useState([
        {
            PROJECT_NO: "110-0001",
            PROJECT_NAME: "測試專案一",
        },
        {
            PROJECT_NO: "110-0002",
            PROJECT_NAME: "測試專案二",
        },
        {
            PROJECT_NO: "110-0003",
            PROJECT_NAME: "測試專案三",
        },
    ]);
    // 組織樹資料
    const [treeData, setTreeData] = useState([
        {
            OU_ID: "1",
            title: "桃園市政府",
        }
    ]);

    // 發送稽催填報
    const sendreport = () => {

    }
    // 發送稽催執行情形填報
    const sendexecutionreport = () => {

    }
    // 刪除
    const clean = () => {

    }
    // 撤銷
    const clacel = () => {

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
                <Button title={"編輯"} icon='edit' look='default' onClick={() => {

                }}
                />
                <Button title={"刪除"} icon='close' look='default' onClick={() => { remove(props.dataItem); }} />
            </CommandCell>
        );
    };

    return (
        <>
            <h3 className="k-dialog-titlebar">計畫登入</h3>
            <div style={{ 'display': 'flex' }}>
                <OrgSelectPanel
                    data={treeData}
                    onSelect={(e) => {}}
                    title={'執行機關'}
                    style={{ overflow: "auto" }}
                />
            <PageContainer
                    toolbar={
                        <>
                            <Button title="發送稽催填報" className="k-button-lighten" onClick={sendreport} >發送稽催填報</Button>
                            <Button title="發送稽催執行情形填報" className="k-button-lighten" onClick={sendexecutionreport} >發送稽催執行情形填報</Button>
                            <Button title="刪除" className="k-button-lighten" onClick={clean} >刪除</Button>
                            <Button title="撤銷" className="k-button-lighten" onClick={clacel} >撤銷</Button>
                        </>
                    }
                    style={{ overflow: "auto", height: "100%" }}
                >
                <Grid
                    style={{ overflow: 'auto', height: '100%'}}
                    resizable={true}
                    data={fakedata}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn field="EDIT" title="編修" cell={DelCommandCell} width="100px" />
                    <GridColumn field="PROJECT_NO" title="計畫編號" />
                    <GridColumn field="PROJECT_NAME" title="計畫名稱" />
                </Grid>
            </PageContainer>
            </div>
        </>
    );
}
export default RDMain5;
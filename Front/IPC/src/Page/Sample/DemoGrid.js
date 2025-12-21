import React, { useState, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Slide } from '@progress/kendo-react-animation';
import { Dialog } from '@progress/kendo-react-dialogs';
import { Grid, GridColumn } from '@progress/kendo-react-grid';
import { Tooltip } from '@progress/kendo-react-tooltip';
import { MessageBoxContext } from '../../Components/Dialogs/MessageBox';
import { Pageable } from '../../Basic/BasicData';


// /* 自訂{修改、刪除、檢視}欄位 */  //範例頁面(暫時先額外抽出來)
export const CustomCell = (showSelectItem) => {
    return (
        <td>
            <Button icon="edit" look="bare" title={"修改"} onClick={() => { showSelectItem("修改", this.props.dataItem); }} />
            <Button icon="close" look="bare" title={"刪除"} onClick={() => { showSelectItem("刪除", this.props.dataItem); }} />
            <Button icon="zoom" look="bare" title={"檢視"} onClick={() => { showSelectItem("檢視", this.props.dataItem); }} />
        </td>
    );
}

const DemoGrid = () => {
    const [slideVisible, setSlideVisible] = useState(true)
    const [gridVisible, setGridVisible] = useState(false)
    const [exampleData, SetExampleData] = useState([])
    const [paging, setPaging] = useState({ skip: 0, take: 10 })

    const { showMessage } = useContext(MessageBoxContext);

    const exampleCode = `
    //需import Component
    import { Grid, GridColumn, GridCell } from '@progress/kendo-react-grid';

    <Tooltip openDelay={10}                                                                         //Tooltip為提示工具。openDelay:多久顯示
             position="bottom"                                                                      //position:顯示位置
             anchorElement="target">                                                                //anchorElement:錨點位子
        <Grid
            style={{                                                                                //設定grid樣式
                height: '100%',
                overflow: 'auto'
            }}
            data={exampleData.slice(paging.skip, paging.take + paging.skip)}                        //塞入data
            total={exampleData.length}                                                              //data總數
            skip={paging.skip}                                                                      //從第幾筆資料開始
            take={paging.take}                                                                      //每頁顯示幾筆資料
            pageable={pageable}                                                                     //分頁所需資料
            onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}               //處理所有數據操作(包括分頁，排序，過濾和分組參數)
            <GridColumn cell={CustomCell(showSelectItem)} width={100} />                            //cell:自訂欄位,width:該欄位寬度
            <GridColumn field="DEMO_ID" title="代碼" />                                             //欄位顯示
            <GridColumn field="DEMO_VAL" title="功能名稱"                                           //欄位顯示
            cell={(props) =>                                                                       //自訂欄位(欄位顯示提示訊息)
                <td title={props.dataItem.DEMO_VAL}>
                    {props.dataItem.DEMO_VAL}
                </td>}                                    
        </Grid>
    </Tooltip>`;

    let showSelectItem = (status, data) => {
        showMessage(status + ":" + data.DEMO_ID)
    }

    let showGrid = () => {
        setGridVisible(true)
        let data = [];
        for (var i = 1; i <= 20; i++) {
            data.push({
                DEMO_ID: '代碼' + i.toString().padStart(3, 0),
                DEMO_VAL: Math.floor((Math.random() * 1000000000) + 1).toString().padStart(10, '0').padEnd(50, '*')
            });
        }
        SetExampleData(data)
    }

    return (
        <div className="fnForm">
            <div>
                <Button icon="menu" onClick={() => setSlideVisible(!slideVisible)} style={{ margin: "0 10px", color: "black", background: "none", border: "none" }}></Button>
            </div>
            <div>
                <Slide>
                    {
                        slideVisible &&
                        <div>
                            <ul>
                                <li><a href="https://www.telerik.com/kendo-react-ui/components/grid/" target="_blank" rel="noopener noreferrer">KendoReact Data Grid (Table) Overview</a></li>
                                <Button onClick={showGrid}>Demo</Button>
                            </ul>
                        </div>
                    }
                </Slide>
                {gridVisible && <Dialog onClose={() => setGridVisible(false)} title={"Grid範例"}
                    width='70%' height='60%' >
                    <Tooltip openDelay={10} position="bottom" anchorElement="target">
                        <Grid
                            style={{
                                height: '100%',
                                overflow: 'auto'
                            }}
                            data={exampleData.slice(paging.skip, paging.take + paging.skip)}
                            total={exampleData.length}
                            skip={paging.skip}
                            take={paging.take}
                            pageable={Pageable}
                            onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
                        >
                            <GridColumn cell={CustomCell(showSelectItem)} width={100} />
                            <GridColumn field="DEMO_ID" title="代碼" />
                            <GridColumn field="DEMO_VAL" title="功能名稱"
                                cell={(props) =>
                                    <td title={props.dataItem.DEMO_VAL}>
                                        {props.dataItem.DEMO_VAL}
                                    </td>} />
                        </Grid>
                    </Tooltip>
                </Dialog>}
                <pre>
                    <i>
                        {exampleCode}
                    </i>
                </pre>
            </div>
        </div >
    );
}

export default DemoGrid;

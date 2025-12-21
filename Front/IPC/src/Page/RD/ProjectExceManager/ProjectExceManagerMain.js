import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { FormatDate } from "../../../Basic/SDOExtension";
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import ProjectExceManagerWindow from './ProjectExceManagerWindow';

/**
 * 研究發展作業系統-委託研究計劃-執行情形管理
 * 執行情形管理
 * PPT P13  
 * 計畫登入"編修"帶資料跳出視窗
 * griddata 為本頁面的假資料
 * fakeData 為跳出視窗RDEditWindow的假資料
 */

const RDMain2 = () => {
    // 這裡假設grid資料從外部加載
    const [griddata, setGridData] = useState([
        { NO: 1, PLAN_YEAR: '112', PLAN_QQ: '第三季', PLAN_EVA: '指標1' },
        { NO: 2, PLAN_YEAR: '112', PLAN_QQ: '第四季', PLAN_EVA: '指標2' },
    ]);
    // 傳入Grid的假資料
    const fakeData = {
        1: { PLAN_NO: 'A-001', PLAN_NAME: '計畫名稱A'},
        2: { PLAN_NO: 'B-001', PLAN_NAME: '計畫名稱B'},
    };
    //表單異動資料
    const formRef = useRef();
    //表單資料
    const [formData, setFormData] = useState({});
    // 控制windowBox的開啟
    const [windowVisible, setWindowVisible] = useState(false);
    // 控制windowBox的資料
    const [currentEditingItem, setCurrentEditingItem] = useState(null);

    // 表單提交
    const save = (props) => {

    };

    // 表單取消
    const cancel = (props) => {
        
    };

    //編輯畫面
    const editCell = (props) => {
        return (
            <CommandCell>
            <Button title={"編輯"} icon='edit' look='default' onClick={() => {
                    setCurrentEditingItem(fakeData[props.dataItem.NO]);
                    setWindowVisible(true);
                }}
            />
            </CommandCell>
        )
    }
        
        return (
            <PageContainer
            style={{ overflow: "auto", height: "100%" }}
            >
                <CollapseBoardCard
                button={
                    <>
                        <Button title="存檔" className="k-button-lighten" onClick={save}>存檔</Button>
                        <Button title="取消" className="k-button-lighten" onClick={cancel}>取消</Button>
                    </>
                }
                title="執行情形管理"
                isFirstArea={true}
                >     
                <Grid
                    data={griddata}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn title='編輯' cell={editCell} width="50px" />
                    <GridColumn field="NO" title="序號" width="50px"></GridColumn>
                    <GridColumn field="PLAN_YEAR" title="年度" width="100px"></GridColumn>
                    <GridColumn field="PLAN_QQ" title="季別" width="150px"></GridColumn>
                    <GridColumn field="PLAN_EVA" title="評核指標" width="760px"></GridColumn>
                </Grid>   
                {windowVisible && 
                    <ProjectExceManagerWindow
                        windowData={currentEditingItem}
                        closeWindow={() => {setWindowVisible(false);}}
                    />
                }
                </CollapseBoardCard>
            </PageContainer>
        )
        }
        export default RDMain2;


        
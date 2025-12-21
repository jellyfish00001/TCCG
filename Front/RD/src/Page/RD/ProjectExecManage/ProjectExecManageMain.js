import React, { useState, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { GetSetParam } from "../../../Basic/CommonService";
import { orderBy } from "@progress/kendo-data-query";
import { Pageable } from '../../../Basic/BasicData';

import ProjectExecManageWindow from './ProjectExecManageWindow';
import { GetRDResPolicyList, GetRDResPolicyIndex } from './ProjectExecManageService';

/**
 * 研究發展作業系統-委託研究計畫-執行情形填報清單
 */
const ProjectExceManageMain = (props) => {
    const {
        location: {
            state: {
                PLAN_NO,
                PLAN_NAME,
                funRole,
            }
        }
    } = props;
    // grid data
    const [gridData, setGridData] = useState([]);
    // grid 分頁
    const [paging, setPaging] = React.useState({ skip: 0, take: 10 });
    // grid 排序
    const [sort, setSort] = React.useState([{ field: "", dir: "" }]);
    // 觸發事件是否為編輯
    const [isEdit, setIsEdit] = useState(true)
    // 執行進度下拉選單
    const [ddlData, setDdlData] = useState()
    // 控制 windowBox 的開啟
    const [windowVisible, setWindowVisible] = useState(false);
    // 控制 windowBox 的資料
    const [currentEditingItem, setCurrentEditingItem] = useState(null);
    /**
     * 頁面載入 grid data 資料
     */
    const loadGridData = async () => {
        SetMaskOnOff(true);
        // 取得執行情形填報清單資料
        let data = await GetRDResPolicyList(PLAN_NO);
        // 取得執行進度下拉選單（Window 內需要用到）
        let progressTypeDropDownList = await GetSetParam("RDProgressType"); 
        progressTypeDropDownList.unshift({SET_VALUE:"請選擇", SET_TYPE:""});
        setDdlData([...progressTypeDropDownList])
        // 設置 Grid 資料        
        setGridData(data)
        SetMaskOnOff(false);
    }
    useEffect(() => {
        loadGridData();
    },[])
    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };
    /**
     * 透過執行情形流水號取得 Window 資料（編輯及查看使用）
     * @param {int} seq 執行情形流水號
     * @param {boolean} isTrigByEdit 觸發事件是否為編輯
     * @return {*} 
     */
    const getWinDataBySEQ = async (seq, isTrigByEdit = true) => {
        SetMaskOnOff(true);
        // 設定是否為編輯
        setIsEdit(isTrigByEdit);
        // 清空 Window 內資料
        setCurrentEditingItem({});
        // 取得執行情形填報明細資料
        let data = await GetRDResPolicyIndex(seq);
        // Window 內要顯示的資料
        let item = {
            ...data,
            SEQ: seq,  // 要透過 SEQ 存檔明細
            PLAN_NAME: PLAN_NAME,
            PLAN_NO: PLAN_NO
        }
        
        setCurrentEditingItem(item);
        SetMaskOnOff(false);
    }
    /**
     * 編輯按鈕 Cell
     * @param {*} props
     * @return {*} 
     */
    const editCell = (props) => {
        return (
            <CommandCell>
            { 
                /* 基本資料審核狀態是審核通過(3)，以及執行情形審核狀態是未送審(1)或審核退回(4)，可以使用編輯 */
                (
                    props.dataItem.RESEARCH_STATUS === '3'
                    &&
                    (props.dataItem.POLICY_STATUS === '1'
                    ||
                    props.dataItem.POLICY_STATUS === '4')
                )
                ?
                <Button title={"編輯"} icon='edit' look='default' onClick={ () => {
                    // 透過執行情形流水號取得 Window 資料
                    getWinDataBySEQ(props.dataItem.SEQ)
                    setWindowVisible(true);
                }}/>
                :
                null
            }
            </CommandCell>
        )
    }
    /**
     * 查看按鈕 Cell
     * @param {*} props
     * @return {*} 
     */
    const viewCell = (props) => {
        return (
            <CommandCell>
                <Button title={"查看"} icon='eye' look='default' onClick={() => {
                        getWinDataBySEQ(props.dataItem.SEQ, false)
                        setWindowVisible(true);
                    }}
                />
            </CommandCell>
        )
    }
    return (
        <PageContainer style={{ overflow: "auto", height: "100%" }} >
            <CollapseBoardCard
                title="執行情形填報"
                isFirstArea={true}
                >     
            <Grid
                style={{ overflow: 'auto', height: '100%'}}
                resizable={true}
                data={gridData.slice(paging.skip, paging.take + paging.skip)}
                total={gridData.length}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                sort={sort}
                onSortChange={sortChange}
                sortable={{ allowUnsort: true, mode: "single" }}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take})}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn title='編輯' cell={editCell} width="50px" />
                <GridColumn title='查看' cell={viewCell} width="50px" />
                <GridColumn field="NO" title="序號" width="50px" />
                <GridColumn field="POLICY_KIND" title="評核類別" />
                <GridColumn field="POLICY_INDEX_DESC" title="評核項目" />
                <GridColumn field="RES_FINISH_DATE" title="預定完成期程" />
                <GridColumn field="STATUS" title="狀態" width="100px" />
            </Grid>   
            {windowVisible && 
                <ProjectExecManageWindow
                    ddlData={ddlData}                           // 執行進度下拉選單
                    windowData={currentEditingItem}            // 執行情形明細
                    setWindowVisible={                        // 設置 Window 開關
                        () => {setWindowVisible(false);}
                    }
                    isEdit={isEdit}                       // 是否透過編輯觸發打開 Window
                />
            }
            </CollapseBoardCard>
        </PageContainer>
    )
    }
    export default ProjectExceManageMain;


        
import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { FormatDate, SetMaskOnOff, IsNullOrEmpty } from "../../../Basic/SDOExtension";
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { getPlanYearList, getMonthList } from "../../../Basic/CommonService";
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { Pageable } from '../../../Basic/BasicData';
import { orderBy } from "@progress/kendo-data-query";

import ProjectExtensionWindow from "./ProjectExtensionWindow";
import { GetRDExtensionList, GetRDExtension, SaveRDExtension } from "./ProjectExtensionService";

/**
 * 研究發展作業系統-委託研究計畫-展延申請
 */
const ProjectExtensionMain = (props) => {
    const {
        location: {
            state: {
                PLAN_NO,
                PLAN_NAME,
                PLAN_ID,
                funRole
            }
        }
    } = props;
    // grid data
    const [gridData, setGridData] = useState([])
    // grid 分頁
    const [paging, setPaging] = React.useState({ skip: 0, take: 10 });
    // grid 排序
    const [sort, setSort] = React.useState([{ field: "", dir: "" }]);
    // 控制windowBox的開啟
    const [windowVisible, setWindowVisible] = useState(false);
    // 控制windowBox的資料
    const [currentEditingItem, setCurrentEditingItem] = useState(null);
    // 是否隱藏功能鍵
    const [isFuncDisable, setIsFuncDisable] = useState(true);
    // 下拉選單資料
    const [ddlData, setDDLData] = useState({
        PLAN_YEAR: [], // 調整計畫期程 年度
        PLAN_MONTH: [] // 調整計畫期程 月份
    });
    /**
     * 頁面載入 grid data 資料
     */
    const loadGridData = async () => {
        SetMaskOnOff(true);
        // 透過計畫編號取得延展紀錄清單
        let data = await GetRDExtensionList(PLAN_NO);
        // 評核指標無資料
        if(data.length === 0){
            // 啟用功能鍵
            setIsFuncDisable(false);
        }
        else{
            // 評核指標和展延有資料
            if(!IsNullOrEmpty(data[0].EXTENSION_NO)){
                // 設置 Grid Data
                setGridData(data);
            }
            // 評核指標全部審核通過就不可以申請展延以及編輯展延紀錄
            if(data[0].RD_RES_POLICY_INDEX_COUNT !== data[0].STATUS_3_COUNT){
                // 啟用功能鍵
                setIsFuncDisable(false);
            }
        }
        
        /** 載入下拉選單資料（年度、月份） **/
        // 年分
        let yearDropDownList = await getPlanYearList(false, 20, "A");
        yearDropDownList.unshift({text:"請選擇", value:""});
        // 月份
        let monthDropDownList = getMonthList();
        monthDropDownList.unshift({text:"請選擇", value:""});
        setDDLData({
            PLAN_YEAR: [...yearDropDownList],
            PLAN_MONTH: [...monthDropDownList]
        });
        SetMaskOnOff(false);
    }
    useEffect(() => {
        loadGridData();
    }, []);

    /**
     * 新增展延申請
     */
    const sendApplication = async () => {
        showGlobalConfirmBox("是否確定新增展延申請？", () => sendApplicationEvent())
    }
    /**
     * 新增展延申請，打開 Window 事件
     * @return {*} 
     */
    const sendApplicationEvent = async () => {
        SetMaskOnOff(true);
        // 取得系統年
        const systemDate = FormatDate(new Date(), 'tYY/MM/DD');
        const year = systemDate.split('/')[0];
        let data = {
            EXTENSION_YEAR: year,
            PLAN_NO:PLAN_NO
        }
        // 建立展延紀錄資料，回傳申請展延編號以及評核指標
        let saveResult = await SaveRDExtension(data);
        // 透過展延編號取得展延紀錄明細資料
        let result = await GetRDExtension(saveResult.data);
        // Window 裡可以帶入的值
        let item = {
            ...result,                       // 其餘展延紀錄明細資料
            PLAN_NAME: PLAN_NAME,            // 計畫名稱
            PLAN_NO: PLAN_NO,                // 計畫編號
            PLAN_ID: PLAN_ID,                // 計畫序號
            EXTENSION_NO: saveResult.data,   // 展延編號
        }
        setCurrentEditingItem(item);
        // 打開 Window
        setWindowVisible(true)
        SetMaskOnOff(false);
    }
    /**
     * 取得 Window 資料
     * @param {object} dataItem
     * @return {*} 
     */
    const getWinData = async (dataItem) => {
        SetMaskOnOff(true);
        // 透過展延編號取得展延紀錄明細資料
        let result = await GetRDExtension(dataItem.EXTENSION_NO);
        // Window 裡可以帶入的值
        let data = {
            ...result,                             // 其餘展延紀錄明細資料
            PLAN_NAME: PLAN_NAME,                  // 計畫名稱
            PLAN_NO: PLAN_NO,                      // 計畫編號
            PLAN_ID: PLAN_ID,                      // 計畫序號
            EXTENSION_NO: dataItem.EXTENSION_NO,   // 展延編號
        }
        setCurrentEditingItem(data);
        SetMaskOnOff(false);
    }
    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };
    /**
     * 編輯按鈕 Cell
     * @param {*} props
     * @return {*} 
     */
    const editCell = (props) => {
        return (
            <CommandCell>
                {
                    (!isFuncDisable && (props.dataItem.EXTENSION_STATUS_CODE === "1" || props.dataItem.EXTENSION_STATUS_CODE === "4"))
                    ?
                    <Button title={"編輯"} icon='edit' look='default' onClick={ async () => {
                        // 透過展延編號取得 Window 資料
                        await getWinData(props.dataItem);
                        setWindowVisible(true);
                    }}/>
                    :
                    null
                }
            </CommandCell>
        )
    }
    return (
        <PageContainer>
            <CollapseBoardCard
                button={
                    <>
                    {
                        isFuncDisable
                        ?
                        null
                        :
                        <Button title="申請展延" className="k-button-lighten" onClick={() =>{sendApplication()}}>申請展延</Button>
                    }
                    </>
                }
                title="展延紀錄"
                isFirstArea={true}
            >     
            <Grid
                style={{ overflow: 'auto', height: '100%'}}
                resizable={true}
                data={gridData.slice(paging.skip, paging.take + paging.skip)}
                total={gridData.length}
                sort={sort}
                onSortChange={sortChange}
                sortable={{ allowUnsort: true, mode: "single" }}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take})}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn title='編輯' cell={editCell} width="50px" />
                <GridColumn field="NO" title="項次" width="50px" />
                <GridColumn field="EXTENSION_NO" title="展延編號" />
                <GridColumn field="ORG_PLAN_DATE" title="原計畫期程" />
                <GridColumn field="ADJ_PLAN_DATE" title="調整計畫期程" />
                <GridColumn field="APPLY_DATE" title="申請時間" />
                <GridColumn field="APPLICANT" title="申請人" />
                <GridColumn field="EXTENSION_STATUS" title="展延申請狀態" />
            </Grid>   
                {windowVisible && 
                    <ProjectExtensionWindow
                        ddlData={ddlData}
                        windowData={currentEditingItem}
                        setWindowVisible=
                        {
                            () => {
                                // 關閉 Window
                                setWindowVisible(false);
                                // 清框 Window 內的值
                                setCurrentEditingItem(null)
                            }
                        }
                    />
                }
            </CollapseBoardCard>
        </PageContainer>
    )
    }
    export default ProjectExtensionMain;
import React, { useEffect, useState ,useRef} from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Pageable } from '../../../Basic/BasicData';
import { orderBy } from "@progress/kendo-data-query";
import '../../../Css/custom/Grid-Td-WordWrap.css';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { Button } from "@progress/kendo-react-buttons";
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { showGlobalMessageBox, showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import { openProjectChapter, DeleteInnProject, downProjectPrint } from './InnProjectManageService';

const InnProjectManageGrid = (props) => {
    let { data, height, additionalCmdCols, isManage, closeDate} = props

    // gridData
    const [gridData, setGridData] = useState([]);
    // grid分頁
    const [paging, setPaging] = useState({ skip: 0, take: 20 });
    
    const exportData=useRef([]);

    // grid排序
    const [sort, setSort] = useState([
        {
            field: "",
            dir: "",
        },
    ]);

    useEffect(() => {
        if (data != null) {
            setGridData([...data])
        }
        setPaging({ ...paging, skip: 0 });
    }, [data])

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    const Deletsave = async (data) => {
        SetMaskOnOff(true);
        let saveResult = await DeleteInnProject(data);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                window.location.reload()
            });
        }
        SetMaskOnOff(false);
    }

    /**
     * 定義指令欄 -編修按鈕
     * @param {*} props Grid傳入的參數
     * @returns CommandCell
     */
    const _CommandCell = (props) => {
        //是否可以編輯及刪除
        const disableButtons = !isManage && new Date(closeDate) < new Date();
        return (
            <CommandCell>
                <Button
                    title={"編修"}
                    icon={'edit'}
                    look='default'
                    disabled={disableButtons}
                    onClick={() => {
                        let item = {
                            PLAN_NO: props.dataItem.INN_PLAN_NO,
                            PLAN_NAME: props.dataItem.INN_PLAN_NAME,
                            IsManage:isManage
                        }
                        openProjectChapter(item);
                
                    }} />
                <Button
                    title={"刪除"}
                    icon={'delete'}
                    look='default'
                    disabled={disableButtons}
                    onClick={() => {
                        let saveData = {
                            INN_PLAN_NO: props.dataItem.INN_PLAN_NO,
                            editType: 3
                        }
                        showGlobalConfirmBox("是否確定刪除計畫？", () => Deletsave(saveData))

                    }} />
                <Button
                    title={"匯出"}
                    icon={'import'}
                    look='default'
                    onClick={() => {
                        exportData.current.push(props.dataItem.INN_PLAN_NO);
                        let model={
                            year:props.dataItem.INN_YEAR,
                            INN_PLAN_NO:exportData.current,
                            FILE_NAME: '創新提案'
                        }
                        downProjectPrint(model).then(() => {
                            // 清空 exportData
                            exportData.current = [];
                        });
                    }} />
            </CommandCell>
        )
    }

    let additionalCmdColsObj =additionalCmdCols.map(item =>
        <GridColumn cell={item.cell} title={item.title} width={item.width ? item.width : "50px"}
        />) 



    return (
        <>
            <Grid
                data={gridData.slice(paging.skip, paging.take + paging.skip)}
                style={{
                    textAlign: "center",
                    height: height ? height : '100%',
                    overflow: 'auto',
                }}
                sort={sort}
                onSortChange={sortChange}
                sortable={{ allowUnsort: true, mode: "single" }}
                total={gridData.length}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
            >
                <GridNoRecords>無資料</GridNoRecords>
                {/* 傳入之功能Grid欄位 */}
                {additionalCmdCols && additionalCmdColsObj}
                <GridColumn cell={_CommandCell} title='編修' width="150px" />
                <GridColumn field="INN_PLAN_NO" title="提案編號" width="110px" />
                <GridColumn field="INN_PLAN_NAME" title="提案名稱" width="300px" />
                <GridColumn field="CODE_VALUE" title="主要提案類別" width="200px" />
                <GridColumn field="SPONSOR_NAME" title="提案人" width="110px" />
                <GridColumn field="SPONSOR_ORG_UNIT" title="提案機關(單位)" />
            </Grid>
        </>
    )
}

export default InnProjectManageGrid;
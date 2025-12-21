import React, { useState, useRef, useEffect } from "react";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { openProjectChapter } from "../ProjectChapter/ProjectChapterService";
import { Pageable } from '../../../Basic/BasicData';
import { orderBy } from "@progress/kendo-data-query";
import CheckBoxList from "../../../Components/Input/CheckBoxList";

export const ProjectListGrid = (props) => {
    const { 
            selectResultData,   // 表單資料
            GridDataForExport,  // 匯出資料
            checkdata,          // 勾選框改變
        } = props;

   // 表單資料
    const [gridData, setGridData] = useState([]);
    // grid分頁
    const [paging, setPaging] = useState({ skip: 0, take: 20 });
    // grid排序
    const [sort, setSort] = useState([
        {
            field: "",
            dir: "",
        },
    ]);
    
    // 勾選框改變
    const setCheck = (e) => {
        if ( e.value ) {
            checkdata.current.push({
                PLANNO: e.dataItem.PLANNO,
                IS_SEND: e.dataItem.IS_SEND,
                PLANKIND: e.dataItem.PLANKIND,
            });
        } else {
            checkdata.current = checkdata.current.filter(item => item !== e.dataItem);
        }
    }

    // 設定表單資料
    useEffect(() => {
        setGridData(selectResultData);
        setPaging({...paging, skip: 0});
    }, [selectResultData]);

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    //編輯欄位
    const DelCommandCell = (props) => {
        return (
            <CommandCell>
                <div style={{ display: 'flex', alignItems: 'center' }}>
                    <CheckBoxList
                        group='CheckBoxList'
                        valueField='id'
                        data={[
                            { 
                                PLANNO: props.dataItem.PLANNO,
                                IS_SEND: props.dataItem.IS_SEND,
                                PLANKIND: props.dataItem.PLANKIND,
                            }
                        ]}
                        onChange={(e) => { setCheck(e); }}
                    />
                    <Button title={"編輯"} icon='edit' look='default' onClick={() => {
                        openProjectChapter(props.dataItem, 0);
                    }} />
                </div>
            </CommandCell>
        );
    };

    return (
        <Grid
                data={gridData.slice(paging.skip, paging.take + paging.skip)}
                exportData={gridData}
                style={{
                    textAlign: "center",
                    height:'100%',
                    overflow: 'auto',
                }}
                sort={sort}
                onSortChange={sortChange}
                sortable={{ allowUnsort: true, mode: "single" }}
                ref={(e) => {
                    if (e != null) {
                        // 組合欄位
                        e.props.data.forEach((item) => {
                            item.UNIT = item.ORGOUNAME + " (" + item.UNITOUNAME + ")";
                        });
                        // 除去編輯欄位
                        const filteredColumns = e.columns.filter(column => column.field !== "EDIT");
                        GridDataForExport.current.Columns = filteredColumns;
                        GridDataForExport.current.props = e.props;
                    }
                }}
                total={gridData.length}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
        >
            <GridNoRecords>無資料</GridNoRecords>
            <GridColumn field="EDIT" title="編修" cell={DelCommandCell} width="100px" />
            <GridColumn field="PLANNO" title="編號" width="100" />
            <GridColumn field="PLANNAME" title="計畫名稱" />
            <GridColumn field="PLANKINDNAME" title="計畫類別" width="100" />
            <GridColumn field="UNIT" title="提報機關(單位)" width="200" cell={(props) => (
                <td>
                    {props.dataItem.ORGOUNAME} ({props.dataItem.UNITOUNAME})
                </td>
            )}/>
            <GridColumn field="SEND_TYPE" title="計畫狀態" width="100" />
        </Grid>
    )
    }
export default ProjectListGrid;
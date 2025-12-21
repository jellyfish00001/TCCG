import React, { useState, useEffect } from "react";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { openProjectChapter } from "../ProjectChapter/ProjectChapterService";
import { Pageable } from '../../../Basic/BasicData';
import { orderBy } from "@progress/kendo-data-query";

export const ReviewProjectGrid = (props) => {
    const { selectResultData, setSelectResultData } = props;

    // grid分頁
    const [paging, setPaging] = useState({ skip: 0, take: 20 });

    // grid排序
    const [sort, setSort] = useState([
        {
            field: "",
            dir: "",
        },
    ]);
    
    // 設定表單資料
    useEffect(() => {
        setSelectResultData(selectResultData);
        setPaging({...paging, skip: 0});
    }, [selectResultData]);

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setSelectResultData(orderBy(selectResultData, event.sort));
        setSort(event.sort);
    };

    /**
     * 刪除欄位
     * @param {*} props 
     * @returns 
     */
    const DelCommandCell = (props) => {
        return (
            <CommandCell>
                <Button title={"編輯"} icon='edit' look='default' onClick={() => {
                    openProjectChapter(props.dataItem, 5);
                }}
                />
            </CommandCell>
        );
    };

    return (
        <Grid
            data={selectResultData.slice(paging.skip, paging.take + paging.skip)}
            style={{
                textAlign: "center",
                height:'100%',
                overflow: 'auto',
            }}
            sort={sort}
            onSortChange={sortChange}
            sortable={{ allowUnsort: true, mode: "single" }}
            total={selectResultData.length}
            skip={paging.skip}
            take={paging.take}
            pageable={Pageable}
            onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
        >
            <GridNoRecords>無資料</GridNoRecords>
            <GridColumn field="Check" title="審核" cell={DelCommandCell} width="100px" />
            <GridColumn field="PLANNO" title="編號" width="100" />
            <GridColumn field="PLANORDERNUMBER" title="優先順序" width="100" />
            <GridColumn field="PLANNAME" title="計畫名稱" />
            <GridColumn field="PLANDATETYPE" title="計畫性質" width="200"/>
            <GridColumn field="SENDTYPE" title="計畫狀態(單位)" width="100" />
        </Grid>
    )
    }
export default ReviewProjectGrid;
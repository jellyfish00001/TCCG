import React, { useEffect, useState } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Pageable } from '../../../Basic/BasicData';
import { orderBy } from "@progress/kendo-data-query";
import '../../../Css/custom/Grid-Td-WordWrap.css';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { Button } from "@progress/kendo-react-buttons";



// 資料登錄、計畫審查清單 共用Grid元件
const InnProjectManageGrid = (props) => {
    let { additionalCmdCols, data, GridDataForExport, reloadGrid, height, funRole } = props

    // gridData
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

    useEffect(() => {
        setGridData([...data])
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

    /**
     * 定義指令欄 -編修按鈕
     * @param {*} props Grid傳入的參數
     * @returns CommandCell
     */
    const _CommandCell = (props) => {
            return (
                <CommandCell>
                    <Button
                        title={"編修"}
                        icon={'edit'}
                        look='default'
                        onClick={() => {

                        }} />
                    <Button
                        title={"刪除"}
                        icon={'delete'}
                        look='default'
                        onClick={() => {

                        }} />
                    <Button
                        title={"下載"}
                        icon={'import'}
                        look='default'
                        onClick={() => {

                        }} />
                </CommandCell>
            )
    }


    return (
        <>
            <Grid
                data={gridData.slice(paging.skip, paging.take + paging.skip)}
                exportData={gridData}
                style={{
                    textAlign: "center",
                    height: height ? height : '100%',
                    overflow: 'auto',
                }}
                sort={sort}
                onSortChange={sortChange}
                sortable={{ allowUnsort: true, mode: "single" }}
                ref={(e) => {
                    if (e != null) {
                        GridDataForExport.current.Columns = e.columns;
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
                {/* 傳入之功能Grid欄位 */}
                <GridColumn cell={_CommandCell} title='編修' width="150px" />
                <GridColumn field="PLAN_NO" title="提案編號" width="110px" />
                <GridColumn field="PLAN_NAME" title="提案名稱" width="110px" />
                <GridColumn field="PLAN_CLASS" title="主要提案類別" width="200px" />
                <GridColumn field="PLAN_MAN" title="提案人" width="110px" />
                <GridColumn field="PLAN_ORG" title="提案機關(單位)" />
            </Grid>

        </>
    )
}

export default InnProjectManageGrid;
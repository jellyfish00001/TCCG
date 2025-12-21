import React, { useState, useEffect } from 'react';
import { Pageable } from '../../../../Basic/BasicData';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import ReactHtmlParser from 'react-html-parser';
// 標案系統執行進度grid
const PCCProgressGrid = props => {
    const { data, headers } = props
    const [paging, setPaging] = useState({ skip: 0, take: 20 })
    const [gridData, setGridData] = useState([])

    // 動態產生GridColsJsx
    const genDynamicCols = () => {

        const headerColKeys = Object.keys(headers);
        const headerColValues = Object.values(headers);
        const gridColsJsx = headerColKeys.map((key, i) => {
            // 每3個字插入換行符號
            let parts = headerColValues[i].match(/.{1,3}/g);
            let val = parts.join('<br />');
            return (
                <GridColumn field={key} title={ReactHtmlParser(val)} ></GridColumn>
            )
        })
        return gridColsJsx
    }

    useEffect(() => {
        if (data) {
            setGridData(data)
        }
    }, [data])

    return (
        <Grid
            data={gridData.slice(paging.skip, paging.take + paging.skip)}
            resizable={true}
            skip={paging.skip}
            take={paging.take}
            pageable={Pageable}
            total={gridData.length}
            onPageChange={(e) => {
                setPaging({ skip: e.page.skip, take: e.page.take })
            }}
        >
            <GridNoRecords>無資料</GridNoRecords>
            {genDynamicCols()}
        </Grid>
    )
}

export default PCCProgressGrid;
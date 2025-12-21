import React, { useState, useEffect } from "react";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Pageable } from '../../../Basic/BasicData';
import { orderBy } from "@progress/kendo-data-query";
import { TextInputCell } from '../../../Components/GridCell/TextInputCell'; 

export const AdjustNumberGrid = (props) => {
    const { selectResultData, setSelectResultData, funRole } = props;

    // grid分頁
    const [paging, setPaging] = useState({ skip: 0, take: 100 });

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

    // grid用input
    const onCellInputBlur = (e) => {
    
    };

    /**
     * grid用輸入框
     * @param {*} prop 
     */
    const textInputCell = (props) => {
        // 未送出不顯示
        if (props.dataItem.IS_SEND == 0 ) {
            return <td></td>;
        } 
        // 已送審不可編輯
        if(props.dataItem.SEND_STATUS == 1){
            return (
            <td>{ props.dataItem.PLANORDERNUMBER }</td>
            );
        }
        else {
            return (
                <TextInputCell
                    {...props}
                    editable={true}
                    onCellInputBlur={onCellInputBlur}
                    onCellInchange={onCellInputBlur}
                    AlwaysEdit={true}
                />
            );
        }
    };

    return (
        <Grid
            data={selectResultData.slice(paging.skip, paging.take + paging.skip)}
            exportData={selectResultData}
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
            <GridColumn field="PLANNO" title="計畫編號" width="100" />
            <GridColumn field="PLANNAME" title="計畫名稱" />
            <GridColumn field="PLANKINDNAME" title="計畫類別" width="100" />
            <GridColumn field="UNIT" title="提報機關(單位)" width="200" cell={(props) => (
                <td>
                    {props.dataItem.ORGOUNAME} ({props.dataItem.UNITOUNAME})
                </td>
            )}/>
            <GridColumn field="PLANORDERNUMBER" title="順序" width="50" cell={textInputCell}/>
            <GridColumn field="SENDTYPE" title="計畫狀態" width="100" />
        </Grid>
    )
    }
export default AdjustNumberGrid;
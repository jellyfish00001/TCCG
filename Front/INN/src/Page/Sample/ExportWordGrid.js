import React, { useState, useEffect } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn } from '@progress/kendo-react-grid';
import { AddNoColumn } from '../../Basic/SDOExtension';
import { apiFetch } from '../../Basic/ApiFetch';
import { ServerConfig } from '../../Basic/BasicData';
import { DropDownListWithValue } from '../../Components/Dropdowns/DropDownListWithValue';
import {getGlobalServerConfig} from '../../Route/RootMiddleware'

export const ExportWordGrid  =(props)=>{

    const [skip, setSkip] = useState(0);
    const [take, setTake] = useState(10);
    const [totalCount, setTotalCount] = useState(10);
    const [paramItemData, setParamItemData] = useState([] ); // grid資料

    const sort= [
        { field: 'SET_ITEM', dir: 'asc' }

    ];
    const [sortSetting,setSortSetting]=useState(sort);

    const pageable = {
        buttonCount: 5,
        info: true,
        /**@type {object} */
        type: 'numeric',
        pageSizes: true,
        previousNext: true
    }
    // 煥頁
    const pageChange = (event) => {
        setTake(event.page.take);
        setSkip(event.page.skip);
    }
    useEffect(() => {
        // console.log('skip:'+skip+' ' + 'take:'+take)
        const fetch= async(skip,take)=>{
            await getParamItemData(
                skip,
                take,
                sortSetting.length>0 ? sortSetting[0].field:null,
                sortSetting.length>0 ? sortSetting[0].dir:null
                );
           }
        fetch(skip,take);
    }, [skip,take])

    const RowClick=(props)=>{
        const data = paramItemData.map(item => (
            { ...item,inEdit:  props.dataItem.SET_ITEM===item.SET_ITEM && item.inEdit !=true ?  true:false }
        ));
        setParamItemData(data);

    }
    const itemChange =(event)=>{
        const data = paramItemData.map(item =>
        item.SET_ITEM === event.dataItem.SET_ITEM
            ? { ...item, [event.field]: event.value }
            : item
        );
        setParamItemData(data);
    };
    const dropchange=(event)=>{
        const props =event.target.props
        itemChange({
            dataItem: props.dataItem,
            field: props.field,
            syntheticEvent: event.syntheticEvent,
            value:event.value[props.field]
        });
          
    }

    const onSortChange=async (e)=>{
        console.log(e.sort);
        setSortSetting(e.sort);
        if(e.sort.length>0){
            await getParamItemData(skip,take,e.sort[0].field,e.sort[0].dir);
        }else{
            await getParamItemData(skip,take);
        }
        // console.log(sortSetting);
    }





    const exitEdit = () => {
          const data = paramItemData.map(item => (
            { ...item,inEdit: item.inEdit==true? false :false}
        ));
        setParamItemData(data);
    }

   const rowRender = (trElement) => {
        const trProps = {
            ...trElement.props,
            onMouseDown: (e) => {
                console.log(e);


                //clearTimeout(this.preventExitTimeout);
                //this.preventExitTimeout = setTimeout(() => { this.preventExit = undefined; });
            },
            onclick: (e) => {
                console.log(e);
                exitEdit();
              //  clearTimeout(this.blurTimeout);
              //  if (!this.preventExit) {
               //     this.blurTimeout = setTimeout(() => { this.exitEdit(); });
              //  }
            },
           // onFocus: () => { clearTimeout(this.blurTimeout); }
        };
        return React.cloneElement(trElement, { ...trProps }, trElement.props.children);
    }

        // 取得參數類別資料
     const getParamItemData = async (skip,take,orderByField,dir) => {
            // let url = ServerConfig.backEndUrl + `SetParamItem?skip=${skip}&take+${take}`
            let url = getGlobalServerConfig().backEndUrl.get() + `SetParamItem/${skip}/${take}`
            console.log(orderByField);
            console.log(dir);

            url+=orderByField  ? `/${orderByField.toLowerCase()}`:"";
            url+=dir  ? `/${dir}`:"";
            console.log(url);

            let paramItemData = {};
            
            let request = new Request(url);
            try {
                let response = await apiFetch(request)
                if (response.ok)
                    paramItemData = AddNoColumn(await response.json());
                
                    console.log(paramItemData);
                // 接到request data後資料更新參數
                setParamItemData(paramItemData.Data)
                setTotalCount(paramItemData.TotalCount)
            }
            catch (e) {
                console.log(e);
            }
        }
        const GridDDL =(props)=>{
            const { dataItem, field } = props;
            const dataValue = dataItem[field] === null ? '' : dataItem[field];
            
            return(
                dataItem.inEdit ?
                <td>
                <DropDownListWithValue
                    value={paramItemData.find(c => c["SET_ITEM_NAME"] === dataValue).SET_ITEM_NAME}
                    data={paramItemData}
                    textField="SET_ITEM_NAME"
                    dataItemKey="SET_ITEM_NAME"
                    onChange={dropchange}
                    
                    dataItem= {props.dataItem}
                    field= {props.field} />
             </td>
             :<td>{dataValue}</td>
            );
    
        }

    return (
        <Grid
        data={paramItemData}
        skip={skip}
        take={take}
        total={totalCount}
        pageable={pageable}
        onPageChange={pageChange}
        className="expand"
        editField="inEdit"
        onRowClick={RowClick}
        onItemChange={itemChange}
        sortable={true}
        sort={sortSetting}
        onSortChange={onSortChange}
      //  rowRender={rowRender}


    >
        <GridColumn width="30px" field="NO" title="No" editable={false} sortable={false}  />
        <GridColumn field="SET_ITEM" title="參數ID" editable={false} />
        <GridColumn field="SET_ITEM_NAME" title="參數名稱" cell={GridDDL}  />
        <GridColumn field="MEMO" title="備註" sortable={false}/>
    </Grid>
    );
}
export default React.memo(ExportWordGrid)
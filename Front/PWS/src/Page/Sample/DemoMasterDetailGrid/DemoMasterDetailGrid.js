import React,{useState,useEffect,useReducer} from 'react';
import { Grid, GridColumn as Column, GridDetailRow } from '@progress/kendo-react-grid';
import { insertItem, getItems, updateItem, deleteItem } from "./services.js";
import { MyCommandCell } from "./MyCommandCell";
import { MyCustomCell } from "./MyCustomCell";
import {DetailComponent} from './DetailComponent';



 const DemoMasterDetailGrid =()=> {
    const [, forceUpdate] = useReducer(x => x + 1, 0);
    const expandChange = (event) => {
        // console.log(event);
        event.dataItem.expanded = !event.dataItem.expanded;
        forceUpdate();
        // this.forceUpdate();
    }

    const editField = "inEdit";
    // state = {
    //     data: []
    // };
    const [StateData,setData]=useState([]);
    // componentDidMount() {
    //     this.setState({
    //         data: getItems()
    //     });
    // }
    useEffect(()=>{
        setData(getItems());
    },[])


    const CommandCell = props => (
      <MyCommandCell
        {...props}
        edit={enterEdit}
        remove={remove}
        add={add}
        discard={discard}
        update={update}
        cancel={cancel}
        editField={editField}
        />
    );

    const customCell = props => (
        <MyCustomCell
          {...props}
          />
      );

    // modify the data in the store, db etc
    const remove = dataItem => {
        // const data = deleteItem(dataItem);
        setData(StateData.filter(item=>item.ProductID!==dataItem.ProductID));
    };

    const add = dataItem => {
        dataItem.inEdit = true;

        const data = insertItem(dataItem);
        setData(data);
    };

    const update = dataItem => {
        dataItem.inEdit = false;
        dataItem.IsUptade=true;
        dataItem.expanded=false;
        const data = updateItem(dataItem);
        setData(data);
    };

    // Local state operations
    const discard = dataItem => {
        const data = [...StateData];
        data.splice(0, 1)
        setData(data);
    };

    const cancel = dataItem => {
        // const originalItem = getItems().find(
        //     p => p.ProductID === dataItem.ProductID
        // );
        // const data = StateData.map(item =>
        //     item.ProductID === originalItem.ProductID ? originalItem : item
        // );

        // setData(olddata=>[...olddata,data]);
        
        const originalItem = getItems().find(p => p.ProductID === dataItem.ProductID);
        const data = StateData.map(item => item.ProductID === originalItem.ProductID ? originalItem : item);

        setData( data );
    };

    const enterEdit = dataItem => {
        const data = StateData.map(item =>
            item.ProductID === dataItem.ProductID ? { ...item, inEdit: true,expanded:true } : item
        );
       
        setData(data);
    };

    const itemChange = event => {
        const data = StateData.map(item =>
            item.ProductID === event.dataItem.ProductID
                ? { ...item, [event.field]: event.value }
                : item
        );

        setData(oldData=>[...oldData,data]);
    };

    const addNew = () => {
        const newDataItem = { inEdit: true, Discontinued: false };

        setData(oldData=>[newDataItem, ...StateData]);
    };

  
    const onCellRender=(defaultRendering,props)=>{
        return defaultRendering.props.className==='k-hierarchy-cell' ? <td></td>:defaultRendering;
    }

    const MyCustomDetail = (props) => <DetailComponent {...props} updateItem={update} />

        return (
          <Grid
            data={StateData}
            detail={MyCustomDetail}
            style={{ height: '400px' }}
            expandField="expanded"
            onExpandChange={expandChange}
            editField={editField}
            cellRender={onCellRender}
            >
            <Column cell={customCell} field="ProductID" title="ID" width="80px" editable={false}/>
            <Column field="ProductName" title="Product" width="300px" editable={false}/>
            <Column field="UnitPrice" title="Unit Price" width="100px" editable={false}/>
            <Column field="QuantityPerUnit" title="Qty Per Unit" editable={false}/>
            <Column cell={CommandCell} width="240px" />
          </Grid>
        );
}


export default DemoMasterDetailGrid;
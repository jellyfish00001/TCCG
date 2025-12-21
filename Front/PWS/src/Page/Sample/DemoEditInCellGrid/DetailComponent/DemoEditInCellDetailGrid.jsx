import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Grid, GridColumn as Column, GridToolbar } from '@progress/kendo-react-grid';
import { sampleProducts } from './sample-products';
import { CellRender, RowRender } from '../renderers2';
import { MyCommandCell } from './MyCommandCell';
import { Fade  } from '@progress/kendo-react-animation';
const App = (props) => {
    const [data, setData] = React.useState(sampleProducts);
    const [editField, setEditField] = React.useState(undefined);
    const [changes, setChanges] = React.useState(false);
    const [expanded, setexpanded] = React.useState(false);

    React.useEffect(()=>{
        if(props)
            setexpanded(props.dataItem.expanded);
    },[props.dataItem.expanded]);

    const enterEdit = (dataItem, field) => {
        let newData = [];
        switch (field) {
            case 'ProductID':
                newData = data.map(item => ({
                    ...item,
                    inEdit: undefined
                })
                );
                break;
            case 'expanded':
                newData = data.map(item => ({
                    ...item,
                    expanded: item.ProductID === dataItem.ProductID ? dataItem.expanded : false
                })
                );
                break;
            default:
                newData = data.map(item => ({
                    ...item,
                    inEdit: item.ProductID === dataItem.ProductID ? field : undefined
                })
                );
                break;
        }

        // const newData = data.map((item) => ({
        //     ...item,
        //     inEdit: item.ProductID === dataItem.ProductID ? field : undefined,
        //   }));
        setData(newData);
        setEditField(field);
    };

    const exitEdit = () => {
        const newData = data.map(item => ({
            ...item,
            inEdit: undefined
        }));
        setData(newData);
        setEditField(undefined);
    };
    const addNew = () => {
        const newRecord = {
            "ProductID": data.length + 1,
            "ProductName": "",
            "SupplierID": null,
            "CategoryID": null,
            "QuantityPerUnit": "",
            "UnitPrice": null,
            "UnitsInStock": null,
            "UnitsOnOrder": null,
            "ReorderLevel": null,
            "Discontinued": false,
            "Category": {
                "CategoryID": null,
                "CategoryName": "",
                "Description": ""
            },
            "FirstOrderedOn": null
        };
        setData([newRecord, ...data]);

    };

    const remove = dataItem => {
        let index = data.findIndex(record => record.ProductID === dataItem.ProductID);
        data.splice(index, 1);
        setData(data);

        // setState({ data: [...state.data] });
    };
    const saveChanges = () => {
        sampleProducts.splice(0, sampleProducts.length, ...data);
        setEditField(undefined);
        setChanges(false);
    };

    const cancelChanges = () => {
        setData(sampleProducts);
        setChanges(false);
    };

    const itemChange = event => {
        let field = event.field || '';
        event.dataItem[field] = event.value;
        let newData = data.map(item => {
            if (item.ProductID === event.dataItem.ProductID) {
                item[field] = event.value;
            }

            return item;
        });
        setData(newData);
        setChanges(true);
    };
    const CommandCell = props => (
        <MyCommandCell
            {...props}
            remove={remove}
        />
    );
    const customCellRender = (td, props) => <CellRender originalProps={props} td={td} enterEdit={enterEdit} editField={editField} />;

    const customRowRender = (tr, props) => <RowRender originalProps={props} tr={tr} exitEdit={exitEdit} editField={editField} />;

    return <Fade 
    //動畫長短設定
    transitionExitDuration={500}
    transitionEnterDuration={500}
    enter={true}
>
    {expanded ?
    <Grid style={{
        height: '420px'
    }} data={data} rowHeight={50} onItemChange={itemChange} cellRender={customCellRender} rowRender={customRowRender} editField="inEdit">
        <GridToolbar>
            <button
                title="新增"
                className="k-button k-primary"
                onClick={addNew}
            >
                新增
              </button>
            <button title="儲存變更" className="k-button" onClick={saveChanges} disabled={!changes}>
                儲存變更
          </button>
            <button title="取消變更" className="k-button" onClick={cancelChanges} disabled={!changes}>
                取消變更
          </button>
        </GridToolbar>
        <Column field="ProductID" title="Id" width="50px" editable={false} />
        <Column title="Product Name" width="200px" field="ProductName" />
        <Column title="Units In Stock" editor="numeric" field="UnitsInStock" />
        <Column title="First Ordered" editor="date" format="{0:d}" width="140px" field="FirstOrderedOn" />
        <Column editor="boolean" field="Discontinued" />
        <Column cell={CommandCell} width="200px" />
    </Grid>:null
}
    </Fade>
    
};
export default App;
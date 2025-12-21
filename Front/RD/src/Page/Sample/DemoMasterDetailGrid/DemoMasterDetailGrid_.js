import React from 'react';
import { Grid, GridColumn as Column, GridDetailRow } from '@progress/kendo-react-grid';
import { insertItem, getItems, updateItem, deleteItem } from "./services.js";
import { MyCommandCell } from "./MyCommandCell";
import { MyCustomCell } from "./MyCustomCell";
import {DetailComponent} from './DetailComponent';



export default class DemoMasterDetailGrid extends React.Component {
    expandChange = (event) => {
        event.dataItem.expanded = !event.dataItem.expanded;
        this.forceUpdate();
    }

    editField = "inEdit";
    state = {
        data: []
    };

    componentDidMount() {
        this.setState({
            data: getItems()
        });
    }

    CommandCell = props => (
      <MyCommandCell
        {...props}
        edit={this.enterEdit}
        remove={this.remove}
        add={this.add}
        discard={this.discard}
        update={this.update}
        cancel={this.cancel}
        editField={this.editField}
        />
    );

    customCell = props => (
        <MyCustomCell
          {...props}
          />
      );

    // modify the data in the store, db etc
    remove = dataItem => {
        const data = deleteItem(dataItem);
        this.setState({ data });
    };

    add = dataItem => {
        dataItem.inEdit = true;

        const data = insertItem(dataItem);
        this.setState({
            data: data
        });
    };

    update = dataItem => {
        dataItem.inEdit = false;
        dataItem.IsUptade=true;
        const data = updateItem(dataItem);
        this.setState({ data });
    };

    // Local state operations
    discard = dataItem => {
        const data = [...this.state.data];
        data.splice(0, 1)
        this.setState({ data });
    };

    cancel = dataItem => {
        const originalItem = getItems().find(
            p => p.ProductID === dataItem.ProductID
        );
        const data = this.state.data.map(item =>
            item.ProductID === originalItem.ProductID ? originalItem : item
        );

        this.setState({ data });
    };

    enterEdit = dataItem => {
        this.setState({
            data: this.state.data.map(item =>
                item.ProductID === dataItem.ProductID ? { ...item, inEdit: true } : item
            )
        });
    };

    itemChange = event => {
        const data = this.state.data.map(item =>
            item.ProductID === event.dataItem.ProductID
                ? { ...item, [event.field]: event.value }
                : item
        );

        this.setState({ data });
    };

    addNew = () => {
        const newDataItem = { inEdit: true, Discontinued: false };

        this.setState({
            data: [newDataItem, ...this.state.data]
        });
    };

    MyCustomDetail = (props) => <DetailComponent {...props} updateItem={this.update} />

    render() {
        return (
          <Grid
            data={this.state.data}
            detail={this.MyCustomDetail}
            style={{ height: '400px' }}
            expandField="expanded"
            onExpandChange={this.expandChange}
            editField={this.editField}
            >
            <Column cell={this.customCell} field="ProductID" title="ID" width="80px" editable={false}/>
            <Column field="ProductName" title="Product" width="300px" editable={false}/>
            <Column field="UnitPrice" title="Unit Price" width="100px" editable={false}/>
            <Column field="QuantityPerUnit" title="Qty Per Unit" editable={false}/>
            <Column cell={this.CommandCell} width="240px" />
          </Grid>
        );
    }
}

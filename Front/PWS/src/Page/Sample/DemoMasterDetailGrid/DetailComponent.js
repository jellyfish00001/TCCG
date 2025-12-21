import React from 'react';
import TextInput from '../../../Components/Input/TextInput'

export const DetailComponent =(props)=> {
      let dataItem = props.dataItem;
      let ProductName=props.dataItem["ProductName"].toString();

  
      const editClick=()=>{
        dataItem.ProductName=ProductName;
        props.updateItem(dataItem);
      }
        let Detail=null;
        if(dataItem.CategoryID !==1){
          Detail=<section>
              <p><strong>In Stock:</strong> {dataItem.UnitsInStock} units</p>
              <p><strong>On Order:</strong> {dataItem.UnitsOnOrder} units</p>
              <p><strong>Reorder Level:</strong> {dataItem.ReorderLevel} units</p>
              <p><strong>Discontinued:</strong> {dataItem.Discontinued}</p>
              <p><strong>Category:</strong> {dataItem.Category.CategoryName} - {dataItem.Category.Description}</p>
            </section>
        }else{
          Detail=
          dataItem.inEdit ? 
          <section>
            <TextInput defaultValue={ProductName} onChange={(e)=>ProductName=e.value}></TextInput>
            <button onClick={()=>editClick()} >修改</button>
            </section>
          :
            <section>
            test
            </section>
        }


        return (
          <div>
            {Detail}
          </div>
        );
}
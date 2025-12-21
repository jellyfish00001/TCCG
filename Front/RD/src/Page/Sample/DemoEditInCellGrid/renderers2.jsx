import * as React from 'react';
import TwDatePicker from '../../../Components/DateInputs/TwDatePicker'
export const CellRender = props => {
  const dataItem = props.originalProps.dataItem;
  const cellField = props.originalProps.field;
  // const inEditField = dataItem[props.editField];
  //修正官方範例帶了錯誤的值導致沒正確focus到指定input
  const inEditField = props.editField;
  const additionalProps = cellField && cellField === inEditField ? {
    ref: td => {
      const input = td && td.querySelector('input');
      //另外撈出日期選項的按鈕
      const calendarBtn = td && td.querySelector('a.k-select');
      const activeElement = document.activeElement;
      if (!input || !activeElement || input === activeElement || !activeElement.contains(input)) {
        return;
      }
      //因focus到input後按日期選擇按鈕會觸發onBlur，故直接觸發按鈕
      if(calendarBtn){
        // props.td.innerHtml=<TwDatePicker></TwDatePicker>;
        console.log(props.td.props.children);
        calendarBtn.click();
        return;
      }
      if (input.type === 'checkbox') {
        input.focus();
      } 
      else {
        input.select();
      }
      
    }
  } : {
    onClick: () => {
      props.enterEdit(dataItem, cellField);
    }
  };
  return React.cloneElement(props.td, { ...props.originalProps,
    ...additionalProps
  }, props.td.props.children);
};
export const RowRender = props => {
  const trProps = { ...props.tr.props,
    onBlur: () => {
      console.log('onBlur');
      props.exitEdit();
    }
  };
  return React.cloneElement(props.tr, { ...trProps
  }, props.tr.props.children);
};
import * as React from 'react';
import TwDatePicker from '../DateInputs/TwDatePicker'
import { FormatDate } from "../../Basic/SDOExtension";
import { date } from 'yup';
import { Tooltip } from '@progress/kendo-react-tooltip';
export const TwDatePickerCell = props => {
  const today = new Date();
  const { dataItem, field , AlwaysEdit = false } = props;
  const dataValue = dataItem[field] === null ? new date() : dataItem[field];
  const [datePicker, setDatePicker] = React.useState({
    dataPickerValue: dataValue,
    dataPickerValid: true,
  })
  const [IsEdit, SetIsEdit] = React.useState(AlwaysEdit ? true : false);
  const [isNewData, SetIsNewData] = React.useState(false);


  const Td = React.useRef(null);

  const onInputChange = (e) => {
    //editType不為1則將editType改為2代表是修改資料
    if (dataItem.editType !== 1)
      dataItem.editType = 2;
    dataItem[field] = e.value;
    //傳回外層的事件
    if (props.onCellInputChange) {
      props.onCellInputChange(dataItem)
    }
    setDatePicker({
      dataPickerValue: e.value,
      dataPickerValid: true,
    })
  }

  React.useEffect(() => {
    let isMounted = true;
    if (Td.current != null && IsEdit) {
      let button = Td.current.querySelector('a');
      if (button && !isNewData)
        button.click();
    }
    //如果Grid有新插入資料
    if (props.newDatas && dataItem.editType === 1 && isMounted) {
      SetIsNewData(true);
    }

    return () => { isMounted = false };
  }, [IsEdit, isNewData])

  return <td onClick={() => SetIsEdit(true)} onBlur={() => { if(!AlwaysEdit) SetIsEdit(false) }} ref={Td}>
    {
      IsEdit || isNewData ?
        <TwDatePicker
        {...props}
          value={typeof datePicker.dataPickerValue === "string" ? new Date(datePicker.dataPickerValue) : datePicker.dataPickerValue}
          onChange={(e) => { onInputChange(e);if(!AlwaysEdit) SetIsEdit(false); }}
          format={props.format ? props.format : "yyy/MM/dd"}
          width={!props.width?'100%':props.width}
        />
        :
        <Tooltip openDelay={10} position="bottom" anchorElement="target">
          <div style={props.labelStyle} title={FormatDate(dataValue.toString(), props.displayFormat ? props.displayFormat : "tYY年MM月DD日")}>
            {FormatDate(dataValue.toString(), props.displayFormat ? props.displayFormat : "tYY年MM月DD日")}</div>
        </Tooltip>
    }
  </td>;
};
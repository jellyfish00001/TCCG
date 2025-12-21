import * as React from 'react';
import TwDatePicker from '../../../Components/DateInputs/TwDatePicker'

export const TwDatePickerCell = props => {
  const today = new Date();
  const { dataItem, field } = props;
  const dataValue = dataItem[field] === null ? "" : dataItem[field];
  const [datePicker, setDatePicker] = React.useState({
    dataPickerValue: dataValue,
    dataPickerValid: true,
})

  return <td>
      <TwDatePicker
      value={datePicker.dataPickerValue}
      onChange={event => setDatePicker({ ...datePicker, dataPickerValue: event.value })}
      format={"yyy/MM"}
      // max={new Date()}
      min={new Date(today.getFullYear(), today.getMonth(), 1)}//最早到當月1號 ps:每月log另存table
       //日期欄位是否驗證
       valid={true}
       //日期欄位是否正確
       dateValidate={(valid) => {
           setDatePicker({ ...datePicker, dataPickerValid: valid })
       }}
    />
  </td>;
};
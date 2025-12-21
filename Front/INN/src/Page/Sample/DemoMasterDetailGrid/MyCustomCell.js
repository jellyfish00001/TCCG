import * as React from "react";

export const MyCustomCell = props => {

  const field = props.dataItem[props.field];

  const IsUptade = props.dataItem["IsUptade"];
  let CellStyle={
    color:IsUptade ? "red":""
  }
  return (
    <td style={CellStyle}> {
            field.toString()+ (IsUptade ? "已更新":"")
          }
    </td>
  );
};
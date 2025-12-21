import * as React from "react";
import { Tooltip } from '@progress/kendo-react-tooltip';

export const CommandCell = props => {
  return (
    <td className="k-command-cell" style={{ textAlign: 'center' }}>
      <Tooltip openDelay={10} position="bottom" anchorElement="target">
        {props.children}
      </Tooltip>
    </td>
  );
};

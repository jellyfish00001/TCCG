//*****提示訊息工具*****
import React from 'react';
import { Tooltip } from '@progress/kendo-react-tooltip';

const CommonTooltip = (props) => {

    const { title, content, position, withoutRedStar } = props;

    return (
        <Tooltip
            content={(props) => <span style={{ fontSize: '15px' }}>{content}</span>}
            openDelay={10}
            position={position ? position : "right"}
            anchorElement="target">
            {/* 預設加星號 */}
            <div className={withoutRedStar ? "" : "addRedStar"}>
                {props.title}
                <span title={title} style={{ verticalAlign: 'middle' }} className="fa-exclamation-circle_RoyalBlue">
                </span>
            </div>
        </Tooltip>
    );
}

export default React.memo(CommonTooltip);
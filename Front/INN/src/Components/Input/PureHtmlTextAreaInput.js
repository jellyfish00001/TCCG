


import React from 'react';
import { Error } from '@progress/kendo-react-labels';

const PureHtmlTextAreaInput = (props) => {
    const { value } = props;
    return (
        <>
            <span className="k-textarea" style={props.style}>
                <textarea
                    {...props}
                    value={value === null ? '' : value}//若為null值則給空字串
                    className="k-input"
                />
            </span>
            {<Error>{props.error}</Error>}
        </>
    )
}

export default React.memo(PureHtmlTextAreaInput)
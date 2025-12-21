


import React from 'react';
import { Input } from '@progress/kendo-react-inputs';
import { Error } from '@progress/kendo-react-labels';

const TextInput = (props) => {
    return (
        <>
            <Input
                {...props}
            >
            </Input>
            {<Error>{props.error}</Error>}
        </>
    )
}

export default React.memo(TextInput)
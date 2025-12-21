import React from 'react';
import { Error } from '@progress/kendo-react-labels';
import { DropDownList } from '@progress/kendo-react-dropdowns';


export const DropDownListWithValue = (props) => {

    const isPresent = value => value !== null && value !== undefined;
    const itemFromValue = (value) => {
        const { data = [], dataItemKey } = props;
        let selectedData = data.find(item => item[dataItemKey] === value);
        return isPresent(selectedData) ?
            selectedData : value;
    }

    const events = {
        onBlur: event => triggerEvent('onBlur', event),
        onFocus: event => triggerEvent('onFocus', event),
        onChange: event => triggerEvent('onChange', event),
        onPageChange: event => triggerEvent('onPageChange', event),
        onFilterChange: event => triggerEvent('onFilterChange', event)
    };

    const triggerEvent = (eventType, event) => {
        if (props[eventType]) {
            props[eventType].call(undefined, {
                ...event,
                target: getValue(event)
            })
        }
    }

    const getValue = (event) => {
        let value = event.target.value
        return {
            ...event.target,
            value: isPresent(value) ? value[props.dataItemKey] : value,
            text: isPresent(value) ? value[props.textField] : value
        };
    }

    return (
        <div>
            <DropDownList
                popupSettings={
                    { className: "dropdown-text-size" }
                }
                {...props}
                defaultValue={itemFromValue(props.value)}
                value={itemFromValue(props.value)}
                {...events}
            />
            {<Error>{props.error}</Error>}
        </div>
    )
}
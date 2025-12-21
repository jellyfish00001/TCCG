import { DropDownList } from '@progress/kendo-react-dropdowns';
import React, { Component } from 'react';
import { IsNullOrEmpty } from '../Basic/SDOExtension';
import { Error } from '@progress/kendo-react-labels';

const isPresent = value => value !== null && value !== undefined;

class DropDownListWithValueField extends Component {
    constructor(props) {
        super(props)
        this.state = {
            value: IsNullOrEmpty(props.value) ? '' : props.value,
            errorMessageVisible: false,
            errorMessage: ''
        }
        this.LoadSetting(props);
    }

    component;
    events = {
        onBlur: event => this.triggerEvent('onBlur', event),
        onFocus: event => this.triggerEvent('onFocus', event),
        onChange: event => this.triggerEvent('onChange', event),
        onPageChange: event => this.triggerEvent('onPageChange', event),
        onFilterChange: event => this.triggerEvent('onFilterChange', event)
    };

    get value() {
        if (this.component) {
            const value = this.component.value;
            return isPresent(value) ? value[this.ddlProps.dataItemKey] : value;
        }
        return "";
    }

    LoadSetting = props => {
        this.ddlProps = { ...props };
        this.ddlProps.value = IsNullOrEmpty(this.ddlProps.value) ? '' : this.ddlProps.value;

        this.require = props.hasOwnProperty('require') && props.require;
        if (props.hasOwnProperty('require')) {
            delete this.ddlProps.require
        }

        this.fieldName = '請設定fileName'
        if (props.hasOwnProperty('fieldName')) {
            this.fieldName = props.fieldName;
            delete this.ddlProps.fieldName;
        }
    }

    //當props或state異動時觸發
    shouldComponentUpdate(props, state) {
        this.LoadSetting(props);

        //state value 與 props value 同步
        this.state.value = IsNullOrEmpty(props.value) ? '' : props.value;

        //return true將執行render 否則不執行
        return true;
    }

    render() {
        return (
            <div>
                <DropDownList
                    {...this.ddlProps}
                    value={this.itemFromValue(this.ddlProps.value)}
                    defaultValue={this.itemFromValue(this.ddlProps.defaultValue)}
                    ref={component => this.component = component}
                    {...this.events}
                />
                {<Error>{this.props.error}</Error>}
                {/* {this.state.errorMessageVisible && <Error>{this.state.errorMessage}</Error>} */}
            </div>
        );
    }

    triggerEvent(eventType, event) {
        if (this.ddlProps[eventType]) {
            this.ddlProps[eventType].call(undefined, {
                ...event,
                target: this
            });
        }
    }

    itemFromValue(value) {
        const { data = [], dataItemKey } = this.props;
        return isPresent(value) ?
            data.find(item => item[dataItemKey] === value) : value;
    }

    validate = () => {
        let result = true;
        let state = {
            errorMessageVisible: false,
            errorMessage: ''
        }
        if (this.require && IsNullOrEmpty(this.state.value)) {
            result = false;
            state.errorMessageVisible = !result
            state.errorMessage = this.fieldName + "為必填"

        }
        this.setState(state)

        return result;
    }
};


export default DropDownListWithValueField;
import React, { useState, useEffect } from 'react';
//import { NumericTextBox } from '@progress/kendo-react-inputs';
import { NumericTextBox } from '../ModifedKendoComponents/numerictextbox/NumericTextBox';
import { Error } from '@progress/kendo-react-labels';
import { withFormik } from 'formik';

/**
 * 
 * @param {object} props 
 * @returns 
 * @example
 * <caption>使用方式1:</caption>
 *      <NumericTextInput 
 *       value={filterField.current.DAY_COUNT}
 *       IsForceMax={true}
 *       IsForceMin={true}
 *       max={100}
 *       min={30}
 *       inputType={'text'}
 *       onChange={(e)=>console.log(e.target.value)}
 *      />
 */
const NumericTextInput = (props) => {
    const { value, WithFormik } = props;

    /**
     * 利用state來存取value，藉此觸發元件渲染以達到動態換值的效果
     */
    const [_Value, setValue] = useState(value);

    /**
     * 供目標元件動態呼叫props的各事件
     * 如果是kendo元件，請檢查其介面是否有實作該事件
     */
    const events = {
        onBlur: event => triggerEvent('onBlur', event),
        onFocus: event => triggerEvent('onFocus', event),
        onChange: event => triggerEvent('onChange', event),
    };
    /**
     * 
     * @param {string} eventType 
     * @param {object} event 
     */
    const triggerEvent = (eventType, event) => {
        // console.log(`on ${eventType} trigger:${_Value}`);
        let targetEle = getEventTarget(event);
        if (props[eventType]) {
            // console.log(eventType)
            props[eventType].call(undefined, {
                ...event,
                target: targetEle
            })
        }
        let value = targetEle.value;
        setValue(value);
    }
    /**
     * 利用event取出目標元件的dom物件，重新包裝後回傳
     * 因kendo元件包裝後value只有getter，故包裝時將value物件更換
     * @param {object} event 傳入元件的event以取出event.target
     * @returns 
     */
    const getEventTarget = (event) => {
        //將原值取出
        let value = event.target.value
        //檢查是否強制蓋過原值
        value = CheckForceValueRange(value);
        //重新包裝後回傳
        return {
            ...event.target,
            value: value//更換value物件
        };
    }


    /**
     * 檢核是否依據限制的最大值/最小值強制蓋過現值
     * @param {number} v 
     * @returns 透過IsForceMax/IsForceMin決定是否蓋過原值，之後傳回
     */
    const CheckForceValueRange = (v) => {
        //強制最大值
        if (props.IsForceMax && props.max) {
            if (props.max < v)
                v = props.max;
        }
        //強制最小值
        if (props.IsForceMin && props.min) {
            if (props.min > v)
                v = props.min;
        }
        return v;
    }

    useEffect(() => {
        setValue(value)
    }, [value])


    return (
        <>
            <NumericTextBox
                {...props}
                defaultValue={WithFormik ? value : _Value}
                value={WithFormik ? value : _Value}
                {...WithFormik ? null : events}
                //是否呈現向上和向下旋轉按鈕，預設打開
                spinners={!props.spinners ? props.spinners : true}
            />
            {<Error>{props.error}</Error>}
        </>
    )
}

export default NumericTextInput
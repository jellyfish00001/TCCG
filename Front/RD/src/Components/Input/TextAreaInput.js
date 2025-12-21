import React from 'react';
import { TextArea } from '@progress/kendo-react-inputs';
import * as Yup from 'yup';
import { Error } from '@progress/kendo-react-labels';

const TextAreaInput = (props) => {

    const [value, setValue] = React.useState(props.value);

    const [valid, SetValid] = React.useState({
        IsValid: true,
        message: ''
    });

    const onInputChange = (e) => {
        setValue(e.value);

        //傳回外層的事件
        if (props.onTextAreaChange && valid.IsValid) {
            props.onTextAreaChange(e.value)
        }
    }

    //準備欄位驗證工具
    const validateHelper = Yup.object().shape({
        value: Yup.string().required('此欄位為必填')
            .max(props.max ? props.max : 2000, '此欄位最多' + (props.max ? props.max : 2000) + '字'),
    });

    React.useEffect(() => {
        //驗證
        validateHelper.validate({
            value: value
        }).then(function () {
            SetValid({ IsValid: true, message: "" });
        }).catch(function (err) {
            if (props.required)
                SetValid({ IsValid: false, message: err.message });
        });
    }, [value])

    React.useEffect(() => {
        setValue(props.defaultValue);
    }, [props.defaultValue])

    return (
        <>
            <TextArea
                {...props}
                value={value}
                onChange={(e) => onInputChange(e)}
            >
            </TextArea>
            {props.error ? <Error>{props.error}</Error> :
                <Error>{valid.IsValid ? '' : valid.message}</Error>}
        </>
    )
}
export default React.memo(TextAreaInput)
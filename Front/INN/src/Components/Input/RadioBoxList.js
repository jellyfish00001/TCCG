import * as React from 'react';
import { RadioButton } from '@progress/kendo-react-inputs';
import { Error } from '@progress/kendo-react-labels';
import { IsNullOrEmpty } from '../../Basic/SDOExtension';
import '../../Css/RadioBoxList.css';

///共用CheckBoxList
///Param:
/// -group(string):群組ID
/// -groupName(string)?:群組名稱 ps:不為空值則顯示Title
/// -data(array<object>):設定資料。 ps:checked欄位可預設是否勾選、disabled欄位可設定停用及啟用
/// -onChange(function):onChange事件
/// -valueField(string):value欄位名稱
/// -textField(string):text欄位名稱
const RadioBoxList = props => {
    let title = null;
    if (!IsNullOrEmpty(props.groupName)) {
        title =
            <li key={props.group + '_title'}>
                <label style={{
                    borderBottom: '1px solid darkgray',
                    minWidth: '240px',
                    margin: '2px 0 5px 0'
                }}>
                    {props.groupName}
                </label>
            </li>
    }

    return (
        <>
            <ul className="checkboxList" style={props.style == null ? {} : props.style}>
                {title}
                {props.data.map(data =>
                    <li key={props.group + '_' + data[props.textField]} style={{ display: 'inline', whiteSpace: 'nowrap' }}>
                        <span style={{ whiteSpace: 'nowrap' }}>
                            <RadioButton
                                name={props.group}
                                value={data[props.valueField]}
                                defaultChecked={data.checked}
                                disabled={data.disabled}
                                onChange={props.onChange}
                                checked={data.checked}
                            />
                            &nbsp;{data[props.textField]}
                        </span>
                    </li>
                )}
            </ul>
            <Error>{props.error}</Error>
        </>
    )
}

export default React.memo(RadioBoxList)

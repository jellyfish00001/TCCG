import * as React from 'react';
import { TextArea } from "@progress/kendo-react-inputs";
import { Error } from '@progress/kendo-react-labels';
import * as Yup from 'yup';
import { Tooltip } from '@progress/kendo-react-tooltip';



export const TextAreaCell = props => {

    const editable = React.useRef(props.editable ? props.editable : false);
    const { dataItem, field, IsAlwaysEdit } = props;
    const dataValue = dataItem[field] === null ? "" : dataItem[field];
    const [isEdit, SetIsEdit] = React.useState(IsAlwaysEdit ? IsAlwaysEdit : false);
    const [isNewData, SetIsNewData] = React.useState(false);
    const [Valid, SetValid] = React.useState({
        IsValid: true,
        message: ''
    });


    const TdEle = React.useRef(null);

    /**供input使用的blur事件 */
    const onInputBlur = (e) => {
        dataItem[field] = e.target.value;
        if (props.onCellInputBlur) {
            //驗證沒過不回傳
            props.onCellInputBlur(dataItem)
        }
        //IsAlwaysEdit=true 永遠開啟編輯模式
        //先SetIsEdit false，再重新打開，藉此觸發rerender
        if (IsAlwaysEdit)
            SetIsEdit(false);
        SetIsEdit(IsAlwaysEdit ? IsAlwaysEdit : false);
    }


    //準備欄位驗證工具
    const validateHelper = Yup.object().shape({
        value: Yup.string().required('此欄位為必填')
    });



    const onInputChange = (e) => {
        //editType不為1則將editType改為2代表是修改資料
        if (dataItem.editType !== 1)
            dataItem.editType = 2;


        dataItem[field] = e.value;
        //傳回外層的事件
        if (props.onCellInputChange && Valid.IsValid) {
            props.onCellInputChange(dataItem)
        }
    }

    const setInput = (td) => {
        let textarea = td.querySelector('textarea');
        if (textarea) {
            if (!isNewData && !IsAlwaysEdit)
                textarea.focus();
            //插入blur事件
            textarea.addEventListener(
                'blur',
                onInputBlur,
                false
            );
            if (props.maxlength)
                textarea.setAttribute('maxlength', props.maxlength);

        }
    }


    React.useEffect(() => {
        let isMounted = true;

        SetValid({ IsValid: true, message: '' });

        if ((isEdit && isNewData && isMounted) || (TdEle.current !== null && !isNewData && isMounted)) {
            setInput(TdEle.current);
        }
        //如果Grid有新插入資料
        if (props.newDatas && dataItem.editType === 1 && isMounted) {
            SetIsNewData(true);
        }

        //驗證
        validateHelper.validate({
            value: dataValue
        }).catch(function (err) {
            if (isMounted && props.required)
                SetValid({ IsValid: false, message: err.message });
        });

        return () => { isMounted = false };
    }, [isEdit, isNewData])




    const returnJsx = () => {
        if (editable.current)
            return (<td onClick={() => SetIsEdit(true)} ref={TdEle}  >
                {
                    isEdit || isNewData ?
                        <>
                            <TextArea
                                {...props}
                                defaultValue={dataValue}
                                onChange={(e) => onInputChange(e)}
                                style ={{width:props.width?props.width :'100%'}}
                                error={Valid.IsValid ? '' : Valid.message}
                            />
                            <Error>{Valid.IsValid ? '' : Valid.message}</Error>
                        </>
                        :
                        <>

                            <div title={dataValue.toString()} 
                            style={props.labelStyle ? props.labelStyle : {  whiteSpace: "pre-line"  }}
                            >
                                {dataValue.toString()}
                            </div>
                            <Error>{Valid.IsValid ? '' : Valid.message}</Error>
                        </>
                }

            </td>);
        else
            return (
                <td> 
                    <Tooltip openDelay={10} position="bottom" anchorElement="target">
                    <div 
                    style={props.labelStyle}
                    title={dataValue.toString()}>{dataValue.toString()}</div>
                </Tooltip>
                </td>
            );
    }
    return (returnJsx());
};
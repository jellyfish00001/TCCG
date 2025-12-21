import * as React from 'react';
import TextInput from '../Input/TextInput';
import { Error } from '@progress/kendo-react-labels';
import * as Yup from 'yup';
import { Tooltip } from '@progress/kendo-react-tooltip';



export const TextInputCell = props => {
  const editable = React.useRef(props.editable ? props.editable : false);
  //AlwaysEdit 永遠開啟編輯
  const { dataItem, field, customValidate, AlwaysEdit = false } = props;
  const dataValue = dataItem[field] === null || dataItem[field] === undefined ? "" : dataItem[field];
  const [isEdit, SetIsEdit] = React.useState(AlwaysEdit ? true : false);
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

    if (!AlwaysEdit) {
      SetIsEdit(false);
    }
  }

  //準備欄位驗證工具
  const validateHelper = customValidate ? customValidate.scheme : Yup.object().shape({
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

  //用此參考追蹤是否已設定input，藉此避免新增的input重複綁定事件
  const trackSetInput = React.useRef(0);

  const setInput = (td) => {
    let input = td.querySelector('input');

    if (!AlwaysEdit) {
      if (input) {
        input.style.width = props.inputWidth ? props.inputWidth : input.style.width;
        if (!isNewData)
          input.focus();
        else
          trackSetInput.current += 1;

        //插入blur事件
        if (trackSetInput.current < 2)
          input.addEventListener(
            'blur',
            onInputBlur,
            false
          );
        if (props.maxlength)
          input.setAttribute('maxlength', props.maxlength);

      }
    }
  }


  React.useEffect(() => {
    let isMounted = true;

    SetValid({ IsValid: true, message: '' });
    //
    if ((isEdit && isMounted) || (TdEle.current !== undefined && isMounted)) {
      setInput(TdEle.current);
    }
    //如果Grid有新插入資料
    if (props.newDatas && dataItem.editType === 1 && isMounted) {
      SetIsNewData(true);
    }

    //如果有從外部傳scheme進來，利用外部scheme驗證單一field
    if (customValidate) {
      if (customValidate.triggerValidate)
        validateHelper.validateAt(field, dataItem)
          .then(() => {
            if (isMounted)
              SetValid({ IsValid: true, message: '' });
          })
          .catch((err) => {
            if (isMounted)
              SetValid({ IsValid: false, message: err.message });
          });
    } else {
      validateHelper.validate({
        value: dataValue
      })
        .then(() => {
          SetValid({ IsValid: true, message: '' });
        })
        .catch((err) => {
          if (isMounted && props.required)
            SetValid({ IsValid: false, message: err.message });
        });
    }


    return () => { isMounted = false };
  }, [isEdit, isNewData])


  const returnJsx = () => {
    if (editable.current)
      return (<td onClick={() => AlwaysEdit ? () => { } : SetIsEdit(true)} ref={TdEle}  >
        {
          isEdit || isNewData ?
            <TextInput
              {...props}
              defaultValue={dataValue}
              onChange={(e) => onInputChange(e)}
              onBlur={(e) => onInputBlur(e)}
              style={{ width: props.width ? props.width : '100%' }}
              error={Valid.IsValid ? '' : Valid.message}
            />
            :
            <>
              <Tooltip openDelay={10} position="bottom" anchorElement="target">
                <div style={props.labelStyle} title={dataValue.toString()}>{dataValue.toString()}</div>
              </Tooltip>
              <Error>{Valid.IsValid ? '' : Valid.message}</Error>
            </>
        }

      </td>);
    else
      return (
        <td> <Tooltip openDelay={10} position="bottom" anchorElement="target">
          <div style={props.labelStyle} title={dataValue.toString()}>{dataValue.toString()}</div>
        </Tooltip></td>
      );
  }
  return (returnJsx());
};
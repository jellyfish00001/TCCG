import * as React from 'react';
import NumericTextInput from '../Input/NumericTextInput';
import { formatNumber } from '@telerik/kendo-intl';
import { Error } from '@progress/kendo-react-labels';
import * as Yup from 'yup';
import { Tooltip } from '@progress/kendo-react-tooltip';

export const NumericTextInputCell = props => {
  const editable = React.useRef(props.editable ? props.editable : false);
  //AlwaysEdit 永遠開啟編輯
  const { dataItem, field, customValidate, AlwaysEdit = false, allowNull = false} = props;
  
  const dataValue = dataItem[field] === null 
    ? allowNull ? null : 0 // 可允許空值
    : parseFloat(dataItem[field]);
  const [isEdit, SetIsEdit] = React.useState(AlwaysEdit ? true : false);
  const [isNewData, SetIsNewData] = React.useState(false);
  const [Valid, SetValid] = React.useState({
    IsValid: true,
    message: ''
  });

  const TdEle = React.useRef(null);

  /**供input使用的blur事件 */
  const onInputBlur = (e) => {
    //空值塞0
    if (typeof e.target.value !== 'number' && !allowNull)
      e.target.value = 0;

    dataItem[field] = e.target.value;
    if (props.onCellInputBlur) {
      props.onCellInputBlur(dataItem)
    }
    // if (!AlwaysEdit)
    //   SetIsEdit(false);
  }

  const onInputChange = (e) => {
    //editType不為1則將editType改為2代表是修改資料
    if (dataItem.editType !== 1)
      dataItem.editType = 2;
    dataItem[field] = e.value;
    //傳回外層的事件
    if (props.onCellInputChange) {
      props.onCellInputChange(dataItem)
    }
  }

  //準備欄位驗證工具
  const validateHelper = customValidate ? customValidate.scheme : Yup.object().shape({
    value: Yup.number().required('此欄位為必填')
  });

  React.useEffect(() => {
    SetValid({ IsValid: true, message: '' });
    let input;
    if (TdEle.current != null) {
      input = TdEle.current.querySelector('input');
    }
    if (!AlwaysEdit) {
      if (input) {
        if (!isNewData)
          input.focus();
          //加入maxlength屬性
          if (props.maxlength)
          input.setAttribute('maxlength', props.maxlength);
      }
      //如果Grid有新插入資料
      if (props.newDatas && dataItem.editType === 1) {
        SetIsNewData(true);
      }
    } else {
      if (input) {

      }
    }

    //如果有從外部傳scheme進來，利用外部scheme驗證單一field
    if (customValidate) {
      if (customValidate.triggerValidate)
        validateHelper.validateAt(field, dataItem)
          .then(() => {
              SetValid({ IsValid: true, message: '' });
          })
          .catch((err) => {
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
          if (props.required)
            SetValid({ IsValid: false, message: err.message });
        });
    }

  }, [isEdit])


  //取得數字格式
  const getNumberFormat = () => {
    if (props.Numberformat) {
      return props.Numberformat;
    }
    else {
      return 'c0';
    }
  }

  const returnJsx = () => {
    if (editable.current)
      return (<td rowSpan={props.rowSpan} onClick={() => AlwaysEdit ? () => { } : SetIsEdit(true)} ref={TdEle}  >
        {
          isEdit || isNewData || AlwaysEdit?
            <NumericTextInput
              {...props}
              value={dataValue}
              onChange={(e) => onInputChange(e)}
              onBlur={(e) => onInputBlur(e)}
              inputType="text"
              width={!props.width?'100%':props.width}
              error={Valid.IsValid ? '' : Valid.message}
            />
            :
            <>
              <Tooltip openDelay={10} position="bottom" anchorElement="target">
                <div
                  style={props.labelStyle ? props.labelStyle : { textAlign: "right", paddingRight: "25px" }}
                  title={formatNumber(dataValue, getNumberFormat())+(props.ExtendTxt?props.ExtendTxt:'')} >
                  {formatNumber(dataValue, getNumberFormat())+(props.ExtendTxt?props.ExtendTxt:'')}
                </div>
              </Tooltip>
              <Error>{Valid.IsValid ? '' : Valid.message}</Error>
            </>

        }

      </td>);
    else
      return (
        <td rowSpan={props.rowSpan}>
          <Tooltip openDelay={10} position="bottom" anchorElement="target">
            <div
              style={props.labelStyle ? props.labelStyle : { textAlign: "right", paddingRight: "25px" }}
              title={formatNumber(dataValue, getNumberFormat())+(props.ExtendTxt?props.ExtendTxt:'')} >
              {formatNumber(dataValue, getNumberFormat())+(props.ExtendTxt?props.ExtendTxt:'')}
            </div>
          </Tooltip>
        </td>
      );
  }
  return (
    returnJsx()
  );
};
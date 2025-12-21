import * as React from 'react';
import { DropDownListWithValue } from '../../Components/Dropdowns/DropDownListWithValue';
import { Tooltip } from '@progress/kendo-react-tooltip';
import * as Yup from 'yup';
import { Error } from '@progress/kendo-react-labels';

export const DropDownListCell = props => {
  const editable = React.useRef(props.editable ? props.editable : false);
  const { dataItem, field, ddlData, customValidate, AlwaysEdit = false, style } = props;
  //取出值
  const [dataValue, setdataValue] = React.useState(dataItem[field] === null || dataItem[field] === undefined ? "" : dataItem[field].toString());
  //利用值從ddlData取出選定的值
  const selectedDataArr = ddlData.filter(i => i[props.dataItemKey].toString() === dataValue);
  let selectedData = null;
  let _style = {};
  if (!style) {
    _style = { width: !props.width ? '100%' : props.width }
  }
  else {
    _style = Object.assign(_style, style)
    _style.width = !props.width ? '100%' : props.width;
  }
  if (selectedDataArr.length > 0) {
    selectedData = selectedDataArr[0];
  } else {
    //如果無選定值，預設第一個為選定值
    selectedData = ddlData[0];
    if (selectedData)
      dataItem[field] = selectedData[props.dataItemKey];
  }
  const [isEdit, SetIsEdit] = React.useState(AlwaysEdit ? true : false);
  const [isNewData, SetIsNewData] = React.useState(false);

  const TdEle = React.useRef(null);
  const dropDown = React.useRef(null);
  const [Valid, SetValid] = React.useState({
    IsValid: true,
    message: ''
  });

  //準備欄位驗證工具
  const validateHelper = customValidate ? customValidate.scheme : Yup.object().shape({
    value: Yup.string().required('此欄位為必填')
  });

  const onDDLChange = (e) => {
    // editType不為1則將editType改為2代表是修改資料
    if (dataItem.editType !== 1)
      dataItem.editType = 2;
    dataItem[field] = e.target.value;
    setdataValue(e.target.value);
    //傳回外層的事件
    if (props.onCellDDLChange) {
      props.onCellDDLChange(dataItem)
    }
  }
  React.useEffect(() => {
    SetValid({ IsValid: true, message: '' });
    let isMounted = true;
    let span = null;
    if (TdEle.current != null) {
      //找出CELL內的span
      span = TdEle.current.querySelector('span[role="listbox"]');
      //設定下拉寬度
      if (span) {
        span.style.width = props.selectWidth ? props.selectWidth : span.style.width;
      }
    }
    if (TdEle.current != null && isEdit) {
      //找出CELL內的select
      let select = TdEle.current.querySelector('select');
      //找出CELL內的span
      // let span = TdEle.current.querySelector('span[role="listbox"]');
      if (select && !isNewData && !AlwaysEdit) {
        select.click();
        // SelectEle.current=select;
      }
      if (span) {
        //如果span存在，利用span內的屬性取得實際dropdown的id
        let dropDownid = span.getAttribute('aria-owns');
        if (dropDownid) {
          setTimeout(() => {
            //動態綁定事件給options
            let dropdown = document.getElementById(span.getAttribute('aria-owns'));
            if (dropdown) {
              let lis = dropdown.getElementsByTagName('li');
              // @ts-ignore
              lis.forEach(li => {
                li.addEventListener(
                  'click',
                  () => {
                    setTimeout(() => {
                      if (isMounted && !AlwaysEdit)
                        SetIsEdit(false);
                    }, 500);
                  },
                  false
                );
              });
            }
          }, 500);
        }
      }
    }
    //如果Grid有新插入資料
    if (props.newDatas && dataItem.editType === 1) {
      if (!isNewData)
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
    let displayText = '';
    if (selectedData) {
      displayText = selectedData[props.textField]
    }

    if (editable.current)
      return (<td onClick={() => SetIsEdit(true)} onBlur={() => {
        if (!AlwaysEdit) {
          SetIsEdit(false);
        }
      }} ref={TdEle}  >
        {
          isEdit || isNewData || AlwaysEdit ?
            <DropDownListWithValue
              data={props.ddlData}
              style={_style}
              textField={props.textField} //表單名稱  
              dataItemKey={props.dataItemKey} //表單代碼
              value={selectedData ? selectedData[props.dataItemKey] : ""}
              onChange={(e) => { onDDLChange(e); }}
              onBlur={() => {
                // if (!AlwaysEdit) {
                //   SetIsEdit(false);
                // }
                SetIsEdit(false);
              }}
              ErrorStyle={props.ErrorStyle}
              error={Valid.IsValid ? '' : Valid.message}
            />
            :
            <>
              <Tooltip openDelay={10} position="bottom" anchorElement="target">
                <div style={props.labelStyle} title={displayText}>{displayText}</div>
              </Tooltip>
              <Error style={props.ErrorStyle}>{Valid.IsValid ? '' : Valid.message}</Error>
            </>

        }

      </td>);
    else
      return (
        <>
          <td>
            <Tooltip openDelay={10} position="bottom" anchorElement="target">
              <div style={props.labelStyle} title={displayText}>{displayText}</div>
            </Tooltip>
            <Error>{Valid.IsValid ? '' : Valid.message}</Error>
          </td>
        </>
      )
  }


  return (
    returnJsx()
  );
};

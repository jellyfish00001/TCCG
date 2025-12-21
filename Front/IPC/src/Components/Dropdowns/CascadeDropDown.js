//*****關聯式下拉選單*****

/**
 * @typedef {object} CascadeDropDownProps
 * @property {Array<object>} fristDdlData //第一層下拉選單資料
 * @property {string} firstDdlValue //第一層下拉選單值
 * @property {string} firstDdlError //第一層下拉選單錯誤訊息
 * @property {string} firstColumn //第一層下拉選單欄位名稱
 * @property {object} firstDdlStyle //第一層下拉選單樣式
 * @property {boolean} fristDdlDisabled // 第一層下拉選單是否使用
 * @property {string} firstTextField //第一層下拉選單文字欄位
 * @property {string} firstDataItemKey //第一層下拉選單Key
 * @property {Array<object>} secondDdlInitData //第二層下拉選單初始資料
 * @property {string} secondDdlValue //第二層下拉選單值
 * @property {string} secondDdlError //第二層下拉錯誤訊息
 * @property {string} secondColumn //第二層下拉選單欄位名稱
 * @property {object} secondDdlStyle //第二層下拉選單樣式
 * @property {object} secondItemRender //第二層下拉選單的項目內容
 * @property {object} secondTextField //第二層下拉選單文字欄位
 * @property {object} secondDataItemKey //第二層下拉選單Key
 * @property {object} getSecondDdlData //取得第二層下拉選單資料
 * @property {object} values //Formik values
 * @property {object} setValues //Formik setValues
 */
import React from 'react';
import { IsNullOrEmpty, SetMaskOnOff } from '../../Basic/SDOExtension';
import { DropDownWithFilter } from '../../Components/Dropdowns/DropDownWithFilter';
import { DropDownListWithValue } from './DropDownListWithValue';

/**
 * 
 * @param {CascadeDropDownProps} props 
 * @returns 
 */
export const CascadeDropDown = (props) => {

    // "第一層"下拉選單
    const [fristDdlData, setFristDdlData] = React.useState([]);
    // "第二層"下拉選單
    const [secondDdlData, setSecondDdlData] = React.useState([]);

    React.useEffect(() => {
        setFristDdlData(props.fristDdlData);
    }, [props.fristDdlData])

    React.useEffect(() => {
        firstDdlChange(props.firstDdlValue, true);
    }, [props.firstDdlValue])

    /**
     * 第一層下拉選單 Change Event
     * @param {*} value 
     * @param {*} isInit 
     */
    const firstDdlChange = async (value, isInit) => {
        SetMaskOnOff(true);
        //若不為空，則取得第二個下拉選單資料
        if (!IsNullOrEmpty(value)) {
            let result = await props.getSecondDdlData(value);
            setSecondDdlData(result)
        } else {
            //第二層下拉選單，設定為初始資料
            setSecondDdlData(props.secondDdlInitData)
        }

        if (!isInit) {
            props.setValues({
                ...props.values,
                [props.firstColumn]: value,
                [props.secondColumn]: ""
            });
        }
        SetMaskOnOff(false);
    }

    return (
        <>
            <div style={{ display: "inline-flex" }}>
                <DropDownListWithValue
                    style={props.firstDdlStyle ? props.firstDdlStyle : {}}
                    data={fristDdlData}
                    textField={props.firstTextField ?? "text"}
                    dataItemKey={props.firstDataItemKey ?? "value"}
                    disabled={props.fristDdlDisabled ?? false}
                    value={props.firstDdlValue ?? ""}
                    error={props.firstDdlError ?? ""}
                    onChange={(e) => firstDdlChange(e.target.value, false)}
                />
                <div style={{ marginLeft: "10px" }}>
                    <DropDownWithFilter
                        ddlData={secondDdlData}
                        style={props.secondDdlStyle ? props.secondDdlStyle : {}}
                        value={props.secondDdlValue ?? ""}
                        textField={props.secondTextField ?? "text"}
                        keyField={props.secondDataItemKey ?? "value"}
                        typingLength={1}
                        onChange={(e) => {
                            let name = props.secondColumn.replace("_C", "_NAME");
                            props.setValues({ ...props.values, [props.secondColumn]: e.value, [name]: e.text });
                        }}
                        error={props.secondDdlError ?? ""}
                    />
                </div>
            </div>
        </>
    );
}

export default React.memo(CascadeDropDown)
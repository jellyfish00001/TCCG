import React from 'react';
import { ComboBox } from "@progress/kendo-react-dropdowns";
import { filterBy } from "@progress/kendo-data-query";
import { Error } from '@progress/kendo-react-labels';
import { IsNullOrEmpty } from '../../Basic/SDOExtension';

/**
 * 單選下拉選單合併篩選功能
 * @param {*} props 
 * @returns JSX
 * @example
 *  <DropDownWithFilter
        ddlData={ddlData}
        style={{ width: '100%' }}
        value={values.PROJECT_NO}
        textField={"PROJECT_NAME"}
        keyField={"PROJECT_NO"}
        filterLength={1} // 打滿1個字才做篩選，預設為3個字
        onChange={(e) => {
            setValues({ ...values, PROJECT_NO: e.PROJECT_NO, PROJECT_NAME: e.PROJECT_NAME });
        }}
        error={errors.PROJECT_NO}
    />
 */
export const DropDownWithFilter = (props) => {
    // ComboBox 資料
    const [comboBoxData, setComboBoxData] = React.useState([]);
    const [tempData, setTempData] = React.useState([]);
    // 是否開啟選單
    const [openList, setOpenList] = React.useState(false);
    // 值
    const [value, setValue] = React.useState({});
    const [defaultValue, setDefaultValue] = React.useState({});

    let typeLength = props.typingLength ? props.typingLength : 3;

    //注音取代為空
    const replaceSpecial = (v) => {
        let pattern = /[\u3105-\u3129\u02CA\u02C7\u02CB\u02D9]/g;
        const newStr = v.replace(pattern, "");
        return newStr;
    }

    // 無資料樣式
    const listNoDataRender = (element) => {
        const noData =
            <h4 style={{ fontSize: "16px" }}>
                <span
                    className="k-icon k-i-warning"
                    style={{ fontSize: "2.5em" }}
                />
                <br />
                <br />
                無資料
            </h4>;
        return React.cloneElement(element, { ...element.props }, noData);
    };

    /**
     * 篩選變動事件
     * @param {*} event 
     */
    const filterChange = async (event) => {
        setOpenList(true);
        const value = event.filter.value;
        if (IsNullOrEmpty(value)) {
            setComboBoxData(tempData);
        }
        else if (replaceSpecial(value).length >= typeLength) {
            setComboBoxData(filterBy(tempData.slice(), event.filter));
        }
    };

    /**
     * value變動事件
     */
    const onClose = () => {
        if (openList) {
            // 關閉下拉選單
            setOpenList(false);
        }
    }

    const onChange = (e) => {
        e.value = e.value ?? defaultValue;
        setValue(e.value);
        props.onChange(e.value);
    }

    React.useEffect(() => {
        if (IsNullOrEmpty(props.ddlData)) {
            setComboBoxData([]);
            setTempData([]);
            setValue({});
            return;
        }

        setComboBoxData(props.ddlData);
        setTempData(props.ddlData);

        let obj = props.ddlData.find(x => x[props.keyField] === props.value) ?? {};
        setValue(obj);
        setDefaultValue(obj);
    }, [props.ddlData])

    return (
        <>
            <ComboBox
                {...props}
                data={comboBoxData}
                value={value}
                filterable={true}
                opened={openList}
                onFilterChange={filterChange}
                onFocus={() => setOpenList(true)}
                onOpen={() => setOpenList(true)}
                onClose={onClose} // 選擇選項之後觸發
                onChange={onChange}
                listNoDataRender={listNoDataRender}
                clearButton={false}
            />
            {<Error>{props.error}</Error>}
        </>
    )
}
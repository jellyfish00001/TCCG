import React, { useState, useEffect, useRef, useMemo } from 'react';
import { DropDownTree } from '@progress/kendo-react-dropdowns';
import { filterBy } from '@progress/kendo-react-data-tools';
import { mapTree, extendDataItem } from '@progress/kendo-react-common';

export const processTreeData = (data, state, fields) => {
    const { selectField, expandField, dataItemKey, subItemsField } = fields;
    const { expanded, value, filter } = state;
    const filtering = Boolean(filter && filter.value);

    return mapTree(
        filtering ? filterBy(data, [ filter ], subItemsField) : data,
        subItemsField,
        item => {
            const props = {
                [expandField]: expanded.includes(item[dataItemKey]),
                [selectField]: value && item[dataItemKey] === value[dataItemKey]
            };

            return filtering ?
                extendDataItem(item, subItemsField, props) :
                { ...item, ...props };
        }
    );
};

export const expandedState = (item, dataItemKey, expanded) => {
    const nextExpanded = expanded.slice();
    const itemKey = item[dataItemKey];
    const index = expanded.indexOf(itemKey);
    index === -1 ? nextExpanded.push(itemKey) : nextExpanded.splice(index, 1);

    return nextExpanded;
};

export const DropDownTreeWithValue = (props) => {
    const [ value, setValue ] = useState(null)
    const [ expanded, setExpanded ] = useState([]);

    const filterData = useRef({
        selectField: "selected",
        expandField: "expanded",
        dataItemKey: "id",
        textField: "text",
        subItemsField: "items",
    })

    useEffect(() => {
        let p = { ...props }
        filterData.current.dataItemKey = p.dataItemKey
        filterData.current.textField = p.textField
        filterData.current.subItemsField = p.subItemsField

        onExpandChange({item: p.data})

        // 設定預設值
        setPreset(p.value, p.data)
    }, [props.data, props.value]);

    /**
     * 設定預設值
     * @param {string} value 初始值
     * @param {Array} data 清單
     */
    const setPreset = (value, data) => {
        let isPresent = value !== undefined && value != "";
        if (isPresent) {
            let itemData = getItem(value, data)
            if (itemData !== undefined) {
                onChange({value: itemData})
            }
        }
    }

    /**
     * 取得預設值的項目
     * @param {string} value 初始值
     * @param {Array} data 清單 
     * @returns 預設值的項目(單筆)
     */
    const getItem = (value, data) => {
        let itemData = undefined
        for (let i in data) {
            if (itemData === undefined) {
                if (data[i][filterData.current.dataItemKey] === value) {
                    itemData = data[i]
                }
                else {
                    // 找不到往內找
                    itemData = getItem(value, data[i].items)
                }
            }
        }
        return itemData
    }

    /**
     * 變更 DropDownTree 值
     * @param {*} e 
     */
    const onChange = (e) => {
        setValue(e.value)
        props.setValue(e)
    };

    const onExpandChange = React.useCallback(
        event => setExpanded(expandedState(event.item, filterData.current.dataItemKey, expanded)),
        [expanded]
    );

    const treeData = React.useMemo(
        () => processTreeData(props.data, { expanded, value }, {
            selectField: filterData.current.selectField,
            expandField: filterData.current.expandField,
            dataItemKey: filterData.current.dataItemKey,
            subItemsField: filterData.current.subItemsField
        }),
        [expanded, value]
    );

    return (
        <>
        <DropDownTree
            {...props}
            data={treeData}
            value={value}
            textField={filterData.current.textField}
            dataItemKey={filterData.current.dataItemKey}
            selectField={filterData.current.selectField}
            expandField={filterData.current.expandField}
            onExpandChange={onExpandChange}
            onChange={(e) => onChange(e)}
            />
        </>
    );
}

export default React.memo(DropDownTreeWithValue)
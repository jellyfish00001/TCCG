
/**
 * @typedef {object} DualListBoxProps
 * @property {Array<object>} selectableData 
 * @property {Array<string>} selectedDataValue
 * @property {object} onChange
 */
import React, { useState, useEffect, useRef } from 'react';
import { ListBox, ListBoxToolbar, processListBoxData, processListBoxDragAndDrop } from '@progress/kendo-react-listbox';


const SELECTED_FIELD = 'selected';

/**
 * 
 * @param {DualListBoxProps} props 
 * @returns 
 */
export const DualListBox = (props) => {
    const InitState = {
        selectableData: [],
        selectedData: [],
        selectedDataValue: [],
        draggedItem: {}
    };

    const [state, setState] = useState(InitState);
    let filteredData = useRef([]);


    //初次載入或props.selectableData/props.selectedDataValue改變時
    useEffect(() => {
        //先塞入所有可選項目
        setState(s => ({ ...s, selectableData: props.selectableData }));
        //將所有可選項目塞入ref
        filteredData.current = props.selectableData;
        //依照props.selectedDataValue過濾掉ref內的值
        for (let i = 0; i < props.selectedDataValue.length; i++) {
            const e1 = props.selectedDataValue[i];
            filteredData.current = filteredData.current.filter(fi => fi.value !== e1);
        }
        //最後利用ref更新selectableData的項目
        setState(s => ({ ...s, selectableData: filteredData.current }));
    }, [props.selectableData, props.selectedDataValue])

    //初次載入或props.selectableData改變時
    useEffect(() => {
        //如果props.selectedDataValue有值
        if (props.selectedDataValue.length > 0) {
            for (let i = 0; i < props.selectedDataValue.length; i++) {
                const e1 = props.selectedDataValue[i];
                let hasData = state.selectedData.filter(fi => fi.value === e1);
                //如果不是已選的項目
                if (hasData.length === 0) {
                    //將已選的項目加入selectedData
                    let _selectedData = props.selectableData.filter(fi => fi.value === e1);
                    setState(s => ({ ...s, selectedData: s.selectedData.concat(_selectedData) }));
                }
            }
        }

    }, [props.selectedDataValue])

    //初次載入或state.selectedData改變時
    useEffect(() => {
        //利用外層onChange事件將state.selectedData的值回傳
        handleOnChange();
    }, [state.selectedData])


    const handleItemClick = (event, data, connectedData) => {
        setState({
            ...state,
            [data]: state[data].map(item => {
                if (item.label === event.dataItem.label) {
                    item[SELECTED_FIELD] = !item[SELECTED_FIELD];
                } else if (!event.nativeEvent.ctrlKey) {
                    item[SELECTED_FIELD] = false;
                }
                return item;
            }),
            [connectedData]: state[connectedData].map(item => {
                item[SELECTED_FIELD] = false;
                return item;
            })
        });
    }

    const handleToolBarClick = (e) => {
        console.log(e.toolName);
        let result = processListBoxData(state.selectableData, state.selectedData, e.toolName, SELECTED_FIELD);
        setState(s => ({
            ...s,
            selectableData: result.listBoxOneData,
            selectedData: result.listBoxTwoData
        }));
    }

    const handleDragStart = (e) => {
        setState({
            ...state,
            draggedItem: e.dataItem
        });
    }

    const handleDrop = (e) => {
        let result = processListBoxDragAndDrop(state.selectableData, state.selectedData, state.draggedItem, e.dataItem, 'label');
        setState({
            ...state,
            selectableData: result.listBoxOneData,
            selectedData: result.listBoxTwoData
        });
    }


    const handleOnChange = () => {
        let newArr = state.selectedData.map(a => a.value);
        props.onChange(newArr);
    }

    return (
        <div className='container'>
            <div className='row justify-content-center'>
                <div className='col'>
                    <ListBox
                        style={{ height: 150, width: '100%' }}
                        data={state.selectableData}
                        textField="label"
                        valueField="value"
                        selectedField={SELECTED_FIELD}
                        onItemClick={(e) => handleItemClick(e, 'selectableData', 'selectedData')}
                        onDragStart={handleDragStart}
                        onDrop={handleDrop}
                        toolbar={() => {
                            return (
                                <ListBoxToolbar
                                    tools={['transferTo', 'transferFrom', 'transferAllTo', 'transferAllFrom']}
                                    data={state.selectableData}
                                    dataConnected={state.selectedData}
                                    onToolClick={handleToolBarClick}
                                />
                            );
                        }}
                    />
                </div>
                <div className='col'>
                    <ListBox
                        style={{ height: 150, width: '100%' }}
                        data={state.selectedData}
                        textField="label"
                        valueField="value"
                        selectedField={SELECTED_FIELD}
                        onItemClick={(e) => handleItemClick(e, 'selectedData', 'selectableData')}
                        onDragStart={handleDragStart}
                        onDrop={handleDrop}
                    />
                </div>
            </div>
        </div>
    );
}


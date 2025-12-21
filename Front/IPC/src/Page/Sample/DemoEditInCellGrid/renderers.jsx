import * as React from 'react';

export const Renderers=(enterEdit, exitEdit, editFieldName)=> {
    const _enterEdit=enterEdit;
    const _exitEdit=exitEdit;
    const _editFieldName=editFieldName;

    let _preventExit=false;
    let _preventExitTimeout=setTimeout(() => { _preventExit = undefined; });
    let _blurTimeout = setTimeout(() => {});
    const cellRender = (tdElement, cellProps) => {

        const dataItem = cellProps.dataItem;
        const cellField = cellProps.field;
        const inEditField = dataItem[_editFieldName];
        // cellField==='ProductID' ?? return;
        const additionalProps = cellField && cellField === inEditField ?
            {
                ref: (td) => {
                    const input = td && td.querySelector('input');
                    const activeElement = document.activeElement;

                    if (!input ||
                        !activeElement ||
                        input === activeElement ||
                        !activeElement.contains(input)) {
                        return;
                    }

                    if (input.type === 'checkbox') {
                        input.focus();
                    } else {
                        input.select();
                    }
                }
            } : {
                onClick: () => {
                    console.log('onClick')
                     _enterEdit(dataItem, cellField); 
                    }
            };
        return React.cloneElement(tdElement, { ...tdElement.props, ...additionalProps }, tdElement.props.children);
    }

    const rowRender = (trElement) => {
        const trProps = {
            ...trElement.props,
            onMouseDown: () => {
                console.log('onMouseDown')
                _preventExit = true;
                clearTimeout(_preventExitTimeout);
                _preventExitTimeout = setTimeout(() => { _preventExit = undefined; });
            },
            onBlur: () => {
                console.log('onBlur')
                clearTimeout(_blurTimeout);
                if (!_preventExit) {
                    _blurTimeout = setTimeout(() => { 
                          console.log('_blurTimeout')

                        _exitEdit();
                     });
                }
            },
            onFocus: () => { clearTimeout(_blurTimeout); }
        };
        return React.cloneElement(trElement, { ...trProps }, trElement.props.children);
    }


    return {cellRender:cellRender,rowRender:rowRender}
}
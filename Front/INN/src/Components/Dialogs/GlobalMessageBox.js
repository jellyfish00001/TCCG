/**
 * @typedef {{
 *  visible:any,
 *  message:any,
 *  okText:any,
 *  onOkAction:any
 * }} stateConfig
 */
import React from 'react';
import { Dialog, DialogActionsBar } from '@progress/kendo-react-dialogs';
import { useState, Downgraded } from '@hookstate/core';
import { SetMaskOnOff } from '../../Basic/SDOExtension';
export const messageSettings = {
    visible: false, //是否顯示
    message: '',  //顯示訊息
    okText: '確定', //確認按鈕顯示字樣
    onOkAction: () => { } //確認事件
}

/**
 * GlobalMessageBox元件
 * @component
 * @param {object} props
 * @param {stateConfig} props.stateConfig
 * @description 以globalState做控制的全域訊息框
 */
export const GlobalMessageBox = (props) => {
    /**建立scope */
    const _globalMessageBoxSettings = useState(props.stateConfig);
    _globalMessageBoxSettings.attach(Downgraded);
    if (_globalMessageBoxSettings.visible.get())
        SetMaskOnOff(false);
    return (
        <div>
            {_globalMessageBoxSettings.visible.get() &&
                <Dialog style={{ zIndex: 600 }}>
                    <p style={{ margin: "25px", textAlign: "center", minWidth: '500px', whiteSpace: "break-spaces" }}>{_globalMessageBoxSettings.message.get()}</p>
                    <DialogActionsBar>
                        <button className="k-button" onClick={() => {
                            _globalMessageBoxSettings.onOkAction.get()();
                            _globalMessageBoxSettings.visible.set(false);
                        }}>{messageSettings.okText}</button>
                    </DialogActionsBar>
                </Dialog>
            }
        </div>
    )
}
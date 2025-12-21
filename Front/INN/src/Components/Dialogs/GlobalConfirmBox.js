/**
 * @typedef {{
 *  visible:any,
 *  message:any,
 *  okText:any,
 *  onOkAction:any,
 *  onCancelAction:any
 * }} stateConfig
 */
import React from 'react';
import { Dialog, DialogActionsBar } from '@progress/kendo-react-dialogs';
import { useState, Downgraded } from '@hookstate/core';

export const ConfirmSettings = {
    visible: false, //是否顯示
    message: '', //顯示訊息
    okText: '確定', //確認按鈕字樣
    onOkAction: () => { }, //確認事件
    cancelText: '取消', //取消按鈕字樣
    onCancelAction: () => { }, //取消事件
}


/**
 * GlobalMessageBox元件
 * @component
 * @param {object} props
 * @param {stateConfig} props.stateConfig
 * @description 以globalState做控制的全域訊息框
 */
export const GlobalConfirmBox = (props) => {
    /**建立scope */
    const _globalConfirmSettings = useState(props.stateConfig);
    _globalConfirmSettings.attach(Downgraded);
    return (
        <div>
            {_globalConfirmSettings.visible.get() &&
                <Dialog style={{ zIndex: 500 }}>
                    <p style={{ margin: "25px", textAlign: "center", whiteSpace: "break-spaces" }}>{_globalConfirmSettings.message.get()}</p>
                    <DialogActionsBar>
                        <button className="k-button" onClick={() => {
                            _globalConfirmSettings.onOkAction.get()();
                            _globalConfirmSettings.visible.set(false);
                        }} >{ConfirmSettings.okText}</button>
                        <button className="k-button" onClick={() => {
                            _globalConfirmSettings.onCancelAction.get()();
                            _globalConfirmSettings.visible.set(false);
                        }} >{ConfirmSettings.cancelText}</button>
                    </DialogActionsBar>
                </Dialog>
            }
        </div>
    )
}
import React, { useState, useContext, createContext } from 'react';
import { Dialog, DialogActionsBar } from '@progress/kendo-react-dialogs';

const defaultConfirmSettings = {
    visible: false, //是否顯示
    message: '', //顯示訊息
    okText: '確定', //確認按鈕字樣
    onOkAction: () => { }, //確認事件
    cancelText: '取消', //取消按鈕字樣
    onCancelAction: () => { }, //取消事件
}

export const ConfirmBoxContext = createContext();

export const ConfirmBoxProvider = props => {
    const [confirmSettings, setConfirmSettings] = useState(defaultConfirmSettings);

    const showConfirmBox = (message, onOkAction = () => { }, confirmSettings = {}) => {
        setConfirmSettings({
            ...defaultConfirmSettings,
            visible: true,
            message: message,
            onOkAction: onOkAction,
            ...confirmSettings
        })
    }

    return (
        <ConfirmBoxContext.Provider value={{
            confirmSettings: confirmSettings,
            setConfirmSettings: setConfirmSettings,
            showConfirmBox: showConfirmBox
        }}>
            {props.children}
        </ConfirmBoxContext.Provider>
    )
}

export const ConfirmBox = props => {
    const { confirmSettings, setConfirmSettings } = useContext(ConfirmBoxContext);

    return (
        <div>
            {confirmSettings.visible &&
                <Dialog style={{ zIndex: 500 }}>
                    <p style={{ margin: "25px", textAlign: "center" }}>{confirmSettings.message}</p>
                    <DialogActionsBar>
                        <button className="k-button" onClick={() => {
                            confirmSettings.onOkAction();
                            setConfirmSettings(defaultConfirmSettings);
                        }} >{confirmSettings.okText}</button>
                        <button className="k-button" onClick={() => {
                            confirmSettings.onCancelAction();
                            setConfirmSettings(defaultConfirmSettings);
                        }} >{confirmSettings.cancelText}</button>
                    </DialogActionsBar>
                </Dialog>
            }
        </div>
    )
}
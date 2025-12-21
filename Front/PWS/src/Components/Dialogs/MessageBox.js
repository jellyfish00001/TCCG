import React, { useState, useContext, createContext } from 'react';
import { Dialog, DialogActionsBar } from '@progress/kendo-react-dialogs';

export const MessageBoxContext = createContext();

const defaultMessageSetting = {
    visible: false, //是否顯示
    message: '',  //顯示訊息
    okText: '確定', //確認按鈕顯示字樣
    onOkAction: () => {} //確認事件
}

export const MessageBoxProvider = props => {
    const [messageSettings, setMessageSettings] = useState(MessageBoxContext);

    const showMessage = (message, setting) => {
        setMessageSettings({
            ...defaultMessageSetting,
            visible: true,
            message: message,
            ...setting
        })
    }

    return(
        <MessageBoxContext.Provider value={{messageSettings:messageSettings, setMessageSettings:setMessageSettings, showMessage:showMessage}}>
            {props.children}
        </MessageBoxContext.Provider>
    )
}

export const MessageBox = props => {
    const { messageSettings, setMessageSettings } = useContext(MessageBoxContext);

    return(
        <div>
            {messageSettings.visible &&
                <Dialog style={{ zIndex: 600 }}>
                    <p style={{ margin: "25px", textAlign: "center" ,whiteSpace: "pre-line"}}>{messageSettings.message}</p>
                    <DialogActionsBar>
                        <button className="k-button" onClick={() => {
                            messageSettings.onOkAction();
                            setMessageSettings(defaultMessageSetting);
                        }}>{messageSettings.okText}</button>
                    </DialogActionsBar>
                </Dialog>
            }
        </div>
    )
}
import React, { useState, useEffect } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Checkbox } from '@progress/kendo-react-inputs';
import { Window } from '@progress/kendo-react-dialogs';
import { WindowResizehook } from '../../../../Hook/useWindowResize';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';

const InterfacingSysConfirm = props => {
    let { visible, onClose, saveUseFtyDataOrNot } = props
    const dimensions = WindowResizehook();
    const [isConfirm, setIsConfirm] = useState("");

    const onConfirm = () => {
        if (isConfirm === "1") {
            saveUseFtyDataOrNot(true);
        }
        else {
            onClose();
        }
    }

    useEffect(() => {
        if (visible) {
            setIsConfirm("");
        }
    }, [visible])


    return (
        <div>
            {visible &&
                <Window
                    title='以國發會資料介接為主'
                    onClose={() => { onClose() }}
                    initialWidth={896}
                    initialHeight={548.15}
                    draggable={false}
                    resizable={false}
                    modal={true}
                >
                    <div style={{
                        'display': 'flex',
                        'flex-direction': 'column',
                        'justify-content': 'space-between',
                        'height': '100%'
                    }}>
                        <div style={{ fontSize: '1.3rem' }}>
                            <span >請選擇是否界接工程會標案資料？</span>
                            <Checkbox
                                style={{ marginLeft: '20px' }}
                                name="isConfirm"
                                label='是'
                                checked={isConfirm === "1"}
                                onChange={(e) => {
                                    setIsConfirm(isConfirm === "1" ? "" : "1")
                                }}
                            ></Checkbox>
                            <Checkbox
                                style={{ marginLeft: '20px' }}
                                name="isNotConfirm"
                                label='否'
                                checked={isConfirm === "0"}
                                onChange={(e) => {
                                    setIsConfirm(isConfirm === "0" ? "" : "0")
                                }}
                            ></Checkbox>
                        </div>
                        <br />
                        <div style={{ fontSize: '1.3rem' }}>
                            <span style={{ color: 'red' }}>界接工程會標案資料</span><br />
                            <span>
                                1.　 將以工程會標案管理系統當月資料匯入本系統，無需再至本系統填報辦理情形，惟後續
                                <br />
                                <span style={{ color: 'red' }}>　　竣工及驗收預定完成日期異動時，亦應向本府智發會申請期程調整</span>
                                。
                            </span><br />

                            <span>
                                2.　 請於每月5日前至工程會標案管理系統完成填報，本系統將於每月6日將辦理情形匯<br />　　入，<span style={{ color: 'red' }}>未於每月5日前完成工程會標案管理系統填報，視為逾期填報</span>。
                            </span><br />

                            <span>
                                3.　 竣工及驗收完成後，請即時於工程會標案管理系統填報竣工及驗收完成日期，避免於本<br />　　系統發生進度落後情形。
                            </span><br />

                            <span>
                                4.　 <span style={{ color: 'red' }}>本系統於填報驗收完成日期後，即終止界接工程會標案管理系統</span>，請於次月至本系統提<br />　　出結案申請或繼續填報辦理情形。
                            </span><br />

                            <span>
                                5.　 若欲取消界接，請於填報週期內（每月20日至次月5日前），至本系統操作，並依限完<br />　　成每月辦理情形填報送出。
                            </span><br />

                            <br />
                            <span style={{ color: 'red' }}>不界接工程會標案資料</span><br />
                            <span>
                                1.　 請於填報週期內（每月20日至次月5日前），至本系統操作，並依限完成每月辦理情形<br />　　填報送出。
                            </span>
                        </div>

                        <div style={{
                            display: 'flex',
                            alignItems: 'center',
                            flexDirection: 'column'
                        }}>
                            <div className='fn-buttons' >
                                <Button type='button' disabled={IsNullOrEmpty(isConfirm)} onClick={() => onConfirm()}>確認</Button>
                            </div>
                        </div>
                    </div>
                </Window>
            }
        </div>
    )
}

export default InterfacingSysConfirm;
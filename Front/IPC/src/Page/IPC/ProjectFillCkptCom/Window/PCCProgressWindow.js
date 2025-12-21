import React, { useState, useEffect } from 'react';
import { Window } from '@progress/kendo-react-dialogs';
import { WindowResizehook } from '../../../../Hook/useWindowResize';
import { getPCCExeProgress } from '../ProjectFillCkptComService';
import { IsNullOrEmpty, SetMaskOnOff } from '../../../../Basic/SDOExtension';
import PCCProgressGrid from '../AssociatePCC/PCCProgressGrid';
import { showGlobalMessageBox } from '../../../../Route/RootMiddleware';

// 工程標案執行進度window
const PCCProgressWindow = props => {
    const { visible, onClose, pccProjectUid } = props
    const dimensions = WindowResizehook();
    // 工程標案各月執行進度 & 落後原因資料
    const [progressData, setProgressData] = useState([])
    const [delayData, setDelayData] = useState([])
    // Grid 動態表頭資料
    const [progressDataHeaders, setProgressDataHeaders] = useState({})
    const [delayDataHeaders, setDelayDataHeaders] = useState({})

    // 取得工程會標案基本資料
    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await getPCCExeProgress(pccProjectUid);
        // map 出新物件
        let progressDatas = result.ProgressDatas == null ? [] : result.ProgressDatas.map(x => { return { ...x } });
        let delayDatas = result.DelayDatas == null ? [] : result.DelayDatas.map(x => { return { ...x } });

        setProgressData(progressDatas)
        setDelayData(delayDatas)

        // set Headers state
        setProgressDataHeaders({ ...result.ProgressHeaders });
        setDelayDataHeaders({ ...result.DelayHeaders })
        SetMaskOnOff(false);
    }

    useEffect(() => {
        if (visible) {
            loadData();
        }
    }, [visible])

    return (
        <div>
            {visible &&
                <Window
                    title='標案系統執行進度'
                    onClose={() => { onClose() }}
                    initialWidth={dimensions.width * .8}
                    initialHeight={dimensions.height * .9}
                    draggable={false}
                    resizable={false}
                    modal={true}
                >
                    {/* 控制Grid 渲染時機，避免動態表頭無值時，導致Grid元件欄寬設定異常 */}
                    {Object.keys(progressDataHeaders).length > 0 &&
                        <>
                            <h3>工程標案各月執行進度</h3>
                            <PCCProgressGrid
                                data={progressData}
                                headers={progressDataHeaders}
                            />
                        </>
                    }
                    <br />
                    {/* 控制Grid 渲染時機，避免動態表頭無值時，導致Grid元件欄寬設定異常 */}
                    {Object.keys(delayDataHeaders).length > 0 &&
                        <>
                            <h3>工程標案落後原因 </h3>
                            <PCCProgressGrid
                                data={delayData}
                                headers={delayDataHeaders}
                            />
                        </>
                    }
                </Window>
            }
        </div>
    )
}

export default PCCProgressWindow;
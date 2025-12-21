import React, { useState, useEffect } from 'react';
import { WindowResizehook } from '../../../../Hook/useWindowResize';
import { SetMaskOnOff } from '../../../../Basic/SDOExtension';
import { Window } from '@progress/kendo-react-dialogs';
import { getPccmDs01 } from '../ProjectFillCkptComService';

const PCCBasicDataWindow = props => {
    let { visible, onClose, pccProjectUid } = props;

    const dimensions = WindowResizehook();
    const [tbJsx, setTbJsx] = useState('');

    // 取得工程會標案基本資料
    const loadFormData = async () => {
        SetMaskOnOff(true);
        let dataObj = await getPccmDs01(pccProjectUid);
        genTableJsx(dataObj);
        SetMaskOnOff(false);
    }

    useEffect(() => {
        if (visible) {
            loadFormData();
        }
    }, [visible])

    // 動態產生Table Jsx
    const genTableJsx = (data) => {
        const colNames = Object.keys(data);
        const colVal = Object.values(data);

        let leftColJsx;
        const tbJsx = colNames.map((colName, i) => {
            // 表頭、內容 jsx
            const thtdDataJsx =
                <>
                    <th style={{ width: '10%' }}>{colName}</th>
                    <td style={{ width: '40%' }}>{colVal[i]}</td>
                </>
            // index 為偶數時，組成左側欄位
            if (i % 2 === 0) {
                leftColJsx = thtdDataJsx
            }

            // index 為奇數時，組合左右欄位成一列
            if (i % 2 !== 0) {
                return (
                    <tr>
                        {leftColJsx}
                        {thtdDataJsx}
                    </tr>
                )
            }

            // if current item is the last item in colNames & item's index is an odd number
            if ((i === (colNames.length - 1)) && i % 2 === 0) {
                return (
                    <tr>
                        <th style={{ width: '10%' }}>{colName}</th>
                        <td colspan={3}>{colVal[i]}</td>
                    </tr>
                )
            }
        })
        setTbJsx(tbJsx);
    }

    return (
        <div>
            {visible &&
                <Window
                    title='標案系統基本資料'
                    onClose={() => { onClose() }}
                    initialWidth={dimensions.width * .8}
                    initialHeight={dimensions.height * .9}
                    draggable={false}
                    resizable={false}
                    modal={true}
                >
                    <form>
                        <table>
                            <tbody>
                                {tbJsx}
                            </tbody>
                        </table>
                    </form>
                </Window>
            }
        </div>
    )
}

export default PCCBasicDataWindow;
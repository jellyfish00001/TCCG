import React from 'react';
import { SetMaskOnOff } from '../../../../Basic/SDOExtension';
import { WindowResizehook } from '../../../../Hook/useWindowResize';
import { Window } from '@progress/kendo-react-dialogs';
import { getPCCDs09 } from '../ProjectFillCkptComService';

const PCCDs09Window = props => {
    const { visible, onClose, pccProjectUid } = props;
    const dimensions = WindowResizehook();
    const [tbHtml, setTbHtml] = React.useState("");

    React.useEffect(() => {
        if (visible) {
            loadData();
        }
    }, [visible])

    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await getPCCDs09(pccProjectUid);
        genTableJsx(result);
        SetMaskOnOff(false);
    }

    // 動態產生Table Jsx
    const genTableJsx = (data) => {
        const colNames = Object.keys(data);

        let leftCol;
        const html = colNames.map((colName, i) => {
            const col =
                <>
                    <th style={{ width: '10%' }}>{colName}</th>
                    <td style={{ width: '40%' }}>{data[colName]}</td>
                </>;

            // index 為偶數時，組成左側欄位
            if (i % 2 === 0) {
                leftCol = col;
            }

            // index 為奇數時，組合左右欄位成一列
            if (i % 2 !== 0) {
                return (
                    <tr>
                        {leftCol}
                        {col}
                    </tr>
                )
            }

            // if current item is the last item in colNames & item's index is an odd number
            if ((i === (colNames.length - 1)) && i % 2 === 0) {
                return (
                    <tr>
                        <th style={{ width: '10%' }}>{colName}</th>
                        <td colspan={3}>{data[colName]}</td>
                    </tr>
                )
            }
        });
        setTbHtml(html);
    }

    return (
        <div>
            {visible &&
                <Window
                    title='工程標案決標資料'
                    onClose={() => { onClose() }}
                    initialWidth={dimensions.width * .8}
                    initialHeight={dimensions.height * .9}
                    draggable={false}
                    resizable={false}
                    modal={true}
                >
                    <form>
                        <table>
                            {tbHtml}
                        </table>
                    </form>
                </Window>
            }
        </div>
    )
}

export default PCCDs09Window;
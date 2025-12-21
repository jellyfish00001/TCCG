import React, { useState } from 'react';
import AssociatePCCForm from './AssociatePCCForm';
import { getProjectMapPCC } from '../ProjectFillCkptComService';
import AssociatePCCGrid from './AssociatePCCGrid';
import { Window } from '@progress/kendo-react-dialogs';
import { WindowResizehook } from '../../../../Hook/useWindowResize';

const AssociatePCCWindow = props => {
    let { visible, onClose, PROJECT_NO, reloadData, startWorkDate } = props
    const dimensions = WindowResizehook();
    const [gridData, setGridData] = useState();

    // 查詢關聯工程會標案資料
    const query = async (queryObj) => {
        let result = await getProjectMapPCC(queryObj);
        if (result !== null) {
            setGridData(result);
        }
    }

    return (
        <div>
            {visible &&
                <Window
                    title='關聯工程會標案'
                    onClose={() => { onClose() }}
                    initialWidth={dimensions.width * .8}
                    initialHeight={dimensions.height * .9}
                    draggable={false}
                    resizable={false}
                    modal={true}
                >
                    {/* 關聯工程會標案查詢form */}
                    <AssociatePCCForm
                        query={query}
                    />

                    {/* 關聯工程會標案計畫Grid */}
                    <AssociatePCCGrid
                        data={gridData}
                        onClose={() => { onClose() }}
                        PROJECT_NO={PROJECT_NO}
                        reloadData={reloadData}
                        startWorkDate={startWorkDate}
                    />
                </Window>
            }
        </div>
    )
}

export default AssociatePCCWindow;
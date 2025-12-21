import React from 'react';
import { showGlobalMessageBox } from '../../../../Route/RootMiddleware';
import ProjectFillCheckPointMain from '../../ProjectFillCheckPoint/ProjectFillCheckPointMain';

const ScheduleExecCheckPointMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            /**傳入的參數 */
            state: {
                projectNo,
                projAdjId,
                isShowBtn,
            } = {
                projectNo: null,
                projAdjId: null,
                isShowBtn: true,
            }
        }
    } = props;

    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => props.history.push('/Home'));
    }

    return (
        <ProjectFillCheckPointMain
            location={{ state: { projectNo, projAdjId, showSomeBtn: isShowBtn } }}
            type={"adjust"}
        />
    )
}

export default ScheduleExecCheckPointMain;
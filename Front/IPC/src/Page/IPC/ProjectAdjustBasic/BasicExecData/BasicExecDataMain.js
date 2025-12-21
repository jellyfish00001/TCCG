import React from 'react';
import { showGlobalMessageBox } from '../../../../Route/RootMiddleware';
import ProjectFillBasicMain from '../../ProjectFillBasic/ProjectFillBasicMain';

const BasicExecDataMain = (props) => {
    const {
        location: { state },
        location: {
            state: {
                projectNo,
                projAdjId
            }
        }
    } = props;

    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => props.history.push('/Home'));
    }

    return (
        <>
            {
                state &&
                <ProjectFillBasicMain
                    type={"adjust"}
                    location={{ state: { projectNo: projectNo, projAdjId: projAdjId } }}
                />
            }
        </>
    )
}

export default BasicExecDataMain;
import React, { useState, useEffect } from 'react';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { CheckIsRDECRole } from '../../../Basic/CommonService';
import ProjectFillCloseTable from './ProjectFillCloseTable';

export const ProjectFillCloseMain = (props) => {

    let {
        location: {
            state
        },
        location: {
            state: {
                projectNo,
                cycleData,
                isRdecFun,
                showSomeBtn,
            } = {
                projectNo: null,
                cycleData: '',
                isRdecFun: false,
                showSomeBtn: null
            }
        }
    } = props;

    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => props.history.push('/Home'));
    }


    return (
        <ProjectFillCloseTable
            projectNo={projectNo}
            cycleData={cycleData}
            isAudit={false}
            isRDEC={isRdecFun}
            showSomeBtn={showSomeBtn}
            state={state}
        />
    )
}
export default ProjectFillCloseMain
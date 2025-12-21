import React, { useState, useEffect } from 'react';
import { CheckIsRDECRole } from '../../../Basic/CommonService';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import ProjectFillCloseTable from './ProjectFillCloseTable';

export const ProjectFillCloseAuditMain = (props) => {

    let {
        location: {
            state
        },
        location: {
            state: {
                projectNo,
                cycleData,
                isRdecFun,
                showSomeBtn
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
            isAudit={true}
            isRDEC={isRdecFun}
            showSomeBtn={showSomeBtn}
            state={state}
        />
    )
}
export default ProjectFillCloseAuditMain
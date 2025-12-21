import React from 'react';
import { MultiSelect } from "@progress/kendo-react-dropdowns";
import ProjectAuditOpinionGrid from './ProjectAuditOpinionGrid';
import ProjectConferenceGrid from './ProjectConferenceGrid';
import ProjectDelayFillGrid from './ProjectDelayFillGrid';
import ProjectMergeLogGrid from './ProjectMergeLogGrid';
import { IsNullOrEmpty, FormatDate } from '../../../../Basic/SDOExtension';

export const ProjectAuditOpinionMain = (props) => {
    const {
        projectNo,
        projectFillAuditData,
        ddlData,
        specNoteSelData,
        setSpecNoteSelData,
        editAuditOpinionGridData,
        editConferenceGridData,
        editDelayFillGridData,
        editedMergeLogGridData,
        isRdecFun } = props;

    /**
     * MultiSelect onChange事件
     * @param {*} e 
     */
    const onChange = (e) => {
        setSpecNoteSelData([...e.value]);
    };

    /**
     * 顯示撤銷資料
     * @returns 
     */
    const showRevokeData = () => {
        let data = projectFillAuditData.RevokeData;
        if (IsNullOrEmpty(data)) {
            return null;
        }
        return (
            <>
                <div>核准日期：{FormatDate(data.APPRV_DATE)}</div>
                <div>撤銷原因：{data.OTHER_REASON}</div>
                <div>撤銷原因說明：{data.ADJUST_REASON}</div>
            </>
        )
    }

    return (
        <>
            <ProjectAuditOpinionGrid
                projectNo={projectNo}
                data={projectFillAuditData.ProjectEngineeringAuditOpinion}
                ipcMemoDdlData={ddlData.COM_IPCMEMO}
                planYearDdlData={ddlData.PLAN_YEAR}
                editedGridData={editAuditOpinionGridData}
                isRdecFun={isRdecFun}
            />

            <form>
                <table>
                    <tbody>
                        <tr>
                            <th>特殊加註</th>
                            <td>
                                <MultiSelect
                                    popupSettings={
                                        { className: "dropdown-text-size" }
                                    }
                                    placeholder="請選擇   "
                                    data={ddlData.SPEC_NOTE}
                                    textField="SET_VALUE"
                                    dataItemKey="SET_TYPE"
                                    onChange={onChange}
                                    value={specNoteSelData}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th>列管會議</th>
                            <td>
                                <ProjectConferenceGrid
                                    projectNo={projectNo}
                                    data={projectFillAuditData.ProjectConference}
                                    conferenceDdlData={ddlData.COM_CONFERENCEGENRE}
                                    editedGridData={editConferenceGridData}
                                    isRdecFun={isRdecFun}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th>資料逾期繳交或填報</th>
                            <td>
                                <ProjectDelayFillGrid
                                    projectNo={projectNo}
                                    data={projectFillAuditData.ProjectDelayfill}
                                    editedGridData={editDelayFillGridData}
                                    isRdecFun={isRdecFun}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th>分案/併案</th>
                            <td>
                                <ProjectMergeLogGrid
                                    projectNo={projectNo}
                                    data={projectFillAuditData.ProjectMergeLog}
                                    PromergeStatusDdlData={ddlData.PROMERGESTATUS}
                                    editedGridData={editedMergeLogGridData}
                                    isRdecFun={isRdecFun}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th>撤銷資料</th>
                            <td>{showRevokeData()}</td>
                        </tr>
                    </tbody>
                </table>
            </form>
        </>
    )
}
export default ProjectAuditOpinionMain;
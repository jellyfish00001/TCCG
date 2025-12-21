import React from 'react';
import { IsNullOrEmpty, FormatDate, SetMaskOnOff } from '../../../Basic/SDOExtension';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';
import { GetSetParam, downProjectAttachment } from '../../../Basic/CommonService';
import { formatNumber } from '@telerik/kendo-intl';
import ProjectPrintService from './ProjectPrintService';
import { loadCheckPoint } from '../ProjectFillCheckPoint/ProjectFillCheckPointService';
import { adjustHistory } from '../ProjectAdjuctHistory/AdjustHistoryService';

// 二、檢核點(執行情形)
const ProjectCkptCom = (props) => {
    const { projectCheckpoint, projectCkptCom } = props;
    const [DdlData, setDdlData] = React.useState({
        CP_KIND: [],
        checkpoint: [],
    });

    const getAllDdl = async () => {
        SetMaskOnOff(true);
        setDdlData({
            CP_KIND: await GetSetParam('CP_KIND', ''),
            checkpoint: await loadCheckPoint(projectCheckpoint.CP_KIND, true),
        });
        SetMaskOnOff(false);
    }
    React.useEffect(() => {
        if (!IsNullOrEmpty(projectCheckpoint)) {
            getAllDdl();
        }
    }, [projectCheckpoint])

    // 組檢核點
    const mapCusCheckpoint = () => {
        if (IsNullOrEmpty(projectCkptCom.CustomChkItemModels)) {
            return (
                <tr>
                    <td colSpan={5} style={{ textAlign: "center" }}>無資料</td>
                </tr>
            )
        }
        let result = projectCkptCom.CustomChkItemModels.map(x => {
            return (
                <tr>
                    <td colSpan={2}>{x.CHECKITEM_NAME}</td>
                    <td style={{ textAlign: "center" }}>{x.PROGRESS}%</td>
                    <td style={{ textAlign: "center" }}>{formatDate(x.ESTIMATED_ENDDATE)}</td>
                    <td style={{ textAlign: "center" }}>{formatDate(x.ACTUAL_ENDDATE)}</td>
                </tr>
            )
        })
        return result;
    }

    // 預定完成期限
    const SetEstimatedEndDate = (data) => {
        let endDate = null;
        if (data.length > 0) {
            endDate = data[data.length - 1].ESTIMATED_ENDDATE;
        }
        return IsNullOrEmpty(endDate) ? "無" : formatDate(endDate);
    }

    // 日期格式化
    const formatDate = (date) => {
        if (IsNullOrEmpty(date)) {
            return "";
        }
        else {
            return FormatDate(date, 'tYY/MM/DD')
        }
    }

    // 工程預定進度表
    const mapFile = () => {
        return projectCkptCom.FileModels.map(x => {
            return (
                <div>{formatDate(x.CRT_DATE)}
                    <a href='/' onClick={(e) => {
                        e.preventDefault();
                        downProjectAttachment(x.IDENTITY_FIELD);
                    }}>{x.FILE_NAME}</a>
                </div>
            )
        });
    }

    return (
        <>
            <p>二、檢核點</p>
            <p>（一）聯繫資訊</p>
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th>計畫實際承辦人</th>
                        <td>{projectCkptCom.REAL_CONTACT}</td>
                    </tr>
                    <tr>
                        <th>計畫承辦人電話</th>
                        <td>{projectCkptCom.REAL_TEL}</td>
                    </tr>
                    <tr>
                        <th>計畫承辦人信箱</th>
                        <td>{projectCkptCom.REAL_EMAIL}</td>
                    </tr>
                </table>
                {/* 工程類才需顯示 CP_KIND == "0" */}
                {projectCkptCom.CP_KIND == "0" &&
                    <table style={{ wordBreak: "break-all" }}>
                        <tr>
                            <th>工程會標案編號</th>
                            <td>{projectCkptCom.PCC_PROJECT_NO}</td>
                        </tr>
                        <tr>
                            <th>工程會標案名稱</th>
                            <td>{projectCkptCom.PCC_PROJECT_NAME}</td>
                        </tr>
                        <tr>
                            <th>標案承辦人</th>
                            <td>{projectCkptCom.FACTORY_CONTACT}</td>
                        </tr>
                        <tr>
                            <th>標案承辦人電話</th>
                            <td>{projectCkptCom.FACTORY_TEL}</td>
                        </tr>
                        <tr>
                            <th>以界接資料填報</th>
                            <td>{projectCkptCom.IS_USER_FTY_DATA == true ? "是" : "否"}</td>
                        </tr>
                    </table>
                }
            </form>
            <p>（二）檢核點完成日期</p>
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th style={{ width: "20%" }}>執行方式</th>
                        <td colSpan={4}>
                            {ProjectPrintService.findText(DdlData.CP_KIND, "SET_VALUE", "SET_TYPE", projectCheckpoint.CP_KIND)}/
                            {ProjectPrintService.findText(DdlData.checkpoint, "CHECKPOINT_CLASS", "CHECKPOINT_CLASS_ID", projectCheckpoint.RUNWAY_C)}
                        </td>
                    </tr>
                    <tr>
                        <th>計畫開始日期</th>
                        <td colSpan={4}>{formatDate(projectCheckpoint.CONTROL_DATE1)}</td>
                    </tr>
                    <tr>
                        <th colSpan={2} style={{ textAlign: "center", width: "35%" }}>檢核點</th>
                        <th style={{ textAlign: "center" }}>管考進度</th>
                        <th style={{ textAlign: "center" }}>預定完成日期</th>
                        <th style={{ textAlign: "center" }}>實際完成日期</th>
                    </tr>
                    {mapCusCheckpoint()}
                    <tr>
                        <th>預定完成期限</th>
                        <td colSpan={4}>
                            {SetEstimatedEndDate(projectCheckpoint.CusCheckpointModels)}
                        </td>
                    </tr>
                    <tr>
                        <th>總期程調整歷程</th>
                        <td colSpan={4}>{adjustHistory(projectCheckpoint.AdjustScheHistoryModels, "Y")}</td>
                    </tr>
                    <tr>
                        <th>分月期程調整歷程</th>
                        <td colSpan={4}>{adjustHistory(projectCheckpoint.AdjustScheHistoryModels, "M")}</td>
                    </tr>
                    <tr>
                        <th>備註</th>
                        <td colSpan={4}><TextAreaWrapInput value={projectCheckpoint.MEMO} /></td>
                    </tr>
                    {/* 工程類才需顯示 CP_KIND == "0" */}
                    {projectCkptCom.CP_KIND == "0" &&
                        <>
                            <tr>
                                <th>契約預定竣工日</th>
                                <td colSpan={4}>{formatDate(projectCkptCom.CONTRACT_FINISH_DATE)}</td>
                            </tr>
                            <tr>
                                <th>工程預定進度表</th>
                                <td colSpan={4}>{mapFile()}</td>
                            </tr>
                            <tr>
                                <th>發包金額(元)</th>
                                <td colSpan={4}>{formatNumber(projectCkptCom.PROCUREMENT_AMT, "n0")}</td>
                            </tr>
                            <tr>
                                <th>決標金額(元)</th>
                                <td colSpan={4}>{formatNumber(projectCkptCom.TENDER_AWARDING_AMT, "n0")}</td>
                            </tr>
                        </>
                    }
                </table>
            </form>
            {/* 處理標格與標題之間的距離 */}
            <p className='preview-table-p'></p>
        </>
    );
}

export default ProjectCkptCom;
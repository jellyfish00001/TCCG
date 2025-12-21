import React from 'react';
import { IsNullOrEmpty, FormatDate, SetMaskOnOff } from '../../../Basic/SDOExtension';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';
import { GetSetParam } from '../../../Basic/CommonService';
import ProjectPrintService from './ProjectPrintService';
import { loadCheckPoint } from '../ProjectFillCheckPoint/ProjectFillCheckPointService';
import { adjustHistory } from '../ProjectAdjuctHistory/AdjustHistoryService';

// 二、檢核點設定
const ProjectCheckPoint = (props) => {
    const { projectCheckpoint } = props;
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

    // 日期格式化
    const formatDate = (date) => {
        if (IsNullOrEmpty(date)) {
            return "";
        }
        else {
            return FormatDate(date, 'tYY/MM/DD')
        }
    }

    // 組檢核點
    const mapCusCheckpoint = () => {
        if (projectCheckpoint.CusCheckpointModels.length == 0) {
            return (
                <tr>
                    <td colSpan={4} style={{ textAlign: "center" }}>無資料</td>
                </tr>
            )
        }
        let result = projectCheckpoint.CusCheckpointModels.map(x => {
            return (
                <tr>
                    <td colSpan={2}>{x.CHECKITEM_NAME}</td>
                    <td style={{ textAlign: "center" }}>{x.PROGRESS}%</td>
                    <td style={{ textAlign: "center" }}>{formatDate(x.ESTIMATED_ENDDATE)}</td>
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

    return (
        <>
            <p>二、檢核點設定</p>
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th style={{ width: "20%" }}>執行方式</th>
                        <td colSpan={3}>
                            {ProjectPrintService.findText(DdlData.CP_KIND, "SET_VALUE", "SET_TYPE", projectCheckpoint.CP_KIND)}/
                            {ProjectPrintService.findText(DdlData.checkpoint, "CHECKPOINT_CLASS", "CHECKPOINT_CLASS_ID", projectCheckpoint.RUNWAY_C)}
                        </td>
                    </tr>
                    <tr>
                        <th>計畫開始日期</th>
                        <td colSpan={3}>{formatDate(projectCheckpoint.CONTROL_DATE1)}</td>
                    </tr>
                    <tr>
                        <th colSpan={2} style={{ textAlign: "center", width: "40%" }}>檢核點</th>
                        <th style={{ textAlign: "center" }}>管考進度</th>
                        <th style={{ textAlign: "center" }}>預定完成日期</th>
                    </tr>
                    {mapCusCheckpoint()}
                    <tr>
                        <th>預定完成期限</th>
                        <td colSpan={3}>
                            {SetEstimatedEndDate(projectCheckpoint.CusCheckpointModels)}
                        </td>
                    </tr>
                    <tr>
                        <th>總期程調整歷程</th>
                        <td colSpan={3}>{adjustHistory(projectCheckpoint.AdjustScheHistoryModels, "Y")}</td>
                    </tr>
                    <tr>
                        <th>分月期程調整歷程</th>
                        <td colSpan={3}>{adjustHistory(projectCheckpoint.AdjustScheHistoryModels, "M")}</td>
                    </tr>
                    <tr>
                        <th>備註</th>
                        <td colSpan={3}><TextAreaWrapInput value={projectCheckpoint.MEMO_CHK_POINT} /></td>
                    </tr>
                </table>
            </form>
            {/* 處理標格與標題之間的距離 */}
            <p className='preview-table-p'></p>
        </>
    );
}

export default ProjectCheckPoint;
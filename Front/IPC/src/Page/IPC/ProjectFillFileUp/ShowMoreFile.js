import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { downProjectAttachment, downProjectAttachmentZip } from '../../../Basic/CommonService';
import { IsNullOrEmpty, FormatDate } from '../../../Basic/SDOExtension';

export const ShowMoreFile = (props) => {
    const { isRdecFun, otherFiles } = props;
    // div 是否隱藏
    const [isDivHide, setIsDivHide] = React.useState(true);

    // adj以外的檔案
    const mapFile = (fileKind) => {
        let result = otherFiles.ProjectOtherAttachmentModels.filter(x => x.FILE_KIND === fileKind).map((x, index) =>
            <>
                {index !== 0 && "、"}
                <a href='/' onClick={(e) => {
                    e.preventDefault();
                    downProjectAttachment(x.IDENTITY_FIELD);
                }}>{x.FILE_MEMO}</a>
            </>
        );
        return result;
    }

    // 機關期程調整准簽
    const mapScheduleFile = () => {
        let result = [];
        result.push(otherFiles.AdjustScheHistoryModels.map((x, index) => {
            let fileName = `第${x.SEQ}次${x.SCHE_TYPE === "Y" ? "總期程調整" : "分月期程調整"}`;
            return (
                <>
                    {index !== 0 && "、"}
                    <a href='/' onClick={(e) => {
                        e.preventDefault();
                        // 主管排除 FILE_KIND = 21,24 研考會/智發會期程調整准簽
                        let files = otherFiles.ProjectOtherAttachmentModels.filter(y => y.FILE_TYPE === "SCHE" && y.SOURCE_ID === x.PROJ_ADJ_ID);
                        if (!isRdecFun) {
                            files = files.filter(y => !(y.FILE_KIND === "21" || y.FILE_KIND === "24"));
                        }
                        let identityFields = files.map(x => x.IDENTITY_FIELD);
                        downProjectAttachmentZip(identityFields, fileName);
                    }}>{fileName}</a>
                </>
            )
        }));
        result.push(<br />);
        result.push(otherFiles.AdjustScheHistoryModels.map(x => {
            if (x.SCHE_TYPE === "Y") {
                return (
                    <li key={x.SEQ} style={{ display: 'flex', alignItems: 'center' }}>
                        {"第" + x.SEQ + "次：" +
                            "原定" + (IsNullOrEmpty(x.ORI_ESTIMATED_ENDDATE) ? '' : FormatDate(x.ORI_ESTIMATED_ENDDATE)) + "完成，" +
                            "調整至" + (IsNullOrEmpty(x.ADJ_LAST_DATE) ? '' : FormatDate(x.ADJ_LAST_DATE)) +
                            "(核准日期：" + (IsNullOrEmpty(x.APPRV_DATE) ? '' : FormatDate(x.APPRV_DATE)) + ")。" +
                            "調整原因：" + (IsNullOrEmpty(x.REASON) ? '' : x.REASON)
                        }
                    </li>
                )
            }
            else if (x.SCHE_TYPE === "M") {
                return (
                    <li key={x.SEQ} style={{ display: 'flex', alignItems: 'center' }}>
                        {"第" + x.SEQ + "次：" +
                            (IsNullOrEmpty(x.ADJ_LAST_DATE) ? '' : FormatDate(x.ADJ_LAST_DATE)) +
                            "(核准日期：" + (IsNullOrEmpty(x.APPRV_DATE) ? '' : FormatDate(x.APPRV_DATE)) + ")。" +
                            "調整原因：" + (IsNullOrEmpty(x.REASON) ? '' : x.REASON)
                        }
                    </li>
                )
            }
            else {
                return null;
            }
        }));
        return result;
    }

    // 基本資料調整佐證文件、撤銷核定公文
    const mapMutiAdjustFile = (fileKind) => {
        let result = [];
        // 因調整的檔案為多檔上傳，須將同次調整的該FILE_KIND檔案一起下載
        let projAdjIds = Array.from(new Set(otherFiles.ProjectOtherAttachmentModels
            .filter(x => x.FILE_KIND === fileKind).map(x => x.SOURCE_ID)));
        for (let i = 0; i < projAdjIds.length; i++) {
            let file = otherFiles.ProjectOtherAttachmentModels.filter(x => x.FILE_KIND === fileKind && x.SOURCE_ID === projAdjIds[i]);
            result.push(
                <>
                    {i !== 0 && "、"}
                    <a href='/' onClick={(e) => {
                        e.preventDefault();
                        downProjectAttachmentZip(file.map(x => x.IDENTITY_FIELD), file[0].FILE_MEMO);
                    }}>{file[0].FILE_MEMO}</a>
                </>
            )
        }
        return result;
    }

    return (
        <>
            <div className="fn-buttons">
                <Button title={isDivHide ? "顯示其他檔案" : "隱藏其他檔案"} onClick={() => setIsDivHide(!isDivHide)}>{isDivHide ? "顯示其他檔案" : "隱藏其他檔案"}</Button>
            </div>
            <div style={isDivHide ? { display: "none" } : {}}>
                <table>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>1.</td>
                        <td>前瞻計畫核定文件：{mapFile("05")}</td>
                    </tr>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>2.</td>
                        <td>中央補助款核定文件：{mapFile("06")}</td>
                    </tr>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>3.</td>
                        <td>工程預定進度網圖：{mapFile("02")}</td>
                    </tr>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>4.</td>
                        <td>實地查證紀錄：{mapFile("07")}</td>
                    </tr>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>5.</td>
                        <td>實地查證參採資料：{mapFile("08")}</td>
                    </tr>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>6.</td>
                        <td>機關期程調整資料：{mapScheduleFile()}</td>
                    </tr>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>7.</td>
                        <td>基本資料調整佐證文件：{mapMutiAdjustFile("09")}</td>
                    </tr>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>8.</td>
                        <td>結案資料：{mapFile("16")}</td>
                    </tr>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>9.</td>
                        <td>分案核定公文：{mapFile("17")}</td>
                    </tr>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>10.</td>
                        <td>併案核定公文：{mapFile("18")}</td>
                    </tr>
                    <tr style={{ verticalAlign: "top" }}>
                        <td>11.</td>
                        <td>撤銷核定公文：{mapMutiAdjustFile("19")}</td>
                    </tr>
                </table>
            </div>
        </>
    )
}
export default ShowMoreFile;
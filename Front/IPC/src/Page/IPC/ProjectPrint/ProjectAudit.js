import React from 'react';
import ProjectField from './ProjectField';
import { IsNullOrEmpty, FormatDate } from '../../../Basic/SDOExtension';
import { GetSetParam, downProjectAttachment } from "../../../Basic/CommonService";
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';
import ProjectPrintService from './ProjectPrintService';

// 四、管考備註
const ProjectAudit = (props) => {
    const { projectAudit, isRdecFun, isShowProjectField } = props;
    let count = 0;
    const numberList = ["一", "二", "三", "四"];

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState({
        SPEC_NOTE: [],
        COM_CONFERENCEGENRE: [],
        PROMERGESTATUS: [],
        COM_IPCMEMO: [],
    });
    // 年終考核type資料
    const type1 = projectAudit.ProjectCloseDetails.filter(x => x.CLOSE_DETAILS_TYPE == "1");
    const type2 = projectAudit.ProjectCloseDetails.filter(x => x.CLOSE_DETAILS_TYPE == "2");
    const type3 = projectAudit.DelayApply;
    const type4 = projectAudit.ProjectCloseDetails.filter(x => x.CLOSE_DETAILS_TYPE == "4");
    const type5 = projectAudit.ProjectMergeLog.filter(x => x.MERGE_STATUS == "01");
    const totalScore = (type2.length > 0 ? 5 : 0) + (5 * type3.length) + (3 * type4.length) + (type5.length > 0 ? 3 : 0);

    const loadAllDdl = async () => {
        let arr = [
            // 特殊加註
            GetSetParam("SPEC_NOTE", "", true),
            // 會議種類
            GetSetParam("COM_CONFERENCEGENRE", "", true),
            // 分案或併案
            GetSetParam("PROMERGESTATUS", "", true),
            // 管考備註項目
            GetSetParam("COM_IPCMEMO", "", true),
        ];
        let dropDowns = await ProjectPrintService.loadAllDropDowns(arr);
        if (dropDowns.length > 0) {
            setDdlData({
                // 特殊加註
                SPEC_NOTE: [...dropDowns[0]],
                // 會議種類
                COM_CONFERENCEGENRE: [...dropDowns[1]],
                // 分案或併案
                PROMERGESTATUS: [...dropDowns[2]],
                // 管考備註項目
                COM_IPCMEMO: [...dropDowns[3]],
            });
        }
    }

    React.useEffect(() => {
        loadAllDdl();

    }, [])

    // 日期格式化
    const formatDate = (date) => {
        if (IsNullOrEmpty(date)) {
            return ""
        }
        else {
            return FormatDate(date, "tYY/MM/DD")
        }
    }

    // 期間
    const DateCell = (item) => {
        return (
            // YYY_MM
            <td style={{ textAlign: "center" }}>{item.YEAR}_{item.MONTH}</td>
        )
    }

    // 多行文字顯示欄位
    const textAreaWrapCell = (data) => {
        return (
            <td>
                <TextAreaWrapInput value={data} />
            </td>
        )
    }

    /**
     * 備註
     * @param {*} memoData 
     * @returns 
     */
    const memoCell = (memoData) => {
        return (
            <td>
                {memoData.map((x, index) => {
                    return (
                        <div>{index + 1}.{ProjectPrintService.findText(ddlData.COM_IPCMEMO, "SET_VALUE", "SET_TYPE", x.SET_TYPE)}

                        </div>
                    )
                })}
            </td>
        )
    }

    // 平時管考意見
    const auditOpinion = () => {
        return (
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th style={{ textAlign: "center", width: "5%" }}>期間</th>
                        <th style={{ textAlign: "center" }}>意見</th>
                        <th style={{ textAlign: "center" }}>備註</th>
                    </tr>
                    {projectAudit.ProjectEngineeringAuditOpinion.length == 0 &&
                        <tr>
                            <td colSpan={3} style={{ textAlign: "center" }}>無資料</td>
                        </tr>
                    }
                    {projectAudit.ProjectEngineeringAuditOpinion.map(x =>
                        <tr>
                            {DateCell(x)}
                            {textAreaWrapCell(x.AUDIT_OPINION)}
                            {memoCell(x.ComIPCMemoMappingData)}
                        </tr>

                    )}
                </table>
            </form>
        )

    }

    // 平時管考意見
    const auditTable = () => {
        return (
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th>特殊加註</th>
                        <td>
                            {ddlData.SPEC_NOTE.filter(x =>
                                projectAudit.SpecNoteMappingData.map(x => x.SET_TYPE).includes(x.SET_TYPE)
                            ).map(x => x.SET_VALUE).join("、")}
                        </td>
                    </tr>
                    <tr>
                        <th>列管會議</th>
                        <td>
                            {projectAudit.ProjectConference.map((x, index) => {
                                return (
                                    <div>{index + 1}. {ProjectPrintService.findText(ddlData.COM_CONFERENCEGENRE, "SET_VALUE", "SET_TYPE", x.CONFERENCE_GENRE)}
                                        （{formatDate(x.CONFERENCE_TIME)} 第{x.CONFERENCE_NUM}次會議）
                                    </div>
                                )
                            })}
                        </td>
                    </tr>
                    <tr>
                        <th>資料逾期繳交或填報</th>
                        <td>
                            {projectAudit.ProjectDelayfill.map((x, index) => {
                                return (
                                    <div>{index + 1}. {formatDate(x.FILL_TIME)} {x.FILL_REASON}</div>
                                )
                            })}
                        </td>
                    </tr>
                    <tr>
                        <th>分案或併案</th>
                        <td>
                            {projectAudit.ProjectMergeLog.map(x => {
                                return (
                                    <div>
                                        {formatDate(x.PROMERGE_DATE)}
                                        {x.MERGE_STATUS == "01" ? "簽准分案" : "簽准併案"}
                                        （<a href='/' onClick={(e) => {
                                            e.preventDefault();
                                            downProjectAttachment(x.File.IDENTITY_FIELD)
                                        }}>{x.File.FILE_NAME}</a>）
                                    </div>
                                )
                            })}
                        </td>
                    </tr>
                    <tr>
                        <th>撤銷資料</th>
                        <td>{showRevokeData()}</td>
                    </tr>
                </table>
            </form>
        )
    }

    /**
     * 顯示撤銷資料
     * @returns 
     */
    const showRevokeData = () => {
        let data = projectAudit.RevokeData;
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

    // 年終考核意見
    const close = () => {
        return (
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th>結案或撤銷日期</th>
                        <td colSpan={2}>
                            {IsNullOrEmpty(projectAudit.ProjectBasic.FINISH_DATE)
                                ? ""
                                : formatDate(projectAudit.ProjectBasic.FINISH_DATE) + projectAudit.ProjectBasic.PROJECT_STATUS
                            }
                        </td>
                    </tr>
                    <tr>
                        <th>基本資料</th>
                        <td colSpan={2}>{projectAudit.ProjectBasic.SCORE_A} 分</td>
                    </tr>
                    <tr>
                        <th>報表品質</th>
                        <td colSpan={2}>各項報表資料內容欠周詳，內容過於簡略，經智發會通知改善{type1.length}次。
                            {type1.map(x => {
                                return (
                                    <div>{formatDate(x.REF_DATE)}：{x.REF_MEMO}</div>
                                )
                            })}
                        </td>
                    </tr>
                    <tr>
                        <th rowSpan={5}>特殊扣分</th>
                        <td style={{ width: "42%" }}>符合列管標準，卻未依第4點規定，主動提報智發會列管，考核分數再扣減{type2.length > 0 ? 5 : 0}分。</td>
                        <td style={{ width: "42%" }}>
                            {type2.map(x => {
                                return (
                                    <div>{formatDate(x.REF_DATE)}：{x.REF_MEMO}</div>
                                )
                            })}
                        </td>
                    </tr>
                    <tr>
                        <td>申請計畫調整，未依第9點規定於期限內提出，按次扣減考核分數5分。共{type3.length}次，扣減{5 * type3.length}分。</td>
                        <td>
                            {type3.length == 0 ? "" :
                                <>
                                    未於期限內提出之期程調整：
                                    {type3.map(x => {
                                        return (
                                            <div>{formatDate(x.CRT_DATE)}{x.SCHE_TYPE}</div>
                                        )
                                    })}
                                </>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td>經查證填報不實者，按次扣減該計畫年終考核分數3分。共{type4.length}次，扣減{3 * type4.length}分。</td>
                        <td>
                            {type4.map(x => {
                                return (
                                    <div>{formatDate(x.REF_DATE)}：{x.REF_MEMO}</div>
                                )
                            })}
                        </td>
                    </tr>
                    <tr>
                        <td>計畫於管考期間，申請分案列管，考核分數再扣減3分。</td>
                        <td>
                            {type5.map(x => {
                                return (
                                    <div>{formatDate(x.PROMERGE_DATE)} 簽准分案</div>
                                )
                            })}
                        </td>
                    </tr>
                    <tr>
                        <td colSpan={2}><b>合計扣減</b> {totalScore} 分</td>
                    </tr>
                    <tr>
                        <th>年終考核備註</th>
                        <td colSpan={2}><TextAreaWrapInput value={projectAudit.ProjectBasic.NOTES_FOR_BUDGET} /></td>
                    </tr>
                </table>
            </form>
        )
    }

    return (
        <>
            <p>四、管考備註</p>
            <p>（{numberList[count++]}）平時管考意見</p>
            {auditOpinion()}
            {auditTable()}
            {isShowProjectField &&
                <ProjectField
                    projectField={projectAudit.ProjectFactFinding}
                    title={`（${numberList[count++]}）實地查證意見`}
                />
            }
            <p>（{numberList[count++]}）年終考核意見</p>
            {close()}
            {isRdecFun &&
                <>
                    <p>（{numberList[count++]}）其他管考備註</p>
                    <form>
                        <table style={{ wordBreak: "break-all" }}>
                            <tr>
                                <th>其他管考備註</th>
                                {textAreaWrapCell(projectAudit.ProjectBasic.NOTES_FOR_SCHEDULE)}
                            </tr>
                        </table>
                    </form>
                </>
            }
        </>
    );
}

export default ProjectAudit;
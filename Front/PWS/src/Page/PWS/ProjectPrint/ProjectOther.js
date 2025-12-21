import React from 'react';
import { IsNullOrEmpty, FormatDate } from '../../../Basic/SDOExtension';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';

// 三、執行情形（五）其它資訊
const ProjectOther = (props) => {
    const { projectOther, title, checkList } = props;
    let count = 0;

    // 日期格式化
    const formatDate = (date) => {
        if (IsNullOrEmpty(date)) {
            return ""
        }
        else {
            return FormatDate(date, "tYY/MM/DD")
        }
    }

    // 招標情形
    const bid = () => {
        let result = projectOther.ProjectBid.map(x => {
            let detailType0Data = projectOther.ProjectBidDetail.filter(y => y.BID_KIND == x.BID_KIND && y.DETAIL_TYPE == 0);
            let detailType1Data = projectOther.ProjectBidDetail.filter(y => y.BID_KIND == x.BID_KIND && y.DETAIL_TYPE == 1);
            if (!IsNullOrEmpty(x.AWARD_BID_DATE) || !IsNullOrEmpty(x.BID_TENDER) ||
                detailType0Data.length != 0 || detailType1Data.length != 0) {
                return (
                    <form>
                        <table style={{ wordBreak: "break-all" }}>
                            <tr>
                                <th colSpan={3} style={{ textAlign: "center" }}>{x.BID_NAME}</th>
                            </tr>
                            <tr>
                                <th style={{ width: "15%" }}>決標日期</th>
                                <td colSpan={2}>{formatDate(x.AWARD_BID_DATE)}</td>
                            </tr>
                            <tr>
                                <th>決標廠商</th>
                                <td colSpan={2}>{x.BID_TENDER}</td>
                            </tr>
                            <tr>
                                <th style={{ textAlign: "center" }}>流標次數</th>
                                <th style={{ textAlign: "center", width: "20%" }}>流標日期</th>
                                <th style={{ textAlign: "center" }}>流標原因</th>
                            </tr>
                            {bidDetail(detailType0Data)}
                            <tr>
                                <th style={{ textAlign: "center" }}>廢標次數</th>
                                <th style={{ textAlign: "center" }}>廢標日期</th>
                                <th style={{ textAlign: "center" }}>廢標原因</th>
                            </tr>
                            {bidDetail(detailType1Data)}
                        </table>
                    </form>
                )
            }
        });
        return result;
    }

    // 流標/廢標歷程
    const bidDetail = (data) => {
        if (data.length == 0) {
            return (
                <tr>
                    <td colSpan={3} style={{ textAlign: "center" }}>無資料</td>
                </tr>
            )
        }
        return data.map((x, index) => {
            return (
                <tr>
                    <td style={{ textAlign: "center" }}>{index + 1}</td>
                    <td style={{ textAlign: "center" }}>{formatDate(x.DETAIL_DATE)}</td>
                    <td><TextAreaWrapInput value={x.DETAIL_REASON} /></td>
                </tr>
            )
        })
    }

    // 相關活動
    const activity = () => {
        return <form>
            <table style={{ wordBreak: "break-all" }}>
                {projectOther.ProjectActivity.map(x => {
                    return (
                        <tr>
                            <th>{x.ACTIVITY_KIND_NAME}</th>
                            {x.ACTIVITY_KIND == 4 && x.editType == 0
                                ? <td colSpan={3}>無資料</td>
                                :
                                <>
                                    <td>{x.ACTIVITY_KIND != 4
                                        ? IsNullOrEmpty(x.IS_ACTIVITY) ? "" : x.IS_ACTIVITY ? "有" : "無"
                                        : x.ACTIVITY_NAME}
                                    </td>
                                    <th>活動日期</th>
                                    <td>{formatDate(x.ACTIVITY_DATE)}</td>
                                </>
                            }
                        </tr>
                    )
                })}
            </table>
        </form>;
    }

    // 相關審查
    const review = () => {
        const content = () => {
            return projectOther.SetParam.map(x => {
                return (
                    <>
                        {projectOther.ProjectReview.filter(y => y.REVIEW_KIND == x.SET_TYPE).map((y) => {
                            return (
                                <tr>
                                    <th style={{ width: "18%" }}>{x.SET_VALUE}</th>
                                    <td style={{ width: "18%" }}>
                                        {y.REVIEW_KIND != "04"
                                            ? IsNullOrEmpty(y.IS_REVIEW) ? "" : y.IS_REVIEW ? "有" : "無"
                                            : y.OTH_RVWNAME}
                                    </td>
                                    <th>送件日期</th>
                                    <td>{formatDate(y.SEND_DATE)}</td>
                                    <th>核定日期</th>
                                    <td>{formatDate(y.REVIEW_DATE)}</td>
                                </tr>
                            )
                        })}
                        {x.SET_TYPE == "04" && projectOther.ProjectReview.filter(y => y.REVIEW_KIND == "04").length == 0 &&
                            <tr>
                                <th>{x.SET_VALUE}</th>
                                <td colSpan={5}>無資料</td>
                            </tr>
                        }
                    </>
                )
            })
        }

        return <form><table style={{ wordBreak: "break-all" }}>{content()}</table></form>;
    }

    // 廠商資訊
    const tender = () => {
        return (
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th style={{ textAlign: "center", width: "12%" }}>廠商類別</th>
                        <th style={{ textAlign: "center" }}>廠商名稱</th>
                        <th style={{ textAlign: "center", width: "7%" }}>統編</th>
                        <th style={{ textAlign: "center" }}>地址</th>
                        <th style={{ textAlign: "center" }}>聯絡人</th>
                        <th style={{ textAlign: "center" }}>聯絡人電話</th>
                        <th style={{ textAlign: "center" }}>聯絡人Email</th>
                    </tr>
                    {projectOther.ProjectTender.length == 0 &&
                        <tr>
                            <td colSpan={7} style={{ textAlign: "center" }}>無資料</td>
                        </tr>
                    }
                    {projectOther.ProjectTender.map(x =>
                        <tr>
                            <td>{x.TENDER_KIND_NAME}</td>
                            <td>{x.TENDER_NAME}</td>
                            <td>{x.REG_NO}</td>
                            <td>{x.TENDER_ADDR}</td>
                            <td>{x.CONTACT_NAME}</td>
                            <td>{x.CONTACT_PHONE}</td>
                            <td>{x.CONTACT_EMAIL}</td>
                        </tr>
                    )}
                </table>
            </form>
        )
    }

    return (
        <>
            <p>{title}</p>
            {checkList[0] &&
                <>
                    <p>{++count}.招標情形</p>
                    {bid()}
                </>
            }

            {checkList[1] &&
                <>
                    <p>{++count}.相關活動</p>
                    {activity()}
                </>
            }

            {checkList[2] &&
                <>
                    <p>{++count}.相關審查</p>
                    {review()}
                </>
            }

            {checkList[3] &&
                <>
                    <p>{++count}.廠商資訊</p>
                    {tender()}
                </>
            }
        </>
    );
}

export default ProjectOther;
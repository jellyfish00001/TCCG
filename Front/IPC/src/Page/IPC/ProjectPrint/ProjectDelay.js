import React from "react";
import { FormatDate, IsNullOrEmpty } from "../../../Basic/SDOExtension";

// 三、執行情形（二）落後原因分析
const ProjectDelay = (props) => {
    const { projectDelay, title } = props;

    return (
        <>
            <p>{title}</p>
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th style={{ textAlign: "center", width: "5%" }}>年度</th>
                        <th style={{ textAlign: "center", width: "5%" }}>月份</th>
                        <th style={{ textAlign: "center", width: "7%" }}>落後類型</th>
                        <th style={{ textAlign: "center" }}>落後類別</th>
                        <th style={{ textAlign: "center" }}>落後項目</th>
                        <th style={{ textAlign: "center" }}>責任歸屬</th>
                        <th style={{ textAlign: "center" }}>落後原因</th>
                        <th style={{ textAlign: "center" }}>解決對策</th>
                        <th style={{ textAlign: "center" }}>須協調事項</th>
                        <th style={{ textAlign: "center", width: "10%" }}>改進完成期限</th>
                    </tr>
                    {projectDelay.length == 0 &&
                        <tr>
                            <td colSpan={10} style={{ textAlign: "center" }}>無資料</td>
                        </tr>
                    }
                    {projectDelay.map(x =>
                        <tr>
                            <td style={{ textAlign: "center" }}>{x.DATA_YEAR}</td>
                            <td style={{ textAlign: "center" }}>{x.DATA_MONTH}</td>
                            <td style={{ textAlign: "center" }}>{x.DELAY_KIND}</td>
                            <td>{x.DELAY_CLASS_C}</td>
                            <td>{x.DELAY_SUBCLASS_C}</td>
                            <td>{x.DELAY_RESPON}</td>
                            <td>{x.DELAY_CAUSAL}</td>
                            <td>{x.SOLUTION}</td>
                            <td>{x.COORDINATION}</td>
                            <td style={{ textAlign: "center" }}>
                                {IsNullOrEmpty(x.DEADLINES) ? "" : FormatDate(x.DEADLINES, 'tYY-MM-DD')}
                            </td>
                        </tr>
                    )}
                </table>
            </form>
        </>
    );

}

export default ProjectDelay;
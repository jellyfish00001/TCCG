import React from 'react';
import { formatNumber } from '@telerik/kendo-intl';
import { IsNullOrEmpty, FormatDate } from '../../../Basic/SDOExtension';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';
import ProjectFillBudgetExecService from '../ProjectFillBudgetExec/ProjectFillBudgetExecService';

// 三、執行情形（三）預算執行情形
export const ProjectBudgetExec = (props) => {
    const { projectBudgetExec, title } = props;

    // 選單資料
    const [ddlData, setDdlData] = React.useState({
        IPCBGTEXECFAILED: [],
        IPCBGTEXECFAILEDDUTY: [],
    });

    const loadDdlData = async () => {
        let dropDowns = await ProjectFillBudgetExecService.getAllDdlData();
        if (dropDowns.length > 0) {
            setDdlData({
                // 原因 
                IPCBGTEXECFAILED: [...dropDowns[0]],
                // 責任歸屬
                IPCBGTEXECFAILEDDUTY: [...dropDowns[1]],
            });
        }
    }

    React.useEffect(() => {
        loadDdlData();
    }, [projectBudgetExec])

    /**
     * 期間
     * @param {*} item 
     * @returns 
     */
    const periodCell = (item) => {
        return (
            <td style={{ textAlign: "center" }}>
                {item.EXEC_YEAR}_{item.EXEC_MONTH}
            </td>
        )
    }

    /**
     * 原因/責任歸屬欄位
     * @param {*} mappingData 
     * @param {*} ddl 
     * @returns 
     */
    const failedCell = (mappingData, ddl) => {
        let type = mappingData.map(x => x.SET_TYPE);
        let data = ddl.filter(x => {
            if (type.includes(x.SET_TYPE)) {
                return x;
            }
        })
        return (
            <td>
                {data.map(x => x.SET_VALUE).join("、")}
            </td>
        )
    }

    // 數字格式
    const formatNumberCell = (data, format) => {
        return (
            <td style={{ textAlign: "right" }}>{formatNumber(data, format)}</td>
        )
    }

    // 日期格式化
    const formatDate = (data) => {
        if (IsNullOrEmpty(data)) {
            return (
                <td></td>
            )
        }
        else {
            return (
                <td style={{ textAlign: "center" }}>{FormatDate(data, "tYY/MM/DD")}</td>
            )
        }
    }

    // 多行文字顯示欄位
    const textAreaWrapCell = (data) => {
        return (
            <td style={{ wordWrap: "break-word" }}>
                <TextAreaWrapInput value={data} />
            </td>
        )
    }

    return (
        <>
            <p>{title}</p>
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th rowSpan={2} style={{ textAlign: "center", width: "5%" }}>期<br />間</th>
                        <th colSpan={7} style={{ textAlign: "center" }}>累計執行情形</th>
                        <th colSpan={7} style={{ textAlign: "center" }}>本年度執行情形</th>
                        <th rowSpan={2} style={{ textAlign: "center", width: "5%" }}>填報<br />日期</th>
                    </tr>
                    <tr>
                        <th style={{ textAlign: "center", width: "5%" }}>累計<br />預定<br />支用<br />(元)<br />(a)</th>
                        <th style={{ textAlign: "center", width: "5%" }}>累計<br />實際<br />完成<br />金額<br />(元)<br />(b+c)</th>
                        <th style={{ textAlign: "center", width: "5%" }}>累計<br />實際<br />支用<br />(元)<br />(b)</th>
                        <th style={{ textAlign: "center", width: "5%" }}>應付<br />未付<br />數<br />(元)<br />(c)</th>
                        <th style={{ textAlign: "center", width: "5%" }}>結餘數<br />(元)<br />(d)</th>
                        <th style={{ textAlign: "center", width: "5%" }}>累計<br />實際<br />支用<br />數<br />(元)<br />(e)</th>
                        <th style={{ textAlign: "center", width: "5%" }}>累計<br />執行<br />率<br />(%)<br />(e/a)</th>

                        <th style={{ textAlign: "center", width: "5%" }}>本年度<br />可支用<br />預算數<br />(元)<br />(j)</th>
                        <th style={{ textAlign: "center", width: "5%" }}>本年度<br />預算<br />分配數<br />(元)<br />(k)</th>
                        <th style={{ textAlign: "center", width: "5%" }}>本年度<br />預算<br />執行數<br />(元)<br />(l)</th>
                        <th style={{ textAlign: "center", width: "5%" }}>本年度<br />執行率<br />(%)<br />(l/k)</th>
                        <th style={{ textAlign: "center" }}>預算<br />執行率<br />未達<br />80%<br />原因</th>
                        <th style={{ textAlign: "center" }}>責任<br />歸屬</th>
                        <th style={{ textAlign: "center" }}>說明</th>
                    </tr>
                    {projectBudgetExec.length == 0 &&
                        <tr>
                            <td colSpan={16} style={{ textAlign: "center" }}>無資料</td>
                        </tr>
                    }
                    {projectBudgetExec.map(x =>
                        <tr>
                            {periodCell(x)}

                            {formatNumberCell(x.GT_EXPANDED_BUDGET, "n0")}
                            {formatNumberCell(x.GT_ACT_BUDGET + x.GT_AP, "n0")}
                            {formatNumberCell(x.GT_ACT_BUDGET, "n0")}
                            {formatNumberCell(x.GT_AP, "n0")}
                            {formatNumberCell(x.GT_BALANCE, "n0")}
                            {formatNumberCell(x.GT_TOTAL, "n0")}
                            {formatNumberCell(x.GT_EXEC_RATE, "n")}

                            {formatNumberCell(x.YEAR_BUDGET_EXPANDED, "n0")}
                            {formatNumberCell(x.YEAR_BUDGET_ALLOCATED, "n0")}
                            {formatNumberCell(x.YEAR_EXEC_BUDGET, "n0")}
                            {formatNumberCell(x.YEAR_EXEC_RATE, "n")}
                            {failedCell(x.FailedMappingData, ddlData.IPCBGTEXECFAILED)}
                            {failedCell(x.FailedDutyMappingData, ddlData.IPCBGTEXECFAILEDDUTY)}
                            {textAreaWrapCell(x.EXEC_RATE_FAILED_NOTE)}

                            {formatDate(x.CRT_DATE)}
                        </tr>
                    )}
                </table>
            </form>
        </>
    )
}
export default ProjectBudgetExec;
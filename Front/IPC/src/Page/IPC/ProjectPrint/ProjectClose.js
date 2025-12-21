import React from 'react';
import { IsNullOrEmpty, FormatDate } from '../../../Basic/SDOExtension';
import { downProjectAttachment } from '../../../Basic/CommonService';
import { formatNumber } from '@telerik/kendo-intl';

// 三、執行情形（六）結案資料
const ProjectClose = (props) => {
    const { projectClose, title } = props;

    const gridData = [{
        DATA_DATE_SHOW: projectClose.DATA_DATE_SHOW,
        TOTAL_BUDGET: projectClose.TOTAL_BUDGET,
        TOTAL_ACTUAL_COMP: projectClose.TOTAL_ACTUAL_COMP,
        ACTUAL_PAY: projectClose.ACTUAL_PAY,
        UNPAY: projectClose.UNPAY,
        BALANCE: projectClose.BALANCE,
        MDF_DATE: projectClose.MDF_DATE
    }];

    // 數字格式化
    const formatNumberCell = (data) => {
        return (
            <td style={{ textAlign: "right" }}>{formatNumber(data, "n0")}</td>
        )
    }

    // 日期格式化
    const formatDateCell = (data) => {
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

    /**
     * 顯示檔案
     * @returns 
     */
    const showFileCell = () => {
        console.log(projectClose)
        if (projectClose.ProjAttachments.length === 0) {
            return
        }

        return projectClose.ProjAttachments.map(x =>
            <>
                <a href='/' onClick={(e) => {
                    e.preventDefault();
                    downProjectAttachment(x.IDENTITY_FIELD);
                }}>{x.FILE_NAME}</a>
                <br />
            </>
        )
    }

    // 組經費支用情形table
    const buildTable = () => {
        return (
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th style={{ textAlign: "center" }}>月份</th>
                        <th style={{ textAlign: "center" }}>計畫總經費(元)</th>
                        <th style={{ textAlign: "center" }}>累計實際完成金額(元)</th>
                        <th style={{ textAlign: "center" }}>累計實際支用(元)</th>
                        <th style={{ textAlign: "center" }}>應付未付數(元)</th>
                        <th style={{ textAlign: "center" }}>結餘數(元)</th>
                        <th style={{ textAlign: "center" }}>填報日期</th>
                    </tr>
                    {gridData.length == 0 &&
                        <tr>
                            <td colSpan={7} style={{ textAlign: "center" }}>無資料</td>
                        </tr>
                    }
                    {gridData.map(x =>
                        <tr>
                            <td>{x.DATA_DATE_SHOW}</td>
                            {formatNumberCell(x.TOTAL_BUDGET)}
                            {formatNumberCell(x.TOTAL_ACTUAL_COMP)}
                            {formatNumberCell(x.ACTUAL_PAY)}
                            {formatNumberCell(x.UNPAY)}
                            {formatNumberCell(x.BALANCE)}
                            {formatDateCell(x.MDF_DATE)}
                        </tr>
                    )}
                </table>
            </form>
        )
    }

    return (
        <>
            <p>{title}</p>
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th>經費支用情形</th>
                        <td>
                            {buildTable()}
                        </td>
                    </tr>
                    <tr>
                        <th>佐證資料</th>
                        <td>{showFileCell()}</td>
                    </tr>
                </table>
            </form>
            {/* 處理標格與標題之間的距離 */}
            <p className='preview-table-p'></p>
        </>
    );
}

export default ProjectClose;
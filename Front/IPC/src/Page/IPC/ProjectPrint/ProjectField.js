import React from 'react';
import { IsNullOrEmpty, FormatDate } from '../../../Basic/SDOExtension';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';
import { downProjectAttachment } from '../../../Basic/CommonService';

// 三、執行情形（四）實地查證情形
const ProjectField = (props) => {
    const { projectField, title } = props;

    // 多行文字顯示欄位
    const textAreaWrapCell = (data) => {
        return (
            <td>
                <TextAreaWrapInput value={data} />
            </td>
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
     * @param {*} item 
     * @returns 
     */
    const showFileCell = (item) => {
        if (IsNullOrEmpty(item)) {
            return (
                <td></td>
            )
        }
        else {
            return (
                <td>
                    {item.map((file, index) => (
                        <div key={index}>
                            <a href='/' onClick={(e) => {
                                e.preventDefault();
                                downProjectAttachment(file.IDENTITY_FIELD);
                            }}>{file.FILE_NAME}</a>
                        </div>
                    ))}
                </td>
            )
        }
    }

    return (
        <>
            <p>{title}</p>
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th style={{ textAlign: "center", width: "5%" }}>次數</th>
                        <th style={{ textAlign: "center", width: "5%" }}>分數</th>
                        <th style={{ textAlign: "center", width: "10%" }}>查證日期</th>
                        <th style={{ textAlign: "center", width: "10%" }}>回覆期限</th>
                        <th style={{ textAlign: "center" }}>管考說明</th>
                        <th style={{ textAlign: "center" }}>查證紀錄</th>
                        <th style={{ textAlign: "center" }}>執行機關參採情形</th>
                        <th style={{ textAlign: "center" }}>查證參採資料</th>
                        <th style={{ textAlign: "center", width: "10%" }}>填報日期</th>
                    </tr>
                    {projectField.length == 0 &&
                        <tr>
                            <td colSpan={9} style={{ textAlign: "center" }}>無資料</td>
                        </tr>
                    }
                    {projectField.map((x, index) =>
                        <tr>
                            <td style={{ textAlign: "center" }}>{index + 1}</td>
                            <td style={{ textAlign: "right" }}>
                                {IsNullOrEmpty(x.FFSCORE) ? "無" : x.FFSCORE}
                            </td>
                            {formatDateCell(x.FFDATE)}
                            {formatDateCell(x.COMPLETEREPLYDATE)}
                            {textAreaWrapCell(x.FFCOMMENT)}
                            {showFileCell(x.RdecFile)}
                            {textAreaWrapCell(x.FFREPORT)}
                            {showFileCell(x.HandFile)}
                            {formatDateCell(x.FFREPORT_DATE)}
                        </tr>
                    )}
                </table>
            </form>
        </>
    );
}

export default ProjectField;
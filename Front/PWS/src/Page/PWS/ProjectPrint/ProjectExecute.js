import React from 'react';
import { IsNullOrEmpty, FormatDate } from '../../../Basic/SDOExtension';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';

// 三、執行情形（一）每月辦理情形
const ProjectExecute = (props) => {
    const { checkEngineeringProgress, engineeringProgress, title } = props;
    // 是否顯示施工進度欄位
    const [isShow, setIsShow] = React.useState(false);
    const [isDone, setIsDone] = React.useState(false);

    const checkIsShow = () => {
        // 施工方式為"工程類"，且辦理開工的實際完成日期已填寫
        setIsShow(IsNullOrEmpty(checkEngineeringProgress) ? false : checkEngineeringProgress.IsEngStartWork);
        setIsDone(true);
    }

    React.useEffect(() => {
        checkIsShow();
    }, [checkEngineeringProgress])

    // 多行文字顯示欄位
    const textAreaWrapCell = (data) => {
        return (
            <td>
                <TextAreaWrapInput value={data} />
            </td>
        )
    }

    return (
        <>
            <p>{title}</p>
            {isDone &&
                <form>
                    <table style={{ wordBreak: "break-all" }}>
                        <tr>
                            <th rowSpan={isShow ? 2 : 1} style={{ textAlign: "center", width: "5%" }}>期間</th>
                            {isShow &&
                                <>
                                    <th colSpan={2} style={{ textAlign: "center" }}>重大建設系統</th>
                                    <th colSpan={2} style={{ textAlign: "center" }}>標案管理系統</th>
                                </>
                            }
                            <th rowSpan={isShow ? 2 : 1} style={{ textAlign: "center" }}>執行情況</th>
                            <th rowSpan={isShow ? 2 : 1} style={{ textAlign: "center" }}>需協辦事項</th>
                            <th rowSpan={isShow ? 2 : 1} style={{ textAlign: "center", width: "8%" }}>填報日期<br />(逾期天數)</th>
                        </tr>
                        {isShow &&
                            <tr>
                                <th style={{ textAlign: "center", width: "8%" }}>累計預定<br />施工進度</th>
                                <th style={{ textAlign: "center", width: "8%" }}>累計實際<br />施工進度</th>
                                <th style={{ textAlign: "center", width: "8%" }}>累計預定<br />施工進度</th>
                                <th style={{ textAlign: "center", width: "8%" }}>累計實際<br />施工進度</th>
                            </tr>
                        }

                        {engineeringProgress.length == 0 &&
                            <tr>
                                <td colSpan={isShow ? 8 : 4} style={{ textAlign: "center" }}>無資料</td>
                            </tr>
                        }
                        {engineeringProgress.map(x =>
                            <tr>
                                <td style={{ textAlign: "center" }}>{`${x.YEAR}_${x.MONTH}`}</td>
                                {isShow &&
                                    <>
                                        <td style={{ textAlign: "right" }}>{x.IPC_RES_PRG}</td>
                                        <td style={{ textAlign: "right" }}>{x.IPC_ACT_PRG}</td>
                                        <td style={{ textAlign: "right" }}>{x.TEN_RES_PRG}</td>
                                        <td style={{ textAlign: "right" }}>{x.TEN_ACT_PRG}</td>
                                    </>
                                }
                                {textAreaWrapCell(x.EXECUTE_CONDITION)}
                                {textAreaWrapCell(x.ASSISTANT_ITEM)}
                                <td>
                                    {IsNullOrEmpty(x.SEND_DATE) ? null : FormatDate(x.SEND_DATE, 'tYY/MM/DD')}
                                    {IsNullOrEmpty(x.SEND_DATE) ? null : <br />}
                                    ({x.DISREGARD ? '不計算' : x.OVERDUE_DAY == 0 ? '無' : x.OVERDUE_DAY})
                                </td>
                            </tr>
                        )}
                    </table>
                </form>
            }
        </>
    );
}

export default ProjectExecute;
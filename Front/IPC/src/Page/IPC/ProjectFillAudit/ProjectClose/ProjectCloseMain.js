import React from 'react';
import TextAreaInput from '../../../../Components/Input/TextAreaInput';
import NumericTextInput from '../../../../Components/Input/NumericTextInput';
import ProjectCloseDetailsGrid from './ProjectCloseDetailsGrid';
import ProjectFillAuditService from '../ProjectFillAuditService';
import { IsNullOrEmpty, FormatDate } from '../../../../Basic/SDOExtension';
import { Error } from '@progress/kendo-react-labels';

// 年終考核意見
export const ProjectCloseMain = (props) => {
    const {
        projectNo,
        data,
        projectBasicSaveData,
        editedGridData,
        totalScoreSaveData,
        isRdecFun } = props;

    const [projectBasicData, setProjectBasicData] = React.useState(data.ProjectBasic);
    const [projectCloseDetailsData, setProjectCloseDetailsData] = React.useState([]);
    const [errors, setErrors] = React.useState({});
    // 只記錄分案資料
    const [type5Data, setType5Data] = React.useState([]);

    // 紀錄次數
    const [cnt1, setCnt1] = React.useState(0);
    const [cnt2, setCnt2] = React.useState(0);
    const [cnt3, setCnt3] = React.useState(0);
    const [cnt4, setCnt4] = React.useState(0);
    const [cnt5, setCnt5] = React.useState(0);
    // 合計扣減
    const [totalScore, setTotalScore] = React.useState(0);

    React.useEffect(() => {
        setProjectBasicData(data.ProjectBasic);
        setProjectCloseDetailsData(data.ProjectCloseDetails);
        setCnt3(data.DelayApply.length);
        let type5 = data.ProjectMergeLog.filter(x => x.MERGE_STATUS == "01");
        setType5Data(type5);
        setCnt5(type5.length);
        editedGridData.current = [];
    }, [data])

    // 計算合計扣減
    React.useEffect(() => {
        let sum = (cnt2 > 0 ? 5 : 0) + (cnt3 * 5) + (cnt4 * 3) + (cnt5 > 0 ? 3 : 0);
        setTotalScore(sum);
        totalScoreSaveData.current = sum;
    }, [cnt2, cnt3, cnt4, cnt5])

    // 日期格式化
    const formatDate = (item) => {
        if (IsNullOrEmpty(item)) {
            return "";
        }
        else {
            return FormatDate(item, "tYY/MM/DD");
        }
    }

    //驗證
    const validate = (validateField) => {
        let data = {};
        validateField.validate(projectBasicData, { abortEarly: false })
            .then((v) => {
                if (v) {
                    setErrors({});
                }
            }).catch(function (err) {
                err.inner.map(e => data[e.path] = e.message);
                setErrors({ ...data });
            });
    }

    return (
        <>
            <form>
                <table>
                    <tbody>
                        <tr>
                            <th>結案/撤銷日期</th>
                            <td colSpan={2}>
                                {IsNullOrEmpty(projectBasicData.FINISH_DATE)
                                    ? ""
                                    : formatDate(projectBasicData.FINISH_DATE) + projectBasicData.PROJECT_STATUS
                                }
                            </td>
                        </tr>
                        <tr>
                            <th>基本資料</th>
                            <td colSpan={2}>
                                <div style={{ display: "flex", alignItems: "center" }}>
                                    <NumericTextInput
                                        name="SCORE_A"
                                        min={0}
                                        max={100}
                                        IsForceMax={true}
                                        inputType={'text'}
                                        width="100px"
                                        value={projectBasicData.SCORE_A}
                                        onBlur={(e) => {
                                            let newProjectBasic = { ...projectBasicData, SCORE_A: e.target.value };
                                            setProjectBasicData(newProjectBasic);
                                            projectBasicSaveData.current = newProjectBasic;
                                            validate(ProjectFillAuditService.validateProjectBasicField)
                                        }}
                                    /> 分
                                </div>
                                {<Error>{errors["SCORE_A"]}</Error>}
                            </td>
                        </tr>
                        <tr>
                            <th>報表品質</th>
                            <td style={{ width: "30%" }}>
                                各項報表資料內容欠周詳，內容過於簡略，經智發會通知改善{cnt1}次。
                            </td>
                            <td>
                                <ProjectCloseDetailsGrid
                                    projectNo={projectNo}
                                    type={"1"}
                                    data={projectCloseDetailsData.filter(x => x.CLOSE_DETAILS_TYPE == "1")}
                                    projectCloseDetailsData={projectCloseDetailsData}
                                    setProjectCloseDetailsData={setProjectCloseDetailsData}
                                    editedGridData={editedGridData}
                                    setCnt={setCnt1}
                                    isRdecFun={isRdecFun}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th rowSpan={5}>特殊扣分</th>
                            <td>
                                符合列管標準，卻未依第4點規定，主動提報智發會列管，考核分數再扣減5分。
                            </td>
                            <td>
                                <ProjectCloseDetailsGrid
                                    projectNo={projectNo}
                                    type={"2"}
                                    data={projectCloseDetailsData.filter(x => x.CLOSE_DETAILS_TYPE == "2")}
                                    projectCloseDetailsData={projectCloseDetailsData}
                                    setProjectCloseDetailsData={setProjectCloseDetailsData}
                                    editedGridData={editedGridData}
                                    setCnt={setCnt2}
                                    isRdecFun={isRdecFun}
                                />
                            </td>
                        </tr>
                        <tr>
                            <td>申請計畫調整，未依第9點規定於期限內提出，按次扣減考核分數5分。共{cnt3}次，扣減{5 * cnt3}分。</td>
                            <td>
                                {cnt3 == 0 ? "" :
                                    <>
                                        未於期限內提出之期程調整：
                                        {data.DelayApply.map(x => {
                                            return (
                                                <div>{formatDate(x.APPRV_DATE) + x.SCHE_TYPE}</div>
                                            )
                                        })}
                                    </>
                                }
                            </td>
                        </tr>
                        <tr>
                            <td>經查證填報不實者，按次扣減該計畫年終考核分數3分。共{cnt4}次，扣減{3 * cnt4}分。</td>
                            <td>
                                <ProjectCloseDetailsGrid
                                    projectNo={projectNo}
                                    type={"4"}
                                    data={projectCloseDetailsData.filter(x => x.CLOSE_DETAILS_TYPE == "4")}
                                    projectCloseDetailsData={projectCloseDetailsData}
                                    setProjectCloseDetailsData={setProjectCloseDetailsData}
                                    editedGridData={editedGridData}
                                    setCnt={setCnt4}
                                    isRdecFun={isRdecFun}
                                />
                            </td>
                        </tr>
                        <tr>
                            <td>計畫於管考期間，申請分案列管，考核分數再扣減3分。</td>
                            <td>
                                {cnt5 == 0 ? "" :
                                    <>
                                        {type5Data.map(x => {
                                            return (
                                                <div>{formatDate(x.PROMERGE_DATE) + "簽准分案"}</div>
                                            )
                                        })}
                                    </>
                                }
                            </td>
                        </tr>
                        <tr>
                            <td>合計扣減</td>
                            <td>{totalScore}分</td>
                        </tr>
                        <tr>
                            <th>年終考核備註</th>
                            <td colSpan={2}>
                                <TextAreaInput
                                    name="NOTES_FOR_BUDGET"
                                    rows={5}
                                    maxlength={500}
                                    style={{ width: "100%" }}
                                    onBlur={(e) => {
                                        let newProjectBasic = { ...projectBasicData, NOTES_FOR_BUDGET: e.target.element.current.value };
                                        setProjectBasicData(newProjectBasic);
                                        projectBasicSaveData.current = newProjectBasic;
                                    }}
                                    defaultValue={projectBasicData.NOTES_FOR_BUDGET == null ? "" : projectBasicData.NOTES_FOR_BUDGET}
                                />
                            </td>
                        </tr>
                    </tbody>
                </table>
            </form>
        </>
    )
}
export default ProjectCloseMain;
import React from 'react';
import { RadioGroup } from "@progress/kendo-react-inputs";
import TwDatePicker from "../../../../Components/DateInputs/TwDatePicker";
import ProjectReviewGrid from './ProjectReviewGrid';

export const ProjectReviewMain = (props) => {
    const { projectNo, paramData, projectReviewData, setProjectReviewData, projectReviewGridData, setProjectReviewGridData, editedGridData } = props;

    // 組表格
    const rowsBuilder = () => {
        let result = paramData.map((x, index) => {
            return (
                <>
                    {projectReviewData.filter(y => y.REVIEW_KIND == x.SET_TYPE).map((y) => {
                        return (
                            <>
                                <tr>
                                    <th style={{ width: "18%" }}>{x.SET_VALUE}</th>
                                    <td style={{ width: "18%" }}>
                                        <RadioGroup
                                            name={"IS_REVIEW" + index}
                                            value={y.IS_REVIEW}
                                            data={[
                                                { label: "無", value: false },
                                                { label: "有", value: true }
                                            ]}
                                            layout={"horizontal"}
                                            onChange={(e) => {
                                                projectReviewData.splice(index, 1,
                                                    {
                                                        ...projectReviewData[index],
                                                        IS_REVIEW: e.value,
                                                        SEND_DATE: !e.value ? null : projectReviewData[index].SEND_DATE,
                                                        REVIEW_DATE: !e.value ? null : projectReviewData[index].REVIEW_DATE,
                                                    }
                                                );
                                                setProjectReviewData([...projectReviewData]);
                                            }}
                                        />
                                    </td>
                                    <th className={y.IS_REVIEW ? "addRedStar" : ""}>送件日期</th>
                                    <td>
                                        <TwDatePicker
                                            name={"SEND_DATE" + index}
                                            format={"yyy/MM/dd"}
                                            onChange={(e) => {
                                                projectReviewData.splice(index, 1, { ...projectReviewData[index], SEND_DATE: e.value });
                                                setProjectReviewData([...projectReviewData]);
                                            }}
                                            value={y.SEND_DATE == null ? null : new Date(y.SEND_DATE)}
                                        />
                                    </td>
                                    <th>核定日期</th>
                                    <td>
                                        <TwDatePicker
                                            name={"REVIEW_DATE" + index}
                                            format={"yyy/MM/dd"}
                                            onChange={(e) => {
                                                projectReviewData.splice(index, 1, { ...projectReviewData[index], REVIEW_DATE: e.value });
                                                setProjectReviewData([...projectReviewData]);
                                            }}
                                            value={y.REVIEW_DATE == null ? null : new Date(y.REVIEW_DATE)}
                                        />
                                    </td>
                                </tr>
                            </>
                        )
                    })}
                    {x.SET_TYPE == "04" &&
                        <tr>
                            <th>{x.SET_VALUE}</th>
                            <td colSpan={5}>
                                <ProjectReviewGrid
                                    projectNo={projectNo}
                                    gridData={projectReviewGridData}
                                    setGridData={setProjectReviewGridData}
                                    editedGridData={editedGridData}
                                />
                            </td>
                        </tr>
                    }
                </>
            )
        });
        return result;
    }

    return (
        <>
            <form >
                <table>
                    <tbody>
                        {rowsBuilder()}
                    </tbody>
                </table>
            </form>
        </>
    )
}
export default ProjectReviewMain;
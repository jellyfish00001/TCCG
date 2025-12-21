import React from 'react';
import TwDatePicker from "../../../../Components/DateInputs/TwDatePicker";
import { RadioGroup } from "@progress/kendo-react-inputs";
import ProjectActivityGrid from './ProjectActivityGrid';

export const ProjectActivityTable = (props) => {
    const { projectNo, activityData, setActivityData, activityGridData, setActivityGridData, editedGridData } = props;

    // 組表格
    const rowsBuilder = () => {
        let result = activityData.map((x, index) => {
            return (
                <>
                    <tr>
                        <th>{x.ACTIVITY_KIND_NAME}</th>
                        {x.ACTIVITY_KIND != "04" ?
                            <>
                                <td>
                                    <RadioGroup
                                        name={"IS_ACTIVITY" + index}
                                        value={x.IS_ACTIVITY}
                                        data={[
                                            { label: "無", value: false },
                                            { label: "有", value: true }
                                        ]}
                                        layout={"horizontal"}
                                        onChange={(e) => {
                                            activityData.splice(index, 1,
                                                {
                                                    ...activityData[index],
                                                    IS_ACTIVITY: e.value,
                                                    ACTIVITY_DATE: !e.value ? null : activityData[index].ACTIVITY_DATE,
                                                    ACTIVITY_NAME: !e.value ? "" : activityData[index].ACTIVITY_NAME,
                                                }
                                            );
                                            setActivityData([...activityData]);
                                        }}
                                    />
                                </td>
                                <th className={x.IS_ACTIVITY ? "addRedStar" : ""}>活動日期</th>
                                <td >
                                    <TwDatePicker
                                        name={"ACTIVITY_DATE" + index}
                                        format={"yyy/MM/dd"}
                                        onChange={(e) => {
                                            activityData.splice(index, 1, { ...activityData[index], ACTIVITY_DATE: e.value });
                                            setActivityData([...activityData]);
                                        }}
                                        value={x.ACTIVITY_DATE == null ? null : new Date(x.ACTIVITY_DATE)}
                                    />
                                </td>
                            </>
                            :
                            <td colSpan={3}>
                                <ProjectActivityGrid
                                    projectNo={projectNo}
                                    gridData={activityGridData}
                                    setGridData={setActivityGridData}
                                    editedGridData={editedGridData}
                                />
                            </td>
                        }
                    </tr>
                </>
            )
        })
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
export default ProjectActivityTable;
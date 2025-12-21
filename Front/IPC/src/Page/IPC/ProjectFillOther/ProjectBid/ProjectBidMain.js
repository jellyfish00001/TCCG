import React from 'react';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import TwDatePicker from "../../../../Components/DateInputs/TwDatePicker";
import TextInput from '../../../../Components/Input/TextInput';
import ProjectBidGrid from './ProjectBidGrid';

export const ProjectBidMain = (props) => {
    const { projectNo, projectBidData, setProjectBidData, projectBidDetailData, setProjectBidDetailData, editedGridData } = props;

    // 組表格
    const tableBuilder = () => {
        let result = projectBidData.map((x, index) => {
            let detailType0Data = projectBidDetailData.filter(y => y.BID_KIND == x.BID_KIND && y.DETAIL_TYPE == 0);
            let detailType1Data = projectBidDetailData.filter(y => y.BID_KIND == x.BID_KIND && y.DETAIL_TYPE == 1);
            return (
                <table>
                    <tbody>
                        <tr>
                            <th className='table-header' colSpan={2}>{x.BID_NAME}</th>
                        </tr>
                        <tr>
                            <th>流標歷程</th>
                            <td>
                                <ProjectBidGrid
                                    projectNo={projectNo}
                                    bidKind={x.BID_KIND}
                                    detailType={0}
                                    initData={detailType0Data}
                                    projectBidDetailData={projectBidDetailData}
                                    setProjectBidDetailData={setProjectBidDetailData}
                                    editedGridData={editedGridData}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th>廢標歷程</th>
                            <td>
                                <ProjectBidGrid
                                    projectNo={projectNo}
                                    bidKind={x.BID_KIND}
                                    detailType={1}
                                    initData={detailType1Data}
                                    projectBidDetailData={projectBidDetailData}
                                    setProjectBidDetailData={setProjectBidDetailData}
                                    editedGridData={editedGridData}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th>決標日期</th>
                            <td colSpan={2}>
                                <TwDatePicker
                                    name={"AWARD_BID_DATE" + index}
                                    format={"yyy/MM/dd"}
                                    onChange={(e) => {
                                        projectBidData.splice(index, 1, { ...projectBidData[index], AWARD_BID_DATE: e.value });
                                        setProjectBidData([...projectBidData]);
                                    }}
                                    value={x.AWARD_BID_DATE == null ? null : new Date(x.AWARD_BID_DATE)}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th className={IsNullOrEmpty(x.AWARD_BID_DATE) ? "" : "addRedStar"}>決標廠商</th>
                            <td colSpan={2}>
                                <TextInput
                                    name={"BID_TENDER" + index}
                                    maxlength={50}
                                    value={x.BID_TENDER == null ? "" : x.BID_TENDER}
                                    onChange={(e) => {
                                        projectBidData.splice(index, 1, { ...projectBidData[index], BID_TENDER: e.value });
                                        setProjectBidData([...projectBidData]);
                                    }}
                                    style={{ width: "100%" }}
                                />
                            </td>
                        </tr>
                    </tbody>
                </table>
            )
        });
        return result;
    }

    return (
        <>
            <form>
                {tableBuilder()}
            </form>
        </>
    )
}
export default ProjectBidMain;
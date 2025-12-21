import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import ProjectBidMain from './ProjectBid/ProjectBidMain';
import ProjectActivityTable from './ProjectActivity/ProjectActivityTable';
import ProjectReviewMain from './ProjectReview/ProjectReviewMain';
import ProjectTenderGrid from './ProjectTender/ProjectTenderGrid';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { SetMaskOnOff, FormatDate } from '../../../Basic/SDOExtension';
import { GetSetParam, openProjectPrint } from "../../../Basic/CommonService";
import ProjectFillOtherService from './ProjectFillOtherService';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";

export const ProjectFillOtherMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            state: {
                projectNo,
                showSomeBtn,
            } = {
                projectNo: "",
                showSomeBtn: null
            }
        }
    } = props;

    // 招標情形資料
    const [projectBidData, setProjectBidData] = React.useState([]);
    const [projectBidDetailData, setProjectBidDetailData] = React.useState([]);
    // 相關活動
    const [activityData, setActivityData] = React.useState([]);
    const [activityGridData, setActivityGridData] = React.useState([]);
    // 相關審查
    const [paramData, setParamData] = React.useState([]);
    const [projectReviewData, setProjectReviewData] = React.useState([]);
    const [projectReviewGridData, setProjectReviewGridData] = React.useState([]);
    // 廠商資訊
    const [projectTenderData, setProjectTenderData] = React.useState([]);
    const [tenderKindDdlData, setTenderKindDdlData] = React.useState([]);

    // 紀錄 招標情形Grid 異動資料
    const editedBidGridData = React.useRef([]);
    // 紀錄 相關活動Grid 異動資料
    const editedActivityGridData = React.useRef([]);
    // 紀錄 相關審查Grid 異動資料
    const editedReviewGridData = React.useRef([]);
    // 紀錄 廠商資訊Grid 異動資料
    const editedTenderGridData = React.useRef([]);

    // 載入廠商類別下拉資料
    const loadTenderKind = async () => {
        let data = await GetSetParam("TENDER_KIND");
        setTenderKindDdlData(data);
    }

    // 載入其他資料
    const loadProjectFillOther = async () => {
        let result = await ProjectFillOtherService.getProjectFillOther(projectNo);
        setProjectBidData(result.ProjectBid);
        setProjectBidDetailData(result.ProjectBidDetail);
        setActivityData(result.ProjectActivity.slice(0, 4));
        setActivityGridData(result.ProjectActivity.filter(x => x.ACTIVITY_KIND == "04" && x.editType != 0));
        setParamData(result.SetParam);
        setProjectReviewData(result.ProjectReview.filter(x => x.REVIEW_KIND != "04"));
        setProjectReviewGridData(result.ProjectReview.filter(x => x.REVIEW_KIND == "04"));
        setProjectTenderData(result.ProjectTender);
    }

    const loadData = async () => {
        SetMaskOnOff(true);
        await loadTenderKind();
        await loadProjectFillOther();
        editedBidGridData.current = [];
        editedActivityGridData.current = [];
        editedReviewGridData.current = [];
        editedTenderGridData.current = [];
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, [])

    // 驗證
    const CheckIsValid = async () => {
        let notValids = [];
        // 招標情形
        for (let index = 0; index < projectBidData.length; index++) {
            const item = projectBidData[index];
            let isValid = await ProjectFillOtherService.validateBidField.isValid(item);
            if (!isValid) {
                notValids.push({ type: "Bid", key: index, value: isValid, item: item });
            }
        }
        // 招標情形Grid
        for (let index = 0; index < projectBidDetailData.length; index++) {
            const item = projectBidDetailData[index];
            //刪除不用驗證
            if (item.editType !== 3) {
                let isValid = await ProjectFillOtherService.validateBidGridField.isValid(item);
                if (!isValid) {
                    notValids.push({ type: "BidGrid", key: index, value: isValid, item: item });
                }
            }
        }
        // 相關活動
        for (let index = 0; index < activityData.length; index++) {
            const item = activityData[index];
            let isValid = await ProjectFillOtherService.validateActivityField.isValid(item);
            if (!isValid) {
                notValids.push({ type: "activity", key: index, value: isValid, item: item });
            }
        }
        // 相關活動Grid
        for (let index = 0; index < activityGridData.length; index++) {
            const item = activityGridData[index]
            //刪除不用驗證
            if (item.editType !== 3) {
                let isValid = await ProjectFillOtherService.validateActivityGridField.isValid(item);
                if (!isValid) {
                    notValids.push({ type: "ActivityGrid", key: index, value: isValid, item: item });
                }
            }
        }
        // 相關審查
        for (let index = 0; index < projectReviewData.length; index++) {
            const item = projectReviewData[index];
            let isValid = await ProjectFillOtherService.validateReviewField.isValid(item);
            if (!isValid) {
                notValids.push({ type: "Review", key: index, value: isValid, item: item });
            }
        }
        // 相關審查Grid
        for (let index = 0; index < projectReviewGridData.length; index++) {
            const item = projectReviewGridData[index]
            //刪除不用驗證
            if (item.editType !== 3) {
                let isValid = await ProjectFillOtherService.validateReviewGridField.isValid(item);
                if (!isValid) {
                    notValids.push({ type: "ReviewGrid", key: index, value: isValid, item: item });
                }
            }
        }
        // 廠商資訊
        for (let index = 0; index < projectTenderData.length; index++) {
            const item = projectTenderData[index];
            let isValid = await ProjectFillOtherService.validateTenderField.isValid(item);
            if (!isValid) {
                notValids.push({ type: "tender", key: index, value: isValid, item: item });
            }
        }
        return notValids.length == 0;
    }

    // 組存檔資料
    const mapSaveData = () => {
        let newActivityData = [...activityData.slice(0, 3), ...editedActivityGridData.current];
        let newReviewData = [...projectReviewData, ...editedReviewGridData.current];

        let model = {
            ProjectBid: projectBidData.map(x => {
                return {
                    ...x,
                    AWARD_BID_DATE: x.AWARD_BID_DATE == null ? null : FormatDate(x.AWARD_BID_DATE, 'YYYY-MM-DD'),
                }
            }),
            ProjectBidDetail: editedBidGridData.current.map(x => {
                return {
                    ...x,
                    DETAIL_DATE: x.DETAIL_DATE == null ? null : FormatDate(x.DETAIL_DATE, 'YYYY-MM-DD'),
                }
            }),
            ProjectActivity: newActivityData.map(x => {
                return {
                    ...x,
                    ACTIVITY_DATE: x.ACTIVITY_DATE == null ? null : FormatDate(x.ACTIVITY_DATE, 'YYYY-MM-DD'),
                }
            }),
            ProjectReview: newReviewData.map(x => {
                return {
                    ...x,
                    SEND_DATE: x.SEND_DATE == null ? null : FormatDate(x.SEND_DATE, 'YYYY-MM-DD'),
                    REVIEW_DATE: x.REVIEW_DATE == null ? null : FormatDate(x.REVIEW_DATE, 'YYYY-MM-DD'),
                }
            }),
            ProjectTender: editedTenderGridData.current
        }
        return model;
    }

    // 存檔
    const save = async () => {
        let isValid = await CheckIsValid();
        if (!isValid) {
            showGlobalMessageBox("請確認資料是否填妥");
        }
        else {
            let saveData = mapSaveData();
            SetMaskOnOff(true);
            let saveResult = await ProjectFillOtherService.saveProjectFillOther(saveData);
            SetMaskOnOff(false);
            if (saveResult.success) {
                showGlobalMessageBox(saveResult.message, () => {
                    window.location.reload();
                });
            }
            else {
                showGlobalMessageBox(saveResult.message);
            }
        }
    }

    return (
        <>
            <CollapseBoardCard
                button={
                    <>
                        {
                            showSomeBtn &&
                            <>
                                <Button title="存檔" onClick={save} >存檔</Button>
                                <Button title="取消" className="k-button-lighten" onClick={loadData}>取消</Button>
                            </>
                        }
                        <Button title="預覽列印" className="k-button-lighten" onClick={() => openProjectPrint(state)}>預覽列印</Button>
                    </>
                }
                title="招標情形"
                isFirstArea={true}
            >
                <ProjectBidMain
                    projectNo={projectNo}
                    projectBidData={projectBidData}
                    setProjectBidData={setProjectBidData}
                    projectBidDetailData={projectBidDetailData}
                    setProjectBidDetailData={setProjectBidDetailData}
                    editedGridData={editedBidGridData}
                />
            </CollapseBoardCard>

            <CollapseBoardCard title="相關活動" initValue={false}>
                <ProjectActivityTable
                    projectNo={projectNo}
                    activityData={activityData}
                    setActivityData={setActivityData}
                    activityGridData={activityGridData}
                    setActivityGridData={setActivityGridData}
                    editedGridData={editedActivityGridData}
                />
            </CollapseBoardCard>

            <CollapseBoardCard title="相關審查" initValue={false}>
                <ProjectReviewMain
                    projectNo={projectNo}
                    paramData={paramData}
                    setParamData={setParamData}
                    projectReviewData={projectReviewData}
                    setProjectReviewData={setProjectReviewData}
                    projectReviewGridData={projectReviewGridData}
                    setProjectReviewGridData={setProjectReviewGridData}
                    editedGridData={editedReviewGridData}
                />
            </CollapseBoardCard>

            <CollapseBoardCard title="廠商資訊" initValue={false}>
                <ProjectTenderGrid
                    projectNo={projectNo}
                    gridData={projectTenderData}
                    setGridData={setProjectTenderData}
                    tenderKindDdlData={tenderKindDdlData}
                    editedGridData={editedTenderGridData}
                />
            </CollapseBoardCard>

        </>
    )
}
export default ProjectFillOtherMain;
import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty, SetMaskOnOff, FormatDate } from '../../../Basic/SDOExtension';
import ProjectFillAuditService from './ProjectFillAuditService';
import ProjectAuditMain from './ProjectAudit/ProjectAuditMain';
import ProjectFactFindingGrid from './ProjectFactFinding/ProjectFactFindingGrid';
import ProjectCloseMain from './ProjectClose/ProjectCloseMain';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import TextAreaInput from '../../../Components/Input/TextAreaInput';
import { openProjectPrint } from '../../../Basic/CommonService';

export const ProjectFillAuditMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            state: {
                projectNo,
                isRdecFun,
                showSomeBtn
            } = {
                projectNo: "",
                isRdecFun: "",
                showSomeBtn
            }
        }
    } = props;

    // 管考備註所有資料
    const [projectFillAuditData, setProjectFillAuditData] = React.useState(ProjectFillAuditService.initData);
    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState({
        SPEC_NOTE: [],
        COM_CONFERENCEGENRE: [],
        PROMERGESTATUS: [],
        COM_IPCMEMO: [],
        PLAN_YEAR: []
    });
    const specNoteDefaultDdlData = React.useRef([]);
    // 紀錄 特殊加註 已選擇資料
    const [specNoteSelData, setSpecNoteSelData] = React.useState([]);
    // 其他管考備註 資料
    const [noteForSchedule, setNoteForSchedule] = React.useState({});

    // 紀錄 平時管考 異動資料
    const editAuditOpinionGridData = React.useRef([]);
    // 紀錄 列管會議 異動資料
    const editConferenceGridData = React.useRef([]);
    // 紀錄 資料逾期繳交或填報 異動資料
    const editDelayFillGridData = React.useRef([]);
    // 紀錄 計畫分併案記錄檔 異動資料
    const editedMergeLogGridData = React.useRef([]);
    // 紀錄 實地查證 異動資料
    const editedFactFindGridData = React.useRef([]);
    // 紀錄 計畫結案明細 異動資料
    const editedCloseDetailsGridData = React.useRef([]);
    // 紀錄 合計扣減
    const totalScoreSaveData = React.useRef(0);
    // 紀錄 計畫主檔 資料
    const projectBasicSaveData = React.useRef({});

    const loadData = async () => {
        await getDdlData();
        await loadProjectFillAudit();
    }
    // 取得下拉選單
    const getDdlData = async () => {
        let dropDowns = await ProjectFillAuditService.getAllDropDowns();
        if (dropDowns.length > 0) {
            setDdlData({
                // 特殊加註
                SPEC_NOTE: [...dropDowns[0]],
                // 會議種類
                COM_CONFERENCEGENRE: [...dropDowns[1]],
                // 分案或併案
                PROMERGESTATUS: [...dropDowns[2]],
                // 管考備註項目
                COM_IPCMEMO: [...dropDowns[3]],
                // 年度
                PLAN_YEAR: [...dropDowns[4]],
            });
            specNoteDefaultDdlData.current = [...dropDowns[0]];
        }
    }

    // 載入管考備註三區塊資料
    const loadProjectFillAudit = async () => {
        SetMaskOnOff(true);
        let result = JSON.parse(JSON.stringify(ProjectFillAuditService.initData));
        result = await ProjectFillAuditService.getProjectFillAudit(projectNo);
        projectBasicSaveData.current = result.ProjectBasic;
        totalScoreSaveData.current = result.ProjectCloseMemo.TOTAL_SCORE;
        setNoteForSchedule(result.ProjectBasic.NOTES_FOR_SCHEDULE);
        // 平時管考 特殊加註已選擇資料
        let selData = result.SpecNoteMappingData;
        let selectedData = specNoteDefaultDdlData.current.filter(i => {
            if (selData.map(x => x.SET_TYPE).includes(i.SET_TYPE)) {
                return i;
            }
        });
        setSpecNoteSelData([...selectedData]);
        setProjectFillAuditData(result);
        editAuditOpinionGridData.current = [];
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, [])

    //驗證
    const CheckIsValid = async () => {
        let notValids = [];
        // 列管會議
        for (let index = 0; index < editConferenceGridData.current.length; index++) {
            const item = editConferenceGridData.current[index];
            //刪除不用驗證
            if (item.editType !== 3) {
                let isValid = await ProjectFillAuditService.validateConferenceField.isValid(item);
                if (!isValid) {
                    notValids.push({ type: "Conference", key: index, value: isValid, item: item });
                }
            }
        }
        // 資料逾期繳交或填報
        for (let index = 0; index < editDelayFillGridData.current.length; index++) {
            const item = editDelayFillGridData.current[index];
            //刪除不用驗證
            if (item.editType !== 3) {
                let isValid = await ProjectFillAuditService.validateDelayfillField.isValid(item);
                if (!isValid) {
                    notValids.push({ type: "DelayFill", key: index, value: isValid, item: item });
                }
            }
        }
        // 分案／併案
        for (let index = 0; index < editedMergeLogGridData.current.length; index++) {
            const item = editedMergeLogGridData.current[index];
            //刪除不用驗證
            if (item.editType !== 3) {
                let isValid = await ProjectFillAuditService.validateMergeLogField.isValid(item);
                if (!isValid) {
                    notValids.push({ type: "MergeLog", key: index, value: isValid, item: item });
                }
                // 判斷有無上傳檔案
                if (!IsNullOrEmpty(item.FILE)) {
                    let haveNewFile = item.FILE.filter(x => x.editType == 1);
                    if (IsNullOrEmpty(haveNewFile)) {
                        let haveDeleteFile = item.FILE.filter(x => x.editType == 3);
                        //如果刪除檔案數等於原有的檔案
                        if (haveDeleteFile && haveDeleteFile.length == item.File.length) {
                            notValids.push({ type: "MergeLog", key: index, value: isValid, item: "無上傳檔案" });
                        }
                    }
                }
            }
        }
        // 實地查證情形
        for (let index = 0; index < editedFactFindGridData.current.length; index++) {
            const item = editedFactFindGridData.current[index];
            //刪除不用驗證
            if (item.editType !== 3) {
                let isValid = await ProjectFillAuditService.validateFactFindingField.isValid(item);
                if (!isValid) {
                    notValids.push({ type: "FactFinding", key: index, value: isValid, item: item });
                }
                // 判斷有無上傳檔案
                if (!IsNullOrEmpty(item.FILE)) {
                    let haveNewFile = item.FILE.filter(x => x.editType == 1);
                    if (IsNullOrEmpty(haveNewFile)) {
                        let haveDeleteFile = item.FILE.filter(x => x.editType == 3);
                        if (haveDeleteFile && haveDeleteFile.length == item.RdecFile.length) {
                            notValids.push({ type: "FactFinding", key: index, value: isValid, item: "無上傳檔案" });
                        }
                    }
                }
            }
        }

        // 結案明細資料
        for (let index = 0; index < editedCloseDetailsGridData.current.length; index++) {
            const item = editedCloseDetailsGridData.current[index];
            //刪除不用驗證
            if (item.editType !== 3) {
                let isValid = await ProjectFillAuditService.validateCloseDetailsField.isValid(item);
                if (!isValid) {
                    notValids.push({ type: "CloseDetails", key: index, value: isValid, item: item });
                }
            }
        }
        // 計畫基本資料
        let isValid = await ProjectFillAuditService.validateProjectBasicField.isValid(projectBasicSaveData.current);
        if (!isValid) {
            notValids.push({ type: "Basic", value: isValid, item: projectBasicSaveData.current });
        }
        return notValids.length == 0;
    }

    //存檔資料format日期欄位
    const formatSaveData = () => {
        let saveData = {
            ProjectBasic: { ...projectBasicSaveData.current, NOTES_FOR_SCHEDULE: noteForSchedule },
            ProjectEngineeringAuditOpinion: editAuditOpinionGridData.current,
            ProjectConference: editConferenceGridData.current.map(x => {
                return {
                    ...x,
                    CONFERENCE_TIME: FormatDate(x.CONFERENCE_TIME, 'YYYY-MM-DD')
                }
            }),
            ProjectDelayfill: editDelayFillGridData.current.map(x => {
                return {
                    ...x,
                    FILL_TIME: FormatDate(x.FILL_TIME, 'YYYY-MM-DD')
                }
            }),
            ProjectMergeLog: editedMergeLogGridData.current.map(x => {
                return {
                    ...x,
                    PROMERGE_DATE: FormatDate(x.PROMERGE_DATE, 'YYYY-MM-DD')
                }
            }),
            SpecNoteMappingData: specNoteSelData,
            ProjectFactFinding: editedFactFindGridData.current.map(x => {
                return {
                    ...x,
                    RdecFile: x.FILE,
                    FFDATE: FormatDate(x.FFDATE, 'YYYY-MM-DD'),
                    COMPLETEREPLYDATE: x.COMPLETEREPLYDATE == null ? null : FormatDate(x.COMPLETEREPLYDATE, 'YYYY-MM-DD'),
                    FileKind: "07"
                }
            }),
            ProjectCloseDetails: editedCloseDetailsGridData.current.map(x => {
                return {
                    ...x,
                    REF_DATE: FormatDate(x.REF_DATE, 'YYYY-MM-DD')
                }
            }),
            ProjectCloseMemo: { ...projectFillAuditData.ProjectCloseMemo, TOTAL_SCORE: totalScoreSaveData.current },
        }
        return saveData;
    }

    // 存檔
    const save = async () => {
        let isValid = await CheckIsValid();
        if (!isValid) {
            showGlobalMessageBox("請確認資料是否填妥及檔案是否上傳");
        }
        else {
            let saveData = formatSaveData();
            SetMaskOnOff(true);
            let saveResult = await ProjectFillAuditService.saveProjectFillAudit(saveData);
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
                                {isRdecFun && <Button title="存檔" onClick={save} >存檔</Button>}
                                <Button title="取消" className="k-button-lighten" onClick={loadProjectFillAudit}>取消</Button>
                            </>
                        }
                        <Button title="預覽列印" className="k-button-lighten" onClick={() => openProjectPrint(state)}>預覽列印</Button>
                    </>
                }
                title="平時管考意見"
                isFirstArea={true}>
                <ProjectAuditMain
                    projectNo={projectNo}
                    projectFillAuditData={projectFillAuditData}
                    ddlData={ddlData}
                    specNoteSelData={specNoteSelData}
                    setSpecNoteSelData={setSpecNoteSelData}
                    editAuditOpinionGridData={editAuditOpinionGridData}
                    editConferenceGridData={editConferenceGridData}
                    editDelayFillGridData={editDelayFillGridData}
                    editedMergeLogGridData={editedMergeLogGridData}
                    isRdecFun={isRdecFun}
                />
            </CollapseBoardCard>

            <CollapseBoardCard title="實地查證意見" initValue={false}>
                <ProjectFactFindingGrid
                    projectNo={projectNo}
                    data={projectFillAuditData.ProjectFactFinding}
                    editedGridData={editedFactFindGridData}
                    isRdecFun={isRdecFun}
                />
            </CollapseBoardCard>

            <CollapseBoardCard title="年終考核意見" initValue={false}>
                <ProjectCloseMain
                    projectNo={projectNo}
                    data={projectFillAuditData}
                    projectBasicSaveData={projectBasicSaveData}
                    editedGridData={editedCloseDetailsGridData}
                    totalScoreSaveData={totalScoreSaveData}
                    isRdecFun={isRdecFun}
                />
            </CollapseBoardCard>

            {isRdecFun &&
                <CollapseBoardCard title="其他管考備註" initValue={false}>
                    <form>
                        <table>
                            <tr>
                                <th>其他管考備註</th>
                                <td>
                                    <TextAreaInput
                                        name="NOTES_FOR_SCHEDULE"
                                        rows={5}
                                        maxlength={500}
                                        style={{ width: "100%" }}
                                        onBlur={(e) => {
                                            setNoteForSchedule(e.target.element.current.value);
                                        }}
                                        defaultValue={noteForSchedule == null ? "" : noteForSchedule}
                                    />
                                </td>
                            </tr>
                        </table>
                    </form>
                </CollapseBoardCard>
            }
        </>
    )
}
export default ProjectFillAuditMain;
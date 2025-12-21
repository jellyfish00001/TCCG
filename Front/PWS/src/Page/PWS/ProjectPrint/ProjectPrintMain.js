import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { SetHistory, SignInStatusContext, HeaderFooterContext } from '../../../Basic/BasicData';
import { FormatDate, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import "../../../Css/ProjectPrint.css";
import ProjectPrintService from './ProjectPrintService';
import ProjectBasic from './ProjectBasic';
import ProjectCheckPoint from './ProjectCheckPoint';
import ProjectCkptCom from './ProjectCkptCom';
import ProjectExecute from './ProjectExecute';
import ProjectDelay from './ProjectDelay';
import ProjectBudgetExec from './ProjectBudgetExec';
import ProjectField from './ProjectField';
import ProjectOther from './ProjectOther';
import ProjectClose from './ProjectClose';
import ProjectAudit from './ProjectAudit';
import { getWorkStage } from '../ProjectChapter/ProjectChapterService';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { GetStorageData } from '../../../Basic/CommonService';
import ReactHtmlParser from 'react-html-parser';

const ProjectPrint = (props) => {

    const {
        location: {
            state
        },
        location: {
            /**傳入的參數 */
            state: {
                projectNo,
                projectName,
                isRdecFun,
                isDiffCompare,
                checkedLog
            } = {
                projectNo: "",
                projectName: "",
                isRdecFun: "",
                isDiffCompare: "",
                checkedLog: null
            }
        }
    } = props;

    const { signInStatus, setSignInStatus } = React.useContext(SignInStatusContext);
    const { headerFooterStatus, setHeaderFooterStatus } = React.useContext(HeaderFooterContext);

    // 資料是否載入完成
    const [isDone, setIsDone] = React.useState(false);

    const [previewData, setPreviewData] = React.useState({
        // 基本資料
        ProjectBasicFill: {},
        // 檢核點設定(基本資料)
        ProjectCheckpoint: {},
        // 檢核點設定(執行情形)
        ProjectCkptCom: {},
        // 每月辦理情形(單筆)
        CheckProjectEngineeringProgress: {},
        // 每月辦理情形
        ProjectEngineeringProgress: [],
        // 落後原因分析
        ProjectDelay: [],
        // 預算執行情形
        ProjectBudgetExecute: [],
        // 實地查證情形
        ProjectFactFinding: [],
        // 其它資訊
        ProjectOther: {},
        // 結案資料
        ProjectClose: {},
        // 管考備註
        ProjectAudit: {},
        // 顯示清單
        ShowList: {},
    });

    // 紀錄查詢條件
    const [query, setQuery] = React.useState("");
    // 查詢下拉選單資料
    const [ddlData, setDdlData] = React.useState([]);

    // 差異比對結果
    const [diffCompareResult, setDiffCompareResult] = React.useState([]);

    // 取得查詢下拉資料
    const getPrintDdlData = async () => {
        const { projectNo, isDiffCompare, isSSOLogin } = props.location.state;
        let queryDdlData = ProjectPrintService.getDdlData();
        if (isDiffCompare) {
            setQuery("A");
        }
        else {
            const chapterDdlData = await getWorkStage(projectNo);
            if (chapterDdlData.filter(x => x.value == "S2").length > 0) {
                setDdlData(queryDdlData);
                setQuery("B");
            }
            else {
                setDdlData(queryDdlData.filter(x => x.value == "A"));
                setQuery("A");
            }
        }
    }

    // 取得預覽資料
    const loadData = async () => {
        SetMaskOnOff(true);
        if (isDiffCompare) {
            let result = await ProjectPrintService.getDiffCompare(getDiffCompareParam("html"));
            setDiffCompareResult(result);
        } else {
            let data = await ProjectPrintService.getProjectPrint(projectNo, query);
            setPreviewData(data);
        }
        setIsDone(true);
        SetMaskOnOff(false);
    }

    // 設定 location state
    const SetLocationState = () => {
        let data = GetStorageData("printInfo");
        props.location.state = data;

        if (!signInStatus) {
            setSignInStatus();
        }
        if (!headerFooterStatus) {
            setHeaderFooterStatus(true);
        }
    }

    React.useEffect(() => {
        SetHistory(props.history);
        SetLocationState();
        getPrintDdlData();
    }, []);

    React.useEffect(() => {
        if (!IsNullOrEmpty(query)) {
            loadData();
        }
    }, [query]);

    /**
     * 下載報表
     * @param {*} extension 
     */
    const downRpt = (extension) => {
        let model = null;

        if (isDiffCompare) {
            model = getDiffCompareParam(extension);
        } else {
            model = {
                PROJECT_NO: projectNo,
                Extension: extension,
                Type: query,
                IsRdecFun: isRdecFun
            }
        }
        ProjectPrintService.downProjectPrint(model);
    }

    /**
     * 取得差異比對參數
     * @param {*} extension 
     * @returns 
     */
    const getDiffCompareParam = (extension) => {
        const data = {
            PROJECT_NO: projectNo,
            Extension: extension,
            Type: query,
            IsDiffCompare: true,
            LOG_ID: checkedLog
        }

        return data;
    }

    return (
        <>
            <div className="navOpen page">
                <CollapseBoardCard
                    button={
                        <div style={{ display: "flex" }}>
                            <Button title="Print" className="export-Print" onClick={() => { window.print(); }} />
                            <Button title="DOCX" className="export-DOCX" onClick={() => { downRpt("DOCX") }} />
                            <Button title="PDF" className="export-PDF" onClick={() => { downRpt("PDF") }} />
                            <Button title="ODT" className="export-ODT" onClick={() => { downRpt("ODT") }} />
                            {!isDiffCompare && <DropDownListWithValue
                                data={ddlData}
                                textField={"text"}
                                dataItemKey={"value"}
                                value={query}
                                onChange={(e) => {
                                    setIsDone(false);
                                    setQuery(e.target.value);
                                }}
                                style={{ width: "300px" }}
                            />}
                        </div>
                    } title={projectName} isFirstArea={true}>
                    <div className='printResult'>
                        {isDone ? (isDiffCompare ?
                            ReactHtmlParser(diffCompareResult) :
                            <>
                                <p className='title'><span>{projectName}</span></p>
                                <p className='sub-title'>資料日期:{FormatDate(new Date(), 'tYY/MM/DD')}</p>
                                <ProjectBasic
                                    projectBasic={previewData.ProjectBasicFill}
                                />
                                {query == "A" ?
                                    <ProjectCheckPoint
                                        projectCheckpoint={previewData.ProjectCheckpoint}
                                    />
                                    :
                                    <>
                                        <ProjectCkptCom
                                            projectCheckpoint={previewData.ProjectCheckpoint}
                                            projectCkptCom={previewData.ProjectCkptCom}
                                        />
                                        <p>三、執行情形</p>
                                        {/* （一）每月辦理情形 */}
                                        {previewData.ShowList.ProjectExecute &&
                                            <ProjectExecute
                                                checkEngineeringProgress={previewData.CheckProjectEngineeringProgress}
                                                engineeringProgress={previewData.ProjectEngineeringProgress}
                                                title={previewData.ShowList.ProjectExecute}
                                            />
                                        }

                                        {/* （二）落後原因分析 */}
                                        {previewData.ShowList.ProjectDelay &&
                                            <ProjectDelay
                                                projectDelay={previewData.ProjectDelay}
                                                title={previewData.ShowList.ProjectDelay}
                                            />
                                        }

                                        {/* （三）預算執行情形 */}
                                        {previewData.ShowList.ProjectBudgetExec &&
                                            <ProjectBudgetExec
                                                projectBudgetExec={previewData.ProjectBudgetExecute}
                                                title={previewData.ShowList.ProjectBudgetExec}
                                            />
                                        }

                                        {/* （四）實地查證情形 */}
                                        {previewData.ShowList.ProjectField &&
                                            <ProjectField
                                                projectField={previewData.ProjectFactFinding}
                                                title={previewData.ShowList.ProjectField}
                                            />
                                        }

                                        {/* （五）其它資訊 */}
                                        {previewData.ShowList.ProjectOther &&
                                            <ProjectOther
                                                projectOther={previewData.ProjectOther}
                                                title={previewData.ShowList.ProjectOther}
                                                checkList={previewData.ShowList.SubProjectOther}
                                            />
                                        }

                                        {/* （六）結案資料 */}
                                        {previewData.ShowList.ProjectClose &&
                                            <ProjectClose
                                                projectClose={previewData.ProjectClose}
                                                title={previewData.ShowList.ProjectClose}
                                            />
                                        }
                                    </>
                                }

                                {/* 四、管考備註 */}
                                {query == "C" &&
                                    <ProjectAudit
                                        projectAudit={previewData.ProjectAudit}
                                        isRdecFun={isRdecFun}
                                        isShowProjectField={previewData.ShowList.ProjectField}
                                    />
                                }
                            </>
                        ) : null}
                    </div>
                </CollapseBoardCard>

            </div>
        </>
    );
}

export default ProjectPrint;
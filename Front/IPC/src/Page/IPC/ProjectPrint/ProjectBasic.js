import React from 'react';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';
import { formatNumber } from '@telerik/kendo-intl';
import { IsNullOrEmpty, FormatDate, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { getUserListByOuId, downProjectAttachment } from '../../../Basic/CommonService';
import ProjectPrintService from './ProjectPrintService';
import ProjectBasicMap from './ProjectBasicMap';

// 基本資料
const ProjectBasic = (props) => {
    const { projectBasic,
        projectBasic: { ProjectBasic: { // destructuring object
            MASTER_ORGAN_C,
            MASTER_UNDERTAKER_C,
            EXEC_ORGAN_C,
            EXEC_UNDERTAKER_C,
            BUDGET_HOLD_ORGAN_C,
            BUDGET_HOLD_UNDERTAKER_C
        } } } = props;
    // 預設位置
    const [myPosition, setMyPosition] = React.useState({});
    const [DdlData, setDdlData] = React.useState({
        BUDGET_CLASS: [],
        PLAN_ITEM_C: [],
        PLAN_ITEM_L: [],
        BUILD_KIND_TYPE: [],
        COM_PLANKIND: [],
        COM_REVIEWITEM: [],
        ORGAN: [],
        TOWN_C: [],
        MasterUser: [],
        ExecUser: [],
        BudgetUser: [],
        PROJECT_STATUS: [],
    });

    const getAllDdl = async () => {
        let result = await ProjectPrintService.getProjectBasicDdl();
        const MasterUserList = IsNullOrEmpty(MASTER_ORGAN_C) ? []
            : await getUserListByOuId(MASTER_ORGAN_C, 1);
        const ExecUserList = IsNullOrEmpty(EXEC_ORGAN_C) ? []
            : await getUserListByOuId(EXEC_ORGAN_C, 2);
        const BudgetUserList = IsNullOrEmpty(BUDGET_HOLD_ORGAN_C) ? []
            : await getUserListByOuId(BUDGET_HOLD_ORGAN_C, 4);
        if (result.length > 0) {
            setDdlData({
                BUDGET_CLASS: [...result[0]],
                PLAN_ITEM_C: [...result[1]],
                PLAN_ITEM_L: [...result[2]],
                BUILD_KIND_TYPE: [...result[3]],
                COM_PLANKIND: [...result[4]],
                COM_REVIEWITEM: [...result[5]],
                ORGAN: [...result[6]],
                TOWN_C: [...result[7]],
                MasterUser: [...MasterUserList],
                ExecUser: [...ExecUserList],
                BudgetUser: [...BudgetUserList],
                PROJECT_STATUS: [...result[8]],
            });
        }
    }

    React.useEffect(() => {
        if (!IsNullOrEmpty(projectBasic)) {
            getAllDdl();
            setMyPosition({
                lat: projectBasic.ProjectBasic.X_COORD,
                lng: projectBasic.ProjectBasic.Y_COORD
            });
        }
    }, [projectBasic])

    /**
    * 檢視點位click event or 設定點位click event
    * @param {*} type view:0，set:1
    */
    const pointClickEvent = (type) => {
        const { PROJECT_NO, X_COORD, Y_COORD } = projectBasic.ProjectBasic;

        let url = "https://maps.tycg.gov.tw/tycg_painter/ResearchMapPainterPage.aspx?Aid=IMC_029&MapPaintId=";

        if (!IsNullOrEmpty(X_COORD) || !IsNullOrEmpty(Y_COORD)) {
            window.open(url + PROJECT_NO + '&Longitude=' + Y_COORD + '&Latitude=' + X_COORD + '&ShareEdit=' + type);
        } else {
            window.open(url + PROJECT_NO + '&Address=桃園區縣府路1號&ShareEdit=' + type);
        }
    }

    // 計算計畫總經費
    const calculateTotalAmount = () => {
        let value = 0;
        projectBasic.ProjectBudgetSourceG.map(x => {
            value += x.BUDGET_CENTRAL + x.BUDGET_LOCAL
        })
        return formatNumber(value, 'N0');
    }

    // 組經費來源
    const mapBudgetSource = () => {
        if (projectBasic.ProjectBudgetSourceG.length == 0) {
            return (
                <tr>
                    <td colSpan={7} style={{ textAlign: "center" }}>無資料</td>
                </tr>
            )
        }
        let result = projectBasic.ProjectBudgetSourceG.map(x => {
            return (
                <tr>
                    <td style={{ textAlign: "center" }}>{x.PLAN_YEAR}</td>
                    <td style={{ textAlign: "center" }}>{ProjectPrintService.findText(DdlData.BUDGET_CLASS, "SET_VALUE", "SET_TYPE", x.BUDGET_CLASS)}</td>
                    <td>{ProjectPrintService.findText(DdlData.PLAN_ITEM_C, "PLAN_ITEM_NAME", "PLAN_ITEM_ID", x.PLAN_ITEM_C)}</td>
                    <td style={{ textAlign: "right" }}>{formatNumber(x.BUDGET_CENTRAL, 'N0')}</td>
                    <td>{ProjectPrintService.findText(DdlData.PLAN_ITEM_L, "PLAN_ITEM_NAME", "PLAN_ITEM_ID", x.PLAN_ITEM_L)}</td>
                    <td style={{ textAlign: "right" }}>{formatNumber(x.BUDGET_LOCAL, 'N0')}</td>
                    <td onLoad={console.log(x.FILE)}>
                        {x.FILE != null && x.FILE.length > 0 ? (
                            x.FILE.length === 1 ? (
                                // 只有一個檔案
                                <a href='/' onClick={(e) => {
                                    e.preventDefault();
                                    downProjectAttachment(x.FILE[0].IDENTITY_FIELD);
                                }}>{x.FILE[0].FILE_NAME}</a>
                            ) : (
                                // 如果有多個檔案,逐列列出多個檔案
                                x.FILE.map((file, index) => (
                                    <div key={index}>
                                        <a href='/' onClick={(e) => {
                                            console.log(file)
                                            e.preventDefault();
                                            downProjectAttachment(file.IDENTITY_FIELD);
                                        }}>{file.FILE_NAME}</a>

                                    </div>
                                ))
                            )
                        ) : (
                            ""
                        )}
                    </td>
                </tr>
            )
        })
        return result;
    }

    // 組建設類別
    const mapBuildKind = () => {
        let result = DdlData.BUILD_KIND_TYPE.map(x => {
            const selBuildKind = projectBasic.ProjectBuildKind.filter(y => y.BUILD_KIND_TYPE == x.SET_TYPE).map(y => y.BUILD_KIND)
            return (
                <div>
                    {x.SET_VALUE}：{DdlData.COM_PLANKIND.filter(y => selBuildKind.includes(y.SET_TYPE)).map(x => x.SET_VALUE).join("、")}
                </div>
            )
        })
        return result;
    }

    // 組相關審查
    const mapReview = () => {
        if (!IsNullOrEmpty(projectBasic.ProjectBasic.REVIEWITEM)) {
            let reviewItem = projectBasic.ProjectBasic.REVIEWITEM.split(',');
            return DdlData.COM_REVIEWITEM.filter(x => reviewItem.includes(x.SET_TYPE)).map(x => x.SET_VALUE).join(",")
        }
        else {
            return;
        }
    }

    // 機關/人員
    const OrganPerson = (organValue, userList, userValue) => {
        if (!IsNullOrEmpty(organValue)) {
            return (
                <>
                    {ProjectPrintService.findText(DdlData.ORGAN, "text", "value", organValue)}/
                    {ProjectPrintService.findText(userList, "text", "value", userValue)}
                </>
            )
        }
        else {
            return;
        }
    }

    // 協辦機關
    const assistant = () => {
        if (projectBasic.ProjectAsstOrg.length == 0) {
            return "無";
        }
        return projectBasic.ProjectAsstOrg.map(x => {
            return (
                <div>{ProjectPrintService.findText(DdlData.ORGAN, "text", "value", x.ASSISTANT_ORGAN_C)}/{x.ASSISTANT_UNDERTAKER_C_NAME}</div>
            )
        })
    }

    const projLogCell = (values, logStatuses) => {
        if (values.ProjLogs === undefined || values.ProjLogs === null) {
            return "";
        }

        return values.ProjLogs
            .filter(x => logStatuses.includes(x.LOG_STATUS_C))
            .map((item, i) =>
                <>
                    <span>{i + 1}.</span>
                    <span>{FormatDate(item.LOG_DATE)} </span>
                    <span>{item.LOG_STATUS}</span>
                    <span>{IsNullOrEmpty(item.MEMO) ? "" : ":"}</span>
                    <span>{item.MEMO}</span>
                    <span>。</span>
                    <br />
                </>
            )
    }

    return (
        <>
            <p>一、基本資料</p>
            <form>
                <table style={{ wordBreak: "break-all" }}>
                    <tr>
                        <th colSpan={2}>計畫年度</th>
                        <td colSpan={3}>{projectBasic.ProjectBasic.PROJECT_YEAR}</td>
                        <th>列管編號</th>
                        <td>{projectBasic.ProjectBasic.PROJECT_NO}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>計畫名稱</th>
                        <td colSpan={5}>{projectBasic.ProjectBasic.PROJECT_NAME}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>列管狀態</th>
                        <td colSpan={5}>{projectBasic.ProjectBasic.TUBE_STATUS_DESC}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>計畫總經費(元)</th>
                        <td colSpan={5}>
                            {calculateTotalAmount()}
                        </td>
                    </tr>
                    <tr>
                        <th colSpan={7} style={{ textAlign: "center" }}>經費來源</th>
                    </tr>
                    <tr>
                        <th style={{ textAlign: "center", width: "50px" }}>年度</th>
                        <th style={{ textAlign: "center", width: "80px" }}>預算類型</th>
                        <th style={{ textAlign: "center" }}>中央預算來源</th>
                        <th style={{ textAlign: "center" }}>中央補助款(元)</th>
                        <th style={{ textAlign: "center" }}>本府預算來源</th>
                        <th style={{ textAlign: "center" }}>本府預算金額(元)</th>
                        <th style={{ textAlign: "center" }}>核定函</th>
                    </tr>

                    {mapBudgetSource()}

                    <tr>
                        <th colSpan={2}>建設類別</th>
                        <td colSpan={5}>{mapBuildKind()}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>相關審查</th>
                        <td colSpan={5}>{mapReview()}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>特殊加註</th>
                        <td colSpan={5}>{projectBasic.ProjectBasic.SPEC_NOTE}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>主管機關/人員</th>
                        <td colSpan={5}>{OrganPerson(MASTER_ORGAN_C, DdlData.MasterUser, MASTER_UNDERTAKER_C)}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>執行機關/人員</th>
                        <td colSpan={5}>{OrganPerson(EXEC_ORGAN_C, DdlData.ExecUser, EXEC_UNDERTAKER_C)}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>協辦機關/人員</th>
                        <td colSpan={5}>{assistant()}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>代辦機關/人員</th>
                        <td colSpan={5}>{IsNullOrEmpty(BUDGET_HOLD_ORGAN_C) ? "無" : OrganPerson(BUDGET_HOLD_ORGAN_C, DdlData.BudgetUser, BUDGET_HOLD_UNDERTAKER_C)}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>辦理地點</th>
                        <td colSpan={5}>{ProjectPrintService.findText(DdlData.TOWN_C, "text", "value", projectBasic.ProjectBasic.TOWN_C)}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>位置說明</th>
                        <td colSpan={5}>{projectBasic.ProjectBasic.PROJECT_LOCATION}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>單點地圖定位</th>
                        <td colSpan={5}>
                            <ProjectBasicMap
                                myPosition={myPosition}
                            />
                        </td>
                    </tr>
                    <tr>
                        <th colSpan={2}>多點地圖定位</th>
                        <td colSpan={5}>
                            <a onClick={() => pointClickEvent(0)}>檢視地點</a>
                        </td>
                    </tr>
                    <tr>
                        <th colSpan={2}>計畫內容</th>
                        <td colSpan={5}><TextAreaWrapInput value={projectBasic.ProjectBasic.ALL_JOB} /></td>
                    </tr>
                    <tr>
                        <th colSpan={2}>計畫效益</th>
                        <td colSpan={5}><TextAreaWrapInput value={projectBasic.ProjectBasic.PROJECT_BENEFIT} /></td>
                    </tr>
                    <tr>
                        <th colSpan={2}>備註</th>
                        <td colSpan={5}><TextAreaWrapInput value={projectBasic.ProjectBasic.MEMO} /></td>
                    </tr>
                    <tr>
                        <th colSpan={2}>立案時間</th>
                        <td colSpan={5}>{IsNullOrEmpty(projectBasic.ProjectBasic.CREATEDTIME) ? "" : FormatDate(projectBasic.ProjectBasic.CREATEDTIME, 'tYY/MM/DD HH:mm')}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>立案審核意見</th>
                        <td colSpan={5}>{projLogCell(projectBasic.ProjectBasic, ["3", "4"])}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>基本資料成績</th>
                        <td colSpan={5}>{projectBasic.ProjectBasic.SCORE_A}</td>
                    </tr>
                    <tr>
                        <th colSpan={2}>結案審核意見</th>
                        <td colSpan={5}>{projLogCell(projectBasic.ProjectBasic, ["6", "7", "8"])}</td>
                    </tr>
                </table>
            </form>
            {/* 處理標格與標題之間的距離 */}
            <p className='preview-table-p'></p>
        </>
    );
}

export default ProjectBasic;
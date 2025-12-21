import React, { useContext } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { withRouter } from 'react-router-dom';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { SetHistory, SignInStatusContext } from '../../../Basic/BasicData';
import { GetStorageData, SetStorageData } from '../../../Basic/CommonService';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { PanelBar, PanelBarUtils } from '@progress/kendo-react-layout';
import { Reveal } from '@progress/kendo-react-animation';
import { getProjectChapter, downProjectFillYearAssRPT, getWorkStage, getProjectStatus } from './ProjectChapterService';
import { PageIndexContext } from './ChapterPageIndexProvider';

const ProjectChapterRoutePanel = (props) => {

    //#region
    const {
        visible,
        location: {
            /**傳入的參數 */
            state: {
                projectNo,
                projectName
            } = {
                projectNo: null,
                projectName: null
            }
        }
    } = props;

    // 計畫作業
    const [Operation, setOperation] = React.useState("");
    // 作業階段
    const [WorkStage, setWorkStage] = React.useState("");

    const [workStageDdlData, setWorkStageDdlData] = React.useState([]);

    const { signInStatus, setSignInStatus } = React.useContext(SignInStatusContext);

    const [items, setItems] = React.useState([]);

    // 紀錄是否是 作業階段 切換，true:則章節表預設第一個url
    const [isWorkStageChange, setIsWorkStageChange] = React.useState(false);

    const { pageIndex, changePageIndex } = useContext(PageIndexContext)

    //#endregion

    /**
     * 取得功能樹
     * @param {*} operation 計畫作業
     * @param {*} workStage 作業階段
     * @param {*} funRole  功能面向角色
     * @param {*} chapter   指定章節
     */
    const getChapterList = async (operation, workStage, funRole, chapter) => {
        const { projectNo } = props.location.state;
        let data = await getProjectChapter(operation, funRole, projectNo, "IPC3")

        // 第一次進來 或 WorkStage切換，才需預設到第一個url
        if (props.location.pathname === "/ProjectChapter") {
            redirectTargetPage({ data, operation, workStage, chapter });
        } else {
            // "計畫作業"為P1:計畫填報，則會有"作業階段"
            if (operation === 'P1') {
                data = data.filter(x => x.STAGE === workStage || x.STAGE === 'S4');
            }
            setChpaterData(data);
            //刷新的時候，要綁上原本的Selected item
            let index = data.findIndex(x => x.SOURCE_PATH === props.location.pathname)
            changePageIndex(`.${index.toString()}`)
        }

        // 設定章節資料 to Storage
        setChapterData(data);

        if (!signInStatus) {
            setSignInStatus();
        }
    }

    /**
     * 將當前章節列表資料寫入storage並以props傳到子頁
     * @param {*} chapterData 
     */
    const setChapterData = (chapterData) => {
        let storageData = GetStorageData();
        storageData.chapterList = chapterData;
        SetStorageData(storageData)
        SetLocationState();
    }

    /**
     * 導向至目標頁
     * @param {*} param 
     */
    const redirectTargetPage = async (param) => {
        let { data, operation, workStage, chapter } = param

        let currentUrl = '';
        let selectedIndex = ".0"
        // 若有指定目的章節，則另外設定 url & 功能樹，否則預設第一個URL
        if (!IsNullOrEmpty(chapter) && !isWorkStageChange) {
            currentUrl = data.find(x => x.CHAPTER_ID === chapter)?.SOURCE_PATH;
            // 依據指定章節位置設定功能樹
            let targetStage = data.find(x => x.CHAPTER_ID === chapter)?.STAGE;
            setWorkStage(targetStage ?? workStage);
            // "計畫作業"為P1:計畫填報，則會有"作業階段"
            if (operation === 'P1') {
                data = data.filter(x => (x.STAGE === (targetStage ? targetStage : workStage)) || x.STAGE === 'S4')
            }
            selectedIndex = `.${data.findIndex(x => x.SOURCE_PATH === currentUrl)}`;
        } else {
            // "計畫作業"為P1:計畫填報，則會有"作業階段"
            if (operation === 'P1') {
                data = data.filter(x => x.STAGE === workStage || x.STAGE === 'S4');
            }
            currentUrl = data[0]?.SOURCE_PATH
        }
        // 確保導頁時，props.location.state的資訊正確
        await SetShowBtnStatus();

        // 設定章節資料 to Storage
        setChapterData(data);

        setChpaterData(data);
        // 導頁
        if (!IsNullOrEmpty(currentUrl) && data[0]?.children == null) {
            changePageIndex(selectedIndex)
            props.history.push(currentUrl, props.location.state);
        }
    }

    const setChpaterData = (data) => {
        // 設定章節功能樹
        setItems([...data]);
    }

    const onSelect = (event) => {
        const { SOURCE_PATH, uniquePrivateKey, CHAPTER_ID } = event.target.props
        if (!IsNullOrEmpty(SOURCE_PATH) && props.history.location.pathname !== SOURCE_PATH) {
            changePageIndex(uniquePrivateKey);
            props.history.push(SOURCE_PATH, props.location.state);
        }
        else if (CHAPTER_ID == "ProjectFillYearAss") {
            downProjectFillYearAssRPT(props.location.state.projectNo);
        }

        if (!signInStatus) {
            setSignInStatus();
        }
    }

    // 設定 location state
    const SetLocationState = () => {
        let data = GetStorageData();
        props.location.state = data;
    }

    React.useEffect(() => {
        SetHistory(props.history);
        SetLocationState();
        const { operation, workStage, funRole, chapter } = props.location.state;
        setOperation(operation);

        // "計畫作業"為P1:計畫填報，則"作業階段"，預設S1:立案
        let currentWorkStage = operation === "P1" && IsNullOrEmpty(workStage) ? "S1" : workStage;
        if (!IsNullOrEmpty(currentWorkStage)) {
            getWorkStageDdlData(currentWorkStage);
        }
        else {
            getChapterList(operation, workStage, funRole, chapter);
        }
    }, []);

    /**
     * 取得 作業階段 清單
     * @param {*} currentWorkStage 
     */
    const getWorkStageDdlData = async (currentWorkStage) => {
        const { projectNo } = props.location.state;
        let data = await getWorkStage(projectNo);
        if (data.filter(x => x.value === currentWorkStage).length > 0) {
            // 紀錄作業階段
            setWorkStage(currentWorkStage);
            setWorkStageDdlData(data);
        } else {
            showGlobalMessageBox("資料有誤，請洽系統管理員")
        }
    }

    React.useEffect(() => {
        if (!IsNullOrEmpty(WorkStage)) {
            SetLocationState();
            const { operation, workStage, funRole, chapter } = props.location.state;

            if (isWorkStageChange) {
                // 切換作業階，預設到第一個url
                props.location.pathname = "/ProjectChapter";
                getChapterList(operation, workStage, funRole, chapter);
                setIsWorkStageChange(false)
            } else {
                getChapterList(operation, workStage, funRole, chapter);
            }
        }
    }, [WorkStage]);

    const SetShowBtnStatus = async () => {
        let data = GetStorageData();

        const projectStatus = await getProjectStatus(data.projectNo);

        // 是否為管考功能
        data.isRdecFun = data.funRole === 2;

        // 在主辦(填報)，且"立案審核通過"時(左邊章節表上方選單若預設「執行情形」)
        // 切換「立案」時的各章節（含「相關檔案上傳」）
        // 其他按鈕不顯示，只顯示"預覽按鈕"
        data.showBtnByLogStatus =
            !(data.funRole === 0
                && workStageDdlData.filter(x => x.value === "S2").length === 1
                && WorkStage === "S1")

        // 在主辦(填報)，且計畫狀態為「立案審核」
        // 「立案」時的各章節（含「相關檔案上傳」），
        // 其他按鈕不顯示，只顯示"預覽按鈕"
        let r1 = !(data.funRole === 0 && projectStatus === 2);
        // 計畫狀態為「結案審核」、「已結案」或「已撤銷」
        // 計畫填報頁面之「立案」或「執行情形」各章節（含「相關檔案上傳」）
        // 其他按鈕不顯示，只顯示"預覽按鈕"
        let r2 = !(projectStatus === 5 || projectStatus === 7 || projectStatus === 8);
        data.showBtnByProjectStatus = r1 && r2;
        data.showSomeBtn = data.isRdecFun || (data.showBtnByLogStatus && data.showBtnByProjectStatus);

        SetStorageData(data);
        SetLocationState();
    }

    const components = PanelBarUtils.mapItemsToComponents(items);

    return (
        <>
            <div>
                {visible
                    ? <div className='plan-name'>{projectNo} {projectName}</div> : null}
            </div>
            <Reveal
                //動畫長短設定
                transitionExitDuration={500}
                transitionEnterDuration={500}
                direction={"horizontal"}
            >
                {
                    visible
                        ? <>
                            {
                                //"計畫作業"為P1:計畫填報，則會顯示"作業階段"下拉選單
                                Operation === "P1"
                                    ?
                                    <DropDownListWithValue
                                        data={workStageDdlData}
                                        textField={"text"}
                                        dataItemKey={"value"}
                                        value={WorkStage}
                                        onChange={(e) => {
                                            let data = GetStorageData();
                                            data.workStage = e.target.value;
                                            SetStorageData(data);
                                            setWorkStage(e.target.value);
                                            setIsWorkStageChange(true)
                                        }}
                                    />
                                    : null
                            }
                            <PanelBar
                                expandMode={"single"}
                                onSelect={onSelect}
                                selected={pageIndex}
                                children={components}
                                style={{ width: '280px' }}
                            />
                        </>
                        : null
                }
            </Reveal>
        </>
    );
}

export default withRouter(ProjectChapterRoutePanel);


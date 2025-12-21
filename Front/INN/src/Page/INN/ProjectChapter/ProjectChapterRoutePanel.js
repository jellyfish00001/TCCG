import React, { useContext, useState, useEffect } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { withRouter } from 'react-router-dom';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { SetHistory, SignInStatusContext } from '../../../Basic/BasicData';
import { GetStorageData, SetStorageData } from '../../../Basic/CommonService';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { PanelBar, PanelBarUtils } from '@progress/kendo-react-layout';
import { Reveal } from '@progress/kendo-react-animation';
import { getProjectChapter } from './ProjectChapterService';
import { PageIndexContext } from './ChapterPageIndexProvider';

const ProjectChapterRoutePanel = (props) => {
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
    const { signInStatus, setSignInStatus } = useContext(SignInStatusContext);
    const [items, setItems] = useState([]);
    
    const { pageIndex, changePageIndex } = useContext(PageIndexContext)

    // 取得計畫章節
    const getChapterList = async () => {
        // 測試章節清單
        let data = await getProjectChapter('P1', '0', projectNo, "INN")

        if (props.location.pathname === "/ProjectChapter") {
            redirectTargetPage(data);
        } else{
            //刷新的時候，要綁上原本的Selected item
            let index = data.findIndex(x => x.SOURCE_PATH === props.location.pathname)
            changePageIndex(`.${index.toString()}`)
        }

        redirectTargetPage(data)
        setItems(data);
        if (!signInStatus)
            setSignInStatus();
    }

    /**
     * 導向至目標頁
     * @param {*} param 
     */
    const redirectTargetPage = async (data) => {
        let currentUrl = data[0]?.SOURCE_PATH;
        // 設定章節資料 to Storage
        setChapterData(data);

        // 導頁
        props.history.push(currentUrl, props.location.state);
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

    // 設定 location state
    const SetLocationState = () => {
        let data = GetStorageData();
        props.location.state = data;
    }

    const onSelect = (event) => {
        const { SOURCE_PATH, CHAPTER_ID } = event.target.props
        if (!IsNullOrEmpty(SOURCE_PATH) && props.history.location.pathname !== SOURCE_PATH) {
            props.history.push(SOURCE_PATH, props.location.state);
        }

        if (!signInStatus) {
            setSignInStatus();
        }
    }

    useEffect(() => {
        SetHistory(props.history);
        SetLocationState();
        getChapterList();
    }, []);

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
                {visible ?
                    <PanelBar
                        expandMode={"single"}
                        onSelect={onSelect}
                        selected={pageIndex}
                        children={components}
                        style={{ width: '280px' }}
                    /> : null}
            </Reveal>
        </>
    );
}

export default withRouter(ProjectChapterRoutePanel);


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
                PLAN_NO,
                PLAN_NAME,
                funRole
            } = {
                PLAN_NO: null,
                PLAN_NAME: null,
                funRole: null
            }
        }
    } = props;
    const { signInStatus, setSignInStatus } = useContext(SignInStatusContext);
    const [items, setItems] = useState([]);
    const { pageIndex, changePageIndex } = useContext(PageIndexContext)

    // 取得計畫章節
    const getChapterList = async () => {
        // 取得章節清單
        let chapterList = await getProjectChapter('P1', '1', PLAN_NO, 'RD2')
        // 第一次進來，才需預設到第一個url
        if (props.location.pathname === "/ProjectChapter") {
            redirectTargetPage(chapterList);
        } else{
            //刷新的時候，要綁上原本的Selected item
            let index = chapterList.findIndex(x => x.SOURCE_PATH === props.location.pathname)
            changePageIndex(`.${index.toString()}`)
        }
        // 設定章節資料 to Storage
        setChapterData(chapterList);
        setItems(chapterList);
        if (!signInStatus)
            setSignInStatus();
    }

    /**
     * 導向至目標頁
     * @param {*} param 
     */
    const redirectTargetPage = async (data) => {
        let currentUrl = '';
        let selectedIndex = ".0"

        // 若有指定目的章節，則另外設定 url & 功能樹，否則預設第一個URL
        if (!IsNullOrEmpty(data.CHAPTER_ID)) {
            currentUrl = data.find(x => x.CHAPTER_ID === data.CHAPTER_ID)?.SOURCE_PATH;
            selectedIndex = `.${data.findIndex(x => x.SOURCE_PATH === currentUrl)}`;
        } else {
            currentUrl = data[0]?.SOURCE_PATH
        }
        // 設定章節資料 to Storage
        setChapterData(data);

        // 導頁
        if (!IsNullOrEmpty(currentUrl) && data[0]?.children == null) {
            changePageIndex(selectedIndex)
            props.history.push(currentUrl, props.location.state);
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

    // 設定 location state
    const SetLocationState = () => {
        let data = GetStorageData();
        props.location.state = data;
    }

    const onSelect = (event) => {
        const { SOURCE_PATH, uniquePrivateKey } = event.target.props
        if (!IsNullOrEmpty(SOURCE_PATH) && props.history.location.pathname !== SOURCE_PATH) {
            changePageIndex(uniquePrivateKey);
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
                    ? <div className='plan-name'>{PLAN_NO} {PLAN_NAME}</div> : null}
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
                        children={components}
                        style={{ width: '280px' }}
                        selected={pageIndex}
                    /> : null}
            </Reveal>
        </>
    );
}

export default withRouter(ProjectChapterRoutePanel);


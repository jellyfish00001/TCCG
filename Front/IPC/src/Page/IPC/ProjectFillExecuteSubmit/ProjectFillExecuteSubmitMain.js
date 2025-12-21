import React, { useState, useEffect, useContext } from 'react';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { GetIsUserFtyData, openProjectPrint } from '../../../Basic/CommonService';
import { closeAndbackToParentWindow, FormatDate, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import ProjectFillFieldService from '../ProjectFillField/ProjectFillFieldService';
import { PageIndexContext } from '../ProjectChapter/ChapterPageIndexProvider';
import { checkProjectFillExecuteSubmit, saveProjectFillExecuteSubmit } from './ProejctFillExecuteSubmitService';

export const ProjectFillExecuteSubmitMain = (props) => {

    let {
        location: {
            state
        },
        location: {
            state: {
                projectNo,
                projectName,
                cycleData,
                chapterList
            } = {
                projectNo: null,
                projectName: '',
                cycleData: '',
                chapterList: []
            }
        }
    } = props;

    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => props.history.push('/Home'));
    }

    // 動態畫面jsx
    const [resultJsx, setResultJsx] = useState();
    const [factFinding, setFactFinding] = useState("");

    const [pageReady, setPageReady] = useState(false);

    const [isUserFtyData, setIsUserFtyData] = useState(false);

    const { changePageIndex } = useContext(PageIndexContext)

    // 檢查計畫可否送出
    const checkCanSubmit = async () => {
        SetMaskOnOff(true);
        let factFindingData = await ProjectFillFieldService.getProjectFactFinding(projectNo);
        // 檢查實地查證情形
        if (factFindingData != null && factFindingData.length > 0) {
            checkFactFinding(factFindingData);
        }

        const isUserFtyData = await GetIsUserFtyData(projectNo);
        setIsUserFtyData(isUserFtyData);

        if (!isUserFtyData) {
            let result = await checkProjectFillExecuteSubmit(projectNo);
            // 取得畫面Jsx
            setResultJsx(await getResultJsx(result.IsSubmitted, result.projectCloseApplyValid, [...result.ErrorModels]));
            setPageReady(true);
        }
        SetMaskOnOff(false);
    }

    /**
     * 檢查該計畫實地查證有回覆期限且未輸入執行機關參採情形的資訊
     * @param {*} datas
     */
    const checkFactFinding = (datas) => {
        // 有回覆期限且未輸入執行機關參採情形的資料
        let needReplyAndNoReportData = datas.filter(x => x.COMPLETEREPLYDATE != null
            && IsNullOrEmpty(x.FFREPORT));
        if (needReplyAndNoReportData != null && needReplyAndNoReportData.length > 0) {
            let jsx = needReplyAndNoReportData.map(x => {
                let ffDate = FormatDate(x.FFDATE)
                let replyDate = FormatDate(x.COMPLETEREPLYDATE)
                return (
                    <div>{`查證日期：${ffDate} ( 回覆期限：${replyDate} ) 尚未回覆執行機關參採情形。`}</div>
                )
            })
            setFactFinding(jsx);
        }
    }

    /**
     * 取得審核結果Jsx
     * @param {*} IsSubmitted 是否已送出
     * @param {*} canApply    可否申請結案
     * @param {*} gridData    錯誤章節資料
     */
    const getResultJsx = async (IsSubmitted, canApply, gridData) => {

        const submitBtnText = IsSubmitted ? "已送出" : "確認送出"
        const canSend = gridData.length === 0 || (gridData.length === 1 && gridData[0].Chapter === "結案資料");
        return (
            <>
                <div className='fn-buttons'>
                    {/* 送出按鈕 */}
                    {canSend &&
                        <Button type='button' onClick={() => { submit(1) }} disabled={IsSubmitted}>{submitBtnText}</Button>
                    }

                    {/* 可送出且申請結案 */}
                    {(canSend && canApply) && <Button type='button' title="結案申請"
                        onClick={() => {
                            showGlobalConfirmBox('請確認是否提出結案申請？', () => { submit(2) })
                        }} > 結案申請</Button>}
                </div>
                {/* 錯誤章節Grid */}
                {gridData.length > 0 &&
                    <Grid
                        style={{
                            height: '100%',
                            overflow: 'auto',
                        }}
                        data={gridData}
                        resizable={true}
                    >
                        <GridNoRecords>無資料</GridNoRecords>
                        <GridColumn field="Chapter"
                            title="錯誤章節"
                            cell={ChapterLinkCell} />
                        <GridColumn
                            field='ErrMsg'
                            title="錯誤內容"
                            cell={ErrContentCell}
                        />
                    </Grid>}
            </>
        )
    }

    // 錯誤章節內容Cell
    const ChapterLinkCell = prop => {
        let data = prop.dataItem;
        let targetChapterIndex = chapterList.findIndex(x => x.CHAPTER_ID === data.ChapterId) ?? 0;
        return (
            <td>
                <a onClick={() => {
                    props.history.push(data.ChapterUrl, props.location.state);
                    changePageIndex(`.${targetChapterIndex}`);
                }}
                >{data.Chapter}</a>
            </td>
        )
    }

    // 錯誤內容Cell
    const ErrContentCell = props => {
        return (
            <span>{props.dataItem.ErrMsg}</span>
        )
    }

    // 送出
    const submit = async (saveType) => {
        SetMaskOnOff(true);
        let result = await saveProjectFillExecuteSubmit(projectNo, saveType);
        SetMaskOnOff(false);

        if (result.success) {
            showGlobalMessageBox(result.message, () => {
                if (saveType === 1) {
                    checkCanSubmit()
                } else {
                    closeAndbackToParentWindow(window);
                }
            });
        }
    }

    // 預覽列印
    const preview = async () => {
        openProjectPrint(state);
    }

    useEffect(() => {
        checkCanSubmit();
    }, [])

    return (
        <CollapseBoardCard
            button={
                <div className='fn-buttons'>
                    <Button type='button' title="預覽列印" className="k-button-lighten" onClick={preview}  >預覽列印</Button>
                </div>
            }
            title={'執行情形送出'}
            isFirstArea={true}
        >
            <div>計畫名稱：{projectName}</div>
            <div>填報週期：{cycleData}</div>
            {isUserFtyData && <div>已使用界接資料填報，無需自行執行「執行情形送出」。</div>}
            <div>{factFinding}</div>
            {pageReady && resultJsx}
        </CollapseBoardCard>
    );
}
export default ProjectFillExecuteSubmitMain
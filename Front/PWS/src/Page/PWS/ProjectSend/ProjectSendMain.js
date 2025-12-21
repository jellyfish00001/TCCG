import React, { useState, useEffect, useContext } from 'react';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { openProjectPrint } from '../../../Basic/CommonService';
import { closeAndbackToParentWindow, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { PageIndexContext } from '../ProjectChapter/ChapterPageIndexProvider';
import { GetPlanErrorData, SavePlanErrorData } from './ProjectSendService';
import { exportRPT } from "../ReportList/ReportService";

export const ProjectSendMain = (props) => {
    // 獲取路由和狀態參數
    const {
        location: {
            state
        },
        location: {
            state: {
                projectNo,
                projectName,
                projectKind,
                cycleData,
                chapterList
            } = {
                projectNo: null,
                projectName: '',
                projectKind: null,
                cycleData: '',
                chapterList: []
            }
        }
    } = props;
    
    // 檢查是否通過正確途徑進入該頁面
    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => props.history.push('/Home'));
    }
    const [resultJsx, setResultJsx] = useState();
    const [pageReady, setPageReady] = useState(false);
    const { changePageIndex } = useContext(PageIndexContext);

    /**
     * 檢查計劃是否可以送出
     * @returns
     */
    const checkCanSubmit = async () => {
        SetMaskOnOff(true);
        let result = await GetPlanErrorData(projectNo, projectKind);
        setResultJsx(await getResultJsx(result.IS_SEND, [...result.ErrorModels]));
        setPageReady(true);
        SetMaskOnOff(false);
    };

    /**
     * 取得審核結果Jsx
     * @param {*} IsSend 是否已送出
     * @param {*} gridData    錯誤章節資料
     */
    const getResultJsx = async (IsSend, gridData) => {
        const submitBtnText = IsSend ? "已送出" : "確認送出";
        const canSend = gridData.length === 0;
        return (
            <>
                <div className='fn-buttons'>
                    {/* 送出按鈕 */}
                    {canSend &&
                        <Button type='button' onClick={() => { submit() }} disabled={IsSend}>{submitBtnText}</Button>
                    }
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
    };

    // 顯示章節的連結 Cell
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

    // 顯示錯誤內容的 Cell
    const ErrContentCell = props => {
        return (
            <span>{props.dataItem.ErrMsg}</span>
        )
    }

    // 提交計劃
    const submit = async () => {
        SetMaskOnOff(true);
        let result = await SavePlanErrorData(projectNo);
        SetMaskOnOff(false);

        if (result.success) {
            showGlobalMessageBox(result.message, () => {
                // 關閉章節頁，並導回來源列表頁
                closeAndbackToParentWindow(window);
            });
        }
    }

    useEffect(() => {
        checkCanSubmit();
    }, [])

        /**
     * 計畫資料報表匯出
     * @param {*} checkdata
     */
        const ReportExport = async () => {
            // 匯出重大或委託
            if(projectKind == 1){
                let data =  {
                    RPT_ID : "RPTProjectPolicyReview",
                    STATISTICS_NAME : "桃園市政府113先期計畫重大施政計畫先期審查表",
                    PlanNoList : [projectNo]
                };
                await exportRPT(data);
            }
            else{
                let data =  {
                    RPT_ID : "RPTProjectEntList",
                    STATISTICS_NAME : "桃園市政府113先期計畫先期審查",
                    PlanNoList : [projectNo]
                };
                await exportRPT(data);
            }
        }

    return (
        <CollapseBoardCard
            button={
                <Button title="預覽計畫" className="k-button-lighten" onClick={() => ReportExport()} >預覽計畫</Button>
            }
            title={'執行情形送出'}
            isFirstArea={true}
        >
            <div>計畫名稱：{projectName}</div>
            {/* <div>填報週期：{cycleData}</div> */}
            {pageReady && resultJsx}
        </CollapseBoardCard>
    );
}
export default ProjectSendMain;

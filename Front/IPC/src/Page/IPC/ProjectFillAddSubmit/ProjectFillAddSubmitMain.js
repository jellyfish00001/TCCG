import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import Table from '../../../Css/custom/Table.module.css';
import { checkProjectCanSubmit, saveProjectFillAddSubmit } from './ProjectFillAddSubmitService'
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { closeAndbackToParentWindow, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { openProjectPrint } from '../../../Basic/CommonService';
import { PageIndexContext } from '../ProjectChapter/ChapterPageIndexProvider';

export const ProjectFillAddSubmitMain = (props) => {

    let {
        location: {
            state
        },
        location: {
            state: {
                projectNo,
                projectName,
                chapterList
            } = {
                projectNo: null,
                projectName: '',
                chapterList: []
            }
        }
    } = props;

    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => props.history.push('/Home'));
    }

    const [resultJsx, setResultJsx] = useState()
    const [pageReady, setPageReady] = useState(false);

    const { changePageIndex } = useContext(PageIndexContext)

    // 檢查計畫可否送出
    const checkCanSubmit = async () => {
        SetMaskOnOff(true);
        let result = await checkProjectCanSubmit(projectNo);

        setResultJsx(await getResultJsx(result.IsSubmitted, result.ErrorModels.length == 0, [...result.ErrorModels]))
        setPageReady(true)
        SetMaskOnOff(false);
    }

    /**
     * 取得審核結果Jsx
     * @param {*} IsSubmitted 是否已送出
     * @param {*} submitValid 可否送出
     * @param {*} gridData    錯誤章節資料
     */
    const getResultJsx = async (IsSubmitted, submitValid, gridData) => {
        // 可送出
        if (submitValid) {
            return (
                <div className='fn-buttons'>
                    {/* 是否已送出 */}
                    {!IsSubmitted ?
                        // 未送出
                        <Button type='button' title="確認送出" onClick={() => {
                            showGlobalConfirmBox('請確認是否執行計畫送審？', () => { submit() })
                        }} >確認送出</Button> :
                        // 已送出
                        <div>計畫已送審</div>
                    }
                </div>
            )
            // 不可送出
        } else {
            return (
                <>
                    {/* 錯誤章節Grid */}
                    <Grid
                        style={{
                            textAlign: "center",
                            height: '100%',
                            overflow: 'auto',
                        }}
                        data={gridData}
                        resizable={true}
                    >
                        <GridNoRecords>無資料</GridNoRecords>
                        <GridColumn field="Chapter"
                            headerClassName={Table.textAlign_center}
                            title="錯誤章節"
                            cell={ChapterLinkCell} />
                        <GridColumn
                            field='ErrMsg'
                            headerClassName={Table.textAlign_center}
                            title="錯誤內容"
                            className='error-msg'
                        />
                    </Grid>
                </>
            )
        }
    }


    // 錯誤章節Cell
    const ChapterLinkCell = prop => {
        let data = prop.dataItem;
        let targetChapterIndex = chapterList.findIndex(x => x.CHAPTER_ID == data.ChapterId) ?? 0;
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

    // 送出
    const submit = async () => {
        SetMaskOnOff(true);
        let result = await saveProjectFillAddSubmit(projectNo);
        SetMaskOnOff(false);
        if (result.success) {
            showGlobalMessageBox(result.message, () => {
                // 關閉章節頁，並導回來源列表頁
                closeAndbackToParentWindow(window);
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
            button={<Button title="預覽列印" className="k-button-lighten" onClick={preview}>預覽列印</Button>}
            title="立案送審"
            isFirstArea={true}
        >
            <div>計畫名稱：{projectName}</div>
            {pageReady && resultJsx}
        </CollapseBoardCard>
    );
}
export default ProjectFillAddSubmitMain
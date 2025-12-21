import React, { useEffect, useRef, useState } from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import OrgSelectPanel from '../../../Components/Selector/OrgSelectPanel';
import { Button } from "@progress/kendo-react-buttons";
import InnProjectManageQueryForm from './InnProjectManageQueryForm';
import InnProjectManageGrid from './InnProjectManageGrid';
import { getProjectList, openProjectChapter, downProjectPrint } from './InnProjectManageService';
import InnProjectAddWindow from './InnProjectAddWindow';
import CheckBoxList from '../../../Components/Input/CheckBoxList';
import { getInnPlanDate } from '../InnAssignOrg/InnAssignOrgService'
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';

const InnProjectManageMain = () => {
    // 查詢條件開關控制
    const [queryConditionVisible, setQueryConditionVisible] = useState(false);
    // 查詢button文字
    const [showHideText, setShowHideText] = useState("篩選計畫");
    // Grid資料
    const [gridData, setGridData] = useState([]);
    // 查詢條件資料
    const [queryCondition, setQueryCondition] = useState({});
    // 預測查詢條件
    const defaultQueryCondition = useRef({});
    // 是否為初次載入
    const [isFirstLoad, setIsFirstLoad] = useState(true);
    // 機關計畫數
    const [projectOrgData, setProjectOrgData] = useState([]);
    //是否顯示Window
    const [projectAddVisible, setProjectAddVisible] = useState(false);
    //判斷頁面
    const [isManage, setIsManage] = useState(true);
    //年
    const [Year, setYear] = useState({});
    //截止日期
    const [closeDate, setCloseDate] = useState();
    //匯出檔案名
    const [exportFileName, setExportFileName] = useState('創新提案');


    // 已選取之計畫
    const checkedPlan = useRef([]);


    /**
     * 查詢資料異動
     * @param {{}} data 查詢條件
     * @param {boolean} isResetOrgTree 是否重新產生組織樹
     */
    const queryDataChange = async (data, isResetOrgTree = true) => {
        setQueryCondition({ ...data });
        let requestData = { ...data };
        let href = window.location.href;
        let isContains = href.includes("ProjectProposal")
        SetMaskOnOff(true);
        if (isContains) {
            requestData = {
                ...requestData,
                isProjectProposal: true
            }
        }
        if (requestData.GROUP == 'A') {
            setExportFileName('創新提案A組')
        } else if (requestData.GROUP == 'B') {
            setExportFileName('創新提案B組')
        }

        let result = await getProjectList(requestData);
        if (isFirstLoad) {
            defaultQueryCondition.current = { ...data };
            setIsFirstLoad(false);
        }
        setYear(data.INN_YEAR);

        //取得截止日期
        let closeDate = await getInnPlanDate((new Date().getFullYear() - 1911).toString())
        setCloseDate(closeDate.CLOSE_DATE)

        // 產生組織樹
        if (isResetOrgTree) {
            // 產生組織樹資料
            let projOrgPanelData = genOrgTreeData([...result]);
            setProjectOrgData([...projOrgPanelData])
        }

        setGridData(result)
        SetMaskOnOff(false)
    };


    /**
     * 組織樹查詢
     * @param {*} ouId 
     */
    const onSelect = async (ouId) => {
        let queryData = {
            ...queryCondition,
            OU_ID: ouId
        }

        await queryDataChange(queryData, false)
    }

    /**
     * 新增後更新grid及開章節
     * @param {*} planNO 
     */
    const saveEvent = async (planNO, planName) => {
        queryDataChange(queryCondition)
        let item = {
            PLAN_NO: planNO,
            PLAN_NAME: planName,
            IsManage: isManage
        }
        openProjectChapter(item);

        // 關閉視窗
        setProjectAddVisible(false);
    }

    // 匯出提案
    const exportPlan = async () => {
        let model = {
            Year: Year,
            INN_PLAN_NO: checkedPlan.current,
            FILE_NAME: exportFileName
        }
        downProjectPrint(model);
    }


    /**
     * 產生組織樹資料
     * @param {*} data 
     * @returns 
     */
    const genOrgTreeData = (data) => {
        // 取得計畫列表機關清單
        let projOrgPanelDataList = data.filter(proj => !IsNullOrEmpty(proj.OU_ID))
            .map(proj => {
                let projCnt = data.filter(x => x.OU_ID === proj.OU_ID).length;
                return {
                    OU_ID: proj.OU_ID,
                    title: proj.OU_NAME + '(' + projCnt + ')',
                    order: proj.OU_ID
                }
            });
        // 不同的 projOrgPanelDataList
        let distinctData = projOrgPanelDataList.filter((value, index, self) =>
            index === self.findIndex((t) => (
                t.OU_ID === value.OU_ID && t.OU_NAME === value.OU_NAME
            ))
        );
        distinctData.sort((a, b) => a.order - b.order);
        distinctData.unshift({ OU_ID: "", title: `桃園市政府(${data.length})` });
        return distinctData;
    }

    /**
     * 隱藏、顯示查詢條件 Event
     */
    const showHideQueryCondition = () => {
        setQueryConditionVisible(!queryConditionVisible)
        queryConditionVisible ? setShowHideText("篩選條件") : setShowHideText("隱藏篩選條件");
    }

    /**
    * 是否可以新增
    */
    const addPlan = async () => {
        if (!isManage) {
            if (!IsNullOrEmpty(closeDate)) {
                let currentDate = new Date();
                let closeDateTime = new Date(closeDate);
                if (currentDate < closeDateTime) {
                    setProjectAddVisible(true);
                } else {
                    showGlobalMessageBox("截止日期已過");
                }
            } else {
                showGlobalMessageBox("請先設定截止日期")
            }
        }
        else{
            setProjectAddVisible(true);
        }
    }


    //將勾選的資料放進checkedPlan
    const SetChecked = (e) => {
        if (e.value === true) {
            checkedPlan.current.push(e.dataItem.id);
        }
        else {
            checkedPlan.current.map((list, index) => {
                if (list === e.dataItem.id) {
                    checkedPlan.current.splice(index, 1);
                }
            });
        }
    }


    const checkBoxCell = props => {
        return (
            <td style={{ 'textAlign': 'center' }}>
                <CheckBoxList
                    group='CheckBoxList'
                    valueField='id'
                    data={[
                        { id: props.dataItem.INN_PLAN_NO, },
                    ]}
                    onChange={(e) => {
                        SetChecked(e);
                    }}
                />
            </td>
        )
    }

    let additionsCmdCols = [
        { cell: checkBoxCell, title: '選擇' },

    ]

    useEffect(() => {
        let href = window.location.href;
        let isContains = href.includes("ProjectProposal")
        if (isContains) {
            setIsManage(false)
        }
    })



    return (
        <>
            <h3 className="k-dialog-titlebar">{isManage ? '提案管理' : '提案登錄'}</h3>
            <div style={{ 'display': 'flex' }}>
                {isManage && (
                    <OrgSelectPanel
                        data={projectOrgData}
                        onSelect={onSelect}
                        title={''}
                        style={{ overflow: "auto" }}
                    />
                )}

                <PageContainer
                    toolbar={
                        <>
                            <Button title="隱藏篩選條件" className='k-button-lighten' onClick={showHideQueryCondition}>{showHideText}</Button>
                            <Button title="新增提案" className='k-button-lighten' onClick={addPlan}>新增提案</Button>
                            <Button title="列印提案" className='k-button-lighten' onClick={exportPlan}>匯出提案</Button>
                            <Button title="匯入創新提案" className='k-button-lighten' style={{ display: isManage ? 'inline-block' : 'none' }}>匯入創新提案</Button>
                            <Button title="重新整理" className='k-button-lighten' onClick={() => { queryDataChange(queryCondition) }}>重新整理</Button>
                        </>
                    }
                    style={{ height: "calc(100vh - 129px)" }}
                >
                    <div style={{ display: queryConditionVisible ? 'inline' : 'none' }}>
                        <InnProjectManageQueryForm
                            visible={queryConditionVisible}
                            queryData={queryDataChange}
                        />
                    </div>

                    <InnProjectManageGrid
                        data={gridData} // 資料顯示格式
                        additionalCmdCols={additionsCmdCols}
                        isManage={isManage}// 用來判斷管理還是提案
                        closeDate={closeDate}
                        reloadGrid={() => {
                            queryDataChange(queryCondition)
                        }}

                    />
                </PageContainer>

                <InnProjectAddWindow
                    visible={projectAddVisible}
                    onClose={() => { setProjectAddVisible(false); queryDataChange(queryCondition) }}
                    saveEvent={(planNO, planName) => saveEvent(planNO, planName)}
                />
            </div>
        </>
    );
}

export default InnProjectManageMain;
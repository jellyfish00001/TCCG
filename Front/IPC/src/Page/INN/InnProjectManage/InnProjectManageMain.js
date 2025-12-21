
import React, { useEffect, useRef, useState } from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import OrgSelectPanel from '../../../Components/Selector/OrgSelectPanel';
import { Button } from "@progress/kendo-react-buttons";
import InnProjectManageQueryForm from './InnProjectManageQueryForm';
import InnProjectManageGrid from './InnProjectManageGrid';


const InnProjectManageMain = ({
    location: { state },
}) => {

    // 查詢條件開關控制
    const [queryConditionVisible, setQueryConditionVisible] = useState(false);
    // 查詢條件初始化次數(用來控制Form Component 初始化查詢條件)
    const [defaultQueryConditionCnt, setDefaultQueryConditionCnt] = useState(0);
    // 查詢button文字
    const [showHideText, setShowHideText] = useState("篩選計畫");
    // Grid資料
    const [gridData, setGridData] = useState([]);
    // 查詢條件資料
    const [queryCondition, setQueryCondition] = useState({});
    // 預測查詢條件
    const defaultQueryCondition = useRef({});
    // 匯出Grid資料
    const GridDataForExport = React.useRef({ props: {}, Columns: [] })
    // 已選取之計畫
    const checkedPlan = useRef([]);
    // 是否為初次載入
    const [isFirstLoad, setIsFirstLoad] = useState(true);
    // 機關計畫數
    const [projectOrgData, setProjectOrgData] = useState([])

    /**
     * 查詢資料異動
     * @param {{}} data 查詢條件
     * @param {boolean} isResetOrgTree 是否重新產生組織樹
     */
    const queryDataChange = async (data, isResetOrgTree = true) => {
        setQueryCondition({ ...data });
        let requestData = { ...data };
        SetMaskOnOff(true);

        if (isFirstLoad) {
            defaultQueryCondition.current = { ...data };
            setIsFirstLoad(false);
        }

        setGridData()
        SetMaskOnOff(false)
    };

    const loadData = async () => {
        const fakeData = Array.from({ length: 3}, (_, index) => ({
            PLAN_NO:  index + 1,
            PLAN_NAME:'test',
            PLAN_CLASS: 'test',
            PLAN_MAN: 'test',
            PLAN_ORG: 'test',
        })); 
        setGridData(fakeData);
    }

    useEffect(() => {
        loadData(gridData);
    }, [])


    // 執行機關選擇事件callback
    const onSelect = async (ouId) => {
        let queryData = {
            ...queryCondition,
            EXEC_DEPT: ouId
        }

        // 清空已勾選計畫
        checkedPlan.current = [];
        await queryDataChange(queryData, false)
    }

    //隱藏、顯示查詢條件 Event
    const showHideQueryCondition = () => {
        setQueryConditionVisible(!queryConditionVisible)
        queryConditionVisible ? setShowHideText("篩選條件") : setShowHideText("隱藏篩選條件");
    }

    //#endregion

    return (
        <>
            <h3 className="k-dialog-titlebar">提案管理</h3>
            <div style={{ 'display': 'flex' }}>
                <OrgSelectPanel
                    data={projectOrgData}
                    onSelect={onSelect}
                    title={''}
                    style={{ overflow: "auto" }}
                />

                <PageContainer
                    toolbar={
                        <>
                            <Button title="隱藏篩選條件" className='k-button-lighten' onClick={showHideQueryCondition}>{showHideText}</Button>
                            <Button title="新增提案" className='k-button-lighten' onClick={() => { }}>新增提案</Button>
                            <Button title="匯入創新提案" className='k-button-lighten' >匯入創新提案</Button>
                        </>
                    }
                    style={{ height: "calc(100vh - 129px)" }}
                >
                    <div style={{ display: queryConditionVisible ? 'inline' : 'none' }}>
                        <InnProjectManageQueryForm
                            visible={queryConditionVisible}
                            queryData={queryDataChange}
                            defaultQueryConditionCnt={defaultQueryConditionCnt}
                        />
                    </div>

                    <InnProjectManageGrid
                        data={gridData} // 資料顯示格式
                        GridDataForExport={GridDataForExport} // 匯出物件格式
                        reloadGrid={() => {
                            //queryDataChange(queryCondition) 
                        }}

                    />
                </PageContainer>
            </div>
        </>
    );
}

export default InnProjectManageMain;
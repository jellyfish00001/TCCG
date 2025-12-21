import React from 'react';
import GotoTopBtn from '../../../Components/Utils/GotoTopBtn';
import { GetHistory } from '../../../Basic/BasicData';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { CheckIsHANDRole } from '../../../Basic/CommonService';
import { Button } from "@progress/kendo-react-buttons";

import Service from './SuperiorAnalysisService';
import AnalyzeQueryForm from './AnalyzeQueryForm';
import StatCntChart from './Chart/StatCntChart';
import StatBudgetAmtChart from './Chart/StatBudgetAmtChart';
import StatCntPieChart from './Chart/StatCntPieChart';
import QueryConditions from './QueryConditions';

const SuperiorAnalysisMain = (props) => {
    const state = props.location.state;

    const tempFormData = {
        CP_KIND: "", // 案件類型
        CP_KIND_DESC: "總覽", // 案件名稱類型
        IS_PROJECT_FINISH: "", // 結案狀態
        IS_PROJECT_FINISH_DESC: "全部", //結案狀態名稱
        PROJ_BUDGET: "", // 計畫經費,
        PROJ_BUDGET_DESC: "全部",// 計畫經費名稱
        CHART_TYPE: 0, // 圖表類別
        CHART_TYPE_DESC: "建設類別",//圖表類別名稱
        MASTER_DEPT: "", // 主管機關
        MASTER_DEPT_DESC: "全部",
        EXEC_DEPT: "", // 執行機關
        EXEC_DEPT_DESC: "全部",
        IS_HAND_ROLE: false, // 是否只有主辦權限

        MAIN_TYPE: "", // 主頁點擊的型態
        MAIN_VALUE: "", // 主頁點擊的值
        IS_GET_MATCH: null, // 是否取得符合 Null:無 True:符合 False:落後
    };
    const [formData, setFormData] = React.useState(tempFormData);

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState({
        MASTER_ORGAN: [], // 主管機關
        EXEC_ORGAN: [], // 執行機關
    });
    // 是否顯示內容
    const [isVisible, setIsVisible] = React.useState(false);
    // 計畫件數資料
    const [cntData, setCntData] = React.useState({ Categories: [], Data: [] });
    // 計畫金額資料
    const [budgetAmtData, setBudgetAmtData] = React.useState({ Categories: [], Data: [] });
    // 進度圓餅圖
    const [statCntpieData, setStatCntPieData] = React.useState([]);
    // 分類名稱
    let categoryName = React.useRef("");
    // Formik innerRef
    const formRef = React.useRef(null);

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = async () => {
        await getAllDropDowns();
        await setStateData();
    }

    // 取得下拉清單
    const getAllDropDowns = async () => {
        SetMaskOnOff(true);
        const checkIsHANDRole = await CheckIsHANDRole();
        const data = await Service.getOrganData(checkIsHANDRole);
        setDdlData({ ...ddlData, MASTER_ORGAN: data, EXEC_ORGAN: data });
        setFormData({ ...formData, IS_HAND_ROLE: checkIsHANDRole });
        SetMaskOnOff(false);
    }

    // 設定原本條件
    const setStateData = async () => {
        if (state) {
            getData(state.mainFormData);
        }
    }

    // 查詢
    const onsubmit = async () => {
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    }

    // 取資料
    const getData = async (data) => {
        SetMaskOnOff(true);
        setIsVisible(false);
        setFormData({ ...data });
        // 分析類別-進度 要顯示件數圓餅圖；其他顯示件數、金額長條圖
        if (data.CHART_TYPE === 3) {
            setStatCntPieData([...await Service.getPieChartStat(data)]);
        }
        else {
            const rdata = await Service.getAnalysis(data);
            setCntData({ ...Service.getStatCnt(rdata.StatCnts) });
            setBudgetAmtData({ ...Service.getStatBudgetAmt(rdata.StatBudgetAmts) });
        }

        setIsVisible(true);
        SetMaskOnOff(false);
    }

    // 清除
    const onClear = () => {
        if (formRef.current) {
            formRef.current.handleReset();
            setIsVisible(false);
            setFormData({ ...tempFormData });
        }
    }

    // 長條圖 狀體 click 事件
    const ChartItemClick = (id, name) => {
        categoryName.current = name;
        let data = formData;
        data.MAIN_TYPE = `${data.CHART_TYPE}`;
        data.MAIN_VALUE = id;

        GetHistory().push('/Home/SuperiorAnalysis/SuperiorAnalysisDetailMain', {
            categoryName: categoryName.current,
            mainFormData: data
        });
    }

    // 圓餅圖 click 事件
    const ChartPieItemClick = (isGetMatch) => {
        categoryName.current = "進度";

        let data = formData;
        data.IS_GET_MATCH = isGetMatch;

        GetHistory().push('/Home/SuperiorAnalysis/SuperiorAnalysisDetailMain', {
            categoryName: categoryName.current,
            mainFormData: data
        });
    }

    return (
        <>
            <div
                className="SuperiorAnalysis" style={{ overflow: "auto", height: "100%" }}
            >
                <h3 className="k-dialog-titlebar">決策分析 – 重大建設分析</h3>
                <div className='fn-buttons'>
                    <Button className='k-button-lighten' onClick={() => onsubmit()}>查詢</Button>
                    <Button className='k-button-lighten' onClick={() => onClear()}>清除</Button>
                    <Button className='k-button-lighten' onClick={() => window.print()}>列印</Button>
                </div>

                <AnalyzeQueryForm
                    ddlData={ddlData}
                    formData={formData}
                    formRef={formRef}
                    getData={getData}
                />

                <QueryConditions data={formData} />

                {
                    isVisible && formData.CHART_TYPE !== 3 &&
                    <>
                        {/* 計畫件數長條圖 */}
                        <h3>計畫件數(單位：件)</h3>
                        <StatCntChart
                            model={cntData}
                            ChartItemClick={ChartItemClick}
                        />

                        {/* 計畫金額長條圖 */}
                        <h3>計畫金額(單位：千元)</h3>
                        <StatBudgetAmtChart
                            model={budgetAmtData}
                            ChartItemClick={ChartItemClick}
                        />
                    </>
                }

                {
                    isVisible && formData.CHART_TYPE === 3 &&
                    <>
                        {/* 計畫件數圓餅圖 */}
                        <h3>計畫件數(單位：件)</h3>
                        <StatCntPieChart
                            pieData={statCntpieData}
                            ChartItemClick={ChartPieItemClick}
                        />
                    </>
                }
            </div>
            <GotoTopBtn targetClass="SuperiorAnalysis" />
        </>
    )
}

export default SuperiorAnalysisMain;
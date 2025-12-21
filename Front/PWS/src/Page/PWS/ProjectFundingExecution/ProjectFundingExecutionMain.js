import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import { SaveFundingExecution, GetGridData, ExecutionValidation, FundingValidation } from './ProjectFundingExecutionService';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import FundingGrid from './FundingGrid';
import ExecutionGrid from './ExecutionGrid';
import * as Yup from 'yup';

const FundingDetailsMain = (props) => { 
    // 外部傳入
    const { 
        location: { 
            state: {
                projectNo,
                projectYear,
                projectIsSend,
            }
        }
     } = props;
     // 經費需求細項
     const [fundingGridData, setFundingGridData] = useState([]);
     // 歷年執行情形
     const [executionGridData, setExecutionGridData] = useState([]);
     const exeGridRef = useRef();
     // 總經費
     const total = useRef(0)

    /**
     * 取計畫資料
     * @returns 
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let data = await GetGridData(projectNo, projectYear);
        total.current = data.TOTAL;
        setExecutionGridData(data.budgetExecListModel);
        setFundingGridData(data.DAMTBListModel);
        SetMaskOnOff(false);
    }

    // 取頁面資料
    useEffect(() => {
        loadData();
    }, []);

    /**
     * 取消
     * @returns 
     */
    const clean = () => {
        loadData();
    }    

    // 檢驗
    const validate = async () => {
        // 驗證
        try {
            await Yup.object().shape({
                fundingGridData: FundingValidation,
                executionGridData: ExecutionValidation
            }).validate({ fundingGridData, executionGridData }, { abortEarly: false });
        } catch (error) {
            if (error instanceof Yup.ValidationError) {
                // 建立一個包含所有錯誤訊息的數組
                const errorMessages = error.inner.map(err => err.message);
                // 顯示錯誤訊息數組中的第一個錯誤訊息
                showGlobalMessageBox(errorMessages[0]);
            } else {
                // 如果錯誤不是由 Yup 引起的，則可能需要其他的錯誤處理邏輯
                showGlobalMessageBox("驗證時發生錯誤");
            }
            return;
        }
        // 驗證計算總計
        const fundingTotal = fundingGridData.reduce((sum, item) => sum + Number(item.FUNDTOT), 0);
        if (fundingTotal !== total.current) {
            showGlobalMessageBox('總計與計畫需求不相符', () => {save();});
            return;
        }
        save();
    }

    /**
     * 存檔
     * @returns 
     */
    const save = async () => {
        SetMaskOnOff(true);
        // 給經費明細
        const DAMTBListModel = fundingGridData.map(row => ({
            ...row,
            PLANNO: projectNo,
        }));
        // 給歷年執行情形明細
        const budgetExecListModel = executionGridData.map(row => ({
            ...row,
            PLANNO: projectNo,
        }));
        budgetExecListModel[2].GROWRATIO = 0;
        // 將計畫ID一起傳入
        const dataTosave = {
            PLANNO: projectNo,
            DAMTBListModel: DAMTBListModel,
            budgetExecListModel: budgetExecListModel
        };
        let saveResult = await SaveFundingExecution(dataTosave);
        SetMaskOnOff(false);
        
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => window.location.reload());
        }
    }

    return (
        <>
            <PageContainer style={{ overflow: "auto", height: "100%" }}>
                <CollapseBoardCard
                    button={
                        <>
                            {!projectIsSend &&
                                <>
                                    <Button title="存檔" type="button" onClick={validate} >存檔</Button>
                                    <Button title="取消" className="k-button-lighten" type="button" onClick={clean} >取消</Button>
                                </>
                            }
                        </>
                    }
                title="經費需求事項"
                isFirstArea={true}
                > 
                <FundingGrid
                    fundingGridData={fundingGridData}
                    setFundingGridData={setFundingGridData}
                />
                </CollapseBoardCard>
                <span>當年度經費 {total.current} 千元</span>
                <CollapseBoardCard title="歷年執行情形" initValue={true}>
                    <ExecutionGrid
                        projectYear={projectYear}
                        executionGridData={executionGridData}
                        exeGridRef={exeGridRef}
                        setExecutionGridData={setExecutionGridData}
                    />
                </CollapseBoardCard>
            </PageContainer>
        </>
    );
}
export default FundingDetailsMain;


import React, { useState, useRef, useEffect } from "react";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';

export const CAddNewPlanGrid = (props) => {
    const { 
        totalMoney,     // 總經費
        toatalMoneyRef,  // 紀錄總經費
        PLANYEAR,       // 年度
        gridChange,  // 追蹤grid資料
        isCrossYear  // 是否跨年度
    } = props;
    
    // Grid資料
    const [crossYearData, setCrossYearData] = useState([]);
    /**
     * 計算經費
     * @param {number} planYear 年度
     * @param {number} totalMoney 總經費
     * @param {array} gridChange 追蹤grid資料
     * @param {boolean} isCrossYear 是否跨年度
     * @returns {void}
     */
    const calculateFunds = () => {
        // 傳入的年度(與原本年度做區分)
        let planYear = PLANYEAR;
        let countData = gridChange.map(item => ({ ...item, PLANYEAR: item.PLANYEAR }));
        let beforeMoney = 0;
        let afterMoney = 0;
        let nowMoney = totalMoney;
        //計算經費
        if (countData) {
            beforeMoney = countData.filter(item => item.PLANYEAR < planYear).reduce((a, b) => a + b.BUDGETCENTRAL+b.BUDGETLOCAL, 0);
            nowMoney += countData.filter(item => item.PLANYEAR == planYear).reduce((a, b) => a + b.BUDGETCENTRAL+b.BUDGETLOCAL, 0);
            afterMoney = countData.filter(item => item.PLANYEAR > planYear).reduce((a, b) => a + b.BUDGETCENTRAL+b.BUDGETLOCAL, 0);
        }
        if(!isCrossYear){
            toatalMoneyRef.current = nowMoney;
            setCrossYearData([{ PLANYEAR: planYear + "年", MONEY: nowMoney }]);
        }
        else {
            toatalMoneyRef.current = beforeMoney + nowMoney + afterMoney;
            // 設定Grid資料
            setCrossYearData([
                { PLANYEAR: planYear - 1 + "年以前", MONEY: beforeMoney },
                { PLANYEAR: planYear + "年", MONEY: nowMoney },
                { PLANYEAR: planYear + 1 + "年以後", MONEY: afterMoney },
                { PLANYEAR: "總和", MONEY: beforeMoney + nowMoney + afterMoney }
            ]);
        }
    };

    // 計算經費
    useEffect(() => {
        if(PLANYEAR){
            calculateFunds();
        }
    }, [PLANYEAR, totalMoney, gridChange, isCrossYear]);

    return (
        <>
            <Grid
                style={{height: '100%',overflow: 'auto',}}
                resizable={true}
                data={crossYearData}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="PLANYEAR" title="年度"/>      
                <GridColumn field="MONEY" title="經費(千元)"/>
            </Grid>
        </>
    )
    }
export default CAddNewPlanGrid;
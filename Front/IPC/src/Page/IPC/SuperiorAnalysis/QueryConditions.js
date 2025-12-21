import React from "react";

const QueryConditions = (props) => {
    const { data } = props;

    return (
        <div>
            <h3>
                執行方式：{data.CP_KIND_DESC}&emsp;結案狀態：{data.IS_PROJECT_FINISH_DESC}&emsp;計畫經費：{data.PROJ_BUDGET_DESC}&emsp;
                分析類別：{data.CHART_TYPE_DESC}&emsp;主管機關：{data.MASTER_DEPT_DESC}&emsp;執行機關：{data.EXEC_DEPT_DESC}
            </h3>
        </div>
    )
}

export default QueryConditions;
import React from 'react';
import { renderRoutes } from 'react-router-config';

import Home from '../HomePage/Home'; /*主頁*/
import ProjectListAllMain from '../Page/IPC/ProjectListAll/ProjectListAllMain';   /*關於*/

/*
利用lazy loading及dynamic動態載入named export components
ex:const Announcement = React.lazy(() =>
  import("../Page/Announce/Announcement/Announcement").then(importedModule => ({
    default: importedModule.Announcement
  }))
);
如果僅是default export僅需使用React.lazy引用即可
ex: const OtherComponent = React.lazy(() => import('./OtherComponent'));
*/

const DemoRPT = React.lazy(
    () => import(/* webpackChunkName: "DemoRPT" */
        '../Page/IPC/DemoRPT/DemoRPT'));

const SetCodeMain = React.lazy(
    () => import(/* webpackChunkName: "SetCode" */
        '../Page/IPC/CodeMaintain/SetCodeMain'));

const PlanImport = React.lazy(
    () => import(/* webpackChunkName: "PlanImport" */
        '../Page/IPC/PlanImport/PlanImportMain'));

const ProjectFillBasicMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillBasic" */
        '../Page/IPC/ProjectFillBasic/ProjectFillBasicMain'));

const ProjectFillCheckPoint = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillCheckPoint" */
        '../Page/IPC/ProjectFillCheckPoint/ProjectFillCheckPointMain'));

const ProjectFillFileUpMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillFileUp" */
        '../Page/IPC/ProjectFillFileUp/ProjectFillFileUpMain'));

const ProjectFillAddSubmitMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillAddSubmit " */
        '../Page/IPC/ProjectFillAddSubmit/ProjectFillAddSubmitMain'));

const ProjectFillAddAuditMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillAddAudit " */
        '../Page/IPC/ProjectFillAddAudit/ProjectFillAddAuditMain'));

const ProjectFillRefFileMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillRefFile" */
        '../Page/IPC/ProjectFillRefFile/ProjectFillRefFileMain'));

const ProjectFillCkptComMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillCkptCom" */
        '../Page/IPC/ProjectFillCkptCom/ProjectFillCkptComMain'));

const SetContactMain = React.lazy(
    () => import(/* webpackChunkName: "SetContact" */
        '../Page/IPC/SetContact/SetContactMain'));

const ProjectListtExecMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectListtExec" */
        '../Page/IPC/ProjectListtExec/ProjectListtExecMain'));

const ProjectAdjustListMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustListExec" */
        '../Page/IPC/ProjectAdjustListExec/ProjectAdjustListExecMain'));

const ProjectAdjustListtReviewMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustListAudit" */
        '../Page/IPC/ProjectAdjustListReview/ProjectAdjustListReviewMain'));

const ProjectAdjustBasicReason = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustBasic" */
        '../Page/IPC/ProjectAdjustBasic/BasicExecReason/BasicExecReasonMain'));

const ProjectAdjustBasicData = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustBasic" */
        '../Page/IPC/ProjectAdjustBasic/BasicExecData/BasicExecDataMain'));

const ProjectAdjustBasicCheck = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustBasic" */
        '../Page/IPC/ProjectAdjustBasic/BasicExecCheck/BasicExecCheckMain'));

const ProjectAdjustScheduleReason = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustBasic" */
        '../Page/IPC/ProjectAdjustSchedule/ScheduleExecReason/ScheduleExecReasonMain'));

const ProjectAdjustScheduleCheckPoint = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustBasic" */
        '../Page/IPC/ProjectAdjustSchedule/ScheduleExecCheckPoint/ScheduleExecCheckPointMain'));

const ProjectAdjustScheduleCheck = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustBasic" */
        '../Page/IPC/ProjectAdjustSchedule/ScheduleExecCheck/ScheduleExecCheckMain'));

const ProjectAdjustRevokeReview = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustRevokeReview" */
        '../Page/IPC/ProjectAdjustRevokeReview/ProjectAdjustRevokeReviewMain'));

const ProjectAdjustBasicReview = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustRevokeReview" */
        '../Page/IPC/ProjectAdjustBasicReview/ProjectAdjustBasicReviewMain'));

const ProjectAdjustScheduleReview = React.lazy(
    () => import(/* webpackChunkName: "ProjectAdjustScheduleReview" */
        '../Page/IPC/ProjectAdjustScheduleReview/ProjectAdjustScheduleReviewMain'));

const ProjectListtReviewMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectListtReview" */
        '../Page/IPC/ProjectListtReview/ProjectListtReviewMain'));

const ProjectFillOtherMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillOther" */
        '../Page/IPC/ProjectFillOther/ProjectFillOtherMain'));

const ProjectFillExecuteMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillExecute" */
        '../Page/IPC/ProjectFillExecute/ProjectFillExecuteMain'));

const ProjectFillDelayMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillDelay" */
        '../Page/IPC/ProjectFillDelay/ProjectFillDelayMain'));

const ProjectFillExecuteSubmitMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillExecuteSubmit" */
        '../Page/IPC/ProjectFillExecuteSubmit/ProjectFillExecuteSubmitMain'));

const ProjectFillCloseMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillClose" */
        '../Page/IPC/ProjectFillClose/ProjectFillCloseMain'));

const ProjectFillCloseAuditMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillCloseAudit" */
        '../Page/IPC/ProjectFillClose/ProjectFillCloseAuditMain'));

const ProjectFillAuditMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillAudit" */
        '../Page/IPC/ProjectFillAudit/ProjectFillAuditMain'));

const ProjectFillFieldMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillField" */
        '../Page/IPC/ProjectFillField/ProjectFillFieldMain'));

const ProjectFillBudgetExecMain = React.lazy(
    () => import(/* webpackChunkName: "ProjectFillBudgetExec" */
        '../Page/IPC/ProjectFillBudgetExec/ProjectFillBudgetExecMain'));

const MailSet = React.lazy(
    () => import(/* webpackChunkName: "Mail" */
        '../Page/IPC/MailSet/MailSetMain'));

const SuperiorAnalysisMain = React.lazy(
    () => import(/* webpackChunkName: "SuperiorAnalysis" */
        '../Page/IPC/SuperiorAnalysis/SuperiorAnalysisMain'));

const SuperiorAnalysisDetailMain = React.lazy(
    () => import(/* webpackChunkName: "SuperiorAnalysisDetail" */
        '../Page/IPC/SuperiorAnalysis/SuperiorAnalysisDetailMain'));

const SuperiorProgessMain = React.lazy(
    () => import(/* webpackChunkName: "SuperiorProgess" */
        '../Page/IPC/SuperiorProgess/SuperiorProgessMain'));

const SuperiorRegionMain = React.lazy(
    () => import(/* webpackChunkName: "SuperiorRegion" */
        '../Page/IPC/SuperiorRegion/SuperiorRegionMain'));

const SuperiorDelayPlanMain = React.lazy(
    () => import(/* webpackChunkName: "SuperiorDelayPlan" */
        '../Page/IPC/SuperiorDelayPlan/SuperiorDelayPlanMain'));

const StatisticsList = React.lazy(
    () => import(/* webpackChunkName: "Statistics" */
        '../Page/IPC/Statistics/StatisticsList'));

const StatisticsQuery = React.lazy(
    () => import(/* webpackChunkName: "Statistics" */
        '../Page/IPC/Statistics/StatisticsQuery'));

const UnitingQueryMain = React.lazy(
    () => import(/* webpackChunkName: "UnitingQuery" */
        '../Page/IPC/UnitingQuery/UnitingQueryMain'));

const UnitingQueryGrid = React.lazy(
    () => import(/* webpackChunkName: "UnitingQuery" */
        '../Page/IPC/UnitingQuery/UnitingQueryGrid'));

const PageNotFound = React.lazy(
    () => import(/* webpackChunkName: "PageNotFound" */
        '../Basic/PageNotFound'));

const PageLoading = React.lazy(
    () => import(/* webpackChunkName: "PageLoading" */
        '../Basic/PageLoading'));

const GDBSync = React.lazy(
    () => import(/* webpackChunkName: "GDBSync" */
        '../Page/IPC/GDBSync/GDBSync'));

const GDBXls = React.lazy(
    () => import(/* webpackChunkName: "GDBXls" */
        '../Page/IPC/GDBXls/GDBXls'));

const GenerateMonthlyDAB = React.lazy(
    () => import(/* webpackChunkName: "GenerateMonthlyDAB" */
        '../Page/IPC/DashBoard/GenerateMonthlyDAB'));

const DashBoardSummary = React.lazy(
    () => import(/* webpackChunkName: "DashBoardSummary" */
        '../Page/IPC/DashBoard/Summary/DashBoardSummary'));

const DashBoardExecution = React.lazy(
    () => import(/* webpackChunkName: "DashBoardExecution" */
        '../Page/IPC/DashBoard/Summary/DashBoardExecution'));

const DashBoardProgress = React.lazy(
    () => import(/* webpackChunkName: "DashBoardProgress" */
        '../Page/IPC/DashBoard/Progress/DashBoardProgress'));

const DashBoardCountAndBudget = React.lazy(
    () => import(/* webpackChunkName: "DashBoardProgress" */
        '../Page/IPC/DashBoard/CountAndBudget/CountAndBudgetMain'));

const DashBoardHistoryMain = React.lazy(
    () => import(/* webpackChunkName: "DashBoardHistoryMain" */
        '../Page/IPC/DashBoard/History/DashBoardHistoryMain'));

const HomeRoute = () => {
    let route = []
    routes[0].routes.forEach(p => { route = route.concat(p.routes ? p.routes : p) });

    return (
        <Home>
            {renderRoutes(route)}
        </Home>
    );
}

export default HomeRoute;

export const routes = [
    {
        path: "/", breadcrumbName: '待辦清單', routes: [
            { exact: true, path: "/Home/", component: ProjectListAllMain, breadcrumbName: '待辦清單' },
            { exact: true, path: "/ProjectChapter/", component: PageLoading, breadcrumbName: '章節表' },
            {
                path: "/Home/", breadcrumbName: '計畫填報', routes: [
                    { exact: true, path: "/Home/ProjectListtExec/ProjectListtExecMain", component: ProjectListtExecMain, breadcrumbName: '資料登錄' },
                    { exact: true, path: "/Home/ProjectAdjustList/Exec", component: ProjectAdjustListMain, breadcrumbName: '調整撤銷' },
                ]
            },
            {
                path: "/Home/", breadcrumbName: '計畫審核', routes: [
                    { exact: true, path: "/Home/ProjectListtReview/ProjectListtReviewMain", component: ProjectListtReviewMain, breadcrumbName: '計畫審查' },
                    { exact: true, path: "/Home/ProjectAdjustList/Audit", component: ProjectAdjustListtReviewMain, breadcrumbName: '調整撤銷' },
                ]
            },
            {
                path: "/Home/", breadcrumbName: '查詢', routes: [
                    { exact: true, path: "/Home/Statistics/StatisticsList", component: StatisticsList, breadcrumbName: '統計報表清單' },
                    { exact: true, path: "/Home/Statistics/StatisticsQuery", component: StatisticsQuery, breadcrumbName: '統計報表查詢' },
                    { exact: true, path: "/Home/UnitingQuery/UnitingQueryMain", component: UnitingQueryMain, breadcrumbName: '綜合查詢' },
                    { exact: true, path: "/Home/UnitingQuery/UnitingQueryGrid", component: UnitingQueryGrid, breadcrumbName: '綜合查詢結果' }
                ]
            },
            {
                path: "/Home/", breadcrumbName: '決策分析', routes: [
                    /* 重大建設分析 */
                    { exact: true, path: "/Home/SuperiorAnalysis/SuperiorAnalysisMain", component: SuperiorAnalysisMain, breadcrumbName: '重大建設分析' },
                    { exact: true, path: "/Home/SuperiorAnalysis/SuperiorAnalysisDetailMain", component: SuperiorAnalysisDetailMain, breadcrumbName: '重大建設分析明細' },
                    /* 建設類別查詢 */
                    { exact: true, path: "/Home/SuperiorProgess/SuperiorProgessMain", component: SuperiorProgessMain, breadcrumbName: '建設類別查詢' },
                    /* 區域統計分析 */
                    { exact: true, path: "/Home/SuperiorRegion/SuperiorRegionMain", component: SuperiorRegionMain, breadcrumbName: '區域統計分析' },
                    /* 落後案件查詢 */
                    { exact: true, path: "/Home/SuperiorDelayPlan/SuperiorDelayPlanMain", component: SuperiorDelayPlanMain, breadcrumbName: '落後案件查詢' }
                ],
            },
            {
                path: "/Home/DashBoard/", breadcrumbName: "儀錶板", routes: [
                    { exact: true, path: "/Home/DashBoard/Summary", component: DashBoardSummary, breadcrumbName: '重要儀錶板' },
                    { exact: true, path: "/Home/DashBoard/Execution", component: DashBoardExecution, breadcrumbName: '執行中列管情形' },
                    { exact: true, path: "/Home/DashBoard/Progress", component: DashBoardProgress, breadcrumbName: '重大工程進度' },
                    { exact: true, path: "/Home/DashBoard/CountAndBudget", component: DashBoardCountAndBudget, breadcrumbName: '件數及經費情形' },
                    { exact: true, path: "/Home/DashBoard/History", component: DashBoardHistoryMain, breadcrumbName: '歷年列管情形' },

                ]
            },
            /* 系統管理 */
            {
                path: "/Home/Sysop/", breadcrumbName: '設定', routes: [
                    { exact: true, path: "/Home/Sysop/SetCode", component: SetCodeMain, breadcrumbName: '代碼維護' },
                    { exact: true, path: "/Home/Sysop/PlanImport", component: PlanImport, breadcrumbName: '計畫匯入' },
                    { exact: true, path: "/Home/Sysop/SetContactMain", component: SetContactMain, breadcrumbName: '機關窗口維護' },
                    { exact: true, path: "/Home/Sysop/ProjectFillRefFileMain", component: ProjectFillRefFileMain, breadcrumbName: '參考資料' },
                    { exact: true, path: "/Home/Sysop/MailSet", component: MailSet, breadcrumbName: '郵件設定' },
                    { exact: true, path: "/Home/Sysop/GDBSync", component: GDBSync, breadcrumbName: '工程標案同步' },
                    { exact: true, path: "/Home/Sysop/GDBXls", component: GDBXls, breadcrumbName: '工程標案Xls' },
                    { exact: true, path: "/Home/Sysop/GenerateMonthlyDAB", component: GenerateMonthlyDAB, breadcrumbName: '產生儀錶板資料' }
                ],
            },
            {
                path: "/ProjectChapter/ProjectFillBasic/", breadcrumbName: '計畫基本資料', routes: [
                    { exact: true, path: "/ProjectChapter/ProjectFillBasic/ProjectFillBasicMain", component: ProjectFillBasicMain, breadcrumbName: '計畫基本資料' },
                    { exact: true, path: "/ProjectChapter/ProjectFillCheckPoint/ProjectFillCheckPointMain", component: ProjectFillCheckPoint, breadcrumbName: '檢查點設定' },
                    { exact: true, path: "/ProjectChapter/ProjectFillFileUp/ProjectFillFileUpMain", component: ProjectFillFileUpMain, breadcrumbName: '相關檔案上傳' },
                    { exact: true, path: "/ProjectChapter/ProjectFillAddSubmit/ProjectFillAddSubmitMain", component: ProjectFillAddSubmitMain, breadcrumbName: '立案送審' },
                    { exact: true, path: "/ProjectChapter/ProjectFillAddAudit/ProjectFillAddAuditMain", component: ProjectFillAddAuditMain, breadcrumbName: '立案審核' },
                ],
            },
            {
                path: "/ProjectChapter/ProjectExecute/", breadcrumbName: '計畫執行情形', routes: [
                    { exact: true, path: "/ProjectChapter/ProjectFillCkptCom/ProjectFillCkptComMain", component: ProjectFillCkptComMain, breadcrumbName: '檢核點完成日期' },
                    { exact: true, path: "/ProjectChapter/ProjectFillExecute/ProjectFillExecuteMain", component: ProjectFillExecuteMain, breadcrumbName: '每月辦理情形' },
                    { exact: true, path: "/ProjectChapter/ProjectFillDelay/ProjectFillDelayMain", component: ProjectFillDelayMain, breadcrumbName: '落後原因分析' },
                    { exact: true, path: "/ProjectChapter/ProjectFillOther/ProjectFillOtherMain", component: ProjectFillOtherMain, breadcrumbName: '其他資訊' },
                    { exact: true, path: "/ProjectChapter/ProjectFillExecuteSubmit/ProjectFillExecuteSubmitMain", component: ProjectFillExecuteSubmitMain, breadcrumbName: '執行情形送出' },
                    { exact: true, path: "/ProjectChapter/ProjectFillClose/ProjectFillCloseMain", component: ProjectFillCloseMain, breadcrumbName: '結案資料' },
                    { exact: true, path: "/ProjectChapter/ProjectFillClose/ProjectFillCloseAuditMain", component: ProjectFillCloseAuditMain, breadcrumbName: '結案審核' },
                    { exact: true, path: "/ProjectChapter/ProjectFillAudit/ProjectFillAuditMain", component: ProjectFillAuditMain, breadcrumbName: '管考備註' },
                    { exact: true, path: "/ProjectChapter/ProjectFillField/ProjectFillFieldMain", component: ProjectFillFieldMain, breadcrumbName: '實地查證情形' },
                    { exact: true, path: "/ProjectChapter/ProjectFillBudgetExec/ProjectFillBudgetExecMain", component: ProjectFillBudgetExecMain, breadcrumbName: '預算執行情形' },
                ]
            },
            /* 調整撤銷 */
            {
                path: "/ProjectChapter/ProjectAdjust/", breadcrumbName: '調整撤銷', routes: [
                    { exact: true, path: "/ProjectChapter/ProjectAdjust/Basic/Reason", component: ProjectAdjustBasicReason, breadcrumbName: '基本資料調整事由' },
                    { exact: true, path: "/ProjectChapter/ProjectAdjust/Basic/Data", component: ProjectAdjustBasicData, breadcrumbName: '基本資料調整' },
                    { exact: true, path: "/ProjectChapter/ProjectAdjust/Basic/Check", component: ProjectAdjustBasicCheck, breadcrumbName: '基本資料調整送審' },
                    { exact: true, path: "/ProjectChapter/ProjectAdjust/Schedule/Reason", component: ProjectAdjustScheduleReason, breadcrumbName: '期程調整事由' },
                    { exact: true, path: "/ProjectChapter/ProjectAdjust/Schedule/CheckPoint", component: ProjectAdjustScheduleCheckPoint, breadcrumbName: '檢核點調整' },
                    { exact: true, path: "/ProjectChapter/ProjectAdjust/Schedule/Check", component: ProjectAdjustScheduleCheck, breadcrumbName: '期程調整送審' },
                    { exact: true, path: "/ProjectChapter/ProjectAdjust/ReviewRevoke", component: ProjectAdjustRevokeReview, breadcrumbName: '計畫撤銷審核' },
                    { exact: true, path: "/ProjectChapter/ProjectAdjust/ReviewBasic", component: ProjectAdjustBasicReview, breadcrumbName: '基本資料調整審核' },
                    { exact: true, path: "/ProjectChapter/ProjectAdjust/ReviewSchedule", component: ProjectAdjustScheduleReview, breadcrumbName: '期程調整審核' },
                    { exact: true, path: "/ProjectChapter/ProjectAdjust/", component: ProjectAdjustScheduleReview, breadcrumbName: '期程調整審核' },
                ]
            },
            { exact: true, path: "*", component: PageNotFound }
        ],
    },
];

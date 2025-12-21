import React from 'react';
import { renderRoutes } from 'react-router-config';
import Home from '../HomePage/Home'; /*主頁*/
import ProjectListMain from '../Page/PWS/ProjectList/ProjectListMain.js';

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

const PageNotFound = React.lazy(
    () => import(/* webpackChunkName: "PageNotFound" */
        '../Basic/PageNotFound'));

const PageLoading = React.lazy(
    () => import(/* webpackChunkName: "PageLoading" */
        '../Basic/PageLoading'));

const ProjectList = React.lazy(
    () => import(/* webpackChunkName: "ProjectList" */
        '../Page/PWS/ProjectList/ProjectListMain.js'));

const ProjectSend = React.lazy(
    () => import(/* webpackChunkName: "ProjectSend" */
        '../Page/PWS/ProjectSend/ProjectSendMain.js'));

const AddNewPlan = React.lazy(
    () => import(/* webpackChunkName: "AddNewPlan" */
        '../Page/PWS/AddNewPlan/AddNewPlanMain.js'));
        
const ProjectFundingExecution = React.lazy(
    () => import(/* webpackChunkName: "ProjectFundingExecution" */
        '../Page/PWS/ProjectFundingExecution/ProjectFundingExecutionMain.js'));

const FundingDetail = React.lazy(
    () => import(/* webpackChunkName: "FundingDetail" */
        '../Page/PWS/FundingDetail/FundingDetailMain.js'));

const CProjectResearch = React.lazy(
    () => import(/* webpackChunkName: "CProjectResearch" */
        '../Page/PWS/CProjectResearch/CProjectResearchMain.js'));

const ProjectExecution = React.lazy(
    () => import(/* webpackChunkName: "ProjectExecution" */
        '../Page/PWS/ProjectExecution/ProjectExecutionMain.js'));

const Upload = React.lazy(
    () => import(/* webpackChunkName: "Upload" */
        '../Page/PWS/Upload/UploadMain.js'));

const AssignWork = React.lazy(
    () => import(/* webpackChunkName: "AssignWork" */
        '../Page/PWS/AssignWork/AssignWorkMain.js'));

const ProjectMaintain = React.lazy(
    () => import(/* webpackChunkName: "ProjectMaintain" */
        '../Page/PWS/ProjectMaintain/ProjectMaintainMain.js'));

const ExportFile = React.lazy(
    () => import(/* webpackChunkName: "ExportFile" */
        '../Page/PWS/ExportFile/ExportFileMain.js'));

const AdjustNumber = React.lazy(
    () => import(/* webpackChunkName: "AdjustNumber" */
        '../Page/PWS/AdjustNumber/AdjustNumberMain.js'));

const ReviewProject = React.lazy(
    () => import(/* webpackChunkName: "ReviewProject" */
        '../Page/PWS/ReviewProject/ReviewProjectMain.js'));
        
const ReportList = React.lazy(
    () => import(/* webpackChunkName: "ReportList" */
        '../Page/PWS/ReportList/ReportMain.js'));

const ReportQuery = React.lazy(
    () => import(/* webpackChunkName: "ReportQuery" */
        '../Page/PWS/ReportList/ReportQuery.js'));

const GpReviewProject = React.lazy(
    () => import(/* webpackChunkName: "CGpReviewProjectMain" */
        '../Page/PWS/GpReviewProject/GpReviewProjectMain.js'));

const CAddNewPlan = React.lazy(
    () => import(/* webpackChunkName: "CAddNewPlan" */
        '../Page/PWS/CAddNewPlan/CAddNewPlanMain.js'));

const MailSet = React.lazy(
    () => import(/* webpackChunkName: "Mail" */
        '../Page/PWS/MailSet/MailSetMain'));


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
            { exact: true, path: "/Home/", component: ProjectListMain, breadcrumbName: '先期計畫登錄' },
            { exact: true, path: "/ProjectChapter/", component: PageLoading, breadcrumbName: '章節表' },
            {
                path: "/Home/", breadcrumbName: '計畫填報', routes: [
                    { exact: true, path: "/Home/ProjectMaintain", component: ProjectMaintain, breadcrumbName: '先期計畫登錄' },
                    { exact: true, path: "/Home/AssignWork", component: AssignWork, breadcrumbName: '指派作業' },
                    { exact: true, path: "/Home/ExportFile", component: ExportFile, breadcrumbName: '計畫列表' },
                    { exact: true, path: "/Home/AdjustNumber", component: AdjustNumber, breadcrumbName: '調整優先順序' },
                    { exact: true, path: "/Home/ReviewProject", component: ReviewProject, breadcrumbName: '調整優先順序' },
                    { exact: true, path: "/Home/Report", component: ReportList, breadcrumbName: '報表列印' },
                    { exact: true, path: "/Home/ReportQuery", component: ReportQuery, breadcrumbName: '報表查詢' },
                    { exact: true, path: "/Home/MailSet", component: MailSet, breadcrumbName: '郵件設定' },
                ]
            },
            {
                path: "/ProjectChapter", breadcrumbName: "計畫章節", routes: [
                    { exact: true, path: "/ProjectChapter/AddNewPlan", component: AddNewPlan, breadcrumbName: '新增計畫'},
                    { exact: true, path: "/ProjectChapter/ProjectFundingExecution", component: ProjectFundingExecution, breadcrumbName: '經費執行情形和歷年執行情形'},
                    { exact: true, path: "/ProjectChapter/FundingDetail", component: FundingDetail, breadcrumbName: '經費需求事項'},
                    { exact: true, path: "/ProjectChapter/ProjectExecution", component: ProjectExecution, breadcrumbName: '歷年執行情形'},
                    { exact: true, path: "/ProjectChapter/Upload", component: Upload, breadcrumbName: '檔案下載'},
                    { exact: true, path: "/ProjectChapter/ProjectSend", component: ProjectSend, breadcrumbName: '計畫送出'},
                    { exact: true, path: "/ProjectChapter/CAddNewPlan", component: CAddNewPlan, breadcrumbName: 'C新增計畫'},
                    { exact: true, path: "/ProjectChapter/CProjectResearch", component: CProjectResearch, breadcrumbName: 'C相關研究'},
                    { exact: true, path: "/ProjectChapter/GpReviewProject", component: GpReviewProject, breadcrumbName: '小組審核作業'},
                ]
            },

            { exact: true, path: "*", component: PageNotFound }
        ],
    },
];

import React from 'react';
import { renderRoutes } from 'react-router-config';
import Home from '../HomePage/Home'; /*主頁*/

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

// [章節]基本資料
const ProjectBasicMain = React.lazy(
    () => import(/* webpackChunkName: "RDMain" */
        '../Page/RD/ProjectBasic/ProjectBasicMain.js'));

// [章節]執行情形填報
const ProjectExecManage = React.lazy(
    () => import(/* webpackChunkName: "RDMain2" */
        '../Page/RD/ProjectExecManage/ProjectExecManageMain.js'));

// [章節]結案成果填報、[章節]續列管一年內參採情形
const ProjectExecClose = React.lazy(
    () => import(/* webpackChunkName: "RDMain3" */
        '../Page/RD/ProjectExecClose/ProjectExecCloseMain.js'));

// [章節]展延申請
const ProjectExtension = React.lazy(
    () => import(/* webpackChunkName: "RDMain3" */
        '../Page/RD/ProjectExtension/ProjectExtensionMain.js'));

// [章節]審核
const ProjectAudit = React.lazy(
    () => import(/* webpackChunkName: "RDMain5" */
        '../Page/RD/ProjectAudit/ProjectAuditMain.js'));

// 計畫登錄、計畫登錄-管理
const ProjectMain = React.lazy(
    () => import(/* webpackChunkName: "RDMain5" */
        '../Page/RD/ProjectMain/ProjectMain.js'));

// 報表清單
const Report = React.lazy(
    () => import(/* webpackChunkName: "RDMain5" */
        '../Page/RD/Report/ReportMain.js'));
// 報表查詢
const ReportsQuery = React.lazy(
    () => import(/* webpackChunkName: "RDMain5" */
        '../Page/RD/Report/ReportsQuery.js'));

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
            { exact: true, path: "/Home/", component: ProjectMain, breadcrumbName: '計畫登錄' },
            { exact: true, path: "/ProjectChapter/", component: PageLoading, breadcrumbName: '章節表' },
            /* 系統管理 */
            {
                path: "/Home/", breadcrumbName: '設定', routes: [
                    { exact: true, path: "/Home/ProjectMain", component: ProjectMain, breadcrumbName: '計畫登錄' },
                    { exact: true, path: "/Home/ProjectManage", component: ProjectMain, breadcrumbName: '計畫登錄-管理' },
                    { exact: true, path: "/Home/Report", component: Report, breadcrumbName: '報表列印' },
                    { exact: true, path: "/Home/ReportsQuery", component: ReportsQuery, breadcrumbName: '報表查詢' }
                ],
            },
            {
                path: "/ProjectChapter", breadcrumbName: "計畫章節", routes: [
                    { exact: true, path: "/ProjectChapter/ProjectBasic", component: ProjectBasicMain, breadcrumbName: '基本資料' },
                    { exact: true, path: "/ProjectChapter/ProjectExecManage", component: ProjectExecManage, breadcrumbName: '執行情形填報' },
                    { exact: true, path: "/ProjectChapter/ProjectExecClose", component: ProjectExecClose, breadcrumbName: '結案成果填報' },
                    { exact: true, path: "/ProjectChapter/ProjectSituaContinue", component: ProjectExecClose, breadcrumbName: '續列管一年內參採情形' },
                    { exact: true, path: "/ProjectChapter/ProjectExtension", component: ProjectExtension, breadcrumbName: '展延申請' },
                    { exact: true, path: "/ProjectChapter/ProjectAudit", component: ProjectAudit, breadcrumbName: '審核作業'}
                ]
            },
            { exact: true, path: "*", component: PageNotFound }
        ],
    },
];

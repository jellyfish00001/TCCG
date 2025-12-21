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

const InnAssignOrg = React.lazy(
    () => import(/* webpackChunkName: "InnAssignOrg" */
        '../Page/INN/InnAssignOrg/InnAssignOrgMain.js'));

const InnProjectTitle = React.lazy(
    () => import(/* webpackChunkName: "InnProjectTitle" */
        '../Page/INN/InnProjectTitle/InnProjectTitleMain.js'));

const InnProjectCusField = React.lazy(
    () => import(/* webpackChunkName: "InnProjectCusField" */
        '../Page/INN/InnProjectCusField/InnProjectCusFieldMain.js'));

const InnProjectBasic = React.lazy(
    () => import(/* webpackChunkName: "InnProjectProposalMain" */
        '../Page/INN/InnProjectBasic/InnProjectBasicMain.js'));

const InnProjectManage = React.lazy(
    () => import(/* webpackChunkName: "InnProjectManage" */
        '../Page/INN/InnProjectManage/InnProjectManageMain.js'));

const StatisticsList = React.lazy(
    () => import(/* webpackChunkName: "Statistics" */
        '../Page/INN/Statistics/StatisticsList'));

const StatisticsQuery = React.lazy(
    () => import(/* webpackChunkName: "Statistics" */
        '../Page/INN/Statistics/StatisticsQuery'));

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
            { exact: true, path: "/ProjectChapter/", component: PageLoading, breadcrumbName: '章節表' },
            {
                path: "/Home/", breadcrumbName: '提案', routes: [
                    { exact: true, path: "/Home/ProjectProposal", component: InnProjectManage, breadcrumbName: '創新提案提報' },
                ],
            },
            {
                path: "/Home/", breadcrumbName: '管理', routes: [
                    { exact: true, path: "/Home/ProjectProposal", component: InnProjectManage, breadcrumbName: '創新提案提報' },
                    { exact: true, path: "/Home/AssignOrg", component: InnAssignOrg, breadcrumbName: '截止時間設定' },
                    { exact: true, path: "/Home/ProjectManage", component: InnProjectManage, breadcrumbName: '創新提案管理' },
                    { exact: true, path: "/Home/ProjectTitle", component: InnProjectTitle, breadcrumbName: '維護指定專題' },
                    { exact: true, path: "/Home/ProjectCusField", component: InnProjectCusField, breadcrumbName: '維護自定義欄位' },
                    { exact: true, path: "/Home/Report", component: StatisticsList, breadcrumbName: '統計報表清單' },
                    { exact: true, path: "/Home/Statistics/StatisticsQuery", component: StatisticsQuery, breadcrumbName: '統計報表查詢' }
                ],
            },
            {
                path: "/ProjectChapter", breadcrumbName: "計畫章節", routes: [
                    { exact: true, path: "/ProjectChapter/ProjectBasic", component: InnProjectBasic, breadcrumbName: '創新提案提報' },
                ]
            },
            { exact: true, path: "*", component: PageNotFound }
        ],
    },
];

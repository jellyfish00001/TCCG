
import React from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from "@progress/kendo-react-buttons";

import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import { Download } from "../../../Basic/Download";

const DemoRPT = () => {
    let APIUrl = getGlobalServerConfig().backEndUrl.get();

    const exportXlsRPT = async () => {
        let url = APIUrl + 'RPT/RPTProjectDeptDetailed';
        Download(url, 'POST', null, new Headers());
    }

    const exportWordRPT = async () => {
        let url = APIUrl + 'RPT/ExportWord';
        Download(url, 'POST', null, new Headers());
    }

    return (
        <>
            <PageContainer
                toolbar={
                    <>
                        <h3 className="k-dialog-titlebar">Demo 報表</h3>
                        <Button onClick={() => { exportXlsRPT() }}>Xls</Button>
                        <Button onClick={() => { exportWordRPT() }}>Word</Button>
                    </>
                }
            />
        </>
    );
}

export default DemoRPT;
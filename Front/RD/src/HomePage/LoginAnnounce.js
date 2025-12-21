import React, { useEffect, useState, useReducer } from 'react';
import { api, handleErrors } from '../Basic/ApiFetch';
import { ServerConfig, GetHistory } from '../Basic/BasicData';
import { Grid, GridColumn } from '@progress/kendo-react-grid';
import { HtmlDecode } from '../Basic/SDOExtension';
import { showGlobalMessageBox, getGlobalServerConfig } from '../Route/RootMiddleware';

const LoginAnnouncement = () => {
    /* use State */
    const [announcement, setAnnouncement] = useState(null);
    const [ignore, forceUpdate] = useReducer(x => x + 1, 0);
    /* use Effect */
    useEffect(() => {
        readDisplayAnnoucement();
    }, [])

    /* function */
    // 讀取display公告
    const readDisplayAnnoucement = async () => {
        // annType=99 列出 APL公告 (annType參數以url傳遞)
        let response = await api.Get(getGlobalServerConfig().backEndUrl.get() + 'Announcement?annType=99')
            .then(handleErrors)
            .catch(
                error => {
                    showGlobalMessageBox("發生伺服器端錯誤，請聯繫管理員。", () => { GetHistory().push('/'); });
                }
            );
        if (response) {
            if (response.ok) { setAnnouncement(await response.json()); }
            else { setAnnouncement({ TITLE: '公告載入失敗' }) }
        }

    }

    //展開公告內容
    const expandChange = (event) => {
        event.dataItem.expanded = !event.dataItem.expanded;
        forceUpdate();
    }
    //自訂row外觀
    const cellStyle = (props) => {
        return (
            <td style={{ width: '100%' }}>
                {props.dataItem.TITLE}<br />
                <align-title>{props.dataItem.EFFECTIVE_TWDATE}</align-title>
            </td>
        );
    }

    //附件下載按鈕
    const downloadCellStyle = (props) => {
        return (props.dataItem.ATTACH_NAME !== null ?
            <td style={{ textAlign: 'center', borderLeft: '1px gray dotted' }}
                onClick={() => {
                    downloadAttachFile(props.dataItem)
                }}>
                <span className="k-icon k-i-download" style={{ color: '#007bff' }} />
            </td>
            :
            <td></td>
        )
    }

    //檔案下載
    const downloadAttachFile = async (dataItem) => {
        try {
            /* 檔案下載 ajax */
            let response = await api.Get(ServerConfig.backEndUrl + 'Announcement/GetAttachment/' + dataItem.SID);
            let fileName = decodeURIComponent(response.headers.get("content-disposition").split("UTF-8''")[1]);
            let blob = await response.blob();
            let href = window.URL.createObjectURL(blob);
            let link = document.createElement('a');
            link.href = href;
            link.download = fileName;
            link.click();
        }
        catch (e) {
            console.log(e);
        }
    }

    //公告內容
    const announcementDetail = (props) => {
        return (
            <span
                style={{ display: 'inline-block', width: '100%', whiteSpace: 'pre-wrap' }}
                dangerouslySetInnerHTML={{ __html: HtmlDecode(props.dataItem.COMMENT) }}
            ></span>
        )
    }

    return (
        <div className="login-announce">
            {announcement ?
                <Grid
                    style={{
                        maxHeight: '700px',
                        minWidth: '400px',
                        marginTop: '50px',
                        width: '90%'
                    }}
                    data={announcement}
                    detail={announcementDetail}
                    expandField="expanded"
                    onExpandChange={expandChange}
                    onRowClick={expandChange}
                >
                    <GridColumn
                        title='公告事項'
                        cell={cellStyle}
                        minResizableWidth={500}
                    />
                    <GridColumn
                        cell={downloadCellStyle}
                        width='58'
                    />
                </Grid> :
                '公告載入中'}
        </div>
    )
}

export default LoginAnnouncement;
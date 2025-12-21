import React, { useState, useEffect } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn } from '@progress/kendo-react-grid';
import { AddNoColumn } from '../../Basic/SDOExtension';
import { apiFetch } from '../../Basic/ApiFetch';
import { ServerConfig } from '../../Basic/BasicData';
import { DropDownListWithValue } from '../../Components/Dropdowns/DropDownListWithValue';
import {ExportWordGrid} from './ExportWordGrid'
import {getGlobalServerConfig} from '../../Route/RootMiddleware'

const ExportWordReport = () => {
    const exportFormat = [
        { text: 'Docx', value: 'docx' },
        { text: 'Doc', value: 'doc' },
        { text: 'PDF', value: 'pdf' },
        { text: 'Odt', value: 'odt' }
    ];
    const [selectValue, setSelectValue] = useState('docx');
  
    //
    const instructionDownload = (e) => {
        download("ExportWordReport");
    }

    //
    const wordReportDownload = (e) => {
        let form = new FormData();
        form.append('saveFormat', selectValue);
        download("ExportWordReport", { method: 'POST', body: form }, true);
    }
    const GanttDemoDownload = (e) => {
        let form = new FormData();
        form.append('saveFormat', selectValue);
        download("GanttDemo", { method: 'POST', body: form }, true);
    }
    const pdfReportDownload = (e) => {
        let form = new FormData();
        form.append('saveFormat', 'pdf');
        download("ExportWordReport/ExportPdf", { method: 'POST', body: form }, true);
    }
   
    //下載檔案
    const download = async (url, init, custom) => {
        try {
            /* 檔案下載 ajax */
            let request = new Request(getGlobalServerConfig().backEndUrl.get() + url, init);
            let response = await apiFetch(request, custom);
            if (response.ok) {
                let fileName = decodeURIComponent(response.headers.get("content-disposition").split("UTF-8''")[1]);

                let blob = await response.blob();
                let href = window.URL.createObjectURL(blob);
                let link = document.createElement('a');

                link.download = fileName;
                link.href = href;

                link.click();
            }
        }
        catch (e) {
            console.log(e);
        }
    }

    // 每次異動input後回寫至state
    const handleChange = (e) => {
        setSelectValue(e.target.value);
    }

    return (
        <div className="fnForm">
            <div className="fn-buttons">
                <Button onClick={instructionDownload}>AsposeWordSet使用說明下載</Button>
                <Button onClick={wordReportDownload}>匯出Word套版</Button>
                <Button onClick={GanttDemoDownload}>匯出Word甘特圖套版</Button>
                <Button onClick={pdfReportDownload}>匯出pdf</Button>
            </div>
            <div>
                <form>
                    <table>
                        <tbody>
                            <tr>
                                <th>使用方式</th>
                                <td>
                                    AsposeWordSet {'=>'} 產製Word套表匯出
                                        <br />
                                        詳細參數設定可參考AsposeWordSet使用說明下載，和參考ExportReportController的程式下載範例
                                    </td>
                            </tr>
                            <tr>
                                <th>支援格式</th>
                                <td>支援 Word 範本轉 Doc、Docx、Odt、Pdf 匯出</td>
                            </tr>
                            <tr>
                                <th>匯出格式</th>
                                <td>
                                    <DropDownListWithValue
                                        value={selectValue}
                                        data={exportFormat}
                                        textField="text"
                                        dataItemKey="value"
                                        onChange={handleChange} />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </form>
                <h3>匯出範例資料</h3>
            </div>
            <ExportWordGrid ></ExportWordGrid>

        </div>
    );
}
export default ExportWordReport;
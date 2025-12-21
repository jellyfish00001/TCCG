import React, { useState } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { apiFetch } from '../../Basic/ApiFetch';
import { ServerConfig } from '../../Basic/BasicData';
import { DropDownListWithValue } from '../../Components/Dropdowns/DropDownListWithValue';
import {getGlobalServerConfig} from '../../Route/RootMiddleware'

const ExportExcelReport = () => {
    const exportFormat = [
        { text: 'Xlsx', value: 'xlsx' },
        { text: 'Xls', value: 'xls' },
        { text: 'PDF', value: 'pdf' },
        { text: 'Ods', value: 'ods' },
        { text: 'Html', value: 'html' },
    ];
    //
    const [selectValue, setSelectValue] = useState('xlsx');
    //
    const excelExampleDownload = () => { download("AsposeExcelSet"); }
    //
    const reportAutoDownload = () => { download("AsposeExcelSet/ReportAuto/" + selectValue); }
    //
    const reportDefaultDownload = () => { download("AsposeExcelSet/ReportDefault/" + selectValue); }
    //
    const reportSingleDownload = () => { download("AsposeExcelSet/ReportSingle/" + selectValue); }

    //下載檔案
    const download = async (url) => {
        try {
            /* 檔案下載 ajax */
            let request = new Request(getGlobalServerConfig().backEndUrl.get() + url);
            let response = await apiFetch(request);
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
            console.log('ErrorMessage: '+e);
        }
    }

    // 每次異動input後回寫至state
    const handleChange = (e) => {
        setSelectValue(e.target.value);
    }

    return (
        <div className="fnForm">
            <div className="fn-buttons">
                <Button onClick={excelExampleDownload}>Excel範例套版檔案</Button>
                <Button onClick={reportAutoDownload}>範本1</Button>
               
            </div>
            <div>
                <form>
                    <table>
                        <tbody>
                            <tr>
                                <th>使用方式</th>
                                <td>
                                    NpoiExcelSet {'=>'} 產製Excel套表匯出
                                        <br />
                                        詳細設定參考ExportExcelReportController的程式下載範例
                                    </td>
                            </tr>
                            <tr>
                                <th>範本樣式</th>
                                <td>範本1：縱向動態長資料範例(自動CreateRow)
                                    <br />範本2：縱向動態長資料範例(預設Row範圍)
                                    <br />範本3：單一欄位替換
                                    <br />以上樣式皆可以參考Excel範例套版檔案
                                    </td>
                            </tr>
                            <tr>
                                <th>支援格式</th>
                                <td>支援 Excel 範本轉 Xlsx、Xls、Ods、Pdf、Html 匯出</td>
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
            </div>
        </div>
    );
}

export default ExportExcelReport
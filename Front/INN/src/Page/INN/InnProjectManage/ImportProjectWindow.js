import React, { useState, useRef, useEffect } from "react";
import { Window } from '@progress/kendo-react-dialogs';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import TextInput from '../../../Components/Input/TextInput';
import { Button } from '@progress/kendo-react-buttons';
import { Formik } from 'formik';
import { showGlobalMessageBox, showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import { IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { saveInnBasic } from "../InnProjectBasic/InnProjectBasicService";
import * as Yup from 'yup';
import { downTemplateFile, importGeneralProject } from './InnProjectManageService'
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import { FileUploader } from '../../../Components/Upload/FileUploader'
import TextAreaInput from '../../../Components/Input/TextAreaInput';
import { PageContainer } from "../../../Basic/PageContainer";
import Table from '../../../Css/custom/Table.module.css';
import { NumericTextBox } from '@progress/kendo-react-inputs';

const ImportProjectWindow = props => {
    const dimensions = WindowResizehook();
    let { visible, onClose } = props

    // 年度
    const [planYear, setPlanYear] = useState(new Date().getFullYear() - 1911);
    // 檔案
    const [files, setFiles] = useState([]);
    // 檔案驗證
    const [fileValid, setFileValid] = useState(false)
    // 錯誤訊息
    const [errorMsg, setErrorMsg] = useState("");
    // 下載範本檔
    const downTemplate = (e) => {
        downTemplateFile('InnProjectSample.xls', e.target.textContent);
    }

    /**
     * 匯入計畫基本資料
     */
    const Import = async () => {
        let importResult = await importGeneralProject(planYear, files);
        if (importResult != null && importResult.success) {
            showGlobalMessageBox(importResult.message, () => {
                setFiles([]);
                setFileValid(false);
                setErrorMsg("");
            })
        } else {
            showGlobalMessageBox("資料格式錯誤於下方顯示，請確認資料正確無誤後重新匯入")
            setErrorMsg(importResult.message);
        }
    }

    // 上傳檔案驗證
    useEffect(() => {
        if (files.length > 0) {
            if ((files[0].extension == '.xls' || files[0].extension == '.xlsx')
                && files[0].size <= 10485759) {
                setFileValid(true);
            } else {
                setFileValid(false)
            }
        } else {
            setErrorMsg("");
        }
    }, [files])

    return (
        <>
            {visible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title='新增提案'
                        onClose={() => { onClose() }}
                        initialWidth={dimensions.width > 700 ? 800 : dimensions.width * .7}
                        initialHeight={dimensions.height > 700 ? 500 : dimensions.height * .8}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <PageContainer style={{
                            height: '100%',
                            overflow: 'auto',
                        }}
                            toolbar={
                                <div>
                                    <Button title="確定新增計畫" disabled={!fileValid} onClick={Import} >確定新增計畫</Button>
                                </div >
                            }
                        >
                            <form >
                                <table className={Table.fullWidth}>
                                    <tbody>
                                        <tr>
                                            <th>計畫年度</th>
                                            <td>
                                                <NumericTextBox
                                                    value={planYear}
                                                    onChange={(e) => { setPlanYear(e.target.value); }}
                                                    max={999}
                                                    min={1}
                                                    format={'n0'}
                                                    onBlur={(e) => {
                                                        if (e.target.value >= 1000 || e.target.value <= 0) {
                                                            showGlobalMessageBox("請輸入民國年");
                                                            setPlanYear(new Date().getFullYear() - 1910);
                                                        }
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>下載Excel範本</th>
                                            <td><a href='#' onClick={(e) => {
                                                e.preventDefault();
                                                downTemplate(e)
                                            }}>創新計畫匯入範本</a></td>
                                        </tr>
                                        <tr>
                                            <th>
                                                <CommonTooltip
                                                    title={"批次上傳計畫"}
                                                    content={"1.上傳檔案格式請使用XLS 2.檔案請勿設定加密或保全 3.單一檔案上限請勿超過10MB"}
                                                />
                                            </th>
                                            <td>
                                                <FileUploader
                                                    multiple={false}
                                                    autoUpload={false}
                                                    maxFileSize={10485760} // 10Mb*1024*1024
                                                    allowedExtensions={[".xls", ".xlsx"]}
                                                    files={files}
                                                    setFiles={setFiles}
                                                />
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </form>
                            {errorMsg != "" && <TextAreaInput
                                style={{ width: "100%", "color": 'red' }}
                                rows={10}
                                name="MEMO"
                                readonly="true"
                                defaultValue={errorMsg}
                            />}
                        </PageContainer >
                    </Window>
                </div >
            }
        </>
    )
}

export default ImportProjectWindow;
import React from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import { showGlobalMessageBox, getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import ProjectFillRefFileService from './ProjectFillRefFileService';
import { downProjectAttachment } from '../../../Basic/CommonService';
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../Components/Upload/TempFileUploadService';
import CacheLoader from '../../../Basic/CacheLoader';

const ProjectFillRefFileMain = () => {
    // 資料是否修改
    const [isDataChange, setIsDataChange] = React.useState(false);
    // 檔案
    const [files, setFiles] = React.useState([]);
    // 相關檔案上傳異動資訊
    const editedFiles = React.useRef([]);

    // 檔案上傳所需參數
    let filesparam = {
        files: files,
        editedFiles: editedFiles,
        setFiles: setFiles,
        setIsDataChange: setIsDataChange,
        multiple: true,
        downFile: downProjectAttachment
    }
    // 暫存檔上傳 Service
    const tempFileUploadService = TempFileUploadService(filesparam);

    // 參考資料檔案列表
    const loadFileList = async () => {
        SetMaskOnOff(true);
        let data = await ProjectFillRefFileService.getRefFile();
        let fileData = fileList(data, "IDENTITY_FIELD");
        setFiles(fileData);
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadFileList();
    }, [])

    /**
     * 存檔
     */
    const save = async () => {
        SetMaskOnOff(true);
        let saveResult = await ProjectFillRefFileService.saveRefFile(editedFiles.current);
        SetMaskOnOff(false);
        if (saveResult.success) {
            editedFiles.current = [];
            showGlobalMessageBox(saveResult.message, () => {
                loadFileList();
            });
        }
    }

    return (
        <>
            <PageContainer toolbar={
                <>
                    <h3 className="k-dialog-titlebar">{"參考資料上傳"}</h3>
                    <Button type="button" title="存檔" onClick={save}>存檔</Button>
                </>
            }>
                <form>
                    <table>
                        <tr>
                            <th>
                                <CommonTooltip title={"參考資料"} content={
                                    <>
                                        單一檔案上限請勿超過20MB<br />
                                        可上傳檔案格式如下<br />
                                        jpg, jpeg, bmp, png,<br />
                                        mpg, doc, docx, ppt,<br />
                                        pptx, pdf, xls, xlsx,<br />
                                        odt, ods, odp, odg
                                    </>}
                                    withoutRedStar={true}
                                />
                            </th>
                            <td>
                                <TempFileUploader
                                    {...tempFileUploadService.uploaderParam}
                                    saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                    saveHeaders={{
                                        'authorization': getGlobalServerConfig().BasicData.token.get(),
                                        // 'CacheToken': CacheLoader().GetCache(),
                                    }}
                                    files={files}
                                    setFiles={setFiles}
                                    multiple={true}
                                />
                            </td>
                        </tr>
                    </table>
                </form>
            </PageContainer>
        </>
    )
}
export default ProjectFillRefFileMain;
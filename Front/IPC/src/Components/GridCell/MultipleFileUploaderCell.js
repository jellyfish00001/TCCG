import React from 'react';
import { getGlobalServerConfig } from '../../Route/RootMiddleware';
import { TempFileUploader } from '../Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../Upload/TempFileUploadService';
import CacheLoader from '../../Basic/CacheLoader';
import { IsNullOrEmpty } from '../../Basic/SDOExtension';
import { downProjectAttachment } from '../../Basic/CommonService';
import * as Yup from 'yup';
import { Error } from '@progress/kendo-react-labels';

/**
 * 多檔上傳grid cell
 * @returns JSX
 * @example
 * const validataSourceGField = Yup.object().shape({
    FILE: Yup.array().required('必須上傳檔案')
 * })
 * <MultipleFileUploaderCell
        field="FILE"
        dataItem={prop.dataItem} // grid dataItem
        targetFile={prop.dataItem.FILE}
        // 當檔案異動時執行的onchange，dataItem: 原grid dataItem, filesInfo: 全部異動的檔案
        setFileChange={(dataItem, filesInfo) => {}}
        customValidate={{ scheme: validataSourceGField, triggerValidate: triggerValidation }}
        disabled={prop.dataItem.disabled}
    />
 */
const MultipleFileUploaderCell = ({ field, dataItem, targetFile, setFileChange, customValidate, disabled }) => {

    //準備欄位驗證工具
    const validateHelper = customValidate ? customValidate.scheme : Yup.object().shape({
        value: Yup.array().required('必須上傳檔案')
    });

    const [Valid, SetValid] = React.useState({
        IsValid: true,
        message: ''
    });

    const [files, setFiles] = React.useState([]);
    // 檔案上傳異動相關資訊
    const editedFiles = React.useRef([]);
    // 紀錄資料資否有異動
    const [isDataChange, setIsDataChange] = React.useState(false);
    const [deleteType, setDeleteType] = React.useState("");
    const [deletedFileUid, setDeletedFileUid] = React.useState("");

    // 下載檔案
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId);
    }

    // 上傳所需參數
    let param = {
        files,
        editedFiles,
        setFiles,
        setIsDataChange,
        multiple: true,
        downFile,
        // For grid file delete
        setDeleteType: setDeleteType,
        setDeletedFileUid: setDeletedFileUid,
    }
    // 暫存檔上傳Service
    const tempFileUploadService = TempFileUploadService(param);

    // 有檔案資料上傳時，設定GridDataItem
    React.useEffect(() => {
        if (isDataChange) {
            // 異動檔案數onFileChange();
            let fileInfo = editedFiles.current.map(file => ({
                EditFiles: [{ ...file }],
                extension: file.Extension,
                IsMultiple: true,
                FILE_NAME: file.EditType == 1 ? file.FileName : file.FileName + file.Extension,
                uid: file.Uid ? file.Uid : null,
                IDENTITY_FIELD: file.FileId ? file.FileId : 0,
                isUploaded: file.EditType == 2,
                editType: file.EditType == 2 ? 3 : 1,

            }))

            // For grid file delete ， 0 : 刪除已上傳檔案， 1 : 刪除暫存檔案
            if (deleteType === "1") {
                fileInfo = fileInfo.filter(file => file.uid != deletedFileUid)
            }
            // 將加工過後的editedFiles 回傳外層
            setFileChange(dataItem, fileInfo);
            setDeleteType("");
            setDeletedFileUid("");
        }

        // 把isDataChange 改為false, 再次上傳才會觸發useEffect
        setIsDataChange(false);

        //如果有從外部傳scheme進來，利用外部scheme驗證單一field
        if (customValidate) {
            if (customValidate.triggerValidate)
                validateHelper.validateAt(field, dataItem)
                    .then(() => {
                        SetValid({ IsValid: true, message: '' });
                    })
                    .catch((err) => {
                        SetValid({ IsValid: false, message: err.message });
                    });
        }
    }, [isDataChange]);

    React.useEffect(() => {
        // 處理畫面上檔案 (排除刪除的檔案)
        if (!IsNullOrEmpty(targetFile)) {
            let filesExcludeDeletedFiles = targetFile.filter(file => file.editType !== 3)
            let result = fileList(filesExcludeDeletedFiles, 'IDENTITY_FIELD', 'FILE_NAME');
            setFiles(result);

            // 紀錄目前異動的檔案
            let newEditedFiles = targetFile.filter(file => file.editType != 0).map(file => file.EditFiles[0]);
            editedFiles.current = newEditedFiles;
        }
    }, []);

    return (
        <>
            <div className={"upload_File"} >
                <TempFileUploader
                    {...tempFileUploadService.uploaderParam}
                    saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                    saveHeaders={{
                        // @ts-ignore
                        'authorization': getGlobalServerConfig().BasicData.token.get(),
                        // 'CacheToken': CacheLoader().GetCache(),
                    }}
                    files={files}
                    setFiles={setFiles}
                    multiple={true}
                    disabled={disabled ?? false}
                />
            </div>
            <Error>{Valid.IsValid ? '' : Valid.message}</Error>
        </>
    )
}

export default MultipleFileUploaderCell;
import React from 'react';
import { getGlobalServerConfig } from '../../Route/RootMiddleware';
import { TempFileUploader } from '../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../Components/Upload/TempFileUploadService';
import CacheLoader from '../../Basic/CacheLoader';
import { IsNullOrEmpty } from '../../Basic/SDOExtension';
import { downProjectAttachment } from '../../Basic/CommonService';
import * as Yup from 'yup';
import { Error } from '@progress/kendo-react-labels';

const FileUploaderCell = ({ field, dataItem, targetFile, setFileChange, customValidate, disabled }) => {

    //準備欄位驗證工具
    const validateHelper = customValidate ? customValidate.scheme : Yup.object().shape({
        value: Yup.object().required('必須上傳檔案')
    });

    const [Valid, SetValid] = React.useState({
        IsValid: true,
        message: ''
    });

    // 檔案上傳異動相關資訊
    const editedFiles = React.useRef([]);
    // 檔案
    const [files, setFiles] = React.useState([]);
    // 紀錄資料資否有異動
    const [isDataChange, setIsDataChange] = React.useState(false);

    const [deleteType, setDeleteType] = React.useState("");

    // 單檔上傳所需參數
    let param = {
        files,
        editedFiles,
        setFiles,
        setIsDataChange,
        multiple: false,
        downFile: downProjectAttachment,
        // For grid file delete
        setDeleteType: setDeleteType
    }

    // 暫存檔上傳Service
    const tempFileUploadService = TempFileUploadService(param);


    // 檔案上傳事件
    const onFileChange = () => {

        let fileInfo = {
            ...targetFile,
            // 把 editedFiles 整包丟進fileInfo裡
            EditFiles: editedFiles.current,
            // 從 files 取得檔案相關資訊，並塞到 fileInfo中
            extension: files[0].extension,
            FILE_NAME: files[0].name,
            getRawFile: files[0].getRawFile,
            uid: editedFiles.current[editedFiles.current.length - 1].Uid,
            // 是否已上傳到正確位置，PS:grid file上傳是採取先上傳到暫存地方，存檔後才放到正確的檔案路徑
            isUploaded: false,
            editType: 1 // 新增
        };

        // 將加工過後的fileInfo 回傳外層 
        setFileChange(dataItem, fileInfo);
    }

    // 有檔案資料上傳時，設定GridDataItem
    React.useEffect(() => {
        // 異動檔案數
        let ecl = editedFiles.current.length;
        if (!IsNullOrEmpty(deleteType)) {
            // For grid file delete ， 0 : 刪除已上傳檔案， 1 : 刪除暫存檔案
            let mdfData = null;

            if (deleteType === "0") {
                //刪除已上傳檔案
                mdfData = {
                    editType: 3, // 刪除
                    EditFiles: editedFiles.current
                }
            }

            setFileChange(dataItem, mdfData);
            setDeleteType("");
        }
        else if (ecl > 0 && isDataChange === true &&
            !(ecl === 1 && editedFiles.current[0].EditType === 2)) {
            onFileChange();
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
        let result = IsNullOrEmpty(targetFile) || targetFile.editType === 3
            ?
            [] : fileList([targetFile], 'IDENTITY_FIELD', 'FILE_NAME');
        setFiles(result);

        editedFiles.current = !IsNullOrEmpty(targetFile) ?
            (!IsNullOrEmpty(targetFile.EditFiles) ? targetFile.EditFiles : []) : [];
    }, []);

    return (
        <>
            <div className={"upload_File"} >
                <TempFileUploader
                    {...tempFileUploadService.uploaderParam}
                    saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                    saveHeaders={{
                        'authorization': getGlobalServerConfig().BasicData.token.get(),
                        'CacheToken': CacheLoader().GetCache(),
                    }}
                    files={files}
                    setFiles={setFiles}
                    multiple={false}
                    disabled={disabled ?? false}
                />
            </div>
            <Error>{Valid.IsValid ? '' : Valid.message}</Error>
        </>
    )
}

export default FileUploaderCell;
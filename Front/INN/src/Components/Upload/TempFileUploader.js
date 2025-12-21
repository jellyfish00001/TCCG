import * as React from 'react';
import { Upload } from '@progress/kendo-react-upload';
import { showGlobalMessageBox } from '../../Route/RootMiddleware';
import '../../Css/custom/uploadFile.css';

/**
 * tempFileIndexs應為外層傳入的ref，以此回傳
 * <p>files及setFiles應該是由外層傳入的state，以此控制檔案清單。</p>
 * 
 * @param {*} props 
 * @returns 
 */
export const TempFileUploader = (props) => {

    // const [files, setFiles] =React.useState([]) // 檔案
    /**
     * 傳入的參數 
    */
    const {
        files,
        setFiles,
        tempFileIndexs
    } = props;

    /**
    * 添加：當選擇新文件上傳時觸發
    * @param {*} e 
    */
    const onUploaderAdd = (e) => {
        //如果單檔且不顯示清單，改以彈出視窗顯示錯誤訊息
        if (!props.multiple && !props.showFileList && e.newState[0].validationErrors) {
            let invalidFileExtension = e.newState[0].validationErrors.filter(o => o === "invalidFileExtension");
            if (invalidFileExtension.length > 0)
                showGlobalMessageBox('不是允許的檔案格式!');
        }
        setFiles(e.newState);


    }

    return (
        <>
            <Upload
                batch={false} //為true，則所有選定文件將在單個請求中上傳
                multiple={true}
                autoUpload={true}
                withCredentials={false} //將憑證附加到請求
                restrictions={{
                    maxFileSize: 20971520,
                    //允許的附檔名，可依各專案自行調整 EXE TXT CSV會有資安疑慮請避免開放
                    allowedExtensions: props.allowedExtensions ? props.allowedExtensions : [
                        // ".rar", ".zip", Nash說因資安考量拿掉
                        ".jpg", ".jpeg", ".bmp", ".png",
                        ".mpg", ".doc", ".docx", ".ppt", ".pptx", ".pdf",
                        ".xls", ".xlsx", ".odt", ".ods", ".odp", ".odg"
                    ]
                }}
                onAdd={onUploaderAdd}
                {...props}
            />
        </>
    )
}
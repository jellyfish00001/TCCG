import React, { useState, useEffect, useRef } from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import Table from '../../../Css/custom/Table.module.css';
import { Formik } from "formik";
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import CacheLoader from '../../../Basic/CacheLoader';
import { downProjectAttachment } from '../../../Basic/CommonService';
import { FileUploader } from '../../../Components/Upload/FileUploader';
import TwDatePicker from '../../../Components/DateInputs/TwDatePicker';
import NumericTextInput from '../../../Components/Input/NumericTextInput';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { adjustHistory } from '../ProjectAdjuctHistory/AdjustHistoryService';

export const ProjectEngSystemForm = (props) => {

    let { data, engSystemFormChange, fileList, isEngineering, isStartWork, isRdecFun } = props

    // Form 資料
    const [formData, setFormData] = useState({});
    const formRef = useRef("");
    // 異動指標(讓Formik reload Form)
    const loadMark = useRef(true);
    // 異動檔案
    const [files, setFiles] = useState([])
    //　工程進度檔案Grid
    const [gridData, setGridData] = useState([]);
    // 移除檔案識別碼
    const removedFileIds = useRef([])

    // 輸入框onBlur事件
    const inputBlur = async () => {
        let outputData = {
            file: files.length > 0 ? files[0] : null,
            formData: formData
        }
        engSystemFormChange({ ...outputData });
    }

    // 下載已上傳檔案 cell
    const downFileCell = props => {
        return (
            <td>
                <a style={{ 'color': 'blue', fontSize: '1.05rem' }} href='/' onClick={async (e) => {
                    e.preventDefault();
                    await downProjectAttachment(props.dataItem.IDENTITY_FIELD)
                }}>{props.dataItem.FILE_NAME}</a>
            </td>
        )
    }

    // 刪除檔案
    const deleteCell = props => {
        let id = props.dataItem.IDENTITY_FIELD;
        return (
            <td style={{ textAlign: 'center' }}>
                <Button title={"刪除"} icon='close' look='default'
                    onClick={() => {
                        removeFile(id);
                    }} />
            </td>
        )
    }

    /**
     * 移除檔案
     * @param {Number} identityField 檔案識別碼
     */
    const removeFile = (identityField) => {
        removedFileIds.current.push(identityField);
        // 異動資料
        let outputData = {
            file: files.length > 0 ? files[0] : null,
            removedFileIds: removedFileIds.current,
            formData: formData
        }
        engSystemFormChange({ ...outputData }) // 異動資料回傳父層

        // 從Grid上移除
        let index = gridData.findIndex(d => d.IDENTITY_FIELD === identityField);
        gridData.splice(index, 1);
        setGridData([...gridData]);
    }

    useEffect(() => {
        if (data) {
            // 設定異動指標，讓Fomik reload Form
            setFormData({ ...data, loadMark: loadMark.current })
            loadMark.current = !loadMark.current;
            setFiles([]);
            removedFileIds.current = [];
        }
    }, [data])

    useEffect(() => {
        let outputData = {
            file: files.length > 0 ? files[0] : null,
            formData: formData
        }
        engSystemFormChange({ ...outputData })
    }, [files])

    useEffect(() => {
        setGridData([...fileList]);
    }, [fileList])

    return (
        <PageContainer
            style={{
                height: '100%',
                overflow: 'auto'
            }}
        >
            <Formik
                initialValues={formData}
                enableReinitialize={true}
                innerRef={formRef}
            >
                {props => {
                    const {
                        values,
                        errors,
                    } = props;
                    return (
                        <form >
                            <table className={Table.fullWidth}>
                                <tbody>
                                    <tr>
                                        <th>總期程調整歷程</th>
                                        <td colSpan={4}>{adjustHistory(values.AdjustScheHistoryModels, "Y")}</td>
                                    </tr>
                                    <tr>
                                        <th>分月期程調整歷程</th>
                                        <td colSpan={4}>{adjustHistory(values.AdjustScheHistoryModels, "M")}</td>
                                    </tr>
                                    {isEngineering && isStartWork &&
                                        <>
                                            <tr>
                                                <th className='addRedStar'>契約預定竣工日</th>
                                                <td colSpan={3}>
                                                    <TwDatePicker
                                                        name="CONTRACT_FINISH_DATE"
                                                        format={"yyy/MM/dd"}
                                                        onChange={(e) => {
                                                            let form = { ...values, CONTRACT_FINISH_DATE: e.target.value }
                                                            let outputData = {
                                                                file: files.length > 0 ? files[0] : null,
                                                                formData: form
                                                            }
                                                            engSystemFormChange({ ...outputData })
                                                            setFormData({ ...form });
                                                        }}
                                                        value={values.CONTRACT_FINISH_DATE == null ? null : new Date(values.CONTRACT_FINISH_DATE)}
                                                        error={errors.CONTRACT_FINISH_DATE}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    <CommonTooltip
                                                        title={"工程預定進度表"}
                                                        content={
                                                            <>
                                                                可上傳檔案格式如下<br />
                                                                jpg, jpeg, bmp, png,<br />
                                                                mpg, doc, docx, ppt,<br />
                                                                pptx, pdf, xls, xlsx,<br />
                                                                odt, ods, odp, odg
                                                            </>
                                                        }
                                                    />
                                                </th>
                                                <td style={{ width: '40%' }}>
                                                    <FileUploader
                                                        multiple={false}
                                                        autoUpload={false}
                                                        saveHeaders={{
                                                            'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                            // 'CacheToken': CacheLoader().GetCache()
                                                        }}
                                                        files={files}
                                                        setFiles={setFiles}
                                                        showFileList={true}
                                                        onStatusChange={() => {
                                                            setFormData({ ...values, file: files })
                                                        }}
                                                    />
                                                </td>
                                                <td>
                                                    <Grid
                                                        data={gridData}
                                                    >
                                                        <GridNoRecords>無資料</GridNoRecords>
                                                        <GridColumn field="IDENTITY_FIELD" width="0px" />
                                                        {/* 管考才有權限刪除 */}
                                                        {isRdecFun && <GridColumn cell={deleteCell} title="刪除" width="50px" />}
                                                        <GridColumn title='上傳日期' field="UploadDate" />
                                                        <GridColumn field="FILE_NAME" title="檔案" cell={downFileCell}
                                                        />
                                                    </Grid>
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className='addRedStar'>發包金額</th>
                                                <td colSpan={3}>
                                                    <NumericTextInput
                                                        name="PROCUREMENT_AMT"
                                                        format={'n0'}
                                                        min={0}
                                                        inputType={'text'}
                                                        value={values.PROCUREMENT_AMT}
                                                        error={errors.PROCUREMENT_AMT}
                                                        onChange={(e) => {
                                                            setFormData({ ...values, PROCUREMENT_AMT: e.value })
                                                        }}
                                                        WithFormik={true}
                                                        onBlur={inputBlur}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className='addRedStar'>決標金額</th>
                                                <td colSpan={3}>
                                                    <NumericTextInput
                                                        name="TENDER_AWARDING_AMT"
                                                        format={'n0'}
                                                        min={0}
                                                        inputType={'text'}
                                                        value={values.TENDER_AWARDING_AMT}
                                                        error={errors.TENDER_AWARDING_AMT}
                                                        onChange={(e) => {
                                                            setFormData({ ...values, TENDER_AWARDING_AMT: e.value })
                                                        }}
                                                        WithFormik={true}
                                                        onBlur={inputBlur}
                                                    />
                                                </td>
                                            </tr>
                                        </>}

                                </tbody>
                            </table>
                        </form>
                    )
                }}
            </Formik>
        </PageContainer>
    );
}
export default ProjectEngSystemForm
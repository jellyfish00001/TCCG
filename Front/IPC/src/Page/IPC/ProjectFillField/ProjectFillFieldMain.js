import React from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import { handleEditedGridData, IsNullOrEmpty, SetMaskOnOff, FormatDate } from '../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { TextAreaCell } from "../../../Components/GridCell/TextAreaCell";
import ProjectFillFieldService from './ProjectFillFieldService';
import FileUploaderCell from '../../../Components/GridCell/FileUploaderCell';
import MultipleFileUploaderCell from '../../../Components/GridCell/MultipleFileUploaderCell';
import { downProjectAttachment, openProjectPrint } from '../../../Basic/CommonService';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import * as Yup from 'yup';

export const ProjectFillFieldMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            state: {
                projectNo,
                showSomeBtn
            } = {
                projectNo: "",
                showSomeBtn: null
            }
        }
    } = props;

    const [gridData, setGridData] = React.useState([]);
    const editedGridData = React.useRef([]);

    //取得實地查證
    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await ProjectFillFieldService.getProjectFactFinding(projectNo);
        setGridData(result);
        editedGridData.current = [];
        SetMaskOnOff(false);
    }
    React.useEffect(() => {
        loadData();
    }, [])

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = async (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SEQ');
    }

    /**
     * 分數欄位
     * @param {*} prop 
     * @returns 
     */
    const scoreCell = (prop) => {
        return (
            <td style={{ textAlign: "right" }}>
                {IsNullOrEmpty(prop.dataItem.FFSCORE) ? "無" : prop.dataItem.FFSCORE}
            </td>
        )
    }

    /**
     * 日期欄位
     * @param {*} prop 
     * @returns 
     */
    const formatDateCell = (prop) => {
        return (
            <td style={{ textAlign: "center" }}>
                {IsNullOrEmpty(prop.dataItem[prop.field]) ? "" : FormatDate(prop.dataItem[prop.field], 'tYY/MM/DD')}
            </td>
        )
    }

    /**
     * 多行文字顯示欄位
     * @param {*} prop 
     * @returns 
     */
    const textAreaWrapCell = (prop) => {
        return (
            <td>
                <TextAreaWrapInput value={prop.dataItem[prop.field]} />
            </td>
        )
    }

    /**
     * 管考檔案欄位
     * @param {*} prop 
     * @returns 
     */
    const showFileCell = (prop) => {
        let files = prop.dataItem.RdecFile;
        if (IsNullOrEmpty(files)) {
            return (
                <td></td>
            )
        }
        else {

            return (
                <td>
                    {files.map((file, index) => (
                        <div key={index}>
                            <a onClick={(e) => {
                                e.preventDefault();
                                downProjectAttachment(file.IDENTITY_FIELD);
                            }}>{file.FILE_NAME}</a>
                        </div>
                    ))}
                </td>
            )
        }
    }

    /**
     * 多行文字輸入框
     * @param {*} prop 
     * @returns 
     */
    const textAreaCell = (prop) => {
        if (IsNullOrEmpty(prop.dataItem.COMPLETEREPLYDATE)) {
            return (
                <td></td>
            )
        }
        return (
            <TextAreaCell
                {...prop}
                required={true}
                editable={true}
                rows={3}
                style={{ width: "100%" }}
                onCellInputBlur={onCellInputBlur}
                newDatas={true}
                IsAlwaysEdit={true}
            />
        )
    }

    /**
     * 上傳功能欄位
     * @param {*} prop 
     * @returns 
     */
    const fileUploaderCell = prop => {
        if (IsNullOrEmpty(prop.dataItem.COMPLETEREPLYDATE)) {
            return (
                <td></td>
            )
        }
        return (
            <MultipleFileUploaderCell
                dataItem={prop.dataItem}
                targetFile={prop.dataItem.HandFile}
                setFileChange={onFileChange}

            />
        )
    }

    /**
     * 檔案上傳 onCallBack 事件
     * @param {*} dataItem grid DataItem 資料
     */
    const onFileChange = (dataItem, fileInfo) => {
        let dataFiles = [];
        if (!IsNullOrEmpty(dataItem.FILE)) {
            // 未異動的檔案
            dataFiles = dataItem.FILE.filter(file =>
                file.editType == 0 && !fileInfo.find(f => file.IDENTITY_FIELD == f.EditFiles[0].FileId));
        }
        fileInfo.forEach(f => {
            // 新增
            if (f.editType == 1) {
                let item = {
                    ...f,
                    PROJECT_NO: dataItem.PROJECT_NO
                }
                dataFiles.push(item);
            }
            // 刪除
            else if (f.editType == 3) {
                dataFiles.push(f);
            }
        })
        dataItem.FILE = dataFiles;

        // editType : 1 (新增) , 2 (修改)
        dataItem.editType = dataItem.editType == 0 ? 2 : dataItem.editType;
        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'SEQ');
        setGridData(gridData, editedGridData.current);
    }


    // 實地查證情形欄位驗證
    const validateFactFindingField = Yup.object().shape({
        FFREPORT: Yup.string().nullable()
            .test('FFREPORT', '此為必填欄位', function (item) {
                return (!IsNullOrEmpty(this.parent.COMPLETEREPLYDATE) && IsNullOrEmpty(item)) ? false : true;
            })
    });

    // 存檔
    const save = async () => {
        //驗證
        let notValids = [];


        for (let index = 0; index < editedGridData.current.length; index++) {
            const item = editedGridData.current[index];

            let isValid = await validateFactFindingField.isValid(item);
            if (!isValid) {
                notValids.push({ key: index, value: isValid, item: item });
            }

            if (!IsNullOrEmpty(item.FILE)) {
                let haveNewFile = item.FILE.filter(x => x.editType == 1);
                if (IsNullOrEmpty(haveNewFile)) {
                    let haveDeleteFile = item.FILE.filter(x => x.editType == 3);
                    if (haveDeleteFile && haveDeleteFile.length == item.RdecFile.length) {
                        notValids.push({ key: index, value: isValid, item: "無上傳檔案" });
                    }
                }
            }
        }

        if (notValids.length > 0) {
            showGlobalMessageBox("請確認資料是否填妥及檔案是否上傳");
        }
        else {
            let saveData = editedGridData.current.map(x => {
                return { ...x, FFREPORT_DATE: FormatDate(new Date(), 'YYYY-MM-DD'), FileKind: "08",HandFile: x.FILE, }
            });
            SetMaskOnOff(true);
            let saveResult = await ProjectFillFieldService.saveProjectFactFinding(saveData);
            SetMaskOnOff(false);
            if (saveResult.success) {
                showGlobalMessageBox(saveResult.message, () => {
                    loadData();
                });
            }
            else {
                showGlobalMessageBox(saveResult.message);
            }
        }
    }

    return (
        <CollapseBoardCard button={
            <>
                {
                    showSomeBtn &&
                    <>
                        <Button title="存檔" onClick={() => save()}>存檔</Button>
                        <Button title="取消" className="fn-buttons k-button-lighten" onClick={() => loadData()}>取消</Button>
                    </>
                }
                <Button title="預覽列印" className="k-button-lighten" onClick={() => openProjectPrint(state)}>預覽列印</Button>
            </>}
            title="實地查證情形" isFirstArea={true} >

            <PageContainer style={{
                height: '100%',
                overflow: 'auto'
            }}>
                <Grid
                    style={{
                        height: '100%',
                        overflow: 'auto',
                    }}
                    resizable={true}
                    data={gridData}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn title="次數" cell={(props) =>
                        <td style={{ textAlign: "center" }}>{props.dataIndex + 1}</td>} width="40px" />
                    <GridColumn field="FFSCORE" title="分數" cell={scoreCell} width="50px" />
                    <GridColumn field="FFDATE" title="查證日期" cell={formatDateCell} width="90px" />
                    <GridColumn field="COMPLETEREPLYDATE" title="回覆期限" cell={formatDateCell} width="90px" />
                    <GridColumn field="FFCOMMENT" title="管考說明" cell={textAreaWrapCell} />
                    <GridColumn title="查證紀錄" cell={showFileCell} width="130px" />
                    <GridColumn field="FFREPORT" title="執行機關參採情形" cell={textAreaCell} headerCell={RequiredHeaderCell} />
                    <GridColumn title="查證參採資料" cell={fileUploaderCell} headerCell={props => {
                        return (
                            <CommonTooltip title={props.title} content={
                                <>
                                    可上傳檔案格式如下<br />
                                    jpg, jpeg, bmp, png,<br />
                                    mpg, doc, docx, ppt,<br />
                                    pptx, pdf, xls, xlsx,<br />
                                    odt, ods, odp, odg
                                </>} position="top"
                            />
                        )
                    }} />
                    <GridColumn field="FFREPORT_DATE" title="填報日期" cell={formatDateCell} width="90px" />
                </Grid>
            </PageContainer>
        </CollapseBoardCard>
    )
}
export default ProjectFillFieldMain;
import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { handleEditedGridData, IsNullOrEmpty, FormatDate } from '../../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { NumericTextInputCell } from '../../../../Components/GridCell/NumericTextInputCell';
import TwDatePicker from "../../../../Components/DateInputs/TwDatePicker";
import { TextAreaCell } from "../../../../Components/GridCell/TextAreaCell";
import { CommandCell } from "../../../../Components/GridCell/CommandCell";
import MultipleFileUploaderCell from '../../../../Components/GridCell/MultipleFileUploaderCell';
import { downProjectAttachment } from '../../../../Basic/CommonService';
import { RequiredHeaderCell } from '../../../../Components/GridCell/RequiredHeaderCell';
import TextAreaWrapInput from '../../../../Components/Input/TextAreaWrapInput';
import CommonTooltip from '../../../../Components/Tooltip/CommonTooltip';

export const ProjectFactFindingGrid = (props) => {
    const { projectNo, data, editedGridData, isRdecFun } = props;
    const [gridData, setGridData] = React.useState([]);

    React.useEffect(() => {
        setGridData(data);
        editedGridData.current = [];
    }, [data])

    // 新增
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            SEQ: 0,
            PROJECT_NO: projectNo,
            FFDATE: new Date(), //查證日期
            COMPLETEREPLYDATE: null, //回覆期限
            FFSCORE: null, //分數
            FFCOMMENT: "", //管考說明
            RdecFile: null, //管考檔案
            FFREPORT: null,
            HandFile: null,
            FFREPORT_DATE: null,
            editType: 1
        }
        editedGridData.current.push(newRecord);
        setGridData([...gridData, newRecord]);
    }

    /**
     * 刪除欄位
     * @param {*} prop 
     * @returns 
     */
    const DelCommandCell = (prop) => {
        return (
            <CommandCell>
                <Button title={"刪除"} icon='close' look='default' onClick={() => {
                    remove(prop.dataItem)
                }} />
            </CommandCell>
        )
    }

    // 移除
    const remove = (dataItem) => {
        let index = dataItem.hiddenIndex ?
            gridData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex)
            :
            gridData.findIndex(record => record.SEQ === dataItem.SEQ);

        gridData.splice(index, 1);
        dataItem.editType = 3;
        if (!IsNullOrEmpty(dataItem.RdecFile)) {
            dataItem.RdecFile.editType = 3;
        }
        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'SEQ');
        setGridData([...gridData]);
    }

    /**
     * 日期更改callback事件
     * @param {*} prop 
     * @param {*} value 
     */
    const onDateCellInputChange = async (prop, value) => {
        let item = prop.dataItem;
        if (item.editType !== 1) {
            item.editType = 2;
        }
        item[prop.field] = value;
        let index = item.hiddenIndex ?
            gridData.findIndex(record => record.hiddenIndex === item.hiddenIndex)
            :
            gridData.findIndex(record => record.SEQ === item.SEQ);
        gridData.splice(index, 1, item);

        setGridData([...gridData]);
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SEQ');
    }

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = async (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SEQ');
    }

    /**
     * 數字輸入框
     * @param {*} prop 
     * @returns 
     */
    const numericCell = (prop) => {
        return (
            <NumericTextInputCell
                {...prop}
                Numberformat={'n2'}
                max={100}
                IsForceMax={true}
                allowNull={true}
                min={0}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInputChange={onCellInputBlur}
                AlwaysEdit={true}
            />
        )
    }

    /**
     * 查證日期欄位
     * @param {*} prop 
     * @returns 
     */
    const ffDateCell = (prop) => {
        return (
            <td>
                <TwDatePicker
                    name={"FFDATE"}
                    format={"yyy/MM/dd"}
                    onChange={(e) => onDateCellInputChange(prop, e.value)}
                    value={IsNullOrEmpty(prop.dataItem.FFDATE) ? "" : new Date(prop.dataItem.FFDATE)}
                    width="100%"
                />
            </td>
        )
    }

    /**
     * 回覆期限
     * @param {*} prop 
     * @returns 
     */
    const completeReplyDateCell = (prop) => {
        return (
            <td>
                <TwDatePicker
                    name={"COMPLETEREPLYDATE"}
                    format={"yyy/MM/dd"}
                    onChange={(e) => onDateCellInputChange(prop, e.value)}
                    value={prop.dataItem.COMPLETEREPLYDATE == null ? null : new Date(prop.dataItem.COMPLETEREPLYDATE)}
                    width="100%"
                />
            </td>
        )
    }

    /**
     * 多行文字輸入框
     * @param {*} prop 
     * @returns 
     */
    const textAreaCell = (prop) => {
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
     * 上傳功能欄位
     * @param {*} prop 
     * @returns 
     */
    const fileUploaderCell = prop => {
        if (isRdecFun) {
            return (
                <MultipleFileUploaderCell
                    dataItem={prop.dataItem}
                    targetFile={prop.dataItem.RdecFile}
                    setFileChange={onFileChange}
                />
            )
        }
        else {
            return (
                <td>
                    {prop.dataItem.RdecFile.map(file => (
                        <div key={file.IDENTITY_FIELD}>
                            <a
                                href="#"
                                onClick={(e) => {
                                    e.preventDefault();
                                    downProjectAttachment(file.IDENTITY_FIELD);
                                }}
                                style={{ color: 'blue', textDecoration: 'underline', cursor: 'pointer' }}
                            >
                                {file.FILE_NAME}
                            </a>
                        </div>
                    ))}
                </td>
            )
        }
    }

    /**
     * 檔案上傳 onCallBack 事件
     * @param {*} dataItem grid DataItem 資料
     */
    const onFileChange = (dataItem, fileInfo) => {
        // 覆寫該列object的FILE
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

        // 該列object的editType
        dataItem.editType = dataItem.editType === 0 ? 2 : dataItem.editType;

        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'IDENTITY_FIELD');
        //紀錄有異動的資料
        setGridData(gridData, editedGridData.current);
        console.log(editedGridData)
    }

    /**
     * 主辦檔案欄位
     * @param {*} prop 
     * @returns 
     */
    const showFileCell = (prop) => {
        let files = prop.dataItem.HandFile;
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

    return (
        <>
            {isRdecFun &&
                <div className='fn-buttons'>
                    <Button type='button' title="新增" onClick={addNew} disabled={!isRdecFun}>新增</Button>
                </div>
            }
            <Grid
                style={{
                    height: '100%',
                    overflow: 'auto',
                }}
                resizable={true}
                data={gridData}
            >
                <GridNoRecords>無資料</GridNoRecords>
                {isRdecFun && <GridColumn title="刪除" cell={DelCommandCell} width="40px" />}
                <GridColumn title="次數" cell={(props) =>
                    <td style={{ textAlign: "center" }}>{props.dataIndex + 1}</td>} width="40px" />
                <GridColumn field="FFSCORE" title="分數" cell={numericCell} width="80px" />
                <GridColumn field="FFDATE" title="查證日期" cell={ffDateCell} width="125px" headerCell={RequiredHeaderCell} />
                <GridColumn field="COMPLETEREPLYDATE" title="回覆期限" cell={completeReplyDateCell} width="125px" />
                <GridColumn field="FFCOMMENT" title="管考說明" cell={textAreaCell} headerCell={RequiredHeaderCell} />
                <GridColumn title="查證紀錄" cell={fileUploaderCell} headerCell={props => {
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
                <GridColumn field="FFREPORT" title={<span>執行機關<br />參採情形</span>} cell={textAreaWrapCell} />
                <GridColumn title="查證參採資料" cell={showFileCell} />
                <GridColumn field="FFREPORT_DATE" title="填報日期" cell={formatDateCell} width="100px" />
            </Grid>
        </>
    )
}
export default ProjectFactFindingGrid;
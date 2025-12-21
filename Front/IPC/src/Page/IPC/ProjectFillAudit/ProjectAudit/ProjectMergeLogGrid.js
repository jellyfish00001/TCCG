import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { handleEditedGridData } from '../../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { DropDownListCell } from '../../../../Components/GridCell/DropDownListCell';
import TwDatePicker from "../../../../Components/DateInputs/TwDatePicker";
import { CommandCell } from "../../../../Components/GridCell/CommandCell";
import MultipleFileUploaderCell from '../../../../Components/GridCell/MultipleFileUploaderCell';
import { downProjectAttachment } from '../../../../Basic/CommonService';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { RequiredHeaderCell } from '../../../../Components/GridCell/RequiredHeaderCell';
import CommonTooltip from '../../../../Components/Tooltip/CommonTooltip';

//分案併案
export const ProjectMergeLogGrid = (props) => {
    const { projectNo, data, PromergeStatusDdlData, editedGridData, isRdecFun } = props;
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
            PROJECT_NO: projectNo,
            SEQ: 0,
            MERGE_STATUS: "",
            PROMERGE_DATE: new Date(),
            File: null,
            editType: 1,
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
        if (!IsNullOrEmpty(dataItem.File)) {
            dataItem.File.editType = 3;
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
     * 分案/併案欄位
     * @param {*} prop 
     * @returns 
     */
    const mergeStatusCell = prop => {
        return (
            <DropDownListCell
                {...prop}
                editable={true}
                ddlData={PromergeStatusDdlData}
                textField={'SET_VALUE'}
                dataItemKey={'SET_TYPE'}
                AlwaysEdit={true}
                onCellDDLChange={onCellInputBlur}
            />
        )
    }

    /**
     * 日期欄位
     * @param {*} prop 
     * @returns 
     */
    const twDatePickerCell = (prop) => {
        return (
            <td>
                <TwDatePicker
                    name="PROMERGE_DATE"
                    format={"yyy/MM/dd"}
                    onChange={(e) => onDateCellInputChange(prop, e.value)}
                    value={IsNullOrEmpty(prop.dataItem.PROMERGE_DATE) ? "" : new Date(prop.dataItem.PROMERGE_DATE)}
                    width="95%"
                />
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
                    field="File"
                    dataItem={prop.dataItem}
                    targetFile={prop.dataItem.File}
                    setFileChange={onFileChange}
                />
            )
        } else {
            return (
                <td>
                    {prop.dataItem.File && prop.dataItem.File.map(file => (
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
        let dataFiles = [];
        if (!IsNullOrEmpty(dataItem.File)) {
            // 未異動的檔案
            dataFiles = dataItem.File.filter(file =>
                file.editType === 0 && !fileInfo.find(f => file.IDENTITY_FIELD === f.EditFiles[0].FileId));
        }
        // 新增或刪除的檔案
        fileInfo.forEach(f => {
            // 新增
            if (f.editType === 1) {
                let item = {
                    ...f,
                    PROJECT_NO: dataItem.PROJECT_NO
                }
                dataFiles.push(item);
            }
            // 刪除
            else if (f.editType === 3) {
                dataFiles.push(f);
            }
        })
        dataItem.File = dataFiles;

        // 該列object的editType
        dataItem.editType = dataItem.editType === 0 ? 2 : dataItem.editType;

        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'SEQ');
        setGridData([...gridData]);
    }

    return (
        <>
            {isRdecFun && <Button type='button' title="新增" onClick={addNew}>新增</Button>}

            <Grid
                style={{
                    height: '100%',
                    overflow: 'auto',
                }}
                resizable={true}
                data={gridData}
            >
                <GridNoRecords>無資料</GridNoRecords>
                {isRdecFun && <GridColumn title="刪除" cell={DelCommandCell} width="50px" />}
                <GridColumn field="MERGE_STATUS" title="分案/併案" cell={mergeStatusCell} width="100px" headerCell={RequiredHeaderCell} />
                <GridColumn field="PROMERGE_DATE" title="核准日期" cell={twDatePickerCell} width="250px" headerCell={RequiredHeaderCell} />
                <GridColumn title="附件" cell={fileUploaderCell} headerCell={props => {
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
            </Grid>
        </>
    )
}
export default ProjectMergeLogGrid;
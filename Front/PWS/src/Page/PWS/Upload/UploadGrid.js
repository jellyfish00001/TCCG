import React, { useState, useRef, useEffect } from "react";
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import { orderBy } from "@progress/kendo-data-query";
import { downProjectAttachment } from '../../../Basic/CommonService';
import { handleEditedGridData } from '../../../Basic/SDOExtension';
import UploadFieldCell from './UploadFieldCell';
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell';
import { validateField } from './UploadService';
import * as Yup from 'yup';

export const UploadGrid = (props) => {
    const { projectNo, gridData, setGridData, editedGridData, fileKindDDL, showSomeBtn } = props;

    // 紀錄是否 grid data 有做異動
    const [isDataChange, setIsDataChange] = useState(false);
    const [newDatas, setNewDatas] = useState(false);
    // 排序
    const [sort, setSort] = useState([{ field: "", dir: "", }]);

    // 可切換欄位的 input onBlur 事件
    const onCellInputBlur = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'IDENTITY_FIELD');
        setNewDatas(!newDatas);
    }

    // 可切換欄位的 input onChange 事件
    const onCellInputChange = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'IDENTITY_FIELD');
    }

    /**
     * 刪除 grid 資料
     * @param {*} dataItem 
     */
    const remove = async (dataItem) => {
        let index = dataItem.hiddenIndex ? gridData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex) : gridData.findIndex(record => record.IDENTITY_FIELD === dataItem.IDENTITY_FIELD);
        gridData.splice(index, 1);
        dataItem.editType = 3;
        handleEditedGridData(editedGridData, dataItem, "hiddenIndex", "IDENTITY_FIELD");
        setGridData([...gridData]);
    }

    // 刪除欄
    const DelCommandCell = (prop) => {
        if (prop.dataItem.editable) {
            return (
                <CommandCell>
                    <Button title={"刪除"} icon='close' look='default' onClick={() => { remove(prop.dataItem) }} />
                </CommandCell>
            )
        } else {
            return ( <td></td> )
        }
    }
    
    /**
     * 檔案類型
     * @param {*} prop 
     * @returns 
     */
    const FileKindCell = prop => {
        if (prop.dataItem.editable) {
            return (
                <DropDownListCell
                    {...prop}
                    editable={true}
                    ddlData={fileKindDDL}
                    textField={'SET_VALUE'}
                    dataItemKey={'SET_TYPE'}
                    AlwaysEdit={true}
                    onCellDDLChange={onCellInputBlur}
                    customValidate={{ scheme: validateField, triggerValidate: true }}
                />
            )
        }
        else {
            return (
                <td>{prop.dataItem.NAME}</td>
            )
        }
    }

    // 檔案描述
    const FileMemoCell = prop => {
        if (prop.dataItem.editable) {
            return (
                <TextInputCell
                    {...prop}
                    maxlength={120}
                    required={true}
                    editable={true}
                    onCellInputBlur={onCellInputBlur}
                    onCellInputChange={onCellInputChange}
                    AlwaysEdit={true}
                />
            )
        } else {
            return ( <td>{prop.dataItem.FILE_MEMO}</td> )
        }
    }

    // 上傳功能欄位
    const fileUploaderCell = prop => {
        if (prop.dataItem.editable) {
            return (
                <UploadFieldCell dataItem={prop.dataItem} setFileChange={onFileChange} />
            )
        } else {
            return (
                <td><a onClick={(e) => { e.preventDefault(); downProjectAttachment(prop.dataItem.IDENTITY_FIELD) }}>{prop.dataItem.FILE_NAME}</a></td>
            )
        }
    }

    // 檔案上傳事件回調
    const onFileChange = (dataItem) => {
        dataItem.editType = dataItem.editType == 0 || dataItem.editType == undefined ? 2 : dataItem.editType;
        if (!dataItem.isUploaded && dataItem.FILE_NAME != null && dataItem.FILE_MEMO === "") {
            dataItem.FILE_MEMO = dataItem.FILE_NAME.slice(0, -dataItem.extension.length);
        }
        let isDataChange = handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'IDENTITY_FIELD');
        setIsDataChange(isDataChange);
        setGridData([...gridData]);
    }

    /**
     * 新增記錄
     */
    const addNew = () => {
        const indexs = gridData.map(item => item.index ? item.index : 0);
        const LasthiddenIndex = indexs.length > 0 ? indexs.sort((a, b) => a - b).pop() : 0;

        const newRecord = {
            hiddenIndex: `hand_${LasthiddenIndex + 1}`,
            index: LasthiddenIndex + 1,
            IDENTITY_FIELD: 0,
            PROJECT_NO: projectNo,
            FILE_MEMO: "",
            FILE_NAME: null,
            FILE_UP_SOURCE: "01",
            FILE_KIND: "06",
            editable: true,
            editType: 1,
            isUploaded: false,
        }
        editedGridData.current.push(newRecord);
        setGridData([...gridData, newRecord]);
    }

    /**
     * 排序變更
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    return (
        <>
            {showSomeBtn &&
                <div className="fn-buttons">
                    <Button title="新增" onClick={addNew} >新增</Button>
                </div>
            }
            <Grid
                style={{ height: '100%', overflow: 'auto' }}
                resizable={true}
                data={gridData}
                sort={sort}
                onSortChange={sortChange}
                sortable={{ allowUnsort: true, mode: "single" }}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn title="刪除" cell={DelCommandCell} width="50px" />
                <GridColumn field="FILE_KIND" title="檔案類型" cell={FileKindCell} headerCell={RequiredHeaderCell} width="180px" />
                <GridColumn field="FILE_MEMO" title="檔案描述" cell={FileMemoCell} headerCell={RequiredHeaderCell} />
                <GridColumn title="檔案名稱" cell={fileUploaderCell} headerCell={props => {
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
export default UploadGrid;

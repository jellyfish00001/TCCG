import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import { orderBy } from "@progress/kendo-data-query";
import { downProjectAttachment } from '../../../Basic/CommonService';
import { handleEditedGridData } from '../../../Basic/SDOExtension';
import ProjectFillFileUpCell from './ProjectFillFileUpCell';
import * as Yup from 'yup';

export const ProjectFillFileUpGrid = (props) => {
    const { projectNo,
        gridData,
        setGridData,
        editedGridData,
        fileKindDDL,
        type,
        showSomeBtn } = props;

    //紀錄是否grid data有做異動
    const [isDataChange, setIsDataChange] = React.useState(false);
    const [newDatas, setNewDatas] = React.useState(false);
    //排序
    const [sort, setSort] = React.useState([
        {
            field: "",
            dir: "",
        },
    ]);

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'IDENTITY_FIELD');
        setNewDatas(!newDatas);
    }

    /**
     * 供可切換欄位的input onChange callback的事件
     * @param {object} item
     */
    const onCellInputChange = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'IDENTITY_FIELD');
    }

    /**
     * 刪除grid資料
     * @param {*} dataItem 
     */
    const remove = async (dataItem) => {
        //判定刪除的是否為新增的資料
        let index = dataItem.hiddenIndex ?
            gridData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex)
            :
            gridData.findIndex(record => record.IDENTITY_FIELD === dataItem.IDENTITY_FIELD);

        gridData.splice(index, 1);
        dataItem.editType = 3;
        handleEditedGridData(editedGridData, dataItem, "hiddenIndex", "IDENTITY_FIELD");
        setGridData([...gridData]);
    }

    /**
     * 刪除欄
     * @param {*} prop 
     * @returns 
     */
    const DelCommandCell = (prop) => {
        if (prop.dataItem.editable) {
            return (
                <CommandCell>
                    <Button title={"刪除"} icon='close' look='default' onClick={() => {
                        remove(prop.dataItem)
                    }} />
                </CommandCell>
            )
        }
        else {
            return (
                <td></td>
            )
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

    /**
     * 檔案描述
     * @param {*} prop 
     * @returns 
     */
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
        }
        else {
            return (
                <td>{prop.dataItem.FILE_MEMO}</td>
            )
        }
    }

    /**
     * 是否公開
     * @param {*} prop 
     * @returns 
     */
    const IsDisplayCell = prop => {
        return (
            <DropDownListCell
                {...prop}
                editable={true}
                ddlData={[{ Value: "false", Text: "否" }, { Value: "true", Text: "是" }]}
                textField={'Text'}
                dataItemKey={'Value'}
                AlwaysEdit={true}
                onCellDDLChange={(item) => {
                    item.IS_DISPLAY = item.IS_DISPLAY == "true";
                    onCellInputBlur(item);
                }}
            />
        )
    }

    /**
     * 上傳功能欄位
     * @param {*} prop 
     * @returns 
     */
    const fileUploaderCell = prop => {
        if (prop.dataItem.editable) {
            return (
                <ProjectFillFileUpCell
                    dataItem={prop.dataItem}
                    setFileChange={onFileChange}
                />
            )
        }
        else {
            return (
                <td><a onClick={(e) => {
                    e.preventDefault();
                    downProjectAttachment(prop.dataItem.IDENTITY_FIELD)
                }}>{prop.dataItem.FILE_NAME}</a></td>
            )
        }
    }

    /**
     * 檔案上傳 onCallBack 事件
     * @param {*} dataItem grid DataItem 資料
     */
    const onFileChange = (dataItem) => {
        // editType : 1 (新增) , 2 (修改)
        dataItem.editType = dataItem.editType == 0 || dataItem.editType == undefined ? 2 : dataItem.editType;
        // 上傳檔案時，若「檔案描述」欄位內容為空值，預設帶入所選檔案名稱
        if (!dataItem.isUploaded && dataItem.FILE_NAME != null && dataItem.FILE_MEMO === "") {
            dataItem.FILE_MEMO = dataItem.FILE_NAME.slice(0, -dataItem.extension.length);
        }
        let isDataChange = handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'IDENTITY_FIELD');
        setIsDataChange(isDataChange);
        setGridData([...gridData]);
    }


    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const indexs = gridData.map(item => item.index ? item.index : 0);
        const LasthiddenIndex = indexs.length > 0 ? indexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: `${type}_${LasthiddenIndex + 1}`,
            index: LasthiddenIndex + 1,
            IDENTITY_FIELD: 0,
            PROJECT_NO: projectNo,
            FILE_KIND: "",
            FILE_MEMO: "",
            FILE_NAME: null,
            FILE_UP_SOURCE: "01",   //01:相關檔案上傳、02:其他地方上傳
            IS_DISPLAY: type == "rdec" ? false : null,   //是否同步顯示於主辦畫面(主辦存null)
            editable: true,
            editType: 1,
            isUploaded: false,
        }
        editedGridData.current.push(newRecord);
        setGridData([...gridData, newRecord]);
    }
    // 欄位驗證
    const validateField = Yup.object().shape({
        FILE_KIND: Yup.string().required("此欄位為必填"),
        FILE_MEMO: Yup.string().required("此欄位為必填").nullable(),
    });

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    return (
        <>
            {
                showSomeBtn &&
                <div className="fn-buttons">
                    <Button title="新增" onClick={addNew} >新增</Button>
                </div>
            }
            {type != "rdec" ?
                <Grid
                    style={{
                        height: '100%',
                        overflow: 'auto',
                    }}
                    resizable={true}
                    data={gridData}
                    sort={sort}
                    onSortChange={sortChange}
                    sortable={{ allowUnsort: true, mode: "single" }}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn title="刪除" cell={DelCommandCell} width="80px" headerCell={props => {
                        return (
                            <CommonTooltip title={props.title} content="管考檔案不能編輯" position="top" withoutRedStar={true} />
                        )
                    }}
                    />
                    <GridColumn field="FILE_KIND" title="檔案類型" cell={FileKindCell} headerCell={RequiredHeaderCell} width="180px" />
                    <GridColumn field="FILE_MEMO" title="檔案描述" cell={FileMemoCell} headerCell={props => {
                        return (
                            <CommonTooltip title={props.title} content="檔案描述說明" position="top" />
                        )
                    }} />
                    <GridColumn title="檔案名稱" cell={fileUploaderCell} headerCell={props => {
                        return (
                            <CommonTooltip title={props.title} content={
                                <>
                                    可上傳檔案格式如下<br />
                                    jpg, jpeg, bmp, png,<br />
                                    mpg, doc, docx, ppt,<br />
                                    pptx, pdf, xls, xlsx,<br />
                                    odt, ods, odp, odg
                                </>} position="top" />
                        )
                    }} />
                </Grid>
                :
                <Grid
                    style={{
                        height: '100%',
                        overflow: 'auto',
                    }}
                    resizable={true}
                    data={gridData}
                    sort={sort}
                    onSortChange={sortChange}
                    sortable={{ allowUnsort: true, mode: "single" }}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn title="刪除" cell={DelCommandCell} width="50px" />
                    <GridColumn field="FILE_KIND" title="檔案類型" width="180px" cell={FileKindCell} headerCell={RequiredHeaderCell} />
                    <GridColumn field="FILE_MEMO" title="檔案描述" cell={FileMemoCell} headerCell={RequiredHeaderCell} />
                    <GridColumn field="IS_DISPLAY" title="公開" width="80px" cell={IsDisplayCell} />
                    <GridColumn title="檔案名稱" cell={fileUploaderCell} headerCell={props => {
                        return (
                            <CommonTooltip title={props.title} content={
                                <>
                                    可上傳檔案格式如下<br />
                                    jpg, jpeg, bmp, png,<br />
                                    mpg, doc, docx, ppt,<br />
                                    pptx, pdf, xls, xlsx,<br />
                                    odt, ods, odp, odg
                                </>} position="top" />
                        )
                    }} />
                </Grid>
            }
        </>
    )
}
export default ProjectFillFileUpGrid;
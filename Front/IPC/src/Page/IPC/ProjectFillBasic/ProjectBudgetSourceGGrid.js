
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import React, { useRef } from 'react';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell';
import { handleEditedGridData, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { getBudgetSourceAllDropDowns, validataSourceGField } from './ProjectFillBasicService';
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import { formatNumber } from '@telerik/kendo-intl';
import MultipleFileUploaderCell from '../../../Components/GridCell/MultipleFileUploaderCell';
import { GetBasicData } from '../../../Basic/BasicData';

const ProjectBudgetSourceGGrid = (props) => {

    const { projectBudgetSourceG, setEditedGridData, editedBudgetSourceG } = props

    //#region 參數宣告
    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState(
        {
            PLAN_YEAR: [],
            BUDGET_CLASS: [],
            PLAN_ITEM_C: [],
            PLAN_ITEM_L: []
        });

    const [gridData, setGridData] = React.useState([]);

    const orgId = useRef("");

    // 紀錄異動資料
    const editedGridData = React.useRef([]);

    // 計畫總經費
    const [totalAmount, setTotalAmount] = React.useState("0");

    // 觸發Grid驗證
    const [triggerValidation, setTriggerValidation] = React.useState(true);

    // 紀錄是否有新增資料
    const [newDatas, setNewDatas] = React.useState(false);

    //#endregion

    // 刪除欄位
    const deleteCell = props => {
        return (
            <CommandCell>
                <Button type="button" title="刪除" icon="close" look="default"
                    onClick={() => remove(props.dataItem)}
                />
            </CommandCell>
        )
    }

    // 移除
    const remove = dataItem => {
        let index = dataItem.hiddenIndex ?
            gridData.findIndex(d => d.hiddenIndex === dataItem.hiddenIndex)
            :
            gridData.findIndex(d => d.IDENTITY_FIELD === dataItem.IDENTITY_FIELD);
        gridData.splice(index, 1);
        dataItem.editType = 3
        const { FILE } = dataItem
        if (!IsNullOrEmpty(FILE)) {
            FILE.editType = 3
        }
        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'IDENTITY_FIELD');
        setGridData([...gridData]);
        //紀錄有異動的資料
        setEditedGridData(gridData, editedGridData.current);
        //計算"計畫總經費"
        calculateTotalCount(gridData);
    }

    // 下拉選單
    const commonDdlCell = (props, data, textField, dataItemKey) => {
        return (
            <DropDownListCell
                {...props}
                newDatas={newDatas}
                editable={true}
                AlwaysEdit={true}
                ddlData={data}
                textField={textField}
                dataItemKey={dataItemKey}
                onCellDDLChange={(e) => callBackEvent(e, true)}
                selectWidth={"100%"}
                customValidate={{ scheme: validataSourceGField, triggerValidate: triggerValidation }}
            />
        );
    }

    // 數字輸入框Cell
    const numericInputCell = props => {
        return (
            <NumericTextInputCell
                {...props}
                required={true}
                editable={true}
                AlwaysEdit={true}
                min={0}
                onCellInputBlur={(e) => callBackEvent(e, true)}
                customValidate={{ scheme: validataSourceGField, triggerValidate: triggerValidation }}
            />
        );

    }

    // 供可切換欄位的 onChange or onBlur callback的事件
    const callBackEvent = (item, setProps = false) => {
        item.disabled = IsNullOrEmpty(item.PLAN_ITEM_C);
        let data = item.hiddenIndex
            ? gridData.find(x => x.hiddenIndex === item.hiddenIndex)
            : gridData.find(x => x.IDENTITY_FIELD === item.IDENTITY_FIELD);
        data.disabled = item.disabled;

        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'IDENTITY_FIELD');
        //是否要更動props，當ddl onChange 跟 input onBlur 需更動
        if (setProps) {
            //紀錄有異動的資料
            setEditedGridData(gridData, editedGridData.current);
            //計算"計畫總經費"
            calculateTotalCount(gridData);
        }
        // 會重新render grid，藉此完整觸發grid欄位驗證
        // (因下拉選單改變，需觸發數字框的驗證，故透過重新render方式觸發完整驗證)
        setGridData([...gridData]);
    }

    // 上傳檔案欄位
    const fileUploaderCell = (prop) => {
        return (
            <>
                <MultipleFileUploaderCell
                    field="FILE"
                    dataItem={prop.dataItem}
                    targetFile={prop.dataItem.FILE}
                    setFileChange={onFileChange}
                    customValidate={{ scheme: validataSourceGField, triggerValidate: triggerValidation }}
                    disabled={prop.dataItem.disabled}
                />
            </>
        );
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
                    // 依據預算類型寫入檔案類別 06: 一般預算 05: 前瞻計畫
                    FILE_KIND: dataItem.BUDGET_CLASS == "0" ? "06" : "05",
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
        setEditedGridData(gridData, editedGridData.current);
    }

    // 計算"計畫總經費"
    const calculateTotalCount = (data) => {
        let value = 0;
        data.map(x => {
            value += x.BUDGET_CENTRAL + x.BUDGET_LOCAL
        })
        setTotalAmount(formatNumber(value, 'N0'));
    }

    // 新增
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            IDENTITY_FIELD: 0,
            PROJECT_NO: "",
            PLAN_YEAR: (new Date().getFullYear() - 1911).toString(),
            BUDGET_CLASS: "",
            PLAN_ITEM_C: "",
            BUDGET_CENTRAL: 0,
            PLAN_ITEM_L: orgId.current,
            BUDGET_LOCAL: 0,
            FILE: null,
            disabled: true,
            editType: 1
        }
        setGridData([...gridData, newRecord]);
        setNewDatas(true);
    }

    // 載入資料
    const loadData = async () => {
        let result = await getBudgetSourceAllDropDowns();
        orgId.current = await GetBasicData("orgId")
        if (result.length > 0) {
            setDdlData({
                PLAN_YEAR: [...result[0]],
                BUDGET_CLASS: [...result[1]],
                PLAN_ITEM_C: [...result[2]],
                PLAN_ITEM_L: [...result[3]]
            })
        }
    }

    React.useEffect(() => {
        loadData();
    }, [])

    React.useEffect(() => {
        setGridData(projectBudgetSourceG);
        //計算"計畫總經費"
        calculateTotalCount(projectBudgetSourceG);
        editedGridData.current = editedBudgetSourceG;
    }, [projectBudgetSourceG])

    return (
        <>
            <div className="fn-buttons">
                <Button title="新增預算" type="button" onClick={() => addNew()}>新增預算</Button>
            </div>
            <Grid
                style={{
                    height: '100%'
                }}
                data={gridData}
                resizable={true}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn title="刪除" cell={deleteCell} width="50px" />
                <GridColumn title="年度" field="PLAN_YEAR" cell={(e) => commonDdlCell(e, ddlData.PLAN_YEAR, "text", "value")} width="95px" />
                <GridColumn title="預算類型" field="BUDGET_CLASS"
                    cell={(e) => commonDdlCell(e, ddlData.BUDGET_CLASS, "SET_VALUE", "SET_TYPE")} width="110px" />
                <GridColumn title="中央預算來源" field="PLAN_ITEM_C"
                    cell={(e) => commonDdlCell(e, ddlData.PLAN_ITEM_C, "PLAN_ITEM_NAME", "PLAN_ITEM_ID")} width="140px" />
                <GridColumn title="中央補助款(元)" field="BUDGET_CENTRAL" cell={numericInputCell} width="150px" />
                <GridColumn title="本府預算來源" field="PLAN_ITEM_L"
                    cell={(e) => commonDdlCell(e, ddlData.PLAN_ITEM_L, "PLAN_ITEM_NAME", "PLAN_ITEM_ID")} width="140px" />
                <GridColumn title="本府預算金額(元)" field="BUDGET_LOCAL" cell={numericInputCell} width="150px" />
                <GridColumn title="補助經費核定文件" cell={fileUploaderCell} />
            </Grid>
            計畫總經費 {totalAmount} 元
        </>
    );
}
const areEqual = (prevProps, nextProps) => {
    /*
    return true if passing nextProps to render would return
    the same result as passing prevProps to render,
    otherwise return false
    */
    return prevProps.fillBasicData === nextProps.fillBasicData;
}
export default React.memo(ProjectBudgetSourceGGrid, areEqual);
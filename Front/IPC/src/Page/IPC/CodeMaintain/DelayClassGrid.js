import React from 'react';
import * as Yup from 'yup';
import { PageContainer } from "../../../Basic/PageContainer";
import { handleEditedGridData, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { GetSetParam } from '../../../Basic/CommonService';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell';
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { orderBy } from "@progress/kendo-data-query";

import { SetCodeService } from './SetCodeService';
import DelayClassDetailGrid from './DelayClassDetailGrid';

export const DelayClassGrid = () => {
    //落後項目資料
    const [gridData, setGridData] = React.useState([]);
    const [expanded, setexpanded] = React.useState(false);
    //紀錄異動資料
    const editedGridData = React.useRef([]);
    const [newDatas, setNewDatas] = React.useState(false);
    //排序
    const [sort, setSort] = React.useState([{ field: "", dir: "" }]);

    React.useEffect(() => {
        loadData();
    }, [])

    /**
     * 載入落後項目類別
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let data = await GetSetParam("DELAY_TYPE", "", false, null);
        let delayClasses = await SetCodeService.loadDelayClasses();
        data.forEach(x => {
            x.IS_DISABLED = delayClasses.find(y => y === x.OLD_SET_TYPE) !== undefined;
        });
        setGridData([...data]);
        setNewDatas(false);
        editedGridData.current = [];
        SetMaskOnOff(false);
    }

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SET_TYPE');
        setNewDatas(!newDatas);
    }

    /**
     * 供可切換欄位的input onChange callback的事件
     * @param {object} item
     */
    const onCellInputChange = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SET_TYPE');
    }

    /**
     * 展開欄位
     * @param {*} props 
     * @returns 
     */
    const ExpandCell = (props) => {
        const ExpandedButton = () => {
            // 新增的話隱藏展開按鈕
            if (props.dataItem.editType === 1) {
                return <></>;
            }
            return (
                <Button title={"展開明細"} icon={!props.dataItem.expanded ? 'plus' : 'minus'} look='default' onClick={(e) => {
                    let newData = gridData.map((item) => {
                        if (item.SET_TYPE === props.dataItem.SET_TYPE) {
                            item.expanded = !props.dataItem.expanded;
                        } else {
                            item.expanded = false;
                        }
                        return item;
                    });
                    setexpanded(!props.dataItem.expanded);
                    setGridData(newData);
                }} />
            )
        }
        return (
            <CommandCell>
                {ExpandedButton()}
            </CommandCell>
        )
    };

    /**
     * 是否刪除cell
     * @param {*} prop 
     * @returns 
     */
    const IsDelCell = prop => {
        return (
            <DropDownListCell
                {...prop}
                editable={true}
                ddlData={[{ Value: true, Text: "是" }, { Value: false, Text: "否" }]}
                textField={'Text'}
                dataItemKey={'Value'}
                AlwaysEdit={true}
                onCellDDLChange={onCellInputBlur}
            />
        )
    }

    /**
     * 排序
     * @param {*} prop 
     * @returns 
     */
    const SortOrderCell = prop => {
        return (
            <NumericTextInputCell
                {...prop}
                Numberformat={'n0'}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInputChange={onCellInputChange}
                AlwaysEdit={true}
                customValidate={{ scheme: validateHelper, triggerValidate: true }}
            />
        )
    }

    /**
     * 文字輸入框
     * @param {*} prop 
     * @returns 
     */
    const TextCell = prop => {
        if (prop.dataItem.IS_DISABLED === false) {
            return (
                <TextInputCell
                    {...prop}
                    maxlength={prop.field === "SET_TYPE" ? 20 : 50}
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
                <td style={{ padding: '7px 11px' }}>{prop.dataItem[prop.field]}</td>
            )
        }

    }

    /**
     * 新增
     */
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            SET_ITEM: "DELAY_TYPE",
            SET_TYPE: "",
            OLD_SET_TYPE: "",
            SET_VALUE: "",
            MEMO: "",
            DEL_FLG: false,
            SORT_ORDER: 0,
            editType: 1,
            IS_DISABLED: false
        }
        editedGridData.current.push(newRecord);
        setGridData([...gridData, newRecord]);
        setNewDatas(true);
    }

    /**
     * 準備欄位驗證工具
     */
    const validateHelper = Yup.object().shape({
        SET_ITEM: Yup.string().required('此欄位為必填'),
        SET_VALUE: Yup.string().required('此欄位為必填'),
        SORT_ORDER: Yup.number().integer("須為整數").required('此欄位為必填'),
    });

    /**
     * 存檔
     */
    const save = async () => {
        // 驗證
        let notValids = [];
        for (let index = 0; index < gridData.length; index++) {
            let isValid = await validateHelper.isValid(gridData[index]);
            if (!isValid) {
                notValids.push(isValid);
            }
        }
        if (notValids.length > 0) {
            showGlobalMessageBox('儲存失敗，請確認資料填妥後重新嘗試!');
            return;
        }

        // 判斷 類別代碼 不能重複
        const setTypes = gridData.map(x => x.SET_TYPE.trim());
        if (setTypes.length !== new Set(setTypes).size) {
            showGlobalMessageBox('儲存失敗，請確認"類別代碼"是否重複!');
            return;
        }

        // 判斷 落後類別 不能重複
        const setValues = gridData.map(x => x.SET_VALUE.trim());
        if (setValues.length !== new Set(setValues).size) {
            showGlobalMessageBox('儲存失敗，請確認"落後類別"是否重複!');
            return;
        }

        SetMaskOnOff(true);
        const saveResult = await SetCodeService.saveParam(editedGridData.current);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => loadData());
        }
        else {
            showGlobalMessageBox(saveResult.message);
        }
    }

    /**
     * 展開grid
     */
    const DetailComponent = (props) => {
        return (
            <DelayClassDetailGrid
                data={props.dataItem}
            />
        )
    }

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    return (
        <PageContainer
            toolbar={
                <>
                    <Button title="存檔" onClick={save} >存檔</Button>
                    <Button title="取消" className="k-button-lighten" onClick={loadData} >取消</Button>
                    <Button title="新增落後類別" onClick={addNew} >新增落後類別</Button>
                </>
            }
        >
            <Grid
                data={gridData}
                resizable={true}
                detail={DetailComponent}
                expandField="expanded"
                sort={sort}
                onSortChange={sortChange}
                sortable={{
                    allowUnsort: true,
                    mode: "single",
                }}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn cell={ExpandCell} width="50px" title="展開" />
                <GridColumn field="DEL_FLG" headerClassName={'breakSpacehHeader'} title="停用"
                    cell={IsDelCell} width="80px" />
                <GridColumn field="SORT_ORDER" headerClassName={'breakSpacehHeader'} title="排序"
                    cell={SortOrderCell} headerCell={RequiredHeaderCell} width="80px" />
                <GridColumn field="SET_TYPE" headerClassName={'breakSpacehHeader'} title="類別代碼"
                    cell={TextCell} headerCell={RequiredHeaderCell} width="120px" />
                <GridColumn field="SET_VALUE" headerClassName={'breakSpacehHeader'} title="落後類別"
                    cell={TextCell} headerCell={RequiredHeaderCell} />
            </Grid>
        </PageContainer>
    );
}
export default DelayClassGrid
import React from 'react';
import * as Yup from 'yup';
import { PageContainer } from "../../../Basic/PageContainer";
import { handleEditedGridData, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { GetSetParam, GetUsedCode } from '../../../Basic/CommonService';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell'
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell'
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell'
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';

import { SetCodeService } from './SetCodeService';

export const SpecialCodeGrid = () => {

    const [gridData, setGridData] = React.useState([]);
    const [newData, setNewDatas] = React.useState(false);
    const editedGridData = React.useRef([]);

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = async () => {
        SetMaskOnOff(true);
        let data = await GetSetParam('SPEC_NOTE', "", false, null);
        // 取得已使用代碼清單
        let usedCodes = await GetUsedCode('SPEC_NOTE');
        data.forEach(x => {
            x.IS_DISABLED = usedCodes.find(y => y === x.OLD_SET_TYPE) !== undefined;
        })
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
        setNewDatas(!newData);
    }

    /**
     * 供可切換欄位的input onChange callback的事件
     * @param {object} item
     */
    const onCellInputChange = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SET_TYPE');
    }

    //是否停用cell
    const IsDelCell = props => {
        return (
            <DropDownListCell
                {...props}
                ddlData={[{ value: false, text: "否" }, { value: true, text: "是" }]}
                textField={'text'}
                dataItemKey={'value'}
                width={"100%"}
                editable={true}
                AlwaysEdit={true}
                onCellDDLChange={(item) => {
                    handleEditedGridData(editedGridData, item, "hiddenIndex", "SET_TYPE");
                    setNewDatas(!newData);
                }}
            >
            </DropDownListCell>
        )
    }

    // 文字輸入框
    const TextCell = props => {
        if (props.dataItem.IS_DISABLED === false) {
            return (
                <TextInputCell
                    {...props}
                    maxlength={props.field === 'SET_TYPE' ? 20 : 50}
                    required={true}
                    editable={true}
                    onCellInputBlur={onCellInputBlur}
                    onCellInputChange={onCellInputChange}
                    AlwaysEdit={true}
                />
            )
        } else {
            const alignment = props.field === 'SET_TYPE' ? 'center' : 'left';
            return (
                <td style={{ textAlign: alignment, padding: '7px 11px' }}>{props.dataItem[props.field]}</td>
            )
        }
    }

    // 數字輸入框
    const SetNumericInputCell = props => {
        return (
            <NumericTextInputCell
                {...props}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInputChange={onCellInputChange}
                AlwaysEdit={true}
            />
        )
    }

    // 欄位驗證
    const validataHelper = Yup.object().shape({
        SET_TYPE: Yup.string().required('此欄位為必填'),
        SET_VALUE: Yup.string().required('此欄位為必填'),
        SORT_ORDER: Yup.number().required("請輸入數字")
    })

    // 新增
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            SET_ITEM: 'SPEC_NOTE',
            DEL_FLG: false,
            SET_TYPE: "",
            OLD_SET_TYPE: "",
            SET_VALUE: "",
            SORT_ORDER: 0,
            editType: 1,
            newItem: true,
            IS_DISABLED: false
        }
        setGridData([...gridData, newRecord]);
        setNewDatas(true);
    }

    // 存檔
    const saveChanges = async () => {
        // 驗證
        let notValids = [];
        for (let index = 0; index < gridData.length; index++) {
            let isValid = await validataHelper.isValid(gridData[index]);
            if (!isValid) {
                notValids.push(isValid);
            }
        }
        if (notValids.length > 0) {
            showGlobalMessageBox('儲存失敗，請確認資料填妥後重新嘗試!');
            return;
        }

        // 判斷 設定代碼 不能重複
        const setTypes = gridData.map(x => x.SET_TYPE.trim());
        if (setTypes.length !== new Set(setTypes).size) {
            showGlobalMessageBox('儲存失敗，請確認"設定代碼"是否重複!');
            return;
        }

        // 判斷 特殊註記項目 不能重複
        const setValues = gridData.map(x => x.SET_VALUE.trim());
        if (setValues.length !== new Set(setValues).size) {
            showGlobalMessageBox('儲存失敗，請確認"特殊註記項目"是否重複!');
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

    return (
        <PageContainer
            toolbar={
                <>
                    <Button title="存檔" onClick={saveChanges} >存檔</Button>
                    <Button title="取消" className="k-button-lighten" onClick={loadData} >取消</Button>
                    <Button title="新增特殊加註" onClick={addNew} >新增特殊加註</Button>
                </>
            }
        >
            <Grid
                data={gridData}
                resizable={true}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="DEL_FLG" title="停用" cell={IsDelCell} width="100px" />
                <GridColumn field="SET_TYPE" title="設定代碼" cell={TextCell} headerCell={RequiredHeaderCell} width="100px" />
                <GridColumn field="SET_VALUE" title="特殊註記項目" cell={TextCell} headerCell={RequiredHeaderCell} />
                <GridColumn field="SORT_ORDER" title="排序" cell={SetNumericInputCell} headerCell={RequiredHeaderCell} width="100px" />
            </Grid>
        </PageContainer>
    );
}
export default SpecialCodeGrid
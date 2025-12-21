import React from 'react';
import * as Yup from 'yup';
import { PageContainer } from "../../../Basic/PageContainer";
import { handleEditedGridData, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell';
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { orderBy } from "@progress/kendo-data-query";

import { SetCodeService } from './SetCodeService';

export const CodePlanItemGrid = ({ levelMark }) => {
    //預算來源資料
    const [gridData, setGridData] = React.useState([]);
    //紀錄異動資料
    const editedGridData = React.useRef([]);
    const [newDatas, setNewDatas] = React.useState(false);
    //排序
    const [sort, setSort] = React.useState([{ field: "", dir: "" }]);

    React.useEffect(() => {
        loadData();
    }, [])

    /**
     * 載入預算來源
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let data = await SetCodeService.loadCodePlanItem(levelMark);
        let planItems = await SetCodeService.loadPlanItems(levelMark);
        data.forEach(x => {
            x.IS_DISABLED = planItems.find(y => y === x.OLD_PLAN_ITEM_ID) !== undefined;
        });
        setGridData(data);
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
     * 文字輸入框
     * @param {*} prop 
     * @returns 
     */
    const TextCell = prop => {
        if (prop.dataItem.IS_DISABLED === false) {
            return (
                <TextInputCell
                    {...prop}
                    maxlength={prop.field === "PLAN_ITEM_ID" ? 30 : 160}
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
            PLAN_YEAR: 999,
            PLAN_ITEM_ID: "",
            OLD_PLAN_ITEM_ID: "",
            PLAN_ITEM_NAME: "",
            PLAN_ITEM_BUDGET: 0,
            LEVEL_MARK: levelMark,
            DEL_FLG: false,
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
        PLAN_ITEM_ID: Yup.string().required('此欄位為必填'),
        PLAN_ITEM_NAME: Yup.string().required('此欄位為必填'),
    });

    /**
     * 存檔
     */
    const save = async () => {
        //驗證
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

        // 判斷 預算編號 不能重複
        let planItemIds = gridData.map(x => x.PLAN_ITEM_ID.trim());
        if (planItemIds.length !== new Set(planItemIds).size) {
            showGlobalMessageBox('儲存失敗，請確認"預算編號"是否重複!');
            return;
        }

        // 判斷 預算名稱 不能重複
        let planItemNames = gridData.map(x => x.PLAN_ITEM_NAME.trim());
        if (planItemNames.length !== new Set(planItemNames).size) {
            showGlobalMessageBox('儲存失敗，請確認"預算名稱"是否重複!');
            return;
        }

        SetMaskOnOff(true);
        let saveResult = await SetCodeService.saveCodePlanItem(editedGridData.current);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => loadData());
        }
        else {
            showGlobalMessageBox(saveResult.message);
        }
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
                    <Button title="新增預算來源" onClick={addNew} >新增預算來源</Button>
                </>
            }
        >
            <Grid
                data={gridData}
                resizable={true}
                sort={sort}
                onSortChange={sortChange}
                sortable={{
                    allowUnsort: true,
                    mode: "single",
                }}>
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="DEL_FLG" headerClassName={'breakSpacehHeader'} title="停用"
                    cell={IsDelCell} width="80px" />
                <GridColumn field="PLAN_ITEM_ID" headerClassName={'breakSpacehHeader'} title="預算編號"
                    cell={TextCell} headerCell={RequiredHeaderCell} width="120px" />
                <GridColumn field="PLAN_ITEM_NAME" headerClassName={'breakSpacehHeader'} title="預算名稱"
                    cell={TextCell} headerCell={RequiredHeaderCell} />
            </Grid>
        </PageContainer>
    );
}
export default CodePlanItemGrid
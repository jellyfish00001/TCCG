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

export const DelayClassDetailGrid = (props) => {
    // 記錄第二層資料
    const [gridData, setGridData] = React.useState([]);
    const [newDatas, setNewDatas] = React.useState(false);
    //紀錄異動資料
    const editedGridData = React.useRef([]);
    //排序
    const [sort, setSort] = React.useState([{ field: "", dir: "" }]);

    React.useEffect(() => {
        loadData();
    }, [])

    /**
     * 載入落後項目
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let data = await SetCodeService.loadCodeDelayClass(props.data.SET_TYPE);
        let delaySubClasses = await SetCodeService.loadDelaySubClasses();
        data.forEach(x => {
            x.IS_DISABLED = delaySubClasses.find(y => y === x.OLD_DELAY_CLASS_SUB_ID) !== undefined;
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
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'DELAY_CLASS_SUB_ID');
        setNewDatas(!newDatas);
    }

    /**
     * 供可切換欄位的input onChange callback的事件
     * @param {object} item
     */
    const onCellInputChange = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'DELAY_CLASS_SUB_ID');
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
                    maxlength={prop.field === "DELAY_CLASS_SUB_ID" ? 3 : 80}
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
            DELAY_CLASS_ID: props.data.SET_TYPE,
            DELAY_CLASS_ITEM: props.data.SET_VALUE,
            DELAY_CLASS_SUB_ID: "",
            OLD_DELAY_CLASS_SUB_ID: "",
            DELAY_CLASS_SUB_ITEM: "",
            IS_SYSTEM: false,
            IS_ENABLED: true,
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
        DELAY_CLASS_SUB_ID: Yup.string().required('此欄位為必填'),
        DELAY_CLASS_SUB_ITEM: Yup.string().required('此欄位為必填'),
    });

    /**
     * 存檔
     */
    const save = async () => {
        //驗證
        let notValids = [];
        for (let index = 0; index < gridData.length; index++) {
            const item = gridData[index];
            let isValid = await validateHelper.isValid(item);
            if (!isValid) {
                notValids.push(isValid);
            }
        }
        if (notValids.length > 0) {
            showGlobalMessageBox('儲存失敗，請確認資料填妥後重新嘗試!');
            return;
        }

        // 判斷 落後項目代碼 不能重複
        let subIds = gridData.map(x => x.DELAY_CLASS_SUB_ID.trim());
        if (subIds.length !== new Set(subIds).size) {
            showGlobalMessageBox('儲存失敗，請確認落後項目代碼是否重複!');
            return;
        }

        // 判斷 落後項目 不能重複
        let subItems = gridData.map(x => x.DELAY_CLASS_SUB_ITEM.trim());
        if (subItems.length !== new Set(subItems).size) {
            showGlobalMessageBox('儲存失敗，請確認落後項目是否重複!');
            return;
        }

        SetMaskOnOff(true);
        // 如果新增將新ID放入舊ID檢查是否使用過 
        // DELAY_CLASS_SUB_ID 放入 OLD_DELAY_CLASS_SUB_ID
        editedGridData.current.forEach(x => {
            if (x.editType === 1) {
                x.OLD_DELAY_CLASS_SUB_ID = x.DELAY_CLASS_SUB_ID;
            }
        });
        let saveResult = await SetCodeService.saveCodeDelayClass(editedGridData.current);
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
        <PageContainer style={{
            height: '100%',
            overflow: 'auto'
        }}
            toolbar={
                <>
                    <Button className={"detailButton"} title="存檔" onClick={save} >存檔</Button>
                    <Button className={"detailButton k-button-lighten"} title="取消" onClick={loadData} >取消</Button>
                    <Button className={"detailButton"} title="新增落後項目" onClick={addNew}>新增落後項目</Button>
                </>
            }
        >
            <Grid
                style={{
                    textAlign: "center",
                    height: '100%',
                    overflow: 'auto',
                }}
                data={gridData}
                resizable={true}
                sort={sort}
                onSortChange={sortChange}
                sortable={{
                    allowUnsort: true,
                    mode: "single",
                }}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="DEL_FLG" headerClassName={'breakSpacehHeader'} title="停用"
                    cell={IsDelCell} width="80px" />
                <GridColumn field="DELAY_CLASS_SUB_ID" headerClassName={'breakSpacehHeader'} title="落後項目代碼"
                    cell={TextCell} headerCell={RequiredHeaderCell} width="120px" />
                <GridColumn field="DELAY_CLASS_SUB_ITEM" headerClassName={'breakSpacehHeader'} title="落後項目"
                    cell={TextCell} headerCell={RequiredHeaderCell} />
            </Grid>
        </PageContainer>
    )
}
export default DelayClassDetailGrid;
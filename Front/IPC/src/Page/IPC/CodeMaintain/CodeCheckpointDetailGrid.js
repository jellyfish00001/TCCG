import React from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import { handleEditedGridData, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell';
import { SetCodeService } from './SetCodeService';
import { orderBy } from "@progress/kendo-data-query";
import * as Yup from 'yup';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';

export const CodeCheckPointDetailGrid = (props) => {
    // 記錄第二層資料
    const [gridData, setGridData] = React.useState([]);
    // 控制點下拉選單資料
    const paramDdlData = props.ctrlPointData;
    // gridData是否有異動
    const [isDataChange, setIsDataChange] = React.useState(false);
    const [newDatas, setNewDatas] = React.useState(false);
    //紀錄異動資料
    const editedGridData = React.useRef([]);
    //排序
    const [sort, setSort] = React.useState([
        {
            field: "",
            dir: "",
        },
    ]);

    /**
     * 載入自訂檢核點
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let data = await SetCodeService.loadCusItem(props.data.CHECKPOINT_CLASS_ID);
        setGridData(data);
        setIsDataChange(false);
        setNewDatas(false);
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
    const onCellInputBlur = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SEQ');
        setIsDataChange(editedGridData.current.length > 0);
        setNewDatas(!newDatas);
    }

    /**
     * 供可切換欄位的input onChange callback的事件
     * @param {object} item
     */
    const onCellInputChange = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'SEQ');
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
     * 檢核點項目
     * @param {*} props 
     * @returns 
     */
    const NameCell = props => {
        return (
            <TextInputCell
                {...props}
                maxlength={50}
                required={true}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInputChange={onCellInputChange}
                AlwaysEdit={true}
            />
        )
    }

    /**
     * 進度
     * @param {*} props 
     * @returns 
     */
    const ProgressCell = props => {
        return (
            <NumericTextInputCell
                {...props}
                Numberformat={'n0'}
                max={100}
                IsForceMax={true}
                required={true}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInputChange={onCellInputChange}
                AlwaysEdit={true}
                customValidate={{ scheme: validateHelper, triggerValidate: true }}
            />
        )
    }

    /**
     * 工作項目
     * @param {*} prop 
     * @returns 
     */
    const CtrlPointCell = prop => {
        return (
            <DropDownListCell
                {...prop}
                editable={true}
                ddlData={paramDdlData}
                textField={'SET_VALUE'}
                dataItemKey={'SET_TYPE'}
                AlwaysEdit={true}
                onCellDDLChange={onCellInputBlur}
            />
        )
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
            CHK_POINT_CLASS_ID: props.data.CHECKPOINT_CLASS_ID,
            NAME: "",
            PROGRESS: 0,
            IS_ENABLE: true,
            CITY_GOV_ID: null,
            DEL_FLG: false,
            CTRL_POINT: "",
            editType: 1
        }
        editedGridData.current.push(newRecord);
        setGridData([...gridData, newRecord]);
        setIsDataChange(true);
        setNewDatas(true);
    }

    /**
     * 準備欄位驗證工具
     */
    const validateHelper = Yup.object().shape({
        NAME: Yup.string().required('此欄位為必填'),
        PROGRESS: Yup.number().integer("須為整數").required('此欄位為必填'),
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
        // 要判斷工作項目不能重複
        let ctrl = gridData.filter(x => !IsNullOrEmpty(x.CTRL_POINT)).map(x => x.CTRL_POINT);
        // 異動資料要判斷檢核點項目不能重複
        let name = editedGridData.current.map(x => x.NAME);
        // 原先資料
        let origin = new Set(gridData.filter(x => x.editType == 0).map(x => x.NAME));
        // 紀錄重複資料
        let repeatName = new Set();
        name.filter((x, index, arr) => {
            // 與原先資料重複
            if (origin.has(x)) {
                repeatName.add(x);
            }
            // 本身資料重複
            if (arr.indexOf(x) !== index) {
                repeatName.add(x);
            }
        });

        if (notValids.length > 0) {
            showGlobalMessageBox('儲存失敗，請確認資料填妥後重新嘗試!');
        }
        else if (repeatName.size != 0) {
            showGlobalMessageBox(`儲存失敗，檢核點項目 ${Array.from(repeatName).join("、")} 重複!`);
        }
        else if (ctrl.length != new Set(ctrl).size) {
            showGlobalMessageBox('儲存失敗，工作項目不可有重複選項');
        }
        else {
            SetMaskOnOff(true);
            let saveResult = await SetCodeService.saveCusItem(editedGridData.current);
            SetMaskOnOff(false);
            if (saveResult.success) {
                showGlobalMessageBox(saveResult.message, () => loadData());
            }
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
                    <Button className={"detailButton"} title="新增檢核點" onClick={addNew}>新增檢核點</Button>
                </>
            }
        >
            <Grid
                style={{
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
                <GridColumn field="DEL_FLG" headerClassName={'breakSpacehHeader'} title="是否停用"
                    cell={IsDelCell} width="80px" />
                <GridColumn field="NAME" headerClassName={'breakSpacehHeader'} title="檢核點項目"
                    cell={NameCell} headerCell={RequiredHeaderCell} />
                <GridColumn field="PROGRESS" headerClassName={'breakSpacehHeader'} title="進度%"
                    cell={ProgressCell} headerCell={RequiredHeaderCell} width="80px" />
                <GridColumn field="CTRL_POINT" headerClassName={'breakSpacehHeader'} title="工作項目"
                    cell={CtrlPointCell} />
            </Grid>
        </PageContainer>
    )
}
export default CodeCheckPointDetailGrid;
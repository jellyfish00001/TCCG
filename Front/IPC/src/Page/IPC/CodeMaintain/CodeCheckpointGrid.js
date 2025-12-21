import React from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import { handleEditedGridData, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { SetCodeService } from './SetCodeService';
import { GetSetParam } from '../../../Basic/CommonService';
import CodeCheckpointDetailGrid from './CodeCheckpointDetailGrid';
import { orderBy } from "@progress/kendo-data-query";
import * as Yup from 'yup';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';

export const CodeCheckPointGrid = () => {
    //執行方式資料
    const [Data, setData] = React.useState([]);
    //執行方式類別下拉選單資料
    const [cpKindDdlData, setCpKindDdlData] = React.useState([]);
    //控制點下拉選單資料
    const [ctrlPointDdlData, setCtrlPointDdlData] = React.useState([]);
    //紀錄下拉查詢條件
    const [queryCondition, setQueryCondition] = React.useState({ text: "", value: -1 });
    const [expanded, setexpanded] = React.useState(false);
    //紀錄異動資料
    const editedGridData = React.useRef([]);
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
     * 載入執行方式資料
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let data = await SetCodeService.loadCheckPoint(queryCondition.value);
        setData(data);
        setIsDataChange(false);
        setNewDatas(false);
        editedGridData.current = [];
        SetMaskOnOff(false);
    }

    /**
     * 載入下拉選單資料
     */
    const loadDropDownData = async () => {
        SetMaskOnOff(true);
        let arr = [
            GetSetParam('CP_KIND', '', true),// 取得執行方式類別
            GetSetParam('CTRL_CHK_POINT_TYPE', '', true),// 取得控制點
        ];
        let data = await SetCodeService.loadAllDropDowns(arr);
        if (data.length > 0) {
            setCpKindDdlData(data[0]);
            setQueryCondition(data[0].length > 0 ? { text: data[0][0].SET_VALUE, value: data[0][0].SET_TYPE } : { text: "", value: -1 });
            data[1].unshift({ SET_TYPE: "", SET_VALUE: '無' });
            setCtrlPointDdlData(data[1]);
        }
        SetMaskOnOff(false);
    }
    React.useEffect(() => {
        loadDropDownData();
    }, [])

    React.useEffect(() => {
        if (queryCondition.value != -1) {
            loadData();
        }
    }, [queryCondition])

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'CHECKPOINT_CLASS_ID');
        setIsDataChange(editedGridData.current.length > 0);
        setNewDatas(!newDatas);
    }

    /**
     * 供可切換欄位的input onChange callback的事件
     * @param {object} item
     */
    const onCellInputChange = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', 'CHECKPOINT_CLASS_ID');
    }

    /**
     * 展開欄位
     * @param {*} props 
     * @returns 
     */
    const _CommandCell1 = (props) => {
        const ExpandedButton = () => {
            //新增的話隱藏展開按鈕
            if (props.dataItem.editType === 1) {
                return <></>;
            }
            return <Button title={"展開明細"} icon={!props.dataItem.expanded ? 'plus' : 'minus'} look='default' onClick={(e) => {
                let newData = Data.map((item) => {
                    if (item.CHECKPOINT_CLASS_ID === props.dataItem.CHECKPOINT_CLASS_ID) {
                        item.expanded = !props.dataItem.expanded;
                    } else {
                        item.expanded = false;
                    }

                    return item;
                });
                setexpanded(!props.dataItem.expanded);
                setData(newData);
            }} />
        }
        return <CommandCell>
            {ExpandedButton()}
        </CommandCell>
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
     * 執行方式名稱
     * @param {*} props 
     * @returns 
     */
    const ChkClassCell = props => {
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
     * 新增
     */
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = Data.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            CHECKPOINT_CLASS: "",
            CP_KIND: queryCondition.value,
            CP_KIND_DESC: queryCondition.text,
            IS_BASIC: true,
            DEL_FLG: false,
            editType: 1
        }
        editedGridData.current.push(newRecord);
        setData([...Data, newRecord]);
        setIsDataChange(true);
        setNewDatas(true);
    }

    /**
     * 準備欄位驗證工具
     */
    const validateHelper = Yup.object().shape({
        CHECKPOINT_CLASS: Yup.string().required('此欄位為必填'),
    });

    /**
     * 存檔
     */
    const save = async () => {
        //驗證
        let notValids = [];
        for (let index = 0; index < Data.length; index++) {
            const item = Data[index];
            let isValid = await validateHelper.isValid(item);
            if (!isValid) {
                notValids.push(isValid);
            }
        }
        // 異動資料要判斷執行方式名稱不能重複
        let name = editedGridData.current.map(x => x.CHECKPOINT_CLASS);
        // 原先資料
        let origin = new Set(Data.filter(x => x.editType == 0).map(x => x.CHECKPOINT_CLASS));
        // 紀錄重複資料
        let repeat = new Set();
        name.filter((x, index, arr) => {
            // 與原先資料重複
            if (origin.has(x)) {
                repeat.add(x);
            }
            // 本身資料重複
            if (arr.indexOf(x) !== index) {
                repeat.add(x);
            }
        })
        if (notValids.length > 0) {
            showGlobalMessageBox('儲存失敗，請確認資料填妥後重新嘗試!');
        }
        else if (repeat.size != 0) {
            showGlobalMessageBox(`儲存失敗，執行方式名稱 ${Array.from(repeat).join("、")} 重複!`);
        }
        else {
            SetMaskOnOff(true);
            let saveResult = await SetCodeService.saveCheckPoint(editedGridData.current);
            SetMaskOnOff(false);
            if (saveResult.success) {
                showGlobalMessageBox(saveResult.message, () => loadData());
            }
        }
    }

    /**
     * 展開grid
     */
    const DetailComponent = (props) => {
        return (
            <CodeCheckpointDetailGrid
                data={props.dataItem}
                ctrlPointData={ctrlPointDdlData}
            />
        )
    }

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setData(orderBy(Data, event.sort));
        setSort(event.sort);
    };

    return (
        <PageContainer
            toolbar={
                <>
                    <Button title="存檔" onClick={save} >存檔</Button>
                    <Button title="取消" className="k-button-lighten" onClick={loadData} >取消</Button>
                    <Button title="新增執行方式" onClick={addNew} disabled={queryCondition.value == -1}>新增執行方式</Button>
                    <div style={{ display: "inline-flex" }}>
                        <h6 style={{ marginTop: '7px' }}><span style={{ color: "red" }}>*</span>執行方式類別：</h6>
                        <div style={{ marginLeft: "10px" }}>
                            <DropDownListWithValue
                                data={cpKindDdlData}
                                textField={"SET_VALUE"}
                                dataItemKey={"SET_TYPE"}
                                value={queryCondition.value}
                                onChange={(e) => {
                                    //更動選項就抓取符合的執行方式清單
                                    setQueryCondition({ text: e.target.text, value: e.target.value })
                                }}
                            />
                        </div>
                    </div>
                </>
            }
        >
            <Grid
                data={Data}
                resizable={true}
                detail={DetailComponent}
                expandField="expanded"
                sort={sort}
                onSortChange={sortChange}
                sortable={{
                    allowUnsort: true,
                    mode: "single",
                }}>
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn cell={_CommandCell1} width="50px" title="展開" />
                <GridColumn field="DEL_FLG" headerClassName={'breakSpacehHeader'} title="停用"
                    cell={IsDelCell} width="80px" />
                <GridColumn field="CP_KIND_DESC" headerClassName={'breakSpacehHeader'} title="執行方式類別"
                    width="200px" />
                <GridColumn field="CHECKPOINT_CLASS" headerClassName={'breakSpacehHeader'} title="執行方式名稱"
                    cell={ChkClassCell} headerCell={RequiredHeaderCell} />
            </Grid>
        </PageContainer>
    );
}
export default CodeCheckPointGrid
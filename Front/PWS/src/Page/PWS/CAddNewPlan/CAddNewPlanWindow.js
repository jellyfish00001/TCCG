import React, { useState, useRef, useEffect } from "react";
import { Button } from '@progress/kendo-react-buttons';
import WindowBox from '../../../Components/Dialogs/WindowBox';
import { Grid, GridColumn } from '@progress/kendo-react-grid';
import { DropDownListCell } from "../../../Components/GridCell/DropDownListCell";
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import { showGlobalConfirmBox } from "../../../Route/RootMiddleware";
import { handleEditedGridData } from '../../../Basic/SDOExtension';
import { handleDropDownKindChange } from "../AddNewPlan/AddNewPlanService";

const CAddNewPlanWindow = (props) => {
  const {
    closeWindow,    //關閉視窗
    yearOptions,    //年度下拉選單資料
    PLANNO,         //計畫編號
    gridChange,     //傳入grid資料
    setGridChange,  //設定grid資料
    editedGridData  //紀錄grid資料異動
  } = props;
  // 過濾掉今年的年度
  const YearOptions =yearOptions.filter(y => y.value !== (new Date().getFullYear() - 1911 + 1).toString())
  // 預設下拉選單     
  const Options = [{ text: "請選擇來源", value: "" }];
  // 預算來源下拉選單資料
  const [budgetSourceOptions, setBudgetSourceOptions] = useState([]);
  // grid的資料
  const [gridData, setGridData] = useState([]);
  // 計算資料編號
  const countNo = useRef(0);
  //初始資料
  const initialData = {
    NO: 1,
    IDENTITYFIELD: 0,
    PLANNO: PLANNO,
    PLANYEAR: (new Date().getFullYear() - 1911).toString(),
    SOURCEKIND: "",
    PLANITEMC: "",
    BUDGETCENTRAL: 0,
    BUDGETLOCAL: 0,
    dataArray: [...Options],
    editType: 1
  }

  /**
  * 初始化資料
  * @returns
  */
  const loadData = async () => {
    // 取得預算來源
    let budgetSources = await handleDropDownKindChange('4');
    setBudgetSourceOptions(budgetSources);

    // 取得gridChange中出現的所有不同的來源SOURCEKIND值
    let sourceKinds = new Set(gridChange.map(item => item.SOURCEKIND));

    // 針對每個不同的來源SOURCEKIND值取得對應的下拉清單數據
    let dropdownData = {};
    for (let kind of sourceKinds) {
      dropdownData[kind] = await handleDropDownKindChange(kind);
    }

    // 使用取得到的下拉清單資料更新gridData
    const data = gridChange.map((item) => {
      return { ...item, dataArray: dropdownData[item.SOURCEKIND] };
    });
    // 紀錄gridData中的最大NO值
    countNo.current = Math.max(...data.map(item => item.NO), 0);

    setGridData(data);
  };

  // 初始化取得資料
  useEffect(() => {
    loadData();
  }, []);

  /**
  * 新增資料行
  * @returns 
  */
  const addNew = () => {
    // 計算編號不重複
    countNo.current += 1;
    const maxId = countNo.current;
    const newDataItem = { ...initialData, NO: maxId + 1, hiddenIndex: maxId + 1 };
    const newGridData = [...gridData, newDataItem];
    setGridData(newGridData);
    // 追蹤編輯資料
    handleEditedGridData(editedGridData, newDataItem, 'hiddenIndex', 'IDENTITYFIELD');
  };

  /**
  * 移除資料行
  * @param {*} dataItem //點擊資料行
  * @returns 
  */
  const remove = (dataItem) => {
    let removedDataItem = dataItem;
    // 移除指定資料行
    const newGridData = gridData.filter(item => item.NO !== dataItem.NO);
    setGridData([...newGridData]);
    removedDataItem.editType = 3;
    handleEditedGridData(editedGridData, removedDataItem, 'hiddenIndex', 'IDENTITYFIELD');
  };

  /**
  * 數字輸入改變
  * @param {*} e //點擊的資料行
  * @returns 
  */
  const onCellInputChange = (e) => {
    handleEditedGridData(editedGridData, e, 'NO', 'IDENTITYFIELD');
  }

  /**
  * 存檔
  * @returns 
  */
  const save = async () => {
    //驗證資料行有無輸入
    const isHasData = gridData.some(item => item.SOURCEKIND == "" || item.PLANITEMC == "");
    const isHasMoney = gridData.some(item => (item.BUDGETCENTRAL + item.BUDGETLOCAL) === 0);
    if (isHasData) {
      showGlobalConfirmBox("請填選，預算來源和項目");
      return;
    }
    if (isHasMoney) {
      showGlobalConfirmBox("請填選中央補助款或地方自籌款");
      return;
    }
    setGridChange(gridData);
    closeWindow();
  }

  /**
  * 取消
  * @returns 
  */
  const clean = () => {
    setGridData([]);
  }

  /**
  * 回計畫資料
  * @returns 
  */
  const back = () => {
    closeWindow();
  }

  /**
  * 下拉選單改變時的處理函數
  * @param {*} e 
  * @param {*} field 
  */
  const onChange = async (e, field) => {
    // 如果是 SOURCEKIND 字段，需要處理額外的邏輯
    if (field === "SOURCEKIND") {
      let dropData = await handleDropDownKindChange(e.SOURCEKIND);
      // 將 dropData 取代當前資料的陣列欄位 dataArray
      const newDataArray = gridData.map(item => {
        if (item.NO === e.NO) {
          return { ...item, dataArray: dropData };
        }
        return item;
      });
      setGridData(newDataArray);
      // 追蹤編輯資料
      handleEditedGridData(editedGridData, e, 'NO', 'IDENTITYFIELD');
    }
    else {
      // 追蹤編輯資料
      handleEditedGridData(editedGridData, e, 'NO', 'IDENTITYFIELD');
    }
  };

  /**
  * 共用下拉選單
  * @param {*} props 
  * @param {*} field 
  * @param {*} data 
  * @returns 
  */
  const DropDownListCustomCell = (props, field, data) => {
    return (
      <DropDownListCell
        {...props}
        editable={true}
        ddlData={data}
        textField="text"
        dataItemKey="value"
        AlwaysEdit={true}
        onCellDDLChange={(e) => onChange(e, field)}
      />
    );
  };


  /**
  * 共用數字輸入框
  * @param {*} props
  * @param {*} field
  * @returns
  * 數字輸入框
  */
  const NumericTextBox = (props, field) => {
    return (
      <NumericTextInputCell
        {...props}
        format={'n0'}
        field={field}
        editable={true}
        onCellInputChange={onCellInputChange}
        AlwaysEdit={true}
        min={0}
      />
    );
  };

  /**
  * 刪除欄位
  * @param {*} props 
  * @returns 
  */
  const CommandCell = (props) => (
    <td>
      <Button icon="close" onClick={() => remove(props.dataItem)} />
    </td>
  );

  return (
    <WindowBox
      width={70}
      height={50}
      onClose={props.closeWindow}
      title={"跨年度經費設定"}
    >

      <Button type="button" title="存檔" style={{ marginRight: '5px' }} onClick={save}>存檔</Button>
      <Button type="button" title="取消" style={{ marginRight: '5px' }} onClick={clean}>取消</Button>
      <Button type="button" title="回計畫資料" style={{ marginRight: '5px' }} onClick={back}>回計畫資料</Button>
      <div className='fn-buttons'>
        <Button type="button" title="新增經費" style={{ marginTop: '10px' }} onClick={addNew}>新增經費</Button>
      </div>
      <Grid
        data={gridData}
      >
        <GridColumn field="DELETE" title="刪除" cell={CommandCell} width={50} />
        <GridColumn field="PLANYEAR" title="年度" cell={(p) => DropDownListCustomCell(p, "PLANYEAR", YearOptions)} />
        <GridColumn field="SOURCEKIND" title="預算來源" cell={(p) => DropDownListCustomCell(p, "SOURCEKIND", budgetSourceOptions)} headerCell={RequiredHeaderCell} />
        <GridColumn field="PLANITEMC" title="預算項目" cell={(p) => DropDownListCustomCell(p, "PLANITEMC", p.dataItem.dataArray)} headerCell={RequiredHeaderCell} />
        <GridColumn field="BUDGETCENTRAL" title="中央補助款(千元)" cell={(p) => NumericTextBox(p, "BUDGETCENTRAL")} />
        <GridColumn field="BUDGETLOCAL" title="地方自籌款(千元)" cell={(p) => NumericTextBox(p, "BUDGETLOCAL")} />
      </Grid>
    </WindowBox>
  );
};

export default CAddNewPlanWindow;
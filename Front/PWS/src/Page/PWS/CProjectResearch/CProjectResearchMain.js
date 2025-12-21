import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { TextInputCell } from "../../../Components/GridCell/TextInputCell";
import { AddNoColumn } from "../../../Basic/SDOExtension";
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import { DropDownListCell } from "../../../Components/GridCell/DropDownListCell";
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { saveThreeYearPlan } from "./CProjectResearchService";
import { getPlanYearList } from "../../../Basic/CommonService";
import { getThreeYearPlan } from "./CProjectResearchService";
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import { handleEditedGridData } from '../../../Basic/SDOExtension';
import { GetSetParam } from "../../../Basic/CommonService";

const CProjectResearchMain = (props) => { 
    // 外部傳入
    const { 
        location: { 
            state: {
                projectNo,
                projectIsSend,
            }
        }
     } = props;
    // 參採下拉
    const [situationOptions, setSituationOptions] = useState([]);

    //年度下拉選單
    const [yearOptions, setYearOptions] = useState([]);
    //grid資料
    const [gridData, setGridData] = useState([]);
    const gridRef = useRef(null);

    /**
     * 取得資料
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let Year = await getPlanYearList(false, 20, "A");
        setYearOptions(Year);
        let result = await getThreeYearPlan(projectNo);
        gridRef.current = result;
        setGridData(result);
        let situation = await GetSetParam('SITUATION_TYPE', '');
        setSituationOptions(situation);
        SetMaskOnOff(false);
    }

    useEffect(() => {
        // 取得下拉選單
        loadData();
    }, []);

    /**
     *刪除
    * @returns
    */
    const clean = () => {
        setGridData(gridRef.current);
    }

    /**
     * 存檔
     * @returns 
     */
    const save = async() => {
        // 檢查是否有空的欄位
        const isEmpty = gridData.some(row => {
            return !row.PLANNAME || !row.STUDYYEAR || !row.BUDGET || !row.SITUATIONTYPE || !row.SITUATIONDESC;
        });
        if (isEmpty) {
            showGlobalMessageBox("請填寫所有欄位");
            return;
        }
        // 如果資料填寫完整，繼續存檔流程
        SetMaskOnOff(true);
        const ThreeYearPlanList = gridData.map(row => ({ 
            ...row, 
            STUDYYEAR: parseInt(row.STUDYYEAR),
            THREEYEARID: row.NO,
            PLANNO: projectNo
        }));
        //將計畫ID一起傳入
        const dataTosave = {
            PLANNO: projectNo,
            ThreeYearPlanList
        }
        let saveResult = await saveThreeYearPlan(dataTosave);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => window.location.reload());
        }
    }
    
    /**
    * 新增Grid資料行
    * @returns
    */
    const addNew = () => {
    // 取得最大序號
    const maxNO = Math.max(...gridData.map(item => item.NO), 0);
        const newDataItem = {
            NO: maxNO + 1,
            PLANNAME: "",
            STUDYYEAR: new Date().getFullYear() - 1911,
            BUDGET: 0,
            SITUATIONTYPE: ""
        };
    setGridData([...gridData, newDataItem]);
    };

    /**
    * 移除Grid資料行
    * @returns
    */
    const remove = (dataItemToRemove) => {
        const filteredData = gridData.filter(item => item !== dataItemToRemove);
        //重新計算序號
        const newGridData = filteredData.map((item, index) => ({ ...item, NO: index + 1 }));
        setGridData(newGridData);

    };

    /**
    * 刪除欄位
    * @param {*} props
    * @returns
    */
    const DelCommandCell = (props) => {
        return (
            <CommandCell>
                <Button title={"刪除"} icon='close' look='default' onClick={() => { remove(props.dataItem); }} />
            </CommandCell>
        );
    };

    /**
    * 下拉選單改變
    * @param {*} event
    * @returns
    */
    const handleInputChange = (event) => {

    };
    /**
    * 輸入框失焦改變
    * @param {*} event
    * @returns
    */
    const onCellInputBlur = (event) => {

    };
    /**
    * 輸入框改變
    * @param {*} event
    * @returns
    */
    const onCellDDLChange = (event) => {
        
    };

    // 下拉選單輸入
    const renderDropDownListCell = (props, field, options) => (

        <td>
        {field === 'STUDYYEAR' &&(
            <DropDownListCell
                {...props}
                editable={true}
                field={field}
                ddlData={options}
                textField={'text'}
                dataItemKey={'value'}
                AlwaysEdit={true}
                onCellDDLChange={handleInputChange}
            />
        )}
        {field === 'SITUATIONTYPE' && (
            <DropDownListCell
            {...props}
            editable={true}
            field={field}
            ddlData={options}
            textField={'SET_VALUE'}
            dataItemKey={'SET_TYPE'}
            AlwaysEdit={true}
            onCellDDLChange={handleInputChange}
        />
        )}
        {field === 'SITUATIONTYPE' && (
            <TextInputCell
                {...props}
                field={'SITUATIONDESC'}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellDDLChange = {onCellDDLChange}
                AlwaysEdit={true}
            />
        )}
        </td>
    );

    //輸入框
    const textInputCell = (props) => {
        return (
            <TextInputCell
                {...props}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellDDLChange = {onCellDDLChange}
                AlwaysEdit={true}
            />
        );
    };

    //數字輸入框
    const numerCell = (props) => {
    return (
        <NumericTextInputCell
            {...props}
            editable={true}
            onCellInputBlur={onCellInputBlur}
            AlwaysEdit={true}
            min={0}
        />
    );
    };

    return (
        <>
            <PageContainer style={{ overflow: "auto", height: "100%" }}>
                <CollapseBoardCard
                    button={
                                <>
                                    {!projectIsSend &&
                                        <>
                                            <Button title="存檔" onClick={save} >存檔</Button>
                                            <Button title="取消" className="k-button-lighten" onClick={clean} >取消</Button>
                                        </>
                                    }
                                </>
                            }
                title="近三年相關研究"
                isFirstArea={true}
                > 
                <Button type='button' title="新增" onClick={addNew} >新增</Button>
                <Grid
                    style={{ overflow: 'auto', height: '100%'}}
                    resizable={true}
                    data={gridData}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn field="Edit" title="編修" cell={DelCommandCell} width="100px" />
                    <GridColumn field="NO" title="序號" width="50px" />
                    <GridColumn field="PLANNAME" title="計畫名稱" cell={textInputCell} headerCell={RequiredHeaderCell}/>
                    <GridColumn
                        field="STUDYYEAR"
                        title="研究年度"
                        cell={(p) => renderDropDownListCell(p, 'STUDYYEAR', yearOptions)}
                        headerCell={RequiredHeaderCell}
                        width="100px"
                    />
                    <GridColumn field="BUDGET" title="研究經費(千元)" cell={numerCell} headerCell={RequiredHeaderCell}/>
                    <GridColumn
                        field="SITUATIONTYPE"
                        title="參採情形"
                        cell={(p) => renderDropDownListCell(p, 'SITUATIONTYPE', situationOptions)}
                        headerCell={RequiredHeaderCell}
                    />
                </Grid>
                </CollapseBoardCard>
            </PageContainer>
        </>
    );
}
export default CProjectResearchMain;
import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import { RadioGroup } from '@progress/kendo-react-inputs';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { handleEditedGridData, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { getInnProjedtTitle, saveInnProjectTitle } from './InnProjectTitleService';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { getInnYear } from "../../../Basic/CommonService";
import * as Yup from 'yup';


const InnProjectTtitleMain = () => {
    //表單資料
    const [gridData, setGridData] = useState();
    const [IsPlural, setIsPlural] = useState();
    const [Year, setYear] = useState('');
    const [IsEdit, setIsEdit] = useState(false);
    //紀錄異動資料
    const editedGridData = useRef([]);
    // 下拉選單資料
    const [ddlData, setDdlData] = useState({ INN_YEAR: [] });

    /**
    * 取得資料
    * @param {*} year 
    */
    const getData = async (year) => {
        let result = await getInnProjedtTitle(year)
        setIsPlural(result.IS_PLURAL);
        setGridData(result.InnProjectTitles);
        if (result.IS_EDIT > 0) {
            setIsEdit(true);
        } else {
            setIsEdit(false);
        }
    }


    /**
     * 下拉選單的 onChange 事件處理函數
     * @param {*} e 
     */
    const handleYearChange = async (e) => {
        SetMaskOnOff(true);
        setYear(e.value.value);
        SetMaskOnOff(true);
        await getData(e.value.value)
        SetMaskOnOff(false);
    };

    /**
     * 載入畫面
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await getInnYear()
        setDdlData({
            INN_YEAR: [...result],
        })
        SetMaskOnOff(false);
    }

    /**
     * 驗證欄位
     */
    const validateField = Yup.object().shape({
        InnProjectTitles: Yup.array().of(
            Yup.object().shape({
                CODE: Yup.string().required(),
                CODE_VALUE: Yup.string().required()
            })
        )
    });

    /**
     * 存檔
     */
    const saveChanges = async () => {
        let requestObj = {
            IS_PLURAL: IsPlural,
            YEAR: Year,
            InnProjectTitles: gridData
        }

        let isValid = await validateField.isValid(requestObj);

        SetMaskOnOff(true);
        if (isValid) {
            let result = await saveInnProjectTitle(requestObj)
            if (result.success) {
                showGlobalMessageBox(result.message, () => getData(Year));
            }
        } else {
            showGlobalMessageBox("請確認資料是否填寫完成")
        }

        SetMaskOnOff(false);
    }

    useEffect(() => {
        loadData();
    }, [])


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
        }
        setGridData([...gridData, newRecord]);
    }


    /**
    * 刪除按鈕
    * @param {*} prop Grid傳入的參數
    * @returns CommandCell
    */
    const _CommandCell = (prop) => {
        return (
            <CommandCell>
                <Button
                    title={"刪除"}
                    icon={'close'}
                    look='default'
                    onClick={() => {
                        remove(prop.dataItem)
                    }} />

            </CommandCell>
        )
    }

    /**
     * 移除
     * @param {*} dataItem 
     */
    const remove = (dataItem) => {
        //判定刪除的是否為新增的資料
        let index = dataItem.hiddenIndex ?
            gridData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex)
            :
            gridData.findIndex(record => record.ID === dataItem.ID);

        gridData.splice(index, 1);
        dataItem.editType = 3
        handleEditedGridData(editedGridData, dataItem, "hiddenIndex", "ID");
        setGridData([...gridData]);
    }

    /**
     * 文字輸入框cell
     * @param {*} props 
     * @returns 
     */
    const textInputCell = props => {
        return (
            <TextInputCell
                {...props}
                required={true}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                AlwaysEdit={true}
            />
        );

    }

    /**
    * 供可切換欄位的input onBlur callback的事件
    * @param {object} item
    */
    const onCellInputBlur = async () => {
        setGridData([...gridData])
    }


    return (
        <PageContainer
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">維護專題</h3>
                </>
            }
        >
            <table style={{ width: '30%', borderCollapse: 'collapse' }}>
                <tr>
                    <th>
                        年度：
                    </th>
                    <td>
                        <DropDownListWithValue
                            data={ddlData.INN_YEAR}
                            textField={"text"}
                            dataItemKey={"value"}
                            value={Year}
                            onChange={
                                handleYearChange
                            }
                        />
                    </td>
                </tr>
            </table>

            <div className="fn-buttons">
                <Button title="新增" type="button" onClick={addNew}>新增</Button>
                <Button title="存檔" type="button" onClick={saveChanges} disabled={IsEdit}>存檔</Button>
            </div>

            <form>
                <table style={{ width: '30%', borderCollapse: 'collapse' }}>
                    <tr>
                        <th>
                            是否開放涉及其他提案類別
                        </th>
                        <td>
                            <RadioGroup
                                name="IS_PLURAL"
                                value={IsNullOrEmpty(IsPlural) ? "" : IsPlural}
                                onChange={(e) => {
                                    setIsPlural(e.value)
                                }}
                                layout={"horizontal"}
                                data={[
                                    { label: "是", value: true },
                                    { label: "否", value: false },
                                ]}
                            />
                        </td>
                    </tr>
                </table>
            </form>

            <Grid
                data={gridData}
                style={{
                    width: '50%'
                }}>

                <GridNoRecords>無資料</GridNoRecords>
                {/* 傳入之功能Grid欄位 */}
                <GridColumn cell={_CommandCell} width="70px" />
                <GridColumn field="CODE" cell={textInputCell} title="專題代號" />
                <GridColumn field="CODE_VALUE" cell={textInputCell} title="專題名稱" />
            </Grid>
        </PageContainer>
    )
}
export default InnProjectTtitleMain;
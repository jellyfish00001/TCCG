import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import CheckBoxList from '../../../Components/Input/CheckBoxList';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { handleEditedGridData, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { getInnYear } from "../../../Basic/CommonService";
import { getProjectCusField, savedata } from "./InnProjectCusFieldService"
import { showGlobalConfirmBox, showGlobalMessageBox } from "../../../Route/RootMiddleware";
import * as Yup from 'yup';

const InnProjectCusFieldMain = () => {
    // Grid資料
    const [gridData, setGridData] = useState([]);
    //年度
    const [Year, setYear] = useState('');

    //欄位數量
    const numberOfRows = 5;

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState({ INN_YEAR: [] });

    //是否可以編輯
    const [IsEdit, setIsEdit] = useState(false);

    /**
     * 取得資料
     * @param {*} year 
     */
    const getData = async (year) => {
        let result = await getProjectCusField(year);
        setGridData(result.InnProjectCusFields);
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
        // 更新年度
        SetMaskOnOff(true);
        setYear(e.value.value)
        await getData(e.value.value)
        SetMaskOnOff(false);
    };

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
        let index = gridData.findIndex(record => record.CUS_FIELD_ID === dataItem.CUS_FIELD_ID);

        gridData.splice(index, 1);

        // 重新排序項次
        gridData.forEach((item, index) => {
            item.CUS_ITEM = index + 1;
        });

        setGridData([...gridData]);
    }

    /**
     * 選擇是否使用
     * @param {*} props 
     * @returns 
     */
    const checkBoxCell = props => {
        return (
            <td style={{ 'textAlign': 'center' }}>
                <CheckBoxList
                    group='IS_USE'
                    valueField='id'
                    data={[
                        { id: props.dataItem.IS_USE }
                    ]}
                    onChange={(e) => {
                        props.dataItem.IS_USE = e.value
                        setGridData([...gridData])
                    }}
                />
            </td>
        )
    }

    /**
     * 新增
     */
    const addNew = () => {
        if (gridData.length >= numberOfRows) {
            return;
        }
        //重0開始
        const NO = Math.max(...gridData.map(item => item.CUS_ITEM), 0);
        //新增時寫入預設值
        const newRecord = {
            CUS_ITEM: NO + 1,
            YEAR: Year,
        }
        setGridData([...gridData, newRecord]);
    }


    /**
     * 文字輸入框Cell
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
     */
    const onCellInputBlur = async () => {
        setGridData([...gridData])
    }


    /**
     * 驗證欄位
     */
    const validateField = Yup.object().shape({
        InnProjectCusFields: Yup.array().of(
            Yup.object().shape({
                IS_USE: Yup.boolean().required(),
                CUS_FIELD_NANE: Yup.string().required()
            })
        )
    });


    /**
     * 存檔
     * @param {*} data 
     */
    const saveChanges = async () => {
        let requestData = {
            YEAR: Year,
            InnProjectCusFields: gridData
        }
        let isValid = await validateField.isValid(requestData)

        SetMaskOnOff(true);
        if (isValid) {
            let result = await savedata(requestData);
            if (result.success) {
                showGlobalMessageBox(result.message, () => getData(Year));
            }
        } else {
            showGlobalMessageBox("請確認資料是否填寫完成")
        }

        SetMaskOnOff(false);
    }

    /**
     * 載入畫面
     */
    const loadData = async () => {
        SetMaskOnOff(false);
        let result = await getInnYear()
        setDdlData({
            INN_YEAR: [...result],
        })
        SetMaskOnOff(false);
    }

    useEffect(() => {
        loadData(gridData);
    }, [])

    return (
        <PageContainer
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">維護自訂義欄位名稱</h3>
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
                            onChange={handleYearChange}
                        />
                    </td>

                </tr>
            </table>

            <div className="fn-buttons">
                <Button title="新增" type="button" onClick={addNew}>新增</Button>
                <Button title="存檔" type="button" onClick={saveChanges} disabled={IsEdit}>存檔</Button>
            </div>


            <Grid
                data={gridData}
                style={{
                    width: '30%'
                }}>

                <GridNoRecords>無資料</GridNoRecords>
                {/* 傳入之功能Grid欄位 */}
                <GridColumn cell={_CommandCell} width="70px" />
                <GridColumn cell={checkBoxCell} field="IS_USE" title="是否使用" width="80px" />
                <GridColumn title="項次" field="CUS_ITEM" cell={(props) =>
                    <td style={{ textAlign: "center" }}>{props.dataIndex + 1}</td>} width="50px" />
                <GridColumn field="CUS_FIELD_NANE" cell={textInputCell} title="欄位名稱" />
            </Grid>

        </PageContainer>
    )
}

export default InnProjectCusFieldMain;
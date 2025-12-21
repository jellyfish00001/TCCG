import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import CheckBoxList from '../../../Components/Input/CheckBoxList';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { handleEditedGridData, IsNullOrEmpty } from '../../../Basic/SDOExtension';

const InnProjectCusFieldMain = (props) => {
    // 這裡假設grid資料從外部加載
    const { data } = props;

    // Grid資料
    const [gridData, setGridData] = useState([]);

    // 已選取之計畫
    const checkedPlan = useRef([]);

    //表單儲存資料
    const [savedData, setSavedData] = useState({});

    // 紀錄異動資料
    const editedGridData = React.useRef([]);

    const checkBoxCell = props => {
        return (
            <td style={{ 'textAlign': 'center' }}>
                <CheckBoxList
                    group='CheckBoxList'
                    valueField='id'
                    data={[
                        { id: props.dataItem.PROJECT_NO, },
                    ]}
                    onChange={(e) => {
                        SetChecked(e);
                    }}
                />
            </td>
        )
    }

    //將勾選的資料放進checkedPlan
    const SetChecked = (e) => {
        if (e.value === true)
            checkedPlan.current.push(e.dataItem.id);
        else {
            checkedPlan.current.map((list, index) => {
                if (list === e.dataItem.id) {
                    checkedPlan.current.splice(index, 1);
                }
            });
        }
    }

    // 文字輸入框Cell
    const textInputCell = props => {
        return (
            <TextInputCell
                {...props}
                required={true}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInputChange={onCellInputBlur}
                AlwaysEdit={true}
            />
        );

    }

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = async (item) => {
        handleEditedGridData(editedGridData, item);
    }

    // 存檔
    const saveChanges = async (data) => {
        console.log(data);
    }

    const loadData = async () => {
        //欄位數量
        const numberOfRows = 5;
        const fakeData = Array.from({ length:numberOfRows  }, (_, index) => ({
            ITEM_NUM: index + 1,
            FIELD_Name: 'test',

        }));
        setGridData(fakeData);
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
            <table style={{ width: '20%', borderCollapse: 'collapse' }}>
                <tr>
                    <th>
                        年度：
                    </th>
                    <td>
                        <DropDownListWithValue
                        // data={ddlData.PROJECT_YEAR}
                        // textField={"text"}
                        // dataItemKey={"value"}
                        // value={values.PROJECT_YEAR}
                        // onChange={(e) => { setQueryData({ ...queryData, PROJECT_YEAR: e.value.value }) }}
                        />
                    </td>
                    <td>
                        <Button title="存檔" type="button">存檔</Button>
                    </td>
                </tr>
            </table>
            <div style={{ margin: '10px 0' }}></div>
            <Grid
                data={gridData}
                style={{
                    width: '30%'
                }}>
                <GridNoRecords>無資料</GridNoRecords>
                {/* 傳入之功能Grid欄位 */}
                <GridColumn cell={checkBoxCell} field="USE_STATUS" title="是否使用" width="80px" />
                <GridColumn field="ITEM_NUM" title="項次" width="50px" />
                <GridColumn field="FIELD_Name" cell={textInputCell} title="欄位名稱" />
            </Grid>

        </PageContainer>
    )
}
export default InnProjectCusFieldMain;
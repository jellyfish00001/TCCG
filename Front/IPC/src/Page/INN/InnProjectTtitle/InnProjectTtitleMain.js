import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import CheckBoxList from '../../../Components/Input/CheckBoxList';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { handleEditedGridData, IsNullOrEmpty } from '../../../Basic/SDOExtension';


const InnProjectTtitleMain= (props) => {
    // 這裡假設grid資料從外部加載
    const { data } = props;

    //表單異動資料
    const formRef = useRef();
    //表單資烙
    const [gridData, setGridData] = useState();
    //表單儲存資料
    const [savedData, setSavedData] = useState({});
    // 紀錄異動資料
    const editedGridData = React.useRef([]);

    // 存檔
    const saveChanges = async (data) => {
        console.log(data);
    }

    const loadData = async () => {
        const fakeData = Array.from({ length: 3 }, (_, index) => ({
            PLAN_NO: index + 1,
            PLAN_NAME: 'test',

        }));
        setGridData(fakeData);
    }

    useEffect(() => {
        loadData(gridData);
    }, [])


    /**
    * 定義指令欄 -審核按鈕
    * @param {*} props Grid傳入的參數
    * @returns CommandCell
    */
    const _CommandCell = (props) => {
        return (
            <CommandCell>
                <Button
                    title={"刪除"}
                    icon={'close'}
                    look='default'
                    onClick={() => {

                    }} />

            </CommandCell>
        )
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





    return (
        <PageContainer
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">維護專題</h3>
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

            <div className="fn-buttons">
                <Button title="新增" type="button">新增</Button>
            </div>
            <form>
                <table style={{ width: '30%', borderCollapse: 'collapse' }}>
                    <tr>
                        <th>
                            是否開放涉及其他提案類別
                        </th>
                        <td>
                            <CheckBoxList
                                group='NumberOfPeople'
                                valueField='value'
                                textField='text'
                                data={[

                                    {
                                        text: '是'
                                    },
                                    {
                                        text: '否'
                                    }
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
                <GridColumn field="PLAN_NO" cell={textInputCell} title="專題代號" />
                <GridColumn field="PLAN_NAME" cell={textInputCell} title="專題名稱" />
            </Grid>
        </PageContainer>
    )
}
export default InnProjectTtitleMain;
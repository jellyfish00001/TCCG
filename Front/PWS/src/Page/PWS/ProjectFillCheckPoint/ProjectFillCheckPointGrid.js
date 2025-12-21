import React, { useState, useEffect } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';
import TwDatePicker from '../../../Components/DateInputs/TwDatePicker';
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import { Tooltip } from '@progress/kendo-react-tooltip';
import { FormatDate, IsNullOrEmpty } from '../../../Basic/SDOExtension';

export const ProjectFillCheckPointGrid = ({ data, gridChange, type, checkpoint }) => {

    const [gridData, setGridData] = useState([]);
    // 紀錄是否有新增資料
    const [newDatas, setNewDatas] = React.useState(false);

    // 文字輸入框Cell
    const textInputCell = (props) => {
        // 若資料來源為代碼維護設定則不可編輯
        if (props.dataItem.CHECKITEM_SEQ || props.dataItem.fromSettings) {
            return (
                <td>{props.dataItem.CHECKITEM_NAME}</td>
            )
        } else {
            return (
                <TextInputCell
                    {...props}
                    maxlength={'50'}
                    editable={true}
                    onCellInputBlur={onCellInputBlur}
                    AlwaysEdit={true}
                />
            )
        }
    }

    // 數字輸入框Cell
    const numericInputCell = (props) => {
        // 若資料來源為代碼維護設定則不可編輯
        if (props.dataItem.CHECKITEM_SEQ || props.dataItem.fromSettings) {
            return (
                <td style={{ 'textAlign': 'right' }}>{props.dataItem.PROGRESS}</td>
            )
        } else {
            return (
                <NumericTextInputCell
                    {...props}
                    editable={true}
                    Numberformat={'n2'}
                    min={0}
                    max={100}
                    onCellInputBlur={onCellInputBlur}
                    AlwaysEdit={true}
                />
            )
        }

    }

    // 原預計完成日期 Cell
    const oriEstimatedEndDateCell = (props) => {
        return (
            <td>
                {FormatDate(props.dataItem.ORI_ESTIMATED_ENDDATE)}
            </td>
        )
    }

    // 預定完成日期 Cell
    const estimatedEndDateCell = (props) => {
        if (type === "adjust" && props.dataItem.ACTUAL_ENDDATE != null) {
            return (
                <td>
                    <Tooltip
                        openDelay={10}
                        position="right"
                        anchorElement="target"
                    >
                        <span title={"實際完成日：" + FormatDate(props.dataItem.ACTUAL_ENDDATE)}>
                            {FormatDate(props.dataItem.ESTIMATED_ENDDATE)}
                        </span>
                    </Tooltip>
                </td>
            )
        }
        else {
            return (
                <td style={{ 'textAlign': 'center' }}>
                    <TwDatePicker
                        name="ESTIMATED_ENDDATE"
                        format={"yyy/MM/dd"}
                        onChange={(e) => {
                            onCellInputChange(props, e.value)
                        }}
                        value={props.dataItem.ESTIMATED_ENDDATE}
                    />
                </td>
            )
        }
    }

    // 供可切換欄位的input change callback的事件
    const onCellInputChange = (props, value) => {
        let item = props.dataItem;
        item[props.field] = value;
        item.editType = item.editType > 0 ? item.editType : 2;
        let index = 0;

        // 自訂檢核點
        if (item.SEQ === 0 && item.CHECKITEM_SEQ === 0) {
            index = gridData.findIndex(d => d.hiddenIndex === item.hiddenIndex);
        } else if (item.SEQ === 0) { // 新增檢核點
            index = gridData.findIndex(d => d.CHECKITEM_SEQ === item.CHECKITEM_SEQ);
        } else {// 修改檢核點
            index = gridData.findIndex(d => d.SEQ === item.SEQ);
        }

        gridData.splice(index, 1, item);

        setGridData([...gridData])
        gridChange([...gridData])
    }

    // 供可切換欄位的input onBlur callback的事件
    const onCellInputBlur = async (item) => {
        let index = item.hiddenIndex
            ? gridData.findIndex(d => d.hiddenIndex === item.hiddenIndex)
            : gridData.findIndex(d => d.SEQ === item.SEQ);
        gridData.splice(index, 1, item);
        gridChange(gridData)
    }

    // 刪除
    const deleteCell = (props) => {
        // 若資料來源為代碼維護設定則不得刪除
        if (props.dataItem.CHECKITEM_SEQ > 0 || props.dataItem.fromSettings) {
            return (
                <td></td>
            )
        } else {
            return (
                <td style={{ 'textAlign': 'center' }}>
                    <Button title={"刪除"} icon='close' look='default' type="button" onClick={() => {
                        remove(props.dataItem);
                    }} />
                </td>
            )
        }
    }

    // 移除
    const remove = (dataItem) => {
        dataItem.editType = 3;
        let index = dataItem.hiddenIndex
            ? gridData.findIndex(d => d.hiddenIndex === dataItem.hiddenIndex)
            : gridData.findIndex(d => d.SEQ === dataItem.SEQ);
        gridData.splice(index, 1, dataItem);
        setGridData([...gridData]);
        gridChange(gridData);
    }

    // 新增
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            SEQ: 0,
            CHECKITEM_SEQ: 0,
            CHECKITEM_NAME: "",
            PROGRESS: 0,
            ESTIMATED_ENDDATE: new Date(),
            IS_DELAY: 0,
            diffMonth: 0,
            CTRL_POINT: "",
            IS_NEW: true,
            fromSettings: false,
            editType: 1
        };
        let newGridData = [...gridData, newRecord];
        setGridData(newGridData);
        setNewDatas(true);
        gridChange(newGridData);
    }

    // 設定外部傳入Grid查詢結果
    useEffect(() => {
        setGridData([...data]);
        gridChange([...data]);
    }, [data])

    return (
        <>
            <div className='fn-buttons'>
                <Button type='button' title="新增自訂檢核點" onClick={() => addNew()} style={{ display: IsNullOrEmpty(checkpoint) ? "none" : "" }} >新增自訂檢核點</Button>
            </div>
            <Grid
                style={{
                    textAlign: "center",
                    height: '100%',
                    overflow: 'auto',
                }}
                data={gridData.filter(x => x.editType !== 3)}
                resizable={true}
            >

                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="SEQ" width="0px" />
                <GridColumn
                    cell={deleteCell}
                    width={"40px"}
                    title="刪除"
                />
                <GridColumn field="CHECKITEM_NAME"
                    headerCell={RequiredHeaderCell}
                    title="檢核點"
                    cell={textInputCell} />
                <GridColumn field="PROGRESS"
                    headerCell={RequiredHeaderCell}
                    title="管考進度%"
                    cell={numericInputCell}
                    width="120px"
                />
                {
                    type === "adjust" &&
                    <GridColumn field="ORI_ESTIMATED_ENDDATE" title="原預計完成日期" cell={oriEstimatedEndDateCell} width="130px" />
                }
                <GridColumn field="ESTIMATED_ENDDATE"
                    headerCell={props => {
                        if (type === "adjust") {
                            return (
                                <CommonTooltip title={props.title} content="若已填寫實際完成日期，無法更改預定完成日期。" position="top" />
                            )
                        }
                        else {
                            return RequiredHeaderCell(props);
                        }
                    }}
                    title="預定完成日期"
                    cell={estimatedEndDateCell}
                />
                <GridColumn
                    width="100px"
                    field='diffMonth'
                    title="所需時間(月)"
                    cell={(props) => <td style={{ textAlign: "right" }}>{props.dataItem.diffMonth}</td>}
                />
            </Grid>
        </>
    );
}
export default React.memo(ProjectFillCheckPointGrid)
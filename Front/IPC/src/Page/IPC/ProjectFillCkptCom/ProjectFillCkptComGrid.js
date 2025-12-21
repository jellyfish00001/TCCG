import React, { useState, useEffect, useRef } from 'react';
import Table from '../../../Css/custom/Table.module.css';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { PageContainer } from '../../../Basic/PageContainer';
import { FormatDate, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import InterfacingSysConfirm from './Window/InterfacingSysConfirm';
import TwDatePicker from '../../../Components/DateInputs/TwDatePicker';
import { saveProjectUsePCC, clearCycleData } from './ProjectFillCkptComService';
import AssociatePCCWindow from './AssociatePCC/AssociatePCCWindow';

export const ProjectFillCkptComGrid = ({
    PROJECT_NO,                      // 計畫編號
    data,                            // Grid 資料
    gridChange,                      // Grid 異動CallBack
    startWork,                       // 是否辦理開工CallBack
    isAssociate,                     // 是否關聯工程會標案
    isUserFtyData,                   // 是否介接國發會資料
    reloadData,                      // 重載頁面資料
    isEngineering,                   // 執行方式是否為工程類
    isStartWork,                     // 是否辦理開工
    isTycgProject,                   // 是否為本府執行案件
    clearStartWorkDate,              // 清除開工實際完成日期
    isInterfacingSysConfirmVisible,  // 控制確認介接工程會標案管理系統視窗 visibility
    setInterfacingSysConfirmVisible, // 控制確認介接工程會標案管理系統視窗 visibility Set
    canSave,                         // 是否可存檔
    showSomeBtn,
    isEndWork,                       // 是否辦理驗收
    isRdecFun                        // 是否為管考
}) => {

    const [gridData, setGridData] = useState([]);
    const editedGridData = useRef([]);

    // #region 關聯工程會相關變數宣告
    const [isAssociatePCCWindowVisible, setIsAssociatePCCWindowVisible] = useState(false);

    const [startWorkDate, setStartWorkDate] = useState("");
    // #endregion

    // 工程會相關控制點代碼
    // A: 辦理開工 B: 辦理竣工 C: 辦理驗收
    const pccCtrlPoint = ['A', 'B', 'C'];

    /**
     * 日期欄位
     * @param {*} props Grid傳入的參數
     * @returns TwDatePickerCell
     */
    const editTwDatePickerCell = props => {
        const { field, dataItem } = props;
        let pccDateJsx = null;

        // 若關聯工程會，辦理開工、竣工、驗收 檢核點需顯示工程會資料
        if (isAssociate && (pccCtrlPoint.indexOf(dataItem.CTRL_POINT) != -1)) {
            pccDateJsx = <span>(標案管理系統： {dataItem[`PCC_${field}`]})</span>
        }
        return (
            <td>
                <TwDatePicker
                    name={`ACTUAL_ENDDATE_${dataItem["CTRL_POINT"]}`}
                    format={"yyy/MM/dd"}
                    onChange={(e) => {
                        if (e.value >= Date.now()) {
                            showGlobalMessageBox('不得選擇未來日期');
                        } else {
                            onCellInputChange(props, e.value);

                            if (e.target.name === "ACTUAL_ENDDATE_A") {
                                if (e.value === null) {
                                    clearStartWorkDate();
                                }
                                else {
                                    setStartWorkDate(FormatDate(e.value));
                                }
                            }

                            if (e.target.name === "ACTUAL_ENDDATE_B") {
                                if (!IsNullOrEmpty(dataItem.ACTUAL_ENDDATE_OLD) && e.value === null) {
                                    showGlobalMessageBox("請重新確認施工進度");
                                }
                            }

                        }
                    }}
                    value={dataItem.ACTUAL_ENDDATE}
                    width={'90%'}
                    disabled={dataItem.disabled}
                />
                {pccDateJsx}
            </td>
        )
    }

    /**
     * 關聯工程會資料格式
     * @param {*} props 
     */
    const PCCFormateCell = props => {
        const { field, dataItem } = props
        // 若關聯工程會，辦理開工、竣工、驗收 檢核點需顯示工程會資料
        if (isAssociate && (pccCtrlPoint.indexOf(dataItem.CTRL_POINT) != -1)) {
            return (
                <td>{dataItem[field]}<br />
                    (標案管理系統： {dataItem[`PCC_${field}`]})
                </td>
            )
        } else {
            return (
                <td>{dataItem[field]}</td>
            )
        }
    }

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} props
     * @param {Date} value
     */
    const onCellInputChange = async (props, value) => {
        let item = props.dataItem;

        // 更新Grid資料
        item[props.field] = value;
        let index = gridData.findIndex(d => d.SEQ === item.SEQ);
        gridData.splice(index, 1, item);

        // 異動的檢核點SEQ，
        let mdfSEQs = [item.SEQ];
        // 若控制點為辦理開工 
        if (item.CTRL_POINT == 'A') {
            // 呼叫CallBack外層component解鎖工程相關填報功能 
            startWork(value != null);

            // 若清空開工檢核點實際完成日期，則清空開工檢核點之後的檢核點實際完成日期
            if (value == null) {
                gridData.map(x => {
                    if (x.PROGRESS >= item.PROGRESS) {
                        x.ACTUAL_ENDDATE = null;
                        mdfSEQs.push(x.SEQ)
                    }
                    return x;
                });
            }
        }

        // 更新 Grid異動清單
        mdfSEQs.forEach(SEQ => {
            const targetItem = gridData.find(x => x.SEQ == SEQ)
            let editedIndex = editedGridData.current.findIndex(x => x.SEQ == SEQ);
            if (editedIndex == -1) {
                editedGridData.current.push(targetItem);
            } else {
                editedGridData.current.splice(editedIndex, 1, targetItem);
            }
        })

        setGridData([...gridData])
        gridChange([...editedGridData.current])
    }

    // #region 介接/取消介接 工程會標案管理系統相關
    // 介接/取消介接 工程會標案系統
    const setUseFtyDataOrNot = (isUserFtyData) => {
        // 介接，開啟確認視窗
        if (isUserFtyData) {
            setInterfacingSysConfirmVisible(true);
        }
        // 取消介接
        else {
            saveUseFtyDataOrNot(isUserFtyData);
        }
    }

    // 儲存是否介接工程會標案管理系統
    const saveUseFtyDataOrNot = (isUserFtyData) => {
        if (isUserFtyData) {
            saveEvent(isUserFtyData)
        } else {
            showGlobalConfirmBox('確定要取消介接工程會標案管理系統？\n（取消後，請依限完成每月辦理情形填報送出。）', () => {
                saveEvent(isUserFtyData)
            })
        }
    }

    // 存檔完畢事件
    const saveEvent = async (isUserFtyData) => {
        if (isUserFtyData) {
            showGlobalConfirmBox("1.當次週期已填報的檢核點完成日期、辦理情形及落後原因將被清空。\n2.後續檢核點完成日期、每月辦理情形及落後原因將以工程會標案系統資料自動匯入，無需於本系統填報。", async () => {
                let saveResult1 = await saveProjectUsePCC(PROJECT_NO, isUserFtyData);
                let saveResult2 = await clearCycleData(PROJECT_NO);
                if (!saveResult1.success) {
                    showGlobalMessageBox(saveResult1.message);
                }
                if (!saveResult2.success) {
                    showGlobalMessageBox(saveResult2.message)
                }
                setInterfacingSysConfirmVisible(false); // 關閉視窗 
                reloadData(); // 重載頁面資料
            })
        }
        else {
            let saveResult = await saveProjectUsePCC(PROJECT_NO, isUserFtyData);
            if (saveResult.success) {
                showGlobalMessageBox("請填報每月辦理情形及落後原因分析，並完成執行情形送出。", () => {
                    setInterfacingSysConfirmVisible(false); // 關閉視窗 
                    reloadData(); // 重載頁面資料
                })
            }
            else {
                showGlobalMessageBox(saveResult.message)
            }
        }
    }
    //#endregion 

    // 設定外部傳入Grid查詢結果
    useEffect(() => {
        setGridData([...data]);
        editedGridData.current = [];

        if (data.length > 0 && data.find(x => x.CTRL_POINT === "A") !== undefined) {
            setStartWorkDate(FormatDate(data.find(x => x.CTRL_POINT === "A").ACTUAL_ENDDATE));
        }
    }, [data])

    return (
        <PageContainer
            style={{
                height: '100%',
                overflow: 'auto'
            }}
        >
            {isEngineering && // 工程類
                <div className='fn-buttons'>
                    {showSomeBtn &&
                        <Button
                            type='button'
                            onClick={() => { setIsAssociatePCCWindowVisible(true) }}
                            // 是本府執行案件 且  開工 且 (當期執行情形未送出 或 未超過填報週期 或 具管考權限) 才可編輯
                            disabled={!(isStartWork && (canSave || isRdecFun) && isTycgProject)}
                        >
                            關聯工程會標案
                        </Button>
                    }

                    {/* 需關聯工程會標案才能介接工程會標案管理系統 */}
                    {(isAssociate && isStartWork && !isUserFtyData && isTycgProject) &&
                        <Button type='button'
                            disabled={!(canSave || isRdecFun) || isEndWork && !isTycgProject}
                            onClick={() => {
                                setUseFtyDataOrNot(!isUserFtyData)
                            }}
                        >
                            界接工程會標案管理系統
                        </Button>
                    }
                    {(isAssociate && isStartWork && isUserFtyData && isTycgProject) &&
                        <Button type='button'
                            disabled={!(canSave || isRdecFun) && !isTycgProject}
                            onClick={() => {
                                setUseFtyDataOrNot(!isUserFtyData)
                            }}
                        >
                            取消界接工程會標案管理系統
                        </Button>
                    }
                </div>
            }
            <Grid
                style={{
                    textAlign: "center",
                    height: '100%',
                    overflow: 'auto',
                }}
                data={gridData}
                resizable={true}
            >

                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="SEQ" width="0px" />

                <GridColumn field="CHECKITEM_NAME"
                    title="檢核點"
                />
                <GridColumn field="PROGRESS"
                    title="管考進度%"
                    className={Table.textAlign_right}
                    width={'100px'}
                />
                <GridColumn field="ESTIMATED_ENDDATE"
                    title="預定完成日期"
                    cell={PCCFormateCell}
                    width={'150px'}
                />
                <GridColumn
                    field='diffMonth'
                    title="所需時間(月)"
                    className={Table.textAlign_center}
                    width={'100px'}
                />
                <GridColumn
                    field="ACTUAL_ENDDATE"
                    title="實際完成日期"
                    cell={editTwDatePickerCell}
                    className={Table.textAlign_center}
                />
                <GridColumn
                    field='diffMonthActual'
                    title={<span>實際完成日期< br />時間差(月)</span>}
                    className={Table.textAlign_center}
                    width={'130px'}
                />
                <GridColumn
                    field='MDF_DATE'
                    title="填報日期"
                    className={Table.textAlign_center}
                    width={'120px'}
                />
            </Grid>

            {/* 關聯工程會標案視窗 */}
            <AssociatePCCWindow
                visible={isAssociatePCCWindowVisible}
                onClose={() => { setIsAssociatePCCWindowVisible(false) }}
                PROJECT_NO={PROJECT_NO}
                reloadData={reloadData}
                startWorkDate={startWorkDate}
            />

            {/* 界接工程會標案管理系統視窗 */}
            <InterfacingSysConfirm
                visible={isInterfacingSysConfirmVisible}
                onClose={() => { setInterfacingSysConfirmVisible(false) }}
                saveUseFtyDataOrNot={saveUseFtyDataOrNot}
            />

        </PageContainer>
    );
}
export default ProjectFillCkptComGrid
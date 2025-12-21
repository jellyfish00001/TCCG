import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { Button } from '@progress/kendo-react-buttons';
import { getInnPlanYear, getInnPlanDate, saveInnAssignOrg } from './InnAssignOrgService';
import { FormatDate, IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { getPlanYearList } from "../../../Basic/CommonService";

const InnAssignOrgMain = () => {
    //表單儲存資料
    const [savedData, setSavedData] = useState({
        INN_YEAR: "",
        CLOSE_DATE: ""
    });

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState(
        {
            INN_YEAR: [],
        });

    // 載入資料
    const loadData = async () => {
        let result = await getPlanYearList(false, 10, "B")
        result.unshift({ text: "請選擇", value: "" });
        setDdlData({
            INN_YEAR: [...result],
        })
        
    }

    React.useEffect(() => {
        loadData();
    }, [])

    // 下拉選單的 onChange 事件處理函數
    const handleYearChange = async (e) => {
        const selectedYear = e.value.value;
        // 根據選擇的年度獲取相應的截止日期
        let result = await getInnPlanDate(selectedYear);

        SetMaskOnOff(true);
        // 更新截止日期
        setSavedData({
            INN_YEAR: selectedYear,
            CLOSE_DATE: result.CLOSE_DATE == null ? "" : new Date(result.CLOSE_DATE)
        });

        SetMaskOnOff(false);
    };


    /**
     * 存檔
     * @param
     */
    const saveChanges = async () => {
        SetMaskOnOff(true);
        let result = await saveInnAssignOrg(savedData)
        showGlobalMessageBox(result.message);
        SetMaskOnOff(false);
    }


    return (
        <PageContainer
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">截止時間</h3>
                    <Button title="存檔" type="button" onClick={saveChanges}>存檔</Button>
                </>
            }
        >
            <table style={{ width: '25%', borderCollapse: 'collapse' }}>
                <tr>
                    <th>
                        年度：
                    </th>
                    <td>
                        <DropDownListWithValue
                            data={ddlData.INN_YEAR}
                            textField={"text"}
                            dataItemKey={"value"}
                            value={savedData.INN_YEAR}
                            onChange={handleYearChange}
                        />
                    </td>
                </tr>
                
                <tr>
                    <th>
                        截止辦理時間：
                    </th>
                    <td>
                        <TwDatePicker
                            name={"CLOSE_DATE"}
                            format={"tYY/MM/dd HH:mm"}
                            value={savedData.CLOSE_DATE}
                            onChange={(e) => {
                                setSavedData({ ...savedData, CLOSE_DATE: e.value });
                            }}
                            timepick={true}
                        />
                    </td>

                </tr>
            </table>

        </PageContainer>
    )
}
export default InnAssignOrgMain;
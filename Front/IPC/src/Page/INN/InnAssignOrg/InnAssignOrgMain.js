import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { Button } from '@progress/kendo-react-buttons';
import * as Yup from 'yup';

const InnAssignOrgMain= (props) => {
    // 這裡假設grid資料從外部加載
    const { data } = props;
    //表單異動資料
    const formRef = useRef();
    //表單儲存資料
    const [savedData, setSavedData] = useState({});

    // 存檔
    const saveChanges = async (data) => {
        console.log(data);
    }


    return (
        <PageContainer
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">提案登錄</h3>
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
                <tr>
                    <th>
                        截止辦理時間：
                    </th>
                    <td>
                        <TwDatePicker
                            name={"AWARD_BID_DATE"}
                            format={"yyy/MM/dd HH:mm"}
                            onChange={(e) => {

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
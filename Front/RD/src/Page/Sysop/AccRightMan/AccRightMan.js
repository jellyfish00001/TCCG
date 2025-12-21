import React from 'react';
import AccRightManService from './accRightMan.service';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button, } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { NumericTextBox, Checkbox, RadioButton } from '@progress/kendo-react-inputs';

const Comment = (props) => {
    const [policyData, setPolicyData] = React.useState({
        POLICY_ID: "SDO",
        POLICY_COMP_ID: "GSS",
        ID_MIN_LEN: 6,
        PASS_MIN_LEN: 7,
        PASS_NO_SAME_ID_NAME: "Y",
        PASS_MIX_CHAR_NUM: "N",
        PASS_NO_SPEC_CHAR: "Y",
        checkboxPASS_AT_LEAST_SPECIAL_CHARS: "N",
        PASS_AT_LEAST_SPECIAL_CHARS: 0,
        PASS_NO_SAME_2: "N",
        PASS_NO_CONT_3: "N",
        checkboxPASS_NO_SAME_PASS_TIMES: "Y",
        PASS_NO_SAME_PASS_TIMES: 6,
        ID_NO_CLOSE: "Y",
        checkboxID_DISABLE_IN_CONT: "N",
        ID_DISABLE_IN_CONT: 0,
        ID_DISABLE_IN_CONT_TIMES: 0,
        checkboxID_DISABLE_IN_NONCONT: "N",
        ID_DISABLE_IN_NONCONT: 0,
        ID_DISABLE_IN_NONCONT_TIMES: 0
    });

    // 取得帳號權限管理
    const getPolicy = async () => {
        // 取得資料
        let result = await AccRightManService.getAccRightMan(policyData.POLICY_ID, policyData.POLICY_COMP_ID);

        // 接到data後資料更新參數
        setPolicyData({
            ...result,
            checkboxPASS_AT_LEAST_SPECIAL_CHARS: result.PASS_AT_LEAST_SPECIAL_CHARS > 0 ? "Y" : "N",
            checkboxPASS_NO_SAME_PASS_TIMES: result.PASS_NO_SAME_PASS_TIMES > 0 ? "Y" : "N",
            checkboxID_DISABLE_IN_CONT: result.ID_DISABLE_IN_CONT > 0 || result.ID_DISABLE_IN_CONT_TIMES > 0 ? "Y" : "N",
            checkboxID_DISABLE_IN_NONCONT: result.ID_DISABLE_IN_NONCONT > 0 || result.ID_DISABLE_IN_NONCONT_TIMES > 0 ? "Y" : "N"
        })
    }

    const loadData = async () => {
        await getPolicy();
    }

    React.useEffect(() => {
        loadData();
    }, [policyData.POLICY_ID]);

    const numericChange = (e) => {
        let value = e.target.value;
        const name = e.target.props.name;

        if ((name === "ID_MIN_LEN" || name === "PASS_MIN_LEN") && (IsNullOrEmpty(value) || value === 0)) {
            // 帳號和密碼長度最短不可少於1
            value = 1;
        }
        else if (IsNullOrEmpty(value)) {
            // 數字欄位空白自動轉為0
            value = 0;
        }

        setPolicyData(prevState => ({
            ...prevState,
            [name]: value
        }));
    }

    const checkboxChange = (e) => {
        const value = e.value;
        const name = e.target.element.name;

        let update = { [name]: value ? 'Y' : 'N' };

        if (name === "PASS_NO_SPEC_CHAR") {
            // 不可含空白或特殊字元勾選後，至少含X個特殊字元為不可勾選 數字歸零
            if (value) {
                update = {
                    ...update,
                    checkboxPASS_AT_LEAST_SPECIAL_CHARS: "N",
                    PASS_AT_LEAST_SPECIAL_CHARS: 0
                };
            }
        }
        else if (name === "checkboxPASS_AT_LEAST_SPECIAL_CHARS" && !value) {
            // 至少含X個特殊字元為取消勾選，後面數字輸入框要自動歸零
            update = {
                ...update,
                PASS_AT_LEAST_SPECIAL_CHARS: 0
            };
        }
        else if (name === "checkboxPASS_NO_SAME_PASS_TIMES" && !value) {
            // 不可與最近X次內重複，後面數字輸入框要自動歸零
            update = {
                ...update,
                PASS_NO_SAME_PASS_TIMES: 0
            };

        }
        else if (name === "checkboxID_DISABLE_IN_CONT" && !value) {
            // 同一天密碼連續錯誤X次X分鐘，數字輸入框要自動歸零
            update = {
                ...update,
                ID_DISABLE_IN_CONT: 0,
                ID_DISABLE_IN_CONT_TIMES: 0
            };
        }
        else if (name === "checkboxID_DISABLE_IN_NONCONT" && !value) {
            // 同一天密碼非連續錯誤X次X分鐘，數字輸入框要自動歸零
            update = {
                ...update,
                ID_DISABLE_IN_NONCONT: 0,
                ID_DISABLE_IN_NONCONT_TIMES: 0
            };
        }

        setPolicyData(prevState => ({
            ...prevState,
            ...update
        }));
    }


    // 是否啟用帳號關閉原則(radioButton)
    const radioButtonChange = (e) => {
        const value = e.value;
        let update = { ID_NO_CLOSE: value };
        // 選擇停用處理
        if (value === "Y") {
            update = {
                ...update,
                checkboxID_DISABLE_IN_CONT: "N",
                ID_DISABLE_IN_CONT: 0,
                ID_DISABLE_IN_CONT_TIMES: 0,
                checkboxID_DISABLE_IN_NONCONT: "N",
                ID_DISABLE_IN_NONCONT: 0,
                ID_DISABLE_IN_NONCONT_TIMES: 0
            };
        }
        setPolicyData(prevState => ({
            ...prevState,
            ...update
        }));

    }

    // 存檔
    const submit = async () => {
        let result = await AccRightManService.updateAccRightMan(policyData);
        showGlobalMessageBox(result.message);
    }

    // 載入default設定
    const resest = () => {
        setPolicyData({
            ...policyData,
            POLICY_COMP_ID: "GSS",
            ID_MIN_LEN: 6,
            PASS_MIN_LEN: 7,
            PASS_NO_SAME_ID_NAME: "Y",
            PASS_MIX_CHAR_NUM: "N",
            PASS_NO_SPEC_CHAR: "Y",
            checkboxPASS_AT_LEAST_SPECIAL_CHARS: "N",
            PASS_AT_LEAST_SPECIAL_CHARS: 0,
            PASS_NO_SAME_2: "N",
            PASS_NO_CONT_3: "N",
            checkboxPASS_NO_SAME_PASS_TIMES: "Y",
            PASS_NO_SAME_PASS_TIMES: 6,
            ID_NO_CLOSE: "Y",
            checkboxID_DISABLE_IN_CONT: "N",
            ID_DISABLE_IN_CONT: 0,
            ID_DISABLE_IN_CONT_TIMES: 0,
            checkboxID_DISABLE_IN_NONCONT: "N",
            ID_DISABLE_IN_NONCONT: 0,
            ID_DISABLE_IN_NONCONT_TIMES: 0
        });
    }

    return (
        <div className="fnForm">
            <div>
                <div>
                    <div className="fn-buttons">
                        <Button onClick={submit}>存檔</Button>
                        <Button onClick={resest}>載入default設定</Button>
                    </div>
                    <table style={{ width: '600px' }}  >
                        <tbody>
                            <tr>
                                <th scope="col">對象</th>
                                <td>
                                    <DropDownListWithValue
                                        data={
                                            [
                                                { text: '審查委員', value: 'RVWUser' },
                                                { text: '共通平台使用者', value: 'SDO' },
                                                { text: '申請廠商', value: 'WAPLUser' }
                                            ]
                                        }
                                        textField={"text"}
                                        dataItemKey={"value"}
                                        value={policyData.POLICY_ID}
                                        onChange={(e) => { setPolicyData({ ...policyData, POLICY_ID: e.value.value }) }}
                                    />
                                </td>
                            </tr>
                            <tr>
                                <th scope="col">帳號限制</th>
                                <td>
                                    最短長度:
                                    <NumericTextBox
                                        defaultValue={policyData.ID_MIN_LEN}
                                        min={1}
                                        max={50}
                                        value={policyData.ID_MIN_LEN}
                                        name="ID_MIN_LEN"
                                        width='70px'
                                        onChange={numericChange}
                                    />
                                    至少個字元
                                </td>
                            </tr>
                            <tr>
                                <th scope="col">密碼限制</th>
                                <td>
                                    最短長度:
                                    <NumericTextBox
                                        defaultValue={policyData.PASS_MIN_LEN}
                                        min={1}
                                        max={50}
                                        value={policyData.PASS_MIN_LEN}
                                        name="PASS_MIN_LEN"
                                        onChange={numericChange}
                                        width='70px'
                                    />
                                    至少個字元
                                </td>
                            </tr>
                            <tr>
                                <th scope="col"></th>
                                <td>
                                    <Checkbox
                                        name="PASS_NO_SAME_ID_NAME"
                                        checked={policyData.PASS_NO_SAME_ID_NAME === "Y"}
                                        onChange={checkboxChange}
                                        label={'不可與帳號或使用者名稱相同'} />
                                </td>
                            </tr>
                            <tr>
                                <th scope="col"></th>
                                <td>
                                    <Checkbox
                                        name="PASS_MIX_CHAR_NUM"
                                        checked={policyData.PASS_MIX_CHAR_NUM === "Y" ? true : false}
                                        onChange={checkboxChange}
                                        label={'密碼必須為文數字混合(英文大、小寫、數字、特殊符號四種須符合三種)'} />
                                </td>
                            </tr>
                            <tr>
                                <th scope="col"></th>
                                <td>
                                    <Checkbox
                                        name="PASS_NO_SPEC_CHAR"
                                        checked={policyData.PASS_NO_SPEC_CHAR === "Y" ? true : false}
                                        onChange={checkboxChange}
                                        label={'不可含空白或特殊字元，只能用文字 A-Z 或數字 1-9 組成'} />
                                </td>
                            </tr>
                            <tr>
                                <th scope="col"></th>
                                <td>
                                    <Checkbox
                                        name="checkboxPASS_AT_LEAST_SPECIAL_CHARS"
                                        disabled={policyData.PASS_NO_SPEC_CHAR === "Y" ? true : false}
                                        checked={policyData.checkboxPASS_AT_LEAST_SPECIAL_CHARS === "Y" ? true : false}
                                        onChange={checkboxChange}
                                        label={'至少含'} />

                                    <NumericTextBox
                                        defaultValue={policyData.PASS_AT_LEAST_SPECIAL_CHARS}
                                        min={0}
                                        max={50}
                                        disabled={policyData.checkboxPASS_AT_LEAST_SPECIAL_CHARS !== "Y" ? true : false}
                                        value={policyData.PASS_AT_LEAST_SPECIAL_CHARS}
                                        name="PASS_AT_LEAST_SPECIAL_CHARS"
                                        onChange={numericChange}
                                        width='70px'
                                    />
                                    個特殊字元
                                </td>
                            </tr>
                            <tr>
                                <th scope="col"></th>
                                <td>
                                    <Checkbox
                                        name="PASS_NO_SAME_2"
                                        checked={policyData.PASS_NO_SAME_2 === "Y" ? true : false}
                                        onChange={checkboxChange}
                                        label={'相鄰二字元不可相同'} />
                                </td>
                            </tr>
                            <tr>
                                <th scope="col"></th>
                                <td>
                                    <Checkbox
                                        name="PASS_NO_CONT_3"
                                        checked={policyData.PASS_NO_CONT_3 === "Y" ? true : false}
                                        onChange={checkboxChange}
                                        label={'相鄰三字元不可為連續升冪或降冪'} />
                                </td>
                            </tr>
                            <tr>
                                <th scope="col"></th>
                                <td>
                                    <Checkbox
                                        name="checkboxPASS_NO_SAME_PASS_TIMES"
                                        checked={policyData.checkboxPASS_NO_SAME_PASS_TIMES === "Y" ? true : false}
                                        onChange={checkboxChange}
                                        label={'不可與最近'} />

                                    <NumericTextBox
                                        defaultValue={policyData.PASS_NO_SAME_PASS_TIMES}
                                        min={1}
                                        max={50}
                                        disabled={policyData.checkboxPASS_NO_SAME_PASS_TIMES !== "Y" ? true : false}
                                        value={policyData.PASS_NO_SAME_PASS_TIMES}
                                        name="PASS_NO_SAME_PASS_TIMES"
                                        onChange={numericChange}
                                        width='70px'
                                    />
                                    次內重複
                                </td>
                            </tr>
                            <tr>
                                <th scope="col">
                                    帳號關閉原則
                                </th>
                                <td>
                                    <RadioButton
                                        name="ID_NO_CLOSE"
                                        value="Y"
                                        checked={policyData.ID_NO_CLOSE === "Y"}
                                        label="停用"
                                        onChange={radioButtonChange} />
                                    <RadioButton
                                        name="ID_NO_CLOSE"
                                        value="N"
                                        checked={policyData.ID_NO_CLOSE === "N"}
                                        label="啟用"
                                        onChange={radioButtonChange} />
                                </td>
                            </tr>
                            <tr>
                                <th scope="col"></th>
                                <td>
                                    <Checkbox
                                        name="checkboxID_DISABLE_IN_CONT"
                                        checked={policyData.checkboxID_DISABLE_IN_CONT === "Y" ? true : false}
                                        onChange={checkboxChange}
                                        disabled={policyData.ID_NO_CLOSE === "Y"}
                                        label={'同一天密碼連續錯誤'} />

                                    <NumericTextBox
                                        defaultValue={policyData.ID_DISABLE_IN_CONT}
                                        min={1}
                                        max={50}
                                        disabled={policyData.checkboxID_DISABLE_IN_CONT !== "Y" ? true : false}
                                        value={policyData.ID_DISABLE_IN_CONT}
                                        name="ID_DISABLE_IN_CONT"
                                        onChange={numericChange}
                                        width='70px'
                                    />
                                    次，帳號鎖定
                                    <NumericTextBox
                                        defaultValue={policyData.ID_DISABLE_IN_CONT_TIMES}
                                        min={1}
                                        max={50}
                                        disabled={policyData.checkboxID_DISABLE_IN_CONT !== "Y" ? true : false}
                                        value={policyData.ID_DISABLE_IN_CONT_TIMES}
                                        name="ID_DISABLE_IN_CONT_TIMES"
                                        onChange={numericChange}
                                        width='70px'
                                    />
                                    分鐘
                                </td>
                            </tr>
                            <tr>
                                <th scope="col"></th>
                                <td>
                                    <Checkbox
                                        name="checkboxID_DISABLE_IN_NONCONT"
                                        checked={policyData.checkboxID_DISABLE_IN_NONCONT === "Y" ? true : false}
                                        onChange={checkboxChange}
                                        disabled={policyData.ID_NO_CLOSE === "Y"}
                                        label={'同一天密碼非連續錯誤'} />

                                    <NumericTextBox
                                        defaultValue={policyData.ID_DISABLE_IN_NONCONT}
                                        min={1}
                                        max={50}
                                        disabled={policyData.checkboxID_DISABLE_IN_NONCONT !== "Y" ? true : false}
                                        value={policyData.ID_DISABLE_IN_NONCONT}
                                        name="ID_DISABLE_IN_NONCONT"
                                        onChange={numericChange}
                                        width='70px'
                                    />
                                    次，帳號鎖定
                                    <NumericTextBox
                                        defaultValue={policyData.ID_DISABLE_IN_NONCONT_TIMES}
                                        min={1}
                                        max={50}
                                        disabled={policyData.checkboxID_DISABLE_IN_NONCONT !== "Y" ? true : false}
                                        value={policyData.ID_DISABLE_IN_NONCONT_TIMES}
                                        name="ID_DISABLE_IN_NONCONT_TIMES"
                                        onChange={numericChange}
                                        width='70px'
                                    />
                                    分鐘
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
}
export default Comment;
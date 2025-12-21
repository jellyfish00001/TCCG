import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import TextInput from '../../../Components/Input/TextInput';
import RadioBoxList from '../../../Components/Input/RadioBoxList';
import { MultiSelect } from '@progress/kendo-react-dropdowns';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import * as Yup from 'yup';
import InnProjectProposalGrid from "./InnProjectProposalGrid";

const InnProjectProposalMain = (props) => {
    // 這裡假設grid資料從外部加載
    const { data } = props;
    // 假資料
    const fakedata = {
        PLAN_NO: "0001",
        PLAN_NAME: "",
        PLAN_CLASS: '',
        EXP_RESULT: '',
        PHONE: '0900000',
        EMAIL: 'test@test.com',
        test: "test",
        PLAN_OTHER_CLASS: [
            { SET_TYPE: 1, SET_VALUE: '選項1' },
            { SET_TYPE: 2, SET_VALUE: '選項2' },
            { SET_TYPE: 3, SET_VALUE: '選項3' },
        ]

    }

    //表單異動資料
    const formRef = useRef();
    //表單資烙
    const [formData, setFormData] = useState(fakedata);
    //表單儲存資料
    const [savedData, setSavedData] = useState({});
    //是否是顯示涉及其他提案類別
    const [isPlanOtherClass, setIsPlanOtherClass] = useState(false);
    //顯示自訂欄位
    const [showCustomFields, setShowCustomFields] = useState(false);

    // 下拉選單
    const Options = [
        { text: '請選擇', value: '' },
        { text: 'AA', value: 'A' },
        { text: 'BB', value: 'B' },
        { text: '選項3', value: '3' }
    ];

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.validateForm().then(errors => {
                // 檢查表單是否有錯誤
                if (Object.keys(errors).length === 0) {
                    formRef.current.handleSubmit();
                } else {
                    // 錯誤訊息
                    console.log('表單驗證失敗', errors);
                }
            });
        }
    };


    // 存檔
    const saveChanges = async (data) => {
        console.log(data);
    }

    // 表單取消
    const cancel = () => {
        if (formRef.current) {
            // 重置表單為初始值
            formRef.current.resetForm({ values: fakedata });
        }
    };



    // 表單檔案上傳
    const upload = (values) => {
        console.log(values);
    };

    return (
        <PageContainer style={{ overflow: "auto", height: "100%" }}>
            <CollapseBoardCard

                button={
                    <>
                        <Button title="存檔" onClick={handleSubmit}>存檔</Button>
                        <Button title="取消" onClick={cancel}>取消</Button>
                    </>
                }
                title="提案填報"
                isFirstArea={true}
            >
                <Formik
                    initialValues={formData}
                    onSubmit={(data) => saveChanges(data)}
                    enableReinitialize={true}
                    innerRef={formRef}
                    //字段改變不驗證
                    validateOnChange={false}
                >
                    {props => {
                        const {
                            values,
                            errors,
                            handleChange,
                            setValues
                        } = props;
                        return (
                            <form>
                                <table>
                                    <tr>
                                        <th>
                                            提案編號
                                        </th>
                                        <td colSpan={3}>
                                            {values.PROJECT_NO}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            提案人數
                                        </th>
                                        <td colSpan={3}>
                                            <RadioBoxList
                                                group='Num_People'
                                                valueField='value'
                                                textField='text'
                                                data={[

                                                    {
                                                        text: '個人提案', value: true,
                                                    },
                                                    {
                                                        text: '團體提案(2人以上)', value: false,
                                                    }
                                                ]}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            組別
                                        </th>
                                        <td colSpan={3}>
                                            <RadioBoxList
                                                group='Group'
                                                valueField='value'
                                                textField='text'
                                                data={[

                                                    {
                                                        text: 'A組(提案可應用於本市各區或特定族群)', value: true,
                                                    },
                                                    {
                                                        text: 'B組(提案可應用於本市部分地區，如行政區里)', value: false,
                                                    }
                                                ]}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            提案名稱
                                        </th>
                                        <td colSpan={3}>
                                            <TextInput
                                                onChange={handleChange}
                                                value={values.PROJECT_NAME}
                                                name="PROJECT_NAME"
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <CommonTooltip title={" 是否有實施提案所訂不受理範圍之各項情形？"} content={
                                                <>
                                                    不受理情形如下<br />
                                                    (1)112年6月30日前已執行完成<br />
                                                    (2)抄襲網站資料、他人委託研究案、論文或其他著作等侵害第三人智慧財產權情事。<br />
                                                    (3)112年6月30日前已在本府或其他機關獲獎者。
                                                </>
                                            } />

                                        </th>
                                        <td colSpan={3}>
                                            <RadioBoxList
                                                group='NumberOfPeople'
                                                valueField='value'
                                                textField='text'
                                                data={[

                                                    {
                                                        text: '無', value: false,
                                                    },
                                                    {
                                                        text: '有', value: true,
                                                    }
                                                ]}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            主要提案類別
                                        </th>
                                        <td colSpan={3}>
                                            <div style={{ display: 'flex', alignItems: 'center' }}>
                                                <DropDownListWithValue
                                                    name="PLAN_CLASS"
                                                    data={Options}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.PLAN_CLASS}
                                                    onChange={(e) => { setValues({ ...values, PLAN_CLASS: e.target.value }) }}
                                                    error={errors.PLAN_CLASS}
                                                />
                                            </div>
                                        </td>
                                    </tr>
                                    {isPlanOtherClass && (
                                        <tr>
                                            <th>
                                                涉及其他提案類別
                                            </th>
                                            <td colSpan={3}>
                                                <MultiSelect
                                                    popupSettings={
                                                        { className: "dropdown-text-size" }
                                                    }
                                                    placeholder="請選擇   "
                                                    name='PLAN_OTHER_CLASS'
                                                    data={values.PLAN_OTHER_CLASS.filter(option => option.SET_TYPE !== values.PLAN_CLASS)}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    onChange={(e) => {
                                                    }}
                                                    value={values.PLAN_OTHER_CLASS}
                                                    error={errors.PLAN_OTHER_CLASS}
                                                />
                                            </td>
                                        </tr>
                                    )}
                                    <tr>
                                        <th className="addRedStar">
                                            問題描述
                                        </th>
                                        <td colSpan={3}>
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='DESCRIBE'
                                                value={values.a}
                                                onChange={handleChange}
                                                error={errors.a}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            提案構想解決方式
                                        </th>
                                        <td colSpan={3}>
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='IDEA'
                                                value={values.b}
                                                onChange={handleChange}
                                                error={errors.b}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            預期效益
                                        </th>
                                        <td colSpan={3}>
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='BENEFIT'
                                                value={values.c}
                                                onChange={handleChange}
                                                error={errors.c}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            是否為本府尚未推行過之創意
                                        </th>
                                        <td colSpan={3}>
                                            <RadioBoxList
                                                group='IS_GOV_FIRST'
                                                valueField='value'
                                                textField='text'
                                                data={[

                                                    {
                                                        text: '是', value: true,
                                                    },
                                                    {
                                                        text: '否', value: false,
                                                    }
                                                ]}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            是否為全國首創？
                                        </th>
                                        <td colSpan={3}>
                                            <RadioBoxList
                                                group='IS_COUNTRY_FIRST'
                                                valueField='value'
                                                textField='text'
                                                data={[

                                                    {
                                                        text: '是', value: true,
                                                    },
                                                    {
                                                        text: '否', value: false,
                                                    }
                                                ]}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            主要提案人員－機關
                                        </th>
                                        <td>
                                            {values.test}
                                        </td>
                                        <th className="addRedStar">
                                            主要提案人員－所屬單位
                                        </th>
                                        <td>
                                            {values.test}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            主要提案人員－職稱
                                        </th>
                                        <td>
                                            {values.test}
                                        </td>
                                        <th className="addRedStar">
                                            主要提案人員－姓名
                                        </th>
                                        <td>
                                            {values.test}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            主要提案人員－性別
                                        </th>
                                        <td>
                                            {values.test}
                                        </td>
                                        <th className="addRedStar">
                                            聯絡人
                                        </th>
                                        <td>
                                            {values.test}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            電話
                                        </th>
                                        <td>
                                            {values.PHONE}
                                        </td>
                                        <th className="addRedStar">
                                            Email
                                        </th>
                                        <td>
                                            {values.EMAIL}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            參與提案人(最多5)名
                                        </th>
                                        <td colSpan={3}>
                                            <InnProjectProposalGrid
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            附件上傳
                                        </th>
                                        <td colSpan={3}>
                                            <Button title="檔案上傳" onClick={upload}>檔案上傳</Button>
                                        </td>
                                    </tr>

                                </table>
                            </form>


                        )

                    }}
                </Formik>
            </CollapseBoardCard>
            {showCustomFields && (<form>
                <table>
                    <tr>
                        <th>

                        </th>
                        <td colSpan={3}>
                            <PureHtmlTextAreaInput
                                rows={2}

                            />
                        </td>
                    </tr>

                </table>
            </form>)}
        </PageContainer>
    )
}
export default InnProjectProposalMain;
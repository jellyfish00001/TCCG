import React, { useState, useRef, useEffect } from "react";
import { Formik} from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import * as Yup from 'yup';

/**
 * 研究發展作業系統-委託研究計劃-結案一年之內參採情形
 * 結案情形
 * PPT P15
 * 下拉選單 資料寫死測試
 */

const ProjectExceClose = (props) => { 
    // 這裡假設grid資料從外部加載
    const { data } = props;
    // 假資料
    const fakedata = {
        PROJECT_NO: "110-0001",
        PROJECT_NAME: "測試專案",
        DATE: "2021/09/01",
        PRO_STATUS: '',
        EXP_RESULT: '',
    }
    //表單異動資料
    const formRef = useRef();
    //表單資烙
    const [formData, setFormData] = useState(fakedata);
    //表單儲存資料
    const [savedData, setSavedData] = useState({});
    //表單錯誤訊息
    const iniError = {};
    const [error, setError] = useState({iniError});
    // 研究情形下拉選單
    const Options = [
        { text: '請選擇', value: '' },
        { text: '採行', value: 'A' },
        { text: '參採', value: 'B' },
        { text: '存查', value: 'C' }
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

    // 驗證規則
    const validationSchema = Yup.object().shape({
        PRO_STATUS: Yup.string().required('研究建議處理情形為必填項'),
        EXP_RESULT: Yup.string().required('預期研究成果為必填項'),
    });

    // 存檔
    const saveChanges = async (data) => {

    }

    // 表單取消
    const cancel = () => {
        if (formRef.current) {
            // 重置表單為初始值
            formRef.current.resetForm({ values: fakedata });
        }
    };

    // 表單送出審核
    const check = (values) => {

    };

    // 表單檔案上傳
    const upload = (values) => {
        
    }; 

        return (
            <PageContainer>
                <CollapseBoardCard
                button={
                    <>
                        <Button title="存檔" className="k-button-lighten" onClick={handleSubmit}>存檔</Button>
                        <Button title="存檔送出" className="k-button-lighten" onClick={check}>存檔送出</Button>
                        <Button title="取消" className="k-button-lighten" onClick={cancel}>取消</Button>
                    </>
                }
                title="結案情形"
                isFirstArea={true}
                >     
                    <Formik
                        initialValues={formData}
                        validationSchema={validationSchema}
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
                                                    計畫編號
                                                </th>
                                                <td>
                                                    {values.PROJECT_NO}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    計畫名稱
                                                </th>
                                                <td >
                                                    {values.PROJECT_NAME}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                    研究建議處理情形
                                                </th>
                                                <td>
                                                    <DropDownListWithValue
                                                        name="PRO_STATUS"
                                                        data={Options}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.PRO_STATUS}
                                                        onChange={(e) => { setValues({ ...values, PRO_STATUS: e.target.value }) }}
                                                        error={errors.PRO_STATUS}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                預期研究成果
                                                </th>
                                                <td>
                                                <PureHtmlTextAreaInput
                                                    rows={5}
                                                    name='EXP_RESULT'
                                                    value={values.EXP_RESULT}
                                                    onChange={handleChange}
                                                    error={errors.EXP_RESULT}
                                                />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                附件上傳
                                                </th>
                                                <td >
                                                <Button title="檔案上傳" onClick={upload}>檔案上傳</Button>
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    結案日期
                                                </th>
                                                <td >
                                                    {values.DATE}
                                                </td>
                                            </tr>
                                    </table>
                                </form>
                            )
                        }}
                    </Formik>
                </CollapseBoardCard>
            </PageContainer>
        )
        }
        export default ProjectExceClose;
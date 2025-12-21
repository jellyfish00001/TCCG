import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import TwDatePicker from '../../../Components/DateInputs/TwDatePicker';
import { getPlanYear, expirationDate, getPlanDeadline } from "./AssignWorkService";
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';

const AssignWorkMain = (props) => { 
    //下拉選單數據
    const [year, setYear] = useState([]);    
    // 表单數據
    const formRef = useRef();
    // 初始表單數據
    const [initialFormData, setInitialFormData] = useState({
        PLANYEAR: '0'
    });
    // 取得年分
    const planYearRef =useRef('0');

    //呼叫表單提交
    const handleSubmit = () => {
        formRef.current.submitForm();
    };

    /**
     * 計畫年度下拉選單
     * @returns
     */
    const PlanYear = async () => {
        let result = await getPlanYear();
        setYear(result);
        if (result.length > 0) {
            planYearRef.current = result[0].PLANYEAR;
            setInitialFormData({ ...initialFormData, PLANYEAR: result[0].PLANYEAR });
        }
    };

    /**
     * 取得截止時間
     * @returns
     * @param {*} data
     */
    const getDeadline = async (data) => {
        let result = await getPlanDeadline(data);
        let DisDeadline = result.DistrictHallEndTime ? new Date(result.DistrictHallEndTime) : null;
        let OrgDeadline = result.OrgEndTime ? new Date(result.OrgEndTime) : null;
        if (result) {
            setInitialFormData({ 
                ...initialFormData, 
                DistrictHallEndTime: DisDeadline, 
                OrgEndTime: OrgDeadline,
                PLANYEAR: data 
            });
        }
    };

    /**
     * 存檔
     * @param {*} data
     * @returns 
     */
    const saveChanges = async (data) => {
        let saveResult = await expirationDate(data);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => window.location.reload());
        }
    };

    // 初始化下拉選單
    useEffect(() => {
        // 取下拉選單
        PlanYear();

        // 取得截止時間
        if (planYearRef.current !== '0') {
            getDeadline(planYearRef.current);
        }
    }, [planYearRef.current]);

    return (
        <PageContainer>
            <CollapseBoardCard
                button={
                    <>
                        <Button title="存檔" className="k-button-lighten" onClick={handleSubmit}>存檔</Button>
                    </>
                }
                title="指派作業"
                isFirstArea={true}
            >     
                <Formik
                    initialValues={initialFormData}
                    onSubmit={(data) => saveChanges(data)}
                    innerRef={formRef}
                    enableReinitialize
                >
                    {props => {
                        const {
                            values,
                            errors,
                            handleChange,
                            handleSubmit,
                            setFieldValue,
                            setValues
                        } = props;
                        return (
                        <form>
                            <table>
                                <tr>
                                    <th>計畫年度</th>
                                    <td>
                                        <DropDownListWithValue
                                            name="PLANYEAR"
                                            data={year}
                                            textField="PLANYEAR"
                                            dataItemKey="PLANYEAR"
                                            value={values.PLANYEAR}
                                            onChange={e => {
                                                getDeadline(e.target.value);
                                                setFieldValue("PLANYEAR", e.target.value);
                                            }}
                                            width={300}
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th>區公所截止辦理時間</th>
                                    <td>
                                        <div style={{ display: 'flex', alignItems: 'center'}}>
                                        <TwDatePicker
                                            id="DATE"
                                            name="DistrictHallEndTime"
                                            format={"tYY/MM/DD HH:mm"}
                                            timepick={true}
                                            value={values.DistrictHallEndTime}
                                            onChange={handleChange}
                                        />
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <th>截止辦理時間</th>
                                    <td>
                                        <div style={{ display: 'flex', alignItems: 'center'}}>
                                        <TwDatePicker
                                            id="DATE"
                                            name="OrgEndTime"
                                            format={"tYY/MM/DD HH:mm"}
                                            timepick={true}
                                            value={values.OrgEndTime}
                                            onChange={handleChange}
                                        />
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </form>
                    )
                    }}
                </Formik>
            </CollapseBoardCard>
        </PageContainer>
    );
};

export default AssignWorkMain;

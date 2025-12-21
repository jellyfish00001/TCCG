import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import OrgSelectPanel from '../../../Components/Selector/OrgSelectPanel';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Formik } from 'formik';
import TextInput from '../../../Components/Input/TextInput';
import { ReviewProjectGrid } from './ReviewProjectGrid';
import { getPlanYearList, GetSetParam } from "../../../Basic/CommonService";
import { SetMaskOnOff, IsNullOrEmpty } from "../../../Basic/SDOExtension";
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { GetPWSProjectList, ReturnProject } from "./ReviewProjectService";

const ReviewProjectMain = (props) => {
    // 這裡假設grid資料從外部加載
    const { data } = props;
    // 表單初始值
    const initialValues = ({
        PLANYEAR: (new Date().getFullYear() - 1911 + 1).toString(),
        SEND_STATUS: null
    });
    // grid資料
    const [selectResultData, setSelectResultData] = useState([]);
    // 表單資料
    const [formData, setFormData] = useState({...initialValues});
    const formRef = useRef(null);
    // 下拉選單年度
    const [yearOptions, setYearOptions] = useState([]);

    // 下拉選單計畫類別
    const [statusOptions, setStatusOptions] = useState([]);
    // 組織樹資料
    const [treeData, setTreeData] = useState([]);
    // 選擇退回的計畫類別
    const [selectedPlanKind, setSelectedPlanKind] = useState('');


    /**
     * 初始畫面
     * @returns
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        // 取得年度下拉選單
        let Year = await getPlanYearList(false, 20, "A");
        setYearOptions(Year);
        let Options = await GetSetParam('AUDIT_STATUS', '');
        setStatusOptions([ { SET_VALUE: "請選擇", SET_TYPE: null }, ...Options]);

        SetMaskOnOff(false);
    }

    /**
     * 查詢
     * @param {*} data 
     * @param {*} isResetOrgTree 
     * @returns
     */
    const select = async (data, isResetOrgTree = true) => {
        SetMaskOnOff(true);
        let result = await GetPWSProjectList(data);
        if(isResetOrgTree){        
            let treeData = genOrgTreeData(result);
            setTreeData(treeData);
        }
        setSelectResultData(result);
        SetMaskOnOff(false);
    }

    //  初始化頁面
    useEffect(() => {
        loadData();
    }, [])

    
    // 及時查詢
    useEffect(() => {
        select(formData);
    }, [formData])

    /**
     * 產生組織樹資料
     * @param {*} data 
     * @returns 
     */
    const genOrgTreeData = (data) => {
        // 取得計劃清單機關清單
        let projOrgPanelDataList = data.filter(proj => !IsNullOrEmpty(proj.OU_ID))
            .map(proj => {
                let projCnt = data.filter(
                    x => x.OU_ID === proj.OU_ID 
                    && x.PLANKIND === proj.PLANKIND 
                    && x.SENDTYPE === proj.SENDTYPE
                    ).length;
                return {
                    OU_ID: proj.OU_ID,
                    PLANKIND: proj.PLANKIND,
                    SENDTYPE: proj.SENDTYPE,
                    SEND_STATUS: proj.SEND_STATUS,
                    title: `${proj.ORGOUNAME} - ${proj.PLANKINDNAME} - ${proj.SENDTYPE} -(${projCnt})`
                }
            });
        // 去除重複的組織數據
        let distinctData = projOrgPanelDataList.filter((value, index, self) =>
            index === self.findIndex((t) => (
                t.OU_ID === value.OU_ID 
                && t.PLANKIND === value.PLANKIND 
                && t.SENDTYPE === value.SENDTYPE
            ))
        );
        // 在清單前面新增一個總項
        distinctData.unshift({ OU_ID: "", title: `桃園市政府(${data.length})` });
        return distinctData;
    };
    
    /**
     * 組織樹選擇
     * @param {*} ouId 
     * @returns
     */
    const onSelect = async (data) => {
        let ouId = data.OU_ID;
        let PlanKind = data.PLANKIND;
        let SendStatus = data.SEND_STATUS;
        // 根據所選組織ID更新查詢條件
        let updatedQueryCondition = {
            ...formRef.current.values,
            OU_ID: ouId,
            PLANKIND: PlanKind,
            SEND_STATUS: SendStatus == "True" ? 1 : SendStatus == "False" ? 0 : ""
        };
        // 重新查詢並更新 gridData
        await select(updatedQueryCondition, false);
        setSelectedPlanKind(PlanKind);
    };

    /**
     * 退回計畫資料
     * @returns
     */
    const back = async () => {
        let result = await ReturnProject(selectResultData);
        if (result.success) {
            showGlobalMessageBox(result.message, () => window.location.reload());
        }
    }

    /**
     * 共用的 onChange 方法
     * @param {*} fieldName 
     * @param {*} value 
     */
    const handleOnChange = (fieldName, value) => {
        const newValues = { ...formData, [fieldName]: value };
        setFormData(newValues); 
    };

    /**
     * 共用的 onBlur 方法
     * @param {*} fieldName 
     * @param {*} value 
     */
    const handleOnBlur = (fieldName, value) => {
        const newValues = { ...formData, [fieldName]: value.trim() };
        setFormData(newValues);
    };
    

    return (
        <>
            <h3 className="k-dialog-titlebar">先期計畫登入</h3>
            <div style={{ 'display': 'flex' }}>
                <OrgSelectPanel
                    data={treeData}
                    onSelect={onSelect}
                    title={'主管機關'}
                    style={{ overflow: "auto" }}
                />
                <PageContainer
                    toolbar={
                        <>
                            {selectedPlanKind === '1' && (
                                <Button title="退回重大施政" className="k-button-lighten" onClick={() => back()}>退回重大施政</Button>
                            )}
                            {selectedPlanKind === '2' && (
                                <Button title="退回委託研究" className="k-button-lighten" onClick={() => back()}>退回委託研究</Button>
                            )}
                        </>
                    }
                    style={{ height: "calc(100vh - 129px)" }}
                >
                    <Formik
                        initialValues={formData}
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
                                setFieldValue,
                                setValues
                            } = props;
                            return (
                                <form>
                                    <table>

                                        <tr>
                                            <th>
                                                年度
                                            </th>
                                            <td>
                                                <DropDownListWithValue
                                                    name="PLANYEAR"
                                                    data={yearOptions}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.PLANYEAR}
                                                    onChange={(e) => { handleOnChange("PLANYEAR", e.target.value) }}
                                                />
                                            </td>
                                            <th>
                                                編號
                                            </th>
                                            <td>
                                                <TextInput
                                                    name="PLANNO"
                                                    value={values.PLANNO}
                                                    onChange={handleChange}
                                                    onBlur={(e) => handleOnBlur("PLANNO", e.target.value)}
                                                    style={{ width: "50%" }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                審查狀態
                                            </th>
                                            <td>
                                                <DropDownListWithValue
                                                    name="SEND_STATUS"
                                                    data={statusOptions}
                                                    textField={"SET_VALUE"}
                                                    dataItemKey={"SET_TYPE"}
                                                    value={values.SEND_STATUS}
                                                    onChange={(e) => { handleOnChange("SEND_STATUS",  e.target.value)}}
                                                />
                                            </td>
                                            <th>
                                                計畫名稱
                                            </th>
                                            <td>
                                                <TextInput
                                                    name="PLANNAME"
                                                    value={values.PLANNAME}
                                                    onChange={handleChange}
                                                    onBlur={(e) => handleOnBlur("PLANNAME", e.target.value)}
                                                    style={{ width: "50%" }}
                                                />
                                            </td>
                                        </tr>
                                    </table>
                                </form>
                            )
                        }}
                    </Formik>
                    <ReviewProjectGrid
                        selectResultData={selectResultData}
                        setSelectResultData={setSelectResultData}
                        funRole={props.funRole}
                    />
                </PageContainer>
            </div>
        </>
    );
}
export default ReviewProjectMain;
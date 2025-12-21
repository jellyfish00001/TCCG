import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import OrgSelectPanel from '../../../Components/Selector/OrgSelectPanel';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Formik } from 'formik';
import { AdjustNumberGrid } from './AdjustNumberGrid';
import { SetMaskOnOff, IsNullOrEmpty } from "../../../Basic/SDOExtension";
import { GetProjectList, SetAdjustNumber, loadDropdownData, validateFormData } from "./AdjustNumberService";
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { GetBasicData } from "../../../Basic/BasicData";

const AdjustNumberMain = (props) => {
    // 這裡假設grid資料從外部加載
    const {  } = props;
    // grid資料
    const [selectResultData, setSelectResultData] = useState([]);
    // 下拉選單年度
    const [yearOptions, setYearOptions] = useState([]);
    // 下拉選單計畫類別
    const [planTypeOptions, setPlanTypeOptions] = useState([]);
    // 下拉選單計畫類別
    const [stateOptions, setStateOptions] = useState([]);
    // 下拉選單基金來源
    const [fundOptions, setFundOptions] = useState([]);
    // 下拉選單經費來源
    const [fundFromOptions, setFundFromOptions] = useState([]);
    // 組織樹資料
    const [treeData, setTreeData] = useState([]);
    // 表單資料
    const [formData, setFormData] = useState({
        PLANYEAR: (new Date().getFullYear() - 1911 + 1).toString(),
        PLANKIND: "1",
        IS_SEND: "",
        FUNDNO: null,
        BUDGETTYPE: "1",
        AdjustNumberModels: []
    });
    const formRef = useRef(null);
    /**
     * 初始化頁面
     * @returns
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        // 畫面資料
        const dropdownData = await loadDropdownData();
        // 年度
        setYearOptions(dropdownData.yearOptions);
        // 計畫類別
        setPlanTypeOptions(dropdownData.planTypeOptions);
        // 計畫狀態
        setStateOptions(dropdownData.stateOptions);
        // 經費來源
        setFundFromOptions(dropdownData.fundFromOptions);
        // 基金名稱
        setFundOptions(dropdownData.fundOptions);
        SetMaskOnOff(false);
    };

    //  初始化頁面
    useEffect(() => {
        loadData();
    }, [])

    // 取基金
    const getFund = async (data) => {
        // 查詢
        let OrgId = await GetBasicData('orgId')
        data = { ...data, OU_ID: OrgId };
        let result = await GetProjectList(data);
        // 取得基金下拉選單
        // 篩選掉重複的FUNDNO
        let distinctFunds = result.filter((value, index, self) =>
            index === self.findIndex((t) => (
            t.FUNDNO === value.FUNDNO
            ))
        );
        // 去掉FUNDNAME為null的值
        let updatedFunds = distinctFunds.filter(fund => fund.FUNDNAME !== null);
        // 取得基金下拉選單
        let optionData = updatedFunds.map(fund => ({
            value: fund.FUNDNO,
            text: fund.FUNDNAME
        }));
        optionData.unshift({ value: null, text: "請選擇" });
        setFundOptions(optionData);
    }

    // 重新查詢
    useEffect(() => {
        setTreeData([{ OU_ID: "", title: `桃園市政府(0)` }]);
        setSelectResultData([]);
        select(formData);
    }, [formData]);

    /**
     * 產生組織樹資料
     * @param {*} data 
     * @returns 
     */
    const genOrgTreeData = (data) => {
        // 取得計劃清單機關清單
        let projOrgPanelDataList = data.filter(proj => !IsNullOrEmpty(proj.CREATEORGOUID))
            .map(proj => {
                let projCnt = data.filter(x => x.CREATEORGOUID === proj.CREATEORGOUID).length;
                return {
                    OU_ID: proj.CREATEORGOUID,
                    PLANKIND: proj.PLANKIND,
                    title: proj.ORGOUNAME + '(' + projCnt + ')',
                }
            });
        // 去除重複的組織數據
        let distinctData = projOrgPanelDataList.filter((value, index, self) =>
            index === self.findIndex((t) => (
                t.OU_ID === value.OU_ID
            ))
        );
        // 在清單前面新增一個總項
        distinctData.unshift({ OU_ID: "", title: `桃園市政府(${data.length})` });
        return distinctData;
    };

    /**
     * 查詢
     * @param {*} data 
     * @param {*} isResetOrgTree 組織數查詢不更新組織數
     * @param {*} source 選擇基金來源不更新grid
     * @returns
     */
    const select = async (data, isResetOrgTree = true) => {
        if(data.BUDGETTYPE == "1")
        {
            data = { ...data, FUNDNO: null}
        }
        // 驗證
        try {
            await validateFormData(data);
          } catch (error) {
            return;
          }
        // 只能看自己機關的資料
        let OrgId = await GetBasicData('orgId')
        data = { ...data, OU_ID: OrgId };
        // 查詢
        let result = await GetProjectList(data);
        // 產生組織樹資料
        if(isResetOrgTree){  
            let treeData = genOrgTreeData(result);
            setTreeData(treeData);
        }
        // 更新gridData
        setSelectResultData(result);
    }

    /**
     * 存檔
     * @param {*} sendType 
     * @returns
     */
    const saveChange = async (sendType) => {
        SetMaskOnOff(true);
        // 篩選有編輯的資料
        let gridChange = selectResultData.filter(item => item.editType === 2);
        // 將PLANORDERNUMBER = ""的值改為null
        gridChange = gridChange.map(item => {
            if (item.PLANORDERNUMBER === "") {
                return { ...item, PLANORDERNUMBER: null };
            }
            return item;
        });
        // 只能看自己機關的資料
        let OrgId = await GetBasicData('orgId')
        // 跟條件資料一起放入model
        const saveData = {
            ...formData,
            OU_ID: OrgId,
            SaveType: sendType ,
            AdjustNumberModels: gridChange
            };
        // 存檔
        let saveResult = await SetAdjustNumber(saveData);
        SetMaskOnOff(false);
        // 成功訊息
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => select(formData));
        }
    }

    /**
     * 共用的下拉 onChange 方法
     * @param {*} fieldName 
     * @param {*} value 
     * @returns
     */
    const handleOnChange = (fieldName, value, values) => {
        const newValues = { ...values, [fieldName]: value };
        if(fieldName == "BUDGETTYPE"){
            getFund(newValues);
        }
        setFormData(newValues);
    };

    /**
     * 組織樹查詢
     * @param {*} ouId 
     * @returns
     */
    const onSelect = async (data) => {
        let CreateOrgOuId = data.OU_ID;
        // 根據所選組織ID更新查詢條件
        let updatedQueryCondition = {
            ...formRef.current.values,
            CREATEORGOUID: CreateOrgOuId
        };
        // 重新查詢並更新 gridData
        await select(updatedQueryCondition, false);
    };

    return (
        <>
            <h3 className="k-dialog-titlebar">調整優先順序</h3>
            <div style={{ 'display': 'flex' }}>
            <OrgSelectPanel
                    data={treeData}
                    onSelect={onSelect}
                    title={'提報機關'}
                    style={{ overflow: "auto" }}
                />
                <PageContainer
                    toolbar={
                        <>
                            {formData.PLANKIND === "1" && (
                                <Button className="k-button-lighten" onClick={() => saveChange("sendReview")}>重大施政送審</Button>
                            )}
                            {formData.PLANKIND === "2" && (
                                <Button className="k-button-lighten" onClick={() => saveChange("sendReview")}>委託研究送審</Button>
                            )}
                            <Button className="k-button-lighten" onClick={() => saveChange("save")}>存檔</Button>
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
                            } = props;
                            return (
                                <form>
                                    <table>

                                        <tr>
                                            <th className="addRedStar">
                                                年度
                                            </th>
                                            <td>
                                                <DropDownListWithValue
                                                    name="PLANYEAR"
                                                    data={yearOptions}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.PLANYEAR}
                                                    onChange={(e) => handleOnChange("PLANYEAR", e.target.value, values)}
                                                />
                                            </td>
                                            <th className="addRedStar">
                                                計畫類別
                                            </th>
                                            <td>
                                                <DropDownListWithValue
                                                    name="PLANKIND"
                                                    data={planTypeOptions}
                                                    textField={"SET_VALUE"}
                                                    dataItemKey={"SET_TYPE"}
                                                    value={values.PLANKIND}
                                                    onChange={(e) => {setFormData({ ...values, PLANKIND: e.target.value, BUDGETTYPE: "1", FUNDNO: null})}}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th >
                                                狀態
                                            </th>
                                            <td colspan={3}>
                                                <DropDownListWithValue
                                                    name="IS_SEND"
                                                    data={stateOptions}
                                                    textField={"SET_VALUE"}
                                                    dataItemKey={"SET_TYPE"}
                                                    value={values.IS_SEND}
                                                    onChange={(e) => handleOnChange("IS_SEND", e.target.value, values)}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                經費來源
                                            </th>
                                            { values.BUDGETTYPE == "2" ? (
                                                <>
                                                    <td>
                                                        <DropDownListWithValue
                                                            name="BUDGETTYPE"
                                                            data={fundFromOptions}
                                                            textField={"SET_VALUE"}
                                                            dataItemKey={"SET_TYPE"}
                                                            value={values.BUDGETTYPE}
                                                            onChange={(e) => {handleOnChange("BUDGETTYPE", e.target.value, values)}}
                                                        />
                                                    </td>
                                                    <th className="addRedStar">
                                                        基金名稱
                                                    </th>
                                                    <td>
                                                        <DropDownListWithValue
                                                            name="FUNDNO"
                                                            data={fundOptions}
                                                            textField={"text"}
                                                            dataItemKey={"value"}
                                                            value={values.FUNDNO}
                                                            onChange={(e) => {handleOnChange("FUNDNO", e.target.value, values)}} 
                                                        />
                                                    </td>
                                                </>
                                            ) : (
                                                <td colspan={3}>
                                                    <DropDownListWithValue
                                                        name="BUDGETTYPE"
                                                        data={fundFromOptions}
                                                        textField={"SET_VALUE"}
                                                        dataItemKey={"SET_TYPE"}
                                                        value={values.BUDGETTYPE}
                                                        onChange={(e) => { handleOnChange("BUDGETTYPE", e.target.value, values)}}
                                                    />
                                                </td>
                                            )}
                                        </tr>
                                    </table>
                                </form>
                            )
                        }}
                    </Formik>
                        <AdjustNumberGrid
                            selectResultData={selectResultData}
                            setSelectResultData={setSelectResultData}
                        />
                </PageContainer>
            </div>
        </>
    );
}
export default AdjustNumberMain;
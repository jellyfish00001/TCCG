import React, { useState, useRef, useEffect } from "react";
import { Formik, Form, Field } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { FormatDate } from "../../../Basic/SDOExtension";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import TextInput from '../../../Components/Input/TextInput';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import { ProjectBasicGrid } from "./ProjectBasicGrid"

/**
 * 研究發展作業系統-委託研究計劃
 * 計畫基本資料
 * PPT P11 P12 
 * 計畫登入"編修"帶資料跳入此頁面
 * Form + Grid + 年月下拉選單 資料寫死測試
 */

const ProjectBasicMain = () => {
    // 這裡假設grid資料從外部加載
    const [data, setData] = useState([]);
    //Grid資料
    const [gridData, setGridData] = useState([]);
    //表單資料
    const formRef = useRef();
    //表單資料
    const [formData, setFormData] = useState({
        PROJECT_NO: '110-0001',
        PROJECT_KEY: '經濟部',
        startYear: '112',
        startMonth: '01',
        endYear: '112',
        endMonth: '01',
        bidYear: '112',
        bidMonth: '01',
        cenYear: '112',
        cenMonth: '01',
        lastYear: '112',
        lastMonth: '01', 
    });
    // 年度下拉選單
    const yearOptions = [
        { text: '110年', value: '110' },
        { text: '111年', value: '111' },
        { text: '112年', value: '112' }
    ];
    // 月份下拉選單
    const monthOptions = [
        { text: '1月', value: '01' },
        { text: '2月', value: '02' },
        { text: '12月', value: '12' }
    ];

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    // 存檔
    const saveChanges = async (data) => {

    }

    // 表單取消
    const cancel = (values) => {

    };

    // 表單送出審核
    const check = (values) => {

    };  

    // 表單解除鎖定
    const onlock = (values) => {

    };  

    // 表單檔案上傳
    const upload = (values) => {

    };
    
    // 通用的下拉選單 onChange 處理函數
    const handleDropdownChange = (name, value) => {
        setFormData(prevValues => ({
            ...prevValues,[name]: value
        }));
    };
    
    //欄位標題調整
    const Th = ({ children, ...props }) => (
        <th colSpan="2" {...props}>
          {children}
        </th>
    );
    //欄位內容調整
    const Td = ({ children, ...props }) => (
        <td colSpan="3" {...props}>
          {children}
        </td>
    );
      
        return (
            <PageContainer
            style={{ overflow: "auto", height: "100%" }}
            >
                <CollapseBoardCard
                button={
                    <>
                        <Button title="存檔" onClick={handleSubmit}>存檔</Button>
                        <Button title="存檔" className="k-button-lighten" onClick={onlock}>解除鎖定</Button>
                        <Button title="送出審核" className="k-button-lighten" onClick={check}>送出審核</Button>
                        <Button title="取消" className="k-button-lighten" onClick={cancel}>取消</Button>
                    </>
                }
                title="計畫基本資料"
                isFirstArea={true}
                >     
                    <Formik
                        initialValues={{...formData}}
                        onSubmit={(data) => saveChanges(data)}
                        enableReinitialize={true}
                        innerRef={formRef}
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
                                            <Th>
                                                計畫編號
                                            </Th>
                                            <Td colSpan={3}>
                                                {values.PROJECT_NO}
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th className="addRedStar">
                                                計畫名稱
                                            </Th>
                                            <Td >
                                                <TextInput
                                                    name="PLAN_NAME"
                                                    value={values.PLAN_NAME}
                                                    onChange={handleChange}
                                                    style={{ width: "50%" }}
                                                />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th>
                                                委託機關
                                            </Th>
                                            <Td>
                                                {values.PROJECT_KEY}
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th className="addRedStar">
                                                受託單位
                                            </Th>
                                            <Td>
                                                <TextInput
                                                    name="REQUEST_UNIT"
                                                    value={values.REQUEST_UNIT}
                                                    error={errors.REQUEST_UNIT}
                                                    onChange={handleChange}
                                                    style={{ width: "50%" }}
                                                />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th className="addRedStar">
                                                研究主持人
                                            </Th>
                                            <Td>
                                                <TextInput
                                                    name="REA_MASTER"
                                                    value={values.REA_MASTER}
                                                    error={errors.REA_MASTER}
                                                    onChange={handleChange}
                                                />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <th rowspan="5" className="addRedStar">
                                                研究期程
                                            </th>
                                        </tr>
                                        <tr>
                                            <th style={{'borderTop':'none'}}>
                                                計畫期間
                                            </th>
                                            <Td>
                                            <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="startYear"
                                                textField="text"
                                                dataItemKey="value"
                                                data={yearOptions}
                                                value={values.startYear}
                                                onChange={(e) => handleDropdownChange('startYear', e.target.value)}
                                                error={errors.startYear}
                                            />
                                            <span style={{ margin: "5px" }}>年</span>
                                            <DropDownListWithValue
                                                name="startMonth"
                                                textField="text"
                                                dataItemKey="value"
                                                data={monthOptions}
                                                value={values.startMonth}
                                                onChange={(e) => handleDropdownChange('startMonth', e.target.value)}
                                                error={errors.startMonth}
                                            />
                                            <span style={{ margin: "5px" }}>月 ~ </span>
                                            <DropDownListWithValue
                                                name="endYear"
                                                textField="text"
                                                dataItemKey="value"
                                                data={yearOptions}
                                                value={values.endYear}
                                                onChange={(e) => handleDropdownChange('endYear', e.target.value)}
                                                error={errors.endYear}
                                            />
                                            <span style={{ margin: "5px" }}>年</span>
                                            <DropDownListWithValue
                                                name="endMonth"
                                                textField="text"
                                                dataItemKey="value"
                                                data={monthOptions}
                                                value={values.endMonth}
                                                onChange={(e) => handleDropdownChange('endMonth', e.target.value)}
                                                error={errors.endMonth}
                                            />
                                            <span style={{ margin: "5px" }}>月</span>
                                            </div>
                                            </Td>
                                        </tr>
                                        <tr>
                                            <th>
                                                決標
                                            </th>
                                            <Td>
                                            <div style={{ display: "flex", alignItems: "center" }}>
                                                <DropDownListWithValue
                                                    name="bidYear"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={yearOptions}
                                                    value={values.bidYear}
                                                    onChange={(e) => handleDropdownChange('bidYear', e.target.value)}
                                                    error={errors.bidYear}
                                                />
                                                <span style={{ margin: "5px" }}>年</span>
                                                <DropDownListWithValue
                                                    name="bidMonth"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={monthOptions}
                                                    value={values.bidMonth}
                                                    onChange={(e) => handleDropdownChange('bidMonth', e.target.value)}
                                                    error={errors.bidMonth}
                                                />
                                                <span style={{ margin: "5px" }}>月</span>
                                            </div>
                                            </Td>
                                        </tr>
                                        <tr>
                                            <th>
                                                期中報告
                                            </th>
                                            <Td>
                                            <div style={{ display: "flex", alignItems: "center" }}>
                                                <DropDownListWithValue
                                                    name="cenYear"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={yearOptions}
                                                    value={values.cenYear}
                                                    onChange={(e) => handleDropdownChange('cenYear', e.target.value)}
                                                    error={errors.cenYear}
                                                />
                                                <span style={{ margin: "5px" }}>年</span>
                                                <DropDownListWithValue
                                                    name="cenMonth"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={monthOptions}
                                                    value={values.cenMonth}
                                                    onChange={(e) => handleDropdownChange('cenMonth', e.target.value)}
                                                    error={errors.cenMonth}
                                                />
                                                <span style={{ margin: "5px" }}>月</span>
                                            </div>
                                            </Td>
                                        </tr>
                                        <tr>
                                            <th>
                                                期末報告
                                            </th>
                                            <Td>
                                            <div style={{ display: "flex", alignItems: "center" }}>
                                                <DropDownListWithValue
                                                    name="lastYear"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={yearOptions}
                                                    value={values.lastYear}
                                                    onChange={(e) => handleDropdownChange('lastYear', e.target.value)}
                                                    error={errors.lastYear}
                                                />
                                                <span style={{ margin: "5px" }}>年</span>
                                                <DropDownListWithValue
                                                    name="lastMonth"
                                                    textField="text"
                                                    dataItemKey="value"
                                                    data={monthOptions}
                                                    value={values.lastMonth}
                                                    onChange={(e) => handleDropdownChange('lastMonth', e.target.value)}
                                                    error={errors.lastMonth}
                                                />
                                                <span style={{ margin: "5px" }}>月</span>
                                            </div>
                                            </Td>
                                        </tr>
                                        <tr>
                                            <th rowspan="5" className="addRedStar">
                                                經費來源
                                            </th>
                                        </tr>
                                        <tr>
                                            <th style={{'borderTop':'none'}}>
                                                公務預算
                                            </th>
                                            <Td >
                                                <TextInput
                                                    name="PUB_BUDGET"
                                                    value={values.PUB_BUDGET}
                                                    onChange={handleChange}
                                                    error={errors.PUB_BUDGET}
                                                />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <th>
                                                基金預算
                                            </th>
                                            <Td>
                                                <TextInput
                                                    name="FUND_BUDGET"
                                                    value={values.FUND_BUDGET}
                                                    onChange={handleChange}
                                                    error={errors.FUND_BUDGET}
                                                />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <th>
                                                中央預算
                                            </th>
                                            <Td >
                                                <TextInput
                                                    name="CEN_BUDGET"
                                                    value={values.CEN_BUDGET}
                                                    onChange={handleChange}
                                                    error={errors.CEN_BUDGET}
                                                    />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <th>
                                            其他
                                            </th>
                                                <td width="350px">
                                                <TextInput
                                                    name="OTHER"
                                                    value={values.OTHER}
                                                    onChange={handleChange}
                                                    error={errors.OTHER}
                                                    style={{ width: "100%" }}
                                                />
                                                </td>
                                            <th >
                                            來源說明
                                            </th>
                                                <Td >
                                                <TextInput
                                                    name="SORUCE"
                                                    value={values.SORUCE}
                                                    onChange={handleChange}
                                                    error={errors.SORUCE}
                                                    style={{ width: "100%" }}
                                                />
                                                </Td>
                                        </tr>
                                        <tr>
                                            <Th className="addRedStar">
                                            每季評核指標
                                            </Th>
                                            <Td >
                                            <ProjectBasicGrid/>
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th className="addRedStar">
                                            研究員因及目的
                                            </Th>
                                            <Td>
                                            <PureHtmlTextAreaInput
                                                rows={5}
                                                name='RES_PURPOSE'
                                                value={values.RES_PURPOSE}
                                                onChange={handleChange}
                                                error={errors.RES_PURPOSE}
                                                style={{ width: "100%" }}
                                            />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th className="addRedStar">
                                            計畫項目內容
                                            </Th>
                                            <Td>
                                            <PureHtmlTextAreaInput
                                                rows={5}
                                                name='PLAN_CONTENT'
                                                value={values.PLAN_CONTENT}
                                                onChange={handleChange}
                                                error={errors.PLAN_CONTENT}
                                                style={{ width: "100%" }}
                                            />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th className="addRedStar">
                                            預期研究成果
                                            </Th>
                                            <Td>
                                            <PureHtmlTextAreaInput
                                                rows={5}
                                                name='PLAN_RESULT'
                                                value={values.PLAN_RESULT}
                                                onChange={handleChange}
                                                error={errors.PLAN_RESULT}
                                                style={{ width: "100%" }}
                                            />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th className="addRedStar">
                                            承辦人
                                            </Th>
                                            <Td>
                                            <TextInput
                                                name="UNDERTAKER"
                                                value={values.UNDERTAKER}
                                                onChange={handleChange}
                                                error={errors.UNDERTAKER}
                                            />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th className="addRedStar">
                                            電話/分機
                                            </Th>
                                            <Td >
                                            <TextInput
                                                name="PHONE"
                                                value={values.PHONE}
                                                onChange={handleChange}
                                                error={errors.PHONE}
                                            />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th className="addRedStar">
                                            承辦人Email
                                            </Th>
                                            <Td >
                                            <TextInput
                                                name="UNDERTAKER_EMAIL"
                                                value={values.UNDERTAKER_EMAIL}
                                                onChange={handleChange}
                                                error={errors.UNDERTAKER_EMAIL}
                                                style={{ width: "50%" }}
                                            />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th>
                                            代理人Eamil
                                            </Th>
                                            <Td >
                                            <TextInput
                                                name="ANGENT_EMAIL"
                                                value={values.ANGENT_EMAIL}
                                                error={errors.ANGENT_EMAIL}
                                                onChange={handleChange}
                                                style={{ width: "50%" }}
                                            />
                                            </Td>
                                        </tr>
                                        <tr>
                                            <Th>
                                            附件上傳
                                            </Th>
                                            <Td >
                                            <Button title="檔案上傳" onClick={upload}>檔案上傳</Button>
                                            </Td>
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
        export default ProjectBasicMain;


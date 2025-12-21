import React, { useState, useEffect, useRef } from 'react';
import Table from '../../../Css/custom/Table.module.css';
import { Formik } from "formik";
import RadioBoxList from '../../../Components/Input/RadioBoxList';
import TextInput from '../../../Components/Input/TextInput';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import { Button } from '@progress/kendo-react-buttons';
import PCCBasicDataWindow from './Window/PCCBasicDataWindow';
import PCCProgressWindow from './Window/PCCProgressWindow';
import PCCDs07Window from './Window/PCCDs07Window';
import PCCDs09Window from './Window/PCCDs09Window';

export const ProjectFillContactForm = (props) => {

    let { data, contactDataChange, isAssociate, isEngineering, isRdecFun } = props

    // Form 資料
    const [formData, setFormData] = useState({});
    const formRef = useRef("");
    // 異動指標 (formik 刷新用)
    const loadMark = useRef(true);
    // 標案系統基本資料 visibility
    const [isPCCBasicDataWindowVisible, setIsPCCBasicDataWindowVisible] = useState(false);
    // 標案系統執行進度 visibility
    const [isPCCPrgressDataWindowVidible, setIsPCCProgressDataWindowVisible] = useState(false);
    // 工程標案工程概要資料 visibility
    const [isPCCDs07Vidible, setIsPCCDs07Vidible] = useState(false);
    // 工程標案決標資料 visibility
    const [isPCCDs09Vidible, setIsPCCDs09Vidible] = useState(false);

    // 輸入框onBlur事件
    const inputBlur = async (formData, errors) => {
        if (Object.keys(errors).length === 0) {
            setFormData({ ...formData })
        }
        contactDataChange({ ...formData }, errors);
    }

    //選擇是否關聯工程會標案
    const handleIsTycgProjectChange = (e, setValues, values) => {
        // 更新 Formik 的狀態
        setValues({ ...values, IS_TYCG_PROJECT: e.value });
        // 呼叫父元件的函數傳遞更新後的值
        contactDataChange({ ...values, IS_TYCG_PROJECT: e.value });
    };

    useEffect(() => {
        if (data) {
            // 設定異動指標，讓Fomik reload Form
            setFormData({ ...data, loadMark: loadMark.current })
            loadMark.current = !loadMark.current;
        }

    }, [data])

    return (

        <CollapseBoardCard title='聯繫資訊' isFirstArea={true}>
            <Formik
                initialValues={formData}
                enableReinitialize={true}
                innerRef={formRef}
            >
                {props => {
                    const {
                        setValues,
                        handleChange,
                        values,
                        errors,
                    } = props;
                    return (
                        <form >
                            <table className={Table.fullWidth}>
                                <tbody>
                                    <tr>
                                        <th className='addRedStar'>計畫實際承辦人</th>
                                        <td>
                                            <TextInput
                                                onChange={handleChange}
                                                value={values.REAL_CONTACT}
                                                name="REAL_CONTACT"
                                                onBlur={(e) => {
                                                    inputBlur({ ...values, REAL_CONTACT: e.target.value }, errors);
                                                }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className='addRedStar'>電話</th>
                                        <td>
                                            <TextInput
                                                onChange={handleChange}
                                                value={values.REAL_TEL}
                                                name="REAL_TEL"
                                                error={errors.REAL_TEL}
                                                onBlur={(e) => {
                                                    inputBlur({ ...values, REAL_TEL: e.target.value }, errors);
                                                }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <CommonTooltip
                                                title={"信箱"}
                                                content={"多筆請用半形;區分"}
                                            />
                                        </th>
                                        <td>
                                            <TextInput
                                                onChange={handleChange}
                                                value={values.REAL_EMAIL}
                                                name="REAL_EMAIL"
                                                onBlur={(e) => {
                                                    inputBlur({ ...values, REAL_EMAIL: e.target.value }, errors);
                                                }}
                                                error={errors.REAL_EMAIL}
                                                style={{ width: '100%', 'min-width': '200px' }}
                                            />
                                        </td>
                                    </tr>
                                    {/* 執行方式為工程類才需顯示 */}
                                    {isEngineering &&
                                        <>
                                            <tr style={{ 'height': '35px' }}>
                                                {/* 需關聯工程會才能顯示按鈕 */}
                                                {isAssociate &&
                                                    <td colSpan={2}>
                                                        <div className='fn-buttons' style={{ display: 'flex', marginLeft: '5px' }}>
                                                            <Button className='k-button k-button-lighten' type='button' onClick={() => { setIsPCCBasicDataWindowVisible(true) }}>標案系統基本資料</Button>
                                                            <Button className='k-button k-button-lighten' type='button' onClick={() => { setIsPCCProgressDataWindowVisible(true) }}>標案系統執行進度</Button>

                                                            <Button className='k-button k-button-lighten' type='button' onClick={() => { setIsPCCDs07Vidible(true) }}>工程標案工程概要資料</Button>
                                                            <Button className='k-button k-button-lighten' type='button' onClick={() => { setIsPCCDs09Vidible(true) }}>工程標案決標資料</Button>
                                                        </div>
                                                    </td>
                                                }
                                            </tr>
                                            <tr>
                                                <th>工程會標案編號</th>
                                                <td>{values.PCC_PROJECT_NO}</td>
                                            </tr>
                                            <tr>
                                                <th>標案承辦人</th>
                                                <td>{values.FACTORY_CONTACT}</td>
                                            </tr>
                                            <tr>
                                                <th>電話</th>
                                                <td>{values.FACTORY_TEL}</td>
                                            </tr>
                                            <tr>
                                                <th>以界接資料填報</th>
                                                <td>
                                                    <RadioBoxList
                                                        group='IS_USER_FTY_DATA'
                                                        valueField='value'
                                                        textField='text'
                                                        data={[

                                                            {
                                                                text: '否', value: false,
                                                                checked: values.IS_USER_FTY_DATA === null
                                                                    || values.IS_USER_FTY_DATA === false,
                                                                disabled: true
                                                            },
                                                            {
                                                                text: '是(不需要重大填報，但需要在每月5日前至工程會標案管理系統完成辦理情形填報)', value: true,
                                                                checked: values.IS_USER_FTY_DATA !== null
                                                                    && values.IS_USER_FTY_DATA === true,
                                                                disabled: true
                                                            }
                                                        ]}
                                                    />
                                                </td>
                                            </tr>
                                        </>
                                    }
                                    <tr>
                                        <th>本府執行案件</th>
                                        <td>
                                            <RadioBoxList
                                                group='IS_TYCG_PROJECT'
                                                valueField='value'
                                                textField='text'
                                                data={[
                                                    {
                                                        text: '否', value: false,
                                                        // 如果values.IS_TYCG_PROJECT為false，則選取此選項
                                                        checked: values.IS_TYCG_PROJECT === false,
                                                        disabled: !isRdecFun
                                                    },
                                                    {
                                                        text: '是', value: true,
                                                        // 如果values.IS_TYCG_PROJECT為true，則選取此選項
                                                        checked: values.IS_TYCG_PROJECT === true,
                                                        disabled: !isRdecFun
                                                    }
                                                ]}
                                                onChange={(e) => handleIsTycgProjectChange(e, setValues, values)}
                                            />
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </form>
                    )
                }}
            </Formik>
            <PCCBasicDataWindow
                visible={isPCCBasicDataWindowVisible}
                onClose={() => { setIsPCCBasicDataWindowVisible(false) }}
                pccProjectUid={data.PCC_PROJECT_UID}
            />

            <PCCProgressWindow
                visible={isPCCPrgressDataWindowVidible}
                onClose={() => { setIsPCCProgressDataWindowVisible(false) }}
                pccProjectUid={data.PCC_PROJECT_UID}
            />

            <PCCDs07Window
                visible={isPCCDs07Vidible}
                onClose={() => { setIsPCCDs07Vidible(false) }}
                pccProjectUid={data.PCC_PROJECT_UID}
            />

            <PCCDs09Window
                visible={isPCCDs09Vidible}
                onClose={() => { setIsPCCDs09Vidible(false) }}
                pccProjectUid={data.PCC_PROJECT_UID}
            />
        </CollapseBoardCard >
    );
}
export default ProjectFillContactForm
import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { PageContainer } from '../../../Basic/PageContainer';
import TextInput from '../../../Components/Input/TextInput';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { EditorTools } from '@progress/kendo-react-editor';
import TextEditor from '../../../Components/Editor/TextEditor';
import { Tooltip } from "@progress/kendo-react-tooltip";
import { Formik } from 'formik';
import MailSetService from './MailSetService';
import { IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';

const {
    Bold, Italic, Underline, Strikethrough,
    ForeColor,
    AlignLeft, AlignCenter, AlignRight, AlignJustify,
    Indent, Outdent, OrderedList, UnorderedList,
} = EditorTools;

const MailSetWindow = (props) => {
    const { windowStatus, loadData } = props;
    console.log(windowStatus);
    console.log(loadData);
    const initData = {
        MAIL_ID: '',
        MAIL_NAME: '',
        MAIL_SUBJECT: '',
        MAIL_CONTENT: '',
        MAIL_MEMO: '',
        DEL_FLG: null,
    }
    const [mailSet, setMailSet] = React.useState(initData);

    // ToolTip
    const firstElement = React.useRef(null);
    const [targetElement, setTargetElement] = React.useState(null);
    const [onoff, setonoff] = React.useState(false);

    // 取得郵件範本by ID
    const getMailById = async (mailId) => {
        let result = JSON.parse(JSON.stringify(initData));
        result = await MailSetService.getMailTemplateById(mailId);
        setMailSet(result);
    }

    React.useEffect(() => {
        if (windowStatus.visible && !IsNullOrEmpty(windowStatus.mailId)) {
            getMailById(windowStatus.mailId);
        }
        else if (!windowStatus.visible) {
            setMailSet(initData);
        }
    }, [windowStatus])

    // ToolTip顯示、隱藏設定
    const showTooltip = (element) => {
        if (onoff) {
            setonoff(false);
            setTargetElement(element.current);
        }
        else {
            setonoff(true);
            setTargetElement(element.current);
        }
    };

    // 提示文字
    const TooltipContentText = () => {
        return <div style={{ marginTop: '5%' }}>
            可自訂郵件主旨及內文的自訂變數項目如下：<br />
            <table >
                <tr>
                    <td style={{ width: "150px" }}>$計畫編號$</td>
                    <td>$計畫名稱$</td>
                </tr>
                <tr>
                    <td>$執行機關$</td>
                    <td></td>
                </tr>
                <tr>
                    <td>$執行機關承辦人$</td>
                    <td>$執行機關承辦人電話$</td>
                </tr>
                <tr>
                    <td>$姓名$</td>
                    <td>$管考電話$</td>
                </tr>
                <tr>
                    <td>$計畫實際承辦人$</td>
                    <td>$計畫實際承辦人電話$</td>
                </tr>
                <tr>
                    <td>$填報月份$</td>
                    <td>$檢核點$</td>
                </tr>
                <tr>
                    <td>$預定完成日期$</td>
                    <td>$落後情形$</td>
                </tr>
                <tr>
                    <td>$管考意見$</td>
                    <td>$管考備註$</td>
                </tr>
            </table>
        </div>
    };

    // 存檔
    const submit = async (data) => {
        SetMaskOnOff(true);
        let saveResult = await MailSetService.saveMailTemplate(data);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                loadData();
            });
        }
        else {
            showGlobalMessageBox(saveResult.message);
        }
    }

    return (
        <Formik
            initialValues={mailSet}
            validationSchema={MailSetService.validateField}
            onSubmit={(data) => submit(data)}
            //允許重複賦予初始值
            enableReinitialize
        >
            {props => {
                const {
                    values,
                    errors,
                    handleBlur,
                    handleSubmit,
                    handleReset,
                    handleChange,
                    setValues
                } = props;
                return (
                    <form onSubmit={handleSubmit} onReset={handleReset}>
                        <PageContainer>
                            <div className="fn-buttons" style={{ display: "flex", alignItems: "center" }}>
                                <Button type="submit">存檔</Button>
                                <Button type="reset" className="fn-buttons k-button-lighten">重設</Button>
                                <Button type="button" iconClass='fa-exclamation-circle_Yellow'
                                    look='default' onClick={() => showTooltip(firstElement)} />
                                <Tooltip
                                    anchorElement="target"
                                    open={onoff}
                                    targetElement={targetElement}
                                    openDelay={1}
                                    position="right"
                                    content={() => <TooltipContentText />}
                                >
                                    <span
                                        ref={firstElement}
                                        title=" "
                                    />
                                </Tooltip>
                            </div>

                            <table>
                                <tbody>
                                    <tr>
                                        <th>停用</th>
                                        <td>
                                            <DropDownListWithValue
                                                data={[{ Value: true, Text: "是" }, { Value: false, Text: "否" }]}
                                                textField={"Text"}
                                                dataItemKey={"Value"}
                                                value={values.DEL_FLG}
                                                onChange={(e) => {
                                                    setValues({ ...values, DEL_FLG: e.target.value });
                                                }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">範本名稱</th>
                                        <td>
                                            <TextInput
                                                name="MAIL_NAME"
                                                maxlength={50}
                                                style={{ width: '100%' }}
                                                value={values.MAIL_NAME}
                                                error={errors.MAIL_NAME}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">郵件主旨</th>
                                        <td>
                                            <TextInput
                                                name="MAIL_SUBJECT"
                                                maxlength={100}
                                                style={{ width: '100%' }}
                                                value={values.MAIL_SUBJECT}
                                                error={errors.MAIL_SUBJECT}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">郵件內容</th>
                                        <td>
                                            <TextEditor
                                                value={values.MAIL_CONTENT}
                                                onChange={(e) => setValues({ ...values, MAIL_CONTENT: e.html })}
                                                error={errors.MAIL_CONTENT}
                                                tools={[
                                                    [Bold, Italic, Underline, Strikethrough],
                                                    ForeColor,
                                                    [AlignLeft, AlignCenter, AlignRight, AlignJustify],
                                                    [Indent, Outdent],
                                                    [OrderedList, UnorderedList]
                                                ]}
                                                clearHtml={true} //貼上時會清除HTML標籤
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>收件者</th>
                                        <td>
                                            <TextAreaWrapInput value={values.MAIL_MEMO} />
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </PageContainer>
                    </form>
                )
            }}
        </Formik>
    )
}

export default MailSetWindow;
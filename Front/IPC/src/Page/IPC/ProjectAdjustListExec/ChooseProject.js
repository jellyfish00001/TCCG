import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { DropDownWithFilter } from '../../../Components/Dropdowns/DropDownWithFilter';
import { getProjectNameDropDown, AddAdujustExec, openAdjustProjectChapter } from './ProjectAdjustListExecService';
import { Formik } from "formik";
import * as Yup from 'yup';

const ChooseProject = (props) => {
    const { closeWindow, awKind, loadListData } = props;

    const [ddlData, setDdlData] = React.useState([]);

    const initData = {
        PROJECT_NO: "", // 計畫列管編號
        PROJECT_NAME: "", // 計畫名稱
        AW_KIND: awKind.SET_TYPE, // 申請項目
    };

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = async () => {
        const data = await getProjectNameDropDown("");
        setDdlData(data);
    }

    // 存檔/送出
    const save = async (data) => {
        showGlobalConfirmBox("請確認計畫名稱是否正確，送出後即進入調整狀態。", async () => {
            SetMaskOnOff(true);
            let projAdjId = await AddAdujustExec(data);
            SetMaskOnOff(false);

            if (projAdjId > 0) {
                closeWindow();
                // 開啟新分頁
                // AW_KIND = 'AW01' 開啟基本資料調整 /ProjectAdjustBasic/BasicExecReason
                // AW_KIND = 'AW02' 開啟期程調整 /ProjectAdjustSchedule/ScheduleExecReason
                let params = {
                    funRole: 0, // 依主辦面向
                    AW_KIND: data.AW_KIND,
                    PROJECT_NO: data.PROJECT_NO,
                    PROJ_ADJ_ID: projAdjId,
                    PROJECT_NAME: data.PROJECT_NAME,
                }
                openAdjustProjectChapter(params);
            } else {
                showGlobalMessageBox("此計畫目前正在調整中，請待目前調整審核結束後再次申請");
            }

            loadListData();
        });
    }

    // 欄位驗證
    const validateField = Yup.object().shape({
        PROJECT_NO: Yup.string().nullable().required("此欄位為必填"),
    });

    return (
        <>
            <Formik
                initialValues={initData}
                validationSchema={validateField}
                onSubmit={(data) => save(data)}
                enableReinitialize //允許重複賦予初始值
            >
                {prop => {
                    const {
                        values,
                        errors,
                        handleBlur,
                        handleSubmit,
                        handleChange,
                        setValues
                    } = prop;
                    return (
                        <form onSubmit={handleSubmit}>
                            <div className="fn-buttons">
                                <Button type="submit">確認申請</Button>
                            </div>
                            <table>
                                <tbody>
                                    <tr>
                                        <th>申請項目</th>
                                        <td>
                                            {awKind.SET_VALUE}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">計畫名稱</th>
                                        <td>
                                            <DropDownWithFilter
                                                ddlData={ddlData}
                                                style={{ width: '100%' }}
                                                textField={"PROJECT_NAME"}
                                                keyField={"PROJECT_NO"}
                                                typingLength={1}
                                                onChange={(e) => {
                                                    setValues({ ...values, PROJECT_NO: e.PROJECT_NO, PROJECT_NAME: e.PROJECT_NAME });
                                                }}
                                                error={errors.PROJECT_NO}
                                            />
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </>
    )
}

export default ChooseProject;
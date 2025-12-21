import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { SetMaskOnOff, IsNullOrEmpty } from "../../../Basic/SDOExtension";
import { GetSetParam } from '../../../Basic/CommonService';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import RadioBoxList from '../../../Components/Input/RadioBoxList';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import { Pageable } from '../../../Basic/BasicData';
import { Formik} from 'formik';
import { orderBy } from "@progress/kendo-data-query";

import { SaveRDAudit, GetRDAuditList, validationSchema } from "../ProjectAudit/ProjectAuditService"
/**
 * 研究發展作業系統-委託研究計畫-審核作業
 */
const ProjectAuditMain = (props) => {
    const {
        location: {
            state: {
                PLAN_NO,
                funRole,
            }
        }
    } = props;
    // grid data
    const [gridData, setGridData] = useState([])
    // grid 分頁
    const [paging, setPaging] = React.useState({ skip: 0, take: 10 });
    // grid 排序
    const [sort, setSort] = React.useState([{ field: "", dir: "" }]);
    // 表單異動資料
    const formRef = useRef();
    // 表單資料
    const [formData, setFormData] = useState({});
    // 審查結果
    const [reviewResultStatus, setReviewResultType] = useState([]);
    /**
     * 頁面載入 grid data 資料
     */
    const loadGridData = async () => {
        SetMaskOnOff(true);
        // 取得審核紀錄清單
        let data = await GetRDAuditList(PLAN_NO);
        setGridData(data)
        
        let reviewResultTypeList = await GetSetParam('AUDIT_RESULT_STATUS', "");
        setReviewResultType(reviewResultTypeList);

        SetMaskOnOff(false);
    }
    useEffect(() => {
        loadGridData();
    },[])
    /**
     * 存檔送出，利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
     */
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.validateForm().then(errors => {
                // 檢查表單是否有錯誤
                if (Object.keys(errors).length === 0) {
                    formRef.current.handleSubmit(); 
                }
            });
        }
    };
    /**
     * 表單送出審核
     */
    const save = () => {
        showGlobalConfirmBox("請確認是否執行動作", () => {
            // 目前表單資料
            let data = formRef.current.values;
            // 沒有審查編號代表沒選資料，不可送出
            if(!IsNullOrEmpty(data.AUDIT_ID)){
                saveChangesEvent(formRef.current.values)
            }else{
                showGlobalMessageBox("未選擇審查資料");
            }
            }
        )
    };
    /**
     * 送審/確認送出
     * @param {*} data
     */
    const audit = async (data) => {
        showGlobalConfirmBox("請確認是否執行動作", () => {
            // 沒有審查編號代表沒選資料，不可送出
            if(!IsNullOrEmpty(data.AUDIT_ID)){
                saveChangesEvent(data, 1)
            }else{
                showGlobalMessageBox("未選擇審查資料");
            }
        }
        )
    }
    /**
     * 存檔事件
     * @param {*} data
     * @param {*} isSend 是否送出，【存檔】預設 IS_SEND 為 0，【確認送出】IS_SEND 為 1
     */
    const saveChangesEvent = async (data, isSend = 0) => {
        SetMaskOnOff(true);
        let item = {
            ...data,
            IS_SEND: isSend,
            PLAN_NO: PLAN_NO
        }
        let result = await SaveRDAudit(item)
        SetMaskOnOff(false);
        if(result != null){
            showGlobalMessageBox(result.message, () => {
                window.location.reload(); // 畫面 reload
            })
        }
    }
    /**
     * 表單取消
     */
    const cancel = () => {
        SetMaskOnOff(true);
        if (formRef.current) {
            // 重置表單為初始值
            formRef.current.resetForm({ values: formData });
        }
        SetMaskOnOff(false);
    };
    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };
    /**
     * 編輯按鈕 Cell
     * @param {*} props
     * @return {*} 
     */
    const editCell = (props) => {
        return (
            <CommandCell>
                {
                    /* 有管考(2)權限以及存檔而已尚未確認送出(0)，才可以使用編輯功能 */
                    (funRole === 2 && props.dataItem.IS_SEND === 0)
                    ?
                    <Button title={"編輯"} icon='edit' look='default' onClick={ () => {
                        SetMaskOnOff(true);
                        setFormData(props.dataItem);
                        SetMaskOnOff(false);
                    }}
                    />
                    :
                    null
                }
            </CommandCell>
        )
    }
    return (
        <PageContainer
            style={{ overflow: "auto", height: "100%" }}
        >
            <CollapseBoardCard
                button={
                    <>
                        <Button title="存檔" className="k-button-lighten" onClick={save}>存檔</Button>
                        <Button title="確認送出" className="k-button-lighten" onClick={handleSubmit}>確認送出</Button>
                        <Button title="取消" className="k-button-lighten" onClick={cancel}>取消</Button>
                    </>
                }
                title="審查作業"
                isFirstArea={true}
            >
                <Formik
                    initialValues={formData}
                    validationSchema={validationSchema}
                    onSubmit={(data) => audit(data)}
                    enableReinitialize={true}
                    innerRef={formRef}
                    validateOnChange={false} //字段改變不驗證
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
                                            審查編號
                                        </th>
                                        <td>
                                            {values.AUDIT_ID}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            審查意見
                                        </th>
                                        <td>
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='REVIEW_COMMENTS'
                                                value={values.REVIEW_COMMENTS}
                                                onChange={handleChange}
                                                error={errors.REVIEW_COMMENTS}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            審查結果
                                        </th>
                                        <td>
                                            <RadioBoxList
                                                group='REVIEW_RESULT'
                                                valueField='SET_TYPE'
                                                textField='SET_VALUE'
                                                data={reviewResultStatus.map(item => ({
                                                    ...item,
                                                    checked: values.REVIEW_RESULT === item.SET_TYPE
                                                }))}
                                                onChange={(e) => setValues({ ...values, REVIEW_RESULT: e.value })}
                                                error={errors.REVIEW_RESULT}
                                            />
                                        </td>
                                    </tr>
                                </table>
                            </form>
                        )
                    }}
                </Formik>
            <Grid
                style={{ height: '100%', overflow: 'auto'}}
                resizable={true}
                data={gridData.slice(paging.skip, paging.take + paging.skip)}
                total={gridData.length}
                sort={sort}
                onSortChange={sortChange}
                sortable={{ allowUnsort: true, mode: "single" }}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take})}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn title='編輯' cell={editCell} width="50px" />
                <GridColumn field="AUDIT_ID" title="審查編號" width="110px" />
                <GridColumn field="PLAN_REVIEW" title="審查類別" width="160px" />
                <GridColumn field="REVIEW_RESULT_STATUS" title="審查結果" width="100px" />
                <GridColumn field="REVIEW_COMMENTS" title="審查意見" />
            </Grid>
            </CollapseBoardCard>
        </PageContainer>
    )
    }
    export default ProjectAuditMain;


        
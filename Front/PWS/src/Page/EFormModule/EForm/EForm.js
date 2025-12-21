//@ts-check
import React, { useState, useEffect, useContext } from 'react';
import { Pageable } from '../../../Basic/BasicData';
import FormService from '../EForm/eForm.service'
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { IsNullOrEmpty, FormatDate } from '../../../Basic/SDOExtension'
import { AddMdf, SendFlow } from './EForm-AddMdf/EFormAddMdf'
import { PageContainer } from '../../../Basic/PageContainer';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import FlowDetail from '../../FlowModule/FlowDetail/FlowDetail';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';

const Query = () => {
    const [slideVisible, setSlideVisible] = useState(true)
    const [paging, setPaging] = useState({ skip: 0, take: 10 })
    const [formData, setFormData] = useState([])//表單資料
    //ddlData:表單下拉選單data，ddlValue:下拉選單目前選到的value(同時也是查詢條件)，預設下拉選單data的第一筆
    const [formDataForDDL, setFormDataForDDL] = useState({ ddlData: [], ddlValue: "" })

    const [isAddMdf, setIsAddMdf] = useState({
        visible: false,
        dataItem: {
            FORM_ID: "",
            FILL_ID: ""
        },
        title: ""
    })
    const [isSendFlow, setIsSendFlow] = useState({ visible: false, fillId: "" })
    const [flowCode, setFlowCode] = useState("")
    const [flowDetailVisible, setFlowDetailVisible] = useState(false);

    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        //取得表單下拉選單data
        getFormDataForDDL()
    }, []);

    useEffect(() => {
        //取得表單清單
        getForm()
    }, [formDataForDDL.ddlData]);

    //取得表單下拉選單data
    const getFormDataForDDL = async () => {
        let data = await FormService.getFormDataForDDL()
        setFormDataForDDL({
            ...formDataForDDL,
            ddlData: data,
            ddlValue: IsNullOrEmpty(data) ? "" : data[0].FORM_ID
        })
    }

    //取得表單清單
    const getForm = async () => {
        if (IsNullOrEmpty(formDataForDDL.ddlValue))
            return;
        setFormData(await FormService.getForm(formDataForDDL.ddlValue))
    }

    const isDelte = (fillId) => {
        showConfirmBox("進行刪除作業，確定嗎？", () => { deleteForm(fillId) })
    }

    // 刪除表單
    const deleteForm = async (fillId) => {
        let response = await FormService.deleteForm(fillId)
        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    getForm()
                }
            }
        })
    }

    const queryConditions = (
        <table style={{ width: '600px' }}   >
            <tbody>
                <tr>
                    <th style={{ textAlign: 'right' }}>表單</th>
                    <td>
                        <DropDownListWithValue
                            name="formId"
                            data={formDataForDDL.ddlData}
                            textField="FORM_NAME" //表單名稱  
                            dataItemKey="FORM_ID" //表單代碼
                            value={formDataForDDL.ddlValue}
                            onChange={(e) => setFormDataForDDL({ ...formDataForDDL, ddlValue: e.target.value })}
                        />
                    </td>
                </tr>
            </tbody>
        </table>
    );

    const toolbar = [
        <Button icon="menu" onClick={() => setSlideVisible(!slideVisible)}></Button>,
        <Button onClick={getForm}>查詢</Button>,
        <Button onClick={() => setIsAddMdf({
            visible: true, dataItem: {
                FORM_ID: "",
                FILL_ID: ""
            }, title: "新增"
        })}>新增</Button>
    ]
    return (
        <PageContainer
            toolbar={toolbar}
        >
            {slideVisible && queryConditions}
            <Grid
                style={{
                    height: '100%',
                    overflow: 'auto'
                }}
                data={formData.slice(paging.skip, paging.take + paging.skip)}
                total={formData.length}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
            >
                <GridNoRecords> </GridNoRecords>
                <GridColumn field="NO" title="No" width={30} />
                <GridColumn width={130} cell={(props) =>
                    <td>
                        <Button icon="delete" look="bare" onClick={() => isDelte(props.dataItem.FILL_ID)} />
                        <Button icon="edit" look="bare" onClick={() => setIsAddMdf({ visible: true, dataItem: props.dataItem, title: "修改" })} />
                        {(!(props.dataItem.FLOW_CODE.length > 0 && props.dataItem.FLOW_EXECUTE)) &&
                            <Button icon="track-changes-accept" look="bare" onClick={() => setIsSendFlow({ visible: true, fillId: props.dataItem.FILL_ID })} />}
                        {(props.dataItem.FLOW_CODE.length > 0) && < Button icon="grid-layout" look="bare" onClick={() => { setFlowCode(props.dataItem.FLOW_CODE); setFlowDetailVisible(true); }} />}
                    </td>
                } />
                <GridColumn field="FORM_TYPENAME" title="表單類型" />
                <GridColumn field="FILL_ID" title="表單代碼" />
                <GridColumn title="填寫時間" cell={(props) =>
                    <td>
                        {FormatDate(props.dataItem.MDF_DATE, "tYY/MM/DD HH:mm:ss")}
                    </td>
                } />
            </Grid>
            {/* 新增or修改視窗 */}
            {isAddMdf.visible && <AddMdf
                title={isAddMdf.title}
                closeWindow={() => setIsAddMdf({
                    visible: false,
                    dataItem: {
                        FORM_ID: "",
                        FILL_ID: ""
                    }, title: ""
                })}
                formId={isAddMdf.dataItem.FORM_ID}
                fillId={isAddMdf.dataItem.FILL_ID}
                formIdForDdl={formDataForDDL.ddlValue}
                refreshGrid={getForm}
            />}

            {/* 送出流程 */}
            {isSendFlow.visible && <SendFlow
                closeWindow={() => setIsSendFlow({ visible: false, fillId: "" })}
                fillId={isSendFlow.fillId}
                refreshGrid={getForm} />}

            {/* 流程關卡 */}
            {flowDetailVisible && <FlowDetail
                flowCode={flowCode}
                isAdmin={false}
                onClose={() => { setFlowCode(""); setFlowDetailVisible(false); }}
            />}
        </PageContainer>
    );
}

export default Query;

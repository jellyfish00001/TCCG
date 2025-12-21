import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Pageable } from '../../../Basic/BasicData';
import FormSetService from '../EFormSet/eFormSet.service'
import { GetOrgValue, FormatDate } from '../../../Basic/SDOExtension'
import TwDatePicker from '../../../Components/DateInputs/TwDatePicker'
import { PageContainer } from '../../../Basic/PageContainer';
import AddMdf from './EFormSet-AddMdf/EFormSetAddMdf'
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import OrgSelect from '../../../Components/Selector/Orgselector'
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';

const Query = () => {
    const [slideVisible, setSlideVisible] = useState(true)
    const [paging, setPaging] = useState({ skip: 0, take: 10 })
    const [formSetData, setFormSetData] = useState([])
    const [formTypeData, setFormTypeData] = useState([])//表單類型下拉選單data
    const [filterField, setFilterField] = useState({
        //查詢條件
        FORM_TYPE: "",
        EFFECTIVE_DATE: new Date(),
        MAP_ORG: []
    })
    const [isAddMdf, setIsAddMdf] = useState({ visible: false, formId: "", title: "" })

    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        //取得表單類型下拉選單的data
        getFormType()
        //取得表單設定清單
        getFormSet()
    }, []);

    //取得表單類型下拉選單的data
    const getFormType = async () => {
        setFormTypeData(await FormSetService.getFormType("全部"))
    }

    //取得表單設定清單
    const getFormSet = async () => {
        let data = { ...filterField }
        //取得Org object value
        data.MAP_ORG = GetOrgValue(data.MAP_ORG)
        data.EFFECTIVE_DATE = FormatDate(data.EFFECTIVE_DATE, 'YYYY-MM-DD')
        setFormSetData(await FormSetService.getFormSet(data))
    }

    const isDelte = (formId) => {
        showConfirmBox("進行刪除作業，確定嗎？", () => { deleteFormSet(formId) })
    }

    //刪除表單設定
    const deleteFormSet = async (formId) => {
        let response = await FormSetService.deleteFormSet(formId)
        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    getFormSet()
                }
            }
        })
    }

    const queryConditions = (
        <table style={{ width: '600px' }}>
            <tbody>
                <tr>
                    <th>表單類型</th>
                    <td>
                        <DropDownListWithValue
                            name="FORM_TYPE"
                            data={formTypeData}
                            textField="SET_VALUE" //表單類型名稱
                            dataItemKey="SET_TYPE" //表單類型代碼
                            value={filterField.FORM_TYPE}
                            onChange={(e) => setFilterField({ ...filterField, FORM_TYPE: e.target.value })}
                        />
                    </td>
                    <th>有效日期</th>
                    <td>
                        <TwDatePicker
                            format={"yyy/MM/dd"}
                            onChange={(e) => {
                                setFilterField({ ...filterField, EFFECTIVE_DATE: e.target.value })
                            }}
                            value={filterField.EFFECTIVE_DATE}
                        />
                    </td>
                </tr>
                <tr>
                    <th>表單應用單位</th>
                    <td colSpan={2}>
                        <OrgSelect
                            name="MAP_ORG"
                            multiple={true}  //多選
                            value={filterField.MAP_ORG}
                            setValue={(data) => setFilterField({ ...filterField, MAP_ORG: data })} // data 為object ，再"送出"的時候再取得value
                        />
                    </td>
                </tr>
            </tbody>
        </table>
    );

    const toolbar = [
        <Button icon="menu" onClick={() => setSlideVisible(!slideVisible)} ></Button>,
        <Button onClick={getFormSet}>查詢</Button>,
        <Button onClick={() => setIsAddMdf({ visible: true, formId: "", title: "新增" })}>新增</Button>
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
                data={formSetData.slice(paging.skip, paging.take + paging.skip)}
                total={formSetData.length}
                skip={paging.skip}
                take={paging.take}
                pageable={Pageable}
                onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
            >
                <GridNoRecords> </GridNoRecords>
                <GridColumn field="NO" title="No" width={30} />
                <GridColumn width={70} cell={(props) =>
                    <td>
                        <Button icon="edit" look="bare" title={"修改"} onClick={() => setIsAddMdf({ visible: true, formId: props.dataItem.FORM_ID, title: "修改" })} />
                        <Button icon="close" look="bare" title={"刪除"} onClick={() => isDelte(props.dataItem.FORM_ID)} />
                    </td>} />
                <GridColumn field="FORM_TYPE_NAME" title="表單類型" />
                <GridColumn field="FORM_NAME" title="表單名稱" />
                <GridColumn field="EFFECTIVE_DATE" title="有效日期" cell={(props) =>
                    <td>
                        {FormatDate(props.dataItem.EFFECTIVE_DATE)}~
                        {FormatDate(props.dataItem.EXPIRE_DATE)}
                    </td>} />
            </Grid>
            {/* 新增or修改視窗 */}
            {isAddMdf.visible && <AddMdf
                title={isAddMdf.title}
                closeWindow={() => setIsAddMdf({ visible: false, formId: "", title: "" })}
                formId={isAddMdf.formId}
                refreshGrid={getFormSet}
            />}
        </PageContainer>
    );
}

export default Query;
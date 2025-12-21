import React, { useState, useEffect } from "react";
import { DropDownListCell } from '../../../Components/GridCell/DropDownListCell';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { handleEditedGridData, SetMaskOnOff, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { GetSetParam } from '../../../Basic/CommonService';
import { Error } from '@progress/kendo-react-labels';
import '../../../Css/custom/Utils.module.css';
/** Cell **/
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { TextAreaCell } from "../../../Components/GridCell/TextAreaCell";
import { TwDatePickerCell } from "../../../Components/GridCell/TwDatePickerCell";
/**********/
import { validataPolicyField } from "./ProjectBasicService";

/**
 * 研究發展作業系統-委託研究計劃-評核指標 多Grid
 * [章節]基本資料、[章節]展延申請 共用畫面此 Grid
 */
export const ProjectBasicGrid = (props) => {
    // 評核指標資料、有異動的grid資料reference、是否是展延申請、評核指標是否要顯示必填訊息
    const { policyIndex, editedGridData, isExtension, isPolicyIndexError } = props;
    // 評核指標識別欄位 IDENTITY_FIELD，辨識是新增的資料還是原有 load 進來的資料
    const [identityField, setIdentityField] = useState("");
    // grid data
    const [gridData, setGridData] = useState([]);
    // 下拉選單
    const [ddlPolicyKind, setDDLPolicyKind] = useState();
    // 觸發Grid驗證
    const [triggerValidation, setTriggerValidation] = React.useState(true);

    /**
     * 載入評核指標資料
     */
    const loadPolicyIndexData = async () => {
        SetMaskOnOff(true);
        // 清空 Grid
        setGridData([])
        
        // 取得評核類別下拉選單
        let policyKindDropDownList = await GetSetParam("POLICY_KIND"); 
        policyKindDropDownList.unshift({SET_VALUE:"請選擇", SET_TYPE:""});
        setDDLPolicyKind([...policyKindDropDownList]);

        // 如果目前是展延申請章節使用此 Grid
        if(isExtension){
            // 設辨識欄位為 POLICY_INDEX_ADJ_ID
            setIdentityField("POLICY_INDEX_ADJ_ID");
            // 展延申請有調整預定完成期程，如果調整預定完成期程是空的，給預設值是預定完成期程
            for(let i = 0; i < policyIndex.length; i++){
                if(IsNullOrEmpty(policyIndex[i].POLICY_EXTP_LANEND_DATE)){
                    policyIndex[i].POLICY_EXTP_LANEND_DATE = policyIndex[i].RES_FINISH_DATE
                }
            }
        }
        // 如果目前是基本資料章節使用此 Grid
        else{
            // 設辨識欄位為 SEQ
            setIdentityField("SEQ");
        }
        setGridData(policyIndex); 
        SetMaskOnOff(false);
    }

    useEffect(() => {
            // 清空
            editedGridData.current = []
            if(policyIndex){
                loadPolicyIndexData();
            }
    }, [policyIndex])

    /**
     * 畫面新增一筆評核指標
     */
    const addNew = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;
        // 新增一筆時的預設值
        let newData = {
            hiddenIndex: LasthiddenIndex + 1,
            // IDENTITY_FIELD: 0,                         // 辨識是新增的資料還是原有 load 進來的資料
            editType: 1,                              // 1:新增 2:編輯 3:刪除
            POLICY_INDEX_ADJ_ID: 0,                  // 評核指標調整表序號，展延申請章節的 IDENTITY_FIELD
            SEQ: 0,                                 // 流水號，基本資料章節的 IDENTITY_FIELD
            POLICY_KIND: '',                       // 評核類別
            POLICY_INDEX_DESC: '',                // 評核項目
            RES_FINISH_DATE: new Date(),         // 預期完成期程
            POLICY_EXTP_LANEND_DATE: new Date() // 調整預定完成期程  
        }
        setGridData([...gridData, newData]);
        handleEditedGridData(editedGridData, newData, 'hiddenIndex', identityField);
    }

    /**
     * 移除指定Grid資料行
     * @param {*} dataItem
     */
    const remove = (dataItem) => {
        dataItem.editType = 3
        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', identityField);
        // 過濾掉此筆資料
        const filteredData = gridData.filter(item => item !== dataItem);
        // 塞回去 Grid
        setGridData(filteredData);
    };
    /**
     * 刪除欄位
     *
     * @param {*} props
     * @return {*} 
     */
    const DelCommandCell = (props) => {
        return (
            <CommandCell>
                <Button title={"刪除"} icon='close' look='default' onClick={() => { remove(props.dataItem); }}/>
            </CommandCell>
        );
    };
    /**
     * 供可切換欄位的 onChange or onBlur callback的事件
     * @param {object} item
     */
    const callBackEvent = (item) => {
        handleEditedGridData(editedGridData, item, 'hiddenIndex', identityField);
    }
    /**
     * grid 用 TextArea
     * @param {*} props
     * @return {*} 
     */
    const textAreaCell = (props) => {
        return (
            <TextAreaCell
                {...props}
                editable={true}
                IsAlwaysEdit={true}
                required={true}
                maxlength={100}
                rows={1}
                onCellInputBlur={callBackEvent}
                customValidate={{ scheme: validataPolicyField, triggerValidate: triggerValidation }}
            />
        );
    };
    /**
     * grid 用 TwDatePicker
     * @param {*} propsGrid Grid Cell props
     * @param {*} itemKey Name
     * @return {*} 
     */
    const twDatePickerCell = (props, itemKey) => {
        return (
            <TwDatePickerCell
                name = {itemKey}
                format="yyy/MM/dd"
                required={true}
                dataItem={props.dataItem}
                field={itemKey}
                AlwaysEdit={true}
                // newDatas={true}
                onCellInputChange={callBackEvent}
                value={ new Date(props.dataItem.RES_FINISH_DATE) }
                customValidate={{ scheme: validataPolicyField, triggerValidate: triggerValidation }}
            />
        );
    };
    /**
     * grid 共用下拉選單
     * @param {object} props Grid Cell props
     * @param {object} options DDL 選項
     * @param {string} itemKey Name
     * @return {*} 
     */
    const dropDownListCell = (props, options, itemKey) => {
        return (
            <DropDownListCell
                {...props}
                name={itemKey}
                editable={true}
                AlwaysEdit={true}
                required={true}
                ddlData={options}
                textField="SET_VALUE"
                dataItemKey="SET_TYPE"
                onCellDDLChange={callBackEvent}
                customValidate={{ scheme: validataPolicyField, triggerValidate: triggerValidation }}
            />
        );
    };
    return (
        <>
            <Button type='button' title="新增" onClick={addNew} >新增</Button>
            <Grid
                style={{height: '100%',overflow: 'auto',}}
                resizable={true}
                data={gridData}
            >
                <GridNoRecords>
                    {
                        isPolicyIndexError
                        ?
                        <span style={{color:"red", fontSize: "12px", fontStyle: "italic"}}>
                            評核指標必填
                        </span>
                        :
                        <>無資料</>
                    }
                </GridNoRecords>
                <GridColumn field="DELETE" title="刪除" cell={DelCommandCell} width="50px" />
                <GridColumn field="POLICY_KIND" title="評核類別" cell={props => dropDownListCell(props, ddlPolicyKind, "POLICY_KIND")} />
                <GridColumn field="POLICY_INDEX_DESC" title="評核項目" cell={textAreaCell} />
                <GridColumn field="RES_FINISH_DATE" title="預定完成期程" cell={props => twDatePickerCell(props, "RES_FINISH_DATE")} />
                { // 是展延申請才顯示此欄位
                isExtension&&
                <GridColumn field="POLICY_EXTP_LANEND_DATE" title="調整預定完成期程" cell={props => twDatePickerCell(props, "POLICY_EXTP_LANEND_DATE")} />
                }
            </Grid>
        </>
    )
}
export default ProjectBasicGrid;
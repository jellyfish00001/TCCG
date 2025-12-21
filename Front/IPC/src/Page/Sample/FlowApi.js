import React, { useState } from 'react';
import { TreeView } from '@progress/kendo-react-treeview';
import { Button } from '@progress/kendo-react-buttons';
import { Slide } from '@progress/kendo-react-animation';

const FlowApi = () => {
    //Treeview 資料
    const item = [
        {
            text: "SQL Table",
            hasSub: true,
            items: [
                { text: "FLOW_SET - 流程主檔" },
                { text: "FLOW_PREARRANGE" },
                { text: "FLOW_ROLE" },
                { text: "FLOW_SETDETAIL" },
                { text: "FLOW_SIGNEDLOG" },
                { text: "SUBFLOW_PREARRANGE" },
            ]
        },
        {
            text: "SQL View",
            hasSub: true,
            items: [
                { text: "V_FLOW_ActiveList" },
                { text: "V_SUBFLOW_List" },
                { text: "V_USER_FLOWROLE" }
            ]
        },
        {
            text: "SQL Stored Procedure",
            hasSub: true,
            items: [
                { text: "usp_FLOW_GetDetail" },
                { text: "usp_FLOW_SetAccept" },
                { text: "usp_FLOW_SetActive" },
                { text: "usp_FLOW_SetDeactive" },
                { text: "usp_FLOW_SetReject" },
                { text: "usp_SUBFLOW_GetDetail" },
                { text: "usp_SUBFLOW_SetAccept" },
                { text: "usp_SUBFLOW_SetActive" },
                { text: "usp_SUBFLOW_SetDeactive" },
            ]
        },
        {
            text: "SQL Function",
            hasSub: true,
            items: [
                { text: "fn_GetFlowMapRole" },
                { text: "fn_Split" },
                { text: "fn_ChkFlowExecute" },
                { text: "fn_ChkSubFlowExecute" },
                { text: "fn_GetAgentDisplay" },
                { text: "fn_GetFlowMapUser" },
                { text: "fn_GetSetParam" },
                { text: "fn_GetUsersOrgId" },
                { text: "fn_HasSubFlow" },
                { text: "fn_HasSubFlowExecute" },
            ]
        },
    ]

    //TreeView Css自訂
    const customItemRender = props => {
        if (props.itemHierarchicalIndex.length <= 1)
            return <h4 key="title">{props.item.text}</h4>
        else
            return <h5 key="title">{props.item.text}</h5>
    }

    //開關組織樹
    const expandChangeHandler = (event) => {
        event.item.expanded = !event.item.expanded;
    }

    //hook控制treeView開關
    const [treeVisible, setTreeVisible] = useState(true);

    return (
        <div>
            <div className="fn-buttons">
                <Button icon="menu" onClick={() => { setTreeVisible(!treeVisible) }}></Button>
            </div>
            {/* 滑出式查詢條 */}
            <Slide
                style={{width: '100%'}}
                //動畫長短設定
                transitionExitDuration={500}
                transitionEnterDuration={500}
            >
                {
                    treeVisible &&
                    <div style={{
                        height: '200px',
                        overflow: 'auto',
                    }}>
                        <TreeView
                            data={item}
                            //顯示文字
                            textField="text"
                            //顯示subTree
                            hasChildrenField="hasSub"
                            //展開狀態
                            expandField="expanded"
                            //展開icon顯示
                            expandIcons={true}
                            //顯示狀態
                            onExpandChange={expandChangeHandler} //展開
                            itemRender={customItemRender}
                        >
                        </TreeView>
                    </div>
                }
            </Slide>
            <div >
                <h3>啟動流程</h3>
                <h4>(啟動主流程) FlowSet.Flow().SetActive(strFlowId, strSenderUserId)</h4>
                <pre>
                    <i>
                        {`
    strFlowId => 流程設定代碼
    strSenderUserId => 使用者代碼
    return {
        SUCCESS: {true|false},
        MESSAGE: {執行訊息},
        RESULT: [{has_signed,has_subflow,flow_code,sort_order,base_odr,status,status_name,org_id,org_name,role_id,role_name,user_id,user_name,user_email,memo,mdf_date]
    }
`}
                    </i>
                </pre>
                <h4>(啟動分會流程) FlowSet.Flow().SetSubActive(strFlowId, strSenderUserId, strFlowUserId, strFlowCode)</h4>
                <pre>
                    <i>
                        {`
    strFlowId => 流程設定代碼
    strSenderUserId => 使用者代碼
    strFlowUserId => 分會使用者代碼
    strFlowCode => 主流程代碼
    return {
        SUCCESS: {true|false},
        MESSAGE: {執行訊息},
        RESULT: [{has_signed,flow_code,sort_order,status,status_name,org_id,org_name,role_id,role_name,user_id,user_name,user_email,memo,mdf_date}]
    }
`}
                    </i>
                </pre>
                <h3>取得執行流程</h3>
                <h4>(取得執行中主流程) FlowSet.Log().GetFlowList()</h4>
                <pre>
                    <i>
                        {`
    return [{flow_code,has_subflow,status,status_name,org_id,org_name,role_id,role_name,user_id,user_name,user_email,flow_decision,flow_option,mdf_date,mdf_user_id,mdf_user_name}, ...]
`}
                    </i>
                </pre>
                <h4>(取得指定使用者待簽核流程代碼)  FlowSet.Log().GetUserList(string strUserId)</h4>
                <pre>
                    <i>
                        {`
    strUserId => 使用者代碼
    return [{flow_code, sub_flow_code, is_subflow}, ...]
`}
                    </i>
                </pre>
                <h3>取得流程執行歷程</h3>
                <h4>(取得主流程歷程) FlowSet.Log().GetFlowDetail(strFlowCode, onlyActLev = false)</h4>
                <pre>
                    <i>
                        {`
    strFlowCode => 主流程執行代碼,
    onlyActLev => 只回傳簽核中關卡
    return [{has_signed,has_subflow,flow_code,sort_order,base_odr,status,status_name,org_id,org_name,role_id,role_name,user_id,user_name,user_email,memo,mdf_date}, ...]
`}
                    </i>
                </pre>

                <h4>(取得流程關卡對應分會流程代碼) FlowSet.Log().GetSubflowCodeList(string strFlowCode, int intSignedOdr)</h4>
                <pre>
                    <i>
                        {`
    strFlowCode => 主流程執行代碼,
    intSignedOdr => 流程關卡順序
    return [{sub_flow_code}, ...]
`}
                    </i>
                </pre>
                <h4>(取得分會流程歷程) FlowSet.Log().GetSubFlowDetail(strSubFlowCode, onlyActLev = false)</h4>
                <pre>
                    <i>
                        {`
    strSubFlowCode => 分會流程執行代碼,
    onlyActLev => 只回傳簽核中關卡
    return [{has_signed,flow_code,sort_order,status,status_name,org_id,org_name,role_id,role_name,user_id,user_name,user_email,memo,mdf_date}, ...]
`}
                    </i>
                </pre>
                <h3>設定流程通過或是退回</h3>
                <h4>(主流程通過關卡) FlowSet.Flow().SetAccept(strFlowCode, strSenderUserId, strSignedMemo, bolSignedDecision = false)</h4>
                <pre>
                    <i>
                        {`
    strFlowCode => 主流程執行代碼,
    strSenderUserId => 使用者代碼,
    strSignedMemo => 執行註記,
    bolSignedDecision => {true|false 決行通過全關}
    return {
        SUCCESS: {true|false},
        MESSAGE: {執行訊息},
        RESULT: [{has_signed,has_subflow,flow_code,sort_order,base_odr,status,status_name,org_id,org_name,role_id,role_name,user_id,user_name,user_email,memo,mdf_date}]
    }
`}
                    </i>
                </pre>
                <h4>(主流程退回關卡) FlowSet.Flow().SetReject(strFlowCode, strSenderUserId, strSignedMemo = "", bolRefuseAll = false)</h4>
                <pre>
                    <i>
                        {`
    strFlowCode => 主流程執行代碼,
    strSenderUserId => 使用者代碼,
    strSignedMemo => 執行註記,
    bolrefuseAll => {true|false 退回到第一關}
    return {
        SUCCESS: {true|false},
        MESSAGE: {執行訊息},
        RESULT: [{has_signed,has_subflow,flow_code,sort_order,base_odr,status,status_name,org_id,org_name,role_id,role_name,user_id,user_name,user_email,memo,mdf_date}]
    }
`}
                    </i>
                </pre>
                <h4>(分會流程通過關卡) FlowSet.Flow().SetSubAccept(strSubFlowCode, strSenderUserId, strSignedMemo)</h4>
                <pre>
                    <i>
                        {`
    strSubFlowCode => 分會流程執行代碼,
    strSenderUserId => 使用者代碼,
    strSignedMemo => 執行註記,
    return {
        SUCCESS: {true|false},
        MESSAGE: {執行訊息},
        RESULT: [{has_signed,flow_code,sort_order,status,status_name,org_id,org_name,role_id,role_name,user_id,user_name,user_email,memo,mdf_date}]
    }
`}
                    </i>
                </pre>
                <h3>關閉流程</h3>
                <h4>(關閉主流程及所有分會流程) FlowSet.Flow().SetDeactive(strFlowCode, strSenderUserId, strSignedMemo = "")</h4>
                <pre>
                    <i>
                        {`
    strFlowCode => 主流程執行代碼,
    strSenderUserId: 使用者代碼,
    strSignedMemo => 執行註記,
    return {
        SUCCESS: {true|false},
        MESSAGE: {執行訊息},
        RESULT: ""
    }
`}
                    </i>
                </pre>
                <h4>(關閉分會流程) FlowSet.Flow().SetSubDeactive(strSubFlowCode, strSenderUserId, strSignedMemo = "")</h4>
                <pre>
                    <i>
                        {`
    strSubFlowCode => 分會流程執行代碼,
    strSenderUserId: 使用者代碼,
    strSignedMemo => 執行註記,
    return {
        SUCCESS: {true|false},
        MESSAGE: {執行訊息},
        RESULT: ""
    }
`}
                    </i>
                </pre>
            </div>
        </div>
    );
}

export default FlowApi;
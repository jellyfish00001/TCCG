
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { RadioGroup } from '@progress/kendo-react-inputs';
import React from 'react';
import { withOrWithoutData } from '../../../Basic/BasicData';
import { handleEditedGridData, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { CascadeDropDown } from '../../../Components/Dropdowns/CascadeDropDown';
import { CommandCell } from '../../../Components/GridCell/CommandCell';
import { getUserListByOuId } from '../../../Basic/CommonService';
import { Window } from '@progress/kendo-react-dialogs';
import { Formik } from 'formik';
import { WindowResizehook } from "../../../Hook/useWindowResize";
import { undertakerType } from './ProjectFillBasicService';
import * as Yup from 'yup';

const ProjectAsstOrgGrid = (props) => {

    const { fillBasicData, setEditedGridData, ddlData } = props

    // "機關"下拉選單
    const [organDdlData, setOrganDdlData] = React.useState([]);
    // "人員"下拉選單 預設資料
    const userDdlData = React.useRef([{ text: "請選擇人員", value: "" }]);

    //視窗狀態
    const [windoIsOpen, setWindoIsOpen] = React.useState(false);
    const dimensions = WindowResizehook();
    const formRef = React.useRef(null);

    const [gridData, setGridData] = React.useState([]);

    const [formData, setFormData] = React.useState({ ASSISTANT_ORGAN_C: "", ASSISTANT_UNDERTAKER_C: "" });

    // 紀錄異動資料
    const editedGridData = React.useRef([]);

    //是否可編輯
    const [enableEdit, setEnableEdit] = React.useState(false);

    // 移除
    const remove = dataItem => {
        let index = dataItem.hiddenIndex ?
            gridData.findIndex(d => d.hiddenIndex === dataItem.hiddenIndex)
            :
            gridData.findIndex(d => d.ASST_ID === dataItem.ASST_ID);
        gridData.splice(index, 1);
        dataItem.editType = 3
        handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'ASST_ID');
        setGridData([...gridData]);
        setEditedGridData(editedGridData.current);
    }

    /**
     * 刪除欄位
     * @param {*} props 
     * @returns 
     */
    const deleteCell = props => {
        return (
            <CommandCell>
                <Button type="button" title="刪除" icon="close" look="default"
                    onClick={() => remove(props.dataItem)} />
            </CommandCell>
        )
    }

    /**
         * 取得關聯下拉選單，第二層的資料
         * @param {*} ouId 
         * @param {*} undertakerType 
         * @returns 
         */
    const getUserDdlData = async (ouId, undertakerType) => {
        return await getUserListByOuId(ouId, undertakerType, "請選擇人員");
    }

    //#region 存檔驗證
    Yup.setLocale({
        //設定必填欄位顯示的訊息
        mixed: {
            required: "此欄位必填",
        }
    });

    /**
     * Grid欄位驗證
     */
    const validateField = Yup.object().shape({
        ASSISTANT_ORGAN_C: Yup.string().required(),
        ASSISTANT_UNDERTAKER_C: Yup.string().when('ASSISTANT_ORGAN_C', {
            is: (ASSISTANT_ORGAN_C) => !IsNullOrEmpty(ASSISTANT_ORGAN_C), then: Yup.string().required()
        })
    });
    //#endregion 

    /**
    * 存檔
    * @param {*} data 
    */
    const saveChanges = async (data) => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;

        //新增時寫入預設值
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            ASST_ID: 0,
            ASSISTANT_ORGAN_C: data.ASSISTANT_ORGAN_C,
            ASSISTANT_UNDERTAKER_C: data.ASSISTANT_UNDERTAKER_C,
            ASSISTANT_UNDERTAKER_C_NAME: data.ASSISTANT_UNDERTAKER_NAME,
            editType: 1
        }
        setGridData([...gridData, newRecord]);
        handleEditedGridData(editedGridData, newRecord, 'hiddenIndex', 'ASST_ID');
        setEditedGridData(editedGridData.current);
        toggleWindow();
    }


    //切換視窗
    const toggleWindow = () => {
        setWindoIsOpen(!windoIsOpen);
    }

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    /**
     * radioChangeEvent
     * @param {*} value 
     */
    const radioChangeEvent = (value) => {
        // 如果選擇"無"，原本grid資料都刪除
        if (!value) {
            gridData.map(dataItem => {
                dataItem.editType = 3
                handleEditedGridData(editedGridData, dataItem, 'hiddenIndex', 'ASST_ID');
            })
            setGridData([]);
            setEditedGridData(editedGridData.current);
        }
        setEnableEdit(value)
    }

    /**
     * 執行機關代碼對應的名稱
     * @param {*} props 
     * @returns 
     */
    const organCell = (props) => {
        const { ASSISTANT_ORGAN_C } = props.dataItem;
        return (<td>{organDdlData.find(x => x.value == ASSISTANT_ORGAN_C)?.text}</td>);
    }

    /**
    * 承辦人員代碼對應的名稱
    * @param {*} props 
    * @returns 
    */
    const userCell = (props) => {
        const { ASSISTANT_UNDERTAKER_C_NAME } = props.dataItem;
        return (<td>{ASSISTANT_UNDERTAKER_C_NAME}</td>);
    }

    React.useEffect(() => {
        // 機關下拉選單
        setOrganDdlData(ddlData);
    }, [ddlData])

    React.useEffect(() => {
        setGridData(fillBasicData.ProjectAsstOrg);
        setEnableEdit(fillBasicData.ProjectAsstOrg.length > 0)
        editedGridData.current = [];
    }, [fillBasicData])

    return (
        <>
            <div style={{ display: "inline-flex" }} >
                <RadioGroup
                    value={enableEdit}
                    onChange={(e) => radioChangeEvent(e.value)}
                    layout={"horizontal"}
                    data={withOrWithoutData}
                />
                <div className="fn-buttons" style={{ marginLeft: "10px" }}>
                    <Button title="新增協辦機關人員" type="button" disabled={!enableEdit} onClick={() => toggleWindow()}>新增協辦機關人員</Button>
                </div>
            </div>
            {windoIsOpen &&
                <>
                    <Window
                        title={"新增協辦機關"}
                        onClose={() => toggleWindow()}
                        initialWidth={dimensions.width * 0.5}
                        initialHeight={dimensions.height * 0.5}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <div className="fn-buttons">
                            <Button title="確認" type="button" onClick={() => handleSubmit()}>確認</Button>
                        </div>
                        <Formik
                            initialValues={formData}
                            onSubmit={(data) => saveChanges(data)}
                            innerRef={formRef}
                            validateOnBlur={false}
                            validationSchema={validateField}
                            //允許重複賦予初始值
                            enableReinitialize
                        >
                            {props => {
                                const {
                                    values,
                                    errors,
                                    handleChange,
                                    handleBlur,
                                    setValues
                                } = props;
                                return (
                                    <form>
                                        <table>
                                            <tr>
                                                <th className="addRedStar">
                                                    協辦機關/人員
                                                </th>
                                                <td>
                                                    <CascadeDropDown
                                                        fristDdlData={organDdlData}
                                                        firstDdlValue={values.ASSISTANT_ORGAN_C}
                                                        firstDdlError={errors.ASSISTANT_ORGAN_C}
                                                        firstColumn={"ASSISTANT_ORGAN_C"}
                                                        secondDdlInitData={userDdlData.current}
                                                        secondDdlValue={values.ASSISTANT_UNDERTAKER_C}
                                                        secondDdlError={errors.ASSISTANT_UNDERTAKER_C}
                                                        secondColumn={"ASSISTANT_UNDERTAKER_C"}
                                                        getSecondDdlData={(data) => getUserDdlData(data, undertakerType.ASSISTANT)}
                                                        values={values}
                                                        setValues={setValues}
                                                    />
                                                </td>
                                            </tr>
                                        </table>
                                    </form>
                                );
                            }}
                        </Formik>
                    </Window>
                </>
            }
            <Grid
                data={gridData}
                resizable={true}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn title="刪除" cell={deleteCell} width="50px" />
                <GridColumn title="執行機關" field="ASSISTANT_ORGAN_C" cell={organCell} />
                <GridColumn title="執行人員" field="ASSISTANT_UNDERTAKER_C" cell={userCell} />
            </Grid>
            <br />
        </>
    );
}
const areEqual = (prevProps, nextProps) => {
    /*
                    return true if passing nextProps to render would return
                    the same result as passing prevProps to render,
                    otherwise return false
                    */
    return (prevProps.fillBasicData == nextProps.fillBasicData);
}
export default React.memo(ProjectAsstOrgGrid, areEqual);
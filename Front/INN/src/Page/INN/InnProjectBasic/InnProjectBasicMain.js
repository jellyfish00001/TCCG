import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import { PageContainer } from "../../../Basic/PageContainer";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import TextInput from '../../../Components/Input/TextInput';
import RadioBoxList from '../../../Components/Input/RadioBoxList';
import { MultiSelect } from '@progress/kendo-react-dropdowns';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import { FormatDate, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import InnProjectPartnerGrid from "./InnProjectPartnerGrid";
import { getGlobalServerConfig, showGlobalMessageBox, showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import CacheLoader from '../../../Basic/CacheLoader';
import { getInnProjectBasic, getAllDropDowns, validateBasicField, initFiles, undertakerType } from "./InnProjectBasicService";
import { saveInnBasic } from "./InnProjectBasicService";
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../Components/Upload/TempFileUploadService';
import { downProjectAttachment, GetSetParam } from "../../../Basic/CommonService";
import { CascadeDropDown } from '../../../Components/Dropdowns/CascadeDropDown';
import { getUserIfonListByOuId } from '../../../Basic/CommonService';

const InnProjectBasicMain = (props) => {
    const {
        location: {
            state: {
                projectNo,
                projectName,
                IsManage
            }
        }
    } = props;

    //表單異動資料
    const formRef = useRef(null);
    //是否是顯示涉及其他提案類別
    const [isPlanOtherClass, setIsPlanOtherClass] = useState(false);
    //提案資料
    const [projectBasicData, setProjectBasicData] = useState({ InnProjectBasic: {}, InnPartner: [] });
    //參與提案人存檔資料
    const [editedInnPartner, setEditedInnPartner] = useState([]);
    //自訂欄位
    const [cusFields, setCusFields] = useState([]);
    //提案人數欄位
    const [sponsorType, setSponsorType] = useState([]);
    //提案人性別欄位
    const [sponsorSex, setSponsorSex] = useState([]);
    //參加組別欄位
    const [group, setGroup] = useState([]);
    //自訂欄位值
    const [cusFieldsValue, setCusValueField] = useState([]);
    //涉及其他主題值
    const [subtitle, setSubtitle] = useState([]);
    //已選擇涉及其他主題值
    const subtitletemDefultDdlData = useRef([]);
    // 上傳檔案
    const [files, setFiles] = useState([]);
    // 檔案上傳異動相關資訊
    const editedFiles = useRef([]);
    // 紀錄資料資否有異動
    const [isDataChange, setIsDataChange] = useState(false);
    // "人員"下拉選單 預設資料
    const userDdlData = useRef([{ text: "請選擇人員", value: "" }]);

    // 下拉選單
    const [ddlData, setDdlData] = useState(
        {
            //提案主題
            PROPOSAL_TYPE: [],
            // 機關
            SPONSOR_ORG: [],
            //涉及其他主題
            SUB_TITLE: [],

        });


    /**
     * 載入
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        // 取得下拉選單
        await getDdlData();
        //取的提案人數欄位
        let sponsorType = await GetSetParam('SPONSORTYPE', "");
        setSponsorType(sponsorType);
        let sponsorSex = await GetSetParam('SPONSOR_SEX', "");
        setSponsorSex(sponsorSex);
        let group = await GetSetParam('GROUP', "");
        setGroup(group);

        // 取得創新提案
        let result = await getInnProjectBasic(projectNo)
        setProjectBasicData(result);
        if (projectBasicData.InnProjectBasic.IS_PLURAL = true) {
            setIsPlanOtherClass(true);
        }

        if (!IsNullOrEmpty(result.InnProjectProposalType)) {
            // "涉及提案"已選擇的資料
            getSubtitleitem(result.InnProjectProposalType);
        }

        //自訂欄位名稱
        const cusFields = result.InnProjectCusFields.map(field => {
            return {
                CUS_FIELD_NANE: field.CUS_FIELD_NANE,
                CUS_FIELD_ID: field.CUS_FIELD_ID,
            };
        });

        //自訂欄位值
        const cusFieldsValue = result.InnProjectCusFieldValue.map(field => {
            return {
                CUS_FIELD_VALUE: field.CUS_FIELD_VALUE,
                CUS_FIELD_ID: field.CUS_FIELD_ID
            }
        });

        //結合自訂欄位
        const mergedCusFields = cusFields.map(field => ({
            ...field,
            CUS_FIELD_VALUE: cusFieldsValue.find(valueField => valueField.CUS_FIELD_ID === field.CUS_FIELD_ID)?.CUS_FIELD_VALUE || ""
        }));

        setCusFields(mergedCusFields);
        setCusValueField(mergedCusFields)

        let approvalFileList = result.Files

        let FileData = fileList(approvalFileList, "IDENTITY_FIELD");
        setFiles(FileData);

        SetMaskOnOff(false);
    }

    /**
     * 取得關聯下拉選單，第二層的資料
     * @param {*} ouId 
     * @param {*} undertakerType 
     * @param {*} userId 
     * @param {*} userName 
     * @returns 
     */
    const getUserDdlData = async (ouId, userId, userName) => {
        userId = userId ?? "";
        userName = userName ?? "";
        let data = await getUserIfonListByOuId(ouId, "請選擇人員", false);
        if (data.find(x => x.value === userId) === undefined) {
            data.push({ text: userName, value: userId, param: 'N' })
        }
        return data;
    }

    /**
     * 下載檔案
     * @param {*} fileId 
     */
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId, 4);
    }

    /**
     * 設定已選涉及提案內容
     * @param {*} data 
     */
    const getSubtitleitem = (data) => {
        let subtitleitem = data
        let selectedData = subtitletemDefultDdlData.current.filter(i => {
            if (subtitleitem.map(x => x.PROPOSAL_TYPE_ID).includes(Number(i.value)))
                return i;
        });
        setSubtitle(selectedData);
    }

    /**
     * 取得下拉選單
     */
    const getDdlData = async () => {
        let dropDowns = await getAllDropDowns();
        subtitletemDefultDdlData.current = [...dropDowns[0]];
        const selectedProposalType = await getInnProjectBasic(projectNo);
        const filteredProposalType = dropDowns[0].filter(item => item.value !== ''
            && item.value !== selectedProposalType.InnProjectBasic.PROPOSAL_TYPE);
        setDdlData({
            PROPOSAL_TYPE: [...dropDowns[0]],
            SPONSOR_ORG: [...dropDowns[1]],
            SUB_TITLE: [...filteredProposalType]
        })

    }

    useEffect(() => {
        loadData();
    }, [])


    /**
     * 存檔
     * @param {*} data 
     */
    const saveChanges = async (data) => {
        SetMaskOnOff(true);

        //處理提案主題資料
        let ProposalType = {}
        if (!IsNullOrEmpty(data.PROPOSAL_TYPE)) {
            ProposalType = { PROPOSAL_TYPE_ID: data.PROPOSAL_TYPE, PROPOSAL_KIND: 1 };
        }

        //處理涉及提案資料
        let SubTitle = []
        if (!IsNullOrEmpty(data.SUB_TITLE)) {
            SubTitle = data.SUB_TITLE.map(subTitleItem => ({
                PROPOSAL_TYPE_ID: subTitleItem.value,
                PROPOSAL_KIND: 2
            }));
        }

        let saveData = {
            ...projectBasicData,
            InnProjectBasic: {
                ...data

            },
            InnPartner: editedInnPartner,
            InnProjectProposalType: [
                ProposalType,
                ...SubTitle
            ],
            InnProjectCusFieldValue: cusFieldsValue,
            Files: [{ ...initFiles, PROJECT_NO: projectNo, FILE_KIND: "01" }],
        }
        saveData.Files[0].EditFiles = editedFiles.current;

        let saveResult = await saveInnBasic(saveData)
        if (saveResult.success) {
            showGlobalMessageBox("存檔成功", () => {
                window.location.reload()
            })
        }
        else {
            showGlobalMessageBox(saveResult.message);
        }

        SetMaskOnOff(false);
    }

    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }


    /**
     * 表單取消
     */
    const Cancel = async () => {
        SetMaskOnOff(true);
        await loadData();
        SetMaskOnOff(false);
    }

    // 上傳所需參數
    let paramFiles = {
        files: files,
        editedFiles: editedFiles,
        setFiles: setFiles,
        setIsDataChange,
        multiple: false,
        downFile
    }
    // 暫存檔上傳Service
    const tempFileUploadService = TempFileUploadService(paramFiles);

    return (

        <PageContainer
            style={{ overflow: "auto", height: "100%" }}
        >
            <CollapseBoardCard
                button={
                    <>
                        <Button title="存檔" onClick={() => handleSubmit()}>存檔</Button>
                        <Button title="取消" className="k-button-lighten" onClick={Cancel}>取消</Button>
                    </>

                }
                title="提案填報" isFirstArea={true}
            >
                <Formik
                    initialValues={projectBasicData.InnProjectBasic}
                    onSubmit={(data) => saveChanges(data)}
                    enableReinitialize={true}
                    innerRef={formRef}
                    validationSchema={validateBasicField}

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
                                            提案編號
                                        </th>
                                        <td colSpan={3}>
                                            {values.INN_PLAN_NO}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            提案人數
                                        </th>
                                        <td colSpan={3}>
                                            <RadioBoxList
                                                group='SPONSOR_TYPE'
                                                valueField='SET_TYPE'
                                                textField='SET_VALUE'
                                                data={sponsorType.map(item => ({
                                                    ...item,
                                                    checked: values.SPONSOR_TYPE === item.SET_TYPE
                                                }))}
                                                onChange={(e) => setValues({ ...values, SPONSOR_TYPE: e.value })}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            組別
                                        </th>
                                        <td colSpan={3}>
                                            <RadioBoxList
                                                group='GROUP'
                                                valueField='SET_TYPE'
                                                textField='SET_VALUE'
                                                data={group.map(item => ({
                                                    ...item,
                                                    checked: values.GROUP === item.SET_TYPE
                                                }))}
                                                onChange={(e) => setValues({ ...values, GROUP: e.value })}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            提案名稱
                                        </th>
                                        <td colSpan={3}>
                                            <TextInput
                                                onChange={handleChange}
                                                value={values.INN_PLAN_NAME}
                                                name="INN_PLAN_NAME"
                                                style={{ width: "100%" }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <CommonTooltip title={"是否有實施提案所訂不受理範圍之各項情形？"} content={
                                                <>
                                                    不受理情形如下<br />
                                                    (1)112年6月30日前已執行完成<br />
                                                    (2)抄襲網站資料、他人委託研究案、論文或其他著作等侵害第三人智慧財產權情事。<br />
                                                    (3)112年6月30日前已在本府或其他機關獲獎者。
                                                </>
                                            }
                                            />
                                        </th>
                                        <td colSpan={3}>
                                            <RadioBoxList
                                                group='REJECT_YN'
                                                valueField='value'
                                                textField='text'
                                                data={[
                                                    {
                                                        text: '無', value: "0", checked: values.REJECT_YN == '0'
                                                    },
                                                    {
                                                        text: '有', value: "1", checked: values.REJECT_YN == '1'
                                                    }
                                                ]}
                                                onChange={(e) => setValues({ ...values, REJECT_YN: e.value })}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            主要提案類別
                                        </th>
                                        <td colSpan={3}>
                                            <div style={{ display: 'flex', alignItems: 'center' }}>
                                                <DropDownListWithValue
                                                    name="PROPOSAL_TYPE"
                                                    data={ddlData.PROPOSAL_TYPE}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.PROPOSAL_TYPE ?? ""}
                                                    onChange={(e) => {
                                                        setValues({ ...values, PROPOSAL_TYPE: e.target.value })
                                                        const subtitleData = ddlData.PROPOSAL_TYPE.filter(item => item.value !== e.target.value && item.value !== '');
                                                        setDdlData(prevState => ({
                                                            ...prevState,
                                                            SUB_TITLE: subtitleData
                                                        }));
                                                    }}
                                                    error={errors.PROPOSAL_TYPE}
                                                />
                                            </div>
                                        </td>
                                    </tr>
                                    {isPlanOtherClass && (
                                        <tr>
                                            <th>
                                                涉及其他提案類別
                                            </th>
                                            <td colSpan={3}>
                                                <MultiSelect
                                                    popupSettings={
                                                        { className: "dropdown-text-size" }
                                                    }
                                                    placeholder="請選擇"
                                                    name='SUB_TITLE'
                                                    data={ddlData.SUB_TITLE}
                                                    textField="text"
                                                    dataItemKey="value"
                                                    onChange={(e) => {
                                                        setSubtitle([...e.value]);
                                                        setValues({ ...values, SUB_TITLE: e.target.value })
                                                    }}
                                                    value={subtitle}
                                                    error={errors.SUB_TITLE}
                                                />
                                            </td>
                                        </tr>
                                    )}
                                    <tr>
                                        <th className="addRedStar">
                                            問題描述
                                        </th>
                                        <td colSpan={3}>
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='INN_DESCRIPTION'
                                                value={values.INN_DESCRIPTION}
                                                onChange={handleChange}
                                                error={errors.INN_DESCRIPTION}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            提案構想解決方式
                                        </th>
                                        <td colSpan={3}>
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='IDEA_CONTENT'
                                                value={values.IDEA_CONTENT}
                                                onChange={handleChange}
                                                error={errors.IDEA_CONTENT}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            預期效益
                                        </th>
                                        <td colSpan={3}>
                                            <PureHtmlTextAreaInput
                                                rows={3}
                                                name='EXPECT_BENEFIT'
                                                value={values.EXPECT_BENEFIT}
                                                onChange={handleChange}
                                                error={errors.EXPECT_BENEFIT}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            是否為本府尚未推行過之創意
                                        </th>
                                        <td colSpan={3}>
                                            <RadioBoxList
                                                group='SPREAD_IDEA_YN'
                                                valueField='value'
                                                textField='text'
                                                data={[
                                                    {
                                                        text: '是', value: "1", checked: values.SPREAD_IDEA_YN == '1'
                                                    },
                                                    {
                                                        text: '否', value: "0", checked: values.SPREAD_IDEA_YN == '0'
                                                    }
                                                ]}
                                                onChange={(e) => setValues({ ...values, SPREAD_IDEA_YN: e.value })}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            是否為全國首創？
                                        </th>
                                        <td colSpan={3}>
                                            <RadioBoxList
                                                group='ORIGINATE_YN'
                                                valueField='value'
                                                textField='text'
                                                data={[
                                                    {
                                                        text: '是', value: "1", checked: values.ORIGINATE_YN == '1'
                                                    },
                                                    {
                                                        text: '否', value: "0", checked: values.ORIGINATE_YN == '0'
                                                    }
                                                ]}
                                                onChange={(e) => setValues({ ...values, ORIGINATE_YN: e.value })}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            選擇機關提案人
                                        </th>
                                        <td colSpan={3}>
                                            <CascadeDropDown
                                                fristDdlData={ddlData.SPONSOR_ORG}
                                                firstDdlValue={values.SPONSOR_ORG}
                                                firstDdlError={errors.SPONSOR_ORG}
                                                firstColumn={"SPONSOR_ORG"}
                                                firstDdlStyle={{ width: '250px' }}
                                                fristDdlDisabled={IsManage ? false : true}
                                                secondDdlInitData={userDdlData.current}
                                                secondDdlValue={values.SPONSOR_NAME}
                                                secondDdlError={errors.SPONSOR_NAME}
                                                secondColumn={"SPONSOR_NAME"}
                                                secondItemRender={(li, item) => {
                                                    if (item.dataItem.param === "Y") {
                                                        return React.cloneElement(li, li.props, <span>{li.props.children}</span>)
                                                    }
                                                    else {
                                                        return <></>
                                                    }
                                                }}
                                                getSecondDdlData={(data) => getUserDdlData(data, values.SPONSOR_NAME, values.SPONSOR_NAME)}
                                                values={values}
                                                setValues={(SPONSOR_ORG, SPONSOR_NAME) => {
                                                    setValues({ ...values, SPONSOR_ORG: SPONSOR_ORG.SPONSOR_ORG })
                                                    if (!IsNullOrEmpty(SPONSOR_NAME)) {
                                                        setValues({
                                                            ...values,
                                                            SPONSOR_NAME: SPONSOR_NAME.text,
                                                            SPONSOR_UNIT: SPONSOR_NAME.SPONSOR_UNIT,
                                                            SPONSOR_TITLE: SPONSOR_NAME.SPONSOR_TITLE
                                                        })
                                                    }
                                                }
                                                }
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar" style={{ width: '25%' }}>
                                            主要提案人員－職稱
                                        </th>
                                        <td style={{ width: '25%' }}>
                                            {values.SPONSOR_TITLE}

                                        </td>
                                        <th className="addRedStar" style={{ width: '25%' }}>
                                            主要提案人員－所屬單位
                                        </th>
                                        <td style={{ width: '25%' }}>
                                            {values.SPONSOR_UNIT}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar" style={{ width: '25%' }}>
                                            主要提案人員－性別
                                        </th>
                                        <td>
                                            <RadioBoxList
                                                group='SPONSOR_SEX'
                                                valueField='SET_TYPE'
                                                textField='SET_VALUE'
                                                data={sponsorSex.map(item => ({
                                                    ...item,
                                                    checked: values.SPONSOR_SEX === item.SET_TYPE
                                                }))}
                                                onChange={(e) => setValues({ ...values, SPONSOR_SEX: e.value })}
                                            />
                                        </td>
                                        <th className="addRedStar">
                                            聯絡人
                                        </th>
                                        <td>
                                            {values.CONTACT_NAME}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            電話
                                        </th>
                                        <td>
                                            {values.CONTACT_TEL}
                                        </td>
                                        <th className="addRedStar">
                                            Email
                                        </th>
                                        <td>
                                            {values.CONTACT_EMAIL}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            參與提案人(最多5)名
                                        </th>
                                        <td colSpan={3}>
                                            <InnProjectPartnerGrid
                                                InnPartnerData={projectBasicData.InnPartner}
                                                setEditedInnPartner={(gridData) => {
                                                    setEditedInnPartner(gridData)
                                                }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            附件上傳
                                        </th>
                                        <td colSpan={3}>
                                            <TempFileUploader
                                                {...tempFileUploadService.uploaderParam}
                                                saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                                saveHeaders={{
                                                    // @ts-ignore
                                                    'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                    'CacheToken': CacheLoader().GetCache(),
                                                }}
                                                files={files}
                                                setFiles={setFiles}
                                                multiple={false}
                                            />
                                        </td>
                                    </tr>
                                </table>
                                {cusFields.length > 0 && (
                                    <table>
                                        {cusFields.map((field, index) => (
                                            <tr key={index}>
                                                <input type="hidden" value={field.CUS_FIELD_ID} />
                                                <th>{field.CUS_FIELD_NANE}</th>
                                                <td colSpan={3}>
                                                    <PureHtmlTextAreaInput
                                                        rows={2}
                                                        value={field.CUS_FIELD_VALUE}
                                                        onChange={(e) => {
                                                            const updatedFields = [...cusFields];
                                                            updatedFields[index].CUS_FIELD_VALUE = e.target.value;
                                                            setCusValueField(updatedFields);
                                                        }}
                                                    />
                                                </td>
                                            </tr>
                                        ))}
                                    </table>
                                )}
                            </form>
                        )
                    }}
                </Formik>
            </CollapseBoardCard>
        </PageContainer>
    )
}
export default InnProjectBasicMain;
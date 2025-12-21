import React, { useState, useEffect, useContext, Fragment } from 'react';
import AnnouncementService from '../announcement.service';
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty, FormatDate } from '../../../../Basic/SDOExtension';
import { FileUploader } from '../../../../Components/Upload/FileUploader';
import TwDatePicker from '../../../../Components/DateInputs/TwDatePicker'
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../../../../Components/Input/TextInput'
import TextEditor from '../../../../Components/Editor/TextEditor'
import { useHookstate } from '@hookstate/core';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';
import RadioBoxList from '../../../../Components/Input/RadioBoxList';
import { getGlobalServerConfig } from '../../../../Route/RootMiddleware';
import { EditorUtils, ProseMirror } from '@progress/kendo-react-editor';
import { tagMark } from '../../../../Components/Editor/new-mark';
import CacheLoader from '../../../../Basic/CacheLoader';

const { Schema, EditorView, EditorState } = ProseMirror;
const AddMdf = (props) => {
    const [files, setFiles] = React.useState([]);
    const [fileSeqNo, setFileSeqNo] = useState([]) //存檔的時傳入的參數型態為string array

    /**@type {any} */
    const _state = useHookstate(props.state);
    const IsAdd = IsNullOrEmpty(_state.sid.get()) ? true : false
    const [annTypeData, setAnnTypeData] = useState([])
    const [announcementData, setAnnouncementData] = useState({
        SID: _state.sid.get(),
        TITLE: "",
        COMMENT: "",
        EFFECTIVE_DATE: new Date(),
        EXPIRE_DATE: new Date(),
        ATTACH_NAME: [],
        ANN_TYPE: "", // 公告類別
        IS_URL_LINK: "N", // 是否為外部連結
        URL_LINK: "", // 連結網址
    });

    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        //取得公告類別清單
        getAnnTypeData()
    }, []);

    useEffect(() => {
        if (!IsAdd) {
            //取得單筆公告bySId
            getAnnouncementBySId()
        }
        else {
            setAnnouncementData({
                SID: _state.sid.get(),
                TITLE: "",
                COMMENT: "",
                EFFECTIVE_DATE: new Date(),
                EXPIRE_DATE: new Date(),
                ATTACH_NAME: [],
                ANN_TYPE: "", // 公告類別
                IS_URL_LINK: "N", // 是否為外部連結
                URL_LINK: "", // 連結網址
            });
        }
    }, [_state.sid.get()]);


    //取得單筆公告bySId
    const getAnnouncementBySId = async () => {
        _state.visible.set(false);
        const result = await AnnouncementService.getAnnouncementBySId(_state.sid.get());
        if (result.ATTACH_NAME.length > 0) {
            await getUploads([...result.ATTACH_NAME]);
        }
        await setAnnouncementData(result);
        _state.visible.set(true);
    }

    //取得公告類別清單
    const getAnnTypeData = async () => {
        let data = await AnnouncementService.getAnnTypeData();
        await setAnnTypeData(data);
    }
    /**
     * 取得上傳檔案資訊
     * @param {*} fileSeqNo 
     */
    const getUploads = async (fileSeqNo) => {
        setFileSeqNo(IsNullOrEmpty(fileSeqNo) ? [] : fileSeqNo);

        let fileResult = await AnnouncementService.getGetUploads(fileSeqNo)
        fileResult.map(x => x.isUploaded = true)
        setFiles(fileResult);
    }

    /**
     * 存檔
     * @param {*} data 
     */
    const submit = async (data) => {

        //資料處理
        let fileNames = { fileNames: data.ATTACH_NAME } //上傳檔案讀取的參數為array
        let announcementData = { ...data }
        announcementData.ATTACH_NAME = announcementData.ATTACH_NAME.toString() //公告存檔的時傳入的參數型態為string
        announcementData.EFFECTIVE_DATE = FormatDate(announcementData.EFFECTIVE_DATE, 'YYYY-MM-DD')
        announcementData.EXPIRE_DATE = FormatDate(announcementData.EXPIRE_DATE, 'YYYY-MM-DD')
        //新增or修改 公告
        let response;
        if (IsAdd) {
            response = await AnnouncementService.insertAnnouncement(fileNames, announcementData)
        } else {
            response = await AnnouncementService.updateAnnouncement(fileNames, announcementData)
        }

        showMessage(response.result, {
            onOkAction: () => {
                if (response.ok) {
                    props.closeWindow();
                    props.refreshGrid();
                    setFiles([]);

                }
            }
        })
    }

    /**
     * 預覽
     */
    const preview = (URL_LINK) => {
        var link = React.createElement('a', { href: URL_LINK }, "");
        link.onClick();
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        TITLE: Yup.string()
            .required('公告標題必填'),
        ANN_TYPE: Yup.string()
            .required('公告類別必選'),
        EFFECTIVE_DATE: Yup.date()
            .required('起始日期必填').nullable(),
        EXPIRE_DATE: Yup.date()
            .required('到期日期必填').nullable()
            .min(Yup.ref('EFFECTIVE_DATE'), "到期日期不可早於起始日期"),

        COMMENT: Yup.string()
            .when("IS_URL_LINK", {
                is: "N",
                then: Yup.string().required('公告事項必填')
            }),
        URL_LINK: Yup.string()
            .when("IS_URL_LINK", {
                is: "Y",
                then: Yup.string().required('連結網址必填')
            })
    });


    const onMount = (event, value) => {
        const { viewProps } = event;
        const { plugins, schema } = viewProps.state;
        const table_cell = { ...schema.spec.nodes.get('table_cell') };
        const table = { ...schema.spec.nodes.get('table') };
        table_cell.attrs['style'] = {
            default: 'border: 1px solid black;',
        };
        table.attrs['style'] = {
            default: 'width:100%;',
        };
        let nodes = schema.spec.nodes.update('table_cell', table_cell).update('table', table);
        const mark = tagMark("s");
        let marks = schema.spec.marks.append(mark);

        const mySchema = new Schema({
            nodes,
            marks
        });
        const doc = EditorUtils.createDocument(mySchema, value);
        return new EditorView(
            {
                mount: event.dom,
            },
            {
                ...event.viewProps,
                state: EditorState.create({
                    doc,
                    plugins,
                }),
            }
        );
    };

    return (
        <Fragment>
            {_state.visible.get() &&
                <WindowBox
                    width={60}
                    height={80}
                    title={_state.title.get()}
                    onClose={props.closeWindow}
                >
                    <Formik
                        initialValues={announcementData}
                        validationSchema={validateField}
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
                                handleChange,
                                setValues
                            } = props;
                            return (
                                <form onSubmit={handleSubmit}>
                                    <div className="fn-buttons">
                                        <Button type="submit">存檔</Button>
                                    </div>
                                    <table>
                                        <tbody>
                                            <tr>
                                                <th>公告標題</th>
                                                <td>
                                                    <TextInput
                                                        name="TITLE"
                                                        value={values.TITLE}
                                                        onChange={handleChange}
                                                        onBlur={handleBlur}
                                                        error={errors.TITLE}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>公告類別</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        data={annTypeData}
                                                        value={values.ANN_TYPE}
                                                        textField="SET_VALUE"
                                                        dataItemKey="SET_TYPE"
                                                        onChange={(e) => setValues({ ...values, ANN_TYPE: e.value != null ? e.value.SET_TYPE : "" })}
                                                        error={errors.ANN_TYPE}
                                                    ></DropDownListWithValue>
                                                </td>
                                            </tr>
                                            <tr >
                                                <th>公告日期</th>
                                                <td>
                                                    <tr style={{ display: 'flex', alignItems: "center" }}>
                                                        <TwDatePicker
                                                            name="EFFECTIVE_DATE"
                                                            format={"yyy/MM/dd"}
                                                            onChange={handleChange}
                                                            value={values.EFFECTIVE_DATE}
                                                            error={errors.EFFECTIVE_DATE}
                                                        />
                                                        &nbsp;~&nbsp;
                                                        <TwDatePicker
                                                            name="EXPIRE_DATE"
                                                            format={"yyy/MM/dd"}
                                                            onChange={handleChange}
                                                            value={values.EXPIRE_DATE}
                                                            error={errors.EXPIRE_DATE}
                                                        />

                                                    </tr>
                                                </td>
                                            </tr>

                                            <tr>
                                                <th>是否為外部連結</th>
                                                <td>
                                                    <RadioBoxList
                                                        group='IS_URL_LINKList'
                                                        valueField='id'
                                                        textField='name'
                                                        data={[
                                                            { id: "Y", name: '是', checked: values.IS_URL_LINK == "Y" },
                                                            { id: "N", name: '否', checked: values.IS_URL_LINK == "N" }
                                                        ]}
                                                        onChange={(e) => setValues({ ...values, IS_URL_LINK: e.value })}
                                                    />
                                                </td>
                                            </tr>

                                            {
                                                values.IS_URL_LINK === "Y" &&
                                                <>
                                                    <tr>
                                                        <th>連結網址</th>
                                                        <td>
                                                            <Button onClick={() => preview(values.URL_LINK)}>預覽</Button>
                                                            <TextInput
                                                                name="URL_LINK"
                                                                value={values.URL_LINK}
                                                                onChange={handleChange}
                                                                onBlur={handleBlur}
                                                                error={errors.URL_LINK}
                                                            />
                                                        </td>
                                                    </tr>
                                                </>
                                            }

                                            {
                                                values.IS_URL_LINK === "N" &&
                                                <>
                                                    <tr>
                                                        <th>公告事項</th>
                                                        <td colSpan={3}>
                                                            <TextEditor
                                                                name="COMMENT"
                                                                // value={values.COMMENT}
                                                                onChange={(e) => {
                                                                    console.log(e.html);
                                                                    setValues({ ...values, COMMENT: e.html })
                                                                }}
                                                                error={errors.COMMENT}
                                                                onMount={(e) => onMount(e, values.COMMENT)}
                                                            />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <th>附件</th>
                                                        <td >
                                                            <FileUploader
                                                                fileSeqNo={values.ATTACH_NAME}
                                                                setFileSeqNo={setFileSeqNo}
                                                                setValue={(data) => setValues({ ...values, ATTACH_NAME: data })}
                                                                reminderText={"上傳檔案限制10Mb"}
                                                                maxFileSize={10485760} // 10Mb*1024*1024
                                                                autoUpload={true}
                                                                files={files}
                                                                setFiles={setFiles}

                                                                saveUrl={getGlobalServerConfig().Bridge_backEndUrl.get()}
                                                                saveHeaders={{
                                                                    'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                                    // 'CacheToken': CacheLoader().GetCache(),
                                                                    'RouteUrl': getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadFile'
                                                                }}
                                                                removeUrl={getGlobalServerConfig().Bridge_backEndUrl.get()}
                                                                removeHeaders={{
                                                                    'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                                    // 'CacheToken': CacheLoader().GetCache(),
                                                                    'RouteUrl': getGlobalServerConfig().backEndUrl.get() + 'UploadFile/RemoveUploads'
                                                                }}
                                                            />
                                                        </td>
                                                    </tr>
                                                </>
                                            }



                                        </tbody>
                                    </table>
                                </form>
                            );
                        }}
                    </Formik>
                </WindowBox>

            }
        </Fragment>
    );
}

export default AddMdf;


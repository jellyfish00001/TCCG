import React from 'react';
import { getBuildKindList } from '../ProjectFillBasic/ProjectFillBasicService';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { Checkbox, RadioButton } from '@progress/kendo-react-inputs';
import { Button } from "@progress/kendo-react-buttons";
import { Window } from '@progress/kendo-react-dialogs';
import { WindowResizehook } from "../../../Hook/useWindowResize";
import { Formik } from 'formik';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';

const ProjectBuildKind = (props) => {
    let { fillBasicData, setSelectData } = props;

    //#region 參數宣告
    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState({
        // "建設項目類別"
        BUILD_KIND_TYPE: [],
        // "建設類別"
        BUILD_KIND: []
    });

    //視窗狀態
    const [windoIsOpen, setWindoIsOpen] = React.useState(false);

    const dimensions = WindowResizehook();

    const formRef = React.useRef(null);

    // 計畫建設類別資料
    const [projectbuildKindData, setProjectbuildKindData] = React.useState({});
    //#endregion

    /**
     * 載入資料
     */
    const loadData = async () => {
        let result = await getBuildKindList();
        if (result.length > 0) {
            setDdlData({
                // 建設項目類別
                BUILD_KIND_TYPE: [...result[0]],
                // 建設類別
                BUILD_KIND: [...result[1]]
            })
        }
    }

    /**
     * 顯示已選取的資料
     * @param {*} data 
     * @returns 
     */
    const showSelectResult = (data) => {
        return ddlData.BUILD_KIND.filter(x => data.includes(x.value)).map(x => x.label).join("、")
    }

    /**
     * 取得選項元素
     * @param {*} data 
     * @param {*} type 
     * @param {*} props 
     * @returns 
     */
    const getOptionElement = (data, type, props) => {
        let { values, setValues } = props;
        let element = [];
        for (let i = 0; i < data.length; i = i + 5) {
            let dateItem = [];
            for (let j = 0; j < 5; j++) {
                if (data[i + j] != undefined) {
                    dateItem.push(data[i + j]);
                } else {
                    dateItem.push("");
                }
            }
            element.push(
                <tr>
                    {
                        dateItem.map(x =>
                            <>
                                {
                                    IsNullOrEmpty(x) ? <td></td> :
                                        (type == "01" ?
                                            //主要建設
                                            <td>
                                                <RadioButton
                                                    checked={values.MainOption == x.value}
                                                    disabled={values.AttachedOption.includes(x.value)}
                                                    value={x.value}
                                                    label={x.label}
                                                    onChange={(e) => {
                                                        setValues({ ...values, MainOption: e.value });
                                                    }}
                                                />
                                            </td> :
                                            //附屬設施
                                            <td>
                                                <Checkbox
                                                    checked={values.AttachedOption.includes(x.value)}
                                                    disabled={values.MainOption.includes(x.value)}
                                                    value={x.value}
                                                    label={x.label}
                                                    onChange={(e) => {
                                                        let dataItem = [...values.AttachedOption];
                                                        if (e.value) {
                                                            dataItem.push(e.target.element.value);
                                                        } else {
                                                            dataItem.splice(dataItem.indexOf(e.target.element.value), 1)
                                                        }
                                                        setValues({ ...values, AttachedOption: dataItem });
                                                    }}
                                                />
                                            </td>
                                        )
                                }
                            </>
                        )
                    }
                </tr>
            );
        }
        return element;
    }

    //切換視窗
    const toggleWindow = () => {
        setWindoIsOpen(!windoIsOpen);
    }

    /**
     * 存檔
     * @param {*} data 
     */
    const saveChanges = async (data) => {
        if (data.MainOption.length == 0) {
            showGlobalMessageBox("主要建設至少選擇一項");
            return;
        }

        let result = [];

        //主要建設
        result.push(
            {
                BUILD_KIND_TYPE: "01",
                BUILD_KIND: data.MainOption
            }
        )
        //附屬設施
        data.AttachedOption.map(x => {
            result.push(
                {
                    BUILD_KIND_TYPE: "02",
                    BUILD_KIND: x
                })
        });

        setSelectData(result);
        toggleWindow();
    }

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    //利用Ref把Formik的reset功能拉出來，以提供外部按鈕呼叫
    const cancelChanges = () => {
        if (formRef.current) {
            formRef.current.handleReset();
        }
    }

    React.useEffect(() => {
        loadData();
    }, [])

    React.useEffect(() => {
        setProjectbuildKindData({
            MainOption: fillBasicData.ProjectBuildKind.filter(x => x.BUILD_KIND_TYPE == "01").map(x => x.BUILD_KIND)[0] ?? "",
            AttachedOption: fillBasicData.ProjectBuildKind.filter(x => x.BUILD_KIND_TYPE == "02").map(x => x.BUILD_KIND)
        });
    }, [fillBasicData])

    return (
        <>
            <div className="fn-buttons">
                <Button title="選擇建設類別" type="button" onClick={() => toggleWindow()}>選擇建設類別</Button>
            </div>
            {
                windoIsOpen &&
                <>
                    <Window
                        title={"選擇建設類別"}
                        onClose={() => toggleWindow()}
                        width={dimensions.width * 0.6 }
                        height={dimensions.height * 0.75}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <div className="fn-buttons">
                            <Button title="確認" type="button" onClick={() => handleSubmit()}>確認</Button>
                            <Button title="取消" type="button" className="k-button-lighten" onClick={() => cancelChanges()}>取消</Button>
                        </div>
                        <Formik
                            initialValues={projectbuildKindData}
                            onSubmit={(data) => saveChanges(data)}
                            innerRef={formRef}
                            validateOnBlur={false}
                            //允許重複賦予初始值
                            enableReinitialize
                        >
                            {props => {
                                return (
                                    <form>
                                        <table>
                                            {ddlData.BUILD_KIND_TYPE.map(x =>
                                                <>
                                                    <tr>
                                                        <th colSpan={5}>{x.label}</th>
                                                    </tr>
                                                    {getOptionElement(ddlData.BUILD_KIND, x.value, props)}
                                                </>
                                            )}
                                        </table>
                                    </form>
                                );
                            }}
                        </Formik>
                    </Window>
                </>
            }
            {
                ddlData.BUILD_KIND_TYPE.map(x =>
                    <>
                        {x.label}：{
                            x.value == "01" ?
                                //主要建設
                                showSelectResult(projectbuildKindData.MainOption) :
                                //附屬設施
                                showSelectResult(projectbuildKindData.AttachedOption)}
                        <br />
                    </>
                )
            }
        </>
    );

}

const areEqual = (prevProps, nextProps) => {
    /*
    return true if passing nextProps to render would return
    the same result as passing prevProps to render,
    otherwise return false
    */
    return prevProps.fillBasicData == nextProps.fillBasicData;
}
export default React.memo(ProjectBuildKind, areEqual);
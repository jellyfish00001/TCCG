import React from 'react';
import { Formik } from 'formik';
import GoogleMapReact from 'google-map-react';
import { FormatDate, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { withOrWithoutData } from '../../../Basic/BasicData';
import { GetStorageData, getUserListByOuId, SetStorageData, openProjectPrint } from '../../../Basic/CommonService';
import TextInput from '../../../Components/Input/TextInput';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { CascadeDropDown } from '../../../Components/Dropdowns/CascadeDropDown';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { showGlobalMessageBox, showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import { RadioGroup } from '@progress/kendo-react-inputs';
import { MultiSelect } from '@progress/kendo-react-dropdowns';
import { Button } from "@progress/kendo-react-buttons";
import ProjectAsstOrgGrid from './ProjectAsstOrgGrid';
import ProjectBudgetSourceGGrid from './ProjectBudgetSourceGGrid';
import ProjectBuildKind from './ProjectBuildKind';
import { getAllDropDowns, getProjectBasicFill, saveProjectBasicAdd, undertakerType, validataSourceGField, validateBasicField } from '../ProjectFillBasic/ProjectFillBasicService'
import { Error } from '@progress/kendo-react-labels';

const ProjectFillBasicMain = (props) => {

    const {
        type,
        projectAddOnSave,
        location: { state },
        location: {
            state: {
                projectNo,
                projAdjId,
                showSomeBtn,
                funRole
            }
        }
    } = props;

    //#region 參數宣告
    // 是否從調整撤銷
    const isAdj = type === "adjust";

    const loadTimes = React.useRef(0);

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState(
        {
            //"相關審查"
            REVIEWITEM: [],
            // "機關"
            ORGAN: [],
            // "辦理地點"
            TOWN_C: []
        });

    const reviewitemDefultDdlData = React.useRef([]);
    const townMDefultDdlData = React.useRef([]);
    // "相關審查"已選擇的資料
    const [reviewitemSelData, setReviewitemSelData] = React.useState([]);
    // "跨區" 縣市已選擇資料
    const [townMSelData, setTownMSelData] = React.useState([])
    // "人員"下拉選單 預設資料
    const userDdlData = React.useRef([{ text: "請選擇人員", value: "" }]);

    const [projectFillBasicData, setProjectFillBasicData] = React.useState({});

    const formRef = React.useRef(null);

    // 紀錄"計畫經費來源"有異動的資料
    const [editedBudgetSourceG, setEditedBudgetSourceG] = React.useState([]);
    // 紀錄"計畫協辦機關"有異動的資料
    const [editedProjectAsstOrg, setEditedProjectAsstOrg] = React.useState([]);

    // 是否載入
    const [isLoad, SetIsLoad] = React.useState(false);

    const projectStatus = React.useRef("")
    //#endregion

    React.useEffect(() => {
        loadData();
    }, [])

    // 載入資料
    const loadData = async () => {
        SetMaskOnOff(true);
        // 取得下拉選單
        await getDdlData();
        // 取得計畫基本資料(含計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關)
        await loadProjectBasicFillData();
        SetIsLoad(true);
        SetMaskOnOff(false);
    }

    // 取得下拉選單
    const getDdlData = async () => {
        let dropDowns = await getAllDropDowns();
        if (dropDowns.length > 0) {
            reviewitemDefultDdlData.current = [...dropDowns[0]];
            townMDefultDdlData.current = [...dropDowns[2]];
            setDdlData({
                REVIEWITEM: [...dropDowns[0]],
                ORGAN: [...dropDowns[1]],
                TOWN_C: [...dropDowns[2]]
            })
        }
    }

    // 從下拉清單資料中過濾出，"相關審查"已選擇的資料
    const getSelReviewitem = (data) => {
        let reviewitem = data.split(',');
        let selectedData = reviewitemDefultDdlData.current.filter(i => {
            if (reviewitem.includes(i.SET_TYPE))
                return i;
        });
        setReviewitemSelData(selectedData);
    }
    // 從下拉清單資料中過濾出，"跨區"已選擇的資料
    const getSelTownM = (data) => {
        let townM = data.split(',');
        let selectedData = townMDefultDdlData.current.filter(i => {

            if (townM.includes(i.value))
                return i;
        });
        setTownMSelData(selectedData);
    }

    // 取得計畫基本資料(含計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關)
    const loadProjectBasicFillData = async () => {
        let result = await getProjectBasicFill(projectNo, projAdjId);
        const {
            ProjectBasic,
            ProjectBasic: {
                CREATEDTIME
            },
        } = result;
        // 計畫狀態
        projectStatus.current = ProjectBasic.PROJECT_STATUS;

        result.ProjectBasic = {
            ...result.ProjectBasic,
            CREATEDTIME: IsNullOrEmpty(CREATEDTIME) ? null : new Date(CREATEDTIME),
            loadTimes: loadTimes.current
        }
        result.ProjectBuildKind = result.ProjectBuildKind ?? []

        if (!IsNullOrEmpty(ProjectBasic.REVIEWITEM)) {
            // "相關審查"已選擇的資料
            getSelReviewitem(ProjectBasic.REVIEWITEM);
        }

        if (!IsNullOrEmpty(ProjectBasic.TOWN_M)) {
            getSelTownM(ProjectBasic.TOWN_M)
        }

        // 紀錄"計畫經費來源"有異動的資料 清空
        setEditedBudgetSourceG([]);

        // 紀錄"計畫協辦機關"有異動的資料 清空
        setEditedProjectAsstOrg([]);

        setProjectFillBasicData(result);
        setMyPosition({ lat: result.ProjectBasic.X_COORD, lng: result.ProjectBasic.Y_COORD });
        loadTimes.current = loadTimes.current + 1;
    }

    const onMultiSelectChange = (event, props) => {
        const { values, setValues } = props;
        // 若選擇"無"，移除其他已選選項
        if (event.value.find(x => x.SET_TYPE === "99")) {
            event.value = event.value.filter(x => x.SET_TYPE === "99");
        }

        setValues({
            ...values,
            REVIEWITEM: event.value.map(item => item.SET_TYPE).join(",")
        })
        setReviewitemSelData([...event.value]);
    };

    // 取得關聯下拉選單，第二層的資料
    const getUserDdlData = async (ouId, undertakerType, userId, userName) => {
        userId = userId ?? "";
        userName = userName ?? "";
        let data = await getUserListByOuId(ouId, undertakerType, "請選擇人員", false);
        if (data.find(x => x.value === userId) === undefined) {
            data.push({ text: userName, value: userId, param: 'N' })
        }
        return data;
    }

    /**
    * 檢視點位click event or 設定點位click event
    * @param {*} type view:0，set:1
    */
    const pointClickEvent = (data, type) => {
        const { PROJECT_NO, X_COORD, Y_COORD } = data;

        //尚未產生PROJECT_NO，顯示訊息
        if (IsNullOrEmpty(PROJECT_NO)) {
            showGlobalMessageBox("請先進行【計畫暫存】取得計畫編號後，方可檢視點位");
            return
        }

        let url = "https://maps.tycg.gov.tw/tycg_painter/ResearchMapPainterPage.aspx?Aid=IMC_029&MapPaintId=";

        if (!IsNullOrEmpty(X_COORD) || !IsNullOrEmpty(Y_COORD)) {
            window.open(url + PROJECT_NO + '&Longitude=' + Y_COORD + '&Latitude=' + X_COORD + '&ShareEdit=' + type);
        } else if (!IsNullOrEmpty(address)) {
            window.open(url + PROJECT_NO + '&Address=' + address + '&ShareEdit=' + type);
        } else {
            window.open(url + PROJECT_NO + '&Address=桃園區縣府路1號&ShareEdit=' + type);
        }
    }

    // 驗證 經費來源 欄位
    const checkSourceGIsValid = async () => {
        //驗證
        let notValids = [];

        if (!IsNullOrEmpty(projectNo) && editedBudgetSourceG.length > 0) {
            // 經費來源資料驗證
            for (let index = 0; index < editedBudgetSourceG.length; index++) {
                const item = editedBudgetSourceG[index];
                if (item.editType !== 3) {
                    let isValid = await validataSourceGField.isValid(item);
                    if (!isValid) {
                        notValids.push(isValid);
                    }
                }
            }
            if (notValids.length > 0) {
                showGlobalMessageBox('經費來源有必填欄位');
                return false;
            }
        }

        return true;
    }

    // 存檔
    const saveChanges = async (data) => {
        SetMaskOnOff(true);
        if (await checkSourceGIsValid()) {
            let saveData = {
                ...projectFillBasicData,
                ProjectBasic: {
                    ...data,
                    CREATEDTIME: data.CREATEDTIME == null ?
                        data.CREATEDTIME : FormatDate(data.CREATEDTIME, 'YYYY-MM-DD')
                },
                ProjectBudgetSourceG: editedBudgetSourceG,
                ProjectAsstOrg: editedProjectAsstOrg
            }

            // 若為調整，設定 PROJ_ADJ_ID
            if (isAdj) {
                saveData.ProjectBasic.PROJ_ADJ_ID = projAdjId;
            }
            let saveResult = await saveProjectBasicAdd(saveData, isAdj);

            if (saveResult.success) {
                showGlobalMessageBox("存檔成功", () => {
                    if (IsNullOrEmpty(projectNo)) {
                        // 新增計畫存檔後，回傳事件
                        projectAddOnSave(saveResult.message);
                    } else {
                        //#region 重新更新session資料
                        let result = GetStorageData();
                        result.projectName = data.PROJECT_NAME;
                        result.execOrgan = data.EXEC_ORGAN_C;
                        SetStorageData(result);
                        props.location.state = result;
                        //#endregion
                        window.location.reload()
                    }
                });
            } else {
                showGlobalMessageBox(saveResult.message);
            }
        }
        SetMaskOnOff(false);
    }

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            // 若為新增計畫，跳出確認視窗
            if (IsNullOrEmpty(projectNo)) {
                showGlobalConfirmBox("是否確定新增計畫？\n新增後，如欲刪除請聯繫本府智發會承辦人", () => formRef.current.handleSubmit())
            } else {
                formRef.current.handleSubmit()
            }
        }
    }

    //#region 單點地圖定位=>google map控制
    const [mapDefaultProps, setMapDefaultProps] = React.useState({
        key: process.env.REACT_APP_GOOGLE_API_KEY,
        zoom: 12,
        //預設地址為 桃園區縣府路1號 - 桃園市政府
        center: {
            lat: 24.993098524588383,
            lng: 121.30101509392262
        }
    });

    // 預設位置
    const [myPosition, setMyPosition] = React.useState({})

    // 地址
    const [address, setAddress] = React.useState("");
    const [mapApiLoaded, setMapApiLoaded] = React.useState(false);
    const [mapInstance, setMapInstance] = React.useState(null);
    const [mapApi, setMapApi] = React.useState(null);
    // 記錄先前的marker
    const prevMarkerRef = React.useRef([]);

    const mapOption = (maps) => {
        return {
            streetViewControl: true,
            scrollwheel: false,  //是否允許使用者對地圖物件使用滑鼠滾輪
            mapTypeId: "roadmap",
            zoomControl: true, //縮放地圖
            zoomControlOptions: {
                position: maps.ControlPosition.TOP_RIGHT
            },
            mapTypeControl: true, //地圖類型
            mapTypeControlOptions: {
                style: maps.MapTypeControlStyle.DROPDOWN_MENU
            }
        }
    }

    // 當地圖載入完成，將地圖實體與地圖 API 傳入 state 供之後使用
    const apiHasLoaded = (map, maps) => {
        let marker = new maps.Marker({
            position: { lat: map.center.lat(), lng: map.center.lng() },
            map
        });
        prevMarkerRef.current = marker;

        setMapInstance(map);
        setMapApi(maps);
        setMapApiLoaded(true);

        return marker;
    };

    // 根據xy找地點
    const findLocationByXY = async (x, y) => {
        if (mapApiLoaded) {
            if (!IsNullOrEmpty(x) && !IsNullOrEmpty(y)) {
                //移除當下位置的marker
                prevMarkerRef.current.setMap(null);

                let marker = new mapApi.Marker({
                    position: { lat: parseFloat(x), lng: parseFloat(y) },
                    map: mapInstance
                });

                prevMarkerRef.current = marker;

                let markerPosition = marker.getPosition();
                let geocoder = new mapApi.Geocoder();
                geocoder.geocode({ 'latLng': markerPosition }, (results, status) => {
                    if (status === mapApi.GeocoderStatus.OK) {
                        if (results) {

                            mapInstance.setCenter(markerPosition);

                            let address = results[0].formatted_address;
                            setAddress(address);
                        }
                    }
                })
            }
        }
    }

    // 根據地址找地點
    const findLocationByAddr = (addr, props) => {
        if (mapApiLoaded) {
            const { values, setValues } = props;
            let geocoder = new mapApi.Geocoder();
            geocoder.geocode({ 'address': addr }, (results, status) => {
                if (status === mapApi.GeocoderStatus.OK) {
                    let lat = results[0].geometry.location.lat();
                    let lng = results[0].geometry.location.lng();

                    //移除當下位置的marker
                    prevMarkerRef.current.setMap(null);

                    let marker = new mapApi.Marker({
                        position: { lat, lng },
                        map: mapInstance
                    });

                    prevMarkerRef.current = marker;

                    let markerPosition = marker.getPosition();
                    mapInstance.setCenter(markerPosition);

                    setValues({
                        ...values,
                        X_COORD: lat.toString(), Y_COORD: lng.toString()
                    })
                } else {
                    showGlobalMessageBox("查無經緯度");
                }
            })
        }
    }

    const handleCenterChange = (props) => {
        if (mapApiLoaded) {
            const { values, setValues } = props;
            let lat = mapInstance.center.lat();
            let lng = mapInstance.center.lng();
            findLocationByXY(lat, lng);
            setValues({
                ...values,
                X_COORD: lat.toString(), Y_COORD: lng.toString()
            })
        }
    }

    React.useEffect(() => {
        findLocationByXY(myPosition.lat, myPosition.lng);
    }, [mapApiLoaded, myPosition])
    //#endregion

    //#region table cloumn
    const tableCloumnBlock1 = (props) => {
        const { values, errors, handleChange, handleBlur } = props;
        return (
            <>
                <tr>
                    <th className="addRedStar">
                        計畫年度
                    </th>
                    <td>
                        {values.PROJECT_YEAR}
                    </td>
                    <th>
                        列管編號
                    </th>
                    <td>
                        {!IsNullOrEmpty(values.PROJECT_NO) ? values.PROJECT_NO : "(存檔後產生)"}
                    </td>
                </tr>
                <tr>
                    <th className="addRedStar">
                        計畫名稱
                    </th>
                    <td colSpan={3}>
                        <TextInput
                            name="PROJECT_NAME"
                            value={values.PROJECT_NAME}
                            error={errors.PROJECT_NAME}
                            onChange={handleChange}
                            onBlur={handleBlur}
                            style={{ width: "100%" }}
                        />
                    </td>
                </tr>
            </>

        )
    }
    const tableCloumnBlock2 = (props) => {
        const { values, errors, setValues } = props;
        return (
            <>
                <tr>
                    <th>
                        <CommonTooltip title={"執行機關/人員"} content={"系統填報機關、洽辦機關"} />
                    </th>
                    <td colSpan={3}>
                        <CascadeDropDown
                            fristDdlData={ddlData.ORGAN}
                            firstDdlValue={values.EXEC_ORGAN_C}
                            firstDdlError={errors.EXEC_ORGAN_C}
                            firstColumn={"EXEC_ORGAN_C"}
                            firstDdlStyle={{ width: '250px' }}
                            fristDdlDisabled={(funRole == 2 || !IsNullOrEmpty(projAdjId)) ? false : true}
                            secondDdlInitData={userDdlData.current}
                            secondDdlValue={values.EXEC_UNDERTAKER_C}
                            secondDdlError={errors.EXEC_UNDERTAKER_C}
                            secondColumn={"EXEC_UNDERTAKER_C"}
                            secondItemRender={(li, item) => {
                                if (item.dataItem.param === "Y") {
                                    return React.cloneElement(li, li.props, <span>{li.props.children}</span>)
                                }
                                else {
                                    return <></>
                                }
                            }}
                            getSecondDdlData={(data) => getUserDdlData(data, undertakerType.EXEC, values.EXEC_UNDERTAKER_C, values.EXEC_UNDERTAKER_NAME)}
                            values={values}
                            setValues={setValues}
                        />
                    </td>
                </tr>
            </>

        )
    }
    const btn = () => {
        return (
            <>
                {
                    (IsNullOrEmpty(projectNo) || showSomeBtn || isAdj) &&
                    <>
                        <Button title="存檔" onClick={() => handleSubmit()}>存檔</Button>
                        <Button title="取消" className="k-button-lighten" onClick={() => loadProjectBasicFillData()}>取消</Button>
                    </>
                }
                {
                    !IsNullOrEmpty(projectNo) &&
                    <>
                        <Button title="預覽列印" className="k-button-lighten" onClick={() => openProjectPrint(state)}>預覽列印</Button>
                    </>
                }
            </>
        )
    }
    //#endregion

    const projLogCell = (values, logStatuses) => {
        if (values.ProjLogs === undefined || values.ProjLogs === null) {
            return "";
        }

        return values.ProjLogs
            .filter(x => logStatuses.includes(x.LOG_STATUS_C))
            .map((item, i) =>
                <>
                    <span>{i + 1}.</span>
                    <span>{FormatDate(item.LOG_DATE)} </span>
                    <span>{item.LOG_STATUS}</span>
                    <span>{IsNullOrEmpty(item.MEMO) ? "" : ":"}</span>
                    <span>{item.MEMO}</span>
                    <span>。</span>
                    <br />
                </>
            )
    }

    return (
        <>
            {
                isLoad ? (
                    IsNullOrEmpty(projectNo) ?
                        <>
                            <div className="fn-buttons">
                                {btn()}
                            </div>
                            <Formik
                                initialValues={projectFillBasicData.ProjectBasic}
                                onSubmit={(data) => saveChanges(data)}
                                innerRef={formRef}
                                validationSchema={validateBasicField}
                                //允許重複賦予初始值
                                enableReinitialize
                            >
                                {props => {
                                    return (
                                        <form>
                                            <table>
                                                {tableCloumnBlock1(props)}
                                                {tableCloumnBlock2(props)}
                                            </table>
                                        </form>
                                    );
                                }}
                            </Formik>
                        </>
                        :
                        <CollapseBoardCard
                            title={isAdj ? "2.基本資料調整" : "計畫基本資料"}
                            button={btn()} isFirstArea={true}>
                            <Formik
                                initialValues={projectFillBasicData.ProjectBasic}
                                onSubmit={(data) => saveChanges(data)}
                                innerRef={formRef}
                                validationSchema={validateBasicField}
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
                                                {tableCloumnBlock1(props)}
                                                <tr>
                                                    <th>
                                                        <CommonTooltip title={"經費來源"} content={
                                                            <>
                                                                依年度、預算類型、來源、是否為中央補助型計畫或前瞻補助案件等填報。<br />
                                                                1.跨年度計畫請依年度新增經費。<br />
                                                                2.補助經費務必上傳中央核定函等相關文件。
                                                            </>
                                                        } />
                                                    </th>
                                                    <td colSpan={3}>
                                                        <ProjectBudgetSourceGGrid
                                                            projectBudgetSourceG={projectFillBasicData.ProjectBudgetSourceG}
                                                            setEditedGridData={(gridData, data) => {
                                                                // 需檢驗至少填一筆資料，故須將資料回塞
                                                                setProjectFillBasicData(
                                                                    {
                                                                        ...projectFillBasicData,
                                                                        ProjectBudgetSourceG: gridData
                                                                    }
                                                                )
                                                                //紀錄有異動的資料
                                                                setEditedBudgetSourceG(data)
                                                            }}
                                                            editedBudgetSourceG={editedBudgetSourceG}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>
                                                        <CommonTooltip title={"建設類別"} content={
                                                            <>
                                                                主要建設(該計畫或工程的主要用途，單選)。<br />
                                                                附屬設施(除主要用途外的其他用途，非必填、可複選)。
                                                            </>
                                                        } />
                                                    </th>
                                                    <td colSpan={3}>
                                                        <ProjectBuildKind
                                                            fillBasicData={projectFillBasicData}
                                                            setSelectData={(data) => {
                                                                setProjectFillBasicData(
                                                                    {
                                                                        ...projectFillBasicData,
                                                                        ProjectBuildKind: data
                                                                    }
                                                                )
                                                            }}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>
                                                        <CommonTooltip title={"相關審查"} content={"挑選需辦理審查項目，可複選；若不需任何審查請選擇無。"} />
                                                    </th>
                                                    <td colSpan={3}>
                                                        <MultiSelect
                                                            popupSettings={
                                                                { className: "dropdown-text-size" }
                                                            }
                                                            placeholder="請選擇   "
                                                            name='REVIEWITEM'
                                                            data={ddlData.REVIEWITEM}
                                                            textField="SET_VALUE"
                                                            dataItemKey="SET_TYPE"
                                                            onChange={(e) => {
                                                                onMultiSelectChange(e, props)
                                                            }}
                                                            value={reviewitemSelData}
                                                            error={errors.REVIEWITEM}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>
                                                        <CommonTooltip
                                                            title={"特殊加註"}
                                                            content={"管考權限"}
                                                            withoutRedStar={true} />
                                                    </th>
                                                    <td colSpan={3}>
                                                        {values.SPEC_NOTE}
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>
                                                        <CommonTooltip title={"主管機關/人員"} content={"預算來源機關、聯繫窗口"} />
                                                    </th>
                                                    <td colSpan={3}>
                                                        <CascadeDropDown
                                                            fristDdlData={ddlData.ORGAN}
                                                            firstDdlValue={values.MASTER_ORGAN_C}
                                                            firstDdlError={errors.MASTER_ORGAN_C}
                                                            firstColumn={"MASTER_ORGAN_C"}
                                                            firstDdlStyle={{ width: '250px' }}
                                                            secondDdlInitData={userDdlData.current}
                                                            secondDdlValue={values.MASTER_UNDERTAKER_C}
                                                            secondDdlError={errors.MASTER_UNDERTAKER_C}
                                                            secondColumn={"MASTER_UNDERTAKER_C"}
                                                            secondItemRender={(li, item) => {
                                                                if (item.dataItem.param === "Y") {
                                                                    return React.cloneElement(li, li.props, <span>{li.props.children}</span>)
                                                                }
                                                                else {
                                                                    return <></>
                                                                }
                                                            }}
                                                            getSecondDdlData={(data) => getUserDdlData(data, undertakerType.MASTER, values.MASTER_UNDERTAKER_C, values.MASTER_UNDERTAKER_NAME)}
                                                            values={values}
                                                            setValues={setValues}
                                                        />
                                                    </td>
                                                </tr>
                                                {tableCloumnBlock2(props)}
                                                <tr>
                                                    <th>
                                                        <CommonTooltip title={"協辦機關/人員"} content={"相關配合機關，無則免填"} />
                                                    </th>
                                                    <td colSpan={3}>
                                                        <ProjectAsstOrgGrid
                                                            ddlData={ddlData.ORGAN}
                                                            fillBasicData={projectFillBasicData}
                                                            setEditedGridData={(data) =>
                                                                //紀錄有異動的資料
                                                                setEditedProjectAsstOrg(data)
                                                            }
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th className="addRedStar">
                                                        代辦機關/人員
                                                    </th>
                                                    <td colSpan={3}>
                                                        <div style={{ display: "inline-flex" }}>
                                                            <RadioGroup
                                                                name="BUDGET_HOLD_IS_ENABLE"
                                                                value={values.BUDGET_HOLD_IS_ENABLE}
                                                                onChange={(e) => {
                                                                    let data = {
                                                                        BUDGET_HOLD_IS_ENABLE: e.value
                                                                    }
                                                                    if (!e.value) {
                                                                        data.BUDGET_HOLD_ORGAN_C = "";
                                                                        data.BUDGET_HOLD_UNDERTAKER_C = "";
                                                                    }
                                                                    setValues({ ...values, ...data })
                                                                }}
                                                                layout={"horizontal"}
                                                                data={withOrWithoutData}
                                                            />
                                                            <div style={{ marginLeft: "10px" }}>
                                                                <CascadeDropDown
                                                                    fristDdlData={ddlData.ORGAN}
                                                                    firstDdlValue={values.BUDGET_HOLD_ORGAN_C}
                                                                    firstDdlError={errors.BUDGET_HOLD_ORGAN_C}
                                                                    firstColumn={"BUDGET_HOLD_ORGAN_C"}
                                                                    fristDdlDisabled={!values.BUDGET_HOLD_IS_ENABLE}
                                                                    firstDdlStyle={{ width: '250px' }}
                                                                    secondDdlInitData={userDdlData.current}
                                                                    secondDdlValue={values.BUDGET_HOLD_UNDERTAKER_C}
                                                                    secondDdlError={errors.BUDGET_HOLD_UNDERTAKER_C}
                                                                    secondColumn={"BUDGET_HOLD_UNDERTAKER_C"}
                                                                    secondItemRender={(li, item) => {
                                                                        if (item.dataItem.param === "Y") {
                                                                            return React.cloneElement(li, li.props, <span>{li.props.children}</span>)
                                                                        }
                                                                        else {
                                                                            return <></>
                                                                        }
                                                                    }}
                                                                    getSecondDdlData={(data) => getUserDdlData(data, undertakerType.BUDGET_HOLD, values.BUDGET_HOLD_UNDERTAKER_C, values.BUDGET_HOLD_UNDERTAKER_NAME)}
                                                                    values={values}
                                                                    setValues={setValues}
                                                                />
                                                            </div>
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th className="addRedStar">
                                                        辦理地點
                                                    </th>
                                                    <td colSpan={3}>
                                                        <DropDownListWithValue
                                                            data={ddlData.TOWN_C}
                                                            textField={"text"}
                                                            dataItemKey={"value"}
                                                            value={values.TOWN_C ?? ""}
                                                            error={errors.TOWN_C}
                                                            onChange={(e) => {
                                                                let townC = e.target.value;
                                                                let townM = values.TOWN_M;
                                                                // console.log(townM);
                                                                if (townC != "H01") {
                                                                    setTownMSelData([])
                                                                    townM = "";
                                                                }
                                                                setValues({ ...values, TOWN_C: townC, TOWN_M: townM });
                                                            }}
                                                        />
                                                        {!IsNullOrEmpty(values.TOWN_C) && values.TOWN_C == "H01" &&
                                                            <><MultiSelect
                                                                popupSettings={
                                                                    { className: "dropdown-text-size" }
                                                                }
                                                                placeholder="請選擇"
                                                                name='TOWN_M'
                                                                // 選項僅保留區
                                                                data={ddlData.TOWN_C.filter(x => x.value !== "" && x.value !== "H00" && x.value !== "H01")}
                                                                textField="text"
                                                                dataItemKey="value"
                                                                onChange={(e) => {
                                                                    setValues({
                                                                        ...values,
                                                                        TOWN_M: e.value.map(item => item.value).join(",")
                                                                    })
                                                                    setTownMSelData([...e.value]);
                                                                }}
                                                                value={townMSelData}
                                                            />
                                                                <Error>{errors.TOWN_M}</Error>
                                                            </>}
                                                    </td>

                                                </tr>
                                                <tr>
                                                    <th>
                                                        <CommonTooltip title={"位置說明"} content={"執行地點有明確地址請寫地址，無地址請以路段表示，或以明顯地標、建築物相對位置說明，皆無才使用地號。"} />
                                                    </th>
                                                    <td colSpan={3}>
                                                        (地址或路段或地號)
                                                        <TextInput
                                                            name="PROJECT_LOCATION"
                                                            value={values.PROJECT_LOCATION}
                                                            error={errors.PROJECT_LOCATION}
                                                            onChange={handleChange}
                                                            onBlur={handleBlur}
                                                            style={{ width: "100%" }}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th className="addRedStar">單點地圖定位</th>
                                                    <td colSpan={3}>
                                                        (可拖曳或放大地圖後，直接在地圖上點左鍵，定位計畫主要位置；或輸入座標或地址查詢後定位)
                                                        <br />
                                                        <div style={{ display: "inline-flex" }}>
                                                            坐標 X：
                                                            <TextInput
                                                                name="X_COORD"
                                                                value={values.X_COORD}
                                                                error={errors.X_COORD}
                                                                onChange={handleChange}
                                                                onBlur={handleBlur}
                                                            />
                                                            Y：
                                                            <TextInput
                                                                name="Y_COORD"
                                                                value={values.Y_COORD}
                                                                error={errors.Y_COORD}
                                                                onChange={handleChange}
                                                                onBlur={handleBlur}
                                                            />
                                                            <div className="fn-buttons" style={{ marginLeft: "10px" }}>
                                                                <Button title="查詢地點" type="button" onClick={() => findLocationByXY(values.X_COORD, values.Y_COORD)}>查詢地點</Button>
                                                            </div>
                                                        </div>
                                                        <br />
                                                        <div style={{ display: "inline-flex", width: "80%" }}>
                                                            地址：
                                                            <TextInput
                                                                value={address}
                                                                onChange={(e) => setAddress(e.value)}
                                                                style={{ width: "40%" }}
                                                            />
                                                            <div className="fn-buttons" style={{ marginLeft: "10px" }}>
                                                                <Button title="查詢地點" type="button" onClick={() => findLocationByAddr(address, props)}>查詢地點</Button>
                                                            </div>
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colSpan={4} align="center">
                                                        <div style={{ width: '700px', height: '350px' }}>
                                                            <GoogleMapReact
                                                                bootstrapURLKeys={{ key: mapDefaultProps.key }}
                                                                options={mapOption}
                                                                defaultCenter={mapDefaultProps.center}
                                                                defaultZoom={mapDefaultProps.zoom}
                                                                yesIWantToUseGoogleMapApiInternals // 設定為 true
                                                                onGoogleApiLoaded={({ map, maps }) => apiHasLoaded(map, maps)}
                                                                // 移動地圖邊界時觸發 handleCenterChange
                                                                onBoundsChange={(e) => handleCenterChange(props)}
                                                            />
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>
                                                        多點地圖定位
                                                    </th>
                                                    <td colSpan={3}>
                                                        <div className="fn-buttons">
                                                            {
                                                                // 立案審核通過後才顯示
                                                                Number(projectStatus.current) >= 4 &&
                                                                <Button title="檢視點位" type="button" onClick={() => pointClickEvent(values, 0)}>檢視點位</Button>
                                                            }
                                                            {
                                                                // 執行情形 設定點位不能顯示
                                                                Number(projectStatus.current) < 4 &&
                                                                <Button title="設定點位" type="button" onClick={() => pointClickEvent(values, 1)}>設定點位</Button>
                                                            }
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>
                                                        <CommonTooltip title={"計畫內容"} content={"具體說明執行內容，如總體工作要項、規模等。"} />
                                                    </th>
                                                    <td colSpan={3}>
                                                        <PureHtmlTextAreaInput
                                                            rows={6}
                                                            name='ALL_JOB'
                                                            value={values.ALL_JOB}
                                                            error={errors.ALL_JOB}
                                                            onChange={handleChange}
                                                            onBlur={handleBlur}
                                                            style={{ width: '100%' }}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>
                                                        <CommonTooltip title={"計畫效益"} content={"預期成果或效益，如提升或改善事項。"} />
                                                    </th>
                                                    <td colSpan={3}>
                                                        <PureHtmlTextAreaInput
                                                            rows={6}
                                                            name='PROJECT_BENEFIT'
                                                            value={values.PROJECT_BENEFIT}
                                                            error={errors.PROJECT_BENEFIT}
                                                            onChange={handleChange}
                                                            onBlur={handleBlur}
                                                            style={{ width: '100%' }}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th >
                                                        備註
                                                    </th>
                                                    <td colSpan={3}>
                                                        <PureHtmlTextAreaInput
                                                            rows={6}
                                                            name='MEMO'
                                                            value={values.MEMO}
                                                            error={errors.MEMO}
                                                            onChange={handleChange}
                                                            onBlur={handleBlur}
                                                            style={{ width: '100%' }}
                                                        />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>立案時間</th>
                                                    <td colSpan={3}>
                                                        {values.CREATEDTIME == null ?
                                                            "" : FormatDate(values.CREATEDTIME, 'tYY/MM/DD HH:mm')}
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>立案審核意見</th>
                                                    <td colSpan={3}>{projLogCell(values, ["3", "4"])}</td>
                                                </tr>
                                                <tr>
                                                    <th>基本資料成績</th>
                                                    <td colSpan={3}>
                                                        {values.SCORE_A}
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>結案審核意見</th>
                                                    <td colSpan={3}>{projLogCell(values, ["6", "7", "8"])}</td>
                                                </tr>
                                            </table>
                                        </form>
                                    );
                                }}
                            </Formik>
                        </CollapseBoardCard>
                )
                    : null
            }
        </>
    );
}

export default ProjectFillBasicMain;
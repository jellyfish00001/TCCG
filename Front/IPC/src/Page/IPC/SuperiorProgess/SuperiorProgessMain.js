import React from 'react';
import { PageContainer } from '../../../Basic/PageContainer';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { CheckIsHANDRole } from '../../../Basic/CommonService';
import { Button } from '@progress/kendo-react-buttons';

import Service from './SuperiorProgessService';
import ProgessQueryForm from './ProgessQueryForm';
import MapPanel from './MapPanel';
import MapGrid from './MapGrid';

const SuperiorProgessMain = (props) => {

    const formData = React.useRef({
        BUILD_KIND: [], // 建設類別
        MASTER_DEPT: "", // 主管機關
        EXEC_DEPT: "", // 執行機關
        IS_HAND_ROLE: false, // 是否只有主辦權限
    });

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState({
        MASTER_ORGAN: [], // 主管機關
        EXEC_ORGAN: [], // 執行機關
        COM_PLANKIND: [], // 計畫年度
    });
    // 是否顯示內容
    const [isVisible, setIsVisible] = React.useState(false);
    // Grid資料
    const [gridData, setGridData] = React.useState([]);
    const [filterTownName, setFilterTownName] = React.useState("");
    // Map資料
    const [mapData, setMapData] = React.useState([]);
    // Formik innerRef
    const formRef = React.useRef(null);

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = async () => {
        await getAllDropDowns();
    }

    // 取得下拉清單
    const getAllDropDowns = async () => {
        SetMaskOnOff(true);
        const checkIsHANDRole = await CheckIsHANDRole();
        const data = await Service.getAllDropDowns(checkIsHANDRole);
        setDdlData({ ...ddlData, MASTER_ORGAN: data[0], EXEC_ORGAN: data[0], COM_PLANKIND: data[1] });
        formData.current.IS_HAND_ROLE = checkIsHANDRole;
        SetMaskOnOff(false);
    }

    // 查詢條件
    const onSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    }

    // 取資料
    const getData = async (data) => {
        SetMaskOnOff(true);
        setIsVisible(false);
        const rdata = await Service.getProgess(data);
        setGridData([...rdata]);
        setMapData([...Service.getMapData(rdata)]);
        setFilterTownName("");
        setIsVisible(true);
        SetMaskOnOff(false);
    }

    // 清除
    const onClear = () => {
        if (formRef.current) {
            formRef.current.handleReset();
            setGridData([]);
            setMapData([]);
            setIsVisible(false);
        }
    }

    return (
        <PageContainer
            toolbar={
                <>
                    <h3 className='k-dialog-titlebar'>決策分析 – 建設類別查詢</h3>
                    <Button className='k-button-lighten' onClick={() => onSubmit()}>查詢</Button>
                    <Button className='k-button-lighten' onClick={() => onClear()}>清除</Button>
                    <Button className='k-button-lighten' onClick={() => window.print()}>列印</Button>
                </>
            }
        >
            <ProgessQueryForm
                ddlData={ddlData}
                formData={formData}
                formRef={formRef}
                getData={getData}
            />

            {
                isVisible &&
                <div style={{ overflow: 'auto', height: 'Calc(100% - 125px)' }}>
                    <div style={{ display: 'flex' }}>
                        <MapPanel
                            mapData={mapData}
                            setFilterTownName={setFilterTownName}
                        />

                        <MapGrid
                            gridData={gridData}
                            filterTownName={filterTownName}
                        />
                    </div>
                </div>
            }
        </PageContainer>
    )
}

export default SuperiorProgessMain;
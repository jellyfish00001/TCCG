import { Window } from '@progress/kendo-react-dialogs';
import { api } from "../../../Basic/ApiFetch";
import { getGlobalServerConfig } from "../../../Route/RootMiddleware";
import React, { useEffect, useState, useImperativeHandle, forwardRef } from 'react';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import ProjectFillBasicMain from '../ProjectFillBasic/ProjectFillBasicMain';
import { GetSetParam } from '../../../Basic/CommonService';
import { Checkbox } from '@progress/kendo-react-inputs';
import { Button } from '@progress/kendo-react-buttons';
import { Download } from '../../../Basic/Download';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';

// 進度甘特圖報表
export const ProjectScheduleWindow = forwardRef((props, ref) => {
    const dimensions = WindowResizehook();
    let { projectNo, checkedItems, projectAwStatus } = props;
    let necessaryObj = {
        location: {
            state: {},
        }
    }
    let APIUrl = getGlobalServerConfig().backEndUrl.get();

    useImperativeHandle(ref, () => ({
        open() {
            setvisible(true);
        }
    }));

    const [scheTypeData, setScheTypeData] = useState([])
    const [visible, setvisible] = useState(false)

    // 取得Checkbox資料
    const loadData = async () => {
        let scheTypes = await GetSetParam('SCHE_TYPE_FOR_RPT');
        if (checkedItems && checkedItems.length > 0) {
            scheTypes = scheTypes.map(x => {
                x.isChecked = checkedItems.includes(x.SET_TYPE);
                return { ...x }
            });
        } else {
            scheTypes = scheTypes.map(x => {
                // 預設勾選「現行預定進度」、「實際執行進度」
                x.isChecked = x.SET_TYPE == "3" || x.SET_TYPE == "5";
                return { ...x }
            });
        }
        // 「本次申請調整之期程」項目 僅期程調整審核中才顯示
        if (projectAwStatus != "B02") {
            scheTypes = scheTypes.filter(x => x.SET_TYPE != "4");
        }
        setScheTypeData(scheTypes);
    }

    /**
     * 取得工程類計畫清單
     * @returns 
     */
    const getEngineeringProjects = async () => {
        let url = APIUrl + 'RPT/getEngineeringProjects';
        let response = await api.Post(url);
        let result = [];
        if (response.ok) {
            result = await response.json();
        }
        return result;
    }

    // 下載報表
    const downloadRpt = async () => {
        let checkedItems = scheTypeData.filter(x => x.isChecked).map(x => x.SET_TYPE);
        let url = APIUrl + 'RPT/ProjectScheduleOverview';
        let requestBody = {
            PROJECT_NO: projectNo,
            ScheTypes: checkedItems
        }
        Download(url, 'POST', requestBody);
    }

    useEffect(() => {
        loadData()
    }, [])

    useEffect(() => {
        loadData()
    }, [projectAwStatus])

    return (
        <>
            {visible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title='進度甘特圖'
                        onClose={() => setvisible(false)}
                        initialWidth={550}
                        initialHeight={180}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <Button type="button" onClick={() => downloadRpt()}>產出</Button>
                        <div>
                            <div>進度甘特圖項目 </div>
                            {scheTypeData.map(item => {
                                return (
                                    <Checkbox
                                        label={item.SET_VALUE}
                                        value={item.SET_TYPE}
                                        checked={item.isChecked}
                                        onChange={(e) => {
                                            let targetItem = { ...item, isChecked: e.value };
                                            let index = scheTypeData.findIndex(x => x.SET_TYPE === item.SET_TYPE);
                                            scheTypeData.splice(index, 1, targetItem);
                                            setScheTypeData([...scheTypeData]);
                                        }} />
                                )
                            })}
                        </div>
                    </Window>
                </div>
            }
        </>
    )
})
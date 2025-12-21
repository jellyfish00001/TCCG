import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { SetHistory, SignInStatusContext } from '../../../Basic/BasicData';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import "../../../Css/ProjectPrint.css";
import { getDiffCompare, downProjectAdjustContent } from './ProjectAdjustContentService';
import { GetStorageData } from '../../../Basic/CommonService';
import ReactHtmlParser from 'react-html-parser';

const ProjectAdjustContentMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            /**傳入的參數 */
            state: {
                awKind,
                projAdjId,
                projectNo,
                projectName,
                isRdecFun,
                logId,
            } = {
                awKind: "",
                projAdjId: 0,
                projectNo: "",
                projectName: "",
                isRdecFun: "",
                logId: 0
            }
        }
    } = props;

    const { signInStatus, setSignInStatus } = React.useContext(SignInStatusContext);

    // 資料是否載入完成
    const [isDone, setIsDone] = React.useState(false);
    // 差異比對結果
    const [diffCompareResult, setDiffCompareResult] = React.useState("");

    /**
     * 取得差異比對參數
     * @param {*} extension 
     * @returns 
     */
    const getDiffCompareParam = (extension) => {
        const data = {
            PROJECT_NO: projectNo,
            Extension: extension,
            Type: awKind,
            IsDiffCompare: true,
            LOG_ID: [logId, projAdjId]
        }
        return data;
    }

    // 取得預覽資料
    const loadData = async () => {
        let result = await getDiffCompare(getDiffCompareParam("html"));
        setDiffCompareResult(result);
        setIsDone(true);
    }

    // 設定 location state
    const SetLocationState = () => {
        let data = GetStorageData("printInfo");
        props.location.state = data;
    }

    React.useEffect(() => {
        SetHistory(props.history);
        SetLocationState();
        if (!signInStatus) {
            setSignInStatus();
        }
    }, []);

    React.useEffect(() => {
        if (!IsNullOrEmpty(projectNo)) {
            loadData();
        }
    }, [projectNo]);

    /**
     * 下載報表
     * @param {*} extension 
     */
    const downRpt = (extension) => {
        downProjectAdjustContent(getDiffCompareParam(extension));
    }

    return (
        <>
            <div className="navOpen page">

                <CollapseBoardCard button={
                    <div style={{ display: "flex" }}>
                        <Button title="Print" className="export-Print" onClick={() => { window.print(); }} />
                        <Button title="DOCX" className="export-DOCX" onClick={() => { downRpt("DOCX") }} />
                        <Button title="PDF" className="export-PDF" onClick={() => { downRpt("PDF") }} />
                        <Button title="ODT" className="export-ODT" onClick={() => { downRpt("ODT") }} />
                    </div>
                } title={projectName} isFirstArea={true}>
                    <div className='printResult'>
                        {isDone && ReactHtmlParser(diffCompareResult) }
                    </div>
                </CollapseBoardCard>

            </div>
        </>
    );
}

export default ProjectAdjustContentMain;
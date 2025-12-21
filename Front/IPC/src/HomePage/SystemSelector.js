import React from 'react';
import ReactHtmlParser from 'react-html-parser';
import 'bootstrap/dist/css/bootstrap.min.css';
import '../Css/Home.css';
import { openProjectChapter } from '../Page/IPC/ProjectList/ProjectListService';
import { GetBasicData, SignInStatusContext } from '../Basic/BasicData';
import { GetScLink, GetSingleSetParam } from '../Basic/CommonService';
import { IsNullOrEmpty, SetMaskOnOff, openPage } from '../Basic/SDOExtension';
import { api } from '../Basic/ApiFetch';
import { getGlobalServerConfig } from '../Route/RootMiddleware';
import { WindowResizehook } from '../Hook/useWindowResize';
import { Window } from '@progress/kendo-react-dialogs';

import icon01 from '../Images/icon01.png';
import icon02 from '../Images/icon02.png';
import icon03 from '../Images/icon03.png';
import icon04 from '../Images/icon04.png';
import icon05 from '../Images/icon05.png';
import icon06 from '../Images/icon06.png';
import icon07 from '../Images/icon07.png';

const SystemSelector = (props) => {
  const SDOUrl = getGlobalServerConfig().backEndUrl.get();

  const { setSignInStatus, signInStatus } = React.useContext(SignInStatusContext);
  const dimensions = WindowResizehook();
  // 使用者擁有的子系統權限
  const [apRole, setApRole] = React.useState({
    isPtms: false,
    isPtms2: false,
    isNDPMpc: false,
    isADPApc: false,
    isPPE: false,
    isPWSSD: false,
    isAdmin: false,
    isRd2: false,
    isInn: false
  })

  // 重大建設
  const [isIPCRoles, setIsIPCRoles] = React.useState(false);
  // 使用者擁有權限等級
  const [userRoles, setUserRoles] = React.useState([]);
  const [projectListData, setProjectListData] = React.useState([]);

  // 公告資訊
  const [scAnnData, setScAnnData] = React.useState([]);
  const [showScAnn, setShowScAnn] = React.useState({
    Visible: false,
    Title: "",
    Comment: "",
    AnnDate: ""
  });

  // 本日到期案件 / 議會案件 / 專案追蹤
  const [trackoData, setTrackoData] = React.useState({
    OverParliamentProjs: [],
    OverTrackProjs: [],
    ParliamentProjs: [],
    TrackProjs: [],
    IsNewVersion: false
  });

  // SC資訊
  const [scData, setScData] = React.useState({
    PwssdCnt: 0,
    RdMaxYear: "",
    RdRpmCnt: 0,
    RdWipCnt: 0
  });

  React.useEffect(() => {
    // 載入選單資料
    loadData();
  }, [])

  // 載入系統選單資料
  const loadData = async () => {
    SetMaskOnOff(true);

    setSignInStatus();

    let roles = await GetBasicData("roles");
    if (roles.length > 0) {
      setIsIPCRoles(true);
      // 重大建設
      await getProjectList();
    }

    let allRoleList = await GetBasicData("allRoles");

    let apRoles = [];
    allRoleList.forEach(x => {
      if (!apRoles.includes(x.AP_ID)) {
        apRoles.push(x.AP_ID)
      }
    });

    // 議會
    const isPtms = apRoles.includes("PA") || apRoles.includes("PTMS");
    // 追蹤
    const isPtms2 = apRoles.includes("PMS") || apRoles.includes("PTMS2");
    // 中程
    const isNDPMpc = apRoles.includes("NDPMpc");
    // 年度
    const isADPApc = apRoles.includes("ADPApc");
    // 績效
    const isPPE = apRoles.includes("PPE");
    // 先期
    const isPWSSD = apRoles.includes("PWS");
    // 施政
    const isAdmin = isNDPMpc && isADPApc && isPPE && isPWSSD;
    // 研究發展
    const isRd2 = apRoles.includes("RD2");
    // 創新提案
    const isInn = apRoles.includes("INN");

    setApRole({ isPtms, isPtms2, isNDPMpc, isADPApc, isPPE, isPWSSD, isAdmin, isRd2: isRd2, isInn: isInn })

    setUserRoles(allRoleList.map(x => x.ROLE_ID));

    // 公告清單
    await getScAnnouncement();
    // 本日到期案件 / 議會案件 / 專案追蹤
    await getTrackoData();
    // SC資訊
    await getScData();
    SetMaskOnOff(false);
  }

  // 重大建設
  const getProjectList = async () => {
    let defaultConditions = {
      //預設計畫狀態為 1：立案中、3：立案退回、4：執行情形 、6：結案退回
      PROJECT_STATUS: ['1', '3', '4', '6'],
      // 年度為當年
      PROJECT_YEAR: new Date().getFullYear() - 1911
    }
    let url = SDOUrl + 'ProjectList/GetProjectList';
    let response = await api.Post(url, JSON.stringify(defaultConditions));
    let data = [];
    if (response.ok) {
      data = await response.json();
    }

    // 只顯示主管機關或執行機關為自己機關的計畫
    let orgId = await GetBasicData("orgId");
    setProjectListData(data.length > 0
      ? [...data.filter(x => x.EXEC_ORGAN_C === orgId)]
      : []);
  }

  // 公告清單
  const getScAnnouncement = async () => {
    let url = SDOUrl + 'Announcement/GetScAnnouncement';
    let response = await api.Post(url);
    if (response.ok) {
      setScAnnData(await response.json());
    }
  }

  // 本日到期案件 / 議會案件 / 專案追蹤
  const getTrackoData = async () => {
    let url = SDOUrl + 'Tracko/GetTrackoData';
    let response = await api.Post(url);
    if (response.ok) {
      setTrackoData(await response.json());
    }
  }

  // SC資訊
  const getScData = async () => {
    let orgId = await GetBasicData("orgId");
    let url = SDOUrl + 'ScApplication/GetScData/' + orgId;
    let response = await api.Get(url, null, new Headers(), true);
    if (response.ok) {
      setScData(await response.json());
    }
  }

  // 連結SC網址
  const onClickScLink = async (dominName, apId, otherParam = "") => {
    let url = await GetScLink(dominName, apId);
    url += (!IsNullOrEmpty(otherParam) ? '&' : '') + otherParam;

    openPage(url, `${dominName}_${apId}`);

    //紀錄登入紀錄
    loginLog(apId);
  }

  // 開啟子系統
  const openSubSystem = async (apId) => {
    openPage(`${window.location.origin}/${apId}/Home`);
    //紀錄登入紀錄
    loginLog(apId);
  }

  // 紀錄系統登入紀錄
  const loginLog = async (apId) => {
    let url = SDOUrl + 'Login/LoginLog';
    let response = await api.Post(url, JSON.stringify(apId), null, false);
    let result = {};
    if (response.ok) {
      result = await response.json();
    }

    if (!result.success) {
      console.log(result.message);
    }
  }

  // 取得照片
  const getImg = (iconSrc) => {
    return (
      <div className="icon">
        <img src={iconSrc} alt="" />
      </div>
    )
  }

  // 設定 本日到期案件 / 議會案件 / 專案追蹤 HTML
  const setToDoListData = (item, ownRole) => {
    if (item.TotalAmount > 0) {
      if (ownRole) {
        return (
          <li>
            <a href="/" onClick={(e) => { e.preventDefault(); onClickTracko(item); }}>
              <div className="title" style={{ maxWidth: "80%" }}>{item.ProjectName}</div>
              <div className="menu-line"></div>
              <div className="btn btn-primary iconBtn">{item.TotalAmount}</div>
            </a>
          </li>
        )
      }
      else {
        return (
          <li>
            <div className="title" style={{ maxWidth: "80%" }}>{item.ProjectName}</div>
            <div className="menu-line"></div>
            <div className="btn btn-primary iconBtn">{item.TotalAmount}</div>
          </li>
        )
      }
    }
    else {
      return (
        <li>
          <div className="title">{item.ProjectName}</div>
        </li>
      )
    }
  }

  // 連結 Tracko 網址
  const onClickTracko = async (item) => {
    // PTMS：TrackoParliamentUrl(議會案件); PTMS2：TrackoTrackUrl(追蹤案件)
    let setParam = await GetSingleSetParam("SystemConfig", item.AP_ID === "PTMS" ? "TrackoParliamentUrl" : "TrackoTrackUrl");
    let url = `${setParam.SET_VALUE}/System/RISRedirect?`;
    url += `userId=${await GetBasicData("userId")}&ProjectNo=${item.ProjectNo}&WorkPeriod=${item.WorkPeriod}`;
    openPage(url, item.AP_ID);
  }

  // 連結 Tracko 網址 (標題)
  const onClickTrackoByTitle = async (AP_ID) => {
    // PTMS：TrackoParliamentUrl(議會案件); PTMS2：TrackoTrackUrl(追蹤案件)
    let setParam = await GetSingleSetParam("SystemConfig", AP_ID === "PTMS" ? "TrackoParliamentUrl" : "TrackoTrackUrl");
    let url = `${setParam.SET_VALUE}/PTMSSSO/TYCGLoginSSO?`;
    url += `uid=${await GetBasicData("userId")}`;
    openPage(url, AP_ID);
  }

  return (
    <div className='portal' style={{ overflow: "auto", height: `${dimensions.height - 100}px` }}>
      <div className='portal_grid'>

        {/* 重大建設 */}
        <div className='gridItem item1'>
          {
            isIPCRoles
              ? <a href='/' onClick={(e) => { e.preventDefault(); openPage(`${process.env.PUBLIC_URL}/Home`, 'IPCHome', '重大建設系統', null); }}>
                {getImg(icon01)}
                <h1>重大建設</h1>
              </a>
              : <>{getImg(icon01)}<h1>重大建設</h1></>
          }
          <ul>
            {
              projectListData.map(x => {
                return (
                  <li>
                    <a href='/' onClick={(e) => { e.preventDefault(); openProjectChapter(x, 0); }}>
                      {x.PROJECT_NAME}({x.PROJECT_STATUS})
                    </a>
                  </li>
                )
              })
            }
          </ul>
        </div>

        {/* 本日到期案件 */}
        <div className='gridItem'>
          {getImg(icon02)}
          <h1>本日到期案件<span style={{ fontSize: "10px" }}>（含已逾期）</span></h1>
          <ul>
            <div className='sort'>議會案件</div>
            {trackoData.OverParliamentProjs.map(x => setToDoListData(x, apRole.isPtms))}

            <div className='sort'>專案追蹤</div>
            {trackoData.OverTrackProjs.map(x => setToDoListData(x, apRole.isPtms2))}
          </ul>
        </div>

        {/* 最新公告 */}
        <div className='gridItem'>
          {getImg(icon03)}
          <h1>最新公告</h1>
          <ul>
            {
              scAnnData.map(x => {
                return (
                  <li>
                    <a href='/' onClick={(e) => {
                      e.preventDefault();
                      setShowScAnn({ ...showScAnn, Visible: true, Title: x.TITLE, Comment: x.COMMENT, AnnDate: x.EFFECTIVE_TWDATE })
                    }}>
                      <div className="date">{x.EFFECTIVE_TWDATE}</div>
                      <div className="title">{x.TITLE}</div>
                    </a>
                  </li>
                )
              })
            }
          </ul>
        </div>

        {/* 議會案件 */}
        <div className='gridItem'>
          {
            apRole.isPtms
              ? <a href='/' onClick={(e) => {
                e.preventDefault();
                // 判斷是否使用新版本
                trackoData.IsNewVersion ? onClickTrackoByTitle("PTMS") : onClickScLink('OLDRIS', 'PA', 'AI=PA')
              }}>
                {getImg(icon04)}
                <h1>議會案件</h1>
              </a>
              : <>{getImg(icon04)}<h1>議會案件</h1></>
          }
          <ul>
            {trackoData.ParliamentProjs.map(x => setToDoListData(x, apRole.isPtms))}
          </ul>
        </div>

        {/* 專案追蹤 */}
        <div className='gridItem'>
          {
            apRole.isPtms2
              ? <a href='/' onClick={(e) => { e.preventDefault(); onClickTrackoByTitle("PTMS2") }}>
                {getImg(icon05)}
                <h1>專案追蹤</h1>
              </a>
              : <>{getImg(icon05)}<h1>專案追蹤</h1></>
          }
          <ul>
            {trackoData.TrackProjs.map(x => setToDoListData(x, apRole.isPtms2))}
          </ul>
        </div>

        {/* 施政計畫 */}
        <div className='gridItem'>
          {
            apRole.isAdmin
              ? <a href='/' onClick={(e) => {
                e.preventDefault();
                openSubSystem('PWS');
              }}>
                {getImg(icon06)}
                <h1>施政計畫</h1>
              </a>
              : <>{getImg(icon06)}<h1>施政計畫</h1></>
          }
          <ul>
            <li>
              {
                apRole.isPWSSD
                  ? <a href="/" onClick={(e) => {
                    e.preventDefault();
                    openSubSystem('PWS');
                  }}>
                    <div className="title">先期計畫</div>
                  </a>
                  : <div className="title">先期計畫</div>
              }
            </li>

          </ul>
        </div>
        <div className='gridItem'>
          {
            apRole.isRd2
              ? <a href='/' onClick={(e) => {
                e.preventDefault();
                openSubSystem('RD')
              }}>
                {getImg(icon07)}
                <h1>研究發展</h1>
              </a>
              : <>{getImg(icon07)}<h1>研究發展</h1></>
          }
          <ul>
            <li>
              {

                apRole.isRd2
                  ? <a href="/" onClick={(e) => {
                    e.preventDefault();
                    openSubSystem('RD')
                  }}>
                    <div className="title">委託研究</div>
                  </a>
                  : <div className="title">委託研究</div>
              }
            </li>
            <li>
              {
                apRole.isInn && (userRoles.includes("HAND_USER_ROL_INN") || userRoles.includes("RDEC_MGR_ROL_INN"))
                  ? <a href="/" onClick={(e) => {
                    e.preventDefault();
                    let url = `${window.location.origin}/INN/Home/`;
                    url += "ProjectProposal"
                    openPage(url);
                    //紀錄登入紀錄
                    loginLog("INN");
                  }}>
                    <div className="title">創新提案</div>
                  </a>
                  : <div className="title">創新提案</div>
              }
            </li>
          </ul>
        </div>
        {
          showScAnn.Visible &&
          <div className="fullscreen window-fullscreen">
            <Window
              title={"最新公告"}
              onClose={() => setShowScAnn({ ...showScAnn, Visible: false, Title: "", Comment: "", AnnDate: "" })}
              width={dimensions.width * 0.7}
              height={dimensions.height * 0.55}
              draggable={false}
              resizable={false}
              modal={true}
            >
              <table className="table">
                <tr>
                  <th style={{ width: "15%", fontSize: '16px' }}>公告日期</th>
                  <td style={{ fontSize: '16px' }}>{showScAnn.AnnDate}</td>
                </tr>
                <tr>
                  <th style={{ fontSize: '16px' }}>標題</th>
                  <td style={{ fontSize: '16px' }}>{showScAnn.Title}</td>
                </tr>
                <tr>
                  <th style={{ fontSize: '16px' }}>內容</th>
                  <td style={{ fontSize: '16px' }}>{ReactHtmlParser(showScAnn.Comment)}</td>
                </tr>
              </table>
            </Window>
          </div>
        }
      </div>
    </div>
  );
}


export default SystemSelector;
import React from 'react';
import { api } from '../Basic/ApiFetch';
import { useTranslation } from 'react-i18next';
import { getGlobalServerConfig } from '../Route/RootMiddleware';
import { GetHistory, HeaderFooterContext, SignInStatusContext } from '../Basic/BasicData';
import { GetSingleSetParam, GetScLink } from '../Basic/CommonService';
import { openPage } from '../Basic/SDOExtension';

const Foot = (props) => {
  const SDOUrl = getGlobalServerConfig().backEndUrl.get();

  const { signInStatus } = React.useContext(SignInStatusContext)
  const { headerFooterStatus } = React.useContext(HeaderFooterContext);
  const [footData, setFootData] = React.useState("");
  const [risUrl, setRisUrl] = React.useState("");
  const { t } = useTranslation();

  // 取得版本號
  const getFootData = async () => {
    let request = new Request(SDOUrl + 'SystemInfo', {
      method: 'GET',
      headers: { 'Content-Type': 'text/plain' }
    });

    let response = await api.Fetch(request, true, false)
    let responseData = [];
    if (response) {
      responseData = await response.text();
    }

    // 接到request data後要做的事情
    setFootData(responseData);
  }

  const getRisUrl = async () => {
    let setParam = await GetSingleSetParam('DOMAIN_NAME', 'OLDRIS');
    setRisUrl(setParam.SET_VALUE);
  }

  // 連結 SC
  const onClickScLink = async (dominName, apId) => {
    let url = await GetScLink(dominName, apId);
    openPage(url, `${dominName}_${apId}`);
  }

  React.useEffect(() => {
    if (signInStatus && !headerFooterStatus) {
      getFootData();
      getRisUrl();
    }
  }, [signInStatus])

  return (
    <footer className={(signInStatus && !headerFooterStatus) ? "" : "chapter-footer"}>
      <div style={{ display: "flex", justifyContent: "space-between" }}>
        <div style={{ height: '20px' }}>
          <p>客服專線：(02)2586-7890轉10258</p>
        </div>
        <div style={{ height: '20px' }}>
          <ul>
            <li>
              <a href="/" onClick={(e) => { e.preventDefault(); openPage(`${process.env.PUBLIC_URL}/UserGuide`, 'UserGuide'); }}>操作手冊</a>
            </li>
            <li>
              <a href="/" onClick={(e) => { e.preventDefault(); onClickScLink('RISSCnet', 'SC32'); }}>系統管理</a>
            </li>
            {/* <li>
              <a href="/" onClick={(e) => { e.preventDefault(); onClickScLink('OLDRIS', 'CBS'); }}>綜合查詢</a>
            </li> */}
            <li>
              <a href="/" onClick={(e) => { e.preventDefault(); onClickScLink('OLDRIS', 'GB'); }}>線上叫修</a>
            </li>
          </ul>
        </div>
      </div>
    </footer>
  );
}
export default Foot;
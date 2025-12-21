import React, { useEffect } from 'react';
import { withRouter } from 'react-router-dom';
import 'bootstrap/dist/css/bootstrap.min.css';
import { useTranslation } from 'react-i18next';
import CacheLoader from '../Basic/CacheLoader';
import  {  GetHistory } from '../Basic/BasicData';

const Home = (props) => {
  const { t } = useTranslation();

  //檢查驗證cookie是否存在
  useEffect(()=>{
    if(!CacheLoader().HasCache()){
      GetHistory().push('/');
    }

  },[])

  return (
      <div className="fnForm">
        <div
          className="expand">
          {/* 結果顯示區域 */}
          {props.children}
        </div>
      </div>
  );
}


export default withRouter(Home);
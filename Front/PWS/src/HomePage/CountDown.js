/**
 * @typedef {{
 *          logic:Object,
 *          filters:Array.<{
 *                          field:string,
 *                          operator:string|function,
 *                          value?: any,
 *                          ignoreCase?:boolean
 *                        }>
 *       }} filter
 */

import React, { useState, useEffect, useRef } from 'react';
import { getGlobalServerConfig } from '../Route/RootMiddleware';
import { openPage } from '../Basic/SDOExtension';
import { api } from '../Basic/ApiFetch';
import { useHookstate, Downgraded } from '@hookstate/core';
import ReactCountdown from 'react-countdown';
import { formatNumber } from '@telerik/kendo-intl';
import { Button } from '@progress/kendo-react-buttons';

export const Countdown = (props) => {
  /**@type {object} */
  const ServerConfig = useHookstate(props.ServerConfig);
  ServerConfig.attach(Downgraded);

  const renderer = ({ minutes, seconds }) => {
    return <div className={'countDownText'}>
      <i className="fa fa-clock-o" style={{ marginRight: "5px" }} />
      自動登出
      {formatNumber(minutes, '00')}:
      {formatNumber(seconds, '00')}
      <Button type="button" onClick={(e) => { setTimerTime() }} style={{ marginLeft: "5px" }}>
        <i className="fa fa-history" />
      </Button>
    </div >
  };

  const timer = useRef(null)
  const key = useRef(Math.random().toString().substr(2, 8))
  const [date, setDate] = useState(localStorage.getItem('expires'));

  useEffect(() => {
    if (timer) {
      timer.current.start();
    }
  }, [getGlobalServerConfig().cacheExpire.get()]);

  const setTimerTime = async () => {
    await api.Get(getGlobalServerConfig().backEndUrl.get() + 'Login/LoadBasic');

    localStorage.removeItem('RestCountDown');
    localStorage.removeItem('IsOpenRCD');
  }

  return (
    <>
      <ReactCountdown
        key={key.current}
        ref={timer}
        date={date}
        renderer={renderer}
        onComplete={() => props.signOut()}
        onTick={(e) => {
          // 重新計時
          if (localStorage.getItem("RestCountDown") === "Y") {
            setTimerTime();
            return;
          }
          // 若"有效期限"更新，到數計時也更新
          if (localStorage.getItem('expires') !== date) {
            setDate(localStorage.getItem('expires'))
            key.current = Math.random().toString().substr(2, 8)
            return
          }

          //利用tick算出正確秒數
          let newTime = (e.minutes * 60) + e.seconds;
          let c = getGlobalServerConfig().countDown.get();
          let exp = getGlobalServerConfig().cacheExpire.get();
          //檢查如果跟cacheExpire一樣更新key來強制重新計時
          if (c === exp)
            key.current = Math.random().toString().substr(2, 8)
          //每次api call成功會更新countDown
          getGlobalServerConfig().countDown.set(newTime);

          // 剩下1分鐘跳出視窗，有開過就不開了
          if (newTime === 60 && localStorage.getItem("IsOpenRCD") == null) {
            // 是否有開啟過"重新計時"視窗
            localStorage.setItem("IsOpenRCD", "Y");
            let url = process.env.PUBLIC_URL + '/ResetCountDown?time=' + newTime;
            openPage(url, '_blank', '', true, 400, 290)
          }
        }}
      />
    </>
  );
}
export default Countdown;
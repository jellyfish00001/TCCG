import React from 'react';

export const Query = () => {
    return(
        <>
            <h2>appsettings.json</h2>
            <h4>
                ConnectionStrings DB連線設定
            </h4>
            <pre>
                {'"ConnectionStrings": {'}{'\n'}
                {'    "MainDBConnection": "Driver={SQL Server};Server={Server Name};UID={SQL Login ID};PWD={SQL Login Password};Database={Database Name};"'}{'\n'}
                {'}'}{'\n'}
            </pre>
            <h4>
                TokenSetting Token(驗證)設定
            </h4>
            <pre>
                {'"TokenSetting": {'}{'\n'}
                {'   "TokenRefresh": true,        //Token是否於每次使用後刷新'}{'\n'}
                {'   "TokenExpireTime": 1800,     //Token有效時長(單位:秒)'}{'\n'}
                {'}'}{'\n'}
            </pre>
            <h4>
                ShowErrorMsg 錯誤訊息顯示
            </h4>
            <pre>
                {'"ShowErrorMsg": false           //系統發生錯誤時是否回傳詳細錯誤訊息'}{'\n'}
            </pre>
            <h4>
                CacheSetting
            </h4>
            <pre>
                {'"CacheSetting": {'}{'\n'}
                {'   "CacheConnection": "Server={Server Name};UID={SQL Login ID};PWD={SQL Login Password};Database={Database Name};",   //分散式快取DB連線資訊'}{'\n'}
                {'   "CacheSchemaName": "dbo",     //SchemaName'}{'\n'}
                {'   "CacheTableName": "CACHE",     //TableName'}{'\n'}
                {'   "CacheExpireTime": 1800,     //Cache有效時長(單位:秒)'}{'\n'}
                {'}'}{'\n'}
            </pre>
        </>
    )
}
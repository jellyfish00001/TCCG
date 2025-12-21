import React from 'react';

const DocumentItem = props =>{
    return(
        <>
            <h4>{props.title}</h4>
            <pre>
                {props.children}
            </pre>
        </>
    )
}

export const Query = () => {
    return(
        <>
            <h2>Dac</h2>
            <DocumentItem title={'IDac注入'}>
                {'private readonly IDac dac;'}{'\n'}
                {'public constructor(IDac dac){'}{'\n'}
                {'    this.dac=dac;'}{'\n'}
                {'}'}{'\n'}
            </DocumentItem>
            <DocumentItem title={'IList<T> ExecuteQuery<T>(string sql, object param = null, bool trace = true)'}>
                {'Paramter:'}{'\n'}
                {'  sql(string):sql語法'}{'\n'}
                {'  param(object)?:代入參數'}{'\n'}
                {'  trace(bool)?:是否紀錄到SqlLog'}{'\n'}
                {'return:IList<T>'}{'\n'}
                {'\n'}
                {'IList<model> dac.ExecuteQuery<model>(sql, param);'}{'\n'}
            </DocumentItem>
            <DocumentItem title={'Task<IList<T>> ExecuteQueryAsync<T>(string sql, object param = null, bool trace = true)'}>
                {'Paramter:'}{'\n'}
                {'  sql(string):sql語法'}{'\n'}
                {'  param(object)?:代入參數'}{'\n'}
                {'  trace(bool)?:是否紀錄到SqlLog'}{'\n'}
                {'return:Task<IList<T>>'}{'\n'}
                {'\n'}
                {'IList<model> dac.ExecuteQueryAsync<model>(sql, param);'}{'\n'}
            </DocumentItem>
            <DocumentItem title={'bool ExecuteCommand(string sql, IDbEditor param = null, bool trace = true)'}>
                {'Paramter:'}{'\n'}
                {'  sql(string):sql語法'}{'\n'}
                {'  param(IDbEditor | IEnumerable<IDbEditor>)?:代入參數'}{'\n'}
                {'  trace(bool)?:是否紀錄到SqlLog'}{'\n'}
                {'return:bool'}{'\n'}
                {'\n'}
                {'IList<model> dac.ExecuteCommand(sql, param);'}{'\n'}
            </DocumentItem>
            <DocumentItem title={'bool ExecuteCommandWithObject(string sql, object param = null, bool trace = true)'}>
                {'非必要請使用ExecuteCommand'}{'\n'}
                {'Paramter:'}{'\n'}
                {'  sql(string):sql語法'}{'\n'}
                {'  param(object)?:代入參數'}{'\n'}
                {'  trace(bool)?:是否紀錄到SqlLog'}{'\n'}
                {'return:bool'}{'\n'}
                {'\n'}
                {'IList<model> dac.ExecuteCommand(sql, param);'}{'\n'}
            </DocumentItem>
            <DocumentItem title={'Task<bool> ExecuteCommandAsync(string sql, IDbEditor param = null, bool trace = true)'}>
                {'Paramter:'}{'\n'}
                {'  sql(string):sql語法'}{'\n'}
                {'  param(IDbEditor | IEnumerable<IDbEditor>)?:代入參數'}{'\n'}
                {'  trace(bool)?:是否紀錄到SqlLog'}{'\n'}
                {'return:Task<bool>'}{'\n'}
                {'\n'}
                {'IList<model> dac.ExecuteCommand(sql, param);'}{'\n'}
            </DocumentItem>
            <DocumentItem title={'Task<bool> ExecuteCommandWithObjectAsync(string sql, object param = null, bool trace = true)'}>
                {'非必要請使用ExecuteCommandAsync'}{'\n'}
                {'Paramter:'}{'\n'}
                {'  sql(string):sql語法'}{'\n'}
                {'  param(object)?:代入參數'}{'\n'}
                {'  trace(bool)?:是否紀錄到SqlLog'}{'\n'}
                {'return:Task<bool>'}{'\n'}
                {'\n'}
                {'IList<model> dac.ExecuteCommand(sql, param);'}{'\n'}
            </DocumentItem>
        </>
    )
}
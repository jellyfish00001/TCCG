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
            <h2>Log</h2>
            <DocumentItem title='ILogger 注入'>
                {'private readonly ILogger<{Class Name}> logger;'}{'\n'}
                {'public constructor(ILogger<{Class Name}> logger){'}{'\n'}
                {'    this.logger = logger;'}{'\n'}
                {'}'}{'\n'}
            </DocumentItem>
            <DocumentItem title='Logger.LogDebug'>
                {'logger.LogDebug(\'Debug\');'}{'\n'}
            </DocumentItem>
            <DocumentItem title='Logger.LogInfo'>
                {'logger.LogInfo(\'Info\');'}{'\n'}
            </DocumentItem>
            <DocumentItem title='Logger.LogError'>
                {'logger.LogError(ex.toString());'}{'\n'}
            </DocumentItem>
        </>
    )
}
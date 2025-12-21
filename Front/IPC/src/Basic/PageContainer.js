import React, { useState, useEffect } from 'react';
import { IsNullOrEmpty } from './SDOExtension';
import { Toolbar } from './Toolbar';

export const PageContainer = props => {
    const { className } = props
    const [toolbar, setToolbar] = useState(null);
    useEffect(() => {
        setToolbar(
            <Toolbar>
                {props.toolbar}
            </Toolbar>
        )
    }, [props.toolbar])

    return (
        <div
            className={`fnForm ${IsNullOrEmpty(className) ? "" : className}`}
            style={props.style}>
            {toolbar}
            {props.children}
        </div>
    )
}
import React, { createContext, useState } from "react";


export const defaultPageIndex = {
    pageIndex: '.0',
    changePageIndex: () => { },
}
export const PageIndexContext = createContext(defaultPageIndex);

export const ChapterPageIndexProvider = props => {
    const [pageIndex, setPageIndex] = useState(defaultPageIndex.pageIndex)

    // 切換頁籤
    const changePageIndex = async (index) => {
        setPageIndex(index);
    };

    const defaultValue = {
        pageIndex,
        changePageIndex
    }

    return (
        <PageIndexContext.Provider value={defaultValue}>
            {props.children}
        </PageIndexContext.Provider>
    )
}

//@ts-nocheck
import React from "react";
import '../Css/PageNotFound.css';

const PageNotFound = props => {
    return (
        <div id="notfound">
            <div class="notfound">
                <div class="notfound-404">
                    <h3>頁面不存在</h3>
                    <h1><span>4</span><span>0</span><span>4</span></h1>
                </div>
                <h2>很抱歉!您要求的頁面並不存在</h2>
            </div>
        </div>
    )
}

export default PageNotFound;
import React, { useState, useEffect } from 'react';
import iconRefresh from '../../Images/icon_refresh_h.png';
export const Captcha = (props) => {

    return (
        <div>
            <img src={"data:image/gif;base64," + props.Img} alt="Red dot" />
            <a href="/" onClick={(e) => {
                e.preventDefault();
                props.GetCaptcha(e)
            }}>
                <img src={iconRefresh} className='iconRefresh' alt='' />
                刷新
            </a>
        </div>
    );
}
export default React.memo(Captcha)
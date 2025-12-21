import React from 'react';
import loading from '../../Images/loading.gif';

const Overlay = () => {
    return (
        <div id="globalOverlay" className="hide fullscreen">
            <div className="k-overlay"></div>
            <img src={loading} alt="loading..." className="overlay-icon" />
        </div>
    );
}

export default Overlay;
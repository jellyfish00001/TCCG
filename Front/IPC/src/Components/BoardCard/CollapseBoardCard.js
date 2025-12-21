import React from 'react';
import { Reveal } from '@progress/kendo-react-animation';
import angleDownIcon from '../../Css/icon/blue/fa-angle-down_White.png';
import angleUpIcon from '../../Css/icon/blue/fa-angle-up_hover.png';

const CollapseBoardCard = (props) => {
    const { initValue, title, button, titleStyle, children, setValue, isFirstArea } = props;

    const [visible, setVisible] = React.useState(initValue === undefined || initValue ? true : false);
    const [visibleStyle, setVisibleStyle] = React.useState(initValue === undefined || initValue ? {} : { display: 'none' });

    const openMenu = () => {
        setVisible(!visible);
        setVisibleStyle(visible ? { display: 'none' } : {});

        if (setValue !== undefined) {
            setValue(!visible);
        }
    }

    React.useEffect(() => {
        if (initValue !== undefined) {
            setVisible(initValue);
            setVisibleStyle(initValue ? {} : { display: 'none' });
        }
    }, [initValue])

    return (
        <div>
            {
                button &&
                <div className="fn-buttons fixed-buttons full-fixed-buttons">{button}</div>
            }
            <h3 className={`collapse-title${isFirstArea ? " collapse-title-first" : ""}`} style={titleStyle} onClick={openMenu}>
                {title}
                <img src={visible ? angleUpIcon : angleDownIcon} alt="" />
            </h3>
            <Reveal
                direction={"down"}
                style={{ width: '-webkit-fill-available' }}
            >
                <div style={visibleStyle}>{children}</div>
            </Reveal>
        </div>
    )
}

export default CollapseBoardCard;
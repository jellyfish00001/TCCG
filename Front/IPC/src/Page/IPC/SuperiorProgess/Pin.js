import React from 'react';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';

const Pin = (props) => {
    const { item: { ENGNEER_STAGE, PRG_OFFSET } } = props
    const iconName = IsNullOrEmpty(ENGNEER_STAGE) ? "refresh"
        : ENGNEER_STAGE === "A" ? "cog"
            : ENGNEER_STAGE === "B" ? "check"
                : "";
    const iconColor = PRG_OFFSET >= 0 ? "G"
        : 0 > PRG_OFFSET && PRG_OFFSET > -5 ? "Y"
            : PRG_OFFSET <= -5 ? "R"
                : "";
    return (<p className={`mapIcon mapIcon-P ${iconName}-${iconColor}`} style={{ marginLeft: '-10px', width: '20px' }} />)
}

export default Pin;
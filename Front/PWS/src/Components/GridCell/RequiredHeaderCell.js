import React from 'react';
/**
 * 
 * @param {*} props 
 * @returns 
 */
export const RequiredHeaderCell = (props) => {
    let additionalStyle = {}
    if (props.addtionStyle) {
        additionalStyle = { ...props.addtionStyle }
    }
    return (
        <>
            <div  className="addRedStar" style={additionalStyle}>
                {props.children ? props.children:props.title}
            </div>
        </>
    );
};
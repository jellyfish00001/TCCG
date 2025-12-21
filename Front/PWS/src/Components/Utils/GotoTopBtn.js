import * as React from 'react';
import Util from '../../Css/custom/Utils.module.css';
import gotoTopPicPath from '../../Images/gototop.png';

/**
 * 回最上面
 * @param {*} targetClass 
 */
export const scrollTop = (targetClass) => {
    document.getElementsByClassName(targetClass)[0].scrollTo({ top: 0, behavior: 'smooth' })
}

// 回頂端浮動按鈕
const GotoTopBtn = (props) => {
    let { targetClass } = props;

    const [IsVisible, setIsVisible] = React.useState(false);



    const toggleVisibility = () => {
        let currentY = document.getElementsByClassName(targetClass)[0].scrollTop;
        if (currentY > 100) {
            setIsVisible(true);
        } else {
            setIsVisible(false);
        }
    }

    React.useEffect(() => {
        document.getElementsByClassName(targetClass)[0].addEventListener('scroll', toggleVisibility)
    }, [])

    return (
        <>
            {
                IsVisible &&
                <div className={Util.gototop} onClick={() => scrollTop(targetClass)}>
                    <a style={{ cursor: 'pointer' }}>
                        <img src={gotoTopPicPath} alt="gotoTop" />
                        <div>TOP</div>
                    </a>
                </div>
            }
        </>
    )
}

export default React.memo(GotoTopBtn)
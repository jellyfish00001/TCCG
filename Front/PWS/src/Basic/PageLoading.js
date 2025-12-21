import React from "react";
import { SetMaskOnOff } from "./SDOExtension";

const PageLoading = () => {

    React.useEffect(() => {
        SetMaskOnOff(true);
    }, []);

    return (
        <>
        </>
    );
}

export default PageLoading;
import React from "react";
import { SetMaskOnOff } from "../Basic/SDOExtension";
import { SignInStatusContext } from '../Basic/BasicData';
import { api } from "../Basic/ApiFetch";
import { WindowResizehook } from '../Hook/useWindowResize';
import GotoTopBtn from "../Components/Utils/GotoTopBtn";
import { getGlobalServerConfig } from "../Route/RootMiddleware";
import { Download } from '../Basic/Download';

const UserGuide = () => {

    let APIUrl = getGlobalServerConfig().backEndUrl.get();

    const [userGuides, setUserGuides] = React.useState([]);
    const { setSignInStatus, signInStatus } = React.useContext(SignInStatusContext);

    const dimensions = WindowResizehook();

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = async () => {
        SetMaskOnOff(true);

        setSignInStatus();

        let data = await getUserGuides();
        console.log(data);
        setUserGuides([...data]);
        SetMaskOnOff(false);
    }

    // 取得操作手冊清單
    const getUserGuides = async () => {
        let url = APIUrl + 'UserGuide/GetUserGuides';
        let response = await api.Post(url);
        let result = [];
        if (response.ok) {
            result = await response.json();
        }
        return result;
    }

    // 下載操作手冊
    const downloadFile = async (groupName, fileName) => {
        let url = APIUrl + 'UserGuide/DownloadFile';
        let form = new FormData();
        form.append("groupName", groupName);
        form.append("fileName", fileName);
        Download(url, "POST", form, new Headers());
    }

    return (
        <div className="portal">
            <div style={{ padding: "10px" }}>
                <div
                    className=" UserGuide"
                    style={{ overflow: "auto", height: `${dimensions.height - 150}px` }}
                >
                    <h3 className='k-dialog-titlebar'>操作手冊</h3>

                    {
                        userGuides.length > 0 &&
                        userGuides.map(x => {
                            return (
                                <div>
                                    <h3>{x.GroupName}</h3>
                                    <ul>
                                        {
                                            x.Files.length > 0 &&
                                            x.Files.map(y => {
                                                return (
                                                    <li>
                                                        <a href='/' onClick={(e) => { e.preventDefault(); downloadFile(x.GroupName, y) }}>
                                                            {y}
                                                        </a>
                                                    </li>

                                                )
                                            })
                                        }
                                    </ul>
                                </div>
                            )
                        })
                    }
                    <GotoTopBtn targetClass="UserGuide" />
                </div>
            </div>
        </div>
    )
}

export default UserGuide;
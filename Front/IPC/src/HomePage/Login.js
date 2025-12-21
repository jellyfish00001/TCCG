//@ts-check
import React from 'react';
// import LoginAnnouncement from '../HomePage/LoginAnnounce';
import '../Css/Login.css';
import LoginPanel from './LoginPanel';
const Login = (props) => {



    return (
        <div className="loginFlex">
            {/* <LoginAnnouncement /> */}
            {
                <LoginPanel {...props} />
            }
        </div>
    );
}

export default Login;
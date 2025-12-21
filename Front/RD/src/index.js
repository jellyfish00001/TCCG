import 'react-app-polyfill/ie11';
import 'react-app-polyfill/stable';
import React from 'react';
import ReactDOM from 'react-dom';
import App from './App'
import * as serviceWorker from './serviceWorker';
import './index.css';
import '@progress/kendo-theme-default/dist/all.css';
import './Css/Site.css';
import './Css/SDObasic-blue.css';
import 'font-awesome/css/font-awesome.min.css'
import 'ie11-custom-properties';

//@ts-ignore
if(window.MSInputMethodContext && document.documentMode){
  // import('ie11-custom-properties');
}

ReactDOM.render(
  <div>
    <React.StrictMode>
      <App />
    </React.StrictMode>
  </div>,
  document.getElementById('root')
);


// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
// serviceWorker.unregister();
serviceWorker.register();


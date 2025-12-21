import '@hookstate/devtools'
import React from 'react';
import { MessageBoxProvider } from './Components/Dialogs/MessageBox';
import { ConfirmBoxProvider } from './Components/Dialogs/ConfirmBox';
import RootRoute from './Route/RootRoute';
import { HeaderFooterStatusProvider } from './Basic/BasicData';

const App = () => {

  return (
    <MessageBoxProvider>
      <ConfirmBoxProvider>
      <HeaderFooterStatusProvider>
        {/* <SignInStatusProvider> */}
        <RootRoute />
        {/* </SignInStatusProvider> */}
        </HeaderFooterStatusProvider>
      </ConfirmBoxProvider>
    </MessageBoxProvider>
  );
}

export default App;

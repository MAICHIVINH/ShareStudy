import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ConfigProvider, App as AntdApp, theme as antdThemeAlgorithm } from 'antd';
import viVN from 'antd/locale/vi_VN';
import { Toaster } from 'react-hot-toast';
import { antdTheme } from './config/theme';
import { AuthProvider } from './contexts/AuthContext';
import { ThemeProvider, useTheme } from './contexts/ThemeContext';
import { SettingsProvider } from './contexts/SettingsContext';
import { AppRoutes } from './routes/AppRoutes';

const AppContent = () => {
  const { isDarkMode } = useTheme();

  const dynamicTheme = {
    ...antdTheme,
    algorithm: isDarkMode ? antdThemeAlgorithm.darkAlgorithm : antdThemeAlgorithm.defaultAlgorithm,
  };

  return (
    <ConfigProvider theme={dynamicTheme} locale={viVN}>
      <AntdApp>
        <BrowserRouter>
          <AuthProvider>
            <SettingsProvider>
              <AppRoutes />
              <Toaster position="top-right" />
            </SettingsProvider>
          </AuthProvider>
        </BrowserRouter>
      </AntdApp>
    </ConfigProvider>
  );
};

export function App() {
  return (
    <ThemeProvider>
      <AppContent />
    </ThemeProvider>
  );
}

export default App;

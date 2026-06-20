import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useAppStore } from './store/appStore';
import Header from './components/Layout/Header';
import Sidebar from './components/Layout/Sidebar';
import StatusBar from './components/Layout/StatusBar';
import Dashboard from './components/Dashboard/Dashboard';
import ServerList from './components/ServerList/ServerList';
import Settings from './components/Settings/Settings';

function App() {
  const { i18n } = useTranslation();
  const { 
    fetchSettings, 
    fetchServers, 
    setTheme, 
    language, 
    activeView,
    theme 
  } = useAppStore();

  useEffect(() => {
    // Load initial data
    fetchSettings();
    fetchServers();
  }, []);

  useEffect(() => {
    // Update language
    i18n.changeLanguage(language);
    document.documentElement.dir = language === 'fa' ? 'rtl' : 'ltr';
  }, [language, i18n]);

  useEffect(() => {
    // Apply theme
    setTheme(theme);
  }, [theme, setTheme]);

  const renderContent = () => {
    switch (activeView) {
      case 'servers':
        return <ServerList />;
      case 'settings':
        return <Settings />;
      default:
        return <Dashboard />;
    }
  };

  return (
    <div className="flex flex-col h-screen bg-[var(--bg-primary)]">
      <Header />
      <div className="flex flex-1 overflow-hidden">
        <Sidebar />
        <main className="flex-1 overflow-auto p-6">
          {renderContent()}
        </main>
      </div>
      <StatusBar />
    </div>
  );
}

export default App;
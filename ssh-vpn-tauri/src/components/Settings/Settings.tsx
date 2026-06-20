import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader } from '../UI/Card';
import { Toggle } from '../UI/Toggle';
import { Button } from '../UI/Button';
import { useAppStore } from '../../store/appStore';

export default function Settings() {
  const { t } = useTranslation();
  const { settings, saveSettings, setTheme, setLanguage, theme, language } = useAppStore();
  const [checkingUpdate, setCheckingUpdate] = useState(false);
  const [updateMessage, setUpdateMessage] = useState('');

  const handleToggle = async (key: keyof typeof settings, value: boolean) => {
    const newSettings = { ...settings, [key]: value };
    await saveSettings(newSettings);
  };

  const handleThemeChange = (newTheme: 'light' | 'dark' | 'system') => {
    setTheme(newTheme);
    saveSettings({ ...settings, theme: newTheme });
  };

  const handleLanguageChange = (newLanguage: 'en' | 'fa') => {
    setLanguage(newLanguage);
    saveSettings({ ...settings, language: newLanguage });
  };

  const checkForUpdates = async () => {
    setCheckingUpdate(true);
    setUpdateMessage('');
    try {
      // Open GitHub releases page
      window.open('https://github.com/tahatehran/CSharp-SSH-VPN/releases', '_blank');
      setUpdateMessage('Please download the latest version from GitHub');
    } catch (e) {
      setUpdateMessage('Failed to check for updates');
    }
    setCheckingUpdate(false);
  };

  return (
    <div className="space-y-6 max-w-2xl">
      <h2 className="text-2xl font-bold text-[var(--text-primary)]">{t('settings.title')}</h2>

      {/* Update Section */}
      <Card>
        <CardHeader>
          <h3 className="text-lg font-semibold text-[var(--text-primary)]">Updates</h3>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-[var(--text-secondary)]">Current Version</p>
              <p className="text-lg font-bold text-[var(--text-primary)]">v1.3.2</p>
            </div>
            <Button onClick={checkForUpdates} isLoading={checkingUpdate}>
              Check for Updates
            </Button>
          </div>
          {updateMessage && (
            <p className="text-sm text-[var(--text-secondary)]">{updateMessage}</p>
          )}
        </CardContent>
      </Card>

      {/* General Settings */}
      <Card>
        <CardHeader>
          <h3 className="text-lg font-semibold text-[var(--text-primary)]">{t('settings.general')}</h3>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Language */}
          <div>
            <label className="block text-sm font-medium text-[var(--text-secondary)] mb-3">
              {t('settings.language')}
            </label>
            <div className="flex gap-2">
              <button
                onClick={() => handleLanguageChange('en')}
                className={`px-4 py-2 rounded-lg transition-colors ${
                  language === 'en'
                    ? 'bg-[var(--accent)] text-white'
                    : 'bg-[var(--bg-tertiary)] text-[var(--text-primary)] hover:bg-[var(--border)]'
                }`}
              >
                English
              </button>
              <button
                onClick={() => handleLanguageChange('fa')}
                className={`px-4 py-2 rounded-lg transition-colors ${
                  language === 'fa'
                    ? 'bg-[var(--accent)] text-white'
                    : 'bg-[var(--bg-tertiary)] text-[var(--text-primary)] hover:bg-[var(--border)]'
                }`}
              >
                فارسی
              </button>
            </div>
          </div>

          {/* Theme */}
          <div>
            <label className="block text-sm font-medium text-[var(--text-secondary)] mb-3">
              {t('settings.theme')}
            </label>
            <div className="flex gap-2">
              <button
                onClick={() => handleThemeChange('light')}
                className={`px-4 py-2 rounded-lg transition-colors flex items-center gap-2 ${
                  theme === 'light'
                    ? 'bg-[var(--accent)] text-white'
                    : 'bg-[var(--bg-tertiary)] text-[var(--text-primary)] hover:bg-[var(--border)]'
                }`}
              >
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
                {t('settings.light')}
              </button>
              <button
                onClick={() => handleThemeChange('dark')}
                className={`px-4 py-2 rounded-lg transition-colors flex items-center gap-2 ${
                  theme === 'dark'
                    ? 'bg-[var(--accent)] text-white'
                    : 'bg-[var(--bg-tertiary)] text-[var(--text-primary)] hover:bg-[var(--border)]'
                }`}
              >
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                </svg>
                {t('settings.dark')}
              </button>
              <button
                onClick={() => handleThemeChange('system')}
                className={`px-4 py-2 rounded-lg transition-colors flex items-center gap-2 ${
                  theme === 'system'
                    ? 'bg-[var(--accent)] text-white'
                    : 'bg-[var(--bg-tertiary)] text-[var(--text-primary)] hover:bg-[var(--border)]'
                }`}
              >
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                </svg>
                {t('settings.system')}
              </button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Connection Settings */}
      <Card>
        <CardHeader>
          <h3 className="text-lg font-semibold text-[var(--text-primary)]">{t('settings.connection')}</h3>
        </CardHeader>
        <CardContent className="space-y-6">
          <Toggle
            checked={settings.auto_reconnect}
            onChange={(value) => handleToggle('auto_reconnect', value)}
            label={t('settings.autoReconnect')}
          />
          
          <Toggle
            checked={settings.kill_switch}
            onChange={(value) => handleToggle('kill_switch', value)}
            label={t('settings.killSwitch')}
          />
          
          <Toggle
            checked={settings.dns_protection}
            onChange={(value) => handleToggle('dns_protection', value)}
            label={t('settings.dnsProtection')}
          />
        </CardContent>
      </Card>

      {/* Advanced Settings */}
      <Card>
        <CardHeader>
          <h3 className="text-lg font-semibold text-[var(--text-primary)]">Advanced</h3>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1">
              SOCKS5 Proxy Port
            </label>
            <input
              type="number"
              value={settings.socks_port}
              onChange={(e) => saveSettings({ ...settings, socks_port: parseInt(e.target.value) || 9000 })}
              className="w-full px-3 py-2 bg-[var(--bg-tertiary)] border border-[var(--border)] rounded-lg 
                         text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)]"
              min={1024}
              max={65535}
            />
            <p className="text-xs text-[var(--text-secondary)] mt-1">
              Local port for SOCKS5 proxy (default: 9000)
            </p>
          </div>

          <div>
            <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1">
              Check Interval (seconds)
            </label>
            <input
              type="number"
              value={settings.check_interval_sec}
              onChange={(e) => saveSettings({ ...settings, check_interval_sec: parseInt(e.target.value) || 30 })}
              className="w-full px-3 py-2 bg-[var(--bg-tertiary)] border border-[var(--border)] rounded-lg 
                         text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)]"
              min={5}
              max={300}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1">
              Max Ping (ms) - Auto-failover threshold
            </label>
            <input
              type="number"
              value={settings.max_ping_ms}
              onChange={(e) => saveSettings({ ...settings, max_ping_ms: parseInt(e.target.value) || 200 })}
              className="w-full px-3 py-2 bg-[var(--bg-tertiary)] border border-[var(--border)] rounded-lg 
                         text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)]"
              min={50}
              max={1000}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
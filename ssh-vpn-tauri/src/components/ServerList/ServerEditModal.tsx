import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Modal } from '../UI/Modal';
import { Button } from '../UI/Button';
import { useAppStore } from '../../store/appStore';

interface ServerEditModalProps {
  isOpen: boolean;
  onClose: () => void;
  serverId?: string;
}

export default function ServerEditModal({ isOpen, onClose, serverId }: ServerEditModalProps) {
  const { t } = useTranslation();
  const { servers, addServer, updateServer } = useAppStore();
  
  const existingServer = serverId ? servers.find(s => s.id === serverId) : null;
  const isEditing = !!existingServer;

  const [formData, setFormData] = useState({
    name: '',
    host: '',
    port: 22,
    username: '',
    password: '',
    country: '',
    city: '',
  });

  useEffect(() => {
    if (existingServer) {
      setFormData({
        name: existingServer.name,
        host: existingServer.host,
        port: existingServer.port,
        username: existingServer.username,
        password: existingServer.password || '',
        country: existingServer.country || '',
        city: existingServer.city || '',
      });
    } else {
      setFormData({
        name: '',
        host: '',
        port: 22,
        username: '',
        password: '',
        country: '',
        city: '',
      });
    }
  }, [existingServer, isOpen]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      if (isEditing && existingServer) {
        await updateServer({
          ...existingServer,
          name: formData.name,
          host: formData.host,
          port: formData.port,
          username: formData.username,
          password: formData.password || undefined,
          country: formData.country || undefined,
          city: formData.city || undefined,
        });
      } else {
        await addServer({
          name: formData.name,
          name_fa: undefined,
          host: formData.host,
          port: formData.port,
          username: formData.username,
          password: formData.password || undefined,
          private_key_path: undefined,
          country: formData.country || undefined,
          city: formData.city || undefined,
          priority: 0,
          is_active: false,
        });
      }
      onClose();
    } catch (error) {
      console.error('Failed to save server:', error);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={isEditing ? t('servers.editServer') : t('servers.addServer')}
      size="md"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1">
            {t('servers.name')}
          </label>
          <input
            type="text"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            className="w-full px-3 py-2 bg-[var(--bg-tertiary)] border border-[var(--border)] rounded-lg 
                       text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)]"
            required
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1">
              {t('servers.host')}
            </label>
            <input
              type="text"
              value={formData.host}
              onChange={(e) => setFormData({ ...formData, host: e.target.value })}
              className="w-full px-3 py-2 bg-[var(--bg-tertiary)] border border-[var(--border)] rounded-lg 
                         text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)]"
              required
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1">
              {t('servers.port')}
            </label>
            <input
              type="number"
              value={formData.port}
              onChange={(e) => setFormData({ ...formData, port: parseInt(e.target.value) || 22 })}
              className="w-full px-3 py-2 bg-[var(--bg-tertiary)] border border-[var(--border)] rounded-lg 
                         text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)]"
              required
              min={1}
              max={65535}
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1">
            {t('servers.username')}
          </label>
          <input
            type="text"
            value={formData.username}
            onChange={(e) => setFormData({ ...formData, username: e.target.value })}
            className="w-full px-3 py-2 bg-[var(--bg-tertiary)] border border-[var(--border)] rounded-lg 
                       text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)]"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1">
            {t('servers.password')}
          </label>
          <input
            type="password"
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            className="w-full px-3 py-2 bg-[var(--bg-tertiary)] border border-[var(--border)] rounded-lg 
                       text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)]"
          />
        </div>

        <div className="flex gap-3 justify-end pt-4">
          <Button type="button" variant="secondary" onClick={onClose}>
            {t('servers.cancel')}
          </Button>
          <Button type="submit">
            {t('servers.save')}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
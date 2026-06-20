import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { Card, CardContent } from '../UI/Card';
import { Button } from '../UI/Button';
import { Modal } from '../UI/Modal';
import ServerCard from './ServerCard';
import ServerEditModal from './ServerEditModal';
import { useAppStore } from '../../store/appStore';

export default function ServerList() {
  const { t } = useTranslation();
  const { servers, deleteServer, setActiveServer } = useAppStore();
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [editingServer, setEditingServer] = useState<string | null>(null);
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);

  const handleDelete = async (id: string) => {
    try {
      await deleteServer(id);
      setDeleteConfirmId(null);
    } catch (error) {
      console.error('Failed to delete server:', error);
    }
  };

  const handleSetActive = async (id: string) => {
    try {
      await setActiveServer(id);
    } catch (error) {
      console.error('Failed to set active server:', error);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-[var(--text-primary)]">{t('servers.title')}</h2>
        <Button onClick={() => setIsAddModalOpen(true)}>
          <svg className="w-4 h-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          {t('servers.addServer')}
        </Button>
      </div>

      {servers.length === 0 ? (
        <Card>
          <CardContent className="text-center py-12">
            <svg className="w-16 h-16 mx-auto text-[var(--text-secondary)] mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M5 12h14M5 12a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v4a2 2 0 01-2 2M5 12a2 2 0 00-2 2v4a2 2 0 002 2h14a2 2 0 002-2v-4a2 2 0 00-2-2m-2-4h.01M17 16h.01" />
            </svg>
            <h3 className="text-lg font-medium text-[var(--text-primary)] mb-2">{t('servers.noServers')}</h3>
            <p className="text-[var(--text-secondary)] mb-4">{t('servers.addFirst')}</p>
            <Button onClick={() => setIsAddModalOpen(true)}>
              {t('servers.addServer')}
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <AnimatePresence>
            {servers.map((server) => (
              <motion.div
                key={server.id}
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                exit={{ opacity: 0, scale: 0.95 }}
              >
                <ServerCard
                  server={server}
                  onEdit={() => setEditingServer(server.id)}
                  onDelete={() => setDeleteConfirmId(server.id)}
                  onSetActive={() => handleSetActive(server.id)}
                />
              </motion.div>
            ))}
          </AnimatePresence>
        </div>
      )}

      {/* Add Server Modal */}
      <ServerEditModal
        isOpen={isAddModalOpen}
        onClose={() => setIsAddModalOpen(false)}
      />

      {/* Edit Server Modal */}
      <ServerEditModal
        isOpen={!!editingServer}
        onClose={() => setEditingServer(null)}
        serverId={editingServer || undefined}
      />

      {/* Delete Confirmation Modal */}
      <Modal
        isOpen={!!deleteConfirmId}
        onClose={() => setDeleteConfirmId(null)}
        title={t('servers.deleteServer')}
        size="sm"
      >
        <div className="space-y-4">
          <p className="text-[var(--text-secondary)]">{t('servers.confirmDelete')}</p>
          <div className="flex gap-3 justify-end">
            <Button variant="secondary" onClick={() => setDeleteConfirmId(null)}>
              {t('servers.cancel')}
            </Button>
            <Button variant="danger" onClick={() => deleteConfirmId && handleDelete(deleteConfirmId)}>
              {t('servers.deleteServer')}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
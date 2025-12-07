import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import MainLayout from '@/layouts/MainLayout';
import { FiHeart, FiList, FiSearch, FiSettings } from 'react-icons/fi';
import FavoritesTab from '@/components/organisms/FavoritesTab';
import MyListingsTab from '@/components/organisms/MyListingsTab';
import SavedSearchesTab from '@/components/organisms/SavedSearchesTab';
import SettingsTab from '@/components/organisms/SettingsTab';

type TabId = 'favorites' | 'listings' | 'searches' | 'settings';

interface Tab {
  id: TabId;
  labelKey: string;
  icon: React.ReactNode;
}

const tabs: Tab[] = [
  { id: 'favorites', labelKey: 'user:dashboard.tabs.favorites', icon: <FiHeart size={20} /> },
  { id: 'listings', labelKey: 'user:dashboard.tabs.listings', icon: <FiList size={20} /> },
  { id: 'searches', labelKey: 'user:dashboard.tabs.searches', icon: <FiSearch size={20} /> },
  { id: 'settings', labelKey: 'user:dashboard.tabs.settings', icon: <FiSettings size={20} /> },
];

export default function UserDashboardPage() {
  const { t } = useTranslation(['common', 'user']);
  const [activeTab, setActiveTab] = useState<TabId>('favorites');

  const renderTabContent = () => {
    switch (activeTab) {
      case 'favorites':
        return <FavoritesTab />;
      case 'listings':
        return <MyListingsTab />;
      case 'searches':
        return <SavedSearchesTab />;
      case 'settings':
        return <SettingsTab />;
      default:
        return null;
    }
  };

  return (
    <MainLayout>
      <div className="bg-gray-50 min-h-screen py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-2">
              {t('common:nav.dashboard')}
            </h1>
            <p className="text-gray-600">
              {t('user:dashboard.subtitle')}
            </p>
          </div>

          {/* Tabs Navigation */}
          <div className="bg-white rounded-xl shadow-card mb-6">
            <div className="border-b border-gray-200">
              <nav className="flex -mb-px overflow-x-auto">
                {tabs.map((tab) => (
                  <button
                    key={tab.id}
                    onClick={() => setActiveTab(tab.id)}
                    className={`
                      flex items-center gap-2 px-6 py-4 text-sm font-medium border-b-2 whitespace-nowrap transition-colors duration-200
                      ${
                        activeTab === tab.id
                          ? 'border-primary text-primary'
                          : 'border-transparent text-gray-600 hover:text-gray-900 hover:border-gray-300'
                      }
                    `}
                  >
                    {tab.icon}
                    <span>{t(tab.labelKey)}</span>
                  </button>
                ))}
              </nav>
            </div>
          </div>

          {/* Tab Content */}
          <div>{renderTabContent()}</div>
        </div>
      </div>
    </MainLayout>
  );
}


import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import MainLayout from '@/layouts/MainLayout';
import { FiHeart, FiList, FiSearch, FiSettings, FiShield, FiX, FiArrowRight } from 'react-icons/fi';
import FavoritesTab from '@/components/organisms/FavoritesTab';
import MyListingsTab from '@/components/organisms/MyListingsTab';
import SavedSearchesTab from '@/components/organisms/SavedSearchesTab';
import SettingsTab from '@/components/organisms/SettingsTab';
import { kycService, type KYCProfile, KYCStatus } from '@/services/kycService';

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
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<TabId>('favorites');

  // KYC verification state
  const [kycProfile, setKycProfile] = useState<KYCProfile | null>(null);
  const [showKycBanner, setShowKycBanner] = useState(true);
  const [kycLoading, setKycLoading] = useState(true);

  // Fetch KYC status on mount
  useEffect(() => {
    const fetchKycStatus = async () => {
      try {
        const userStr = localStorage.getItem('user');
        if (userStr) {
          const user = JSON.parse(userStr);
          const profile = await kycService.getProfileByUserId(user.id);
          setKycProfile(profile);
        }
      } catch (err) {
        // No KYC profile yet - show the banner
        console.log('No KYC profile found');
        setKycProfile(null);
      } finally {
        setKycLoading(false);
      }
    };
    fetchKycStatus();
  }, []);

  // Check if user needs to complete KYC verification
  const needsKycVerification =
    !kycLoading && (!kycProfile || kycProfile.status !== KYCStatus.Approved);

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
          {/* KYC Verification Banner */}
          {needsKycVerification && showKycBanner && (
            <div className="mb-6 bg-gradient-to-r from-blue-600 to-indigo-600 rounded-xl shadow-lg overflow-hidden">
              <div className="relative p-6">
                <button
                  onClick={() => setShowKycBanner(false)}
                  className="absolute top-4 right-4 text-white/80 hover:text-white transition-colors"
                  aria-label="Cerrar banner"
                >
                  <FiX size={20} />
                </button>
                <div className="flex flex-col md:flex-row items-start md:items-center gap-4">
                  <div className="flex-shrink-0 p-3 bg-white/20 rounded-full">
                    <FiShield className="text-white" size={32} />
                  </div>
                  <div className="flex-1">
                    <h3 className="text-lg font-bold text-white mb-1">¡Verifica tu Identidad!</h3>
                    <p className="text-blue-100 text-sm">
                      Completa la verificación de identidad para publicar vehículos, contactar
                      vendedores y acceder a todas las funciones de la plataforma.
                    </p>
                    {kycProfile && kycProfile.status === KYCStatus.Rejected && (
                      <p className="text-yellow-200 text-xs mt-2">
                        ⚠️ Tu verificación anterior fue rechazada.{' '}
                        {kycProfile.rejectionReason || 'Por favor, intenta de nuevo.'}
                      </p>
                    )}
                  </div>
                  <div className="flex-shrink-0">
                    <button
                      onClick={() => navigate('/kyc/biometric-verify')}
                      className="inline-flex items-center gap-2 px-5 py-2.5 bg-white text-blue-600 font-semibold rounded-lg hover:bg-blue-50 transition-colors shadow-md"
                    >
                      Verificar Ahora
                      <FiArrowRight size={16} />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-2">
              {t('common:nav.dashboard')}
            </h1>
            <p className="text-gray-600">{t('user:dashboard.subtitle')}</p>
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

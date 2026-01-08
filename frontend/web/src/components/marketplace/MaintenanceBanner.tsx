import { useState, useEffect } from 'react';
import { FiAlertCircle, FiX } from 'react-icons/fi';
import Button from '@/components/atoms/Button';

interface MaintenanceBannerProps {
  onClose?: () => void;
}

export const MaintenanceBanner = ({ onClose }: MaintenanceBannerProps) => {
  const [isVisible, setIsVisible] = useState(false);
  const [maintenanceData, setMaintenanceData] = useState<{
    isActive: boolean;
    message: string;
    startTime: string;
    endTime: string;
    severity: 'info' | 'warning' | 'error';
  } | null>(null);

  useEffect(() => {
    // Fetch current maintenance status
    fetch('https://api.okla.com.do/api/maintenance/current')
      .then((res) => res.json())
      .then((data) => {
        if (data && data.isActive) {
          setMaintenanceData(data);
          setIsVisible(true);
        }
      })
      .catch(console.error);
  }, []);

  const handleClose = () => {
    setIsVisible(false);
    onClose?.();
  };

  if (!isVisible || !maintenanceData) return null;

  const bgColor = {
    info: 'bg-blue-50 border-blue-200',
    warning: 'bg-yellow-50 border-yellow-200',
    error: 'bg-red-50 border-red-200',
  }[maintenanceData.severity];

  const iconColor = {
    info: 'text-blue-600',
    warning: 'text-yellow-600',
    error: 'text-red-600',
  }[maintenanceData.severity];

  return (
    <div className={`w-full ${bgColor} border-b px-4 py-3`}>
      <div className="container mx-auto">
        <div className="flex items-center justify-between w-full">
          <div className="flex items-center gap-3">
            <FiAlertCircle className={`h-5 w-5 ${iconColor}`} />
            <div>
              <p className="text-sm font-medium">{maintenanceData.message}</p>
              <p className="text-xs text-gray-600 mt-1">
                {new Date(maintenanceData.startTime).toLocaleTimeString()} -{' '}
                {new Date(maintenanceData.endTime).toLocaleTimeString()}
              </p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm">
              <a href="/maintenance-status" className="text-xs">
                Ver detalles
              </a>
            </Button>
            <button
              onClick={handleClose}
              className="p-1 hover:bg-black/5 rounded"
              aria-label="Cerrar banner"
            >
              <FiX className="h-4 w-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

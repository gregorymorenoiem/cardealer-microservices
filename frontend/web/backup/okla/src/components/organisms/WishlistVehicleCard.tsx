import { memo } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { formatPrice } from '@/utils/formatters';
import { 
  FiTrash2, FiEdit3, FiFolder, FiX, FiCheck 
} from 'react-icons/fi';
import type { Vehicle } from '@/data/mockVehicles';

interface WishlistFolder {
  id: string;
  name: string;
  vehicleIds: string[];
}

interface WishlistVehicleCardProps {
  vehicle: Vehicle;
  note: string;
  isEditingNote: boolean;
  editingNoteText: string;
  vehicleFolders: WishlistFolder[];
  allFolders: WishlistFolder[];
  onRemoveFromFavorites: (vehicleId: string) => void;
  onStartEditNote: (vehicleId: string, note: string) => void;
  onSaveNote: (vehicleId: string) => void;
  onCancelEditNote: () => void;
  onDeleteNote: (vehicleId: string) => void;
  onNoteTextChange: (text: string) => void;
  onAddToFolder: (vehicleId: string, folderId: string) => void;
  onRemoveFromFolder: (vehicleId: string, folderId: string) => void;
}

const WishlistVehicleCard = memo(({
  vehicle,
  note,
  isEditingNote,
  editingNoteText,
  vehicleFolders,
  allFolders,
  onRemoveFromFavorites,
  onStartEditNote,
  onSaveNote,
  onCancelEditNote,
  onDeleteNote,
  onNoteTextChange,
  onAddToFolder,
  onRemoveFromFolder,
}: WishlistVehicleCardProps) => {
  const { t } = useTranslation(['vehicles', 'common']);
  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow">
      <div className="flex flex-col md:flex-row">
        {/* Image */}
        <Link to={`/vehicles/${vehicle.id}`} className="md:w-1/3">
          <img
            src={vehicle.images[0]}
            alt={`${vehicle.make} ${vehicle.model}`}
            className="w-full h-48 md:h-full object-cover hover:opacity-90 transition-opacity"
          />
        </Link>

        {/* Content */}
        <div className="flex-1 p-6">
          <div className="flex items-start justify-between mb-4">
            <Link to={`/vehicles/${vehicle.id}`}>
              <h3 className="text-2xl font-bold text-gray-900 hover:text-primary transition-colors">
                {vehicle.year} {vehicle.make} {vehicle.model}
              </h3>
              <p className="text-gray-600 mt-1">{vehicle.location}</p>
            </Link>
            <button
              onClick={() => onRemoveFromFavorites(vehicle.id)}
              className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
            >
              <FiTrash2 className="text-xl" />
            </button>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4 text-sm">
            <div>
              <span className="text-gray-600">{t('common:labels.price')}</span>
              <p className="font-semibold text-lg text-primary">
                {formatPrice(vehicle.price)}
              </p>
            </div>
            <div>
              <span className="text-gray-600">{t('vehicles:filters.mileage')}</span>
              <p className="font-semibold">{vehicle.mileage.toLocaleString()} mi</p>
            </div>
            <div>
              <span className="text-gray-600">{t('vehicles:filters.transmission')}</span>
              <p className="font-semibold">{vehicle.transmission}</p>
            </div>
            <div>
              <span className="text-gray-600">{t('vehicles:filters.fuelType')}</span>
              <p className="font-semibold">{vehicle.fuelType}</p>
            </div>
          </div>

          {/* Folders tags */}
          {vehicleFolders.length > 0 && (
            <div className="flex flex-wrap gap-2 mb-4">
              {vehicleFolders.map(folder => (
                <span
                  key={folder.id}
                  className="flex items-center gap-1 px-2 py-1 bg-blue-50 text-blue-700 rounded text-xs font-medium"
                >
                  <FiFolder className="text-xs" />
                  {folder.name}
                  <button
                    onClick={() => onRemoveFromFolder(vehicle.id, folder.id)}
                    className="ml-1 hover:text-blue-900"
                  >
                    <FiX className="text-xs" />
                  </button>
                </span>
              ))}
            </div>
          )}

          {/* Add to folder */}
          {allFolders.length > 1 && (
            <div className="mb-4">
              <select
                onChange={(e) => {
                  if (e.target.value) {
                    onAddToFolder(vehicle.id, e.target.value);
                    e.target.value = '';
                  }
                }}
                className="text-sm px-3 py-1.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                defaultValue=""
              >
                <option value="" disabled>{t('common:wishlist.addToFolder')}</option>
                {allFolders.filter(f => f.id !== 'all' && !f.vehicleIds.includes(vehicle.id)).map(folder => (
                  <option key={folder.id} value={folder.id}>{folder.name}</option>
                ))}
              </select>
            </div>
          )}

          {/* Notes */}
          <div className="border-t border-gray-200 pt-4">
            {isEditingNote ? (
              <div className="space-y-2">
                <textarea
                  value={editingNoteText}
                  onChange={(e) => onNoteTextChange(e.target.value)}
                  placeholder={t('common:wishlist.addNotePlaceholder')}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent resize-none text-sm"
                  autoFocus
                />
                <div className="flex gap-2">
                  <button
                    onClick={() => onSaveNote(vehicle.id)}
                    className="flex items-center gap-1 px-3 py-1.5 bg-primary text-white rounded-lg hover:bg-primary-hover transition-colors text-sm font-semibold"
                  >
                    <FiCheck />
                    {t('common:buttons.save')}
                  </button>
                  <button
                    onClick={onCancelEditNote}
                    className="px-3 py-1.5 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors text-sm font-semibold"
                  >
                    {t('common:buttons.cancel')}
                  </button>
                </div>
              </div>
            ) : note ? (
              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-3">
                <div className="flex items-start justify-between gap-2">
                  <p className="text-sm text-gray-700 flex-1">{note}</p>
                  <div className="flex gap-1">
                    <button
                      onClick={() => onStartEditNote(vehicle.id, note)}
                      className="p-1 text-gray-600 hover:text-primary transition-colors"
                    >
                      <FiEdit3 className="text-sm" />
                    </button>
                    <button
                      onClick={() => onDeleteNote(vehicle.id)}
                      className="p-1 text-gray-600 hover:text-red-600 transition-colors"
                    >
                      <FiX className="text-sm" />
                    </button>
                  </div>
                </div>
              </div>
            ) : (
              <button
                onClick={() => onStartEditNote(vehicle.id, '')}
                className="text-sm text-gray-500 hover:text-primary transition-colors flex items-center gap-1"
              >
                <FiEdit3 />
                {t('common:wishlist.addNote')}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
});

WishlistVehicleCard.displayName = 'WishlistVehicleCard';

export default WishlistVehicleCard;

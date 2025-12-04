import { useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import MainLayout from '@/layouts/MainLayout';
import { useFavorites } from '@/hooks/useFavorites';
import { mockVehicles } from '@/data/mockVehicles';
import WishlistVehicleCard from '@/components/organisms/WishlistVehicleCard';
import { 
  FiHeart, FiFolderPlus, FiCheck,
  FiShare2, FiX, FiCopy, FiMail, FiMessageCircle, FiTrash2, FiFolder
} from 'react-icons/fi';

interface VehicleNote {
  vehicleId: string;
  note: string;
}

interface WishlistFolder {
  id: string;
  name: string;
  vehicleIds: string[];
}

export default function WishlistPage() {
  const { favorites, removeFavorite } = useFavorites();
  const [notes, setNotes] = useState<VehicleNote[]>([]);
  
  const clearFavorites = useCallback(() => {
    favorites.forEach(id => removeFavorite(id));
  }, [favorites, removeFavorite]);
  const [folders, setFolders] = useState<WishlistFolder[]>([
    { id: 'all', name: 'All Favorites', vehicleIds: [] },
  ]);
  const [activeFolder, setActiveFolder] = useState('all');
  const [editingNote, setEditingNote] = useState<string | null>(null);
  const [editingNoteText, setEditingNoteText] = useState('');
  const [showShareModal, setShowShareModal] = useState(false);
  const [showNewFolderModal, setShowNewFolderModal] = useState(false);
  const [newFolderName, setNewFolderName] = useState('');
  const [shareLink, setShareLink] = useState('');
  const [copied, setCopied] = useState(false);

  const favoriteVehicles = mockVehicles.filter(v => favorites.includes(v.id));
  
  // Filter vehicles by folder
  const displayedVehicles = activeFolder === 'all' 
    ? favoriteVehicles
    : favoriteVehicles.filter(v => {
        const folder = folders.find(f => f.id === activeFolder);
        return folder?.vehicleIds.includes(v.id);
      });

  const getNoteForVehicle = (vehicleId: string) => {
    return notes.find(n => n.vehicleId === vehicleId)?.note || '';
  };

  const handleSaveNote = useCallback((vehicleId: string) => {
    const existingNoteIndex = notes.findIndex(n => n.vehicleId === vehicleId);
    if (existingNoteIndex >= 0) {
      const newNotes = [...notes];
      newNotes[existingNoteIndex] = { vehicleId, note: editingNoteText };
      setNotes(newNotes);
    } else {
      setNotes([...notes, { vehicleId, note: editingNoteText }]);
    }
    setEditingNote(null);
    setEditingNoteText('');
  }, [notes, editingNoteText]);

  const handleDeleteNote = useCallback((vehicleId: string) => {
    setNotes(notes.filter(n => n.vehicleId !== vehicleId));
  }, [notes]);

  const handleStartEditNote = useCallback((vehicleId: string, note: string) => {
    setEditingNote(vehicleId);
    setEditingNoteText(note);
  }, []);

  const handleCancelEditNote = useCallback(() => {
    setEditingNote(null);
    setEditingNoteText('');
  }, []);

  const handleNoteTextChange = useCallback((text: string) => {
    setEditingNoteText(text);
  }, []);

  const handleCreateFolder = () => {
    if (!newFolderName.trim()) return;
    const newFolder: WishlistFolder = {
      id: Date.now().toString(),
      name: newFolderName.trim(),
      vehicleIds: [],
    };
    setFolders([...folders, newFolder]);
    setNewFolderName('');
    setShowNewFolderModal(false);
  };

  const handleAddToFolder = useCallback((vehicleId: string, folderId: string) => {
    const folder = folders.find(f => f.id === folderId);
    if (!folder || folder.vehicleIds.includes(vehicleId)) return;
    
    const newFolders = folders.map(f => 
      f.id === folderId 
        ? { ...f, vehicleIds: [...f.vehicleIds, vehicleId] }
        : f
    );
    setFolders(newFolders);
  }, [folders]);

  const handleRemoveFromFolder = useCallback((vehicleId: string, folderId: string) => {
    const newFolders = folders.map(f => 
      f.id === folderId 
        ? { ...f, vehicleIds: f.vehicleIds.filter(id => id !== vehicleId) }
        : f
    );
    setFolders(newFolders);
  }, [folders]);

  const handleShareWishlist = () => {
    const vehicleIds = displayedVehicles.map(v => v.id).join(',');
    const link = `${window.location.origin}/wishlist/shared?vehicles=${vehicleIds}`;
    setShareLink(link);
    setShowShareModal(true);
  };

  const handleCopyLink = () => {
    navigator.clipboard.writeText(shareLink);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleShareEmail = () => {
    const subject = 'Check out my wishlist!';
    const body = `I've shared my car wishlist with you:\n\n${shareLink}`;
    window.location.href = `mailto:?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`;
  };

  return (
    <MainLayout>
      <div className="bg-gray-50 min-h-screen py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-3">
                <FiHeart className="text-3xl text-primary fill-primary" />
                <h1 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900">
                  My Wishlist
                </h1>
              </div>
              <div className="flex gap-2">
                {favorites.length > 0 && (
                  <>
                    <button
                      onClick={handleShareWishlist}
                      className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors font-semibold"
                    >
                      <FiShare2 />
                      <span className="hidden sm:inline">Share</span>
                    </button>
                    <button
                      onClick={() => setShowNewFolderModal(true)}
                      className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors font-semibold"
                    >
                      <FiFolderPlus />
                      <span className="hidden sm:inline">New Folder</span>
                    </button>
                    <button
                      onClick={clearFavorites}
                      className="flex items-center gap-2 px-4 py-2 bg-red-50 text-red-600 rounded-lg hover:bg-red-100 transition-colors font-semibold"
                    >
                      <FiTrash2 />
                      <span className="hidden sm:inline">Clear All</span>
                    </button>
                  </>
                )}
              </div>
            </div>
            <p className="text-gray-600">
              {favorites.length === 0 
                ? 'Start adding vehicles to your wishlist'
                : `${favorites.length} vehicle${favorites.length !== 1 ? 's' : ''} saved`
              }
            </p>
          </div>

          {/* Folders */}
          {folders.length > 1 && (
            <div className="mb-6">
              <div className="flex gap-2 overflow-x-auto pb-2">
                {folders.map(folder => (
                  <button
                    key={folder.id}
                    onClick={() => setActiveFolder(folder.id)}
                    className={`flex items-center gap-2 px-4 py-2 rounded-lg font-semibold whitespace-nowrap transition-colors ${
                      activeFolder === folder.id
                        ? 'bg-primary text-white'
                        : 'bg-white text-gray-700 hover:bg-gray-50 border border-gray-300'
                    }`}
                  >
                    <FiFolder />
                    {folder.name}
                    {folder.id !== 'all' && (
                      <span className="text-xs opacity-75">
                        ({folder.vehicleIds.length})
                      </span>
                    )}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Content */}
          {displayedVehicles.length > 0 ? (
            <div className="grid grid-cols-1 gap-6">
              {displayedVehicles.map((vehicle) => {
                const note = getNoteForVehicle(vehicle.id);
                const isEditingThisNote = editingNote === vehicle.id;
                const vehicleFolders = folders.filter(f => 
                  f.id !== 'all' && f.vehicleIds.includes(vehicle.id)
                );

                return (
                  <WishlistVehicleCard
                    key={vehicle.id}
                    vehicle={vehicle}
                    note={note}
                    isEditingNote={isEditingThisNote}
                    editingNoteText={editingNoteText}
                    vehicleFolders={vehicleFolders}
                    allFolders={folders}
                    onRemoveFromFavorites={removeFavorite}
                    onStartEditNote={handleStartEditNote}
                    onSaveNote={handleSaveNote}
                    onCancelEditNote={handleCancelEditNote}
                    onDeleteNote={handleDeleteNote}
                    onNoteTextChange={handleNoteTextChange}
                    onAddToFolder={handleAddToFolder}
                    onRemoveFromFolder={handleRemoveFromFolder}
                  />
                );
              })}
            </div>
          ) : (
            <div className="bg-white rounded-lg shadow-md p-12 text-center">
              <FiHeart className="text-6xl text-gray-400 mx-auto mb-4" />
              <h3 className="text-2xl font-bold text-gray-900 mb-2">
                {activeFolder === 'all' ? 'Your wishlist is empty' : 'No vehicles in this folder'}
              </h3>
              <p className="text-gray-600 mb-6">
                {activeFolder === 'all' 
                  ? 'Start exploring vehicles and add them to your wishlist'
                  : 'Add vehicles to this folder to organize your favorites'
                }
              </p>
              <Link
                to="/browse"
                className="inline-block px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-hover transition-colors font-semibold"
              >
                Browse Vehicles
              </Link>
            </div>
          )}
        </div>
      </div>

      {/* New Folder Modal */}
      {showNewFolderModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-bold text-gray-900">Create New Folder</h2>
              <button
                onClick={() => setShowNewFolderModal(false)}
                className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
              >
                <FiX className="text-xl" />
              </button>
            </div>
            <input
              type="text"
              value={newFolderName}
              onChange={(e) => setNewFolderName(e.target.value)}
              placeholder="Folder name..."
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent mb-4"
              autoFocus
            />
            <div className="flex gap-2">
              <button
                onClick={handleCreateFolder}
                disabled={!newFolderName.trim()}
                className="flex-1 px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-hover transition-colors font-semibold disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Create
              </button>
              <button
                onClick={() => setShowNewFolderModal(false)}
                className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors font-semibold"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Share Modal */}
      {showShareModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-bold text-gray-900">Share Wishlist</h2>
              <button
                onClick={() => setShowShareModal(false)}
                className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
              >
                <FiX className="text-xl" />
              </button>
            </div>
            
            <p className="text-gray-600 mb-4">
              Share your wishlist with friends and family
            </p>

            <div className="bg-gray-50 rounded-lg p-3 mb-4 flex items-center gap-2">
              <input
                type="text"
                value={shareLink}
                readOnly
                className="flex-1 bg-transparent text-sm text-gray-700 outline-none"
              />
              <button
                onClick={handleCopyLink}
                className="p-2 text-primary hover:bg-white rounded-lg transition-colors"
              >
                {copied ? <FiCheck /> : <FiCopy />}
              </button>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <button
                onClick={handleShareEmail}
                className="flex items-center justify-center gap-2 px-4 py-3 bg-blue-50 text-blue-600 rounded-lg hover:bg-blue-100 transition-colors font-semibold"
              >
                <FiMail />
                Email
              </button>
              <button
                onClick={() => {
                  if (navigator.share) {
                    navigator.share({
                      title: 'My Car Wishlist',
                      text: 'Check out my wishlist!',
                      url: shareLink,
                    });
                  }
                }}
                className="flex items-center justify-center gap-2 px-4 py-3 bg-green-50 text-green-600 rounded-lg hover:bg-green-100 transition-colors font-semibold"
              >
                <FiMessageCircle />
                More
              </button>
            </div>
          </div>
        </div>
      )}
    </MainLayout>
  );
}

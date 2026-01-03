import { useState } from 'react';
import { FiX, FiUpload } from 'react-icons/fi';
import StarRating from '@/components/atoms/StarRating';

interface ReviewFormProps {
  onSubmit: (data: ReviewFormData) => void;
  onCancel: () => void;
}

export interface ReviewFormData {
  rating: number;
  title: string;
  comment: string;
  pros: string[];
  cons: string[];
  photos: File[];
}

interface InputItem {
  id: string;
  value: string;
}

export default function ReviewForm({ onSubmit, onCancel }: ReviewFormProps) {
  const [rating, setRating] = useState(0);
  const [title, setTitle] = useState('');
  const [comment, setComment] = useState('');
  const [pros, setPros] = useState<InputItem[]>(() => [{ id: `pro-${Date.now()}`, value: '' }]);
  const [cons, setCons] = useState<InputItem[]>(() => [{ id: `con-${Date.now()}`, value: '' }]);
  const [photos, setPhotos] = useState<File[]>([]);

  const handleProChange = (id: string, value: string) => {
    setPros(pros.map(p => p.id === id ? { ...p, value } : p));
  };

  const handleConChange = (id: string, value: string) => {
    setCons(cons.map(c => c.id === id ? { ...c, value } : c));
  };

  const addPro = () => {
    if (pros.length < 5) {
      setPros([...pros, { id: `pro-${Date.now()}-${Math.random()}`, value: '' }]);
    }
  };

  const addCon = () => {
    if (cons.length < 5) {
      setCons([...cons, { id: `con-${Date.now()}-${Math.random()}`, value: '' }]);
    }
  };

  const removePro = (id: string) => {
    setPros(pros.filter(p => p.id !== id));
  };

  const removeCon = (id: string) => {
    setCons(cons.filter(c => c.id !== id));
  };

  const handlePhotoUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const newPhotos = Array.from(e.target.files);
      setPhotos([...photos, ...newPhotos].slice(0, 5)); // Max 5 photos
    }
  };

  const removePhoto = (index: number) => {
    setPhotos(photos.filter((_, i) => i !== index));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (rating === 0) {
      alert('Please select a rating');
      return;
    }

    if (!title.trim() || !comment.trim()) {
      alert('Please fill in all required fields');
      return;
    }

    onSubmit({
      rating,
      title: title.trim(),
      comment: comment.trim(),
      pros: pros.map(p => p.value).filter(v => v.trim() !== ''),
      cons: cons.map(c => c.value).filter(v => v.trim() !== ''),
      photos,
    });
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
          <h2 className="text-2xl font-bold text-gray-900">Write a Review</h2>
          <button
            onClick={onCancel}
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <FiX className="text-xl" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {/* Rating */}
          <div>
            <label className="block text-sm font-semibold text-gray-900 mb-2">
              Overall Rating <span className="text-red-500">*</span>
            </label>
            <StarRating
              rating={rating}
              size="lg"
              interactive
              onChange={setRating}
            />
          </div>

          {/* Title */}
          <div>
            <label htmlFor="title" className="block text-sm font-semibold text-gray-900 mb-2">
              Review Title <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Sum up your experience"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              maxLength={100}
            />
          </div>

          {/* Comment */}
          <div>
            <label htmlFor="comment" className="block text-sm font-semibold text-gray-900 mb-2">
              Your Review <span className="text-red-500">*</span>
            </label>
            <textarea
              id="comment"
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              placeholder="Share details about your experience with this vehicle"
              rows={5}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent resize-none"
              maxLength={1000}
            />
            <p className="text-xs text-gray-500 mt-1">
              {comment.length}/1000 characters
            </p>
          </div>

          {/* Pros */}
          <div>
            <label className="block text-sm font-semibold text-gray-900 mb-2">
              Pros (Optional)
            </label>
            <div className="space-y-2">
              {pros.map((pro) => (
                <div key={pro.id} className="flex gap-2">
                  <input
                    type="text"
                    value={pro.value}
                    onChange={(e) => handleProChange(pro.id, e.target.value)}
                    placeholder="What did you like?"
                    className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent"
                  />
                  {pros.length > 1 && (
                    <button
                      type="button"
                      onClick={() => removePro(pro.id)}
                      className="p-2 text-red-600 hover:bg-red-50 rounded-lg"
                    >
                      <FiX />
                    </button>
                  )}
                </div>
              ))}
            </div>
            {pros.length < 5 && (
              <button
                type="button"
                onClick={addPro}
                className="mt-2 text-sm text-green-600 hover:text-green-700 font-medium"
              >
                + Add another pro
              </button>
            )}
          </div>

          {/* Cons */}
          <div>
            <label className="block text-sm font-semibold text-gray-900 mb-2">
              Cons (Optional)
            </label>
            <div className="space-y-2">
              {cons.map((con) => (
                <div key={con.id} className="flex gap-2">
                  <input
                    type="text"
                    value={con.value}
                    onChange={(e) => handleConChange(con.id, e.target.value)}
                    placeholder="What could be improved?"
                    className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent"
                  />
                  {cons.length > 1 && (
                    <button
                      type="button"
                      onClick={() => removeCon(con.id)}
                      className="p-2 text-red-600 hover:bg-red-50 rounded-lg"
                    >
                      <FiX />
                    </button>
                  )}
                </div>
              ))}
            </div>
            {cons.length < 5 && (
              <button
                type="button"
                onClick={addCon}
                className="mt-2 text-sm text-red-600 hover:text-red-700 font-medium"
              >
                + Add another con
              </button>
            )}
          </div>

          {/* Photos */}
          <div>
            <label className="block text-sm font-semibold text-gray-900 mb-2">
              Photos (Optional)
            </label>
            <div className="space-y-3">
              {photos.length > 0 && (
                <div className="grid grid-cols-3 gap-2">
                  {photos.map((photo, index) => (
                    <div key={index} className="relative group">
                      <img
                        src={URL.createObjectURL(photo)}
                        alt={`Upload ${index + 1}`}
                        className="w-full h-24 object-cover rounded-lg"
                      />
                      <button
                        type="button"
                        onClick={() => removePhoto(index)}
                        className="absolute top-1 right-1 p-1 bg-red-600 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                      >
                        <FiX className="text-sm" />
                      </button>
                    </div>
                  ))}
                </div>
              )}
              
              {photos.length < 5 && (
                <label className="flex flex-col items-center justify-center w-full h-32 border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:bg-gray-50 transition-colors">
                  <FiUpload className="text-3xl text-gray-400 mb-2" />
                  <span className="text-sm text-gray-600">
                    Upload photos ({photos.length}/5)
                  </span>
                  <input
                    type="file"
                    accept="image/*"
                    multiple
                    onChange={handlePhotoUpload}
                    className="hidden"
                  />
                </label>
              )}
            </div>
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-4 border-t border-gray-200">
            <button
              type="button"
              onClick={onCancel}
              className="flex-1 px-6 py-3 border border-gray-300 rounded-lg font-semibold text-gray-700 hover:bg-gray-50 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="flex-1 px-6 py-3 bg-primary text-white rounded-lg font-semibold hover:bg-primary-hover transition-colors"
            >
              Submit Review
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

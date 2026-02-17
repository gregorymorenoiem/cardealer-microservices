/**
 * Create Seller Profile Page
 * Page for individual users to create their seller profile
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  User,
  ArrowLeft,
  Check,
  Sparkles,
  Shield,
  Eye,
  MessageCircle,
  TrendingUp,
  ChevronRight,
} from 'lucide-react';
import { useCreateSeller } from '@/hooks/useSeller';
import { useAuth } from '@/hooks/useAuth';
import { SellerForm } from '@/components/dealer/SellerForm';
import type { CreateSellerRequest } from '@/types/dealer';
import toast from 'react-hot-toast';

export const CreateSellerPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [showForm, setShowForm] = useState(false);
  
  const createSellerMutation = useCreateSeller();

  const handleSubmit = async (data: CreateSellerRequest) => {
    if (!user?.id) return;
    
    try {
      await createSellerMutation.mutateAsync({
        ...data,
        userId: user.id,
      });
      
      toast.success('Seller profile created successfully!');
      navigate('/profile');
    } catch (error) {
      toast.error('Failed to create seller profile');
    }
  };

  const benefits = [
    {
      icon: Eye,
      title: 'Increase Visibility',
      description: 'Your listings will be more visible to potential buyers',
      color: 'text-blue-600 bg-blue-100',
    },
    {
      icon: Shield,
      title: 'Build Trust',
      description: 'Verified profiles get more responses and faster sales',
      color: 'text-green-600 bg-green-100',
    },
    {
      icon: MessageCircle,
      title: 'Better Communication',
      description: 'Buyers can reach you through multiple channels',
      color: 'text-purple-600 bg-purple-100',
    },
    {
      icon: TrendingUp,
      title: 'Track Performance',
      description: 'See detailed stats about your listings and engagement',
      color: 'text-amber-600 bg-amber-100',
    },
  ];

  if (!showForm) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-purple-50 to-indigo-50">
        <div className="max-w-4xl mx-auto px-4 py-12">
          <button
            onClick={() => navigate(-1)}
            className="flex items-center gap-2 text-gray-500 hover:text-gray-700 mb-8 transition-colors"
          >
            <ArrowLeft className="h-5 w-5" />
            Back
          </button>

          <div className="text-center mb-12">
            <div className="w-20 h-20 bg-gradient-to-br from-purple-500 to-indigo-600 rounded-full flex items-center justify-center mx-auto mb-6">
              <User className="h-10 w-10 text-white" />
            </div>
            
            <h1 className="text-4xl font-bold text-gray-900 mb-4">
              Start Selling on CarDealer
            </h1>
            <p className="text-xl text-gray-600 max-w-2xl mx-auto">
              Create your seller profile to connect with buyers and start selling your vehicles today.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-12">
            {benefits.map(({ icon: Icon, title, description, color }) => (
              <div
                key={title}
                className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex items-start gap-4"
              >
                <div className={`p-3 rounded-lg ${color}`}>
                  <Icon className="h-6 w-6" />
                </div>
                <div>
                  <h3 className="font-semibold text-gray-900 mb-1">{title}</h3>
                  <p className="text-gray-500 text-sm">{description}</p>
                </div>
              </div>
            ))}
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-8 mb-8">
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Sparkles className="h-5 w-5 text-amber-500" />
              What you'll need
            </h3>
            <ul className="space-y-3 text-gray-600">
              <li className="flex items-center gap-3">
                <Check className="h-5 w-5 text-green-500" />
                A valid email address
              </li>
              <li className="flex items-center gap-3">
                <Check className="h-5 w-5 text-green-500" />
                Phone number (optional but recommended)
              </li>
              <li className="flex items-center gap-3">
                <Check className="h-5 w-5 text-green-500" />
                Your location (city and state)
              </li>
              <li className="flex items-center gap-3">
                <Check className="h-5 w-5 text-green-500" />
                A short bio about yourself (optional)
              </li>
            </ul>
          </div>

          <div className="flex justify-center">
            <button
              onClick={() => setShowForm(true)}
              className="px-8 py-4 bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700 text-white text-lg font-medium rounded-xl transition-colors inline-flex items-center gap-2"
            >
              Create My Seller Profile
              <ChevronRight className="h-5 w-5" />
            </button>
          </div>

          <p className="text-center text-sm text-gray-500 mt-6">
            Already a dealer? <a href="/dealer/onboarding" className="text-blue-600 hover:underline">Create a dealer account instead</a>
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-4xl mx-auto px-4 py-12">
        <button
          onClick={() => setShowForm(false)}
          className="flex items-center gap-2 text-gray-500 hover:text-gray-700 mb-8 transition-colors"
        >
          <ArrowLeft className="h-5 w-5" />
          Back
        </button>

        <div className="text-center mb-8">
          <h2 className="text-3xl font-bold text-gray-900 mb-2">
            Create Your Seller Profile
          </h2>
          <p className="text-gray-600">
            Fill in your details to get started
          </p>
        </div>

        <SellerForm
          userId={user?.id}
          onSubmit={handleSubmit}
          isLoading={createSellerMutation.isPending}
          mode="create"
        />
      </div>
    </div>
  );
};

export default CreateSellerPage;

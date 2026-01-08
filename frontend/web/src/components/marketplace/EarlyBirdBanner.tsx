import { useEffect, useState } from 'react';
import { FiGift, FiClock, FiStar } from 'react-icons/fi';
import Button from '@/components/atoms/Button';

interface EarlyBirdBannerProps {
  onEnroll?: () => void;
}

export const EarlyBirdBanner = ({ onEnroll }: EarlyBirdBannerProps) => {
  const [timeLeft, setTimeLeft] = useState({
    days: 0,
    hours: 0,
    minutes: 0,
    seconds: 0,
  });
  const [isEnrolled, setIsEnrolled] = useState(false);
  const [loading, setLoading] = useState(true);

  // Deadline: January 31, 2026 23:59:59
  const deadline = new Date('2026-01-31T23:59:59Z');

  useEffect(() => {
    // Check enrollment status
    const token = localStorage.getItem('authToken');
    if (token) {
      fetch('https://api.okla.com.do/api/billing/earlybird/status', {
        headers: { Authorization: `Bearer ${token}` },
      })
        .then((res) => res.json())
        .then((data) => {
          setIsEnrolled(data.isEnrolled);
          setLoading(false);
        })
        .catch(() => setLoading(false));
    } else {
      setLoading(false);
    }

    // Update countdown every second
    const interval = setInterval(() => {
      const now = new Date();
      const diff = deadline.getTime() - now.getTime();

      if (diff <= 0) {
        clearInterval(interval);
        setTimeLeft({ days: 0, hours: 0, minutes: 0, seconds: 0 });
        return;
      }

      setTimeLeft({
        days: Math.floor(diff / (1000 * 60 * 60 * 24)),
        hours: Math.floor((diff / (1000 * 60 * 60)) % 24),
        minutes: Math.floor((diff / (1000 * 60)) % 60),
        seconds: Math.floor((diff / 1000) % 60),
      });
    }, 1000);

    return () => clearInterval(interval);
  }, []);

  const handleEnroll = async () => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      window.location.href = '/login?redirect=/earlybird';
      return;
    }

    try {
      const response = await fetch('https://api.okla.com.do/api/billing/earlybird/enroll', {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        setIsEnrolled(true);
        onEnroll?.();
      }
    } catch (error) {
      console.error('Failed to enroll:', error);
    }
  };

  if (loading) return null;
  if (isEnrolled) return null; // Don't show if already enrolled
  if (timeLeft.days === 0 && timeLeft.hours === 0) return null; // Expired

  return (
    <div className="w-full bg-gradient-to-r from-yellow-400 via-orange-500 to-red-500 text-white">
      <div className="container mx-auto px-4 py-4">
        <div className="flex flex-col md:flex-row items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-white/20 rounded-full">
              <FiGift className="h-6 w-6" />
            </div>
            <div>
              <div className="flex items-center gap-2 mb-1">
                <h3 className="text-lg font-bold">¡OFERTA DE LANZAMIENTO!</h3>
                <span className="bg-white text-orange-600 px-2 py-1 rounded-full text-sm font-semibold flex items-center gap-1">
                  <FiStar className="h-3 w-3" />
                  Limitado
                </span>
              </div>
              <p className="text-sm opacity-90">
                3 MESES GRATIS + Badge Exclusivo de "Miembro Fundador" 🏆
              </p>
            </div>
          </div>

          <div className="flex items-center gap-4">
            {/* Countdown */}
            <div className="flex items-center gap-2 bg-white/20 rounded-lg px-4 py-2">
              <FiClock className="h-5 w-5" />
              <div className="flex gap-1 text-center">
                <div>
                  <div className="text-xl font-bold">{timeLeft.days}</div>
                  <div className="text-xs opacity-75">días</div>
                </div>
                <div className="mx-1 text-xl">:</div>
                <div>
                  <div className="text-xl font-bold">{String(timeLeft.hours).padStart(2, '0')}</div>
                  <div className="text-xs opacity-75">hrs</div>
                </div>
                <div className="mx-1 text-xl">:</div>
                <div>
                  <div className="text-xl font-bold">
                    {String(timeLeft.minutes).padStart(2, '0')}
                  </div>
                  <div className="text-xs opacity-75">min</div>
                </div>
              </div>
            </div>

            <Button
              onClick={handleEnroll}
              size="lg"
              className="bg-white text-orange-600 hover:bg-orange-50 font-bold"
            >
              Inscribirse Ahora
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

/**
 * VideoHelpButton — Contextual Help Button with Video Tutorial Dialog
 *
 * Displays a small "?" or "📹 Ayuda" button that opens a Dialog
 * with an auto-playing HTML5 video tutorial for the current dealer section.
 *
 * - Uses native <video> element (no YouTube, loads in < 2s from CDN)
 * - Videos are < 90 seconds in Dominican Spanish
 * - Preloads metadata for instant playback
 * - Shows topic list below the video
 */

'use client';

import * as React from 'react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { CircleHelp, Play, Pause, Volume2, VolumeX } from 'lucide-react';
import {
  getDealerTutorial,
  formatTutorialDuration,
  type DealerTutorial,
} from '@/config/dealer-tutorials';

interface VideoHelpButtonProps {
  /** The section key matching the dealer tutorial config */
  sectionKey: string;
  /** Optional: override the default button variant */
  variant?: 'icon' | 'text' | 'compact';
  /** Optional: additional CSS classes */
  className?: string;
}

export function VideoHelpButton({
  sectionKey,
  variant = 'compact',
  className,
}: VideoHelpButtonProps) {
  const tutorial = getDealerTutorial(sectionKey);
  const videoRef = React.useRef<HTMLVideoElement>(null);
  const [isPlaying, setIsPlaying] = React.useState(false);
  const [isMuted, setIsMuted] = React.useState(false);
  const [open, setOpen] = React.useState(false);

  // Reset video when dialog closes
  React.useEffect(() => {
    if (!open && videoRef.current) {
      videoRef.current.pause();
      videoRef.current.currentTime = 0;
      setIsPlaying(false);
    }
  }, [open]);

  if (!tutorial) {
    return null; // No tutorial configured for this section
  }

  const handlePlayPause = () => {
    if (!videoRef.current) return;
    if (videoRef.current.paused) {
      videoRef.current.play();
      setIsPlaying(true);
    } else {
      videoRef.current.pause();
      setIsPlaying(false);
    }
  };

  const handleMuteToggle = () => {
    if (!videoRef.current) return;
    videoRef.current.muted = !videoRef.current.muted;
    setIsMuted(!isMuted);
  };

  const triggerButton = () => {
    switch (variant) {
      case 'icon':
        return (
          <Button
            variant="ghost"
            size="icon"
            className={`text-muted-foreground hover:text-primary h-8 w-8 ${className || ''}`}
            title={`Ayuda: ${tutorial.title}`}
          >
            <CircleHelp className="h-4 w-4" />
          </Button>
        );
      case 'text':
        return (
          <Button
            variant="outline"
            size="sm"
            className={`text-muted-foreground hover:text-primary ${className || ''}`}
          >
            <CircleHelp className="mr-1.5 h-4 w-4" />
            Ver Tutorial
          </Button>
        );
      case 'compact':
      default:
        return (
          <Button
            variant="ghost"
            size="sm"
            className={`text-muted-foreground hover:text-primary gap-1.5 ${className || ''}`}
            title={`Ayuda: ${tutorial.title}`}
          >
            <CircleHelp className="h-4 w-4" />
            <span className="hidden sm:inline">Ayuda</span>
          </Button>
        );
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>{triggerButton()}</DialogTrigger>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <CircleHelp className="text-primary h-5 w-5" />
            {tutorial.title}
          </DialogTitle>
          <DialogDescription>
            {tutorial.description}{' '}
            <span className="text-muted-foreground text-xs">
              ({formatTutorialDuration(tutorial.durationSeconds)})
            </span>
          </DialogDescription>
        </DialogHeader>

        <VideoPlayer
          tutorial={tutorial}
          videoRef={videoRef}
          isPlaying={isPlaying}
          isMuted={isMuted}
          onPlayPause={handlePlayPause}
          onMuteToggle={handleMuteToggle}
          onPlay={() => setIsPlaying(true)}
          onPause={() => setIsPlaying(false)}
          onEnded={() => setIsPlaying(false)}
        />

        {/* Topic list */}
        {tutorial.topics && tutorial.topics.length > 0 && (
          <div className="mt-2">
            <p className="text-muted-foreground mb-2 text-xs font-medium tracking-wide uppercase">
              En este video aprenderás:
            </p>
            <ul className="grid grid-cols-1 gap-1.5 sm:grid-cols-2">
              {tutorial.topics.map((topic, i) => (
                <li key={i} className="text-muted-foreground flex items-center gap-2 text-sm">
                  <span className="bg-primary/10 text-primary inline-flex h-5 w-5 flex-shrink-0 items-center justify-center rounded-full text-xs font-medium">
                    {i + 1}
                  </span>
                  {topic}
                </li>
              ))}
            </ul>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}

/** Internal video player sub-component */
function VideoPlayer({
  tutorial,
  videoRef,
  isPlaying,
  isMuted,
  onPlayPause,
  onMuteToggle,
  onPlay,
  onPause,
  onEnded,
}: {
  tutorial: DealerTutorial;
  videoRef: React.RefObject<HTMLVideoElement | null>;
  isPlaying: boolean;
  isMuted: boolean;
  onPlayPause: () => void;
  onMuteToggle: () => void;
  onPlay: () => void;
  onPause: () => void;
  onEnded: () => void;
}) {
  return (
    <div className="bg-muted relative overflow-hidden rounded-lg">
      {/* Native HTML5 video — preload metadata for instant start */}
      <video
        ref={videoRef}
        src={tutorial.videoUrl}
        poster={tutorial.posterUrl}
        preload="metadata"
        playsInline
        className="aspect-video w-full rounded-lg"
        onPlay={onPlay}
        onPause={onPause}
        onEnded={onEnded}
      >
        Tu navegador no soporta videos HTML5.
      </video>

      {/* Play overlay when paused */}
      {!isPlaying && (
        <button
          onClick={onPlayPause}
          className="absolute inset-0 flex items-center justify-center bg-black/30 transition-colors hover:bg-black/40"
          aria-label="Reproducir video"
        >
          <div className="bg-primary flex h-16 w-16 items-center justify-center rounded-full shadow-lg">
            <Play className="ml-1 h-8 w-8 text-white" fill="white" />
          </div>
        </button>
      )}

      {/* Minimal controls bar */}
      <div className="absolute right-0 bottom-0 left-0 flex items-center justify-between bg-gradient-to-t from-black/60 to-transparent px-3 py-2">
        <button
          onClick={onPlayPause}
          className="rounded-full p-1.5 text-white transition-colors hover:bg-white/20"
          aria-label={isPlaying ? 'Pausar' : 'Reproducir'}
        >
          {isPlaying ? <Pause className="h-4 w-4" /> : <Play className="h-4 w-4" />}
        </button>

        <div className="flex items-center gap-2">
          <span className="text-xs text-white/80">
            {formatTutorialDuration(tutorial.durationSeconds)}
          </span>
          <button
            onClick={onMuteToggle}
            className="rounded-full p-1.5 text-white transition-colors hover:bg-white/20"
            aria-label={isMuted ? 'Activar sonido' : 'Silenciar'}
          >
            {isMuted ? <VolumeX className="h-4 w-4" /> : <Volume2 className="h-4 w-4" />}
          </button>
        </div>
      </div>
    </div>
  );
}

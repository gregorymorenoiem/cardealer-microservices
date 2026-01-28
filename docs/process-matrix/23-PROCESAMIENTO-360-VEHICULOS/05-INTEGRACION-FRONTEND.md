# 📱 Integración con Frontend - Visor 360° de Vehículos

## 📋 Descripción

Este documento detalla cómo integrar el sistema de procesamiento 360° con las aplicaciones frontend (React Web y Flutter Mobile) para mostrar el visor interactivo de vehículos.

## 🎯 Componente Final: Vehicle360Viewer

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                             │
│                        VEHICLE 360° VIEWER                                  │
│                                                                             │
│   ┌───────────────────────────────────────────────────────────────────┐     │
│   │                                                                   │     │
│   │                         [FRAME ACTUAL]                           │     │
│   │                                                                   │     │
│   │                           ┌──────┐                               │     │
│   │                           │  🚗  │                               │     │
│   │                           │      │                               │     │
│   │                           │Toyota│                               │     │
│   │                           │Camry │                               │     │
│   │                           └──────┘                               │     │
│   │                                                                   │     │
│   │  ◄─────────────── DRAG TO ROTATE ───────────────►                │     │
│   │                                                                   │     │
│   └───────────────────────────────────────────────────────────────────┘     │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────┐       │
│   │ ○ ○ ● ○ ○ ○   [60°]   [Auto] [Zoom+] [Zoom-] [Fullscreen]      │       │
│   └─────────────────────────────────────────────────────────────────┘       │
│                                                                             │
│   Thumbnails:                                                               │
│   ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐                                │
│   │ 0° │ │60° │ │120°│ │180°│ │240°│ │300°│                                │
│   │    │ │ ●  │ │    │ │    │ │    │ │    │                                │
│   └────┘ └────┘ └────┘ └────┘ └────┘ └────┘                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 🌐 Frontend Web (React + TypeScript)

### 1. API Service

```typescript
// src/services/vehicle360Service.ts

import axios from "axios";

const API_BASE = process.env.REACT_APP_API_URL || "https://api.okla.com.do";

export interface Vehicle360Frame {
  index: number;
  angle: number;
  name: string;
  imageUrl: string;
  thumbnailUrl: string;
  width: number;
  height: number;
}

export interface Vehicle360ViewConfig {
  autoRotate: boolean;
  autoRotateSpeed: number;
  allowDrag: boolean;
  allowZoom: boolean;
  showAngleIndicator: boolean;
  preloadAll: boolean;
}

export interface Vehicle360View {
  vehicleId: string;
  isReady: boolean;
  createdAt: string;
  frames: Vehicle360Frame[];
  config: Vehicle360ViewConfig;
}

export interface CreateJobRequest {
  vehicleId: string;
  videoUrl: string;
  options?: {
    removeBackground?: boolean;
    video360Provider?: string;
    backgroundRemovalProvider?: string;
  };
}

export interface ProcessingJob {
  jobId: string;
  vehicleId: string;
  status: "Pending" | "Processing" | "Completed" | "Failed";
  progress: number;
  currentStep?: string;
  steps: ProcessingStep[];
  totalCostUsd?: number;
  errorMessage?: string;
}

export interface ProcessingStep {
  step: string;
  status: "Pending" | "Processing" | "Completed" | "Failed";
  progress?: number;
  total?: number;
  durationMs?: number;
  cost?: number;
}

export const vehicle360Service = {
  // Obtener vista 360° de un vehículo
  async getView(vehicleId: string): Promise<Vehicle360View | null> {
    try {
      const response = await axios.get<Vehicle360View>(
        `${API_BASE}/api/vehicle360/views/${vehicleId}`,
      );
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return null; // No tiene vista 360°
      }
      throw error;
    }
  },

  // Iniciar procesamiento 360°
  async startProcessing(request: CreateJobRequest): Promise<ProcessingJob> {
    const response = await axios.post<ProcessingJob>(
      `${API_BASE}/api/vehicle360/process`,
      request,
    );
    return response.data;
  },

  // Obtener estado del job
  async getJobStatus(jobId: string): Promise<ProcessingJob> {
    const response = await axios.get<ProcessingJob>(
      `${API_BASE}/api/vehicle360/jobs/${jobId}`,
    );
    return response.data;
  },

  // Subir video y procesar
  async uploadAndProcess(
    vehicleId: string,
    videoFile: File,
    options?: CreateJobRequest["options"],
  ): Promise<ProcessingJob> {
    const formData = new FormData();
    formData.append("video", videoFile);
    formData.append("vehicleId", vehicleId);

    if (options?.removeBackground !== undefined) {
      formData.append("removeBackground", String(options.removeBackground));
    }
    if (options?.video360Provider) {
      formData.append("video360Provider", options.video360Provider);
    }
    if (options?.backgroundRemovalProvider) {
      formData.append(
        "backgroundRemovalProvider",
        options.backgroundRemovalProvider,
      );
    }

    const response = await axios.post<ProcessingJob>(
      `${API_BASE}/api/vehicle360/upload-and-process`,
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
      },
    );
    return response.data;
  },
};
```

### 2. React Hook: useVehicle360

```typescript
// src/hooks/useVehicle360.ts

import { useState, useEffect, useCallback } from "react";
import {
  vehicle360Service,
  Vehicle360View,
  ProcessingJob,
} from "../services/vehicle360Service";

interface UseVehicle360Options {
  autoFetch?: boolean;
  pollInterval?: number; // Para seguir el progreso del procesamiento
}

interface UseVehicle360Return {
  view: Vehicle360View | null;
  job: ProcessingJob | null;
  isLoading: boolean;
  isProcessing: boolean;
  error: string | null;
  startProcessing: (videoUrl: string) => Promise<void>;
  uploadAndProcess: (videoFile: File) => Promise<void>;
  refetch: () => Promise<void>;
}

export function useVehicle360(
  vehicleId: string,
  options: UseVehicle360Options = {},
): UseVehicle360Return {
  const { autoFetch = true, pollInterval = 2000 } = options;

  const [view, setView] = useState<Vehicle360View | null>(null);
  const [job, setJob] = useState<ProcessingJob | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isProcessing =
    job?.status === "Pending" || job?.status === "Processing";

  // Fetch vista existente
  const fetchView = useCallback(async () => {
    if (!vehicleId) return;

    setIsLoading(true);
    setError(null);

    try {
      const data = await vehicle360Service.getView(vehicleId);
      setView(data);
    } catch (err) {
      setError("Error al cargar la vista 360°");
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  }, [vehicleId]);

  // Iniciar procesamiento con URL
  const startProcessing = useCallback(
    async (videoUrl: string) => {
      if (!vehicleId) return;

      setError(null);

      try {
        const newJob = await vehicle360Service.startProcessing({
          vehicleId,
          videoUrl,
          options: {
            removeBackground: true,
            video360Provider: "FfmpegApi",
            backgroundRemovalProvider: "ClipDrop",
          },
        });
        setJob(newJob);
      } catch (err) {
        setError("Error al iniciar el procesamiento");
        console.error(err);
      }
    },
    [vehicleId],
  );

  // Subir video y procesar
  const uploadAndProcess = useCallback(
    async (videoFile: File) => {
      if (!vehicleId) return;

      setError(null);

      try {
        const newJob = await vehicle360Service.uploadAndProcess(
          vehicleId,
          videoFile,
          {
            removeBackground: true,
          },
        );
        setJob(newJob);
      } catch (err) {
        setError("Error al subir y procesar el video");
        console.error(err);
      }
    },
    [vehicleId],
  );

  // Auto-fetch al montar
  useEffect(() => {
    if (autoFetch) {
      fetchView();
    }
  }, [autoFetch, fetchView]);

  // Poll job status mientras está procesando
  useEffect(() => {
    if (!job || !isProcessing) return;

    const intervalId = setInterval(async () => {
      try {
        const updatedJob = await vehicle360Service.getJobStatus(job.jobId);
        setJob(updatedJob);

        if (updatedJob.status === "Completed") {
          // Refetch la vista cuando termine
          await fetchView();
        }
      } catch (err) {
        console.error("Error polling job status:", err);
      }
    }, pollInterval);

    return () => clearInterval(intervalId);
  }, [job, isProcessing, pollInterval, fetchView]);

  return {
    view,
    job,
    isLoading,
    isProcessing,
    error,
    startProcessing,
    uploadAndProcess,
    refetch: fetchView,
  };
}
```

### 3. Componente: Vehicle360Viewer

```tsx
// src/components/Vehicle360Viewer.tsx

import React, { useState, useEffect, useRef, useCallback } from "react";
import { Vehicle360View, Vehicle360Frame } from "../services/vehicle360Service";
import { useVehicle360 } from "../hooks/useVehicle360";

interface Vehicle360ViewerProps {
  vehicleId: string;
  className?: string;
  showThumbnails?: boolean;
  showControls?: boolean;
  initialFrame?: number;
  onFrameChange?: (frame: Vehicle360Frame) => void;
}

export const Vehicle360Viewer: React.FC<Vehicle360ViewerProps> = ({
  vehicleId,
  className = "",
  showThumbnails = true,
  showControls = true,
  initialFrame = 0,
  onFrameChange,
}) => {
  const { view, isLoading, error } = useVehicle360(vehicleId);

  const [currentFrameIndex, setCurrentFrameIndex] = useState(initialFrame);
  const [isAutoRotating, setIsAutoRotating] = useState(false);
  const [isDragging, setIsDragging] = useState(false);
  const [zoomLevel, setZoomLevel] = useState(1);
  const [isFullscreen, setIsFullscreen] = useState(false);

  const containerRef = useRef<HTMLDivElement>(null);
  const dragStartX = useRef(0);
  const lastFrameIndex = useRef(0);

  // Preload all images
  useEffect(() => {
    if (view?.config.preloadAll) {
      view.frames.forEach((frame) => {
        const img = new Image();
        img.src = frame.imageUrl;
      });
    }
  }, [view]);

  // Auto-rotate
  useEffect(() => {
    if (!isAutoRotating || !view) return;

    const interval = setInterval(() => {
      setCurrentFrameIndex((prev) => (prev + 1) % view.frames.length);
    }, view.config.autoRotateSpeed / view.frames.length);

    return () => clearInterval(interval);
  }, [isAutoRotating, view]);

  // Notify on frame change
  useEffect(() => {
    if (view && onFrameChange) {
      onFrameChange(view.frames[currentFrameIndex]);
    }
  }, [currentFrameIndex, view, onFrameChange]);

  // Drag handlers
  const handleMouseDown = useCallback(
    (e: React.MouseEvent) => {
      if (!view?.config.allowDrag) return;

      setIsDragging(true);
      setIsAutoRotating(false);
      dragStartX.current = e.clientX;
      lastFrameIndex.current = currentFrameIndex;
    },
    [view, currentFrameIndex],
  );

  const handleMouseMove = useCallback(
    (e: React.MouseEvent) => {
      if (!isDragging || !view) return;

      const deltaX = e.clientX - dragStartX.current;
      const frameWidth = containerRef.current?.clientWidth || 400;
      const framesPerPixel = view.frames.length / frameWidth;
      const frameDelta = Math.round(deltaX * framesPerPixel);

      let newIndex = (lastFrameIndex.current + frameDelta) % view.frames.length;
      if (newIndex < 0) newIndex += view.frames.length;

      setCurrentFrameIndex(newIndex);
    },
    [isDragging, view],
  );

  const handleMouseUp = useCallback(() => {
    setIsDragging(false);
  }, []);

  // Touch handlers for mobile
  const handleTouchStart = useCallback(
    (e: React.TouchEvent) => {
      if (!view?.config.allowDrag) return;

      setIsDragging(true);
      setIsAutoRotating(false);
      dragStartX.current = e.touches[0].clientX;
      lastFrameIndex.current = currentFrameIndex;
    },
    [view, currentFrameIndex],
  );

  const handleTouchMove = useCallback(
    (e: React.TouchEvent) => {
      if (!isDragging || !view) return;

      const deltaX = e.touches[0].clientX - dragStartX.current;
      const frameWidth = containerRef.current?.clientWidth || 400;
      const framesPerPixel = view.frames.length / frameWidth;
      const frameDelta = Math.round(deltaX * framesPerPixel);

      let newIndex = (lastFrameIndex.current + frameDelta) % view.frames.length;
      if (newIndex < 0) newIndex += view.frames.length;

      setCurrentFrameIndex(newIndex);
    },
    [isDragging, view],
  );

  // Zoom handlers
  const handleZoomIn = () => {
    if (view?.config.allowZoom) {
      setZoomLevel((prev) => Math.min(prev + 0.25, 3));
    }
  };

  const handleZoomOut = () => {
    if (view?.config.allowZoom) {
      setZoomLevel((prev) => Math.max(prev - 0.25, 1));
    }
  };

  // Fullscreen
  const toggleFullscreen = () => {
    if (!containerRef.current) return;

    if (!isFullscreen) {
      containerRef.current.requestFullscreen?.();
      setIsFullscreen(true);
    } else {
      document.exitFullscreen?.();
      setIsFullscreen(false);
    }
  };

  // Loading state
  if (isLoading) {
    return (
      <div className={`vehicle-360-viewer loading ${className}`}>
        <div className="loading-spinner" />
        <p>Cargando vista 360°...</p>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className={`vehicle-360-viewer error ${className}`}>
        <p className="error-message">{error}</p>
      </div>
    );
  }

  // No view available
  if (!view || !view.isReady) {
    return (
      <div className={`vehicle-360-viewer not-available ${className}`}>
        <p>Vista 360° no disponible para este vehículo</p>
      </div>
    );
  }

  const currentFrame = view.frames[currentFrameIndex];

  return (
    <div
      ref={containerRef}
      className={`vehicle-360-viewer ${className} ${isFullscreen ? "fullscreen" : ""}`}
    >
      {/* Main image container */}
      <div
        className="viewer-main"
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseUp}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleMouseUp}
        style={{ cursor: isDragging ? "grabbing" : "grab" }}
      >
        <img
          src={currentFrame.imageUrl}
          alt={`Vista ${currentFrame.name}`}
          style={{ transform: `scale(${zoomLevel})` }}
          draggable={false}
        />

        {/* Angle indicator */}
        {view.config.showAngleIndicator && (
          <div className="angle-indicator">
            {currentFrame.angle}° - {currentFrame.name}
          </div>
        )}

        {/* Drag hint */}
        {!isDragging && (
          <div className="drag-hint">← Arrastra para rotar →</div>
        )}
      </div>

      {/* Controls */}
      {showControls && (
        <div className="viewer-controls">
          {/* Frame dots */}
          <div className="frame-dots">
            {view.frames.map((frame, index) => (
              <button
                key={index}
                className={`dot ${index === currentFrameIndex ? "active" : ""}`}
                onClick={() => {
                  setCurrentFrameIndex(index);
                  setIsAutoRotating(false);
                }}
                aria-label={`Ver ${frame.name}`}
              />
            ))}
          </div>

          {/* Control buttons */}
          <div className="control-buttons">
            <button
              className={`control-btn ${isAutoRotating ? "active" : ""}`}
              onClick={() => setIsAutoRotating(!isAutoRotating)}
              aria-label={isAutoRotating ? "Detener rotación" : "Auto rotar"}
            >
              {isAutoRotating ? "⏸" : "▶"}
            </button>

            {view.config.allowZoom && (
              <>
                <button
                  className="control-btn"
                  onClick={handleZoomOut}
                  disabled={zoomLevel <= 1}
                  aria-label="Alejar"
                >
                  🔍-
                </button>
                <button
                  className="control-btn"
                  onClick={handleZoomIn}
                  disabled={zoomLevel >= 3}
                  aria-label="Acercar"
                >
                  🔍+
                </button>
              </>
            )}

            <button
              className="control-btn"
              onClick={toggleFullscreen}
              aria-label={
                isFullscreen
                  ? "Salir de pantalla completa"
                  : "Pantalla completa"
              }
            >
              {isFullscreen ? "⛶" : "⛶"}
            </button>
          </div>
        </div>
      )}

      {/* Thumbnails */}
      {showThumbnails && (
        <div className="viewer-thumbnails">
          {view.frames.map((frame, index) => (
            <button
              key={index}
              className={`thumbnail ${index === currentFrameIndex ? "active" : ""}`}
              onClick={() => {
                setCurrentFrameIndex(index);
                setIsAutoRotating(false);
              }}
            >
              <img src={frame.thumbnailUrl} alt={frame.name} loading="lazy" />
              <span className="thumbnail-label">{frame.angle}°</span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
};
```

### 4. CSS Styles

```css
/* src/components/Vehicle360Viewer.css */

.vehicle-360-viewer {
  display: flex;
  flex-direction: column;
  background: #000;
  border-radius: 12px;
  overflow: hidden;
  user-select: none;
}

.vehicle-360-viewer.fullscreen {
  border-radius: 0;
}

/* Main viewer */
.viewer-main {
  position: relative;
  aspect-ratio: 16/9;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  background: linear-gradient(180deg, #1a1a2e 0%, #16213e 100%);
}

.viewer-main img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  transition: transform 0.2s ease;
}

.angle-indicator {
  position: absolute;
  top: 16px;
  left: 16px;
  background: rgba(0, 0, 0, 0.7);
  color: white;
  padding: 8px 16px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 500;
}

.drag-hint {
  position: absolute;
  bottom: 16px;
  left: 50%;
  transform: translateX(-50%);
  background: rgba(0, 0, 0, 0.5);
  color: rgba(255, 255, 255, 0.8);
  padding: 8px 24px;
  border-radius: 20px;
  font-size: 12px;
  pointer-events: none;
  opacity: 0;
  transition: opacity 0.3s;
}

.viewer-main:hover .drag-hint {
  opacity: 1;
}

/* Controls */
.viewer-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background: #1a1a2e;
}

.frame-dots {
  display: flex;
  gap: 8px;
}

.dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.3);
  border: none;
  cursor: pointer;
  transition: all 0.2s;
}

.dot.active {
  background: #3b82f6;
  transform: scale(1.2);
}

.dot:hover:not(.active) {
  background: rgba(255, 255, 255, 0.5);
}

.control-buttons {
  display: flex;
  gap: 8px;
}

.control-btn {
  width: 40px;
  height: 40px;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.1);
  border: none;
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.control-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.control-btn.active {
  background: #3b82f6;
}

.control-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

/* Thumbnails */
.viewer-thumbnails {
  display: flex;
  gap: 8px;
  padding: 12px;
  background: #16213e;
  overflow-x: auto;
  scrollbar-width: thin;
}

.thumbnail {
  flex-shrink: 0;
  width: 80px;
  border: none;
  background: none;
  cursor: pointer;
  padding: 0;
  position: relative;
  border-radius: 8px;
  overflow: hidden;
  opacity: 0.6;
  transition: all 0.2s;
}

.thumbnail.active {
  opacity: 1;
  outline: 2px solid #3b82f6;
  outline-offset: 2px;
}

.thumbnail:hover:not(.active) {
  opacity: 0.8;
}

.thumbnail img {
  width: 100%;
  height: auto;
  display: block;
}

.thumbnail-label {
  position: absolute;
  bottom: 4px;
  left: 50%;
  transform: translateX(-50%);
  background: rgba(0, 0, 0, 0.7);
  color: white;
  font-size: 10px;
  padding: 2px 6px;
  border-radius: 4px;
}

/* Loading state */
.vehicle-360-viewer.loading {
  aspect-ratio: 16/9;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #1a1a2e;
  color: white;
}

.loading-spinner {
  width: 48px;
  height: 48px;
  border: 4px solid rgba(255, 255, 255, 0.1);
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* Error state */
.vehicle-360-viewer.error,
.vehicle-360-viewer.not-available {
  aspect-ratio: 16/9;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #1a1a2e;
  color: rgba(255, 255, 255, 0.6);
}

/* Responsive */
@media (max-width: 640px) {
  .viewer-thumbnails {
    padding: 8px;
    gap: 6px;
  }

  .thumbnail {
    width: 60px;
  }

  .control-btn {
    width: 36px;
    height: 36px;
  }

  .angle-indicator {
    font-size: 12px;
    padding: 6px 12px;
  }
}
```

### 5. Uso en Página de Detalle

```tsx
// src/pages/VehicleDetailPage.tsx

import React from "react";
import { useParams } from "react-router-dom";
import { Vehicle360Viewer } from "../components/Vehicle360Viewer";
import { useVehicle360 } from "../hooks/useVehicle360";

export const VehicleDetailPage: React.FC = () => {
  const { vehicleId } = useParams<{ vehicleId: string }>();
  const { view, job, isProcessing, uploadAndProcess } = useVehicle360(
    vehicleId || "",
  );

  const handleUpload360Video = async (
    e: React.ChangeEvent<HTMLInputElement>,
  ) => {
    const file = e.target.files?.[0];
    if (file) {
      await uploadAndProcess(file);
    }
  };

  return (
    <div className="vehicle-detail-page">
      <h1>Detalles del Vehículo</h1>

      {/* Sección de Vista 360° */}
      <section className="vehicle-360-section">
        <h2>Vista 360°</h2>

        {view?.isReady ? (
          // Mostrar visor 360°
          <Vehicle360Viewer
            vehicleId={vehicleId!}
            showThumbnails={true}
            showControls={true}
          />
        ) : isProcessing ? (
          // Mostrar progreso de procesamiento
          <div className="processing-status">
            <h3>Procesando video 360°...</h3>
            <div className="progress-bar">
              <div
                className="progress-fill"
                style={{ width: `${job?.progress || 0}%` }}
              />
            </div>
            <p>
              {job?.progress}% - {job?.currentStep}
            </p>

            <ul className="steps-list">
              {job?.steps.map((step, index) => (
                <li key={index} className={`step ${step.status.toLowerCase()}`}>
                  {step.status === "Completed" && "✓"}
                  {step.status === "Processing" && "⏳"}
                  {step.status === "Pending" && "○"}
                  {step.status === "Failed" && "✗"} {step.step}
                </li>
              ))}
            </ul>
          </div>
        ) : (
          // Mostrar opción de subir video
          <div className="upload-360">
            <p>Agrega una vista 360° de tu vehículo</p>
            <label className="upload-btn">
              📹 Subir video 360°
              <input
                type="file"
                accept="video/mp4,video/mov,video/avi,video/webm"
                onChange={handleUpload360Video}
                hidden
              />
            </label>
            <p className="upload-hint">
              Graba un video girando alrededor del vehículo (15-60 segundos)
            </p>
          </div>
        )}
      </section>

      {/* Resto del contenido... */}
    </div>
  );
};
```

## 📱 Frontend Mobile (Flutter)

### 1. API Service

```dart
// lib/services/vehicle360_service.dart

import 'dart:io';
import 'package:dio/dio.dart';

class Vehicle360Frame {
  final int index;
  final int angle;
  final String name;
  final String imageUrl;
  final String thumbnailUrl;
  final int width;
  final int height;

  Vehicle360Frame({
    required this.index,
    required this.angle,
    required this.name,
    required this.imageUrl,
    required this.thumbnailUrl,
    required this.width,
    required this.height,
  });

  factory Vehicle360Frame.fromJson(Map<String, dynamic> json) {
    return Vehicle360Frame(
      index: json['index'],
      angle: json['angle'],
      name: json['name'],
      imageUrl: json['imageUrl'],
      thumbnailUrl: json['thumbnailUrl'],
      width: json['width'],
      height: json['height'],
    );
  }
}

class Vehicle360View {
  final String vehicleId;
  final bool isReady;
  final List<Vehicle360Frame> frames;
  final Vehicle360ViewConfig config;

  Vehicle360View({
    required this.vehicleId,
    required this.isReady,
    required this.frames,
    required this.config,
  });

  factory Vehicle360View.fromJson(Map<String, dynamic> json) {
    return Vehicle360View(
      vehicleId: json['vehicleId'],
      isReady: json['isReady'],
      frames: (json['frames'] as List)
          .map((f) => Vehicle360Frame.fromJson(f))
          .toList(),
      config: Vehicle360ViewConfig.fromJson(json['config']),
    );
  }
}

class Vehicle360ViewConfig {
  final bool autoRotate;
  final int autoRotateSpeed;
  final bool allowDrag;
  final bool allowZoom;
  final bool showAngleIndicator;
  final bool preloadAll;

  Vehicle360ViewConfig({
    this.autoRotate = true,
    this.autoRotateSpeed = 5000,
    this.allowDrag = true,
    this.allowZoom = true,
    this.showAngleIndicator = true,
    this.preloadAll = true,
  });

  factory Vehicle360ViewConfig.fromJson(Map<String, dynamic> json) {
    return Vehicle360ViewConfig(
      autoRotate: json['autoRotate'] ?? true,
      autoRotateSpeed: json['autoRotateSpeed'] ?? 5000,
      allowDrag: json['allowDrag'] ?? true,
      allowZoom: json['allowZoom'] ?? true,
      showAngleIndicator: json['showAngleIndicator'] ?? true,
      preloadAll: json['preloadAll'] ?? true,
    );
  }
}

class Vehicle360Service {
  final Dio _dio;
  final String baseUrl;

  Vehicle360Service({required this.baseUrl})
      : _dio = Dio(BaseOptions(baseUrl: baseUrl));

  Future<Vehicle360View?> getView(String vehicleId) async {
    try {
      final response = await _dio.get('/api/vehicle360/views/$vehicleId');
      return Vehicle360View.fromJson(response.data);
    } on DioError catch (e) {
      if (e.response?.statusCode == 404) {
        return null;
      }
      rethrow;
    }
  }

  Future<ProcessingJob> uploadAndProcess(String vehicleId, File videoFile) async {
    final formData = FormData.fromMap({
      'video': await MultipartFile.fromFile(videoFile.path),
      'vehicleId': vehicleId,
      'removeBackground': 'true',
    });

    final response = await _dio.post(
      '/api/vehicle360/upload-and-process',
      data: formData,
    );
    return ProcessingJob.fromJson(response.data);
  }

  Future<ProcessingJob> getJobStatus(String jobId) async {
    final response = await _dio.get('/api/vehicle360/jobs/$jobId');
    return ProcessingJob.fromJson(response.data);
  }
}
```

### 2. Widget: Vehicle360Viewer

```dart
// lib/widgets/vehicle_360_viewer.dart

import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../services/vehicle360_service.dart';

class Vehicle360Viewer extends StatefulWidget {
  final Vehicle360View view;
  final bool showThumbnails;
  final bool showControls;
  final Function(Vehicle360Frame)? onFrameChange;

  const Vehicle360Viewer({
    Key? key,
    required this.view,
    this.showThumbnails = true,
    this.showControls = true,
    this.onFrameChange,
  }) : super(key: key);

  @override
  State<Vehicle360Viewer> createState() => _Vehicle360ViewerState();
}

class _Vehicle360ViewerState extends State<Vehicle360Viewer>
    with SingleTickerProviderStateMixin {
  int _currentFrameIndex = 0;
  bool _isAutoRotating = false;
  double _scale = 1.0;
  late AnimationController _autoRotateController;

  @override
  void initState() {
    super.initState();
    _autoRotateController = AnimationController(
      duration: Duration(milliseconds: widget.view.config.autoRotateSpeed),
      vsync: this,
    );

    // Preload images
    if (widget.view.config.preloadAll) {
      for (final frame in widget.view.frames) {
        precacheImage(
          CachedNetworkImageProvider(frame.imageUrl),
          context,
        );
      }
    }
  }

  @override
  void dispose() {
    _autoRotateController.dispose();
    super.dispose();
  }

  void _onPanUpdate(DragUpdateDetails details) {
    if (!widget.view.config.allowDrag) return;

    setState(() {
      _isAutoRotating = false;

      final sensitivity = widget.view.frames.length / MediaQuery.of(context).size.width;
      final delta = (details.delta.dx * sensitivity).round();

      _currentFrameIndex = (_currentFrameIndex - delta) % widget.view.frames.length;
      if (_currentFrameIndex < 0) {
        _currentFrameIndex += widget.view.frames.length;
      }
    });

    widget.onFrameChange?.call(widget.view.frames[_currentFrameIndex]);
  }

  void _toggleAutoRotate() {
    setState(() {
      _isAutoRotating = !_isAutoRotating;
    });

    if (_isAutoRotating) {
      _startAutoRotate();
    } else {
      _autoRotateController.stop();
    }
  }

  void _startAutoRotate() {
    _autoRotateController.repeat();
    _autoRotateController.addListener(() {
      if (_isAutoRotating) {
        final progress = _autoRotateController.value;
        final frameIndex = (progress * widget.view.frames.length).floor() % widget.view.frames.length;

        if (frameIndex != _currentFrameIndex) {
          setState(() {
            _currentFrameIndex = frameIndex;
          });
          widget.onFrameChange?.call(widget.view.frames[_currentFrameIndex]);
        }
      }
    });
  }

  void _selectFrame(int index) {
    setState(() {
      _currentFrameIndex = index;
      _isAutoRotating = false;
    });
    _autoRotateController.stop();
    widget.onFrameChange?.call(widget.view.frames[_currentFrameIndex]);
  }

  @override
  Widget build(BuildContext context) {
    final currentFrame = widget.view.frames[_currentFrameIndex];

    return Container(
      decoration: BoxDecoration(
        color: Colors.black,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        children: [
          // Main viewer
          Expanded(
            child: GestureDetector(
              onPanUpdate: _onPanUpdate,
              onScaleUpdate: widget.view.config.allowZoom
                  ? (details) {
                      setState(() {
                        _scale = (_scale * details.scale).clamp(1.0, 3.0);
                      });
                    }
                  : null,
              child: Stack(
                alignment: Alignment.center,
                children: [
                  // Main image
                  InteractiveViewer(
                    scaleEnabled: widget.view.config.allowZoom,
                    minScale: 1.0,
                    maxScale: 3.0,
                    child: CachedNetworkImage(
                      imageUrl: currentFrame.imageUrl,
                      fit: BoxFit.contain,
                      placeholder: (context, url) => const Center(
                        child: CircularProgressIndicator(),
                      ),
                      errorWidget: (context, url, error) => const Icon(
                        Icons.error,
                        color: Colors.red,
                      ),
                    ),
                  ),

                  // Angle indicator
                  if (widget.view.config.showAngleIndicator)
                    Positioned(
                      top: 16,
                      left: 16,
                      child: Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 16,
                          vertical: 8,
                        ),
                        decoration: BoxDecoration(
                          color: Colors.black54,
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Text(
                          '${currentFrame.angle}° - ${currentFrame.name}',
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 14,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                      ),
                    ),
                ],
              ),
            ),
          ),

          // Controls
          if (widget.showControls)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              color: const Color(0xFF1A1A2E),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  // Frame dots
                  Row(
                    children: List.generate(
                      widget.view.frames.length,
                      (index) => GestureDetector(
                        onTap: () => _selectFrame(index),
                        child: Container(
                          width: 12,
                          height: 12,
                          margin: const EdgeInsets.only(right: 8),
                          decoration: BoxDecoration(
                            shape: BoxShape.circle,
                            color: index == _currentFrameIndex
                                ? Colors.blue
                                : Colors.white30,
                          ),
                        ),
                      ),
                    ),
                  ),

                  // Control buttons
                  Row(
                    children: [
                      IconButton(
                        icon: Icon(
                          _isAutoRotating ? Icons.pause : Icons.play_arrow,
                          color: Colors.white,
                        ),
                        onPressed: _toggleAutoRotate,
                      ),
                      if (widget.view.config.allowZoom) ...[
                        IconButton(
                          icon: const Icon(Icons.zoom_out, color: Colors.white),
                          onPressed: () {
                            setState(() {
                              _scale = (_scale - 0.25).clamp(1.0, 3.0);
                            });
                          },
                        ),
                        IconButton(
                          icon: const Icon(Icons.zoom_in, color: Colors.white),
                          onPressed: () {
                            setState(() {
                              _scale = (_scale + 0.25).clamp(1.0, 3.0);
                            });
                          },
                        ),
                      ],
                    ],
                  ),
                ],
              ),
            ),

          // Thumbnails
          if (widget.showThumbnails)
            Container(
              height: 80,
              color: const Color(0xFF16213E),
              child: ListView.builder(
                scrollDirection: Axis.horizontal,
                padding: const EdgeInsets.all(8),
                itemCount: widget.view.frames.length,
                itemBuilder: (context, index) {
                  final frame = widget.view.frames[index];
                  final isSelected = index == _currentFrameIndex;

                  return GestureDetector(
                    onTap: () => _selectFrame(index),
                    child: Container(
                      width: 80,
                      margin: const EdgeInsets.only(right: 8),
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(8),
                        border: isSelected
                            ? Border.all(color: Colors.blue, width: 2)
                            : null,
                      ),
                      child: Stack(
                        alignment: Alignment.bottomCenter,
                        children: [
                          ClipRRect(
                            borderRadius: BorderRadius.circular(6),
                            child: CachedNetworkImage(
                              imageUrl: frame.thumbnailUrl,
                              fit: BoxFit.cover,
                              width: 80,
                              height: 64,
                            ),
                          ),
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 6,
                              vertical: 2,
                            ),
                            decoration: BoxDecoration(
                              color: Colors.black54,
                              borderRadius: BorderRadius.circular(4),
                            ),
                            child: Text(
                              '${frame.angle}°',
                              style: const TextStyle(
                                color: Colors.white,
                                fontSize: 10,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  );
                },
              ),
            ),
        ],
      ),
    );
  }
}
```

### 3. Uso en Pantalla de Detalle

```dart
// lib/screens/vehicle_detail_screen.dart

import 'package:flutter/material.dart';
import '../services/vehicle360_service.dart';
import '../widgets/vehicle_360_viewer.dart';

class VehicleDetailScreen extends StatefulWidget {
  final String vehicleId;

  const VehicleDetailScreen({Key? key, required this.vehicleId}) : super(key: key);

  @override
  State<VehicleDetailScreen> createState() => _VehicleDetailScreenState();
}

class _VehicleDetailScreenState extends State<VehicleDetailScreen> {
  final _vehicle360Service = Vehicle360Service(
    baseUrl: 'https://api.okla.com.do',
  );

  Vehicle360View? _view;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadView();
  }

  Future<void> _loadView() async {
    setState(() => _isLoading = true);

    try {
      final view = await _vehicle360Service.getView(widget.vehicleId);
      setState(() {
        _view = view;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      // Handle error
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Detalles del Vehículo')),
      body: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Vista 360°
            if (_isLoading)
              const AspectRatio(
                aspectRatio: 16 / 9,
                child: Center(child: CircularProgressIndicator()),
              )
            else if (_view != null && _view!.isReady)
              AspectRatio(
                aspectRatio: 16 / 9,
                child: Vehicle360Viewer(
                  view: _view!,
                  showThumbnails: true,
                  showControls: true,
                ),
              )
            else
              AspectRatio(
                aspectRatio: 16 / 9,
                child: Container(
                  color: Colors.grey[900],
                  child: const Center(
                    child: Text(
                      'Vista 360° no disponible',
                      style: TextStyle(color: Colors.white54),
                    ),
                  ),
                ),
              ),

            // Resto del contenido...
          ],
        ),
      ),
    );
  }
}
```

---

**Anterior:** [04-VEHICLE360PROCESSINGSERVICE.md](./04-VEHICLE360PROCESSINGSERVICE.md)  
**Siguiente:** [06-TABLA-PROVEEDORES-PRECIOS.md](./06-TABLA-PROVEEDORES-PRECIOS.md)

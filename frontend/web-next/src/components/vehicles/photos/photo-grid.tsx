'use client';

import { useCallback, useMemo, useState } from 'react';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
  type DragStartEvent,
  DragOverlay,
} from '@dnd-kit/core';
import {
  SortableContext,
  sortableKeyboardCoordinates,
  rectSortingStrategy,
  useSortable,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Plus } from 'lucide-react';
import { PhotoCard, type PhotoItem } from './photo-card';

// ============================================================
// TYPES
// ============================================================

interface PhotoGridProps {
  photos: PhotoItem[];
  onReorder: (photos: PhotoItem[]) => void;
  onSetPrimary: (id: string) => void;
  onRemove: (id: string) => void;
  onCrop: (id: string) => void;
  onView: (id: string) => void;
  onRetry?: (id: string) => void;
  onAddMore: () => void;
  maxPhotos: number;
}

// ============================================================
// SORTABLE WRAPPER
// ============================================================

function SortablePhotoCard({
  photo,
  onSetPrimary,
  onRemove,
  onCrop,
  onView,
  onRetry,
}: {
  photo: PhotoItem;
  onSetPrimary: (id: string) => void;
  onRemove: (id: string) => void;
  onCrop: (id: string) => void;
  onView: (id: string) => void;
  onRetry?: (id: string) => void;
}) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: photo.id,
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.3 : 1,
  };

  return (
    <div ref={setNodeRef} style={style} {...attributes}>
      <PhotoCard
        photo={photo}
        onSetPrimary={onSetPrimary}
        onRemove={onRemove}
        onCrop={onCrop}
        onView={onView}
        onRetry={onRetry}
        isDragging={isDragging}
        dragHandleProps={listeners}
      />
    </div>
  );
}

// ============================================================
// ADD MORE CARD
// ============================================================

function AddMoreCard({ remaining, onClick }: { remaining: number; onClick: () => void }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="flex aspect-[4/3] w-full flex-col items-center justify-center rounded-xl border-2 border-dashed border-gray-300 bg-gray-50 text-gray-400 transition-all hover:border-emerald-400 hover:bg-emerald-50 hover:text-emerald-600"
    >
      <Plus className="h-8 w-8" />
      <span className="mt-1 text-sm font-medium">Agregar más</span>
      <span className="text-xs">{remaining} restantes</span>
    </button>
  );
}

// ============================================================
// MAIN COMPONENT
// ============================================================

export function PhotoGrid({
  photos,
  onReorder,
  onSetPrimary,
  onRemove,
  onCrop,
  onView,
  onRetry,
  onAddMore,
  maxPhotos,
}: PhotoGridProps) {
  const [activeId, setActiveId] = useState<string | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 5 },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const remaining = maxPhotos - photos.length;

  const activePhoto = useMemo(
    () => photos.find(p => p.id === activeId) ?? null,
    [photos, activeId]
  );

  const handleDragStart = useCallback((event: DragStartEvent) => {
    setActiveId(String(event.active.id));
  }, []);

  const handleDragEnd = useCallback(
    (event: DragEndEvent) => {
      setActiveId(null);
      const { active, over } = event;
      if (!over || active.id === over.id) return;

      const oldIndex = photos.findIndex(p => p.id === active.id);
      const newIndex = photos.findIndex(p => p.id === over.id);

      if (oldIndex === -1 || newIndex === -1) return;

      const reordered = [...photos];
      const [moved] = reordered.splice(oldIndex, 1);
      reordered.splice(newIndex, 0, moved);

      // Update order indices
      const updated = reordered.map((p, i) => ({ ...p, order: i }));
      onReorder(updated);
    },
    [photos, onReorder]
  );

  const sortableIds = useMemo(() => photos.map(p => p.id), [photos]);

  if (photos.length === 0) return null;

  return (
    <DndContext
      sensors={sensors}
      collisionDetection={closestCenter}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
    >
      <SortableContext items={sortableIds} strategy={rectSortingStrategy}>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
          {photos.map(photo => (
            <SortablePhotoCard
              key={photo.id}
              photo={photo}
              onSetPrimary={onSetPrimary}
              onRemove={onRemove}
              onCrop={onCrop}
              onView={onView}
              onRetry={onRetry}
            />
          ))}

          {/* Add more card */}
          {remaining > 0 && <AddMoreCard remaining={remaining} onClick={onAddMore} />}
        </div>
      </SortableContext>

      {/* Drag overlay for visual feedback */}
      <DragOverlay dropAnimation={null}>
        {activePhoto ? (
          <PhotoCard
            photo={activePhoto}
            onSetPrimary={onSetPrimary}
            onRemove={onRemove}
            onCrop={onCrop}
            onView={onView}
            isDragging
          />
        ) : null}
      </DragOverlay>
    </DndContext>
  );
}

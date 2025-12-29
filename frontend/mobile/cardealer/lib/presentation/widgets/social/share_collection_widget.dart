import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/typography.dart';
import '../../../core/theme/spacing.dart';

/// Share Collections Widget
/// Sprint 8 - SF-004: Share Collections Feature
class ShareCollectionWidget extends StatelessWidget {
  final String collectionId;
  final String collectionName;
  final int vehicleCount;
  final VoidCallback? onShare;

  const ShareCollectionWidget({
    super.key,
    required this.collectionId,
    required this.collectionName,
    required this.vehicleCount,
    this.onShare,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        color: AppColors.backgroundSecondary,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(Icons.folder_special, color: AppColors.primary, size: 28),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      collectionName,
                      style: AppTypography.h4.copyWith(
                        color: AppColors.textPrimary,
                      ),
                    ),
                    Text(
                      '$vehicleCount vehículos',
                      style: AppTypography.bodySmall.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.lg),
          Row(
            children: [
              Expanded(
                child: OutlinedButton.icon(
                  onPressed: () => _showShareOptionsSheet(
                      context, collectionId, collectionName),
                  icon: const Icon(Icons.share),
                  label: const Text('Compartir'),
                  style: OutlinedButton.styleFrom(
                    padding:
                        const EdgeInsets.symmetric(vertical: AppSpacing.md),
                  ),
                ),
              ),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: ElevatedButton.icon(
                  onPressed: () => _copyLinkToClipboard(context, collectionId),
                  icon: const Icon(Icons.link),
                  label: const Text('Copiar Link'),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    foregroundColor: Colors.white,
                    padding:
                        const EdgeInsets.symmetric(vertical: AppSpacing.md),
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

// Helper functions for ShareCollectionWidget
void _showShareOptionsSheet(
    BuildContext context, String collectionId, String collectionName) {
  showModalBottomSheet(
    context: context,
    shape: const RoundedRectangleBorder(
      borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
    ),
    builder: (context) => ShareCollectionSheet(
      collectionId: collectionId,
      collectionName: collectionName,
    ),
  );
}

void _copyLinkToClipboard(BuildContext context, String collectionId) {
  final link = 'https://cardealer.com/collections/$collectionId';
  Clipboard.setData(ClipboardData(text: link));
  ScaffoldMessenger.of(context).showSnackBar(
    const SnackBar(
      content: Row(
        children: [
          Icon(Icons.check_circle, color: Colors.white),
          SizedBox(width: AppSpacing.sm),
          Text('Link copiado al portapapeles'),
        ],
      ),
    ),
  );
}

class ShareCollectionSheet extends StatefulWidget {
  final String collectionId;
  final String collectionName;

  const ShareCollectionSheet({
    super.key,
    required this.collectionId,
    required this.collectionName,
  });

  @override
  State<ShareCollectionSheet> createState() => _ShareCollectionSheetState();
}

class _ShareCollectionSheetState extends State<ShareCollectionSheet> {
  bool _isPublic = true;
  bool _allowComments = true;
  bool _trackViews = true;

  @override
  Widget build(BuildContext context) {
    final shareLink =
        'https://cardealer.com/collections/${widget.collectionId}';

    return Container(
      padding: const EdgeInsets.all(AppSpacing.xl),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(AppSpacing.md),
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    colors: [
                      AppColors.primary.withValues(alpha: 0.2),
                      AppColors.accent.withValues(alpha: 0.2),
                    ],
                  ),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: const Icon(Icons.share, color: AppColors.primary, size: 28),
              ),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Compartir Colección',
                      style: AppTypography.h3,
                    ),
                    Text(
                      widget.collectionName,
                      style: AppTypography.bodySmall.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ),
              ),
              IconButton(
                icon: const Icon(Icons.close),
                onPressed: () => Navigator.pop(context),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.xl),

          // Share Link
          Container(
            padding: const EdgeInsets.all(AppSpacing.md),
            decoration: BoxDecoration(
              color: AppColors.background,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: Colors.grey.shade300),
            ),
            child: Row(
              children: [
                Expanded(
                  child: Text(
                    shareLink,
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.primary,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                IconButton(
                  icon: const Icon(Icons.copy, size: 20),
                  onPressed: () {
                    Clipboard.setData(ClipboardData(text: shareLink));
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text('Link copiado')),
                    );
                  },
                ),
              ],
            ),
          ),
          const SizedBox(height: AppSpacing.xl),

          // Settings
          Text(
            'Configuración de Compartir',
            style: AppTypography.bodyLarge.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: AppSpacing.md),

          SwitchListTile(
            value: _isPublic,
            onChanged: (value) => setState(() => _isPublic = value),
            title: const Text('Público'),
            subtitle: const Text('Cualquiera con el link puede ver'),
            activeThumbColor: AppColors.primary,
          ),
          SwitchListTile(
            value: _allowComments,
            onChanged: (value) => setState(() => _allowComments = value),
            title: const Text('Permitir Comentarios'),
            subtitle: const Text('Los visitantes pueden comentar'),
            activeThumbColor: AppColors.primary,
          ),
          SwitchListTile(
            value: _trackViews,
            onChanged: (value) => setState(() => _trackViews = value),
            title: const Text('Rastrear Vistas'),
            subtitle: const Text('Ver estadísticas de visitantes'),
            activeThumbColor: AppColors.primary,
          ),

          const SizedBox(height: AppSpacing.xl),

          // Share Buttons
          Text(
            'Compartir en',
            style: AppTypography.bodyLarge.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: AppSpacing.md),

          Row(
            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
            children: [
              _buildShareButton(
                'WhatsApp',
                Icons.message,
                Colors.green,
                () => _shareToWhatsApp(context),
              ),
              _buildShareButton(
                'Facebook',
                Icons.facebook,
                Colors.blue,
                () => _shareToFacebook(context),
              ),
              _buildShareButton(
                'Twitter',
                Icons.share,
                Colors.lightBlue,
                () => _shareToTwitter(context),
              ),
              _buildShareButton(
                'Email',
                Icons.email,
                Colors.orange,
                () => _shareToEmail(context),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildShareButton(
    String label,
    IconData icon,
    Color color,
    VoidCallback onTap,
  ) {
    return GestureDetector(
      onTap: onTap,
      child: Column(
        children: [
          Container(
            padding: const EdgeInsets.all(AppSpacing.md),
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(icon, color: color, size: 28),
          ),
          const SizedBox(height: AppSpacing.xs),
          Text(
            label,
            style: AppTypography.caption,
          ),
        ],
      ),
    );
  }

  void _shareToWhatsApp(BuildContext context) {
    Navigator.pop(context);
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Compartiendo en WhatsApp...')),
    );
  }

  void _shareToFacebook(BuildContext context) {
    Navigator.pop(context);
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Compartiendo en Facebook...')),
    );
  }

  void _shareToTwitter(BuildContext context) {
    Navigator.pop(context);
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Compartiendo en Twitter...')),
    );
  }

  void _shareToEmail(BuildContext context) {
    Navigator.pop(context);
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Abriendo cliente de email...')),
    );
  }
}

/// Vehicle Notes Widget
/// Sprint 8 - SF-005: Vehicle Notes System
class VehicleNotesWidget extends StatefulWidget {
  final String vehicleId;
  final List<VehicleNote> initialNotes;

  const VehicleNotesWidget({
    super.key,
    required this.vehicleId,
    this.initialNotes = const [],
  });

  @override
  State<VehicleNotesWidget> createState() => _VehicleNotesWidgetState();
}

class _VehicleNotesWidgetState extends State<VehicleNotesWidget> {
  late List<VehicleNote> _notes;
  final _noteController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _notes = List.from(widget.initialNotes);
  }

  @override
  void dispose() {
    _noteController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.backgroundSecondary,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Row(
              children: [
                const Icon(Icons.note_alt_outlined, color: AppColors.primary),
                const SizedBox(width: AppSpacing.sm),
                Text(
                  'Mis Notas',
                  style: AppTypography.h4.copyWith(
                    color: AppColors.textPrimary,
                  ),
                ),
                const Spacer(),
                TextButton.icon(
                  onPressed: _showAddNoteDialog,
                  icon: const Icon(Icons.add, size: 18),
                  label: const Text('Agregar'),
                ),
              ],
            ),
          ),
          if (_notes.isEmpty)
            Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: Center(
                child: Column(
                  children: [
                    Icon(
                      Icons.note_add_outlined,
                      size: 48,
                      color: AppColors.textSecondary.withValues(alpha: 0.5),
                    ),
                    const SizedBox(height: AppSpacing.sm),
                    Text(
                      'Sin notas aún',
                      style: AppTypography.bodyMedium.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Text(
                      'Agrega notas personales sobre este vehículo',
                      style: AppTypography.bodySmall.copyWith(
                        color: AppColors.textSecondary,
                      ),
                      textAlign: TextAlign.center,
                    ),
                  ],
                ),
              ),
            )
          else
            ListView.separated(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              padding: const EdgeInsets.all(AppSpacing.md),
              itemCount: _notes.length,
              separatorBuilder: (context, index) => const Divider(height: 1),
              itemBuilder: (context, index) => _buildNoteItem(_notes[index]),
            ),
        ],
      ),
    );
  }

  Widget _buildNoteItem(VehicleNote note) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: AppSpacing.sm),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: 8,
            height: 8,
            margin: const EdgeInsets.only(top: 6),
            decoration: BoxDecoration(
              color: _getCategoryColor(note.category),
              shape: BoxShape.circle,
            ),
          ),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  note.content,
                  style: AppTypography.bodyMedium,
                ),
                const SizedBox(height: AppSpacing.xs),
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: AppSpacing.sm,
                        vertical: 2,
                      ),
                      decoration: BoxDecoration(
                        color: _getCategoryColor(note.category)
                            .withValues(alpha: 0.2),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Text(
                        note.category,
                        style: AppTypography.caption.copyWith(
                          color: _getCategoryColor(note.category),
                        ),
                      ),
                    ),
                    const SizedBox(width: AppSpacing.sm),
                    Text(
                      _formatDate(note.createdAt),
                      style: AppTypography.caption.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          PopupMenuButton<String>(
            onSelected: (value) => _handleNoteAction(value, note),
            itemBuilder: (context) => [
              const PopupMenuItem(value: 'edit', child: Text('Editar')),
              const PopupMenuItem(value: 'delete', child: Text('Eliminar')),
            ],
          ),
        ],
      ),
    );
  }

  Color _getCategoryColor(String category) {
    switch (category.toLowerCase()) {
      case 'importante':
        return Colors.red;
      case 'recordatorio':
        return Colors.orange;
      case 'pros':
        return Colors.green;
      case 'contras':
        return Colors.red;
      default:
        return AppColors.primary;
    }
  }

  String _formatDate(DateTime date) {
    final now = DateTime.now();
    final diff = now.difference(date);

    if (diff.inDays == 0) {
      return 'Hoy ${date.hour}:${date.minute.toString().padLeft(2, '0')}';
    } else if (diff.inDays == 1) {
      return 'Ayer';
    } else if (diff.inDays < 7) {
      return '${diff.inDays} días';
    } else {
      return '${date.day}/${date.month}/${date.year}';
    }
  }

  void _showAddNoteDialog() {
    String selectedCategory = 'General';
    _noteController.clear();

    showDialog(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: const Text('Agregar Nota'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              TextField(
                controller: _noteController,
                maxLines: 3,
                decoration: const InputDecoration(
                  hintText: 'Escribe tu nota aquí...',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: AppSpacing.lg),
              Text(
                'Categoría',
                style: AppTypography.bodyMedium.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: AppSpacing.sm),
              Wrap(
                spacing: AppSpacing.sm,
                children: [
                  'General',
                  'Importante',
                  'Recordatorio',
                  'Pros',
                  'Contras'
                ].map((category) {
                  return ChoiceChip(
                    label: Text(category),
                    selected: selectedCategory == category,
                    onSelected: (selected) {
                      setDialogState(() => selectedCategory = category);
                    },
                  );
                }).toList(),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Cancelar'),
            ),
            ElevatedButton(
              onPressed: () {
                if (_noteController.text.isNotEmpty) {
                  setState(() {
                    _notes.add(VehicleNote(
                      id: DateTime.now().millisecondsSinceEpoch.toString(),
                      content: _noteController.text,
                      category: selectedCategory,
                      createdAt: DateTime.now(),
                    ));
                  });
                  Navigator.pop(context);
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Nota agregada')),
                  );
                }
              },
              child: const Text('Agregar'),
            ),
          ],
        ),
      ),
    );
  }

  void _handleNoteAction(String action, VehicleNote note) {
    switch (action) {
      case 'edit':
        _editNote(note);
        break;
      case 'delete':
        _deleteNote(note);
        break;
    }
  }

  void _editNote(VehicleNote note) {
    _noteController.text = note.content;
    // Show edit dialog (similar to add)
  }

  void _deleteNote(VehicleNote note) {
    setState(() {
      _notes.removeWhere((n) => n.id == note.id);
    });
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Nota eliminada')),
    );
  }
}

class VehicleNote {
  final String id;
  final String content;
  final String category;
  final DateTime createdAt;

  VehicleNote({
    required this.id,
    required this.content,
    required this.category,
    required this.createdAt,
  });
}

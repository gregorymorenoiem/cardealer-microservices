import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

/// PE-008: Activity History (Sprint 11)
/// Historial completo de actividad del usuario con filtros y exportación
class ActivityHistoryPage extends StatefulWidget {
  const ActivityHistoryPage({super.key});

  @override
  State<ActivityHistoryPage> createState() => _ActivityHistoryPageState();
}

class _ActivityHistoryPageState extends State<ActivityHistoryPage> {
  // Time range filter
  String _selectedTimeRange = '7days';

  // Activity type filters
  final Set<String> _selectedTypes = {
    'viewed',
    'favorited',
    'searched',
    'messaged',
    'published'
  };

  // Mock activity data
  final List<Map<String, dynamic>> _activities = [
    {
      'type': 'viewed',
      'title': 'Viste Toyota Camry 2023',
      'description': 'Precio: \$28,500 • Miami, FL',
      'timestamp': DateTime.now().subtract(const Duration(hours: 2)),
    },
    {
      'type': 'favorited',
      'title': 'Añadiste a favoritos Honda Accord',
      'description': 'Precio: \$26,900 • Orlando, FL',
      'timestamp': DateTime.now().subtract(const Duration(hours: 5)),
    },
    {
      'type': 'searched',
      'title': 'Buscaste "SUV bajo 50k"',
      'description': '127 resultados encontrados',
      'timestamp': DateTime.now().subtract(const Duration(days: 1)),
    },
    {
      'type': 'messaged',
      'title': 'Enviaste mensaje a Premium Motors',
      'description': 'Consulta sobre BMW X5 2022',
      'timestamp': DateTime.now().subtract(const Duration(days: 1, hours: 3)),
    },
    {
      'type': 'published',
      'title': 'Publicaste Ford F-150 2021',
      'description': 'Precio: \$42,000',
      'timestamp': DateTime.now().subtract(const Duration(days: 2)),
    },
    {
      'type': 'viewed',
      'title': 'Viste Chevrolet Silverado 2023',
      'description': 'Precio: \$45,000 • Tampa, FL',
      'timestamp': DateTime.now().subtract(const Duration(days: 3)),
    },
    {
      'type': 'searched',
      'title': 'Buscaste "pickup 4x4"',
      'description': '89 resultados encontrados',
      'timestamp': DateTime.now().subtract(const Duration(days: 5)),
    },
    {
      'type': 'favorited',
      'title': 'Añadiste a favoritos Jeep Wrangler',
      'description': 'Precio: \$38,500 • Fort Lauderdale, FL',
      'timestamp': DateTime.now().subtract(const Duration(days: 6)),
    },
    {
      'type': 'messaged',
      'title': 'Enviaste mensaje a AutoMax',
      'description': 'Consulta sobre Mercedes-Benz C-Class',
      'timestamp': DateTime.now().subtract(const Duration(days: 8)),
    },
    {
      'type': 'viewed',
      'title': 'Viste Tesla Model 3 2023',
      'description': 'Precio: \$42,000 • Miami, FL',
      'timestamp': DateTime.now().subtract(const Duration(days: 10)),
    },
  ];

  List<Map<String, dynamic>> get _filteredActivities {
    final now = DateTime.now();
    DateTime cutoffDate;

    switch (_selectedTimeRange) {
      case '7days':
        cutoffDate = now.subtract(const Duration(days: 7));
        break;
      case '30days':
        cutoffDate = now.subtract(const Duration(days: 30));
        break;
      case 'all':
      default:
        cutoffDate = DateTime(2000);
    }

    return _activities.where((activity) {
      final isInTimeRange = activity['timestamp'].isAfter(cutoffDate);
      final isSelectedType = _selectedTypes.contains(activity['type']);
      return isInTimeRange && isSelectedType;
    }).toList();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Historial de Actividad'),
        actions: [
          IconButton(
            icon: const Icon(Icons.file_download),
            onPressed: _showExportDialog,
            tooltip: 'Exportar',
          ),
          PopupMenuButton<String>(
            icon: const Icon(Icons.more_vert),
            onSelected: (value) {
              if (value == 'clear') {
                _showClearHistoryDialog();
              } else if (value == 'privacy') {
                _showPrivacyDialog();
              }
            },
            itemBuilder: (context) => [
              const PopupMenuItem(
                value: 'clear',
                child: ListTile(
                  leading: Icon(Icons.delete_sweep),
                  title: Text('Borrar historial'),
                  contentPadding: EdgeInsets.zero,
                ),
              ),
              const PopupMenuItem(
                value: 'privacy',
                child: ListTile(
                  leading: Icon(Icons.security),
                  title: Text('Privacidad'),
                  contentPadding: EdgeInsets.zero,
                ),
              ),
            ],
          ),
        ],
      ),
      body: Column(
        children: [
          // Time range chips
          SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            child: Row(
              children: [
                FilterChip(
                  label: const Text('Últimos 7 días'),
                  selected: _selectedTimeRange == '7days',
                  onSelected: (selected) {
                    setState(() {
                      _selectedTimeRange = '7days';
                    });
                  },
                ),
                const SizedBox(width: 8),
                FilterChip(
                  label: const Text('Últimos 30 días'),
                  selected: _selectedTimeRange == '30days',
                  onSelected: (selected) {
                    setState(() {
                      _selectedTimeRange = '30days';
                    });
                  },
                ),
                const SizedBox(width: 8),
                FilterChip(
                  label: const Text('Todo el historial'),
                  selected: _selectedTimeRange == 'all',
                  onSelected: (selected) {
                    setState(() {
                      _selectedTimeRange = 'all';
                    });
                  },
                ),
              ],
            ),
          ),

          // Activity type filters
          SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Row(
              children: [
                FilterChip(
                  avatar: const Icon(Icons.visibility, size: 18),
                  label: const Text('Vistos'),
                  selected: _selectedTypes.contains('viewed'),
                  onSelected: (selected) {
                    setState(() {
                      if (selected) {
                        _selectedTypes.add('viewed');
                      } else {
                        _selectedTypes.remove('viewed');
                      }
                    });
                  },
                ),
                const SizedBox(width: 8),
                FilterChip(
                  avatar: const Icon(Icons.favorite, size: 18),
                  label: const Text('Favoritos'),
                  selected: _selectedTypes.contains('favorited'),
                  onSelected: (selected) {
                    setState(() {
                      if (selected) {
                        _selectedTypes.add('favorited');
                      } else {
                        _selectedTypes.remove('favorited');
                      }
                    });
                  },
                ),
                const SizedBox(width: 8),
                FilterChip(
                  avatar: const Icon(Icons.search, size: 18),
                  label: const Text('Búsquedas'),
                  selected: _selectedTypes.contains('searched'),
                  onSelected: (selected) {
                    setState(() {
                      if (selected) {
                        _selectedTypes.add('searched');
                      } else {
                        _selectedTypes.remove('searched');
                      }
                    });
                  },
                ),
                const SizedBox(width: 8),
                FilterChip(
                  avatar: const Icon(Icons.message, size: 18),
                  label: const Text('Mensajes'),
                  selected: _selectedTypes.contains('messaged'),
                  onSelected: (selected) {
                    setState(() {
                      if (selected) {
                        _selectedTypes.add('messaged');
                      } else {
                        _selectedTypes.remove('messaged');
                      }
                    });
                  },
                ),
                const SizedBox(width: 8),
                FilterChip(
                  avatar: const Icon(Icons.add_circle, size: 18),
                  label: const Text('Publicados'),
                  selected: _selectedTypes.contains('published'),
                  onSelected: (selected) {
                    setState(() {
                      if (selected) {
                        _selectedTypes.add('published');
                      } else {
                        _selectedTypes.remove('published');
                      }
                    });
                  },
                ),
              ],
            ),
          ),
          const Divider(height: 1),

          // Activity list
          Expanded(
            child: _filteredActivities.isEmpty
                ? Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          Icons.history,
                          size: 64,
                          color: Colors.grey.shade300,
                        ),
                        const SizedBox(height: 16),
                        Text(
                          'No hay actividad',
                          style: theme.textTheme.titleLarge?.copyWith(
                            color: Colors.grey.shade600,
                          ),
                        ),
                        const SizedBox(height: 8),
                        Text(
                          'Ajusta los filtros para ver más actividad',
                          style: TextStyle(color: Colors.grey.shade500),
                        ),
                      ],
                    ),
                  )
                : ListView.builder(
                    itemCount: _filteredActivities.length,
                    itemBuilder: (context, index) {
                      final activity = _filteredActivities[index];

                      // Check if we need a date header
                      final showDateHeader = index == 0 ||
                          !_isSameDay(
                            activity['timestamp'],
                            _filteredActivities[index - 1]['timestamp'],
                          );

                      return Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          if (showDateHeader)
                            _DateHeader(date: activity['timestamp']),
                          _ActivityTile(activity: activity),
                        ],
                      );
                    },
                  ),
          ),
        ],
      ),
    );
  }

  bool _isSameDay(DateTime date1, DateTime date2) {
    return date1.year == date2.year &&
        date1.month == date2.month &&
        date1.day == date2.day;
  }

  void _showExportDialog() {
    showModalBottomSheet(
      context: context,
      builder: (context) => _ExportDialog(
        selectedTimeRange: _selectedTimeRange,
        selectedTypes: _selectedTypes,
      ),
    );
  }

  void _showClearHistoryDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Borrar Historial'),
        content: const Text(
          '¿Estás seguro de que deseas borrar todo tu historial de actividad? Esta acción no se puede deshacer.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () {
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Historial borrado')),
              );
            },
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Borrar'),
          ),
        ],
      ),
    );
  }

  void _showPrivacyDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Configuración de Privacidad'),
        content: const Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text('Controla quién puede ver tu actividad'),
            SizedBox(height: 16),
            ListTile(
              leading: Icon(Icons.public),
              title: Text('Público'),
              subtitle: Text('Todos pueden ver'),
              contentPadding: EdgeInsets.zero,
            ),
            ListTile(
              leading: Icon(Icons.people),
              title: Text('Contactos'),
              subtitle: Text('Solo tus contactos'),
              contentPadding: EdgeInsets.zero,
            ),
            ListTile(
              leading: Icon(Icons.lock),
              title: Text('Privado'),
              subtitle: Text('Solo tú'),
              contentPadding: EdgeInsets.zero,
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cerrar'),
          ),
        ],
      ),
    );
  }
}

/// Date header widget
class _DateHeader extends StatelessWidget {
  final DateTime date;

  const _DateHeader({required this.date});

  @override
  Widget build(BuildContext context) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    final yesterday = today.subtract(const Duration(days: 1));
    final dateOnly = DateTime(date.year, date.month, date.day);

    String label;
    if (dateOnly == today) {
      label = 'Hoy';
    } else if (dateOnly == yesterday) {
      label = 'Ayer';
    } else {
      label = DateFormat('EEEE, d MMMM', 'es').format(date);
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      color: Theme.of(context).colorScheme.surfaceContainerHighest,
      child: Text(
        label,
        style: const TextStyle(
          fontSize: 13,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }
}

/// Activity tile widget
class _ActivityTile extends StatelessWidget {
  final Map<String, dynamic> activity;

  const _ActivityTile({required this.activity});

  @override
  Widget build(BuildContext context) {
    final icon = _getIcon(activity['type']);
    final color = _getColor(activity['type']);
    final timeString = DateFormat('HH:mm').format(activity['timestamp']);

    return ListTile(
      leading: Container(
        padding: const EdgeInsets.all(8),
        decoration: BoxDecoration(
          color: color.withAlpha(50),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Icon(icon, color: color, size: 20),
      ),
      title: Text(activity['title']),
      subtitle: Text(activity['description']),
      trailing: Text(
        timeString,
        style: TextStyle(
          fontSize: 12,
          color: Colors.grey.shade600,
        ),
      ),
    );
  }

  IconData _getIcon(String type) {
    switch (type) {
      case 'viewed':
        return Icons.visibility;
      case 'favorited':
        return Icons.favorite;
      case 'searched':
        return Icons.search;
      case 'messaged':
        return Icons.message;
      case 'published':
        return Icons.add_circle;
      default:
        return Icons.circle;
    }
  }

  Color _getColor(String type) {
    switch (type) {
      case 'viewed':
        return Colors.blue;
      case 'favorited':
        return Colors.red;
      case 'searched':
        return Colors.orange;
      case 'messaged':
        return Colors.green;
      case 'published':
        return Colors.purple;
      default:
        return Colors.grey;
    }
  }
}

/// Export dialog
class _ExportDialog extends StatefulWidget {
  final String selectedTimeRange;
  final Set<String> selectedTypes;

  const _ExportDialog({
    required this.selectedTimeRange,
    required this.selectedTypes,
  });

  @override
  State<_ExportDialog> createState() => _ExportDialogState();
}

class _ExportDialogState extends State<_ExportDialog> {
  String _exportFormat = 'csv';
  bool _isExporting = false;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Exportar Historial',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 24),

          // Format selection
          const Text('Formato de exportación:'),
          const SizedBox(height: 8),
          SegmentedButton<String>(
            segments: const [
              ButtonSegment(value: 'csv', label: Text('CSV')),
              ButtonSegment(value: 'pdf', label: Text('PDF')),
              ButtonSegment(value: 'json', label: Text('JSON')),
            ],
            selected: {_exportFormat},
            onSelectionChanged: (Set<String> newSelection) {
              setState(() {
                _exportFormat = newSelection.first;
              });
            },
          ),
          const SizedBox(height: 24),

          // Summary
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Theme.of(context).colorScheme.surfaceContainerHighest,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('Se exportará:',
                    style: TextStyle(fontWeight: FontWeight.bold)),
                const SizedBox(height: 8),
                Text(
                    '• Rango: ${_getTimeRangeLabel(widget.selectedTimeRange)}'),
                Text('• Tipos: ${widget.selectedTypes.length} categorías'),
              ],
            ),
          ),
          const SizedBox(height: 24),

          // Export button
          SizedBox(
            width: double.infinity,
            child: FilledButton.icon(
              onPressed: _isExporting ? null : _exportData,
              icon: const Icon(Icons.download),
              label: _isExporting
                  ? const Text('Exportando...')
                  : const Text('Exportar'),
            ),
          ),
        ],
      ),
    );
  }

  String _getTimeRangeLabel(String range) {
    switch (range) {
      case '7days':
        return 'Últimos 7 días';
      case '30days':
        return 'Últimos 30 días';
      case 'all':
        return 'Todo el historial';
      default:
        return range;
    }
  }

  Future<void> _exportData() async {
    setState(() {
      _isExporting = true;
    });

    // Simulate export
    await Future.delayed(const Duration(seconds: 2));

    if (mounted) {
      Navigator.pop(context);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Historial exportado en formato $_exportFormat'),
          backgroundColor: Colors.green,
        ),
      );
    }
  }
}

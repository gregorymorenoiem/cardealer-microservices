import 'package:flutter/material.dart';

/// CM-010: Conversation Search
/// Búsqueda dentro de conversaciones con filtros
class ConversationSearchPage extends StatefulWidget {
  const ConversationSearchPage({super.key});

  @override
  State<ConversationSearchPage> createState() => _ConversationSearchPageState();
}

class _ConversationSearchPageState extends State<ConversationSearchPage> {
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';
  String _filterType = 'all';
  DateTime? _fromDate;
  DateTime? _toDate;

  final List<Map<String, dynamic>> _mockResults = [
    {
      'id': '1',
      'text': 'El precio final es \$27,500 con todos los documentos incluidos',
      'sender': 'dealer',
      'timestamp': DateTime.now().subtract(const Duration(days: 1)),
      'type': 'text',
      'conversationWith': 'Premium Motors',
    },
    {
      'id': '2',
      'text': '¿Cuál es el precio final con todo incluido?',
      'sender': 'user',
      'timestamp': DateTime.now().subtract(const Duration(days: 1, hours: 1)),
      'type': 'text',
      'conversationWith': 'Premium Motors',
    },
    {
      'id': '3',
      'text': 'Te envío fotos del interior',
      'sender': 'dealer',
      'timestamp': DateTime.now().subtract(const Duration(days: 2)),
      'type': 'media',
      'conversationWith': 'Auto Elite',
    },
    {
      'id': '4',
      'text':
          'Aquí está el link de financiamiento: https://example.com/financing',
      'sender': 'dealer',
      'timestamp': DateTime.now().subtract(const Duration(days: 3)),
      'type': 'link',
      'conversationWith': 'Cars & More',
    },
    {
      'id': '5',
      'text': 'El precio incluye garantía extendida de 2 años',
      'sender': 'dealer',
      'timestamp': DateTime.now().subtract(const Duration(days: 4)),
      'type': 'text',
      'conversationWith': 'Luxury Autos',
    },
  ];

  List<Map<String, dynamic>> get _filteredResults {
    if (_searchQuery.isEmpty) return [];

    var results = _mockResults.where((message) {
      final text = message['text'].toString().toLowerCase();
      final query = _searchQuery.toLowerCase();
      return text.contains(query);
    }).toList();

    // Filter by type
    if (_filterType != 'all') {
      results = results.where((m) => m['type'] == _filterType).toList();
    }

    // Filter by date range
    if (_fromDate != null) {
      results = results.where((m) {
        final timestamp = m['timestamp'] as DateTime;
        return timestamp.isAfter(_fromDate!);
      }).toList();
    }

    if (_toDate != null) {
      results = results.where((m) {
        final timestamp = m['timestamp'] as DateTime;
        return timestamp.isBefore(_toDate!.add(const Duration(days: 1)));
      }).toList();
    }

    return results;
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: TextField(
          controller: _searchController,
          autofocus: true,
          decoration: InputDecoration(
            hintText: 'Buscar en conversaciones...',
            border: InputBorder.none,
            hintStyle: TextStyle(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
          style: theme.textTheme.titleMedium,
          onChanged: (value) {
            setState(() => _searchQuery = value);
          },
        ),
        actions: [
          if (_searchQuery.isNotEmpty)
            IconButton(
              icon: const Icon(Icons.clear),
              onPressed: () {
                _searchController.clear();
                setState(() => _searchQuery = '');
              },
            ),
        ],
      ),
      body: Column(
        children: [
          // Filters
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: theme.colorScheme.surface,
              border: Border(
                bottom: BorderSide(
                  color: theme.colorScheme.outlineVariant,
                  width: 1,
                ),
              ),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Type filters
                SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: [
                      _FilterChip(
                        label: 'Todos',
                        value: 'all',
                        isSelected: _filterType == 'all',
                        onSelected: (value) {
                          setState(() => _filterType = value);
                        },
                      ),
                      const SizedBox(width: 8),
                      _FilterChip(
                        label: 'Texto',
                        value: 'text',
                        isSelected: _filterType == 'text',
                        onSelected: (value) {
                          setState(() => _filterType = value);
                        },
                      ),
                      const SizedBox(width: 8),
                      _FilterChip(
                        label: 'Multimedia',
                        value: 'media',
                        isSelected: _filterType == 'media',
                        onSelected: (value) {
                          setState(() => _filterType = value);
                        },
                      ),
                      const SizedBox(width: 8),
                      _FilterChip(
                        label: 'Enlaces',
                        value: 'link',
                        isSelected: _filterType == 'link',
                        onSelected: (value) {
                          setState(() => _filterType = value);
                        },
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 12),

                // Date filters
                Row(
                  children: [
                    Expanded(
                      child: OutlinedButton.icon(
                        onPressed: () => _selectDate(context, true),
                        icon: const Icon(Icons.calendar_today, size: 16),
                        label: Text(
                          _fromDate == null
                              ? 'Desde'
                              : _formatShortDate(_fromDate!),
                          style: theme.textTheme.bodySmall,
                        ),
                      ),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: OutlinedButton.icon(
                        onPressed: () => _selectDate(context, false),
                        icon: const Icon(Icons.calendar_today, size: 16),
                        label: Text(
                          _toDate == null
                              ? 'Hasta'
                              : _formatShortDate(_toDate!),
                          style: theme.textTheme.bodySmall,
                        ),
                      ),
                    ),
                    if (_fromDate != null || _toDate != null)
                      IconButton(
                        icon: const Icon(Icons.clear, size: 20),
                        onPressed: () {
                          setState(() {
                            _fromDate = null;
                            _toDate = null;
                          });
                        },
                      ),
                  ],
                ),
              ],
            ),
          ),

          // Results
          Expanded(
            child: _buildResults(),
          ),
        ],
      ),
    );
  }

  Widget _buildResults() {
    if (_searchQuery.isEmpty) {
      return _buildEmptyState(
        icon: Icons.search,
        title: 'Buscar mensajes',
        subtitle: 'Ingresa palabras clave para buscar en tus conversaciones',
      );
    }

    final results = _filteredResults;

    if (results.isEmpty) {
      return _buildEmptyState(
        icon: Icons.search_off,
        title: 'Sin resultados',
        subtitle: 'No se encontraron mensajes con "$_searchQuery"',
      );
    }

    return ListView.builder(
      itemCount: results.length,
      itemBuilder: (context, index) {
        final result = results[index];
        return _SearchResultTile(
          result: result,
          searchQuery: _searchQuery,
          onTap: () {
            // TODO: Navigate to message in conversation
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Navegando al mensaje...')),
            );
          },
        );
      },
    );
  }

  Widget _buildEmptyState({
    required IconData icon,
    required String title,
    required String subtitle,
  }) {
    final theme = Theme.of(context);

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              icon,
              size: 80,
              color: theme.colorScheme.outline,
            ),
            const SizedBox(height: 16),
            Text(
              title,
              style: theme.textTheme.titleLarge,
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 8),
            Text(
              subtitle,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _selectDate(BuildContext context, bool isFrom) async {
    final date = await showDatePicker(
      context: context,
      initialDate: (isFrom ? _fromDate : _toDate) ?? DateTime.now(),
      firstDate: DateTime.now().subtract(const Duration(days: 365)),
      lastDate: DateTime.now(),
    );

    if (date != null) {
      setState(() {
        if (isFrom) {
          _fromDate = date;
        } else {
          _toDate = date;
        }
      });
    }
  }

  String _formatShortDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year}';
  }
}

class _FilterChip extends StatelessWidget {
  final String label;
  final String value;
  final bool isSelected;
  final ValueChanged<String> onSelected;

  const _FilterChip({
    required this.label,
    required this.value,
    required this.isSelected,
    required this.onSelected,
  });

  @override
  Widget build(BuildContext context) {
    return FilterChip(
      label: Text(label),
      selected: isSelected,
      onSelected: (_) => onSelected(value),
    );
  }
}

class _SearchResultTile extends StatelessWidget {
  final Map<String, dynamic> result;
  final String searchQuery;
  final VoidCallback onTap;

  const _SearchResultTile({
    required this.result,
    required this.searchQuery,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final text = result['text'] as String;
    final sender = result['sender'] as String;
    final timestamp = result['timestamp'] as DateTime;
    final conversationWith = result['conversationWith'] as String;
    final type = result['type'] as String;

    return InkWell(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          border: Border(
            bottom: BorderSide(
              color: theme.colorScheme.outlineVariant,
              width: 1,
            ),
          ),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            Row(
              children: [
                Icon(
                  type == 'media'
                      ? Icons.image
                      : type == 'link'
                          ? Icons.link
                          : Icons.message,
                  size: 16,
                  color: theme.colorScheme.primary,
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    conversationWith,
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: theme.colorScheme.primary,
                    ),
                  ),
                ),
                Text(
                  _formatTimestamp(timestamp),
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),

            // Message text with highlighted search query
            RichText(
              text: _buildHighlightedText(
                text,
                searchQuery,
                theme.textTheme.bodyMedium!,
                theme.colorScheme.primary,
              ),
            ),
            const SizedBox(height: 4),

            // Sender indicator
            Text(
              sender == 'user' ? 'Tú' : conversationWith,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
                fontStyle: FontStyle.italic,
              ),
            ),
          ],
        ),
      ),
    );
  }

  TextSpan _buildHighlightedText(
    String text,
    String query,
    TextStyle baseStyle,
    Color highlightColor,
  ) {
    if (query.isEmpty) {
      return TextSpan(text: text, style: baseStyle);
    }

    final lowercaseText = text.toLowerCase();
    final lowercaseQuery = query.toLowerCase();
    final spans = <TextSpan>[];

    int start = 0;
    int index = lowercaseText.indexOf(lowercaseQuery);

    while (index != -1) {
      if (index > start) {
        spans.add(TextSpan(
          text: text.substring(start, index),
          style: baseStyle,
        ));
      }

      spans.add(TextSpan(
        text: text.substring(index, index + query.length),
        style: baseStyle.copyWith(
          backgroundColor: highlightColor.withValues(alpha: 0.2),
          fontWeight: FontWeight.bold,
        ),
      ));

      start = index + query.length;
      index = lowercaseText.indexOf(lowercaseQuery, start);
    }

    if (start < text.length) {
      spans.add(TextSpan(
        text: text.substring(start),
        style: baseStyle,
      ));
    }

    return TextSpan(children: spans);
  }

  String _formatTimestamp(DateTime timestamp) {
    final now = DateTime.now();
    final difference = now.difference(timestamp);

    if (difference.inDays == 0) {
      return 'Hoy';
    } else if (difference.inDays == 1) {
      return 'Ayer';
    } else if (difference.inDays < 7) {
      return '${difference.inDays} días';
    } else {
      return '${timestamp.day}/${timestamp.month}/${timestamp.year}';
    }
  }
}

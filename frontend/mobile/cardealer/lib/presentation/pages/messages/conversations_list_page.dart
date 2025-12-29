import 'package:flutter/material.dart';

/// CM-001: Conversations List Redesign
/// Lista de conversaciones con estado online, preview del último mensaje,
/// badges de mensajes no leídos y filtros
class ConversationsListPage extends StatefulWidget {
  const ConversationsListPage({super.key});

  @override
  State<ConversationsListPage> createState() => _ConversationsListPageState();
}

class _ConversationsListPageState extends State<ConversationsListPage> {
  String _selectedFilter = 'all';

  final List<Map<String, dynamic>> _conversations = [
    {
      'id': '1',
      'dealerName': 'Premium Motors',
      'vehicleInfo': 'Toyota Camry 2024',
      'lastMessage': '¿Cuándo puedes venir a verlo?',
      'timestamp': DateTime.now().subtract(const Duration(minutes: 5)),
      'unreadCount': 2,
      'isOnline': true,
      'isDelivered': true,
      'isRead': false,
      'avatar': 'https://ui-avatars.com/api/?name=Premium+Motors',
    },
    {
      'id': '2',
      'dealerName': 'Auto Elite',
      'vehicleInfo': 'Honda Civic 2023',
      'lastMessage': 'El precio ya incluye todo',
      'timestamp': DateTime.now().subtract(const Duration(hours: 2)),
      'unreadCount': 0,
      'isOnline': false,
      'isDelivered': true,
      'isRead': true,
      'avatar': 'https://ui-avatars.com/api/?name=Auto+Elite',
    },
    {
      'id': '3',
      'dealerName': 'Cars & More',
      'vehicleInfo': 'Ford Escape 2024',
      'lastMessage': 'Perfecto, te espero mañana',
      'timestamp': DateTime.now().subtract(const Duration(days: 1)),
      'unreadCount': 0,
      'isOnline': true,
      'isDelivered': true,
      'isRead': true,
      'avatar': 'https://ui-avatars.com/api/?name=Cars+More',
    },
    {
      'id': '4',
      'dealerName': 'Luxury Autos',
      'vehicleInfo': 'BMW X5 2024',
      'lastMessage': 'Puedo ofrecerte un descuento',
      'timestamp': DateTime.now().subtract(const Duration(days: 2)),
      'unreadCount': 5,
      'isOnline': false,
      'isDelivered': true,
      'isRead': false,
      'avatar': 'https://ui-avatars.com/api/?name=Luxury+Autos',
    },
    {
      'id': '5',
      'dealerName': 'Express Cars',
      'vehicleInfo': 'Mazda CX-5 2023',
      'lastMessage': 'Gracias por tu interés',
      'timestamp': DateTime.now().subtract(const Duration(days: 3)),
      'unreadCount': 0,
      'isOnline': false,
      'isDelivered': true,
      'isRead': true,
      'avatar': 'https://ui-avatars.com/api/?name=Express+Cars',
    },
  ];

  List<Map<String, dynamic>> get _filteredConversations {
    switch (_selectedFilter) {
      case 'unread':
        return _conversations.where((c) => c['unreadCount'] > 0).toList();
      case 'archived':
        return [];
      default:
        return _conversations;
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final unreadCount = _conversations.fold<int>(
      0,
      (sum, c) => sum + (c['unreadCount'] as int),
    );

    return Scaffold(
      appBar: AppBar(
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Mensajes'),
            if (unreadCount > 0)
              Text(
                '$unreadCount mensajes sin leer',
                style: theme.textTheme.bodySmall?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
              ),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.search),
            onPressed: () {
              Navigator.pushNamed(
                context,
                '/messages/conversation-search',
              );
            },
          ),
          IconButton(
            icon: const Icon(Icons.more_vert),
            onPressed: () {
              _showOptionsMenu(context);
            },
          ),
        ],
      ),
      body: Column(
        children: [
          // Filter chips
          SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                _FilterChip(
                  label: 'Todos',
                  value: 'all',
                  isSelected: _selectedFilter == 'all',
                  count: _conversations.length,
                  onSelected: (value) {
                    setState(() => _selectedFilter = value);
                  },
                ),
                const SizedBox(width: 8),
                _FilterChip(
                  label: 'No leídos',
                  value: 'unread',
                  isSelected: _selectedFilter == 'unread',
                  count:
                      _conversations.where((c) => c['unreadCount'] > 0).length,
                  onSelected: (value) {
                    setState(() => _selectedFilter = value);
                  },
                ),
                const SizedBox(width: 8),
                _FilterChip(
                  label: 'Archivados',
                  value: 'archived',
                  isSelected: _selectedFilter == 'archived',
                  count: 0,
                  onSelected: (value) {
                    setState(() => _selectedFilter = value);
                  },
                ),
              ],
            ),
          ),

          // Conversations list
          Expanded(
            child: _filteredConversations.isEmpty
                ? _buildEmptyState()
                : ListView.builder(
                    itemCount: _filteredConversations.length,
                    itemBuilder: (context, index) {
                      final conversation = _filteredConversations[index];
                      return _ConversationTile(
                        conversation: conversation,
                        onTap: () {
                          Navigator.pushNamed(
                            context,
                            '/messages/chat',
                            arguments: conversation,
                          );
                        },
                      );
                    },
                  ),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: 'conversations_fab',
        onPressed: () {
          // TODO: Navigate to dealer search for new conversation
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Buscar concesionario')),
          );
        },
        icon: const Icon(Icons.add),
        label: const Text('Nueva conversación'),
      ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.inbox_outlined,
            size: 80,
            color: Theme.of(context).colorScheme.outline,
          ),
          const SizedBox(height: 16),
          Text(
            'No hay conversaciones',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 8),
          Text(
            _selectedFilter == 'unread'
                ? 'No tienes mensajes sin leer'
                : _selectedFilter == 'archived'
                    ? 'No tienes conversaciones archivadas'
                    : 'Inicia una conversación con un concesionario',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  void _showOptionsMenu(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.mark_chat_read),
              title: const Text('Marcar todas como leídas'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Todas marcadas como leídas')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.settings),
              title: const Text('Configuración de notificaciones'),
              onTap: () {
                Navigator.pop(context);
                Navigator.pushNamed(context, '/messages/notification-settings');
              },
            ),
          ],
        ),
      ),
    );
  }
}

class _FilterChip extends StatelessWidget {
  final String label;
  final String value;
  final bool isSelected;
  final int count;
  final ValueChanged<String> onSelected;

  const _FilterChip({
    required this.label,
    required this.value,
    required this.isSelected,
    required this.count,
    required this.onSelected,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return FilterChip(
      label: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(label),
          if (count > 0) ...[
            const SizedBox(width: 8),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
              decoration: BoxDecoration(
                color: isSelected
                    ? theme.colorScheme.onPrimary
                    : theme.colorScheme.primary,
                borderRadius: BorderRadius.circular(10),
              ),
              child: Text(
                count.toString(),
                style: theme.textTheme.labelSmall?.copyWith(
                  color: isSelected
                      ? theme.colorScheme.primary
                      : theme.colorScheme.onPrimary,
                ),
              ),
            ),
          ],
        ],
      ),
      selected: isSelected,
      onSelected: (_) => onSelected(value),
    );
  }
}

class _ConversationTile extends StatelessWidget {
  final Map<String, dynamic> conversation;
  final VoidCallback onTap;

  const _ConversationTile({
    required this.conversation,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final unreadCount = conversation['unreadCount'] as int;
    final isOnline = conversation['isOnline'] as bool;
    final timestamp = conversation['timestamp'] as DateTime;
    final isDelivered = conversation['isDelivered'] as bool;
    final isRead = conversation['isRead'] as bool;

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
        child: Row(
          children: [
            // Avatar with online indicator
            Stack(
              children: [
                CircleAvatar(
                  radius: 28,
                  backgroundImage: NetworkImage(conversation['avatar']),
                ),
                if (isOnline)
                  Positioned(
                    right: 0,
                    bottom: 0,
                    child: Container(
                      width: 16,
                      height: 16,
                      decoration: BoxDecoration(
                        color: theme.colorScheme.tertiary,
                        shape: BoxShape.circle,
                        border: Border.all(
                          color: theme.colorScheme.surface,
                          width: 2,
                        ),
                      ),
                    ),
                  ),
              ],
            ),
            const SizedBox(width: 12),

            // Content
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Dealer name and timestamp
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          conversation['dealerName'],
                          style: theme.textTheme.titleMedium?.copyWith(
                            fontWeight: unreadCount > 0
                                ? FontWeight.bold
                                : FontWeight.normal,
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
                  const SizedBox(height: 4),

                  // Vehicle info
                  Text(
                    conversation['vehicleInfo'],
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.primary,
                    ),
                  ),
                  const SizedBox(height: 4),

                  // Last message
                  Row(
                    children: [
                      if (unreadCount == 0) ...[
                        Icon(
                          isRead
                              ? Icons.done_all
                              : isDelivered
                                  ? Icons.done
                                  : Icons.schedule,
                          size: 16,
                          color: isRead
                              ? theme.colorScheme.primary
                              : theme.colorScheme.onSurfaceVariant,
                        ),
                        const SizedBox(width: 4),
                      ],
                      Expanded(
                        child: Text(
                          conversation['lastMessage'],
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: unreadCount > 0
                                ? theme.colorScheme.onSurface
                                : theme.colorScheme.onSurfaceVariant,
                            fontWeight: unreadCount > 0
                                ? FontWeight.w500
                                : FontWeight.normal,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),

            // Unread badge
            if (unreadCount > 0) ...[
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: theme.colorScheme.primary,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(
                  unreadCount.toString(),
                  style: theme.textTheme.labelSmall?.copyWith(
                    color: theme.colorScheme.onPrimary,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  String _formatTimestamp(DateTime timestamp) {
    final now = DateTime.now();
    final difference = now.difference(timestamp);

    if (difference.inMinutes < 60) {
      return '${difference.inMinutes}m';
    } else if (difference.inHours < 24) {
      return '${difference.inHours}h';
    } else if (difference.inDays < 7) {
      return '${difference.inDays}d';
    } else {
      return '${timestamp.day}/${timestamp.month}';
    }
  }
}

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// SF-008: Wishlist Notifications
/// Sistema de notificaciones inteligentes para wishlist con alertas
/// personalizadas, triggers configurables y gestión de preferencias

class WishlistNotificationsPage extends StatefulWidget {
  const WishlistNotificationsPage({super.key});

  @override
  State<WishlistNotificationsPage> createState() =>
      _WishlistNotificationsPageState();
}

class _WishlistNotificationsPageState extends State<WishlistNotificationsPage>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  List<WishlistNotification> _notifications = [];
  List<NotificationRule> _rules = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _loadData();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadData() async {
    setState(() => _isLoading = true);
    await Future.delayed(const Duration(seconds: 1));

    setState(() {
      _notifications = _generateMockNotifications();
      _rules = _generateMockRules();
      _isLoading = false;
    });
  }

  List<WishlistNotification> _generateMockNotifications() {
    final now = DateTime.now();
    return [
      WishlistNotification(
        id: '1',
        vehicleName: 'Toyota Corolla 2022',
        type: NotificationType.priceDown,
        message: 'El precio bajó \$2,000',
        oldValue: '\$18,000',
        newValue: '\$16,000',
        timestamp: now.subtract(const Duration(hours: 2)),
        isRead: false,
        vehicleId: 'v1',
        imageUrl: 'https://via.placeholder.com/400x300',
      ),
      WishlistNotification(
        id: '2',
        vehicleName: 'Honda Civic 2023',
        type: NotificationType.available,
        message: 'Ahora disponible en tu zona',
        timestamp: now.subtract(const Duration(hours: 5)),
        isRead: false,
        vehicleId: 'v2',
        imageUrl: 'https://via.placeholder.com/400x300',
      ),
      WishlistNotification(
        id: '3',
        vehicleName: 'Ford Mustang 2021',
        type: NotificationType.similar,
        message: '3 vehículos similares disponibles',
        timestamp: now.subtract(const Duration(days: 1)),
        isRead: true,
        vehicleId: 'v3',
        imageUrl: 'https://via.placeholder.com/400x300',
      ),
      WishlistNotification(
        id: '4',
        vehicleName: 'Mazda CX-5 2022',
        type: NotificationType.expiringSoon,
        message: 'Esta oferta expira en 2 días',
        timestamp: now.subtract(const Duration(days: 1, hours: 3)),
        isRead: true,
        vehicleId: 'v4',
        imageUrl: 'https://via.placeholder.com/400x300',
      ),
    ];
  }

  List<NotificationRule> _generateMockRules() {
    return [
      NotificationRule(
        id: '1',
        name: 'Bajada de precio',
        type: NotificationType.priceDown,
        isEnabled: true,
        threshold: 5.0, // 5% de descuento
        frequency: NotificationFrequency.immediate,
      ),
      NotificationRule(
        id: '2',
        name: 'Nuevos disponibles',
        type: NotificationType.available,
        isEnabled: true,
        frequency: NotificationFrequency.daily,
      ),
      NotificationRule(
        id: '3',
        name: 'Vehículos similares',
        type: NotificationType.similar,
        isEnabled: false,
        frequency: NotificationFrequency.weekly,
      ),
      NotificationRule(
        id: '4',
        name: 'Ofertas próximas a expirar',
        type: NotificationType.expiringSoon,
        isEnabled: true,
        frequency: NotificationFrequency.daily,
      ),
    ];
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Notificaciones de Wishlist'),
        actions: [
          IconButton(
            icon: const Icon(Icons.mark_email_read),
            onPressed: _markAllAsRead,
            tooltip: 'Marcar todo como leído',
          ),
          IconButton(
            icon: const Icon(Icons.settings),
            onPressed: _showNotificationSettings,
            tooltip: 'Configuración',
          ),
        ],
        bottom: TabBar(
          controller: _tabController,
          tabs: [
            Tab(
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Text('Notificaciones'),
                  if (_notifications.where((n) => !n.isRead).isNotEmpty)
                    Padding(
                      padding: const EdgeInsets.only(left: AppSpacing.xs),
                      child: Container(
                        padding: const EdgeInsets.all(4),
                        decoration: const BoxDecoration(
                          color: AppColors.error,
                          shape: BoxShape.circle,
                        ),
                        child: Text(
                          '${_notifications.where((n) => !n.isRead).length}',
                          style: AppTypography.bodySmall.copyWith(
                            color: Colors.white,
                            fontSize: 10,
                          ),
                        ),
                      ),
                    ),
                ],
              ),
            ),
            const Tab(text: 'Reglas'),
          ],
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : TabBarView(
              controller: _tabController,
              children: [
                _buildNotificationsTab(),
                _buildRulesTab(),
              ],
            ),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: 'wishlist_fab',
        onPressed: _createNewRule,
        icon: const Icon(Icons.add),
        label: const Text('Nueva Regla'),
      ),
    );
  }

  Widget _buildNotificationsTab() {
    if (_notifications.isEmpty) {
      return _buildEmptyState();
    }

    return RefreshIndicator(
      onRefresh: _loadData,
      child: ListView.builder(
        padding: const EdgeInsets.all(AppSpacing.md),
        itemCount: _notifications.length,
        itemBuilder: (context, index) {
          final notification = _notifications[index];
          return _buildNotificationCard(notification, index);
        },
      ),
    );
  }

  Widget _buildNotificationCard(WishlistNotification notification, int index) {
    return Dismissible(
      key: Key(notification.id),
      direction: DismissDirection.endToStart,
      background: Container(
        alignment: Alignment.centerRight,
        padding: const EdgeInsets.only(right: AppSpacing.lg),
        decoration: BoxDecoration(
          color: AppColors.error,
          borderRadius: BorderRadius.circular(12),
        ),
        child: const Icon(Icons.delete, color: Colors.white),
      ),
      onDismissed: (direction) {
        setState(() {
          _notifications.removeAt(index);
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: const Text('Notificación eliminada'),
            action: SnackBarAction(
              label: 'Deshacer',
              onPressed: () {
                setState(() {
                  _notifications.insert(index, notification);
                });
              },
            ),
          ),
        );
      },
      child: Card(
        margin: const EdgeInsets.only(bottom: AppSpacing.md),
        color: notification.isRead
            ? null
            : AppColors.primary.withValues(alpha: 0.05),
        child: InkWell(
          onTap: () {
            setState(() {
              notification.isRead = true;
            });
            _navigateToVehicle(notification.vehicleId);
          },
          borderRadius: BorderRadius.circular(12),
          child: Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Type Icon
                Container(
                  width: 48,
                  height: 48,
                  decoration: BoxDecoration(
                    color: _getNotificationColor(notification.type)
                        .withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Icon(
                    _getNotificationIcon(notification.type),
                    color: _getNotificationColor(notification.type),
                    size: 24,
                  ),
                ),
                const SizedBox(width: AppSpacing.md),

                // Content
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              notification.vehicleName,
                              style: AppTypography.bodyLarge.copyWith(
                                fontWeight: notification.isRead
                                    ? FontWeight.normal
                                    : FontWeight.bold,
                              ),
                            ),
                          ),
                          if (!notification.isRead)
                            Container(
                              width: 8,
                              height: 8,
                              decoration: const BoxDecoration(
                                color: AppColors.primary,
                                shape: BoxShape.circle,
                              ),
                            ),
                        ],
                      ),
                      const SizedBox(height: AppSpacing.xs),
                      Text(
                        notification.message,
                        style: AppTypography.bodyMedium.copyWith(
                          color: _getNotificationColor(notification.type),
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      if (notification.oldValue != null &&
                          notification.newValue != null) ...[
                        const SizedBox(height: AppSpacing.xs),
                        Row(
                          children: [
                            Text(
                              notification.oldValue!,
                              style: AppTypography.bodySmall.copyWith(
                                decoration: TextDecoration.lineThrough,
                                color: Colors.grey[600],
                              ),
                            ),
                            const SizedBox(width: AppSpacing.sm),
                            const Icon(Icons.arrow_forward, size: 12),
                            const SizedBox(width: AppSpacing.sm),
                            Text(
                              notification.newValue!,
                              style: AppTypography.bodySmall.copyWith(
                                color: AppColors.success,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ],
                        ),
                      ],
                      const SizedBox(height: AppSpacing.xs),
                      Text(
                        _formatTimestamp(notification.timestamp),
                        style: AppTypography.bodySmall.copyWith(
                          color: Colors.grey[600],
                        ),
                      ),
                    ],
                  ),
                ),

                // Vehicle Thumbnail
                if (notification.imageUrl != null)
                  ClipRRect(
                    borderRadius: BorderRadius.circular(8),
                    child: Image.network(
                      notification.imageUrl!,
                      width: 60,
                      height: 50,
                      fit: BoxFit.cover,
                    ),
                  ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildRulesTab() {
    return ListView.builder(
      padding: const EdgeInsets.all(AppSpacing.md),
      itemCount: _rules.length,
      itemBuilder: (context, index) {
        final rule = _rules[index];
        return _buildRuleCard(rule);
      },
    );
  }

  Widget _buildRuleCard(NotificationRule rule) {
    return Card(
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          children: [
            Row(
              children: [
                Icon(
                  _getNotificationIcon(rule.type),
                  color: _getNotificationColor(rule.type),
                ),
                const SizedBox(width: AppSpacing.md),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        rule.name,
                        style: AppTypography.bodyLarge.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Text(
                        _getFrequencyText(rule.frequency),
                        style: AppTypography.bodySmall.copyWith(
                          color: Colors.grey[600],
                        ),
                      ),
                    ],
                  ),
                ),
                Switch(
                  value: rule.isEnabled,
                  onChanged: (value) {
                    setState(() {
                      rule.isEnabled = value;
                    });
                  },
                  activeThumbColor: AppColors.primary,
                ),
              ],
            ),
            if (rule.threshold != null) ...[
              const SizedBox(height: AppSpacing.md),
              Row(
                children: [
                  const Icon(Icons.trending_down, size: 16),
                  const SizedBox(width: AppSpacing.sm),
                  Text(
                    'Umbral: ${rule.threshold}%',
                    style: AppTypography.bodyMedium,
                  ),
                  const Spacer(),
                  TextButton(
                    onPressed: () => _editRuleThreshold(rule),
                    child: const Text('Editar'),
                  ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.notifications_none,
            size: 80,
            color: Colors.grey[300],
          ),
          const SizedBox(height: AppSpacing.lg),
          Text(
            'No hay notificaciones',
            style: AppTypography.h6.copyWith(
              color: Colors.grey[600],
            ),
          ),
          const SizedBox(height: AppSpacing.sm),
          Text(
            'Configura reglas para recibir alertas',
            style: AppTypography.bodyMedium.copyWith(
              color: Colors.grey[500],
            ),
          ),
        ],
      ),
    );
  }

  IconData _getNotificationIcon(NotificationType type) {
    switch (type) {
      case NotificationType.priceDown:
        return Icons.trending_down;
      case NotificationType.available:
        return Icons.check_circle;
      case NotificationType.similar:
        return Icons.compare_arrows;
      case NotificationType.expiringSoon:
        return Icons.access_time;
    }
  }

  Color _getNotificationColor(NotificationType type) {
    switch (type) {
      case NotificationType.priceDown:
        return AppColors.success;
      case NotificationType.available:
        return AppColors.info;
      case NotificationType.similar:
        return AppColors.warning;
      case NotificationType.expiringSoon:
        return AppColors.error;
    }
  }

  String _getFrequencyText(NotificationFrequency frequency) {
    switch (frequency) {
      case NotificationFrequency.immediate:
        return 'Notificación inmediata';
      case NotificationFrequency.daily:
        return 'Resumen diario';
      case NotificationFrequency.weekly:
        return 'Resumen semanal';
    }
  }

  String _formatTimestamp(DateTime timestamp) {
    final now = DateTime.now();
    final diff = now.difference(timestamp);

    if (diff.inMinutes < 60) {
      return 'Hace ${diff.inMinutes}m';
    } else if (diff.inHours < 24) {
      return 'Hace ${diff.inHours}h';
    } else if (diff.inDays == 1) {
      return 'Ayer';
    } else if (diff.inDays < 7) {
      return 'Hace ${diff.inDays}d';
    } else {
      return DateFormat('d MMM', 'es').format(timestamp);
    }
  }

  void _markAllAsRead() {
    setState(() {
      for (var notification in _notifications) {
        notification.isRead = true;
      }
    });
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
          content: Text('Todas las notificaciones marcadas como leídas')),
    );
  }

  void _showNotificationSettings() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) => const NotificationSettingsSheet(),
    );
  }

  void _createNewRule() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Nueva Regla'),
        content: const Text(
          'Esta funcionalidad estará disponible próximamente.',
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

  void _editRuleThreshold(NotificationRule rule) {
    showDialog(
      context: context,
      builder: (context) {
        double newThreshold = rule.threshold ?? 5.0;
        return StatefulBuilder(
          builder: (context, setState) {
            return AlertDialog(
              title: const Text('Editar Umbral'),
              content: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    'Notificar cuando el precio baje ${newThreshold.toStringAsFixed(0)}% o más',
                  ),
                  const SizedBox(height: AppSpacing.md),
                  Slider(
                    value: newThreshold,
                    min: 1,
                    max: 20,
                    divisions: 19,
                    label: '${newThreshold.toStringAsFixed(0)}%',
                    onChanged: (value) {
                      setState(() {
                        newThreshold = value;
                      });
                    },
                  ),
                ],
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.pop(context),
                  child: const Text('Cancelar'),
                ),
                FilledButton(
                  onPressed: () {
                    this.setState(() {
                      rule.threshold = newThreshold;
                    });
                    Navigator.pop(context);
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text('Umbral actualizado')),
                    );
                  },
                  child: const Text('Guardar'),
                ),
              ],
            );
          },
        );
      },
    );
  }

  void _navigateToVehicle(String vehicleId) {
    // Navigate to vehicle detail
  }
}

class NotificationSettingsSheet extends StatefulWidget {
  const NotificationSettingsSheet({super.key});

  @override
  State<NotificationSettingsSheet> createState() =>
      _NotificationSettingsSheetState();
}

class _NotificationSettingsSheetState extends State<NotificationSettingsSheet> {
  bool _pushEnabled = true;
  bool _emailEnabled = false;
  bool _smsEnabled = false;
  bool _soundEnabled = true;
  bool _vibrationEnabled = true;
  final String _quietHoursStart = '22:00';
  final String _quietHoursEnd = '08:00';

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Configuración de Notificaciones',
            style: AppTypography.h5.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: AppSpacing.lg),
          SwitchListTile(
            value: _pushEnabled,
            onChanged: (value) => setState(() => _pushEnabled = value),
            title: const Text('Notificaciones Push'),
            subtitle: const Text('En la aplicación'),
            secondary: const Icon(Icons.notifications_active),
            activeThumbColor: AppColors.primary,
          ),
          SwitchListTile(
            value: _emailEnabled,
            onChanged: (value) => setState(() => _emailEnabled = value),
            title: const Text('Notificaciones por Email'),
            subtitle: const Text('Resúmenes periódicos'),
            secondary: const Icon(Icons.email),
            activeThumbColor: AppColors.primary,
          ),
          SwitchListTile(
            value: _smsEnabled,
            onChanged: (value) => setState(() => _smsEnabled = value),
            title: const Text('Notificaciones por SMS'),
            subtitle: const Text('Solo alertas importantes'),
            secondary: const Icon(Icons.sms),
            activeThumbColor: AppColors.primary,
          ),
          const Divider(height: AppSpacing.xl),
          SwitchListTile(
            value: _soundEnabled,
            onChanged: (value) => setState(() => _soundEnabled = value),
            title: const Text('Sonido'),
            activeThumbColor: AppColors.primary,
          ),
          SwitchListTile(
            value: _vibrationEnabled,
            onChanged: (value) => setState(() => _vibrationEnabled = value),
            title: const Text('Vibración'),
            activeThumbColor: AppColors.primary,
          ),
          const SizedBox(height: AppSpacing.md),
          Text(
            'Horario silencioso: $_quietHoursStart - $_quietHoursEnd',
            style: AppTypography.bodyMedium,
          ),
          const SizedBox(height: AppSpacing.xl),
          SizedBox(
            width: double.infinity,
            child: FilledButton(
              onPressed: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Configuración guardada')),
                );
              },
              child: const Text('Guardar'),
            ),
          ),
        ],
      ),
    );
  }
}

// Models
class WishlistNotification {
  final String id;
  final String vehicleName;
  final NotificationType type;
  final String message;
  final String? oldValue;
  final String? newValue;
  final DateTime timestamp;
  bool isRead;
  final String vehicleId;
  final String? imageUrl;

  WishlistNotification({
    required this.id,
    required this.vehicleName,
    required this.type,
    required this.message,
    this.oldValue,
    this.newValue,
    required this.timestamp,
    this.isRead = false,
    required this.vehicleId,
    this.imageUrl,
  });
}

class NotificationRule {
  final String id;
  final String name;
  final NotificationType type;
  bool isEnabled;
  double? threshold;
  final NotificationFrequency frequency;

  NotificationRule({
    required this.id,
    required this.name,
    required this.type,
    required this.isEnabled,
    this.threshold,
    required this.frequency,
  });
}

enum NotificationType {
  priceDown,
  available,
  similar,
  expiringSoon,
}

enum NotificationFrequency {
  immediate,
  daily,
  weekly,
}

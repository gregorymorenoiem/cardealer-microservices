import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// SF-006: Recently Viewed Tracker
/// Widget para mostrar historial de vehículos vistos recientemente
/// con acciones rápidas y análisis de comportamiento

class RecentlyViewedWidget extends StatefulWidget {
  final String userId;
  final int maxItems;
  final bool showAnalytics;

  const RecentlyViewedWidget({
    super.key,
    required this.userId,
    this.maxItems = 20,
    this.showAnalytics = true,
  });

  @override
  State<RecentlyViewedWidget> createState() => _RecentlyViewedWidgetState();
}

class _RecentlyViewedWidgetState extends State<RecentlyViewedWidget>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  // ignore: unused_field
  final String _viewMode = 'recent'; // recent, grouped, analytics
  List<ViewedVehicle> _recentVehicles = [];
  Map<String, List<ViewedVehicle>> _groupedVehicles = {};
  ViewingAnalytics? _analytics;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _loadData();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadData() async {
    setState(() => _isLoading = true);

    // Simular carga de datos
    await Future.delayed(const Duration(seconds: 1));

    setState(() {
      _recentVehicles = _generateMockRecentVehicles();
      _groupedVehicles = _groupVehiclesByDate();
      _analytics = _generateMockAnalytics();
      _isLoading = false;
    });
  }

  List<ViewedVehicle> _generateMockRecentVehicles() {
    final now = DateTime.now();
    return List.generate(15, (index) {
      final hoursAgo = index * 2;
      return ViewedVehicle(
        id: 'vehicle_$index',
        name: 'Toyota Corolla ${2020 + index}',
        brand: 'Toyota',
        model: 'Corolla',
        year: 2020 + index,
        price: 15000 + (index * 1000),
        imageUrl: 'https://via.placeholder.com/400x300',
        viewedAt: now.subtract(Duration(hours: hoursAgo)),
        viewDuration: Duration(minutes: 2 + index),
        viewCount: index + 1,
        isFavorite: index % 3 == 0,
      );
    });
  }

  Map<String, List<ViewedVehicle>> _groupVehiclesByDate() {
    final Map<String, List<ViewedVehicle>> grouped = {};

    for (var vehicle in _recentVehicles) {
      final date = DateFormat('yyyy-MM-dd').format(vehicle.viewedAt);
      grouped.putIfAbsent(date, () => []).add(vehicle);
    }

    return grouped;
  }

  ViewingAnalytics _generateMockAnalytics() {
    return ViewingAnalytics(
      totalViews: 45,
      uniqueVehicles: 15,
      averageViewDuration: const Duration(minutes: 3, seconds: 30),
      topBrands: ['Toyota', 'Honda', 'Ford'],
      topPriceRange: '\$15,000 - \$25,000',
      mostViewedTime: '18:00 - 21:00',
      repeatViews: 12,
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Vistos Recientemente'),
        actions: [
          IconButton(
            icon: const Icon(Icons.delete_outline),
            onPressed: _showClearHistoryDialog,
            tooltip: 'Borrar historial',
          ),
          PopupMenuButton<String>(
            icon: const Icon(Icons.more_vert),
            onSelected: (value) {
              if (value == 'export') {
                _exportHistory();
              } else if (value == 'privacy') {
                _showPrivacySettings();
              }
            },
            itemBuilder: (context) => [
              const PopupMenuItem(
                value: 'export',
                child: Row(
                  children: [
                    Icon(Icons.download),
                    SizedBox(width: AppSpacing.sm),
                    Text('Exportar historial'),
                  ],
                ),
              ),
              const PopupMenuItem(
                value: 'privacy',
                child: Row(
                  children: [
                    Icon(Icons.privacy_tip_outlined),
                    SizedBox(width: AppSpacing.sm),
                    Text('Configuración de privacidad'),
                  ],
                ),
              ),
            ],
          ),
        ],
        bottom: TabBar(
          controller: _tabController,
          tabs: const [
            Tab(icon: Icon(Icons.history), text: 'Recientes'),
            Tab(icon: Icon(Icons.calendar_today), text: 'Por Fecha'),
            Tab(icon: Icon(Icons.insights), text: 'Análisis'),
          ],
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : TabBarView(
              controller: _tabController,
              children: [
                _buildRecentTab(),
                _buildGroupedTab(),
                _buildAnalyticsTab(),
              ],
            ),
    );
  }

  Widget _buildRecentTab() {
    if (_recentVehicles.isEmpty) {
      return _buildEmptyState();
    }

    return RefreshIndicator(
      onRefresh: _loadData,
      child: ListView.builder(
        padding: const EdgeInsets.all(AppSpacing.md),
        itemCount: _recentVehicles.length,
        itemBuilder: (context, index) {
          final vehicle = _recentVehicles[index];
          return _buildVehicleCard(vehicle, index);
        },
      ),
    );
  }

  Widget _buildVehicleCard(ViewedVehicle vehicle, int index) {
    return Dismissible(
      key: Key(vehicle.id),
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
          _recentVehicles.removeAt(index);
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('${vehicle.name} eliminado del historial'),
            action: SnackBarAction(
              label: 'Deshacer',
              onPressed: () {
                setState(() {
                  _recentVehicles.insert(index, vehicle);
                });
              },
            ),
          ),
        );
      },
      child: Card(
        margin: const EdgeInsets.only(bottom: AppSpacing.md),
        child: InkWell(
          onTap: () => _navigateToVehicleDetail(vehicle.id),
          borderRadius: BorderRadius.circular(12),
          child: Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Vehicle Image
                ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: Image.network(
                    vehicle.imageUrl,
                    width: 100,
                    height: 80,
                    fit: BoxFit.cover,
                  ),
                ),
                const SizedBox(width: AppSpacing.md),

                // Vehicle Info
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              vehicle.name,
                              style: AppTypography.bodyLarge.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ),
                          if (vehicle.isFavorite)
                            const Icon(
                              Icons.favorite,
                              size: 16,
                              color: AppColors.error,
                            ),
                        ],
                      ),
                      const SizedBox(height: AppSpacing.xs),
                      Text(
                        '\$${NumberFormat('#,###').format(vehicle.price)}',
                        style: AppTypography.bodyMedium.copyWith(
                          color: AppColors.primary,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      const SizedBox(height: AppSpacing.xs),
                      Row(
                        children: [
                          Icon(
                            Icons.access_time,
                            size: 14,
                            color: Colors.grey[600],
                          ),
                          const SizedBox(width: 4),
                          Text(
                            _formatTimeAgo(vehicle.viewedAt),
                            style: AppTypography.bodySmall.copyWith(
                              color: Colors.grey[600],
                            ),
                          ),
                          const SizedBox(width: AppSpacing.md),
                          Icon(
                            Icons.remove_red_eye,
                            size: 14,
                            color: Colors.grey[600],
                          ),
                          const SizedBox(width: 4),
                          Text(
                            '${vehicle.viewCount}x',
                            style: AppTypography.bodySmall.copyWith(
                              color: Colors.grey[600],
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),

                // Quick Actions
                Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    IconButton(
                      icon: Icon(
                        vehicle.isFavorite
                            ? Icons.favorite
                            : Icons.favorite_border,
                        color: vehicle.isFavorite ? AppColors.error : null,
                      ),
                      onPressed: () => _toggleFavorite(vehicle),
                      padding: EdgeInsets.zero,
                      constraints: const BoxConstraints(),
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    IconButton(
                      icon: const Icon(Icons.share_outlined),
                      onPressed: () => _shareVehicle(vehicle),
                      padding: EdgeInsets.zero,
                      constraints: const BoxConstraints(),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildGroupedTab() {
    if (_groupedVehicles.isEmpty) {
      return _buildEmptyState();
    }

    final sortedDates = _groupedVehicles.keys.toList()
      ..sort((a, b) => b.compareTo(a));

    return ListView.builder(
      padding: const EdgeInsets.all(AppSpacing.md),
      itemCount: sortedDates.length,
      itemBuilder: (context, index) {
        final date = sortedDates[index];
        final vehicles = _groupedVehicles[date]!;

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: const EdgeInsets.symmetric(vertical: AppSpacing.sm),
              child: Row(
                children: [
                  Text(
                    _formatDate(DateTime.parse(date)),
                    style: AppTypography.h6.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.sm,
                      vertical: 2,
                    ),
                    decoration: BoxDecoration(
                      color: AppColors.primary.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Text(
                      '${vehicles.length} vehículos',
                      style: AppTypography.bodySmall.copyWith(
                        color: AppColors.primary,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ],
              ),
            ),
            ...vehicles.map((vehicle) => _buildVehicleCard(
                  vehicle,
                  _recentVehicles.indexOf(vehicle),
                )),
            const SizedBox(height: AppSpacing.md),
          ],
        );
      },
    );
  }

  Widget _buildAnalyticsTab() {
    if (_analytics == null) {
      return const Center(child: Text('No hay datos de análisis'));
    }

    return SingleChildScrollView(
      padding: const EdgeInsets.all(AppSpacing.md),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Summary Cards
          Row(
            children: [
              Expanded(
                child: _buildStatCard(
                  'Total Vistas',
                  '${_analytics!.totalViews}',
                  Icons.visibility,
                  AppColors.primary,
                ),
              ),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: _buildStatCard(
                  'Vehículos Únicos',
                  '${_analytics!.uniqueVehicles}',
                  Icons.directions_car,
                  AppColors.success,
                ),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.md),

          Row(
            children: [
              Expanded(
                child: _buildStatCard(
                  'Vistas Repetidas',
                  '${_analytics!.repeatViews}',
                  Icons.repeat,
                  AppColors.warning,
                ),
              ),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: _buildStatCard(
                  'Duración Promedio',
                  '${_analytics!.averageViewDuration.inMinutes}m',
                  Icons.timer,
                  AppColors.info,
                ),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.xl),

          // Top Brands
          Card(
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.trending_up, color: AppColors.primary),
                      const SizedBox(width: AppSpacing.sm),
                      Text(
                        'Marcas Más Vistas',
                        style: AppTypography.h6.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: AppSpacing.md),
                  ..._analytics!.topBrands.asMap().entries.map((entry) {
                    final index = entry.key;
                    final brand = entry.value;
                    return Padding(
                      padding: const EdgeInsets.only(bottom: AppSpacing.sm),
                      child: Row(
                        children: [
                          CircleAvatar(
                            radius: 16,
                            backgroundColor:
                                AppColors.primary.withValues(alpha: 0.1),
                            child: Text(
                              '${index + 1}',
                              style: AppTypography.bodySmall.copyWith(
                                color: AppColors.primary,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ),
                          const SizedBox(width: AppSpacing.md),
                          Text(
                            brand,
                            style: AppTypography.bodyLarge,
                          ),
                        ],
                      ),
                    );
                  }),
                ],
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.md),

          // Viewing Patterns
          Card(
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.insights, color: AppColors.success),
                      const SizedBox(width: AppSpacing.sm),
                      Text(
                        'Patrones de Búsqueda',
                        style: AppTypography.h6.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: AppSpacing.md),
                  _buildInfoRow(
                      'Rango de precio favorito', _analytics!.topPriceRange),
                  _buildInfoRow(
                      'Horario más activo', _analytics!.mostViewedTime),
                  _buildInfoRow(
                    'Tasa de repetición',
                    '${((_analytics!.repeatViews / _analytics!.totalViews) * 100).toStringAsFixed(1)}%',
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.md),

          // Insights
          Card(
            color: AppColors.primary.withValues(alpha: 0.1),
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.lightbulb_outline,
                          color: AppColors.primary),
                      const SizedBox(width: AppSpacing.sm),
                      Text(
                        'Insights',
                        style: AppTypography.h6.copyWith(
                          fontWeight: FontWeight.bold,
                          color: AppColors.primary,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: AppSpacing.md),
                  const Text(
                    'Basado en tu historial, te recomendamos:',
                    style: AppTypography.bodyMedium,
                  ),
                  const SizedBox(height: AppSpacing.sm),
                  _buildInsightItem(
                      'Crear alertas para ${_analytics!.topBrands.first}'),
                  _buildInsightItem(
                      'Buscar en el rango ${_analytics!.topPriceRange}'),
                  _buildInsightItem(
                      'Revisitar vehículos guardados en favoritos'),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatCard(
      String label, String value, IconData icon, Color color) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          children: [
            Icon(icon, color: color, size: 32),
            const SizedBox(height: AppSpacing.sm),
            Text(
              value,
              style: AppTypography.h4.copyWith(
                fontWeight: FontWeight.bold,
                color: color,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              label,
              style: AppTypography.bodySmall.copyWith(
                color: Colors.grey[600],
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.sm),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: AppTypography.bodyMedium.copyWith(
              color: Colors.grey[700],
            ),
          ),
          Text(
            value,
            style: AppTypography.bodyMedium.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildInsightItem(String text) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.xs),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Icon(
            Icons.check_circle,
            size: 16,
            color: AppColors.success,
          ),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Text(
              text,
              style: AppTypography.bodyMedium,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.history,
            size: 80,
            color: Colors.grey[300],
          ),
          const SizedBox(height: AppSpacing.lg),
          Text(
            'No hay vehículos vistos recientemente',
            style: AppTypography.h6.copyWith(
              color: Colors.grey[600],
            ),
          ),
          const SizedBox(height: AppSpacing.sm),
          Text(
            'Explora nuestro catálogo para empezar',
            style: AppTypography.bodyMedium.copyWith(
              color: Colors.grey[500],
            ),
          ),
        ],
      ),
    );
  }

  String _formatTimeAgo(DateTime dateTime) {
    final now = DateTime.now();
    final difference = now.difference(dateTime);

    if (difference.inDays > 0) {
      return 'Hace ${difference.inDays}d';
    } else if (difference.inHours > 0) {
      return 'Hace ${difference.inHours}h';
    } else if (difference.inMinutes > 0) {
      return 'Hace ${difference.inMinutes}m';
    } else {
      return 'Ahora';
    }
  }

  String _formatDate(DateTime dateTime) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    final yesterday = today.subtract(const Duration(days: 1));
    final date = DateTime(dateTime.year, dateTime.month, dateTime.day);

    if (date == today) {
      return 'Hoy';
    } else if (date == yesterday) {
      return 'Ayer';
    } else if (now.difference(date).inDays < 7) {
      return DateFormat('EEEE', 'es').format(dateTime);
    } else {
      return DateFormat('d \'de\' MMMM', 'es').format(dateTime);
    }
  }

  void _showClearHistoryDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Borrar Historial'),
        content: const Text(
          '¿Estás seguro de que quieres borrar todo tu historial de vehículos vistos?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () {
              setState(() {
                _recentVehicles.clear();
                _groupedVehicles.clear();
              });
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Historial borrado')),
              );
            },
            style: FilledButton.styleFrom(
              backgroundColor: AppColors.error,
            ),
            child: const Text('Borrar'),
          ),
        ],
      ),
    );
  }

  void _exportHistory() {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Exportando historial...'),
        duration: Duration(seconds: 2),
      ),
    );
  }

  void _showPrivacySettings() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) => const PrivacySettingsSheet(),
    );
  }

  void _navigateToVehicleDetail(String vehicleId) {
    // Navigate to vehicle detail page
  }

  void _toggleFavorite(ViewedVehicle vehicle) {
    setState(() {
      vehicle.isFavorite = !vehicle.isFavorite;
    });
  }

  void _shareVehicle(ViewedVehicle vehicle) {
    // Share vehicle
  }
}

class PrivacySettingsSheet extends StatefulWidget {
  const PrivacySettingsSheet({super.key});

  @override
  State<PrivacySettingsSheet> createState() => _PrivacySettingsSheetState();
}

class _PrivacySettingsSheetState extends State<PrivacySettingsSheet> {
  bool _trackHistory = true;
  bool _showInProfile = false;
  bool _allowAnalytics = true;
  int _retentionDays = 30;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Configuración de Privacidad',
            style: AppTypography.h5.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: AppSpacing.lg),
          SwitchListTile(
            value: _trackHistory,
            onChanged: (value) => setState(() => _trackHistory = value),
            title: const Text('Rastrear historial'),
            subtitle: const Text('Guardar vehículos que visites'),
            activeThumbColor: AppColors.primary,
          ),
          SwitchListTile(
            value: _showInProfile,
            onChanged: (value) => setState(() => _showInProfile = value),
            title: const Text('Mostrar en perfil'),
            subtitle: const Text('Visible para otros usuarios'),
            activeThumbColor: AppColors.primary,
          ),
          SwitchListTile(
            value: _allowAnalytics,
            onChanged: (value) => setState(() => _allowAnalytics = value),
            title: const Text('Permitir análisis'),
            subtitle: const Text('Generar insights personalizados'),
            activeThumbColor: AppColors.primary,
          ),
          const SizedBox(height: AppSpacing.md),
          Text(
            'Retención de datos: $_retentionDays días',
            style: AppTypography.bodyMedium.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
          Slider(
            value: _retentionDays.toDouble(),
            min: 7,
            max: 90,
            divisions: 11,
            label: '$_retentionDays días',
            onChanged: (value) =>
                setState(() => _retentionDays = value.round()),
          ),
          const SizedBox(height: AppSpacing.lg),
          SizedBox(
            width: double.infinity,
            child: FilledButton(
              onPressed: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('Configuración guardada'),
                  ),
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
class ViewedVehicle {
  final String id;
  final String name;
  final String brand;
  final String model;
  final int year;
  final double price;
  final String imageUrl;
  final DateTime viewedAt;
  final Duration viewDuration;
  final int viewCount;
  bool isFavorite;

  ViewedVehicle({
    required this.id,
    required this.name,
    required this.brand,
    required this.model,
    required this.year,
    required this.price,
    required this.imageUrl,
    required this.viewedAt,
    required this.viewDuration,
    required this.viewCount,
    this.isFavorite = false,
  });
}

class ViewingAnalytics {
  final int totalViews;
  final int uniqueVehicles;
  final Duration averageViewDuration;
  final List<String> topBrands;
  final String topPriceRange;
  final String mostViewedTime;
  final int repeatViews;

  ViewingAnalytics({
    required this.totalViews,
    required this.uniqueVehicles,
    required this.averageViewDuration,
    required this.topBrands,
    required this.topPriceRange,
    required this.mostViewedTime,
    required this.repeatViews,
  });
}

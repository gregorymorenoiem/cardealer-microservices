// ignore_for_file: deprecated_member_use

import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/typography.dart';
import '../../../core/theme/spacing.dart';
import '../../widgets/buttons/gradient_button.dart';

/// Price Alerts Management System
/// Sprint 8 - SF-003: Price Alerts
class PriceAlertsPage extends StatefulWidget {
  const PriceAlertsPage({super.key});

  @override
  State<PriceAlertsPage> createState() => _PriceAlertsPageState();
}

class _PriceAlertsPageState extends State<PriceAlertsPage> {
  final List<PriceAlert> _alerts = [
    PriceAlert(
      id: '1',
      vehicleId: 'v1',
      vehicleName: 'BMW X5 M Sport',
      currentPrice: 45000,
      targetPrice: 42000,
      alertType: AlertType.priceDrop,
      isActive: true,
      createdAt: DateTime.now().subtract(const Duration(days: 5)),
    ),
    PriceAlert(
      id: '2',
      vehicleId: 'v2',
      vehicleName: 'Mercedes GLE 450',
      currentPrice: 52000,
      targetPrice: 50000,
      alertType: AlertType.priceDrop,
      isActive: true,
      createdAt: DateTime.now().subtract(const Duration(days: 3)),
    ),
    PriceAlert(
      id: '3',
      vehicleId: 'v3',
      vehicleName: 'Audi Q7 Premium',
      currentPrice: 48000,
      targetPrice: 47000,
      alertType: AlertType.anyChange,
      isActive: false,
      createdAt: DateTime.now().subtract(const Duration(days: 10)),
    ),
  ];

  @override
  Widget build(BuildContext context) {
    final activeAlerts = _alerts.where((a) => a.isActive).length;
    final inactiveAlerts = _alerts.where((a) => !a.isActive).length;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        title: Text(
          'Alertas de Precio',
          style: AppTypography.h3.copyWith(
            color: AppColors.textPrimary,
          ),
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.info_outline),
            onPressed: _showInfoDialog,
          ),
        ],
      ),
      body: CustomScrollView(
        slivers: [
          _buildStatsHeader(activeAlerts, inactiveAlerts),
          _buildAlertsList(),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: 'alerts_fab',
        onPressed: _createNewAlert,
        backgroundColor: AppColors.primary,
        icon: const Icon(Icons.add_alert, color: Colors.white),
        label: Text(
          'Nueva Alerta',
          style: AppTypography.button.copyWith(color: Colors.white),
        ),
      ),
    );
  }

  Widget _buildStatsHeader(int active, int inactive) {
    return SliverToBoxAdapter(
      child: Container(
        margin: const EdgeInsets.all(AppSpacing.lg),
        padding: const EdgeInsets.all(AppSpacing.lg),
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [
              AppColors.primary.withValues(alpha: 0.1),
              AppColors.accent.withValues(alpha: 0.1),
            ],
          ),
          borderRadius: BorderRadius.circular(16),
        ),
        child: Column(
          children: [
            Row(
              children: [
                Expanded(
                  child: _buildStatCard(
                    'Activas',
                    active.toString(),
                    Icons.notifications_active,
                    Colors.green,
                  ),
                ),
                const SizedBox(width: AppSpacing.md),
                Expanded(
                  child: _buildStatCard(
                    'Inactivas',
                    inactive.toString(),
                    Icons.notifications_off,
                    Colors.grey,
                  ),
                ),
              ],
            ),
            const SizedBox(height: AppSpacing.md),
            Container(
              padding: const EdgeInsets.all(AppSpacing.md),
              decoration: BoxDecoration(
                color: Colors.blue.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: Colors.blue.withValues(alpha: 0.3),
                ),
              ),
              child: Row(
                children: [
                  const Icon(
                    Icons.lightbulb_outline,
                    color: Colors.blue,
                    size: 20,
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  Expanded(
                    child: Text(
                      'Recibirás notificaciones cuando los precios cambien',
                      style: AppTypography.bodySmall.copyWith(
                        color: Colors.blue.shade700,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatCard(
      String label, String value, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.backgroundSecondary,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        children: [
          Icon(icon, color: color, size: 28),
          const SizedBox(height: AppSpacing.sm),
          Text(
            value,
            style: AppTypography.h2.copyWith(color: color),
          ),
          Text(
            label,
            style: AppTypography.caption.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAlertsList() {
    if (_alerts.isEmpty) {
      return SliverFillRemaining(
        child: _buildEmptyState(),
      );
    }

    return SliverPadding(
      padding: const EdgeInsets.all(AppSpacing.lg),
      sliver: SliverList(
        delegate: SliverChildBuilderDelegate(
          (context, index) => _buildAlertCard(_alerts[index]),
          childCount: _alerts.length,
        ),
      ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xl),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              padding: const EdgeInsets.all(AppSpacing.xl),
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  colors: [
                    AppColors.primary.withValues(alpha: 0.1),
                    AppColors.accent.withValues(alpha: 0.1),
                  ],
                ),
                shape: BoxShape.circle,
              ),
              child: const Icon(
                Icons.notifications_none,
                size: 80,
                color: AppColors.primary,
              ),
            ),
            const SizedBox(height: AppSpacing.xl),
            Text(
              'Sin Alertas Configuradas',
              style: AppTypography.h2.copyWith(
                color: AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: AppSpacing.md),
            Text(
              'Crea alertas de precio para recibir notificaciones\ncuando los vehículos que te interesan bajen de precio',
              style: AppTypography.bodyLarge.copyWith(
                color: AppColors.textSecondary,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildAlertCard(PriceAlert alert) {
    final priceDiff = alert.currentPrice - alert.targetPrice;
    final percentDiff = (priceDiff / alert.currentPrice * 100).abs();

    return Container(
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.backgroundSecondary,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: alert.isActive
              ? AppColors.primary.withValues(alpha: 0.3)
              : Colors.grey.withValues(alpha: 0.2),
        ),
      ),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Row(
              children: [
                // Vehicle Image
                Container(
                  width: 80,
                  height: 80,
                  decoration: BoxDecoration(
                    color: Colors.grey[300],
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(Icons.directions_car, size: 32),
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
                              alert.vehicleName,
                              style: AppTypography.bodyLarge.copyWith(
                                fontWeight: FontWeight.w600,
                              ),
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ),
                          Switch(
                            value: alert.isActive,
                            onChanged: (value) => _toggleAlert(alert.id, value),
                            activeTrackColor: AppColors.primary,
                          ),
                        ],
                      ),
                      const SizedBox(height: AppSpacing.xs),
                      Row(
                        children: [
                          _buildPriceTag(
                            'Actual',
                            '\$${alert.currentPrice.toStringAsFixed(0)}',
                            Colors.grey,
                          ),
                          const SizedBox(width: AppSpacing.sm),
                          const Icon(
                            Icons.arrow_forward,
                            size: 16,
                            color: AppColors.textSecondary,
                          ),
                          const SizedBox(width: AppSpacing.sm),
                          _buildPriceTag(
                            'Objetivo',
                            '\$${alert.targetPrice.toStringAsFixed(0)}',
                            Colors.green,
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),

          // Alert Info Bar
          Container(
            padding: const EdgeInsets.symmetric(
              horizontal: AppSpacing.md,
              vertical: AppSpacing.sm,
            ),
            decoration: BoxDecoration(
              color: alert.isActive
                  ? AppColors.primary.withValues(alpha: 0.05)
                  : Colors.grey.withValues(alpha: 0.05),
              borderRadius: const BorderRadius.vertical(
                bottom: Radius.circular(16),
              ),
            ),
            child: Row(
              children: [
                Icon(
                  _getAlertTypeIcon(alert.alertType),
                  size: 16,
                  color: AppColors.textSecondary,
                ),
                const SizedBox(width: AppSpacing.xs),
                Text(
                  _getAlertTypeLabel(alert.alertType),
                  style: AppTypography.caption.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
                const Spacer(),
                if (priceDiff > 0)
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.sm,
                      vertical: 2,
                    ),
                    decoration: BoxDecoration(
                      color: Colors.orange.withValues(alpha: 0.2),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Text(
                      'Falta ${percentDiff.toStringAsFixed(0)}%',
                      style: AppTypography.caption.copyWith(
                        color: Colors.orange.shade700,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                const SizedBox(width: AppSpacing.sm),
                PopupMenuButton<String>(
                  onSelected: (value) => _handleAlertAction(value, alert.id),
                  itemBuilder: (context) => [
                    const PopupMenuItem(value: 'edit', child: Text('Editar')),
                    const PopupMenuItem(
                        value: 'delete', child: Text('Eliminar')),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPriceTag(String label, String price, Color color) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: AppTypography.caption.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
        Text(
          price,
          style: AppTypography.bodyMedium.copyWith(
            color: color,
            fontWeight: FontWeight.bold,
          ),
        ),
      ],
    );
  }

  IconData _getAlertTypeIcon(AlertType type) {
    switch (type) {
      case AlertType.priceDrop:
        return Icons.trending_down;
      case AlertType.priceIncrease:
        return Icons.trending_up;
      case AlertType.anyChange:
        return Icons.swap_vert;
      case AlertType.targetPrice:
        return Icons.gps_fixed;
    }
  }

  String _getAlertTypeLabel(AlertType type) {
    switch (type) {
      case AlertType.priceDrop:
        return 'Alerta de reducción';
      case AlertType.priceIncrease:
        return 'Alerta de aumento';
      case AlertType.anyChange:
        return 'Cualquier cambio';
      case AlertType.targetPrice:
        return 'Precio objetivo';
    }
  }

  void _toggleAlert(String id, bool value) {
    setState(() {
      final alert = _alerts.firstWhere((a) => a.id == id);
      alert.isActive = value;
    });

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(value ? 'Alerta activada' : 'Alerta desactivada'),
      ),
    );
  }

  void _handleAlertAction(String action, String alertId) {
    switch (action) {
      case 'edit':
        _editAlert(alertId);
        break;
      case 'delete':
        _deleteAlert(alertId);
        break;
    }
  }

  void _editAlert(String id) {
    final alert = _alerts.firstWhere((a) => a.id == id);
    _showAlertDialog(alert: alert);
  }

  void _deleteAlert(String id) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Eliminar Alerta'),
        content: const Text('¿Estás seguro de eliminar esta alerta de precio?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () {
              setState(() {
                _alerts.removeWhere((a) => a.id == id);
              });
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Alerta eliminada')),
              );
            },
            child: const Text('Eliminar', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );
  }

  void _createNewAlert() {
    _showAlertDialog();
  }

  void _showAlertDialog({PriceAlert? alert}) {
    final isEdit = alert != null;
    final targetPriceController = TextEditingController(
      text: alert?.targetPrice.toString() ?? '',
    );
    AlertType selectedType = alert?.alertType ?? AlertType.priceDrop;

    showDialog(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: Text(isEdit ? 'Editar Alerta' : 'Nueva Alerta de Precio'),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                if (!isEdit) ...[
                  Text(
                    'Vehículo',
                    style: AppTypography.bodyMedium.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: AppSpacing.sm),
                  GestureDetector(
                    onTap: () {
                      // Select vehicle
                    },
                    child: Container(
                      padding: const EdgeInsets.all(AppSpacing.md),
                      decoration: BoxDecoration(
                        color: AppColors.backgroundSecondary,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: Colors.grey.shade300),
                      ),
                      child: const Row(
                        children: [
                          Icon(Icons.directions_car, color: AppColors.primary),
                          SizedBox(width: AppSpacing.sm),
                          Text('Seleccionar vehículo'),
                          Spacer(),
                          Icon(Icons.arrow_forward_ios, size: 16),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: AppSpacing.lg),
                ],
                Text(
                  'Tipo de Alerta',
                  style: AppTypography.bodyMedium.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: AppSpacing.sm),
                RadioGroup<AlertType>(
                  groupValue: selectedType,
                  onChanged: (value) {
                    setDialogState(() => selectedType = value!);
                  },
                  child: Column(
                    children: AlertType.values.map((type) {
                      return ListTile(
                        leading: Radio<AlertType>(
                          value: type,
                        ),
                        title: Text(_getAlertTypeLabel(type)),
                        dense: true,
                        onTap: () {
                          setDialogState(() => selectedType = type);
                        },
                      );
                    }).toList(),
                  ),
                ),
                const SizedBox(height: AppSpacing.lg),
                TextField(
                  controller: targetPriceController,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    labelText: 'Precio Objetivo',
                    prefixText: '\$ ',
                    hintText: '40000',
                  ),
                ),
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Cancelar'),
            ),
            GradientButton(
              text: isEdit ? 'Guardar' : 'Crear',
              onPressed: () {
                // Save alert
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content:
                        Text(isEdit ? 'Alerta actualizada' : 'Alerta creada'),
                  ),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  void _showInfoDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Alertas de Precio'),
        content: SingleChildScrollView(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text(
                'Las alertas de precio te notifican cuando:',
                style: AppTypography.bodyMedium,
              ),
              const SizedBox(height: AppSpacing.md),
              _buildInfoItem(
                Icons.trending_down,
                'Reducción de Precio',
                'El precio baja del valor objetivo',
              ),
              _buildInfoItem(
                Icons.trending_up,
                'Aumento de Precio',
                'El precio sube del valor objetivo',
              ),
              _buildInfoItem(
                Icons.swap_vert,
                'Cualquier Cambio',
                'El precio cambia en cualquier dirección',
              ),
              _buildInfoItem(
                Icons.gps_fixed,
                'Precio Objetivo',
                'El precio alcanza el valor exacto',
              ),
            ],
          ),
        ),
        actions: [
          GradientButton(
            text: 'Entendido',
            onPressed: () => Navigator.pop(context),
          ),
        ],
      ),
    );
  }

  Widget _buildInfoItem(IconData icon, String title, String description) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, color: AppColors.primary, size: 20),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: AppTypography.bodyMedium.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
                ),
                Text(
                  description,
                  style: AppTypography.bodySmall.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

enum AlertType {
  priceDrop,
  priceIncrease,
  anyChange,
  targetPrice,
}

class PriceAlert {
  final String id;
  final String vehicleId;
  final String vehicleName;
  final double currentPrice;
  final double targetPrice;
  final AlertType alertType;
  bool isActive;
  final DateTime createdAt;

  PriceAlert({
    required this.id,
    required this.vehicleId,
    required this.vehicleName,
    required this.currentPrice,
    required this.targetPrice,
    required this.alertType,
    required this.isActive,
    required this.createdAt,
  });
}

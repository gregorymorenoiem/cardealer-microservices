import 'package:flutter/material.dart';

/// DP-008: Quick Actions Widget
/// Widget de acciones rápidas para gestión de publicaciones
class QuickActionsWidget extends StatelessWidget {
  final String vehicleId;
  final String vehicleTitle;
  final String currentStatus;
  final Function(String action)? onActionSelected;

  const QuickActionsWidget({
    super.key,
    required this.vehicleId,
    required this.vehicleTitle,
    required this.currentStatus,
    this.onActionSelected,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      margin: const EdgeInsets.all(16),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Acciones Rápidas',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                _QuickActionButton(
                  icon: Icons.check_circle,
                  label: 'Marcar Vendido',
                  color: Colors.green,
                  onPressed: () => _handleAction(context, 'mark_sold'),
                  enabled: currentStatus != 'sold',
                ),
                _QuickActionButton(
                  icon: Icons.attach_money,
                  label: 'Ajustar Precio',
                  color: Colors.orange,
                  onPressed: () => _showPriceAdjustmentDialog(context),
                ),
                _QuickActionButton(
                  icon: Icons.trending_up,
                  label: 'Promover',
                  color: Colors.blue,
                  onPressed: () => _showBoostDialog(context),
                  enabled: currentStatus == 'active',
                ),
                _QuickActionButton(
                  icon: Icons.refresh,
                  label: 'Renovar',
                  color: Colors.purple,
                  onPressed: () => _handleAction(context, 'renew'),
                  enabled: currentStatus == 'active',
                ),
                _QuickActionButton(
                  icon: Icons.pause,
                  label: currentStatus == 'active' ? 'Pausar' : 'Activar',
                  color: currentStatus == 'active' ? Colors.grey : Colors.green,
                  onPressed: () => _handleAction(
                    context,
                    currentStatus == 'active' ? 'pause' : 'activate',
                  ),
                  enabled: currentStatus != 'sold',
                ),
                _QuickActionButton(
                  icon: Icons.edit,
                  label: 'Editar',
                  color: theme.colorScheme.primary,
                  onPressed: () => _handleAction(context, 'edit'),
                ),
                _QuickActionButton(
                  icon: Icons.share,
                  label: 'Compartir',
                  color: theme.colorScheme.tertiary,
                  onPressed: () => _handleAction(context, 'share'),
                ),
                _QuickActionButton(
                  icon: Icons.delete,
                  label: 'Eliminar',
                  color: Colors.red,
                  onPressed: () => _showDeleteConfirmation(context),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  void _handleAction(BuildContext context, String action) {
    if (onActionSelected != null) {
      onActionSelected!(action);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Acción: $action para $vehicleTitle')),
      );
    }
  }

  void _showPriceAdjustmentDialog(BuildContext context) {
    final theme = Theme.of(context);
    final TextEditingController priceController = TextEditingController();

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Ajustar Precio'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              vehicleTitle,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(height: 16),
            TextField(
              controller: priceController,
              keyboardType: TextInputType.number,
              decoration: const InputDecoration(
                labelText: 'Nuevo precio',
                prefixText: '\$',
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 16),
            Text(
              'Sugerencias de ajuste:',
              style: theme.textTheme.titleSmall?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            _PriceAdjustmentChip(
              label: '-5%',
              onTap: () {
                priceController.text = '95% del precio actual';
              },
            ),
            const SizedBox(height: 4),
            _PriceAdjustmentChip(
              label: '-10%',
              onTap: () {
                priceController.text = '90% del precio actual';
              },
            ),
            const SizedBox(height: 4),
            _PriceAdjustmentChip(
              label: '+5%',
              onTap: () {
                priceController.text = '105% del precio actual';
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
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Precio actualizado')),
              );
              if (onActionSelected != null) {
                onActionSelected!('adjust_price');
              }
            },
            child: const Text('Guardar'),
          ),
        ],
      ),
    );
  }

  void _showBoostDialog(BuildContext context) {
    final theme = Theme.of(context);

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Promover Publicación'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              vehicleTitle,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(height: 16),
            Text(
              'Selecciona el tipo de promoción:',
              style: theme.textTheme.titleSmall?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 12),
            const _BoostOption(
              title: 'Destacado 24h',
              description: 'Tu anuncio aparecerá en la parte superior',
              price: '\$9.99',
              recommended: false,
            ),
            const SizedBox(height: 8),
            const _BoostOption(
              title: 'Destacado 7 días',
              description: 'Máxima visibilidad durante una semana',
              price: '\$49.99',
              recommended: true,
            ),
            const SizedBox(height: 8),
            const _BoostOption(
              title: 'Destacado 30 días',
              description: 'Promoción extendida con mejores resultados',
              price: '\$149.99',
              recommended: false,
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
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Publicación promovida')),
              );
              if (onActionSelected != null) {
                onActionSelected!('boost');
              }
            },
            child: const Text('Promover'),
          ),
        ],
      ),
    );
  }

  void _showDeleteConfirmation(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Eliminar Publicación'),
        content: Text(
            '¿Estás seguro de eliminar "$vehicleTitle"? Esta acción no se puede deshacer.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () {
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Publicación eliminada')),
              );
              if (onActionSelected != null) {
                onActionSelected!('delete');
              }
            },
            style: FilledButton.styleFrom(
              backgroundColor: Colors.red,
            ),
            child: const Text('Eliminar'),
          ),
        ],
      ),
    );
  }
}

class _QuickActionButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onPressed;
  final bool enabled;

  const _QuickActionButton({
    required this.icon,
    required this.label,
    required this.color,
    required this.onPressed,
    this.enabled = true,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return OutlinedButton.icon(
      onPressed: enabled ? onPressed : null,
      icon: Icon(icon, size: 18),
      label: Text(label),
      style: OutlinedButton.styleFrom(
        foregroundColor:
            enabled ? color : theme.colorScheme.onSurface.withValues(alpha: 0.38),
        side: BorderSide(
          color: enabled
              ? color.withValues(alpha: 0.5)
              : theme.colorScheme.outline.withValues(alpha: 0.38),
        ),
      ),
    );
  }
}

class _PriceAdjustmentChip extends StatelessWidget {
  final String label;
  final VoidCallback onTap;

  const _PriceAdjustmentChip({
    required this.label,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        decoration: BoxDecoration(
          border: Border.all(color: Theme.of(context).colorScheme.outline),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(label),
            const Icon(Icons.arrow_forward, size: 16),
          ],
        ),
      ),
    );
  }
}

class _BoostOption extends StatelessWidget {
  final String title;
  final String description;
  final String price;
  final bool recommended;

  const _BoostOption({
    required this.title,
    required this.description,
    required this.price,
    this.recommended = false,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        border: Border.all(
          color: recommended
              ? theme.colorScheme.primary
              : theme.colorScheme.outline,
          width: recommended ? 2 : 1,
        ),
        borderRadius: BorderRadius.circular(8),
        color: recommended
            ? theme.colorScheme.primaryContainer.withValues(alpha: 0.1)
            : null,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                title,
                style: theme.textTheme.titleSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              if (recommended)
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                  decoration: BoxDecoration(
                    color: theme.colorScheme.primary,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    'RECOMENDADO',
                    style: theme.textTheme.labelSmall?.copyWith(
                      color: theme.colorScheme.onPrimary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
            ],
          ),
          const SizedBox(height: 4),
          Text(
            description,
            style: theme.textTheme.bodySmall?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            price,
            style: theme.textTheme.titleLarge?.copyWith(
              color: theme.colorScheme.primary,
              fontWeight: FontWeight.bold,
            ),
          ),
        ],
      ),
    );
  }
}

/// Floating Quick Actions Button
/// Botón flotante con menú de acciones rápidas
class QuickActionsFAB extends StatelessWidget {
  final String vehicleId;
  final String vehicleTitle;
  final Function(String action)? onActionSelected;

  const QuickActionsFAB({
    super.key,
    required this.vehicleId,
    required this.vehicleTitle,
    this.onActionSelected,
  });

  @override
  Widget build(BuildContext context) {
    return FloatingActionButton(
      onPressed: () => _showQuickActionsMenu(context),
      child: const Icon(Icons.flash_on),
    );
  }

  void _showQuickActionsMenu(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Padding(
              padding: const EdgeInsets.all(16),
              child: Text(
                'Acciones Rápidas',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
              ),
            ),
            ListTile(
              leading: const Icon(Icons.check_circle, color: Colors.green),
              title: const Text('Marcar como vendido'),
              onTap: () {
                Navigator.pop(context);
                _handleAction(context, 'mark_sold');
              },
            ),
            ListTile(
              leading: const Icon(Icons.attach_money, color: Colors.orange),
              title: const Text('Ajustar precio'),
              onTap: () {
                Navigator.pop(context);
                _handleAction(context, 'adjust_price');
              },
            ),
            ListTile(
              leading: const Icon(Icons.trending_up, color: Colors.blue),
              title: const Text('Promover publicación'),
              onTap: () {
                Navigator.pop(context);
                _handleAction(context, 'boost');
              },
            ),
            ListTile(
              leading: const Icon(Icons.refresh, color: Colors.purple),
              title: const Text('Renovar publicación'),
              onTap: () {
                Navigator.pop(context);
                _handleAction(context, 'renew');
              },
            ),
          ],
        ),
      ),
    );
  }

  void _handleAction(BuildContext context, String action) {
    if (onActionSelected != null) {
      onActionSelected!(action);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Acción: $action para $vehicleTitle')),
      );
    }
  }
}

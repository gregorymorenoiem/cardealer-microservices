import 'package:flutter/material.dart';

/// DP-003: Listings Management
/// Gestión de todas las publicaciones del dealer
class ListingsManagementPage extends StatefulWidget {
  const ListingsManagementPage({super.key});

  @override
  State<ListingsManagementPage> createState() => _ListingsManagementPageState();
}

class _ListingsManagementPageState extends State<ListingsManagementPage> {
  String _viewMode = 'list'; // 'list' or 'grid'
  String _statusFilter = 'all';
  final Set<String> _selectedListings = {};
  bool _isSelectionMode = false;

  final List<Map<String, dynamic>> _listings = [
    {
      'id': '1',
      'title': 'Toyota Camry 2024',
      'price': '\$28,000',
      'status': 'active',
      'views': 1245,
      'leads': 23,
      'daysActive': 5,
      'image': 'https://via.placeholder.com/400x300',
    },
    {
      'id': '2',
      'title': 'Honda Civic 2023',
      'price': '\$22,500',
      'status': 'active',
      'views': 892,
      'leads': 15,
      'daysActive': 12,
      'image': 'https://via.placeholder.com/400x300',
    },
    {
      'id': '3',
      'title': 'Ford Escape 2024',
      'price': '\$32,000',
      'status': 'pending',
      'views': 145,
      'leads': 3,
      'daysActive': 2,
      'image': 'https://via.placeholder.com/400x300',
    },
    {
      'id': '4',
      'title': 'BMW X5 2024',
      'price': '\$65,000',
      'status': 'sold',
      'views': 2341,
      'leads': 45,
      'daysActive': 18,
      'image': 'https://via.placeholder.com/400x300',
    },
    {
      'id': '5',
      'title': 'Mazda CX-5 2023',
      'price': '\$28,500',
      'status': 'inactive',
      'views': 543,
      'leads': 8,
      'daysActive': 30,
      'image': 'https://via.placeholder.com/400x300',
    },
  ];

  List<Map<String, dynamic>> get _filteredListings {
    if (_statusFilter == 'all') return _listings;
    return _listings.where((l) => l['status'] == _statusFilter).toList();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: _isSelectionMode
            ? Text('${_selectedListings.length} seleccionados')
            : const Text('Mis Publicaciones'),
        leading: _isSelectionMode
            ? IconButton(
                icon: const Icon(Icons.close),
                onPressed: () {
                  setState(() {
                    _isSelectionMode = false;
                    _selectedListings.clear();
                  });
                },
              )
            : null,
        actions: [
          if (!_isSelectionMode) ...[
            IconButton(
              icon: Icon(_viewMode == 'list' ? Icons.grid_view : Icons.list),
              onPressed: () {
                setState(() {
                  _viewMode = _viewMode == 'list' ? 'grid' : 'list';
                });
              },
            ),
            IconButton(
              icon: const Icon(Icons.filter_list),
              onPressed: () => _showFilterSheet(context),
            ),
            IconButton(
              icon: const Icon(Icons.search),
              onPressed: () {
                // TODO: Implement search
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Buscar publicaciones')),
                );
              },
            ),
          ] else ...[
            IconButton(
              icon: const Icon(Icons.more_vert),
              onPressed: () => _showBulkActionsSheet(context),
            ),
          ],
        ],
      ),
      body: Column(
        children: [
          // Status filter chips
          _buildStatusFilters(theme),

          // Statistics bar
          _buildStatisticsBar(theme),

          // Listings
          Expanded(
            child: _filteredListings.isEmpty
                ? _buildEmptyState(theme)
                : _viewMode == 'list'
                    ? _buildListView()
                    : _buildGridView(),
          ),
        ],
      ),
      floatingActionButton: _isSelectionMode
          ? null
          : FloatingActionButton.extended(
              heroTag: 'listings_fab',
              onPressed: () {
                Navigator.pushNamed(context, '/dealer/publish-vehicle');
              },
              icon: const Icon(Icons.add),
              label: const Text('Nueva publicación'),
            ),
    );
  }

  Widget _buildStatusFilters(ThemeData theme) {
    final filters = {
      'all': 'Todos',
      'active': 'Activos',
      'pending': 'Pendientes',
      'sold': 'Vendidos',
      'inactive': 'Inactivos',
    };

    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      padding: const EdgeInsets.all(16),
      child: Row(
        children: filters.entries.map((entry) {
          final count = entry.key == 'all'
              ? _listings.length
              : _listings.where((l) => l['status'] == entry.key).length;

          return Padding(
            padding: const EdgeInsets.only(right: 8),
            child: FilterChip(
              label: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(entry.value),
                  const SizedBox(width: 8),
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                    decoration: BoxDecoration(
                      color: _statusFilter == entry.key
                          ? theme.colorScheme.onPrimary
                          : theme.colorScheme.primary,
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Text(
                      count.toString(),
                      style: theme.textTheme.labelSmall?.copyWith(
                        color: _statusFilter == entry.key
                            ? theme.colorScheme.primary
                            : theme.colorScheme.onPrimary,
                      ),
                    ),
                  ),
                ],
              ),
              selected: _statusFilter == entry.key,
              onSelected: (_) {
                setState(() => _statusFilter = entry.key);
              },
            ),
          );
        }).toList(),
      ),
    );
  }

  Widget _buildStatisticsBar(ThemeData theme) {
    final activeListings =
        _listings.where((l) => l['status'] == 'active').length;
    final totalViews =
        _listings.fold<int>(0, (sum, l) => sum + (l['views'] as int));
    final totalLeads =
        _listings.fold<int>(0, (sum, l) => sum + (l['leads'] as int));

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: theme.colorScheme.surfaceContainerHighest,
        border: Border(
          top: BorderSide(color: theme.colorScheme.outlineVariant),
          bottom: BorderSide(color: theme.colorScheme.outlineVariant),
        ),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: [
          _StatItem(
            icon: Icons.check_circle,
            label: 'Activos',
            value: activeListings.toString(),
            color: Colors.green,
          ),
          _StatItem(
            icon: Icons.visibility,
            label: 'Vistas',
            value: _formatNumber(totalViews),
            color: theme.colorScheme.primary,
          ),
          _StatItem(
            icon: Icons.people,
            label: 'Leads',
            value: totalLeads.toString(),
            color: theme.colorScheme.tertiary,
          ),
        ],
      ),
    );
  }

  Widget _buildListView() {
    return ListView.builder(
      itemCount: _filteredListings.length,
      itemBuilder: (context, index) {
        final listing = _filteredListings[index];
        final isSelected = _selectedListings.contains(listing['id']);

        return _ListingListTile(
          listing: listing,
          isSelected: isSelected,
          isSelectionMode: _isSelectionMode,
          onTap: () {
            if (_isSelectionMode) {
              setState(() {
                if (isSelected) {
                  _selectedListings.remove(listing['id']);
                } else {
                  _selectedListings.add(listing['id'] as String);
                }
              });
            } else {
              // TODO: Navigate to listing details
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(content: Text('Ver ${listing['title']}')),
              );
            }
          },
          onLongPress: () {
            if (!_isSelectionMode) {
              setState(() {
                _isSelectionMode = true;
                _selectedListings.add(listing['id'] as String);
              });
            }
          },
          onActionPressed: () => _showListingActions(context, listing),
        );
      },
    );
  }

  Widget _buildGridView() {
    return GridView.builder(
      padding: const EdgeInsets.all(16),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        childAspectRatio: 0.75,
        crossAxisSpacing: 12,
        mainAxisSpacing: 12,
      ),
      itemCount: _filteredListings.length,
      itemBuilder: (context, index) {
        final listing = _filteredListings[index];
        final isSelected = _selectedListings.contains(listing['id']);

        return _ListingGridTile(
          listing: listing,
          isSelected: isSelected,
          isSelectionMode: _isSelectionMode,
          onTap: () {
            if (_isSelectionMode) {
              setState(() {
                if (isSelected) {
                  _selectedListings.remove(listing['id']);
                } else {
                  _selectedListings.add(listing['id'] as String);
                }
              });
            } else {
              // TODO: Navigate to listing details
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(content: Text('Ver ${listing['title']}')),
              );
            }
          },
          onLongPress: () {
            if (!_isSelectionMode) {
              setState(() {
                _isSelectionMode = true;
                _selectedListings.add(listing['id'] as String);
              });
            }
          },
        );
      },
    );
  }

  Widget _buildEmptyState(ThemeData theme) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.inventory_2_outlined,
              size: 80,
              color: theme.colorScheme.outline,
            ),
            const SizedBox(height: 16),
            Text(
              'No hay publicaciones',
              style: theme.textTheme.titleLarge,
            ),
            const SizedBox(height: 8),
            Text(
              _statusFilter == 'all'
                  ? 'Aún no has publicado ningún vehículo'
                  : 'No hay publicaciones con este estado',
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 24),
            if (_statusFilter == 'all')
              FilledButton.icon(
                onPressed: () {
                  Navigator.pushNamed(context, '/dealer/publish-vehicle');
                },
                icon: const Icon(Icons.add),
                label: const Text('Publicar vehículo'),
              ),
          ],
        ),
      ),
    );
  }

  void _showFilterSheet(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.sort),
              title: const Text('Ordenar por fecha'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Ordenado por fecha')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.visibility),
              title: const Text('Ordenar por vistas'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Ordenado por vistas')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.attach_money),
              title: const Text('Ordenar por precio'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Ordenado por precio')),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  void _showBulkActionsSheet(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.check_circle),
              title: const Text('Activar seleccionados'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text(
                        '${_selectedListings.length} publicaciones activadas'),
                  ),
                );
                setState(() {
                  _isSelectionMode = false;
                  _selectedListings.clear();
                });
              },
            ),
            ListTile(
              leading: const Icon(Icons.pause_circle),
              title: const Text('Desactivar seleccionados'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text(
                        '${_selectedListings.length} publicaciones desactivadas'),
                  ),
                );
                setState(() {
                  _isSelectionMode = false;
                  _selectedListings.clear();
                });
              },
            ),
            ListTile(
              leading: const Icon(Icons.delete, color: Colors.red),
              title: const Text('Eliminar seleccionados',
                  style: TextStyle(color: Colors.red)),
              onTap: () {
                Navigator.pop(context);
                _showDeleteConfirmation(context);
              },
            ),
          ],
        ),
      ),
    );
  }

  void _showListingActions(BuildContext context, Map<String, dynamic> listing) {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.edit),
              title: const Text('Editar'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text('Editar ${listing['title']}')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.trending_up),
              title: const Text('Promover'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text('Promover ${listing['title']}')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.check_circle),
              title: const Text('Marcar como vendido'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                      content:
                          Text('${listing['title']} marcado como vendido')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.delete, color: Colors.red),
              title:
                  const Text('Eliminar', style: TextStyle(color: Colors.red)),
              onTap: () {
                Navigator.pop(context);
                _showDeleteConfirmation(context);
              },
            ),
          ],
        ),
      ),
    );
  }

  void _showDeleteConfirmation(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Eliminar publicaciones'),
        content: Text(
          _isSelectionMode
              ? '¿Eliminar ${_selectedListings.length} publicaciones?'
              : '¿Eliminar esta publicación?',
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
                const SnackBar(content: Text('Publicaciones eliminadas')),
              );
              if (_isSelectionMode) {
                setState(() {
                  _isSelectionMode = false;
                  _selectedListings.clear();
                });
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

  String _formatNumber(int number) {
    if (number >= 1000) {
      return '${(number / 1000).toStringAsFixed(1)}K';
    }
    return number.toString();
  }
}

class _StatItem extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  final Color color;

  const _StatItem({
    required this.icon,
    required this.label,
    required this.value,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      children: [
        Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 16, color: color),
            const SizedBox(width: 4),
            Text(
              value,
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
                color: color,
              ),
            ),
          ],
        ),
        Text(
          label,
          style: theme.textTheme.bodySmall?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
          ),
        ),
      ],
    );
  }
}

class _ListingListTile extends StatelessWidget {
  final Map<String, dynamic> listing;
  final bool isSelected;
  final bool isSelectionMode;
  final VoidCallback onTap;
  final VoidCallback onLongPress;
  final VoidCallback onActionPressed;

  const _ListingListTile({
    required this.listing,
    required this.isSelected,
    required this.isSelectionMode,
    required this.onTap,
    required this.onLongPress,
    required this.onActionPressed,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final status = listing['status'] as String;

    return InkWell(
      onTap: onTap,
      onLongPress: onLongPress,
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: isSelected
              ? theme.colorScheme.primaryContainer.withValues(alpha: 0.3)
              : null,
          border: Border(
            bottom: BorderSide(
              color: theme.colorScheme.outlineVariant,
            ),
          ),
        ),
        child: Row(
          children: [
            if (isSelectionMode)
              Checkbox(
                value: isSelected,
                onChanged: (_) => onTap(),
              )
            else
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: Image.network(
                  listing['image'],
                  width: 80,
                  height: 60,
                  fit: BoxFit.cover,
                ),
              ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          listing['title'],
                          style: theme.textTheme.titleSmall?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      _StatusBadge(status: status),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    listing['price'],
                    style: theme.textTheme.titleMedium?.copyWith(
                      color: theme.colorScheme.primary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Icon(Icons.visibility,
                          size: 14, color: theme.colorScheme.onSurfaceVariant),
                      const SizedBox(width: 4),
                      Text('${listing['views']}',
                          style: theme.textTheme.bodySmall),
                      const SizedBox(width: 16),
                      Icon(Icons.people,
                          size: 14, color: theme.colorScheme.onSurfaceVariant),
                      const SizedBox(width: 4),
                      Text('${listing['leads']}',
                          style: theme.textTheme.bodySmall),
                      const SizedBox(width: 16),
                      Icon(Icons.access_time,
                          size: 14, color: theme.colorScheme.onSurfaceVariant),
                      const SizedBox(width: 4),
                      Text('${listing['daysActive']}d',
                          style: theme.textTheme.bodySmall),
                    ],
                  ),
                ],
              ),
            ),
            if (!isSelectionMode)
              IconButton(
                icon: const Icon(Icons.more_vert),
                onPressed: onActionPressed,
              ),
          ],
        ),
      ),
    );
  }
}

class _ListingGridTile extends StatelessWidget {
  final Map<String, dynamic> listing;
  final bool isSelected;
  final bool isSelectionMode;
  final VoidCallback onTap;
  final VoidCallback onLongPress;

  const _ListingGridTile({
    required this.listing,
    required this.isSelected,
    required this.isSelectionMode,
    required this.onTap,
    required this.onLongPress,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final status = listing['status'] as String;

    return InkWell(
      onTap: onTap,
      onLongPress: onLongPress,
      child: Card(
        elevation: isSelected ? 4 : 1,
        color: isSelected
            ? theme.colorScheme.primaryContainer.withValues(alpha: 0.3)
            : null,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Stack(
              children: [
                ClipRRect(
                  borderRadius:
                      const BorderRadius.vertical(top: Radius.circular(12)),
                  child: Image.network(
                    listing['image'],
                    width: double.infinity,
                    height: 120,
                    fit: BoxFit.cover,
                  ),
                ),
                if (isSelectionMode)
                  Positioned(
                    top: 8,
                    right: 8,
                    child: Checkbox(
                      value: isSelected,
                      onChanged: (_) => onTap(),
                    ),
                  )
                else
                  Positioned(
                    top: 8,
                    right: 8,
                    child: _StatusBadge(status: status),
                  ),
              ],
            ),
            Padding(
              padding: const EdgeInsets.all(12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    listing['title'],
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    listing['price'],
                    style: theme.textTheme.titleMedium?.copyWith(
                      color: theme.colorScheme.primary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Row(
                        children: [
                          Icon(Icons.visibility,
                              size: 14,
                              color: theme.colorScheme.onSurfaceVariant),
                          const SizedBox(width: 4),
                          Text('${listing['views']}',
                              style: theme.textTheme.bodySmall),
                        ],
                      ),
                      Row(
                        children: [
                          Icon(Icons.people,
                              size: 14,
                              color: theme.colorScheme.onSurfaceVariant),
                          const SizedBox(width: 4),
                          Text('${listing['leads']}',
                              style: theme.textTheme.bodySmall),
                        ],
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _StatusBadge extends StatelessWidget {
  final String status;

  const _StatusBadge({required this.status});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final config = _getStatusConfig(status);

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: config['color'],
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        config['label'],
        style: theme.textTheme.labelSmall?.copyWith(
          color: Colors.white,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }

  Map<String, dynamic> _getStatusConfig(String status) {
    switch (status) {
      case 'active':
        return {'label': 'Activo', 'color': Colors.green};
      case 'pending':
        return {'label': 'Pendiente', 'color': Colors.orange};
      case 'sold':
        return {'label': 'Vendido', 'color': Colors.blue};
      case 'inactive':
        return {'label': 'Inactivo', 'color': Colors.grey};
      default:
        return {'label': status, 'color': Colors.grey};
    }
  }
}

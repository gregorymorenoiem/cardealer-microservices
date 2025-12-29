import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/typography.dart';
import '../../../core/theme/spacing.dart';
import '../../widgets/buttons/gradient_button.dart';

/// Premium Favorites Page with Collections and Grid View
/// Sprint 8 - SF-001: Favorites Page Redesign
class FavoritesPagePremium extends StatefulWidget {
  const FavoritesPagePremium({super.key});

  @override
  State<FavoritesPagePremium> createState() => _FavoritesPagePremiumState();
}

class _FavoritesPagePremiumState extends State<FavoritesPagePremium>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  // ignore: unused_field
  final String _selectedCollection = 'all';
  String _viewMode = 'grid'; // grid or list
  bool _selectionMode = false;
  final Set<String> _selectedItems = {};

  final List<FavoriteCollection> _collections = [
    FavoriteCollection(
        id: 'all', name: 'Todos', count: 24, color: AppColors.primary),
    FavoriteCollection(id: 'suv', name: 'SUVs', count: 8, color: Colors.blue),
    FavoriteCollection(
        id: 'sedan', name: 'Sedanes', count: 10, color: Colors.green),
    FavoriteCollection(
        id: 'luxury', name: 'Lujo', count: 6, color: Colors.purple),
  ];

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: _collections.length, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: CustomScrollView(
        slivers: [
          _buildAppBar(),
          _buildCollectionTabs(),
          if (_selectionMode) _buildBulkActions(),
          _buildContent(),
        ],
      ),
      floatingActionButton: _buildFAB(),
    );
  }

  Widget _buildAppBar() {
    return SliverAppBar(
      expandedHeight: 120,
      floating: false,
      pinned: true,
      backgroundColor: AppColors.background,
      elevation: 0,
      flexibleSpace: FlexibleSpaceBar(
        title: Text(
          'Mis Favoritos',
          style: AppTypography.h2.copyWith(
            color: AppColors.textPrimary,
          ),
        ),
        centerTitle: false,
        titlePadding:
            const EdgeInsets.only(left: AppSpacing.lg, bottom: AppSpacing.md),
      ),
      actions: [
        IconButton(
          icon: Icon(
            _viewMode == 'grid' ? Icons.view_list : Icons.grid_view,
            color: AppColors.textPrimary,
          ),
          onPressed: () {
            setState(() {
              _viewMode = _viewMode == 'grid' ? 'list' : 'grid';
            });
          },
        ),
        IconButton(
          icon: Icon(
            _selectionMode ? Icons.close : Icons.select_all,
            color: AppColors.textPrimary,
          ),
          onPressed: () {
            setState(() {
              _selectionMode = !_selectionMode;
              if (!_selectionMode) {
                _selectedItems.clear();
              }
            });
          },
        ),
        PopupMenuButton<String>(
          icon: const Icon(Icons.more_vert),
          onSelected: _handleMenuAction,
          itemBuilder: (context) => [
            const PopupMenuItem(value: 'sort', child: Text('Ordenar')),
            const PopupMenuItem(value: 'filter', child: Text('Filtrar')),
            const PopupMenuItem(value: 'export', child: Text('Exportar')),
          ],
        ),
      ],
    );
  }

  Widget _buildCollectionTabs() {
    return SliverToBoxAdapter(
      child: Container(
        height: 50,
        margin: const EdgeInsets.symmetric(vertical: AppSpacing.md),
        child: TabBar(
          controller: _tabController,
          isScrollable: true,
          labelColor: AppColors.textPrimary,
          unselectedLabelColor: AppColors.textSecondary,
          indicatorColor: AppColors.primary,
          indicatorWeight: 3,
          tabs: _collections.map((collection) {
            return Tab(
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Container(
                    width: 8,
                    height: 8,
                    decoration: BoxDecoration(
                      color: collection.color,
                      shape: BoxShape.circle,
                    ),
                  ),
                  const SizedBox(width: AppSpacing.xs),
                  Text(collection.name),
                  const SizedBox(width: AppSpacing.xs),
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 6,
                      vertical: 2,
                    ),
                    decoration: BoxDecoration(
                      color: collection.color.withValues(alpha: 0.2),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Text(
                      '${collection.count}',
                      style: AppTypography.caption.copyWith(
                        color: collection.color,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
              ),
            );
          }).toList(),
        ),
      ),
    );
  }

  Widget _buildBulkActions() {
    return SliverToBoxAdapter(
      child: Container(
        padding: const EdgeInsets.all(AppSpacing.md),
        margin: const EdgeInsets.symmetric(horizontal: AppSpacing.lg),
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [
              AppColors.primary.withValues(alpha: 0.1),
              AppColors.accent.withValues(alpha: 0.1),
            ],
          ),
          borderRadius: BorderRadius.circular(12),
        ),
        child: Row(
          children: [
            Text(
              '${_selectedItems.length} seleccionados',
              style: AppTypography.bodyMedium.copyWith(
                fontWeight: FontWeight.w600,
              ),
            ),
            const Spacer(),
            IconButton(
              icon: const Icon(Icons.folder_outlined, size: 20),
              onPressed: () => _showMoveToCollectionSheet(),
              tooltip: 'Mover a colección',
            ),
            IconButton(
              icon: const Icon(Icons.share, size: 20),
              onPressed: () => _shareSelected(),
              tooltip: 'Compartir',
            ),
            IconButton(
              icon:
                  const Icon(Icons.delete_outline, size: 20, color: Colors.red),
              onPressed: () => _deleteSelected(),
              tooltip: 'Eliminar',
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildContent() {
    return SliverPadding(
      padding: const EdgeInsets.all(AppSpacing.lg),
      sliver: _viewMode == 'grid' ? _buildGridView() : _buildListView(),
    );
  }

  Widget _buildGridView() {
    return SliverGrid(
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        childAspectRatio: 0.75,
        crossAxisSpacing: AppSpacing.md,
        mainAxisSpacing: AppSpacing.md,
      ),
      delegate: SliverChildBuilderDelegate(
        (context, index) => _buildGridCard(index),
        childCount: 24,
      ),
    );
  }

  Widget _buildListView() {
    return SliverList(
      delegate: SliverChildBuilderDelegate(
        (context, index) => _buildListCard(index),
        childCount: 24,
      ),
    );
  }

  Widget _buildGridCard(int index) {
    final isSelected = _selectedItems.contains('vehicle_$index');

    return GestureDetector(
      onTap: () => _handleCardTap(index),
      onLongPress: () => _enableSelectionMode(index),
      child: Stack(
        children: [
          Container(
            decoration: BoxDecoration(
              color: AppColors.backgroundSecondary,
              borderRadius: BorderRadius.circular(16),
              border: Border.all(
                color: isSelected ? AppColors.primary : Colors.transparent,
                width: 2,
              ),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.05),
                  blurRadius: 8,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Image
                Expanded(
                  flex: 3,
                  child: Stack(
                    children: [
                      ClipRRect(
                        borderRadius: const BorderRadius.vertical(
                          top: Radius.circular(16),
                        ),
                        child: Container(
                          color: Colors.grey[300],
                          width: double.infinity,
                          child: const Icon(Icons.directions_car, size: 40),
                        ),
                      ),
                      // Price Tag
                      Positioned(
                        top: AppSpacing.sm,
                        left: AppSpacing.sm,
                        child: Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: AppSpacing.sm,
                            vertical: 4,
                          ),
                          decoration: BoxDecoration(
                            gradient: const LinearGradient(
                              colors: [AppColors.primary, AppColors.accent],
                            ),
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: Text(
                            '\$45,000',
                            style: AppTypography.bodySmall.copyWith(
                              color: Colors.white,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                      ),
                      // Favorite Icon
                      if (!_selectionMode)
                        Positioned(
                          top: AppSpacing.sm,
                          right: AppSpacing.sm,
                          child: GestureDetector(
                            onTap: () => _removeFavorite(index),
                            child: Container(
                              padding: const EdgeInsets.all(6),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                shape: BoxShape.circle,
                                boxShadow: [
                                  BoxShadow(
                                    color: Colors.black.withValues(alpha: 0.1),
                                    blurRadius: 4,
                                  ),
                                ],
                              ),
                              child: const Icon(
                                Icons.favorite,
                                color: Colors.red,
                                size: 18,
                              ),
                            ),
                          ),
                        ),
                    ],
                  ),
                ),
                // Info
                Expanded(
                  flex: 2,
                  child: Padding(
                    padding: const EdgeInsets.all(AppSpacing.sm),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'BMW X5 M Sport',
                              style: AppTypography.bodyMedium.copyWith(
                                fontWeight: FontWeight.w600,
                              ),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                            const SizedBox(height: 2),
                            Text(
                              '2023 • 15,000 km',
                              style: AppTypography.bodySmall.copyWith(
                                color: AppColors.textSecondary,
                              ),
                            ),
                          ],
                        ),
                        Row(
                          children: [
                            const Icon(
                              Icons.location_on,
                              size: 12,
                              color: AppColors.textSecondary,
                            ),
                            const SizedBox(width: 2),
                            Expanded(
                              child: Text(
                                'Miami, FL',
                                style: AppTypography.caption.copyWith(
                                  color: AppColors.textSecondary,
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
                ),
              ],
            ),
          ),
          // Selection Checkbox
          if (_selectionMode)
            Positioned(
              top: AppSpacing.sm,
              right: AppSpacing.sm,
              child: Container(
                padding: const EdgeInsets.all(2),
                decoration: BoxDecoration(
                  color: Colors.white,
                  shape: BoxShape.circle,
                  border: Border.all(
                    color: isSelected ? AppColors.primary : Colors.grey,
                    width: 2,
                  ),
                ),
                child: Icon(
                  isSelected ? Icons.check : null,
                  size: 16,
                  color: AppColors.primary,
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildListCard(int index) {
    final isSelected = _selectedItems.contains('vehicle_$index');

    return GestureDetector(
      onTap: () => _handleCardTap(index),
      onLongPress: () => _enableSelectionMode(index),
      child: Container(
        margin: const EdgeInsets.only(bottom: AppSpacing.md),
        decoration: BoxDecoration(
          color: AppColors.backgroundSecondary,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isSelected ? AppColors.primary : Colors.transparent,
            width: 2,
          ),
        ),
        child: Row(
          children: [
            // Image
            ClipRRect(
              borderRadius:
                  const BorderRadius.horizontal(left: Radius.circular(16)),
              child: Container(
                width: 120,
                height: 100,
                color: Colors.grey[300],
                child: const Icon(Icons.directions_car, size: 40),
              ),
            ),
            // Info
            Expanded(
              child: Padding(
                padding: const EdgeInsets.all(AppSpacing.md),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            'BMW X5 M Sport',
                            style: AppTypography.bodyLarge.copyWith(
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                        if (_selectionMode)
                          Icon(
                            isSelected
                                ? Icons.check_circle
                                : Icons.circle_outlined,
                            color: isSelected ? AppColors.primary : Colors.grey,
                          )
                        else
                          IconButton(
                            icon: const Icon(Icons.favorite, color: Colors.red),
                            onPressed: () => _removeFavorite(index),
                          ),
                      ],
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Text(
                      '2023 • 15,000 km',
                      style: AppTypography.bodyMedium.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          '\$45,000',
                          style: AppTypography.h4.copyWith(
                            color: AppColors.primary,
                          ),
                        ),
                        Row(
                          children: [
                            const Icon(
                              Icons.location_on,
                              size: 14,
                              color: AppColors.textSecondary,
                            ),
                            Text(
                              'Miami, FL',
                              style: AppTypography.bodySmall.copyWith(
                                color: AppColors.textSecondary,
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildFAB() {
    return FloatingActionButton.extended(
      heroTag: 'favorites_fab',
      onPressed: _showCreateCollectionDialog,
      backgroundColor: AppColors.primary,
      icon: const Icon(Icons.create_new_folder, color: Colors.white),
      label: Text(
        'Nueva Colección',
        style: AppTypography.button.copyWith(color: Colors.white),
      ),
    );
  }

  void _handleCardTap(int index) {
    if (_selectionMode) {
      setState(() {
        final id = 'vehicle_$index';
        if (_selectedItems.contains(id)) {
          _selectedItems.remove(id);
        } else {
          _selectedItems.add(id);
        }
      });
    } else {
      // Navigate to detail
    }
  }

  void _enableSelectionMode(int index) {
    setState(() {
      _selectionMode = true;
      _selectedItems.add('vehicle_$index');
    });
  }

  void _removeFavorite(int index) {
    // Remove from favorites
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Eliminado de favoritos')),
    );
  }

  void _handleMenuAction(String action) {
    switch (action) {
      case 'sort':
        _showSortSheet();
        break;
      case 'filter':
        _showFilterSheet();
        break;
      case 'export':
        _exportFavorites();
        break;
    }
  }

  void _showMoveToCollectionSheet() {
    showModalBottomSheet(
      context: context,
      builder: (context) => Container(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text(
              'Mover a Colección',
              style: AppTypography.h3,
            ),
            const SizedBox(height: AppSpacing.lg),
            ..._collections.where((c) => c.id != 'all').map((collection) {
              return ListTile(
                leading: Container(
                  width: 12,
                  height: 12,
                  decoration: BoxDecoration(
                    color: collection.color,
                    shape: BoxShape.circle,
                  ),
                ),
                title: Text(collection.name),
                trailing: Text('${collection.count} items'),
                onTap: () {
                  Navigator.pop(context);
                  _moveToCollection(collection.id);
                },
              );
            }),
          ],
        ),
      ),
    );
  }

  void _moveToCollection(String collectionId) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('${_selectedItems.length} vehículos movidos'),
      ),
    );
    setState(() {
      _selectedItems.clear();
      _selectionMode = false;
    });
  }

  void _shareSelected() {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Compartiendo ${_selectedItems.length} vehículos'),
      ),
    );
  }

  void _deleteSelected() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Eliminar Favoritos'),
        content: Text(
          '¿Eliminar ${_selectedItems.length} vehículos de favoritos?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              setState(() {
                _selectedItems.clear();
                _selectionMode = false;
              });
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Favoritos eliminados')),
              );
            },
            child: const Text('Eliminar', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );
  }

  void _showSortSheet() {
    showModalBottomSheet(
      context: context,
      builder: (context) => Container(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('Ordenar por', style: AppTypography.h3),
            const SizedBox(height: AppSpacing.lg),
            ListTile(
              leading: const Icon(Icons.calendar_today),
              title: const Text('Fecha agregado'),
              onTap: () => Navigator.pop(context),
            ),
            ListTile(
              leading: const Icon(Icons.attach_money),
              title: const Text('Precio: Menor a Mayor'),
              onTap: () => Navigator.pop(context),
            ),
            ListTile(
              leading: const Icon(Icons.attach_money),
              title: const Text('Precio: Mayor a Menor'),
              onTap: () => Navigator.pop(context),
            ),
            ListTile(
              leading: const Icon(Icons.speed),
              title: const Text('Kilometraje'),
              onTap: () => Navigator.pop(context),
            ),
          ],
        ),
      ),
    );
  }

  void _showFilterSheet() {
    // Show filter options
  }

  void _exportFavorites() {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Exportando favoritos...')),
    );
  }

  void _showCreateCollectionDialog() {
    final nameController = TextEditingController();
    // ignore: unused_local_variable
    Color selectedColor = AppColors.primary;

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Nueva Colección'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
              controller: nameController,
              decoration: const InputDecoration(
                labelText: 'Nombre',
                hintText: 'Ej: Mis SUVs favoritos',
              ),
            ),
            const SizedBox(height: AppSpacing.lg),
            Wrap(
              spacing: AppSpacing.sm,
              children: [
                Colors.blue,
                Colors.green,
                Colors.purple,
                Colors.orange,
                Colors.red,
                Colors.teal,
              ].map((color) {
                return GestureDetector(
                  onTap: () => selectedColor = color,
                  child: Container(
                    width: 40,
                    height: 40,
                    decoration: BoxDecoration(
                      color: color,
                      shape: BoxShape.circle,
                    ),
                  ),
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
          GradientButton(
            text: 'Crear',
            onPressed: () {
              if (nameController.text.isNotEmpty) {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text('Colección "${nameController.text}" creada'),
                  ),
                );
              }
            },
          ),
        ],
      ),
    );
  }
}

class FavoriteCollection {
  final String id;
  final String name;
  final int count;
  final Color color;

  FavoriteCollection({
    required this.id,
    required this.name,
    required this.count,
    required this.color,
  });
}

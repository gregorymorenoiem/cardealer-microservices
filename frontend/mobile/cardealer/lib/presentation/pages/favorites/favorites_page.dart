import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../../core/responsive/responsive_utils.dart';
import '../../bloc/favorites/favorites_bloc.dart';
import '../../bloc/favorites/favorites_event.dart';
import '../../bloc/favorites/favorites_state.dart';
import '../../widgets/empty_state_widget.dart';
import 'widgets/favorites_grid.dart';

/// Favorites page displaying user's saved vehicles
class FavoritesPage extends StatelessWidget {
  const FavoritesPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt<FavoritesBloc>()..add(LoadFavorites()),
      child: const _FavoritesPageContent(),
    );
  }
}

class _FavoritesPageContent extends StatelessWidget {
  const _FavoritesPageContent();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Favoritos'),
        actions: [
          BlocBuilder<FavoritesBloc, FavoritesState>(
            builder: (context, state) {
              if (state is FavoritesLoaded && state.favorites.isNotEmpty) {
                return PopupMenuButton<String>(
                  onSelected: (value) {
                    if (value == 'clear_all') {
                      _showClearAllDialog(context);
                    }
                  },
                  itemBuilder: (context) => [
                    const PopupMenuItem(
                      value: 'clear_all',
                      child: Row(
                        children: [
                          Icon(Icons.delete_outline, color: Colors.red),
                          SizedBox(width: 8),
                          Text('Limpiar Todo'),
                        ],
                      ),
                    ),
                  ],
                );
              }
              return const SizedBox.shrink();
            },
          ),
        ],
      ),
      body: BlocConsumer<FavoritesBloc, FavoritesState>(
        listener: (context, state) {
          if (state is FavoritesError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
              ),
            );
          } else if (state is FavoriteRemoved) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('Vehículo eliminado de favoritos'),
              ),
            );
          }
        },
        builder: (context, state) {
          if (state is FavoritesLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          if (state is FavoritesEmpty) {
            return EmptyStateWidget(
              icon: Icons.favorite_outline,
              title: 'Sin Favoritos',
              message: 'Aún no has guardado ningún vehículo en favoritos',
              actionText: 'Explorar Vehículos',
              onAction: () {
                Navigator.of(context).pop();
              },
            );
          }

          if (state is FavoritesError) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.error_outline, size: 64, color: Colors.red),
                  const SizedBox(height: 16),
                  Text(state.message),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () {
                      context.read<FavoritesBloc>().add(LoadFavorites());
                    },
                    child: const Text('Reintentar'),
                  ),
                ],
              ),
            );
          }

          if (state is FavoritesLoaded || state is FavoriteRemoving) {
            final favorites = state is FavoritesLoaded
                ? state.favorites
                : (state as FavoriteRemoving).currentFavorites;

            return RefreshIndicator(
              onRefresh: () async {
                context.read<FavoritesBloc>().add(RefreshFavorites());
                await Future.delayed(const Duration(seconds: 1));
              },
              child: FavoritesGrid(
                vehicles: favorites,
                onRemove: (vehicleId) {
                  context
                      .read<FavoritesBloc>()
                      .add(RemoveFavoriteEvent(vehicleId));
                },
              ),
            );
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }

  void _showClearAllDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Limpiar Favoritos'),
        content: const Text(
          '¿Estás seguro de que deseas eliminar todos los vehículos de favoritos?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(dialogContext).pop();
              context.read<FavoritesBloc>().add(ClearAllFavorites());
            },
            child: const Text(
              'Limpiar Todo',
              style: TextStyle(color: Colors.red),
            ),
          ),
        ],
      ),
    );
  }
}

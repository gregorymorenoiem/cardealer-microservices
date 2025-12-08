import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../bloc/profile/profile_bloc.dart';
import '../../bloc/profile/profile_event.dart';
import '../../bloc/profile/profile_state.dart';
import '../../widgets/loading_indicator.dart';
import 'widgets/profile_header.dart';
import 'widgets/profile_menu_item.dart';
import 'widgets/edit_profile_dialog.dart';

/// Profile page displaying user information and settings
class ProfilePage extends StatelessWidget {
  const ProfilePage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt<ProfileBloc>()..add(LoadProfile()),
      child: const _ProfilePageContent(),
    );
  }
}

class _ProfilePageContent extends StatelessWidget {
  const _ProfilePageContent({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Perfil'),
        actions: [
          IconButton(
            icon: const Icon(Icons.edit_outlined),
            onPressed: () => _showEditProfileDialog(context),
            tooltip: 'Editar perfil',
          ),
        ],
      ),
      body: BlocConsumer<ProfileBloc, ProfileState>(
        listener: (context, state) {
          if (state is ProfileError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
              ),
            );
          } else if (state is ProfileUpdated) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('Perfil actualizado correctamente'),
                backgroundColor: Colors.green,
              ),
            );
            // Reload profile
            context.read<ProfileBloc>().add(RefreshProfile());
          } else if (state is ProfileDeleted) {
            Navigator.of(context).pushReplacementNamed('/login');
          }
        },
        builder: (context, state) {
          if (state is ProfileLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          if (state is ProfileError) {
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
                      context.read<ProfileBloc>().add(LoadProfile());
                    },
                    child: const Text('Reintentar'),
                  ),
                ],
              ),
            );
          }

          if (state is ProfileLoaded ||
              state is ProfileUpdating ||
              state is ProfileAvatarUploading) {
            final user = state is ProfileLoaded
                ? state.user
                : state is ProfileUpdating
                    ? state.currentUser
                    : (state as ProfileAvatarUploading).currentUser;

            final isUpdating =
                state is ProfileUpdating || state is ProfileAvatarUploading;

            return Stack(
              children: [
                SingleChildScrollView(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    children: [
                      // Profile Header
                      ProfileHeader(
                        user: user,
                        onAvatarTap: () => _showAvatarPicker(context),
                      ),
                      const SizedBox(height: 32),

                      // Personal Information Section
                      _buildSectionTitle('Información Personal'),
                      const SizedBox(height: 8),
                      ProfileMenuItem(
                        icon: Icons.person_outline,
                        title: 'Nombre',
                        subtitle: '${user.firstName} ${user.lastName}',
                        onTap: () => _showEditProfileDialog(context),
                      ),
                      ProfileMenuItem(
                        icon: Icons.email_outlined,
                        title: 'Email',
                        subtitle: user.email,
                        onTap: () {}, // Email no editable
                      ),
                      ProfileMenuItem(
                        icon: Icons.phone_outlined,
                        title: 'Teléfono',
                        subtitle: user.phoneNumber ?? 'No especificado',
                        onTap: () => _showEditProfileDialog(context),
                      ),
                      const SizedBox(height: 24),

                      // Account Settings Section
                      _buildSectionTitle('Configuración de Cuenta'),
                      const SizedBox(height: 8),
                      ProfileMenuItem(
                        icon: Icons.notifications_outlined,
                        title: 'Notificaciones',
                        subtitle: 'Gestionar preferencias',
                        onTap: () {
                          // TODO: Navigate to notifications settings
                        },
                      ),
                      ProfileMenuItem(
                        icon: Icons.security_outlined,
                        title: 'Seguridad',
                        subtitle: 'Cambiar contraseña',
                        onTap: () {
                          // TODO: Navigate to security settings
                        },
                      ),
                      ProfileMenuItem(
                        icon: Icons.language_outlined,
                        title: 'Idioma',
                        subtitle: 'Español',
                        onTap: () {
                          // TODO: Navigate to language settings
                        },
                      ),
                      const SizedBox(height: 24),

                      // Preferences Section
                      _buildSectionTitle('Preferencias'),
                      const SizedBox(height: 8),
                      ProfileMenuItem(
                        icon: Icons.favorite_outline,
                        title: 'Favoritos',
                        subtitle: 'Ver vehículos guardados',
                        onTap: () {
                          Navigator.of(context).pushNamed('/favorites');
                        },
                      ),
                      ProfileMenuItem(
                        icon: Icons.history_outlined,
                        title: 'Historial de Búsquedas',
                        subtitle: 'Ver búsquedas recientes',
                        onTap: () {
                          // TODO: Navigate to search history
                        },
                      ),
                      const SizedBox(height: 24),

                      // Logout Button
                      SizedBox(
                        width: double.infinity,
                        child: OutlinedButton.icon(
                          icon: const Icon(Icons.logout),
                          label: const Text('Cerrar Sesión'),
                          style: OutlinedButton.styleFrom(
                            foregroundColor: Colors.red,
                            side: const BorderSide(color: Colors.red),
                            padding: const EdgeInsets.all(16),
                          ),
                          onPressed: () => _showLogoutDialog(context),
                        ),
                      ),
                      const SizedBox(height: 16),

                      // Delete Account Button
                      TextButton(
                        onPressed: () => _showDeleteAccountDialog(context),
                        child: const Text(
                          'Eliminar Cuenta',
                          style: TextStyle(color: Colors.red),
                        ),
                      ),
                      const SizedBox(height: 32),
                    ],
                  ),
                ),
                if (isUpdating)
                  Container(
                    color: Colors.black26,
                    child: const Center(
                      child: Card(
                        child: Padding(
                          padding: EdgeInsets.all(24),
                          child: Column(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              CircularProgressIndicator(),
                              SizedBox(height: 16),
                              Text('Actualizando perfil...'),
                            ],
                          ),
                        ),
                      ),
                    ),
                  ),
              ],
            );
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }

  Widget _buildSectionTitle(String title) {
    return Align(
      alignment: Alignment.centerLeft,
      child: Text(
        title,
        style: const TextStyle(
          fontSize: 18,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }

  void _showEditProfileDialog(BuildContext context) {
    final bloc = context.read<ProfileBloc>();
    final state = bloc.state;

    if (state is ProfileLoaded) {
      showDialog(
        context: context,
        builder: (_) => BlocProvider.value(
          value: bloc,
          child: EditProfileDialog(user: state.user),
        ),
      );
    }
  }

  void _showAvatarPicker(BuildContext context) {
    // TODO: Implement image picker
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Selección de avatar en desarrollo')),
    );
  }

  void _showLogoutDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Cerrar Sesión'),
        content: const Text('¿Estás seguro de que deseas cerrar sesión?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              // TODO: Call logout use case
              Navigator.of(context).pushReplacementNamed('/login');
            },
            child: const Text(
              'Cerrar Sesión',
              style: TextStyle(color: Colors.red),
            ),
          ),
        ],
      ),
    );
  }

  void _showDeleteAccountDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Eliminar Cuenta'),
        content: const Text(
          '¿Estás seguro de que deseas eliminar tu cuenta? Esta acción no se puede deshacer.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              context.read<ProfileBloc>().add(DeleteAccount());
            },
            child: const Text(
              'Eliminar',
              style: TextStyle(color: Colors.red),
            ),
          ),
        ],
      ),
    );
  }
}

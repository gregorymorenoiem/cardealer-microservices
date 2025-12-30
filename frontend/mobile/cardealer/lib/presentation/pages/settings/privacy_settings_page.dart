import 'package:flutter/material.dart';

/// PE-004: Privacy Settings (Sprint 11)
/// Control de privacidad y visibilidad del perfil
class PrivacySettingsPage extends StatefulWidget {
  const PrivacySettingsPage({super.key});

  @override
  State<PrivacySettingsPage> createState() => _PrivacySettingsPageState();
}

class _PrivacySettingsPageState extends State<PrivacySettingsPage> {
  // Profile visibility
  String _profileVisibility = 'public';

  // Activity privacy
  bool _showFavorites = true;
  bool _showViewHistory = false;
  bool _showSearchHistory = false;
  bool _showActivityFeed = true;

  // Location sharing
  bool _shareLocation = true;
  bool _preciseLocation = false;

  // Contact privacy
  bool _showPhoneNumber = false;
  bool _showEmail = true;

  // Data controls
  final List<String> _blockedUsers = ['user123', 'dealer456'];

  bool _isSaving = false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Privacidad'),
        actions: [
          if (_isSaving)
            const Padding(
              padding: EdgeInsets.all(16),
              child: SizedBox(
                width: 24,
                height: 24,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
            ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Security banner
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.blue.shade50,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.blue.shade200),
              ),
              child: Row(
                children: [
                  Icon(Icons.shield, color: Colors.blue.shade700),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Text(
                      'Controla quién puede ver tu información y actividad',
                      style: TextStyle(
                        color: Colors.blue.shade900,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),

            // Profile Visibility Section
            const _SectionHeader(
              icon: Icons.visibility,
              title: 'Visibilidad del Perfil',
              subtitle: 'Controla quién puede ver tu perfil',
            ),
            const SizedBox(height: 16),

            Card(
              child: RadioGroup<String>(
                groupValue: _profileVisibility,
                onChanged: (value) {
                  setState(() {
                    _profileVisibility = value!;
                  });
                },
                child: Column(
                  children: [
                    ListTile(
                      leading: const Radio<String>(
                        value: 'public',
                      ),
                      title: const Text('Público'),
                      subtitle: const Text('Cualquiera puede ver tu perfil'),
                      trailing: const Icon(Icons.public),
                      onTap: () {
                        setState(() {
                          _profileVisibility = 'public';
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'contacts',
                      ),
                      title: const Text('Solo Contactos'),
                      subtitle: const Text(
                          'Solo personas con las que has interactuado'),
                      trailing: const Icon(Icons.people),
                      onTap: () {
                        setState(() {
                          _profileVisibility = 'contacts';
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'private',
                      ),
                      title: const Text('Privado'),
                      subtitle: const Text('Solo tú puedes ver tu perfil'),
                      trailing: const Icon(Icons.lock),
                      onTap: () {
                        setState(() {
                          _profileVisibility = 'private';
                        });
                      },
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Activity Privacy Section
            const _SectionHeader(
              icon: Icons.history,
              title: 'Privacidad de Actividad',
              subtitle: 'Controla qué actividades son visibles',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  SwitchListTile(
                    value: _showFavorites,
                    onChanged: (value) {
                      setState(() {
                        _showFavorites = value;
                      });
                    },
                    title: const Text('Mostrar Favoritos'),
                    subtitle:
                        const Text('Otros pueden ver tus vehículos favoritos'),
                    secondary: const Icon(Icons.favorite),
                  ),
                  const Divider(height: 1),
                  SwitchListTile(
                    value: _showViewHistory,
                    onChanged: (value) {
                      setState(() {
                        _showViewHistory = value;
                      });
                    },
                    title: const Text('Historial de Visitas'),
                    subtitle: const Text('Mostrar vehículos que has visto'),
                    secondary: const Icon(Icons.remove_red_eye),
                  ),
                  const Divider(height: 1),
                  SwitchListTile(
                    value: _showSearchHistory,
                    onChanged: (value) {
                      setState(() {
                        _showSearchHistory = value;
                      });
                    },
                    title: const Text('Historial de Búsquedas'),
                    subtitle: const Text('Mostrar tus búsquedas recientes'),
                    secondary: const Icon(Icons.search),
                  ),
                  const Divider(height: 1),
                  SwitchListTile(
                    value: _showActivityFeed,
                    onChanged: (value) {
                      setState(() {
                        _showActivityFeed = value;
                      });
                    },
                    title: const Text('Feed de Actividad'),
                    subtitle: const Text('Mostrar tu actividad reciente'),
                    secondary: const Icon(Icons.feed),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Location Privacy Section
            const _SectionHeader(
              icon: Icons.location_on,
              title: 'Ubicación',
              subtitle: 'Controla el uso de tu ubicación',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  SwitchListTile(
                    value: _shareLocation,
                    onChanged: (value) {
                      setState(() {
                        _shareLocation = value;
                        if (!value) {
                          _preciseLocation = false;
                        }
                      });
                    },
                    title: const Text('Compartir Ubicación'),
                    subtitle: const Text('Permite búsquedas cercanas'),
                    secondary: const Icon(Icons.my_location),
                  ),
                  if (_shareLocation) ...[
                    const Divider(height: 1),
                    SwitchListTile(
                      value: _preciseLocation,
                      onChanged: (value) {
                        setState(() {
                          _preciseLocation = value;
                        });
                      },
                      title: const Text('Ubicación Precisa'),
                      subtitle: const Text('Compartir ubicación exacta'),
                      secondary: const Icon(Icons.gps_fixed),
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Icon(Icons.info_outline),
                      title: const Text('Información'),
                      subtitle: Text(
                        _preciseLocation
                            ? 'Tu ubicación exacta será compartida con vendedores'
                            : 'Solo se compartirá tu ciudad aproximada',
                        style: TextStyle(
                            fontSize: 12, color: Colors.grey.shade600),
                      ),
                    ),
                  ],
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Contact Information Section
            const _SectionHeader(
              icon: Icons.contact_page,
              title: 'Información de Contacto',
              subtitle: 'Controla qué información es visible',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  SwitchListTile(
                    value: _showEmail,
                    onChanged: (value) {
                      setState(() {
                        _showEmail = value;
                      });
                    },
                    title: const Text('Mostrar Email'),
                    subtitle: const Text('Vendedores pueden ver tu email'),
                    secondary: const Icon(Icons.email),
                  ),
                  const Divider(height: 1),
                  SwitchListTile(
                    value: _showPhoneNumber,
                    onChanged: (value) {
                      setState(() {
                        _showPhoneNumber = value;
                      });
                    },
                    title: const Text('Mostrar Teléfono'),
                    subtitle: const Text('Vendedores pueden ver tu teléfono'),
                    secondary: const Icon(Icons.phone),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Blocked Users Section
            const _SectionHeader(
              icon: Icons.block,
              title: 'Usuarios Bloqueados',
              subtitle: 'Gestiona usuarios que has bloqueado',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  ListTile(
                    leading: const Icon(Icons.block),
                    title: Text('${_blockedUsers.length} usuarios bloqueados'),
                    trailing: const Icon(Icons.chevron_right),
                    onTap: _showBlockedUsers,
                  ),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Data Controls Section
            const _SectionHeader(
              icon: Icons.storage,
              title: 'Control de Datos',
              subtitle: 'Gestiona tu información personal',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  ListTile(
                    leading: const Icon(Icons.download),
                    title: const Text('Descargar Mis Datos'),
                    subtitle: const Text('Exporta toda tu información'),
                    trailing: const Icon(Icons.chevron_right),
                    onTap: _downloadData,
                  ),
                  const Divider(height: 1),
                  ListTile(
                    leading:
                        const Icon(Icons.delete_forever, color: Colors.red),
                    title: const Text(
                      'Eliminar Mi Cuenta',
                      style: TextStyle(color: Colors.red),
                    ),
                    subtitle: const Text('Eliminar permanentemente tu cuenta'),
                    trailing:
                        const Icon(Icons.chevron_right, color: Colors.red),
                    onTap: _showDeleteAccountDialog,
                  ),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Save Button
            SizedBox(
              width: double.infinity,
              child: FilledButton.icon(
                onPressed: _isSaving ? null : _saveSettings,
                icon: const Icon(Icons.save),
                label: _isSaving
                    ? const Text('Guardando...')
                    : const Text('Guardar Configuración'),
              ),
            ),
            const SizedBox(height: 16),

            // Privacy Policy Link
            Center(
              child: TextButton.icon(
                onPressed: () {
                  // Navigate to privacy policy
                },
                icon: const Icon(Icons.policy, size: 18),
                label: const Text('Ver Política de Privacidad'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _saveSettings() async {
    setState(() {
      _isSaving = true;
    });

    // Simulate API call
    await Future.delayed(const Duration(seconds: 2));

    if (mounted) {
      setState(() {
        _isSaving = false;
      });

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Configuración de privacidad guardada'),
          backgroundColor: Colors.green,
        ),
      );
    }
  }

  void _showBlockedUsers() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.6,
        minChildSize: 0.4,
        maxChildSize: 0.9,
        expand: false,
        builder: (context, scrollController) {
          return Column(
            children: [
              Padding(
                padding: const EdgeInsets.all(16),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Usuarios Bloqueados',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    IconButton(
                      icon: const Icon(Icons.close),
                      onPressed: () => Navigator.pop(context),
                    ),
                  ],
                ),
              ),
              Expanded(
                child: ListView.separated(
                  controller: scrollController,
                  itemCount: _blockedUsers.length,
                  separatorBuilder: (context, index) =>
                      const Divider(height: 1),
                  itemBuilder: (context, index) {
                    final user = _blockedUsers[index];
                    return ListTile(
                      leading: CircleAvatar(
                        child: Text(user[0].toUpperCase()),
                      ),
                      title: Text(user),
                      subtitle: const Text('Usuario bloqueado'),
                      trailing: TextButton(
                        onPressed: () {
                          setState(() {
                            _blockedUsers.removeAt(index);
                          });
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(content: Text('$user desbloqueado')),
                          );
                        },
                        child: const Text('Desbloquear'),
                      ),
                    );
                  },
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  void _downloadData() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Descargar Mis Datos'),
        content: const Text(
          'Se generará un archivo con toda tu información personal, '
          'publicaciones, mensajes y actividad. Este proceso puede tardar unos minutos. '
          'Te enviaremos un email cuando esté listo.',
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
                const SnackBar(
                  content: Text('Generando archivo... Te avisaremos por email'),
                  duration: Duration(seconds: 3),
                ),
              );
            },
            child: const Text('Generar'),
          ),
        ],
      ),
    );
  }

  void _showDeleteAccountDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Row(
          children: [
            Icon(Icons.warning, color: Colors.red),
            SizedBox(width: 8),
            Text('Eliminar Cuenta'),
          ],
        ),
        content: const Text(
          '¿Estás seguro de que deseas eliminar tu cuenta?\n\n'
          'Esta acción es PERMANENTE y no se puede deshacer.\n\n'
          'Se eliminarán:\n'
          '• Tu perfil y toda tu información\n'
          '• Tus publicaciones de vehículos\n'
          '• Tus favoritos y búsquedas\n'
          '• Tu historial de mensajes',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () {
              Navigator.pop(context);
              _showConfirmDeleteDialog();
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

  void _showConfirmDeleteDialog() {
    final controller = TextEditingController();

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Confirmar Eliminación'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Escribe "ELIMINAR" para confirmar:'),
            const SizedBox(height: 16),
            TextField(
              controller: controller,
              decoration: const InputDecoration(
                border: OutlineInputBorder(),
                hintText: 'ELIMINAR',
              ),
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
              if (controller.text == 'ELIMINAR') {
                Navigator.pop(context);
                // TODO: Call delete account API
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('Cuenta eliminada'),
                    backgroundColor: Colors.red,
                  ),
                );
              } else {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content:
                        Text('Por favor escribe "ELIMINAR" para confirmar'),
                  ),
                );
              }
            },
            style: FilledButton.styleFrom(
              backgroundColor: Colors.red,
            ),
            child: const Text('Confirmar'),
          ),
        ],
      ),
    );
  }
}

/// Section header widget
class _SectionHeader extends StatelessWidget {
  final IconData icon;
  final String title;
  final String subtitle;

  const _SectionHeader({
    required this.icon,
    required this.title,
    required this.subtitle,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: Theme.of(context).colorScheme.primaryContainer,
            borderRadius: BorderRadius.circular(8),
          ),
          child: Icon(icon, color: Theme.of(context).colorScheme.primary),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                subtitle,
                style: TextStyle(
                  fontSize: 13,
                  color: Colors.grey.shade600,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

import 'package:flutter/material.dart';

/// PE-003: Notification Preferences (Sprint 11)
/// Control granular de notificaciones por canal y categoría
class NotificationPreferencesPage extends StatefulWidget {
  const NotificationPreferencesPage({super.key});

  @override
  State<NotificationPreferencesPage> createState() =>
      _NotificationPreferencesPageState();
}

class _NotificationPreferencesPageState
    extends State<NotificationPreferencesPage> {
  // Channel toggles
  bool _emailNotifications = true;
  bool _pushNotifications = true;
  bool _smsNotifications = false;

  // Category preferences
  final Map<String, bool> _categoryPreferences = {
    'messages': true,
    'offers': true,
    'updates': true,
    'marketing': false,
    'security': true,
    'activity': true,
  };

  // Frequency
  String _notificationFrequency = 'instant';

  // Quiet hours
  bool _quietHoursEnabled = false;
  TimeOfDay _quietHoursStart = const TimeOfDay(hour: 22, minute: 0);
  TimeOfDay _quietHoursEnd = const TimeOfDay(hour: 8, minute: 0);

  bool _isSaving = false;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Notificaciones'),
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
            // Info banner
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: theme.colorScheme.primaryContainer.withAlpha(50),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: theme.colorScheme.primary.withAlpha(50),
                ),
              ),
              child: Row(
                children: [
                  Icon(
                    Icons.info_outline,
                    color: theme.colorScheme.primary,
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Text(
                      'Personaliza cómo y cuándo recibes notificaciones',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: theme.colorScheme.onSurface,
                      ),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),

            // Channels Section
            const _SectionHeader(
              icon: Icons.notifications_active,
              title: 'Canales de Notificación',
              subtitle: 'Activa o desactiva canales',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  SwitchListTile(
                    value: _pushNotifications,
                    onChanged: (value) {
                      setState(() {
                        _pushNotifications = value;
                      });
                    },
                    title: const Text('Notificaciones Push'),
                    subtitle: const Text('Notificaciones en tu dispositivo'),
                    secondary: const Icon(Icons.notifications),
                  ),
                  const Divider(height: 1),
                  SwitchListTile(
                    value: _emailNotifications,
                    onChanged: (value) {
                      setState(() {
                        _emailNotifications = value;
                      });
                    },
                    title: const Text('Email'),
                    subtitle: const Text('Notificaciones por correo'),
                    secondary: const Icon(Icons.email),
                  ),
                  const Divider(height: 1),
                  SwitchListTile(
                    value: _smsNotifications,
                    onChanged: (value) {
                      setState(() {
                        _smsNotifications = value;
                      });
                    },
                    title: const Text('SMS'),
                    subtitle: const Text('Mensajes de texto'),
                    secondary: const Icon(Icons.sms),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Categories Section
            const _SectionHeader(
              icon: Icons.category,
              title: 'Categorías',
              subtitle: 'Elige qué tipos de notificaciones recibir',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  _CategoryTile(
                    icon: Icons.message,
                    title: 'Mensajes',
                    subtitle: 'Nuevos mensajes de compradores y vendedores',
                    value: _categoryPreferences['messages']!,
                    onChanged: (value) {
                      setState(() {
                        _categoryPreferences['messages'] = value;
                      });
                    },
                  ),
                  const Divider(height: 1),
                  _CategoryTile(
                    icon: Icons.local_offer,
                    title: 'Ofertas y Descuentos',
                    subtitle: 'Promociones especiales y ofertas',
                    value: _categoryPreferences['offers']!,
                    onChanged: (value) {
                      setState(() {
                        _categoryPreferences['offers'] = value;
                      });
                    },
                  ),
                  const Divider(height: 1),
                  _CategoryTile(
                    icon: Icons.system_update,
                    title: 'Actualizaciones',
                    subtitle: 'Cambios en tus publicaciones y favoritos',
                    value: _categoryPreferences['updates']!,
                    onChanged: (value) {
                      setState(() {
                        _categoryPreferences['updates'] = value;
                      });
                    },
                  ),
                  const Divider(height: 1),
                  _CategoryTile(
                    icon: Icons.campaign,
                    title: 'Marketing',
                    subtitle: 'Noticias, tips y recomendaciones',
                    value: _categoryPreferences['marketing']!,
                    onChanged: (value) {
                      setState(() {
                        _categoryPreferences['marketing'] = value;
                      });
                    },
                  ),
                  const Divider(height: 1),
                  _CategoryTile(
                    icon: Icons.security,
                    title: 'Seguridad',
                    subtitle: 'Alertas de seguridad y cuenta',
                    value: _categoryPreferences['security']!,
                    onChanged: (value) {
                      setState(() {
                        _categoryPreferences['security'] = value;
                      });
                    },
                    important: true,
                  ),
                  const Divider(height: 1),
                  _CategoryTile(
                    icon: Icons.history,
                    title: 'Actividad',
                    subtitle: 'Resumen de tu actividad',
                    value: _categoryPreferences['activity']!,
                    onChanged: (value) {
                      setState(() {
                        _categoryPreferences['activity'] = value;
                      });
                    },
                  ),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Frequency Section
            const _SectionHeader(
              icon: Icons.schedule,
              title: 'Frecuencia',
              subtitle: 'Con qué frecuencia recibes notificaciones',
            ),
            const SizedBox(height: 16),

            Card(
              child: RadioGroup<String>(
                groupValue: _notificationFrequency,
                onChanged: (value) {
                  setState(() {
                    _notificationFrequency = value!;
                  });
                },
                child: Column(
                  children: [
                    ListTile(
                      leading: const Radio<String>(
                        value: 'instant',
                      ),
                      title: const Text('Instantáneas'),
                      subtitle:
                          const Text('Recibe notificaciones inmediatamente'),
                      trailing: const Icon(Icons.flash_on),
                      onTap: () {
                        setState(() {
                          _notificationFrequency = 'instant';
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'daily',
                      ),
                      title: const Text('Resumen Diario'),
                      subtitle: const Text('Una vez al día a las 9:00 AM'),
                      trailing: const Icon(Icons.today),
                      onTap: () {
                        setState(() {
                          _notificationFrequency = 'daily';
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'weekly',
                      ),
                      title: const Text('Resumen Semanal'),
                      subtitle: const Text('Todos los lunes a las 9:00 AM'),
                      trailing: const Icon(Icons.calendar_today),
                      onTap: () {
                        setState(() {
                          _notificationFrequency = 'weekly';
                        });
                      },
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Quiet Hours Section
            const _SectionHeader(
              icon: Icons.bedtime,
              title: 'Horario de Silencio',
              subtitle: 'No recibas notificaciones en horarios específicos',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  SwitchListTile(
                    value: _quietHoursEnabled,
                    onChanged: (value) {
                      setState(() {
                        _quietHoursEnabled = value;
                      });
                    },
                    title: const Text('Activar Horario de Silencio'),
                    subtitle: Text(
                      _quietHoursEnabled
                          ? 'De ${_quietHoursStart.format(context)} a ${_quietHoursEnd.format(context)}'
                          : 'Recibe notificaciones todo el tiempo',
                    ),
                    secondary: const Icon(Icons.do_not_disturb),
                  ),
                  if (_quietHoursEnabled) ...[
                    const Divider(height: 1),
                    ListTile(
                      leading: const Icon(Icons.access_time),
                      title: const Text('Hora de inicio'),
                      trailing: Text(
                        _quietHoursStart.format(context),
                        style: theme.textTheme.titleMedium,
                      ),
                      onTap: () => _selectTime(context, true),
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Icon(Icons.access_time_filled),
                      title: const Text('Hora de fin'),
                      trailing: Text(
                        _quietHoursEnd.format(context),
                        style: theme.textTheme.titleMedium,
                      ),
                      onTap: () => _selectTime(context, false),
                    ),
                  ],
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Save Button
            SizedBox(
              width: double.infinity,
              child: FilledButton.icon(
                onPressed: _isSaving ? null : _savePreferences,
                icon: const Icon(Icons.save),
                label: _isSaving
                    ? const Text('Guardando...')
                    : const Text('Guardar Preferencias'),
              ),
            ),
            const SizedBox(height: 16),

            // Test Notification Button
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                onPressed: _sendTestNotification,
                icon: const Icon(Icons.send),
                label: const Text('Enviar Notificación de Prueba'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _selectTime(BuildContext context, bool isStart) async {
    final TimeOfDay? picked = await showTimePicker(
      context: context,
      initialTime: isStart ? _quietHoursStart : _quietHoursEnd,
    );

    if (picked != null) {
      setState(() {
        if (isStart) {
          _quietHoursStart = picked;
        } else {
          _quietHoursEnd = picked;
        }
      });
    }
  }

  Future<void> _savePreferences() async {
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
          content: Text('Preferencias guardadas correctamente'),
          backgroundColor: Colors.green,
        ),
      );
    }
  }

  void _sendTestNotification() {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: const Text('Notificación de prueba enviada'),
        action: SnackBarAction(
          label: 'Ver',
          onPressed: () {},
        ),
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

/// Category tile widget with switch
class _CategoryTile extends StatelessWidget {
  final IconData icon;
  final String title;
  final String subtitle;
  final bool value;
  final ValueChanged<bool> onChanged;
  final bool important;

  const _CategoryTile({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.value,
    required this.onChanged,
    this.important = false,
  });

  @override
  Widget build(BuildContext context) {
    return SwitchListTile(
      value: value,
      onChanged: important
          ? null
          : onChanged, // Security notifications can't be disabled
      title: Row(
        children: [
          Text(title),
          if (important) ...[
            const SizedBox(width: 8),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
              decoration: BoxDecoration(
                color: Colors.orange,
                borderRadius: BorderRadius.circular(4),
              ),
              child: const Text(
                'REQUERIDO',
                style: TextStyle(
                  fontSize: 9,
                  fontWeight: FontWeight.bold,
                  color: Colors.white,
                ),
              ),
            ),
          ],
        ],
      ),
      subtitle: Text(subtitle),
      secondary: Icon(icon),
    );
  }
}

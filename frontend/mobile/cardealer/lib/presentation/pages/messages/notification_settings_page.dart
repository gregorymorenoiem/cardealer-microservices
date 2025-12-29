import 'package:flutter/material.dart';

/// CM-008: Notification Settings
/// Configuración de notificaciones por conversación y Do Not Disturb
class NotificationSettingsPage extends StatefulWidget {
  const NotificationSettingsPage({super.key});

  @override
  State<NotificationSettingsPage> createState() =>
      _NotificationSettingsPageState();
}

class _NotificationSettingsPageState extends State<NotificationSettingsPage> {
  bool _notifyMessages = true;
  bool _notifyDeals = true;
  bool _notifyPriceDrops = true;
  bool _notifyNewListings = false;
  bool _notifyReviews = true;

  bool _soundEnabled = true;
  bool _vibrationEnabled = true;
  bool _ledEnabled = true;

  bool _dndEnabled = false;
  TimeOfDay _dndStart = const TimeOfDay(hour: 22, minute: 0);
  TimeOfDay _dndEnd = const TimeOfDay(hour: 8, minute: 0);

  final List<Map<String, dynamic>> _mutedConversations = [];

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Notificaciones'),
      ),
      body: ListView(
        children: [
          // Notification types section
          _buildSectionHeader('Tipos de notificaciones'),
          _buildNotificationToggle(
            icon: Icons.message,
            title: 'Mensajes',
            subtitle: 'Nuevos mensajes de concesionarios',
            value: _notifyMessages,
            onChanged: (value) => setState(() => _notifyMessages = value),
          ),
          _buildNotificationToggle(
            icon: Icons.local_offer,
            title: 'Ofertas especiales',
            subtitle: 'Promociones y descuentos',
            value: _notifyDeals,
            onChanged: (value) => setState(() => _notifyDeals = value),
          ),
          _buildNotificationToggle(
            icon: Icons.trending_down,
            title: 'Bajadas de precio',
            subtitle: 'Cuando un vehículo favorito baja de precio',
            value: _notifyPriceDrops,
            onChanged: (value) => setState(() => _notifyPriceDrops = value),
          ),
          _buildNotificationToggle(
            icon: Icons.new_releases,
            title: 'Nuevos anuncios',
            subtitle: 'Vehículos que coinciden con tus búsquedas',
            value: _notifyNewListings,
            onChanged: (value) => setState(() => _notifyNewListings = value),
          ),
          _buildNotificationToggle(
            icon: Icons.star,
            title: 'Reseñas',
            subtitle: 'Cuando alguien responde a tu reseña',
            value: _notifyReviews,
            onChanged: (value) => setState(() => _notifyReviews = value),
          ),
          const Divider(height: 32),

          // Alert settings section
          _buildSectionHeader('Configuración de alertas'),
          _buildNotificationToggle(
            icon: Icons.volume_up,
            title: 'Sonido',
            subtitle: 'Reproducir sonido de notificación',
            value: _soundEnabled,
            onChanged: (value) => setState(() => _soundEnabled = value),
          ),
          _buildNotificationToggle(
            icon: Icons.vibration,
            title: 'Vibración',
            subtitle: 'Vibrar al recibir notificaciones',
            value: _vibrationEnabled,
            onChanged: (value) => setState(() => _vibrationEnabled = value),
          ),
          _buildNotificationToggle(
            icon: Icons.lightbulb_outline,
            title: 'LED de notificación',
            subtitle: 'Parpadear LED para notificaciones',
            value: _ledEnabled,
            onChanged: (value) => setState(() => _ledEnabled = value),
          ),
          const Divider(height: 32),

          // Do Not Disturb section
          _buildSectionHeader('No molestar'),
          SwitchListTile(
            secondary: Icon(
              Icons.do_not_disturb_on,
              color: _dndEnabled ? theme.colorScheme.primary : null,
            ),
            title: const Text('Modo No molestar'),
            subtitle: Text(
              _dndEnabled
                  ? 'Activo de ${_formatTime(_dndStart)} a ${_formatTime(_dndEnd)}'
                  : 'Silenciar notificaciones en horario específico',
            ),
            value: _dndEnabled,
            onChanged: (value) => setState(() => _dndEnabled = value),
          ),
          if (_dndEnabled) ...[
            ListTile(
              leading: const SizedBox(width: 40),
              title: const Text('Hora de inicio'),
              trailing: OutlinedButton(
                onPressed: () => _selectDndTime(context, true),
                child: Text(_formatTime(_dndStart)),
              ),
            ),
            ListTile(
              leading: const SizedBox(width: 40),
              title: const Text('Hora de fin'),
              trailing: OutlinedButton(
                onPressed: () => _selectDndTime(context, false),
                child: Text(_formatTime(_dndEnd)),
              ),
            ),
          ],
          const Divider(height: 32),

          // Muted conversations section
          _buildSectionHeader('Conversaciones silenciadas'),
          if (_mutedConversations.isEmpty)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 32),
              child: Center(
                child: Column(
                  children: [
                    Icon(
                      Icons.notifications_off_outlined,
                      size: 64,
                      color: theme.colorScheme.outline,
                    ),
                    const SizedBox(height: 16),
                    Text(
                      'No hay conversaciones silenciadas',
                      style: theme.textTheme.bodyLarge?.copyWith(
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                    ),
                  ],
                ),
              ),
            )
          else
            ..._mutedConversations.map(
              (conversation) => ListTile(
                leading: CircleAvatar(
                  backgroundImage: NetworkImage(conversation['avatar']),
                ),
                title: Text(conversation['dealerName']),
                subtitle: Text(conversation['vehicleInfo']),
                trailing: IconButton(
                  icon: const Icon(Icons.volume_up),
                  onPressed: () {
                    setState(() => _mutedConversations.remove(conversation));
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(
                          'Notificaciones activadas para ${conversation['dealerName']}',
                        ),
                      ),
                    );
                  },
                ),
              ),
            ),
          const SizedBox(height: 16),
        ],
      ),
    );
  }

  Widget _buildSectionHeader(String title) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
      child: Text(
        title,
        style: Theme.of(context).textTheme.titleMedium?.copyWith(
              fontWeight: FontWeight.bold,
              color: Theme.of(context).colorScheme.primary,
            ),
      ),
    );
  }

  Widget _buildNotificationToggle({
    required IconData icon,
    required String title,
    required String subtitle,
    required bool value,
    required ValueChanged<bool> onChanged,
  }) {
    return SwitchListTile(
      secondary: Icon(icon),
      title: Text(title),
      subtitle: Text(subtitle),
      value: value,
      onChanged: onChanged,
    );
  }

  Future<void> _selectDndTime(BuildContext context, bool isStart) async {
    final time = await showTimePicker(
      context: context,
      initialTime: isStart ? _dndStart : _dndEnd,
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: Theme.of(context).colorScheme.copyWith(
                  primary: Theme.of(context).colorScheme.primary,
                ),
          ),
          child: child!,
        );
      },
    );

    if (time != null) {
      setState(() {
        if (isStart) {
          _dndStart = time;
        } else {
          _dndEnd = time;
        }
      });
    }
  }

  String _formatTime(TimeOfDay time) {
    final hour = time.hourOfPeriod == 0 ? 12 : time.hourOfPeriod;
    final minute = time.minute.toString().padLeft(2, '0');
    final period = time.period == DayPeriod.am ? 'AM' : 'PM';
    return '$hour:$minute $period';
  }
}

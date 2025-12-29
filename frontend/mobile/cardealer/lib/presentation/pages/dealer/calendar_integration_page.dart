import 'package:flutter/material.dart';
import 'package:table_calendar/table_calendar.dart';

/// DP-009: Calendar Integration
/// Calendario de citas y sincronización
class CalendarIntegrationPage extends StatefulWidget {
  const CalendarIntegrationPage({super.key});

  @override
  State<CalendarIntegrationPage> createState() =>
      _CalendarIntegrationPageState();
}

class _CalendarIntegrationPageState extends State<CalendarIntegrationPage> {
  CalendarFormat _calendarFormat = CalendarFormat.month;
  DateTime _focusedDay = DateTime.now();
  DateTime? _selectedDay;

  final Map<DateTime, List<Map<String, dynamic>>> _appointments = {
    DateTime.now(): [
      {
        'time': '10:00 AM',
        'title': 'Prueba de manejo - Toyota Camry',
        'client': 'Juan Pérez',
        'type': 'test_drive',
      },
      {
        'time': '2:00 PM',
        'title': 'Entrega de vehículo - Honda Civic',
        'client': 'María García',
        'type': 'delivery',
      },
    ],
    DateTime.now().add(const Duration(days: 1)): [
      {
        'time': '11:00 AM',
        'title': 'Consulta - BMW X5',
        'client': 'Carlos Rodríguez',
        'type': 'consultation',
      },
    ],
    DateTime.now().add(const Duration(days: 3)): [
      {
        'time': '3:00 PM',
        'title': 'Inspección - Ford Escape',
        'client': 'Ana López',
        'type': 'inspection',
      },
    ],
  };

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Calendario de Citas'),
        actions: [
          IconButton(
            icon: const Icon(Icons.sync),
            onPressed: _syncCalendar,
            tooltip: 'Sincronizar',
          ),
          IconButton(
            icon: const Icon(Icons.settings),
            onPressed: _showSettings,
            tooltip: 'Configuración',
          ),
        ],
      ),
      body: Column(
        children: [
          // Calendar
          Card(
            margin: const EdgeInsets.all(16),
            child: TableCalendar(
              firstDay: DateTime.utc(2024, 1, 1),
              lastDay: DateTime.utc(2025, 12, 31),
              focusedDay: _focusedDay,
              calendarFormat: _calendarFormat,
              selectedDayPredicate: (day) {
                return isSameDay(_selectedDay, day);
              },
              onDaySelected: (selectedDay, focusedDay) {
                setState(() {
                  _selectedDay = selectedDay;
                  _focusedDay = focusedDay;
                });
              },
              onFormatChanged: (format) {
                setState(() {
                  _calendarFormat = format;
                });
              },
              onPageChanged: (focusedDay) {
                _focusedDay = focusedDay;
              },
              eventLoader: (day) {
                return _getAppointmentsForDay(day);
              },
              calendarStyle: CalendarStyle(
                todayDecoration: BoxDecoration(
                  color: theme.colorScheme.primaryContainer,
                  shape: BoxShape.circle,
                ),
                selectedDecoration: BoxDecoration(
                  color: theme.colorScheme.primary,
                  shape: BoxShape.circle,
                ),
                markerDecoration: BoxDecoration(
                  color: theme.colorScheme.secondary,
                  shape: BoxShape.circle,
                ),
              ),
              headerStyle: const HeaderStyle(
                formatButtonVisible: true,
                titleCentered: true,
              ),
            ),
          ),

          // Appointments list
          Expanded(
            child: _selectedDay != null
                ? _buildAppointmentsList(theme)
                : _buildNoSelectionMessage(theme),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: 'calendar_fab',
        onPressed: () => _showAddAppointmentDialog(context),
        icon: const Icon(Icons.add),
        label: const Text('Nueva Cita'),
      ),
    );
  }

  Widget _buildAppointmentsList(ThemeData theme) {
    final appointments = _getAppointmentsForDay(_selectedDay!);

    if (appointments.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.event_available,
              size: 64,
              color: theme.colorScheme.outline,
            ),
            const SizedBox(height: 16),
            Text(
              'No hay citas para este día',
              style: theme.textTheme.titleMedium?.copyWith(
                color: theme.colorScheme.outline,
              ),
            ),
          ],
        ),
      );
    }

    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: appointments.length,
      itemBuilder: (context, index) {
        final appointment = appointments[index];
        return _AppointmentCard(
          appointment: appointment,
          onTap: () => _showAppointmentDetails(context, appointment),
          onEdit: () => _editAppointment(appointment),
          onDelete: () => _deleteAppointment(appointment),
        );
      },
    );
  }

  Widget _buildNoSelectionMessage(ThemeData theme) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.calendar_today,
            size: 64,
            color: theme.colorScheme.outline,
          ),
          const SizedBox(height: 16),
          Text(
            'Selecciona un día para ver las citas',
            style: theme.textTheme.titleMedium?.copyWith(
              color: theme.colorScheme.outline,
            ),
          ),
        ],
      ),
    );
  }

  List<Map<String, dynamic>> _getAppointmentsForDay(DateTime day) {
    for (final entry in _appointments.entries) {
      if (isSameDay(entry.key, day)) {
        return entry.value;
      }
    }
    return [];
  }

  void _syncCalendar() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Sincronizar Calendario'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.calendar_today),
              title: const Text('Google Calendar'),
              trailing: const Icon(Icons.chevron_right),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                      content: Text('Sincronizando con Google Calendar...')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.apple),
              title: const Text('Apple Calendar'),
              trailing: const Icon(Icons.chevron_right),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                      content: Text('Sincronizando con Apple Calendar...')),
                );
              },
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
        ],
      ),
    );
  }

  void _showSettings() {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Padding(
              padding: EdgeInsets.all(16),
              child: Text(
                'Configuración de Calendario',
                style: TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
            SwitchListTile(
              title: const Text('Recordatorios'),
              subtitle: const Text('15 minutos antes de cada cita'),
              value: true,
              onChanged: (value) {},
            ),
            SwitchListTile(
              title: const Text('Notificaciones push'),
              subtitle: const Text('Recibir alertas de nuevas citas'),
              value: true,
              onChanged: (value) {},
            ),
            ListTile(
              leading: const Icon(Icons.access_time),
              title: const Text('Horario laboral'),
              subtitle: const Text('9:00 AM - 6:00 PM'),
              trailing: const Icon(Icons.chevron_right),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Configurar horario laboral')),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  void _showAddAppointmentDialog(BuildContext context) {
    final theme = Theme.of(context);

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Nueva Cita'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                decoration: const InputDecoration(
                  labelText: 'Título',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: 16),
              TextFormField(
                decoration: const InputDecoration(
                  labelText: 'Cliente',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<String>(
                decoration: const InputDecoration(
                  labelText: 'Tipo de cita',
                  border: OutlineInputBorder(),
                ),
                items: const [
                  DropdownMenuItem(
                    value: 'test_drive',
                    child: Text('Prueba de manejo'),
                  ),
                  DropdownMenuItem(
                    value: 'delivery',
                    child: Text('Entrega'),
                  ),
                  DropdownMenuItem(
                    value: 'consultation',
                    child: Text('Consulta'),
                  ),
                  DropdownMenuItem(
                    value: 'inspection',
                    child: Text('Inspección'),
                  ),
                ],
                onChanged: (value) {},
              ),
              const SizedBox(height: 16),
              TextFormField(
                decoration: const InputDecoration(
                  labelText: 'Hora',
                  border: OutlineInputBorder(),
                  suffixIcon: Icon(Icons.access_time),
                ),
                readOnly: true,
                onTap: () async {
                  final time = await showTimePicker(
                    context: context,
                    initialTime: TimeOfDay.now(),
                  );
                  if (time != null) {
                    // Handle time selection
                  }
                },
              ),
            ],
          ),
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
                const SnackBar(content: Text('Cita creada')),
              );
            },
            child: const Text('Crear'),
          ),
        ],
      ),
    );
  }

  void _showAppointmentDetails(
      BuildContext context, Map<String, dynamic> appointment) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.6,
        minChildSize: 0.4,
        maxChildSize: 0.9,
        expand: false,
        builder: (context, scrollController) {
          return _AppointmentDetailsSheet(
            appointment: appointment,
            scrollController: scrollController,
          );
        },
      ),
    );
  }

  void _editAppointment(Map<String, dynamic> appointment) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Editar: ${appointment['title']}')),
    );
  }

  void _deleteAppointment(Map<String, dynamic> appointment) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Eliminar cita'),
        content: Text('¿Eliminar "${appointment['title']}"?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () {
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Cita eliminada')),
              );
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

class _AppointmentCard extends StatelessWidget {
  final Map<String, dynamic> appointment;
  final VoidCallback onTap;
  final VoidCallback onEdit;
  final VoidCallback onDelete;

  const _AppointmentCard({
    required this.appointment,
    required this.onTap,
    required this.onEdit,
    required this.onDelete,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final type = appointment['type'] as String;
    final config = _getTypeConfig(type);

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              Container(
                width: 4,
                height: 60,
                decoration: BoxDecoration(
                  color: config['color'] as Color,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Icon(
                          config['icon'] as IconData,
                          size: 16,
                          color: config['color'] as Color,
                        ),
                        const SizedBox(width: 8),
                        Text(
                          appointment['time'],
                          style: theme.textTheme.labelLarge?.copyWith(
                            color: theme.colorScheme.primary,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(
                      appointment['title'],
                      style: theme.textTheme.titleSmall?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        Icon(
                          Icons.person,
                          size: 14,
                          color: theme.colorScheme.onSurfaceVariant,
                        ),
                        const SizedBox(width: 4),
                        Text(
                          appointment['client'],
                          style: theme.textTheme.bodySmall,
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              PopupMenuButton(
                itemBuilder: (context) => [
                  const PopupMenuItem(
                    value: 'edit',
                    child: Row(
                      children: [
                        Icon(Icons.edit),
                        SizedBox(width: 8),
                        Text('Editar'),
                      ],
                    ),
                  ),
                  const PopupMenuItem(
                    value: 'delete',
                    child: Row(
                      children: [
                        Icon(Icons.delete, color: Colors.red),
                        SizedBox(width: 8),
                        Text('Eliminar', style: TextStyle(color: Colors.red)),
                      ],
                    ),
                  ),
                ],
                onSelected: (value) {
                  if (value == 'edit') {
                    onEdit();
                  } else if (value == 'delete') {
                    onDelete();
                  }
                },
              ),
            ],
          ),
        ),
      ),
    );
  }

  Map<String, dynamic> _getTypeConfig(String type) {
    switch (type) {
      case 'test_drive':
        return {'icon': Icons.directions_car, 'color': Colors.blue};
      case 'delivery':
        return {'icon': Icons.local_shipping, 'color': Colors.green};
      case 'consultation':
        return {'icon': Icons.chat, 'color': Colors.orange};
      case 'inspection':
        return {'icon': Icons.search, 'color': Colors.purple};
      default:
        return {'icon': Icons.event, 'color': Colors.grey};
    }
  }
}

class _AppointmentDetailsSheet extends StatelessWidget {
  final Map<String, dynamic> appointment;
  final ScrollController scrollController;

  const _AppointmentDetailsSheet({
    required this.appointment,
    required this.scrollController,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Material(
      child: ListView(
        controller: scrollController,
        padding: const EdgeInsets.all(16),
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Detalles de la Cita',
                style: theme.textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              IconButton(
                icon: const Icon(Icons.close),
                onPressed: () => Navigator.pop(context),
              ),
            ],
          ),
          const SizedBox(height: 24),
          ListTile(
            leading: const Icon(Icons.title),
            title: const Text('Título'),
            subtitle: Text(appointment['title']),
          ),
          ListTile(
            leading: const Icon(Icons.person),
            title: const Text('Cliente'),
            subtitle: Text(appointment['client']),
          ),
          ListTile(
            leading: const Icon(Icons.access_time),
            title: const Text('Hora'),
            subtitle: Text(appointment['time']),
          ),
          const SizedBox(height: 24),
          Row(
            children: [
              Expanded(
                child: OutlinedButton.icon(
                  onPressed: () {
                    Navigator.pop(context);
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text('Llamando al cliente...')),
                    );
                  },
                  icon: const Icon(Icons.phone),
                  label: const Text('Llamar'),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: FilledButton.icon(
                  onPressed: () {
                    Navigator.pop(context);
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text('Enviando mensaje...')),
                    );
                  },
                  icon: const Icon(Icons.message),
                  label: const Text('Mensaje'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

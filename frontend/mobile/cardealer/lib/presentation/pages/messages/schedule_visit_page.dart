import 'package:flutter/material.dart';

/// CM-007: Schedule Visit
/// Página para agendar visitas con el concesionario
class ScheduleVisitPage extends StatefulWidget {
  const ScheduleVisitPage({super.key});

  @override
  State<ScheduleVisitPage> createState() => _ScheduleVisitPageState();
}

class _ScheduleVisitPageState extends State<ScheduleVisitPage> {
  String _visitType = 'test_drive';
  DateTime? _selectedDate;
  TimeOfDay? _selectedTime;
  String _locationType = 'dealer';
  final TextEditingController _addressController = TextEditingController();
  final TextEditingController _notesController = TextEditingController();
  bool _setReminder = true;

  final Map<String, String> _visitTypes = {
    'test_drive': 'Prueba de manejo',
    'inspection': 'Inspección del vehículo',
    'negotiation': 'Negociación de precio',
  };

  final Map<String, String> _locationTypes = {
    'dealer': 'En el concesionario',
    'home': 'En mi domicilio',
    'other': 'Otra ubicación',
  };

  @override
  void dispose() {
    _addressController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final dealerInfo =
        ModalRoute.of(context)?.settings.arguments as Map<String, dynamic>?;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Agendar Visita'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Dealer info card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Row(
                  children: [
                    CircleAvatar(
                      radius: 24,
                      backgroundImage: NetworkImage(
                        dealerInfo?['avatar'] ??
                            'https://ui-avatars.com/api/?name=Dealer',
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            dealerInfo?['dealerName'] ?? 'Premium Motors',
                            style: theme.textTheme.titleMedium?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          Text(
                            dealerInfo?['vehicleInfo'] ?? 'Toyota Camry 2024',
                            style: theme.textTheme.bodyMedium?.copyWith(
                              color: theme.colorScheme.primary,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 24),

            // Visit type section
            Text(
              'Tipo de visita',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 12),
            RadioGroup<String>(
              groupValue: _visitType,
              onChanged: (value) {
                setState(() => _visitType = value!);
              },
              child: Column(
                children: _visitTypes.entries.map((entry) {
                  return ListTile(
                    title: Text(entry.value),
                    leading: Radio<String>(
                      value: entry.key,
                    ),
                    contentPadding: EdgeInsets.zero,
                    onTap: () {
                      setState(() => _visitType = entry.key);
                    },
                  );
                }).toList(),
              ),
            ),
            const SizedBox(height: 24),

            // Date and time section
            Text(
              'Fecha y hora',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: () => _selectDate(context),
                    icon: const Icon(Icons.calendar_today),
                    label: Text(
                      _selectedDate == null
                          ? 'Seleccionar fecha'
                          : _formatDate(_selectedDate!),
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: _selectedDate == null
                        ? null
                        : () => _selectTime(context),
                    icon: const Icon(Icons.access_time),
                    label: Text(
                      _selectedTime == null
                          ? 'Seleccionar hora'
                          : _formatTime(_selectedTime!),
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 24),

            // Location section
            Text(
              'Ubicación',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 12),
            RadioGroup<String>(
              groupValue: _locationType,
              onChanged: (value) {
                setState(() => _locationType = value!);
              },
              child: Column(
                children: _locationTypes.entries.map((entry) {
                  return ListTile(
                    title: Text(entry.value),
                    leading: Radio<String>(
                      value: entry.key,
                    ),
                    contentPadding: EdgeInsets.zero,
                    onTap: () {
                      setState(() => _locationType = entry.key);
                    },
                  );
                }).toList(),
              ),
            ),
            if (_locationType == 'other') ...[
              const SizedBox(height: 12),
              TextField(
                controller: _addressController,
                decoration: const InputDecoration(
                  labelText: 'Dirección',
                  hintText: 'Ingresa la dirección',
                  prefixIcon: Icon(Icons.location_on),
                  border: OutlineInputBorder(),
                ),
                maxLines: 2,
              ),
            ],
            const SizedBox(height: 24),

            // Notes section
            Text(
              'Notas adicionales',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            Text(
              'Opcional',
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _notesController,
              decoration: const InputDecoration(
                hintText: 'Añade cualquier información relevante...',
                border: OutlineInputBorder(),
              ),
              maxLines: 3,
            ),
            const SizedBox(height: 24),

            // Reminder section
            SwitchListTile(
              title: const Text('Recordarme antes de la visita'),
              subtitle: const Text('Recibirás una notificación 1 hora antes'),
              value: _setReminder,
              onChanged: (value) {
                setState(() => _setReminder = value);
              },
              contentPadding: EdgeInsets.zero,
            ),
            const SizedBox(height: 32),

            // Action buttons
            Row(
              children: [
                Expanded(
                  child: OutlinedButton(
                    onPressed: () => Navigator.pop(context),
                    child: const Text('Cancelar'),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: FilledButton(
                    onPressed:
                        _canSchedule ? () => _scheduleVisit(context) : null,
                    child: const Text('Agendar'),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  bool get _canSchedule {
    if (_selectedDate == null || _selectedTime == null) return false;
    if (_locationType == 'other' && _addressController.text.trim().isEmpty) {
      return false;
    }
    return true;
  }

  Future<void> _selectDate(BuildContext context) async {
    final now = DateTime.now();
    final firstDate = now;
    final lastDate = now.add(const Duration(days: 30));

    final date = await showDatePicker(
      context: context,
      initialDate: _selectedDate ?? firstDate,
      firstDate: firstDate,
      lastDate: lastDate,
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

    if (date != null) {
      setState(() => _selectedDate = date);
    }
  }

  Future<void> _selectTime(BuildContext context) async {
    final time = await showTimePicker(
      context: context,
      initialTime: _selectedTime ?? TimeOfDay.now(),
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
      setState(() => _selectedTime = time);
    }
  }

  void _scheduleVisit(BuildContext context) {
    final visitData = {
      'type': _visitType,
      'typeName': _visitTypes[_visitType],
      'date': _selectedDate,
      'time': _selectedTime,
      'locationType': _locationType,
      'locationName': _locationTypes[_locationType],
      'address': _locationType == 'other' ? _addressController.text : null,
      'notes': _notesController.text.trim(),
      'reminder': _setReminder,
    };

    // Show confirmation dialog
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Confirmar visita'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildConfirmationRow(
              Icons.event_note,
              'Tipo',
              visitData['typeName'] as String,
            ),
            const SizedBox(height: 8),
            _buildConfirmationRow(
              Icons.calendar_today,
              'Fecha',
              _formatDate(visitData['date'] as DateTime),
            ),
            const SizedBox(height: 8),
            _buildConfirmationRow(
              Icons.access_time,
              'Hora',
              _formatTime(visitData['time'] as TimeOfDay),
            ),
            const SizedBox(height: 8),
            _buildConfirmationRow(
              Icons.location_on,
              'Ubicación',
              visitData['locationName'] as String,
            ),
            if (visitData['address'] != null) ...[
              const SizedBox(height: 4),
              Padding(
                padding: const EdgeInsets.only(left: 32),
                child: Text(
                  visitData['address'] as String,
                  style: Theme.of(context).textTheme.bodySmall,
                ),
              ),
            ],
            if ((visitData['notes'] as String).isNotEmpty) ...[
              const SizedBox(height: 8),
              _buildConfirmationRow(
                Icons.note,
                'Notas',
                visitData['notes'] as String,
              ),
            ],
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Editar'),
          ),
          FilledButton(
            onPressed: () {
              // TODO: Send visit request to backend
              Navigator.pop(context); // Close dialog
              Navigator.pop(context); // Go back to chat
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Visita agendada exitosamente'),
                  behavior: SnackBarBehavior.floating,
                ),
              );
            },
            child: const Text('Confirmar'),
          ),
        ],
      ),
    );
  }

  Widget _buildConfirmationRow(IconData icon, String label, String value) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 20),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: Theme.of(context).colorScheme.onSurfaceVariant,
                    ),
              ),
              Text(
                value,
                style: Theme.of(context).textTheme.bodyMedium,
              ),
            ],
          ),
        ),
      ],
    );
  }

  String _formatDate(DateTime date) {
    final months = [
      'Enero',
      'Febrero',
      'Marzo',
      'Abril',
      'Mayo',
      'Junio',
      'Julio',
      'Agosto',
      'Septiembre',
      'Octubre',
      'Noviembre',
      'Diciembre'
    ];
    return '${date.day} de ${months[date.month - 1]} ${date.year}';
  }

  String _formatTime(TimeOfDay time) {
    final hour = time.hourOfPeriod == 0 ? 12 : time.hourOfPeriod;
    final minute = time.minute.toString().padLeft(2, '0');
    final period = time.period == DayPeriod.am ? 'AM' : 'PM';
    return '$hour:$minute $period';
  }
}

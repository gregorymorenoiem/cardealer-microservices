import 'package:flutter/material.dart';

/// DP-006: Leads Management
/// Gestión de leads y seguimiento de contactos
class LeadsManagementPage extends StatefulWidget {
  const LeadsManagementPage({super.key});

  @override
  State<LeadsManagementPage> createState() => _LeadsManagementPageState();
}

class _LeadsManagementPageState extends State<LeadsManagementPage> {
  String _statusFilter = 'all';

  final List<Map<String, dynamic>> _leads = [
    {
      'id': '1',
      'name': 'Juan Pérez',
      'email': 'juan@email.com',
      'phone': '+1 234 567 8900',
      'vehicle': 'Toyota Camry 2024',
      'status': 'new',
      'source': 'web',
      'date': '2024-12-10',
      'notes': 'Interesado en financiamiento',
      'avatar': 'https://via.placeholder.com/100',
    },
    {
      'id': '2',
      'name': 'María García',
      'email': 'maria@email.com',
      'phone': '+1 234 567 8901',
      'vehicle': 'Honda Civic 2023',
      'status': 'contacted',
      'source': 'phone',
      'date': '2024-12-09',
      'notes': 'Quiere agendar prueba de manejo',
      'avatar': 'https://via.placeholder.com/100',
    },
    {
      'id': '3',
      'name': 'Carlos Rodríguez',
      'email': 'carlos@email.com',
      'phone': '+1 234 567 8902',
      'vehicle': 'Ford Escape 2024',
      'status': 'qualified',
      'source': 'social',
      'date': '2024-12-08',
      'notes': 'Crédito preaprobado, listo para comprar',
      'avatar': 'https://via.placeholder.com/100',
    },
    {
      'id': '4',
      'name': 'Ana López',
      'email': 'ana@email.com',
      'phone': '+1 234 567 8903',
      'vehicle': 'BMW X5 2024',
      'status': 'negotiating',
      'source': 'referral',
      'date': '2024-12-07',
      'notes': 'Negociando precio final',
      'avatar': 'https://via.placeholder.com/100',
    },
    {
      'id': '5',
      'name': 'Luis Martínez',
      'email': 'luis@email.com',
      'phone': '+1 234 567 8904',
      'vehicle': 'Mazda CX-5 2023',
      'status': 'lost',
      'source': 'web',
      'date': '2024-12-05',
      'notes': 'Compró en otra agencia',
      'avatar': 'https://via.placeholder.com/100',
    },
  ];

  List<Map<String, dynamic>> get _filteredLeads {
    if (_statusFilter == 'all') return _leads;
    return _leads.where((l) => l['status'] == _statusFilter).toList();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Mis Leads'),
        actions: [
          IconButton(
            icon: const Icon(Icons.search),
            onPressed: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Buscar leads')),
              );
            },
          ),
          IconButton(
            icon: const Icon(Icons.filter_list),
            onPressed: () => _showFilterSheet(context),
          ),
        ],
      ),
      body: Column(
        children: [
          // Status filters
          _buildStatusFilters(theme),

          // Statistics
          _buildStatistics(theme),

          // Leads list
          Expanded(
            child: _filteredLeads.isEmpty
                ? _buildEmptyState(theme)
                : ListView.builder(
                    itemCount: _filteredLeads.length,
                    itemBuilder: (context, index) {
                      final lead = _filteredLeads[index];
                      return _LeadCard(
                        lead: lead,
                        onTap: () => _showLeadDetails(context, lead),
                        onCall: () {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                                content: Text('Llamando a ${lead['name']}')),
                          );
                        },
                        onEmail: () {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                                content:
                                    Text('Enviando email a ${lead['name']}')),
                          );
                        },
                      );
                    },
                  ),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: 'leads_fab',
        onPressed: () {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Agregar lead manualmente')),
          );
        },
        icon: const Icon(Icons.add),
        label: const Text('Nuevo Lead'),
      ),
    );
  }

  Widget _buildStatusFilters(ThemeData theme) {
    final filters = {
      'all': 'Todos',
      'new': 'Nuevos',
      'contacted': 'Contactados',
      'qualified': 'Calificados',
      'negotiating': 'Negociando',
      'lost': 'Perdidos',
    };

    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      padding: const EdgeInsets.all(16),
      child: Row(
        children: filters.entries.map((entry) {
          final count = entry.key == 'all'
              ? _leads.length
              : _leads.where((l) => l['status'] == entry.key).length;

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

  Widget _buildStatistics(ThemeData theme) {
    final newLeads = _leads.where((l) => l['status'] == 'new').length;
    final conversionRate =
        (_leads.where((l) => l['status'] == 'negotiating').length /
                _leads.length *
                100)
            .toInt();
    const avgResponseTime = '2.5h';

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: theme.colorScheme.surfaceContainerHighest,
        border: Border(
          bottom: BorderSide(color: theme.colorScheme.outlineVariant),
        ),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: [
          _StatItem(
            icon: Icons.new_releases,
            label: 'Nuevos',
            value: newLeads.toString(),
            color: Colors.orange,
          ),
          _StatItem(
            icon: Icons.trending_up,
            label: 'Conversión',
            value: '$conversionRate%',
            color: Colors.green,
          ),
          _StatItem(
            icon: Icons.access_time,
            label: 'Resp. Promedio',
            value: avgResponseTime,
            color: theme.colorScheme.primary,
          ),
        ],
      ),
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
              Icons.people_outline,
              size: 80,
              color: theme.colorScheme.outline,
            ),
            const SizedBox(height: 16),
            Text(
              'No hay leads',
              style: theme.textTheme.titleLarge,
            ),
            const SizedBox(height: 8),
            Text(
              _statusFilter == 'all'
                  ? 'Aún no has recibido ningún lead'
                  : 'No hay leads con este estado',
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
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
              leading: const Icon(Icons.priority_high),
              title: const Text('Ordenar por prioridad'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Ordenado por prioridad')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.source),
              title: const Text('Filtrar por fuente'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Filtrando por fuente')),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  void _showLeadDetails(BuildContext context, Map<String, dynamic> lead) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.9,
        minChildSize: 0.5,
        maxChildSize: 0.95,
        expand: false,
        builder: (context, scrollController) {
          return _LeadDetailsSheet(
            lead: lead,
            scrollController: scrollController,
            onStatusChange: (newStatus) {
              setState(() {
                lead['status'] = newStatus;
              });
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Estado actualizado')),
              );
            },
          );
        },
      ),
    );
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

class _LeadCard extends StatelessWidget {
  final Map<String, dynamic> lead;
  final VoidCallback onTap;
  final VoidCallback onCall;
  final VoidCallback onEmail;

  const _LeadCard({
    required this.lead,
    required this.onTap,
    required this.onCall,
    required this.onEmail,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final status = lead['status'] as String;

    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  CircleAvatar(
                    backgroundImage: NetworkImage(lead['avatar']),
                    radius: 24,
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
                                lead['name'],
                                style: theme.textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            _StatusBadge(status: status),
                          ],
                        ),
                        const SizedBox(height: 4),
                        Text(
                          lead['vehicle'],
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: theme.colorScheme.primary,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              Row(
                children: [
                  Icon(Icons.email,
                      size: 14, color: theme.colorScheme.onSurfaceVariant),
                  const SizedBox(width: 4),
                  Expanded(
                    child: Text(
                      lead['email'],
                      style: theme.textTheme.bodySmall,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 4),
              Row(
                children: [
                  Icon(Icons.phone,
                      size: 14, color: theme.colorScheme.onSurfaceVariant),
                  const SizedBox(width: 4),
                  Text(
                    lead['phone'],
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  _SourceChip(source: lead['source'] as String),
                  const SizedBox(width: 8),
                  Icon(Icons.calendar_today,
                      size: 14, color: theme.colorScheme.onSurfaceVariant),
                  const SizedBox(width: 4),
                  Text(
                    lead['date'],
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ),
              if (lead['notes'] != null &&
                  (lead['notes'] as String).isNotEmpty) ...[
                const SizedBox(height: 12),
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: theme.colorScheme.surfaceContainerHighest,
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Row(
                    children: [
                      Icon(
                        Icons.notes,
                        size: 16,
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          lead['notes'],
                          style: theme.textTheme.bodySmall,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
              const SizedBox(height: 12),
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  TextButton.icon(
                    onPressed: onCall,
                    icon: const Icon(Icons.phone, size: 18),
                    label: const Text('Llamar'),
                  ),
                  const SizedBox(width: 8),
                  TextButton.icon(
                    onPressed: onEmail,
                    icon: const Icon(Icons.email, size: 18),
                    label: const Text('Email'),
                  ),
                ],
              ),
            ],
          ),
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
      case 'new':
        return {'label': 'Nuevo', 'color': Colors.blue};
      case 'contacted':
        return {'label': 'Contactado', 'color': Colors.orange};
      case 'qualified':
        return {'label': 'Calificado', 'color': Colors.purple};
      case 'negotiating':
        return {'label': 'Negociando', 'color': Colors.green};
      case 'lost':
        return {'label': 'Perdido', 'color': Colors.red};
      default:
        return {'label': status, 'color': Colors.grey};
    }
  }
}

class _SourceChip extends StatelessWidget {
  final String source;

  const _SourceChip({required this.source});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final config = _getSourceConfig(source);

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: theme.colorScheme.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: theme.colorScheme.outline),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(config['icon'], size: 12),
          const SizedBox(width: 4),
          Text(
            config['label'],
            style: theme.textTheme.labelSmall,
          ),
        ],
      ),
    );
  }

  Map<String, dynamic> _getSourceConfig(String source) {
    switch (source) {
      case 'web':
        return {'label': 'Web', 'icon': Icons.language};
      case 'phone':
        return {'label': 'Teléfono', 'icon': Icons.phone};
      case 'social':
        return {'label': 'Redes', 'icon': Icons.share};
      case 'referral':
        return {'label': 'Referido', 'icon': Icons.people};
      default:
        return {'label': source, 'icon': Icons.help_outline};
    }
  }
}

class _LeadDetailsSheet extends StatefulWidget {
  final Map<String, dynamic> lead;
  final ScrollController scrollController;
  final Function(String) onStatusChange;

  const _LeadDetailsSheet({
    required this.lead,
    required this.scrollController,
    required this.onStatusChange,
  });

  @override
  State<_LeadDetailsSheet> createState() => _LeadDetailsSheetState();
}

class _LeadDetailsSheetState extends State<_LeadDetailsSheet> {
  final TextEditingController _notesController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _notesController.text = widget.lead['notes'] ?? '';
  }

  @override
  void dispose() {
    _notesController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Material(
      child: ListView(
        controller: widget.scrollController,
        padding: const EdgeInsets.all(16),
        children: [
          // Header
          Row(
            children: [
              CircleAvatar(
                backgroundImage: NetworkImage(widget.lead['avatar']),
                radius: 32,
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      widget.lead['name'],
                      style: theme.textTheme.headlineSmall?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 4),
                    _StatusBadge(status: widget.lead['status']),
                  ],
                ),
              ),
              IconButton(
                icon: const Icon(Icons.close),
                onPressed: () => Navigator.pop(context),
              ),
            ],
          ),
          const SizedBox(height: 24),

          // Contact info
          Text('Información de contacto', style: theme.textTheme.titleMedium),
          const SizedBox(height: 12),
          ListTile(
            leading: const Icon(Icons.email),
            title: Text(widget.lead['email']),
            trailing: IconButton(
              icon: const Icon(Icons.copy),
              onPressed: () {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Email copiado')),
                );
              },
            ),
          ),
          ListTile(
            leading: const Icon(Icons.phone),
            title: Text(widget.lead['phone']),
            trailing: IconButton(
              icon: const Icon(Icons.call),
              onPressed: () {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text('Llamando a ${widget.lead['name']}')),
                );
              },
            ),
          ),
          const SizedBox(height: 24),

          // Vehicle interest
          Text('Vehículo de interés', style: theme.textTheme.titleMedium),
          const SizedBox(height: 12),
          Card(
            child: ListTile(
              leading: const Icon(Icons.directions_car),
              title: Text(widget.lead['vehicle']),
              trailing: const Icon(Icons.chevron_right),
              onTap: () {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Ver vehículo')),
                );
              },
            ),
          ),
          const SizedBox(height: 24),

          // Status change
          Text('Cambiar estado', style: theme.textTheme.titleMedium),
          const SizedBox(height: 12),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: ['new', 'contacted', 'qualified', 'negotiating', 'lost']
                .map((status) => FilterChip(
                      label: Text(_StatusBadge(status: status)
                          .build(context)
                          .toString()),
                      selected: widget.lead['status'] == status,
                      onSelected: (_) => widget.onStatusChange(status),
                    ))
                .toList(),
          ),
          const SizedBox(height: 24),

          // Notes
          Text('Notas', style: theme.textTheme.titleMedium),
          const SizedBox(height: 12),
          TextField(
            controller: _notesController,
            maxLines: 5,
            decoration: const InputDecoration(
              hintText: 'Agregar notas sobre este lead...',
              border: OutlineInputBorder(),
            ),
          ),
          const SizedBox(height: 16),
          FilledButton(
            onPressed: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Notas guardadas')),
              );
            },
            child: const Text('Guardar notas'),
          ),
          const SizedBox(height: 24),

          // Contact history
          Text('Historial de contacto', style: theme.textTheme.titleMedium),
          const SizedBox(height: 12),
          _buildTimelineItem(
            theme,
            icon: Icons.phone,
            title: 'Llamada telefónica',
            date: '2024-12-10 14:30',
            description: 'Conversación sobre opciones de financiamiento',
          ),
          _buildTimelineItem(
            theme,
            icon: Icons.email,
            title: 'Email enviado',
            date: '2024-12-09 10:15',
            description: 'Información adicional del vehículo',
          ),
          _buildTimelineItem(
            theme,
            icon: Icons.add_circle,
            title: 'Lead creado',
            date: '2024-12-08 16:45',
            description: 'Formulario web - Página del vehículo',
          ),
        ],
      ),
    );
  }

  Widget _buildTimelineItem(
    ThemeData theme, {
    required IconData icon,
    required String title,
    required String date,
    required String description,
  }) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: theme.colorScheme.primaryContainer,
              shape: BoxShape.circle,
            ),
            child: Icon(icon, size: 20, color: theme.colorScheme.primary),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  date,
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  description,
                  style: theme.textTheme.bodyMedium,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

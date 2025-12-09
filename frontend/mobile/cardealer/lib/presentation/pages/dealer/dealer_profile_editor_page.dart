import 'package:flutter/material.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';

/// DP-010: Dealer Profile Editor
/// Editor de perfil público del dealer
class DealerProfileEditorPage extends StatefulWidget {
  const DealerProfileEditorPage({super.key});

  @override
  State<DealerProfileEditorPage> createState() =>
      _DealerProfileEditorPageState();
}

class _DealerProfileEditorPageState extends State<DealerProfileEditorPage> {
  final _formKey = GlobalKey<FormState>();

  // Controllers
  final _nameController = TextEditingController(text: 'AutoMax Premium');
  final _phoneController = TextEditingController(text: '+1 305-555-0123');
  final _emailController = TextEditingController(text: 'info@automax.com');
  final _addressController =
      TextEditingController(text: '123 Main St, Miami, FL');
  final _descriptionController = TextEditingController(
    text:
        'Concesionario líder en vehículos premium con más de 20 años de experiencia.',
  );

  // Business hours
  final Map<String, Map<String, dynamic>> _businessHours = {
    'Lunes': {'open': '09:00 AM', 'close': '06:00 PM', 'enabled': true},
    'Martes': {'open': '09:00 AM', 'close': '06:00 PM', 'enabled': true},
    'Miércoles': {'open': '09:00 AM', 'close': '06:00 PM', 'enabled': true},
    'Jueves': {'open': '09:00 AM', 'close': '06:00 PM', 'enabled': true},
    'Viernes': {'open': '09:00 AM', 'close': '06:00 PM', 'enabled': true},
    'Sábado': {'open': '10:00 AM', 'close': '02:00 PM', 'enabled': true},
    'Domingo': {'open': '', 'close': '', 'enabled': false},
  };

  // Photos
  final List<String> _showroomPhotos = [
    'photo1.jpg',
    'photo2.jpg',
    'photo3.jpg',
  ];

  // Location
  LatLng _dealerLocation = const LatLng(25.7617, -80.1918); // Miami

  @override
  void dispose() {
    _nameController.dispose();
    _phoneController.dispose();
    _emailController.dispose();
    _addressController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Editar Perfil'),
        actions: [
          TextButton.icon(
            onPressed: _previewProfile,
            icon: const Icon(Icons.preview),
            label: const Text('Vista Previa'),
          ),
          TextButton.icon(
            onPressed: _saveProfile,
            icon: const Icon(Icons.save),
            label: const Text('Guardar'),
          ),
        ],
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            _buildBasicInfoSection(),
            const SizedBox(height: 24),
            _buildBusinessHoursSection(),
            const SizedBox(height: 24),
            _buildLocationSection(),
            const SizedBox(height: 24),
            _buildShowroomPhotosSection(),
            const SizedBox(height: 24),
            _buildCertificationsSection(),
            const SizedBox(height: 40),
          ],
        ),
      ),
    );
  }

  Widget _buildBasicInfoSection() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Información Básica',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _nameController,
              decoration: const InputDecoration(
                labelText: 'Nombre del concesionario *',
                border: OutlineInputBorder(),
                prefixIcon: Icon(Icons.store),
              ),
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Campo requerido';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _phoneController,
              decoration: const InputDecoration(
                labelText: 'Teléfono *',
                border: OutlineInputBorder(),
                prefixIcon: Icon(Icons.phone),
              ),
              keyboardType: TextInputType.phone,
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Campo requerido';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _emailController,
              decoration: const InputDecoration(
                labelText: 'Correo electrónico *',
                border: OutlineInputBorder(),
                prefixIcon: Icon(Icons.email),
              ),
              keyboardType: TextInputType.emailAddress,
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Campo requerido';
                }
                if (!value.contains('@')) {
                  return 'Correo inválido';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _addressController,
              decoration: const InputDecoration(
                labelText: 'Dirección *',
                border: OutlineInputBorder(),
                prefixIcon: Icon(Icons.location_on),
              ),
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Campo requerido';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _descriptionController,
              decoration: const InputDecoration(
                labelText: 'Descripción',
                border: OutlineInputBorder(),
                prefixIcon: Icon(Icons.description),
                hintText: 'Describe tu concesionario...',
              ),
              maxLines: 4,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildBusinessHoursSection() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Horario de Atención',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 16),
            ..._businessHours.entries.map((entry) {
              final day = entry.key;
              final hours = entry.value;
              return Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: _BusinessHourRow(
                  day: day,
                  isEnabled: hours['enabled'] as bool,
                  openTime: hours['open'] as String,
                  closeTime: hours['close'] as String,
                  onEnabledChanged: (value) {
                    setState(() {
                      hours['enabled'] = value;
                    });
                  },
                  onOpenTimeChanged: (value) {
                    setState(() {
                      hours['open'] = value;
                    });
                  },
                  onCloseTimeChanged: (value) {
                    setState(() {
                      hours['close'] = value;
                    });
                  },
                ),
              );
            }),
          ],
        ),
      ),
    );
  }

  Widget _buildLocationSection() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Ubicación',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 16),
            Container(
              height: 200,
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.grey.shade300),
              ),
              child: ClipRRect(
                borderRadius: BorderRadius.circular(12),
                child: GoogleMap(
                  initialCameraPosition: CameraPosition(
                    target: _dealerLocation,
                    zoom: 15,
                  ),
                  markers: {
                    Marker(
                      markerId: const MarkerId('dealer'),
                      position: _dealerLocation,
                    ),
                  },
                  onTap: (position) {
                    setState(() {
                      _dealerLocation = position;
                    });
                  },
                ),
              ),
            ),
            const SizedBox(height: 12),
            Text(
              'Toca el mapa para ajustar la ubicación',
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: Theme.of(context).colorScheme.outline,
                  ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildShowroomPhotosSection() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Fotos del Showroom',
                  style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
                TextButton.icon(
                  onPressed: _addPhoto,
                  icon: const Icon(Icons.add_photo_alternate),
                  label: const Text('Agregar'),
                ),
              ],
            ),
            const SizedBox(height: 16),
            GridView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 3,
                crossAxisSpacing: 12,
                mainAxisSpacing: 12,
              ),
              itemCount: _showroomPhotos.length,
              itemBuilder: (context, index) {
                return Stack(
                  children: [
                    Container(
                      decoration: BoxDecoration(
                        color: Colors.grey.shade300,
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: const Center(
                        child: Icon(Icons.image, size: 40),
                      ),
                    ),
                    Positioned(
                      top: 4,
                      right: 4,
                      child: IconButton.filled(
                        icon: const Icon(Icons.close, size: 16),
                        onPressed: () {
                          setState(() {
                            _showroomPhotos.removeAt(index);
                          });
                        },
                        style: IconButton.styleFrom(
                          backgroundColor: Colors.red,
                          foregroundColor: Colors.white,
                          padding: const EdgeInsets.all(4),
                        ),
                      ),
                    ),
                  ],
                );
              },
            ),
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.blue.shade50,
                borderRadius: BorderRadius.circular(8),
              ),
              child: Row(
                children: [
                  Icon(Icons.info, size: 16, color: Colors.blue.shade700),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      'Agrega fotos de tu showroom para atraer más clientes',
                      style: TextStyle(
                        fontSize: 12,
                        color: Colors.blue.shade700,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCertificationsSection() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Certificaciones y Premios',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 16),
            _CertificationItem(
              title: 'Dealer Certificado Mercedes-Benz',
              icon: Icons.verified,
              onTap: () {},
            ),
            _CertificationItem(
              title: 'Premio Excelencia 2023',
              icon: Icons.emoji_events,
              onTap: () {},
            ),
            const SizedBox(height: 12),
            OutlinedButton.icon(
              onPressed: () {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Agregar certificación')),
                );
              },
              icon: const Icon(Icons.add),
              label: const Text('Agregar certificación'),
            ),
          ],
        ),
      ),
    );
  }

  void _addPhoto() {
    setState(() {
      _showroomPhotos.add('new_photo.jpg');
    });
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Foto agregada')),
    );
  }

  void _previewProfile() {
    showDialog(
      context: context,
      builder: (context) => Dialog(
        child: Container(
          padding: const EdgeInsets.all(16),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    'Vista Previa',
                    style: Theme.of(context).textTheme.titleLarge?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.close),
                    onPressed: () => Navigator.pop(context),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              ListTile(
                leading: const CircleAvatar(
                  child: Icon(Icons.store),
                ),
                title: Text(_nameController.text),
                subtitle: Text(_phoneController.text),
              ),
              const SizedBox(height: 8),
              Text(_descriptionController.text),
            ],
          ),
        ),
      ),
    );
  }

  void _saveProfile() {
    if (_formKey.currentState!.validate()) {
      showDialog(
        context: context,
        builder: (context) => AlertDialog(
          icon: const Icon(Icons.check_circle, color: Colors.green, size: 48),
          title: const Text('¡Perfil actualizado!'),
          content: const Text('Los cambios han sido guardados correctamente.'),
          actions: [
            FilledButton(
              onPressed: () {
                Navigator.pop(context);
                Navigator.pop(context);
              },
              child: const Text('OK'),
            ),
          ],
        ),
      );
    }
  }
}

class _BusinessHourRow extends StatelessWidget {
  final String day;
  final bool isEnabled;
  final String openTime;
  final String closeTime;
  final ValueChanged<bool> onEnabledChanged;
  final ValueChanged<String> onOpenTimeChanged;
  final ValueChanged<String> onCloseTimeChanged;

  const _BusinessHourRow({
    required this.day,
    required this.isEnabled,
    required this.openTime,
    required this.closeTime,
    required this.onEnabledChanged,
    required this.onOpenTimeChanged,
    required this.onCloseTimeChanged,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        SizedBox(
          width: 80,
          child: Text(
            day,
            style: const TextStyle(fontWeight: FontWeight.w500),
          ),
        ),
        Switch(
          value: isEnabled,
          onChanged: onEnabledChanged,
        ),
        if (isEnabled) ...[
          const SizedBox(width: 8),
          Expanded(
            child: Row(
              children: [
                Expanded(
                  child: OutlinedButton(
                    onPressed: () {},
                    child: Text(openTime),
                  ),
                ),
                const Padding(
                  padding: EdgeInsets.symmetric(horizontal: 8),
                  child: Text('-'),
                ),
                Expanded(
                  child: OutlinedButton(
                    onPressed: () {},
                    child: Text(closeTime),
                  ),
                ),
              ],
            ),
          ),
        ] else
          const Expanded(
            child: Text('Cerrado', style: TextStyle(color: Colors.grey)),
          ),
      ],
    );
  }
}

class _CertificationItem extends StatelessWidget {
  final String title;
  final IconData icon;
  final VoidCallback onTap;

  const _CertificationItem({
    required this.title,
    required this.icon,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: Icon(icon, color: Colors.amber),
      title: Text(title),
      trailing: IconButton(
        icon: const Icon(Icons.delete, color: Colors.red),
        onPressed: () {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Eliminar: $title')),
          );
        },
      ),
      onTap: onTap,
    );
  }
}

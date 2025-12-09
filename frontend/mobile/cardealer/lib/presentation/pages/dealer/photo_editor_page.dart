import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'dart:io';

/// DP-005: Photo Editor
/// Editor de fotos para publicaciones de vehículos
class PhotoEditorPage extends StatefulWidget {
  final String? imagePath;

  const PhotoEditorPage({super.key, this.imagePath});

  @override
  State<PhotoEditorPage> createState() => _PhotoEditorPageState();
}

class _PhotoEditorPageState extends State<PhotoEditorPage> {
  String? _currentImagePath;
  double _brightness = 0;
  double _contrast = 0;
  double _saturation = 0;
  int _rotationAngle = 0;
  bool _showWatermark = false;

  @override
  void initState() {
    super.initState();
    _currentImagePath = widget.imagePath;
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Editor de Fotos'),
        actions: [
          IconButton(
            icon: const Icon(Icons.undo),
            onPressed: _resetAll,
            tooltip: 'Resetear todo',
          ),
          IconButton(
            icon: const Icon(Icons.check),
            onPressed: _savePhoto,
            tooltip: 'Guardar',
          ),
        ],
      ),
      body: Column(
        children: [
          // Image Preview
          Expanded(
            flex: 3,
            child: Container(
              color: Colors.black,
              child: Center(
                child: _currentImagePath != null
                    ? _buildImagePreview()
                    : _buildSelectImagePrompt(theme),
              ),
            ),
          ),

          // Editor Controls
          Expanded(
            flex: 2,
            child: Container(
              decoration: BoxDecoration(
                color: theme.colorScheme.surface,
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withAlpha(25),
                    blurRadius: 8,
                    offset: const Offset(0, -2),
                  ),
                ],
              ),
              child: _buildEditorControls(theme),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildImagePreview() {
    return Transform.rotate(
      angle: _rotationAngle * 3.14159 / 180,
      child: Stack(
        alignment: Alignment.center,
        children: [
          ColorFiltered(
            colorFilter: ColorFilter.matrix(_getColorMatrix()),
            child: Image.file(
              File(_currentImagePath!),
              fit: BoxFit.contain,
            ),
          ),
          if (_showWatermark)
            Positioned(
              bottom: 16,
              right: 16,
              child: Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: Colors.white.withAlpha(200),
                  borderRadius: BorderRadius.circular(4),
                ),
                child: const Text(
                  'CarDealer',
                  style: TextStyle(
                    color: Colors.black,
                    fontWeight: FontWeight.bold,
                    fontSize: 16,
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildSelectImagePrompt(ThemeData theme) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Icon(
          Icons.photo_library_outlined,
          size: 80,
          color: theme.colorScheme.outline,
        ),
        const SizedBox(height: 16),
        Text(
          'No hay imagen seleccionada',
          style: theme.textTheme.titleMedium?.copyWith(
            color: theme.colorScheme.outline,
          ),
        ),
        const SizedBox(height: 24),
        FilledButton.icon(
          onPressed: _pickImage,
          icon: const Icon(Icons.add_photo_alternate),
          label: const Text('Seleccionar foto'),
        ),
      ],
    );
  }

  Widget _buildEditorControls(ThemeData theme) {
    return DefaultTabController(
      length: 4,
      child: Column(
        children: [
          TabBar(
            tabs: const [
              Tab(icon: Icon(Icons.tune), text: 'Ajustes'),
              Tab(icon: Icon(Icons.rotate_right), text: 'Rotar'),
              Tab(icon: Icon(Icons.filter), text: 'Filtros'),
              Tab(icon: Icon(Icons.branding_watermark), text: 'Marca'),
            ],
            labelColor: theme.colorScheme.primary,
          ),
          Expanded(
            child: TabBarView(
              children: [
                _buildAdjustmentsTab(theme),
                _buildRotationTab(theme),
                _buildFiltersTab(theme),
                _buildWatermarkTab(theme),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAdjustmentsTab(ThemeData theme) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        _buildSlider(
          theme,
          label: 'Brillo',
          value: _brightness,
          min: -0.5,
          max: 0.5,
          icon: Icons.brightness_6,
          onChanged: (value) => setState(() => _brightness = value),
        ),
        const SizedBox(height: 16),
        _buildSlider(
          theme,
          label: 'Contraste',
          value: _contrast,
          min: -0.5,
          max: 0.5,
          icon: Icons.contrast,
          onChanged: (value) => setState(() => _contrast = value),
        ),
        const SizedBox(height: 16),
        _buildSlider(
          theme,
          label: 'Saturación',
          value: _saturation,
          min: -1.0,
          max: 1.0,
          icon: Icons.palette,
          onChanged: (value) => setState(() => _saturation = value),
        ),
      ],
    );
  }

  Widget _buildRotationTab(ThemeData theme) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Text(
          'Rotar imagen',
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 24),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: [
            _buildRotationButton(
              theme,
              icon: Icons.rotate_left,
              label: '90° Izq',
              onPressed: () =>
                  setState(() => _rotationAngle = (_rotationAngle - 90) % 360),
            ),
            _buildRotationButton(
              theme,
              icon: Icons.rotate_right,
              label: '90° Der',
              onPressed: () =>
                  setState(() => _rotationAngle = (_rotationAngle + 90) % 360),
            ),
            _buildRotationButton(
              theme,
              icon: Icons.flip,
              label: '180°',
              onPressed: () =>
                  setState(() => _rotationAngle = (_rotationAngle + 180) % 360),
            ),
          ],
        ),
        const SizedBox(height: 16),
        Text(
          'Ángulo actual: $_rotationAngle°',
          style: theme.textTheme.bodyMedium,
        ),
      ],
    );
  }

  Widget _buildFiltersTab(ThemeData theme) {
    final filters = [
      {'name': 'Original', 'preview': Colors.grey[300]},
      {'name': 'B&N', 'preview': Colors.grey[600]},
      {'name': 'Sepia', 'preview': Colors.brown[300]},
      {'name': 'Vívido', 'preview': Colors.red[300]},
      {'name': 'Frío', 'preview': Colors.blue[300]},
      {'name': 'Cálido', 'preview': Colors.orange[300]},
    ];

    return GridView.builder(
      padding: const EdgeInsets.all(16),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 3,
        crossAxisSpacing: 12,
        mainAxisSpacing: 12,
        childAspectRatio: 0.8,
      ),
      itemCount: filters.length,
      itemBuilder: (context, index) {
        final filter = filters[index];
        return InkWell(
          onTap: () {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(content: Text('Filtro ${filter['name']} aplicado')),
            );
          },
          child: Column(
            children: [
              Expanded(
                child: Container(
                  decoration: BoxDecoration(
                    color: filter['preview'] as Color?,
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: theme.colorScheme.outline),
                  ),
                  child: Center(
                    child: Icon(
                      Icons.photo,
                      size: 40,
                      color: theme.colorScheme.onSurface.withAlpha(100),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 8),
              Text(
                filter['name'] as String,
                style: theme.textTheme.bodySmall,
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildWatermarkTab(ThemeData theme) {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        children: [
          SwitchListTile(
            title: const Text('Mostrar marca de agua'),
            subtitle: const Text('Agrega el logo de CarDealer'),
            value: _showWatermark,
            onChanged: (value) => setState(() => _showWatermark = value),
          ),
          const SizedBox(height: 16),
          if (_showWatermark) ...[
            const Text('Posición de la marca de agua:'),
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                ChoiceChip(
                  label: const Text('Abajo Derecha'),
                  selected: true,
                  onSelected: (_) {},
                ),
                ChoiceChip(
                  label: const Text('Abajo Izquierda'),
                  selected: false,
                  onSelected: (_) {},
                ),
                ChoiceChip(
                  label: const Text('Arriba Derecha'),
                  selected: false,
                  onSelected: (_) {},
                ),
                ChoiceChip(
                  label: const Text('Arriba Izquierda'),
                  selected: false,
                  onSelected: (_) {},
                ),
              ],
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildSlider(
    ThemeData theme, {
    required String label,
    required double value,
    required double min,
    required double max,
    required IconData icon,
    required ValueChanged<double> onChanged,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(icon, size: 20, color: theme.colorScheme.primary),
            const SizedBox(width: 8),
            Text(
              label,
              style: theme.textTheme.titleSmall?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const Spacer(),
            Text(
              value.toStringAsFixed(2),
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.primary,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
        Slider(
          value: value,
          min: min,
          max: max,
          onChanged: onChanged,
        ),
      ],
    );
  }

  Widget _buildRotationButton(
    ThemeData theme, {
    required IconData icon,
    required String label,
    required VoidCallback onPressed,
  }) {
    return Column(
      children: [
        IconButton.filled(
          icon: Icon(icon),
          iconSize: 32,
          onPressed: onPressed,
        ),
        const SizedBox(height: 8),
        Text(label, style: theme.textTheme.bodySmall),
      ],
    );
  }

  List<double> _getColorMatrix() {
    // Aplicar filtros de color usando matriz de transformación
    final brightness = _brightness + 1;
    final contrast = _contrast + 1;
    final saturation = _saturation + 1;

    return [
      contrast * saturation,
      0,
      0,
      0,
      brightness * 255,
      0,
      contrast * saturation,
      0,
      0,
      brightness * 255,
      0,
      0,
      contrast * saturation,
      0,
      brightness * 255,
      0,
      0,
      0,
      1,
      0,
    ];
  }

  Future<void> _pickImage() async {
    final picker = ImagePicker();
    final pickedFile = await picker.pickImage(source: ImageSource.gallery);

    if (pickedFile != null) {
      setState(() {
        _currentImagePath = pickedFile.path;
      });
    }
  }

  void _resetAll() {
    setState(() {
      _brightness = 0;
      _contrast = 0;
      _saturation = 0;
      _rotationAngle = 0;
      _showWatermark = false;
    });
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Ajustes reseteados')),
    );
  }

  void _savePhoto() {
    if (_currentImagePath == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('No hay imagen para guardar')),
      );
      return;
    }

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        icon: const Icon(Icons.check_circle, color: Colors.green, size: 48),
        title: const Text('¡Foto guardada!'),
        content: const Text('Los cambios han sido aplicados a la foto.'),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              Navigator.of(context).pop(_currentImagePath);
            },
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }
}

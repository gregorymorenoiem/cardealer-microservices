import 'package:flutter/material.dart';

/// PE-005: Appearance Settings (Sprint 11)
/// Personalización de tema, tamaño de fuente e idioma
class AppearanceSettingsPage extends StatefulWidget {
  const AppearanceSettingsPage({super.key});

  @override
  State<AppearanceSettingsPage> createState() => _AppearanceSettingsPageState();
}

class _AppearanceSettingsPageState extends State<AppearanceSettingsPage> {
  // Theme mode
  ThemeMode _themeMode = ThemeMode.system;

  // Font size
  double _fontSize = 1.0; // 0.8 = small, 1.0 = medium, 1.2 = large, 1.4 = xl

  // Language
  String _selectedLanguage = 'es';

  // Accent color (optional feature)
  Color _accentColor = Colors.blue;

  bool _isSaving = false;

  final List<Map<String, dynamic>> _languages = [
    {'code': 'es', 'name': 'Español', 'flag': '🇪🇸'},
    {'code': 'en', 'name': 'English', 'flag': '🇺🇸'},
    {'code': 'pt', 'name': 'Português', 'flag': '🇧🇷'},
    {'code': 'fr', 'name': 'Français', 'flag': '🇫🇷'},
  ];

  final List<Color> _accentColors = [
    Colors.blue,
    Colors.purple,
    Colors.green,
    Colors.orange,
    Colors.red,
    Colors.teal,
  ];

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Apariencia'),
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
            // Preview Section
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: theme.colorScheme.surfaceContainerHighest,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: theme.colorScheme.outline.withAlpha(50),
                ),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Icon(Icons.preview, color: theme.colorScheme.primary),
                      const SizedBox(width: 8),
                      const Text(
                        'Vista Previa',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 16),
                  _PreviewCard(fontSize: _fontSize, accentColor: _accentColor),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Theme Section
            const _SectionHeader(
              icon: Icons.palette,
              title: 'Tema',
              subtitle: 'Elige el modo de color',
            ),
            const SizedBox(height: 16),

            Card(
              child: RadioGroup<ThemeMode>(
                groupValue: _themeMode,
                onChanged: (value) {
                  setState(() {
                    _themeMode = value!;
                  });
                },
                child: Column(
                  children: [
                    ListTile(
                      leading: const Radio<ThemeMode>(
                        value: ThemeMode.light,
                      ),
                      title: const Text('Modo Claro'),
                      subtitle: const Text('Fondo blanco, texto oscuro'),
                      trailing: Icon(
                        Icons.light_mode,
                        color: _themeMode == ThemeMode.light
                            ? Colors.orange
                            : null,
                      ),
                      onTap: () {
                        setState(() {
                          _themeMode = ThemeMode.light;
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<ThemeMode>(
                        value: ThemeMode.dark,
                      ),
                      title: const Text('Modo Oscuro'),
                      subtitle: const Text('Fondo oscuro, texto claro'),
                      trailing: Icon(
                        Icons.dark_mode,
                        color:
                            _themeMode == ThemeMode.dark ? Colors.blue : null,
                      ),
                      onTap: () {
                        setState(() {
                          _themeMode = ThemeMode.dark;
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<ThemeMode>(
                        value: ThemeMode.system,
                      ),
                      title: const Text('Automático'),
                      subtitle:
                          const Text('Sigue la configuración del sistema'),
                      trailing: Icon(
                        Icons.brightness_auto,
                        color: _themeMode == ThemeMode.system
                            ? Colors.purple
                            : null,
                      ),
                      onTap: () {
                        setState(() {
                          _themeMode = ThemeMode.system;
                        });
                      },
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Font Size Section
            const _SectionHeader(
              icon: Icons.text_fields,
              title: 'Tamaño de Texto',
              subtitle: 'Ajusta el tamaño de la fuente',
            ),
            const SizedBox(height: 16),

            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          'Pequeño',
                          style: TextStyle(
                            fontSize: 14 * 0.8,
                            color: _fontSize == 0.8
                                ? theme.colorScheme.primary
                                : null,
                            fontWeight:
                                _fontSize == 0.8 ? FontWeight.bold : null,
                          ),
                        ),
                        Text(
                          'Mediano',
                          style: TextStyle(
                            fontSize: 14,
                            color: _fontSize == 1.0
                                ? theme.colorScheme.primary
                                : null,
                            fontWeight:
                                _fontSize == 1.0 ? FontWeight.bold : null,
                          ),
                        ),
                        Text(
                          'Grande',
                          style: TextStyle(
                            fontSize: 14 * 1.2,
                            color: _fontSize == 1.2
                                ? theme.colorScheme.primary
                                : null,
                            fontWeight:
                                _fontSize == 1.2 ? FontWeight.bold : null,
                          ),
                        ),
                        Text(
                          'Muy Grande',
                          style: TextStyle(
                            fontSize: 14 * 1.4,
                            color: _fontSize == 1.4
                                ? theme.colorScheme.primary
                                : null,
                            fontWeight:
                                _fontSize == 1.4 ? FontWeight.bold : null,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    Slider(
                      value: _fontSize,
                      min: 0.8,
                      max: 1.4,
                      divisions: 3,
                      label: _getFontSizeLabel(),
                      onChanged: (value) {
                        setState(() {
                          _fontSize = value;
                        });
                      },
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Accent Color Section
            const _SectionHeader(
              icon: Icons.color_lens,
              title: 'Color de Acento',
              subtitle: 'Personaliza el color principal',
            ),
            const SizedBox(height: 16),

            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Wrap(
                  spacing: 12,
                  runSpacing: 12,
                  children: _accentColors.map((color) {
                    final isSelected = _accentColor == color;
                    return InkWell(
                      onTap: () {
                        setState(() {
                          _accentColor = color;
                        });
                      },
                      child: Container(
                        width: 50,
                        height: 50,
                        decoration: BoxDecoration(
                          color: color,
                          shape: BoxShape.circle,
                          border: Border.all(
                            color:
                                isSelected ? Colors.white : Colors.transparent,
                            width: 3,
                          ),
                          boxShadow: isSelected
                              ? [
                                  BoxShadow(
                                    color: color.withAlpha(100),
                                    blurRadius: 8,
                                    spreadRadius: 2,
                                  ),
                                ]
                              : null,
                        ),
                        child: isSelected
                            ? const Icon(Icons.check,
                                color: Colors.white, size: 28)
                            : null,
                      ),
                    );
                  }).toList(),
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Language Section
            const _SectionHeader(
              icon: Icons.language,
              title: 'Idioma',
              subtitle: 'Cambia el idioma de la aplicación',
            ),
            const SizedBox(height: 16),

            Card(
              child: RadioGroup<String>(
                groupValue: _selectedLanguage,
                onChanged: (value) {
                  setState(() {
                    _selectedLanguage = value!;
                  });
                },
                child: Column(
                  children: _languages.map((lang) {
                    final isLast = lang == _languages.last;
                    return Column(
                      children: [
                        ListTile(
                          leading: Radio<String>(
                            value: lang['code'] as String,
                          ),
                          title: Text('${lang['flag']} ${lang['name']}'),
                          trailing: _selectedLanguage == lang['code']
                              ? const Icon(Icons.check_circle,
                                  color: Colors.green)
                              : null,
                          onTap: () {
                            setState(() {
                              _selectedLanguage = lang['code'] as String;
                            });
                          },
                        ),
                        if (!isLast) const Divider(height: 1),
                      ],
                    );
                  }).toList(),
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Additional Options
            const _SectionHeader(
              icon: Icons.tune,
              title: 'Opciones Adicionales',
              subtitle: 'Otras preferencias de visualización',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  SwitchListTile(
                    value: true,
                    onChanged: (value) {
                      // TODO: Implement compact mode
                    },
                    title: const Text('Modo Compacto'),
                    subtitle: const Text('Reduce el espaciado entre elementos'),
                    secondary: const Icon(Icons.view_compact),
                  ),
                  const Divider(height: 1),
                  SwitchListTile(
                    value: false,
                    onChanged: (value) {
                      // TODO: Implement high contrast
                    },
                    title: const Text('Alto Contraste'),
                    subtitle: const Text('Mejora la legibilidad'),
                    secondary: const Icon(Icons.contrast),
                  ),
                  const Divider(height: 1),
                  SwitchListTile(
                    value: true,
                    onChanged: (value) {
                      // TODO: Implement animations
                    },
                    title: const Text('Animaciones'),
                    subtitle: const Text('Mostrar transiciones animadas'),
                    secondary: const Icon(Icons.animation),
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
                    ? const Text('Aplicando...')
                    : const Text('Aplicar Cambios'),
              ),
            ),
            const SizedBox(height: 16),

            // Reset Button
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                onPressed: _resetToDefaults,
                icon: const Icon(Icons.restore),
                label: const Text('Restaurar Valores Predeterminados'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  String _getFontSizeLabel() {
    if (_fontSize == 0.8) return 'Pequeño';
    if (_fontSize == 1.0) return 'Mediano';
    if (_fontSize == 1.2) return 'Grande';
    if (_fontSize == 1.4) return 'Muy Grande';
    return 'Personalizado';
  }

  Future<void> _saveSettings() async {
    setState(() {
      _isSaving = true;
    });

    // Simulate API call and theme application
    await Future.delayed(const Duration(seconds: 2));

    if (mounted) {
      setState(() {
        _isSaving = false;
      });

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Configuración aplicada correctamente'),
          backgroundColor: Colors.green,
        ),
      );
    }
  }

  void _resetToDefaults() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Restaurar Valores'),
        content: const Text(
          '¿Estás seguro de que deseas restaurar todos los valores predeterminados?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () {
              setState(() {
                _themeMode = ThemeMode.system;
                _fontSize = 1.0;
                _selectedLanguage = 'es';
                _accentColor = Colors.blue;
              });
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Valores restaurados'),
                ),
              );
            },
            child: const Text('Restaurar'),
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

/// Preview card widget
class _PreviewCard extends StatelessWidget {
  final double fontSize;
  final Color accentColor;

  const _PreviewCard({
    required this.fontSize,
    required this.accentColor,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surface,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(
          color: Theme.of(context).colorScheme.outline.withAlpha(50),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  color: accentColor,
                  borderRadius: BorderRadius.circular(8),
                ),
                child: const Icon(Icons.directions_car, color: Colors.white),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Toyota Camry 2024',
                      style: TextStyle(
                        fontSize: 16 * fontSize,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    Text(
                      'Sedán • 2024 • 25,000 km',
                      style: TextStyle(
                        fontSize: 12 * fontSize,
                        color: Colors.grey.shade600,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Text(
            '\$32,500',
            style: TextStyle(
              fontSize: 20 * fontSize,
              fontWeight: FontWeight.bold,
              color: accentColor,
            ),
          ),
          const SizedBox(height: 8),
          FilledButton(
            onPressed: () {},
            style: FilledButton.styleFrom(
              backgroundColor: accentColor,
            ),
            child: Text(
              'Ver Detalles',
              style: TextStyle(fontSize: 14 * fontSize),
            ),
          ),
        ],
      ),
    );
  }
}

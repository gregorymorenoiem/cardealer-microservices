import 'package:flutter/material.dart';
import 'package:cardealer_mobile/core/performance/performance_monitor.dart';
import 'package:cardealer_mobile/core/performance/app_size_optimizer.dart';

/// Página de configuración de performance
class PerformanceSettingsPage extends StatefulWidget {
  const PerformanceSettingsPage({super.key});

  @override
  State<PerformanceSettingsPage> createState() =>
      _PerformanceSettingsPageState();
}

class _PerformanceSettingsPageState extends State<PerformanceSettingsPage> {
  final _optimizer = AppSizeOptimizer();
  String _cacheSize = 'Calculando...';
  bool _isCleaningCache = false;

  @override
  void initState() {
    super.initState();
    _loadCacheSize();
  }

  Future<void> _loadCacheSize() async {
    final size = await _optimizer.getCacheSize();
    setState(() {
      _cacheSize = _formatBytes(size);
    });
  }

  Future<void> _cleanCache() async {
    setState(() => _isCleaningCache = true);

    try {
      final result = await _optimizer.performFullCleanup();

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              'Limpieza completada: ${result.totalFreedFormatted} liberados',
            ),
            backgroundColor: Colors.green,
          ),
        );

        await _loadCacheSize();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Error al limpiar caché: $e'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isCleaningCache = false);
      }
    }
  }

  Future<void> _showStorageInfo() async {
    final info = await _optimizer.getStorageInfo();

    if (!mounted) return;

    await showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Información de Almacenamiento'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildStorageInfoRow('Caché', info.cacheSizeFormatted),
            _buildStorageInfoRow('Temporales', info.tempSizeFormatted),
            _buildStorageInfoRow('Documentos', info.documentsSizeFormatted),
            const Divider(),
            _buildStorageInfoRow('Total', info.totalSizeFormatted,
                isBold: true),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cerrar'),
          ),
        ],
      ),
    );
  }

  Widget _buildStorageInfoRow(String label, String value,
      {bool isBold = false}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(
              fontWeight: isBold ? FontWeight.bold : FontWeight.normal,
            ),
          ),
          Text(
            value,
            style: TextStyle(
              fontWeight: isBold ? FontWeight.bold : FontWeight.normal,
            ),
          ),
        ],
      ),
    );
  }

  String _formatBytes(int bytes) {
    if (bytes < 1024) return '$bytes B';
    if (bytes < 1024 * 1024) return '${(bytes / 1024).toStringAsFixed(2)} KB';
    if (bytes < 1024 * 1024 * 1024) {
      return '${(bytes / (1024 * 1024)).toStringAsFixed(2)} MB';
    }
    return '${(bytes / (1024 * 1024 * 1024)).toStringAsFixed(2)} GB';
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Optimización'),
        elevation: 0,
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // Cache Section
          Card(
            child: Column(
              children: [
                ListTile(
                  leading: const Icon(Icons.storage),
                  title: const Text('Almacenamiento'),
                  subtitle: Text('Tamaño en caché: $_cacheSize'),
                  trailing: IconButton(
                    icon: const Icon(Icons.info_outline),
                    onPressed: _showStorageInfo,
                  ),
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.cleaning_services),
                  title: const Text('Limpiar caché'),
                  subtitle: const Text('Elimina archivos temporales y caché'),
                  trailing: _isCleaningCache
                      ? const SizedBox(
                          width: 24,
                          height: 24,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : null,
                  onTap: _isCleaningCache ? null : _cleanCache,
                ),
              ],
            ),
          ),

          const SizedBox(height: 16),

          // Performance Metrics
          Card(
            child: Column(
              children: [
                const ListTile(
                  leading: Icon(Icons.speed),
                  title: Text('Métricas de Performance'),
                  subtitle: Text('Ver estadísticas de rendimiento'),
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.analytics),
                  title: const Text('Ver métricas'),
                  onTap: _showPerformanceMetrics,
                ),
              ],
            ),
          ),

          const SizedBox(height: 16),

          // Tips
          Card(
            color: Colors.blue[50],
            child: const Padding(
              padding: EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Icon(Icons.lightbulb_outline, color: Colors.blue),
                      SizedBox(width: 8),
                      Text(
                        'Consejos de optimización',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          color: Colors.blue,
                        ),
                      ),
                    ],
                  ),
                  SizedBox(height: 12),
                  Text(
                    '• Limpia la caché regularmente para liberar espacio\n'
                    '• Las imágenes se optimizan automáticamente\n'
                    '• El rendimiento mejora con el uso constante',
                    style: TextStyle(fontSize: 14),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  void _showPerformanceMetrics() {
    final monitor = PerformanceMonitor();
    final report = monitor.generateReport();

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Métricas de Performance'),
        content: SingleChildScrollView(
          child: Text(
            report,
            style: const TextStyle(fontFamily: 'monospace', fontSize: 12),
          ),
        ),
        actions: [
          TextButton(
            onPressed: () {
              monitor.clear();
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Métricas limpiadas'),
                ),
              );
            },
            child: const Text('Limpiar'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cerrar'),
          ),
        ],
      ),
    );
  }
}

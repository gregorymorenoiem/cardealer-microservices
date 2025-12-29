import 'dart:io';
import 'package:flutter/foundation.dart';
import 'package:path_provider/path_provider.dart';

/// Optimizador de tamaño de aplicación
class AppSizeOptimizer {
  static final AppSizeOptimizer _instance = AppSizeOptimizer._internal();
  factory AppSizeOptimizer() => _instance;
  AppSizeOptimizer._internal();

  /// Limpia archivos temporales y caché
  Future<int> cleanTemporaryFiles() async {
    var totalFreed = 0;

    try {
      final tempDir = await getTemporaryDirectory();
      if (await tempDir.exists()) {
        final size = await _getDirectorySize(tempDir);
        await tempDir.delete(recursive: true);
        await tempDir.create();
        totalFreed += size;
        debugPrint('Cleaned ${_formatBytes(size)} from temporary files');
      }
    } catch (e) {
      debugPrint('Error cleaning temporary files: $e');
    }

    return totalFreed;
  }

  /// Limpia caché de aplicación
  Future<int> cleanAppCache() async {
    var totalFreed = 0;

    try {
      final cacheDir = await getApplicationCacheDirectory();
      if (await cacheDir.exists()) {
        final files = await cacheDir.list(recursive: true).toList();

        for (final file in files) {
          if (file is File) {
            try {
              final stat = await file.stat();
              final size = stat.size;
              await file.delete();
              totalFreed += size;
            } catch (e) {
              debugPrint('Error deleting cache file: $e');
            }
          }
        }

        debugPrint('Cleaned ${_formatBytes(totalFreed)} from app cache');
      }
    } catch (e) {
      debugPrint('Error cleaning app cache: $e');
    }

    return totalFreed;
  }

  /// Obtiene el tamaño de un directorio
  Future<int> _getDirectorySize(Directory directory) async {
    var totalSize = 0;

    try {
      if (await directory.exists()) {
        final files = await directory.list(recursive: true).toList();

        for (final file in files) {
          if (file is File) {
            final stat = await file.stat();
            totalSize += stat.size;
          }
        }
      }
    } catch (e) {
      debugPrint('Error getting directory size: $e');
    }

    return totalSize;
  }

  /// Obtiene el tamaño de caché actual
  Future<int> getCacheSize() async {
    var totalSize = 0;

    try {
      final cacheDir = await getApplicationCacheDirectory();
      totalSize += await _getDirectorySize(cacheDir);

      final tempDir = await getTemporaryDirectory();
      totalSize += await _getDirectorySize(tempDir);
    } catch (e) {
      debugPrint('Error getting cache size: $e');
    }

    return totalSize;
  }

  /// Obtiene información de almacenamiento
  Future<StorageInfo> getStorageInfo() async {
    final cacheDir = await getApplicationCacheDirectory();
    final tempDir = await getTemporaryDirectory();
    final documentsDir = await getApplicationDocumentsDirectory();

    final cacheSize = await _getDirectorySize(cacheDir);
    final tempSize = await _getDirectorySize(tempDir);
    final documentsSize = await _getDirectorySize(documentsDir);
    final totalSize = cacheSize + tempSize + documentsSize;

    return StorageInfo(
      cacheSize: cacheSize,
      tempSize: tempSize,
      documentsSize: documentsSize,
      totalSize: totalSize,
    );
  }

  /// Limpia archivos antiguos (más de X días)
  Future<int> cleanOldFiles(int daysOld) async {
    var totalFreed = 0;

    try {
      final cacheDir = await getApplicationCacheDirectory();
      final now = DateTime.now();

      if (await cacheDir.exists()) {
        final files = await cacheDir.list(recursive: true).toList();

        for (final file in files) {
          if (file is File) {
            try {
              final stat = await file.stat();
              final age = now.difference(stat.modified).inDays;

              if (age > daysOld) {
                final size = stat.size;
                await file.delete();
                totalFreed += size;
              }
            } catch (e) {
              debugPrint('Error deleting old file: $e');
            }
          }
        }

        debugPrint('Cleaned ${_formatBytes(totalFreed)} from old files');
      }
    } catch (e) {
      debugPrint('Error cleaning old files: $e');
    }

    return totalFreed;
  }

  /// Formatea bytes a formato legible
  String _formatBytes(int bytes) {
    if (bytes < 1024) return '$bytes B';
    if (bytes < 1024 * 1024) return '${(bytes / 1024).toStringAsFixed(2)} KB';
    if (bytes < 1024 * 1024 * 1024) {
      return '${(bytes / (1024 * 1024)).toStringAsFixed(2)} MB';
    }
    return '${(bytes / (1024 * 1024 * 1024)).toStringAsFixed(2)} GB';
  }

  /// Realiza una limpieza completa
  Future<CleanupResult> performFullCleanup() async {
    final startTime = DateTime.now();

    final tempFreed = await cleanTemporaryFiles();
    final cacheFreed = await cleanAppCache();
    final oldFilesFreed = await cleanOldFiles(7); // 7 días

    final totalFreed = tempFreed + cacheFreed + oldFilesFreed;
    final duration = DateTime.now().difference(startTime);

    return CleanupResult(
      tempFilesFreed: tempFreed,
      cacheFreed: cacheFreed,
      oldFilesFreed: oldFilesFreed,
      totalFreed: totalFreed,
      duration: duration,
    );
  }
}

/// Información de almacenamiento
class StorageInfo {
  final int cacheSize;
  final int tempSize;
  final int documentsSize;
  final int totalSize;

  const StorageInfo({
    required this.cacheSize,
    required this.tempSize,
    required this.documentsSize,
    required this.totalSize,
  });

  String get cacheSizeFormatted => _formatBytes(cacheSize);
  String get tempSizeFormatted => _formatBytes(tempSize);
  String get documentsSizeFormatted => _formatBytes(documentsSize);
  String get totalSizeFormatted => _formatBytes(totalSize);

  String _formatBytes(int bytes) {
    if (bytes < 1024) return '$bytes B';
    if (bytes < 1024 * 1024) return '${(bytes / 1024).toStringAsFixed(2)} KB';
    if (bytes < 1024 * 1024 * 1024) {
      return '${(bytes / (1024 * 1024)).toStringAsFixed(2)} MB';
    }
    return '${(bytes / (1024 * 1024 * 1024)).toStringAsFixed(2)} GB';
  }

  @override
  String toString() {
    return '''
StorageInfo:
  Cache: $cacheSizeFormatted
  Temp: $tempSizeFormatted
  Documents: $documentsSizeFormatted
  Total: $totalSizeFormatted
''';
  }
}

/// Resultado de limpieza
class CleanupResult {
  final int tempFilesFreed;
  final int cacheFreed;
  final int oldFilesFreed;
  final int totalFreed;
  final Duration duration;

  const CleanupResult({
    required this.tempFilesFreed,
    required this.cacheFreed,
    required this.oldFilesFreed,
    required this.totalFreed,
    required this.duration,
  });

  String get tempFilesFreedFormatted => _formatBytes(tempFilesFreed);
  String get cacheFreedFormatted => _formatBytes(cacheFreed);
  String get oldFilesFreedFormatted => _formatBytes(oldFilesFreed);
  String get totalFreedFormatted => _formatBytes(totalFreed);

  String _formatBytes(int bytes) {
    if (bytes < 1024) return '$bytes B';
    if (bytes < 1024 * 1024) return '${(bytes / 1024).toStringAsFixed(2)} KB';
    if (bytes < 1024 * 1024 * 1024) {
      return '${(bytes / (1024 * 1024)).toStringAsFixed(2)} MB';
    }
    return '${(bytes / (1024 * 1024 * 1024)).toStringAsFixed(2)} GB';
  }

  @override
  String toString() {
    return '''
Cleanup Result:
  Temp Files: $tempFilesFreedFormatted
  Cache: $cacheFreedFormatted
  Old Files: $oldFilesFreedFormatted
  Total Freed: $totalFreedFormatted
  Duration: ${duration.inSeconds}s
''';
  }
}

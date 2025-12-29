import 'dart:io';
import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:path_provider/path_provider.dart';
import 'package:crypto/crypto.dart';

/// Gestor de caché de imágenes optimizado
class ImageCacheManager {
  static final ImageCacheManager _instance = ImageCacheManager._internal();
  factory ImageCacheManager() => _instance;
  ImageCacheManager._internal();

  static const String _cacheDirectoryName = 'image_cache';
  static const int _maxCacheSize = 100 * 1024 * 1024; // 100 MB
  static const int _maxCacheAge = 7; // 7 días

  Directory? _cacheDirectory;
  final Map<String, File> _memoryCache = {};

  /// Inicializa el gestor de caché
  Future<void> initialize() async {
    try {
      final tempDir = await getTemporaryDirectory();
      _cacheDirectory = Directory('${tempDir.path}/$_cacheDirectoryName');

      if (!await _cacheDirectory!.exists()) {
        await _cacheDirectory!.create(recursive: true);
      }

      // Limpiar caché antiguo al inicializar
      await _cleanOldCache();
    } catch (e) {
      debugPrint('Error initializing ImageCacheManager: $e');
    }
  }

  /// Genera un nombre de archivo basado en la URL
  String _getCacheFileName(String url) {
    final bytes = utf8.encode(url);
    final digest = md5.convert(bytes);
    return digest.toString();
  }

  /// Obtiene el archivo de caché para una URL
  File? _getCacheFile(String url) {
    if (_cacheDirectory == null) return null;
    final fileName = _getCacheFileName(url);
    return File('${_cacheDirectory!.path}/$fileName');
  }

  /// Verifica si una URL está en caché
  Future<bool> isCached(String url) async {
    if (_cacheDirectory == null) await initialize();

    final cacheFile = _getCacheFile(url);
    if (cacheFile == null) return false;

    return await cacheFile.exists();
  }

  /// Obtiene una imagen de la caché
  Future<File?> getFromCache(String url) async {
    if (_cacheDirectory == null) await initialize();

    // Verificar caché en memoria primero
    if (_memoryCache.containsKey(url)) {
      final file = _memoryCache[url]!;
      if (await file.exists()) {
        return file;
      } else {
        _memoryCache.remove(url);
      }
    }

    // Verificar caché en disco
    final cacheFile = _getCacheFile(url);
    if (cacheFile == null) return null;

    if (await cacheFile.exists()) {
      // Actualizar tiempo de acceso
      await cacheFile.setLastModified(DateTime.now());

      // Agregar a caché en memoria
      _memoryCache[url] = cacheFile;

      return cacheFile;
    }

    return null;
  }

  /// Guarda una imagen en la caché
  Future<File?> saveToCache(String url, Uint8List bytes) async {
    if (_cacheDirectory == null) await initialize();

    try {
      final cacheFile = _getCacheFile(url);
      if (cacheFile == null) return null;

      // Escribir archivo
      await cacheFile.writeAsBytes(bytes);

      // Agregar a caché en memoria
      _memoryCache[url] = cacheFile;

      // Verificar tamaño de caché
      await _checkCacheSize();

      return cacheFile;
    } catch (e) {
      debugPrint('Error saving to cache: $e');
      return null;
    }
  }

  /// Elimina una imagen de la caché
  Future<void> removeFromCache(String url) async {
    if (_cacheDirectory == null) await initialize();

    try {
      // Eliminar de caché en memoria
      _memoryCache.remove(url);

      // Eliminar de disco
      final cacheFile = _getCacheFile(url);
      if (cacheFile != null && await cacheFile.exists()) {
        await cacheFile.delete();
      }
    } catch (e) {
      debugPrint('Error removing from cache: $e');
    }
  }

  /// Limpia toda la caché
  Future<void> clearCache() async {
    if (_cacheDirectory == null) await initialize();

    try {
      // Limpiar caché en memoria
      _memoryCache.clear();

      // Limpiar caché en disco
      if (_cacheDirectory != null && await _cacheDirectory!.exists()) {
        await _cacheDirectory!.delete(recursive: true);
        await _cacheDirectory!.create(recursive: true);
      }
    } catch (e) {
      debugPrint('Error clearing cache: $e');
    }
  }

  /// Limpia la caché antigua (mayor a _maxCacheAge días)
  Future<void> _cleanOldCache() async {
    if (_cacheDirectory == null) return;

    try {
      final files = await _cacheDirectory!.list().toList();
      final now = DateTime.now();

      for (final file in files) {
        if (file is File) {
          final stat = await file.stat();
          final age = now.difference(stat.modified).inDays;

          if (age > _maxCacheAge) {
            await file.delete();
          }
        }
      }
    } catch (e) {
      debugPrint('Error cleaning old cache: $e');
    }
  }

  /// Verifica el tamaño de la caché y elimina archivos si es necesario
  Future<void> _checkCacheSize() async {
    if (_cacheDirectory == null) return;

    try {
      final files = await _cacheDirectory!.list().toList();
      var totalSize = 0;

      // Calcular tamaño total
      final fileStats = <File, FileStat>{};
      for (final file in files) {
        if (file is File) {
          final stat = await file.stat();
          fileStats[file] = stat;
          totalSize += stat.size;
        }
      }

      // Si excede el límite, eliminar archivos más antiguos
      if (totalSize > _maxCacheSize) {
        final sortedFiles = fileStats.entries.toList()
          ..sort((a, b) => a.value.modified.compareTo(b.value.modified));

        for (final entry in sortedFiles) {
          if (totalSize <= _maxCacheSize * 0.8) break; // Reducir a 80%

          final fileSize = entry.value.size;
          await entry.key.delete();
          totalSize -= fileSize;
        }
      }
    } catch (e) {
      debugPrint('Error checking cache size: $e');
    }
  }

  /// Obtiene el tamaño actual de la caché
  Future<int> getCacheSize() async {
    if (_cacheDirectory == null) await initialize();
    if (_cacheDirectory == null) return 0;

    try {
      final files = await _cacheDirectory!.list().toList();
      var totalSize = 0;

      for (final file in files) {
        if (file is File) {
          final stat = await file.stat();
          totalSize += stat.size;
        }
      }

      return totalSize;
    } catch (e) {
      debugPrint('Error getting cache size: $e');
      return 0;
    }
  }

  /// Obtiene el tamaño de la caché en formato legible
  Future<String> getCacheSizeFormatted() async {
    final size = await getCacheSize();
    return _formatBytes(size);
  }

  /// Formatea bytes a un formato legible
  String _formatBytes(int bytes) {
    if (bytes < 1024) return '$bytes B';
    if (bytes < 1024 * 1024) return '${(bytes / 1024).toStringAsFixed(2)} KB';
    return '${(bytes / (1024 * 1024)).toStringAsFixed(2)} MB';
  }

  /// Precarga una lista de imágenes
  Future<void> precacheImages(List<String> urls) async {
    for (final url in urls) {
      if (!await isCached(url)) {
        // Aquí se podría implementar la descarga y caché de imágenes
        // Por ahora solo verificamos si están en caché
      }
    }
  }
}

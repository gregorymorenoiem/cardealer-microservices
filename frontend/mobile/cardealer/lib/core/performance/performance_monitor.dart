import 'dart:developer' as developer;
import 'package:flutter/foundation.dart';

/// Métrica de performance
class PerformanceMetric {
  final String operationName;
  final Duration duration;
  final DateTime timestamp;

  const PerformanceMetric({
    required this.operationName,
    required this.duration,
    required this.timestamp,
  });

  @override
  String toString() {
    return 'PerformanceMetric(operation: $operationName, duration: ${duration.inMilliseconds}ms, timestamp: $timestamp)';
  }
}

/// Monitor de performance para medir tiempos de carga y rendimiento
class PerformanceMonitor {
  static final PerformanceMonitor _instance = PerformanceMonitor._internal();
  factory PerformanceMonitor() => _instance;
  PerformanceMonitor._internal();

  final Map<String, DateTime> _startTimes = {};
  final Map<String, Duration> _durations = {};
  final List<PerformanceMetric> _metrics = [];

  /// Inicia el seguimiento de una operación
  void startTracking(String operationName) {
    if (kDebugMode) {
      _startTimes[operationName] = DateTime.now();
      developer.log('Started tracking: $operationName', name: 'Performance');
    }
  }

  /// Finaliza el seguimiento de una operación
  void endTracking(String operationName) {
    if (kDebugMode && _startTimes.containsKey(operationName)) {
      final startTime = _startTimes[operationName]!;
      final duration = DateTime.now().difference(startTime);
      _durations[operationName] = duration;

      _metrics.add(PerformanceMetric(
        operationName: operationName,
        duration: duration,
        timestamp: DateTime.now(),
      ));

      developer.log(
        'Completed: $operationName in ${duration.inMilliseconds}ms',
        name: 'Performance',
      );

      _startTimes.remove(operationName);
    }
  }

  /// Registra un tiempo personalizado
  void logDuration(String operationName, Duration duration) {
    if (kDebugMode) {
      _durations[operationName] = duration;
      _metrics.add(PerformanceMetric(
        operationName: operationName,
        duration: duration,
        timestamp: DateTime.now(),
      ));

      developer.log(
        '$operationName: ${duration.inMilliseconds}ms',
        name: 'Performance',
      );
    }
  }

  /// Obtiene la duración de una operación
  Duration? getDuration(String operationName) {
    return _durations[operationName];
  }

  /// Obtiene todas las métricas registradas
  List<PerformanceMetric> getMetrics() {
    return List.unmodifiable(_metrics);
  }

  /// Obtiene métricas filtradas por nombre
  List<PerformanceMetric> getMetricsByOperation(String operationName) {
    return _metrics
        .where((metric) => metric.operationName == operationName)
        .toList();
  }

  /// Obtiene el promedio de duración para una operación
  Duration? getAverageDuration(String operationName) {
    final metrics = getMetricsByOperation(operationName);
    if (metrics.isEmpty) return null;

    final totalMilliseconds = metrics.fold<int>(
      0,
      (sum, metric) => sum + metric.duration.inMilliseconds,
    );

    return Duration(milliseconds: totalMilliseconds ~/ metrics.length);
  }

  /// Limpia todas las métricas
  void clear() {
    _startTimes.clear();
    _durations.clear();
    _metrics.clear();
  }

  /// Genera un reporte de performance
  String generateReport() {
    if (_metrics.isEmpty) {
      return 'No performance metrics available';
    }

    final buffer = StringBuffer();
    buffer.writeln('=== Performance Report ===');
    buffer.writeln('Total metrics: ${_metrics.length}');
    buffer.writeln('');

    final groupedMetrics = <String, List<PerformanceMetric>>{};
    for (final metric in _metrics) {
      groupedMetrics.putIfAbsent(metric.operationName, () => []).add(metric);
    }

    for (final entry in groupedMetrics.entries) {
      final operationName = entry.key;
      final metrics = entry.value;
      final avgDuration = getAverageDuration(operationName);

      buffer.writeln('Operation: $operationName');
      buffer.writeln('  Count: ${metrics.length}');
      buffer.writeln('  Average: ${avgDuration?.inMilliseconds}ms');
      buffer.writeln(
          '  Min: ${metrics.map((m) => m.duration.inMilliseconds).reduce((a, b) => a < b ? a : b)}ms');
      buffer.writeln(
          '  Max: ${metrics.map((m) => m.duration.inMilliseconds).reduce((a, b) => a > b ? a : b)}ms');
      buffer.writeln('');
    }

    return buffer.toString();
  }

  /// Ejecuta una operación y mide su tiempo
  Future<T> measureAsync<T>(
    String operationName,
    Future<T> Function() operation,
  ) async {
    startTracking(operationName);
    try {
      return await operation();
    } finally {
      endTracking(operationName);
    }
  }

  /// Ejecuta una operación síncrona y mide su tiempo
  T measureSync<T>(
    String operationName,
    T Function() operation,
  ) {
    startTracking(operationName);
    try {
      return operation();
    } finally {
      endTracking(operationName);
    }
  }
}

/// Extension para facilitar el uso del monitor
extension PerformanceMonitorExtension<T> on Future<T> Function() {
  Future<T> withPerformanceTracking(String operationName) {
    return PerformanceMonitor().measureAsync(operationName, this);
  }
}

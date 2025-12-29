library;

/// Performance optimization utilities
/// Provides tools for monitoring and improving app performance
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'dart:async';

/// Performance metrics
class PerformanceMetrics {
  final String operation;
  final Duration duration;
  final DateTime timestamp;
  final Map<String, dynamic>? metadata;

  PerformanceMetrics({
    required this.operation,
    required this.duration,
    DateTime? timestamp,
    this.metadata,
  }) : timestamp = timestamp ?? DateTime.now();

  Map<String, dynamic> toJson() => {
        'operation': operation,
        'duration_ms': duration.inMilliseconds,
        'timestamp': timestamp.toIso8601String(),
        if (metadata != null) 'metadata': metadata,
      };
}

/// Performance monitor
class PerformanceMonitor {
  static final PerformanceMonitor _instance = PerformanceMonitor._internal();
  factory PerformanceMonitor() => _instance;
  PerformanceMonitor._internal();

  final List<PerformanceMetrics> _metrics = [];
  final Map<String, Stopwatch> _activeOperations = {};

  List<PerformanceMetrics> get metrics => List.unmodifiable(_metrics);

  void startOperation(String operationName) {
    final stopwatch = Stopwatch()..start();
    _activeOperations[operationName] = stopwatch;
  }

  void endOperation(
    String operationName, {
    Map<String, dynamic>? metadata,
  }) {
    final stopwatch = _activeOperations.remove(operationName);
    if (stopwatch == null) return;

    stopwatch.stop();
    final metric = PerformanceMetrics(
      operation: operationName,
      duration: stopwatch.elapsed,
      metadata: metadata,
    );

    _metrics.add(metric);

    if (kDebugMode && stopwatch.elapsedMilliseconds > 100) {
      debugPrint(
        'SLOW OPERATION: $operationName took ${stopwatch.elapsedMilliseconds}ms',
      );
    }
  }

  Future<T> measure<T>(
    String operationName,
    Future<T> Function() operation, {
    Map<String, dynamic>? metadata,
  }) async {
    startOperation(operationName);
    try {
      return await operation();
    } finally {
      endOperation(operationName, metadata: metadata);
    }
  }

  T measureSync<T>(
    String operationName,
    T Function() operation, {
    Map<String, dynamic>? metadata,
  }) {
    startOperation(operationName);
    try {
      return operation();
    } finally {
      endOperation(operationName, metadata: metadata);
    }
  }

  void clearMetrics() {
    _metrics.clear();
  }

  Map<String, dynamic> getStatistics() {
    final grouped = <String, List<Duration>>{};

    for (final metric in _metrics) {
      grouped.putIfAbsent(metric.operation, () => []).add(metric.duration);
    }

    final stats = <String, dynamic>{};
    grouped.forEach((operation, durations) {
      final sorted = List<Duration>.from(durations)..sort();
      final total = durations.fold<Duration>(
        Duration.zero,
        (a, b) => a + b,
      );

      stats[operation] = {
        'count': durations.length,
        'avg_ms': (total.inMilliseconds / durations.length).round(),
        'min_ms': sorted.first.inMilliseconds,
        'max_ms': sorted.last.inMilliseconds,
        'p50_ms': sorted[sorted.length ~/ 2].inMilliseconds,
        'p95_ms': sorted[(sorted.length * 0.95).round()].inMilliseconds,
      };
    });

    return stats;
  }
}

/// Memory usage monitor
class MemoryMonitor {
  static final MemoryMonitor _instance = MemoryMonitor._internal();
  factory MemoryMonitor() => _instance;
  MemoryMonitor._internal();

  Timer? _timer;
  final List<int> _memorySamples = [];

  void startMonitoring({Duration interval = const Duration(seconds: 5)}) {
    _timer?.cancel();
    _timer = Timer.periodic(interval, (_) {
      _logMemoryUsage();
    });
  }

  void stopMonitoring() {
    _timer?.cancel();
    _timer = null;
  }

  void _logMemoryUsage() {
    // In a real app, you'd use platform-specific APIs
    // to get actual memory usage
    if (kDebugMode) {
      debugPrint('Memory check: ${_memorySamples.length} samples collected');
    }
  }

  void clearSamples() {
    _memorySamples.clear();
  }
}

/// Frame rate monitor
class FrameRateMonitor {
  static final FrameRateMonitor _instance = FrameRateMonitor._internal();
  factory FrameRateMonitor() => _instance;
  FrameRateMonitor._internal();

  final List<Duration> _frameTimes = [];
  DateTime? _lastFrameTime;
  bool _isMonitoring = false;

  void startMonitoring() {
    if (_isMonitoring) return;

    _isMonitoring = true;
    _lastFrameTime = DateTime.now();

    WidgetsBinding.instance.addPostFrameCallback(_onFrame);
  }

  void stopMonitoring() {
    _isMonitoring = false;
    _lastFrameTime = null;
  }

  void _onFrame(Duration timestamp) {
    if (!_isMonitoring) return;

    final now = DateTime.now();
    if (_lastFrameTime != null) {
      final frameDuration = now.difference(_lastFrameTime!);
      _frameTimes.add(frameDuration);

      // Keep only last 60 frames
      if (_frameTimes.length > 60) {
        _frameTimes.removeAt(0);
      }

      // Warn about dropped frames
      if (frameDuration.inMilliseconds > 16 && kDebugMode) {
        debugPrint('DROPPED FRAME: ${frameDuration.inMilliseconds}ms');
      }
    }

    _lastFrameTime = now;
    WidgetsBinding.instance.addPostFrameCallback(_onFrame);
  }

  double getAverageFPS() {
    if (_frameTimes.isEmpty) return 0;

    final avgDuration = _frameTimes.fold<Duration>(
          Duration.zero,
          (a, b) => a + b,
        ) ~/
        _frameTimes.length;

    return 1000 / avgDuration.inMilliseconds;
  }

  void clearFrameTimes() {
    _frameTimes.clear();
  }
}

/// Lazy initialization helper
class Lazy<T> {
  T? _value;
  final T Function() _initializer;

  Lazy(this._initializer);

  T get value {
    _value ??= _initializer();
    return _value!;
  }

  bool get isInitialized => _value != null;

  void reset() {
    _value = null;
  }
}

/// Debouncer
class Debouncer {
  final Duration duration;
  Timer? _timer;

  Debouncer({this.duration = const Duration(milliseconds: 300)});

  void call(VoidCallback action) {
    _timer?.cancel();
    _timer = Timer(duration, action);
  }

  void cancel() {
    _timer?.cancel();
  }

  void dispose() {
    _timer?.cancel();
  }
}

/// Throttler
class Throttler {
  final Duration duration;
  DateTime? _lastExecution;

  Throttler({this.duration = const Duration(milliseconds: 300)});

  void call(VoidCallback action) {
    final now = DateTime.now();

    if (_lastExecution == null || now.difference(_lastExecution!) >= duration) {
      _lastExecution = now;
      action();
    }
  }
}

/// Batch processor
class BatchProcessor<T> {
  final Duration batchWindow;
  final int maxBatchSize;
  final Future<void> Function(List<T>) processor;

  final List<T> _queue = [];
  Timer? _timer;

  BatchProcessor({
    required this.processor,
    this.batchWindow = const Duration(milliseconds: 500),
    this.maxBatchSize = 50,
  });

  void add(T item) {
    _queue.add(item);

    if (_queue.length >= maxBatchSize) {
      _processBatch();
    } else {
      _startTimer();
    }
  }

  void addAll(List<T> items) {
    _queue.addAll(items);

    if (_queue.length >= maxBatchSize) {
      _processBatch();
    } else {
      _startTimer();
    }
  }

  void _startTimer() {
    _timer?.cancel();
    _timer = Timer(batchWindow, _processBatch);
  }

  Future<void> _processBatch() async {
    _timer?.cancel();

    if (_queue.isEmpty) return;

    final batch = List<T>.from(_queue);
    _queue.clear();

    await processor(batch);
  }

  Future<void> flush() async {
    _timer?.cancel();
    await _processBatch();
  }

  void dispose() {
    _timer?.cancel();
    _queue.clear();
  }
}

/// Performance overlay widget
class PerformanceOverlay extends StatefulWidget {
  final Widget child;
  final bool enabled;

  const PerformanceOverlay({
    super.key,
    required this.child,
    this.enabled = kDebugMode,
  });

  @override
  State<PerformanceOverlay> createState() => _PerformanceOverlayState();
}

class _PerformanceOverlayState extends State<PerformanceOverlay> {
  @override
  void initState() {
    super.initState();
    if (widget.enabled) {
      FrameRateMonitor().startMonitoring();
    }
  }

  @override
  void dispose() {
    if (widget.enabled) {
      FrameRateMonitor().stopMonitoring();
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (!widget.enabled) return widget.child;

    return Stack(
      children: [
        widget.child,
        Positioned(
          top: MediaQuery.of(context).padding.top + 10,
          right: 10,
          child: Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: Colors.black87,
              borderRadius: BorderRadius.circular(8),
            ),
            child: StreamBuilder<void>(
              stream: Stream.periodic(const Duration(seconds: 1)),
              builder: (context, snapshot) {
                final fps = FrameRateMonitor().getAverageFPS();
                return Text(
                  'FPS: ${fps.toStringAsFixed(1)}',
                  style: TextStyle(
                    color: fps >= 55 ? Colors.green : Colors.red,
                    fontSize: 12,
                    fontWeight: FontWeight.bold,
                  ),
                );
              },
            ),
          ),
        ),
      ],
    );
  }
}

/// Cached computation helper
class CachedComputation<TKey, TValue> {
  final TValue Function(TKey) compute;
  final Map<TKey, TValue> _cache = {};
  final Duration? expiration;
  final Map<TKey, DateTime> _timestamps = {};

  CachedComputation({
    required this.compute,
    this.expiration,
  });

  TValue call(TKey key) {
    if (expiration != null) {
      final timestamp = _timestamps[key];
      if (timestamp != null &&
          DateTime.now().difference(timestamp) > expiration!) {
        _cache.remove(key);
        _timestamps.remove(key);
      }
    }

    if (!_cache.containsKey(key)) {
      _cache[key] = compute(key);
      _timestamps[key] = DateTime.now();
    }

    return _cache[key]!;
  }

  void invalidate(TKey key) {
    _cache.remove(key);
    _timestamps.remove(key);
  }

  void invalidateAll() {
    _cache.clear();
    _timestamps.clear();
  }

  int get size => _cache.length;
}

/// List chunking helper
extension ListChunking<T> on List<T> {
  List<List<T>> chunk(int size) {
    final chunks = <List<T>>[];
    for (var i = 0; i < length; i += size) {
      chunks.add(sublist(i, i + size > length ? length : i + size));
    }
    return chunks;
  }
}

/// Image size optimizer
class ImageOptimizer {
  static String optimizeUrl(
    String url, {
    int? width,
    int? height,
    int? quality,
  }) {
    // For services that support URL parameters for image optimization
    final uri = Uri.parse(url);
    final params = Map<String, String>.from(uri.queryParameters);

    if (width != null) params['w'] = width.toString();
    if (height != null) params['h'] = height.toString();
    if (quality != null) params['q'] = quality.toString();

    return uri.replace(queryParameters: params).toString();
  }

  static int getOptimalWidth(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;
    final devicePixelRatio = MediaQuery.of(context).devicePixelRatio;
    return (screenWidth * devicePixelRatio).round();
  }
}

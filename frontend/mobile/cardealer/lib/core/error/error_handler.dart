library;

/// Comprehensive error handling system
/// Provides error boundaries, retry mechanisms, and user-friendly error screens
import 'package:flutter/material.dart';
import 'dart:async';

/// Error severity levels
enum ErrorSeverity {
  low,
  medium,
  high,
  critical,
}

/// App error model
class AppError {
  final String message;
  final String? technicalMessage;
  final ErrorSeverity severity;
  final DateTime timestamp;
  final StackTrace? stackTrace;
  final String? code;
  final dynamic originalError;

  AppError({
    required this.message,
    this.technicalMessage,
    this.severity = ErrorSeverity.medium,
    DateTime? timestamp,
    this.stackTrace,
    this.code,
    this.originalError,
  }) : timestamp = timestamp ?? DateTime.now();

  bool get isCritical => severity == ErrorSeverity.critical;
  bool get isHigh => severity == ErrorSeverity.high;

  Map<String, dynamic> toJson() => {
        'message': message,
        'technicalMessage': technicalMessage,
        'severity': severity.name,
        'timestamp': timestamp.toIso8601String(),
        'code': code,
      };
}

/// Global error handler
class GlobalErrorHandler {
  static final GlobalErrorHandler _instance = GlobalErrorHandler._internal();
  factory GlobalErrorHandler() => _instance;
  GlobalErrorHandler._internal();

  final List<AppError> _errorHistory = [];
  final _errorController = StreamController<AppError>.broadcast();

  Stream<AppError> get errorStream => _errorController.stream;
  List<AppError> get errorHistory => List.unmodifiable(_errorHistory);

  void handleError(
    dynamic error, {
    StackTrace? stackTrace,
    String? message,
    ErrorSeverity severity = ErrorSeverity.medium,
    String? code,
  }) {
    final appError = AppError(
      message: message ?? _getErrorMessage(error),
      technicalMessage: error.toString(),
      severity: severity,
      stackTrace: stackTrace,
      code: code,
      originalError: error,
    );

    _errorHistory.add(appError);
    _errorController.add(appError);

    // Log critical errors
    if (appError.isCritical) {
      debugPrint('CRITICAL ERROR: ${appError.message}');
      debugPrint('Stack trace: ${appError.stackTrace}');
    }
  }

  String _getErrorMessage(dynamic error) {
    if (error == null) return 'Error desconocido';

    final errorString = error.toString().toLowerCase();

    if (errorString.contains('socket') || errorString.contains('network')) {
      return 'Error de conexión. Verifica tu internet.';
    }
    if (errorString.contains('timeout')) {
      return 'La operación tardó demasiado tiempo.';
    }
    if (errorString.contains('unauthorized') || errorString.contains('401')) {
      return 'No tienes autorización para esta acción.';
    }
    if (errorString.contains('forbidden') || errorString.contains('403')) {
      return 'Acceso denegado.';
    }
    if (errorString.contains('not found') || errorString.contains('404')) {
      return 'Recurso no encontrado.';
    }
    if (errorString.contains('500') || errorString.contains('server')) {
      return 'Error del servidor. Inténtalo más tarde.';
    }

    return 'Ha ocurrido un error. Inténtalo de nuevo.';
  }

  void clearHistory() {
    _errorHistory.clear();
  }

  void dispose() {
    _errorController.close();
  }
}

/// Error boundary widget
class ErrorBoundary extends StatefulWidget {
  final Widget child;
  final Widget Function(BuildContext context, AppError error)? errorBuilder;
  final void Function(AppError error)? onError;

  const ErrorBoundary({
    super.key,
    required this.child,
    this.errorBuilder,
    this.onError,
  });

  @override
  State<ErrorBoundary> createState() => _ErrorBoundaryState();
}

class _ErrorBoundaryState extends State<ErrorBoundary> {
  AppError? _error;

  @override
  void initState() {
    super.initState();

    // Listen to global errors
    GlobalErrorHandler().errorStream.listen((error) {
      if (mounted && error.isCritical) {
        setState(() {
          _error = error;
        });
        widget.onError?.call(error);
      }
    });
  }

  void _clearError() {
    setState(() {
      _error = null;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_error != null) {
      return widget.errorBuilder?.call(context, _error!) ??
          ErrorScreen(
            error: _error!,
            onRetry: _clearError,
          );
    }

    return widget.child;
  }
}

/// Retry mechanism
class RetryConfig {
  final int maxAttempts;
  final Duration initialDelay;
  final double backoffMultiplier;
  final Duration maxDelay;

  const RetryConfig({
    this.maxAttempts = 3,
    this.initialDelay = const Duration(seconds: 1),
    this.backoffMultiplier = 2.0,
    this.maxDelay = const Duration(seconds: 30),
  });

  Duration getDelay(int attempt) {
    final delay = initialDelay * (backoffMultiplier * attempt);
    return delay > maxDelay ? maxDelay : delay;
  }
}

/// Retry wrapper
Future<T> retry<T>(
  Future<T> Function() operation, {
  RetryConfig config = const RetryConfig(),
  bool Function(dynamic error)? shouldRetry,
}) async {
  int attempt = 0;

  while (true) {
    try {
      return await operation();
    } catch (error, stackTrace) {
      attempt++;

      final canRetry = shouldRetry?.call(error) ?? true;

      if (attempt >= config.maxAttempts || !canRetry) {
        GlobalErrorHandler().handleError(
          error,
          stackTrace: stackTrace,
          message: 'Error después de $attempt intentos',
        );
        rethrow;
      }

      final delay = config.getDelay(attempt);
      debugPrint('Retry attempt $attempt after ${delay.inSeconds}s');
      await Future.delayed(delay);
    }
  }
}

/// Error screen widget
class ErrorScreen extends StatelessWidget {
  final AppError error;
  final VoidCallback? onRetry;
  final VoidCallback? onGoBack;
  final String? title;
  final String? actionText;

  const ErrorScreen({
    super.key,
    required this.error,
    this.onRetry,
    this.onGoBack,
    this.title,
    this.actionText,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: Padding(
            padding: const EdgeInsets.all(24.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(
                  _getIcon(),
                  size: 80,
                  color: _getColor(),
                ),
                const SizedBox(height: 24),
                Text(
                  title ?? 'Oops!',
                  style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 12),
                Text(
                  error.message,
                  style: Theme.of(context).textTheme.bodyLarge,
                  textAlign: TextAlign.center,
                ),
                if (error.technicalMessage != null) ...[
                  const SizedBox(height: 16),
                  ExpansionTile(
                    title: const Text(
                      'Detalles técnicos',
                      style: TextStyle(fontSize: 14),
                    ),
                    children: [
                      Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: SelectableText(
                          error.technicalMessage!,
                          style: const TextStyle(
                            fontFamily: 'monospace',
                            fontSize: 12,
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
                const SizedBox(height: 32),
                if (onRetry != null)
                  ElevatedButton.icon(
                    onPressed: onRetry,
                    icon: const Icon(Icons.refresh),
                    label: Text(actionText ?? 'Reintentar'),
                    style: ElevatedButton.styleFrom(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 32,
                        vertical: 16,
                      ),
                    ),
                  ),
                if (onGoBack != null) ...[
                  const SizedBox(height: 12),
                  TextButton(
                    onPressed: onGoBack,
                    child: const Text('Volver'),
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }

  IconData _getIcon() {
    switch (error.severity) {
      case ErrorSeverity.low:
        return Icons.info_outline;
      case ErrorSeverity.medium:
        return Icons.warning_amber;
      case ErrorSeverity.high:
        return Icons.error_outline;
      case ErrorSeverity.critical:
        return Icons.dangerous;
    }
  }

  Color _getColor() {
    switch (error.severity) {
      case ErrorSeverity.low:
        return Colors.blue;
      case ErrorSeverity.medium:
        return Colors.orange;
      case ErrorSeverity.high:
        return Colors.red;
      case ErrorSeverity.critical:
        return Colors.red.shade900;
    }
  }
}

/// Inline error widget
class InlineError extends StatelessWidget {
  final String message;
  final VoidCallback? onRetry;
  final IconData? icon;

  const InlineError({
    super.key,
    required this.message,
    this.onRetry,
    this.icon,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.red.shade50,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: Colors.red.shade200,
          width: 1,
        ),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Row(
            children: [
              Icon(
                icon ?? Icons.error_outline,
                color: Colors.red.shade700,
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  message,
                  style: TextStyle(
                    color: Colors.red.shade900,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
            ],
          ),
          if (onRetry != null) ...[
            const SizedBox(height: 12),
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                onPressed: onRetry,
                icon: const Icon(Icons.refresh),
                label: const Text('Reintentar'),
                style: OutlinedButton.styleFrom(
                  foregroundColor: Colors.red.shade700,
                  side: BorderSide(color: Colors.red.shade300),
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }
}

/// Error snackbar helper
void showErrorSnackbar(
  BuildContext context,
  String message, {
  VoidCallback? onRetry,
  Duration duration = const Duration(seconds: 4),
}) {
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(
      content: Text(message),
      backgroundColor: Colors.red.shade700,
      duration: duration,
      action: onRetry != null
          ? SnackBarAction(
              label: 'Reintentar',
              textColor: Colors.white,
              onPressed: onRetry,
            )
          : null,
    ),
  );
}

/// Empty state widget
class EmptyState extends StatelessWidget {
  final String title;
  final String? subtitle;
  final IconData? icon;
  final String? actionText;
  final VoidCallback? onAction;

  const EmptyState({
    super.key,
    required this.title,
    this.subtitle,
    this.icon,
    this.actionText,
    this.onAction,
  });

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              icon ?? Icons.inbox_outlined,
              size: 80,
              color: Colors.grey.shade400,
            ),
            const SizedBox(height: 24),
            Text(
              title,
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    color: Colors.grey.shade700,
                    fontWeight: FontWeight.bold,
                  ),
              textAlign: TextAlign.center,
            ),
            if (subtitle != null) ...[
              const SizedBox(height: 8),
              Text(
                subtitle!,
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: Colors.grey.shade600,
                    ),
                textAlign: TextAlign.center,
              ),
            ],
            if (onAction != null && actionText != null) ...[
              const SizedBox(height: 24),
              ElevatedButton(
                onPressed: onAction,
                child: Text(actionText!),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

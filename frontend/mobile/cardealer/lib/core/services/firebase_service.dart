import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_analytics/firebase_analytics.dart';
import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:firebase_remote_config/firebase_remote_config.dart';
import 'package:flutter/foundation.dart';
import '../../app_config.dart';

class FirebaseService {
  static FirebaseAnalytics? _analytics;
  static FirebaseCrashlytics? _crashlytics;
  static FirebaseRemoteConfig? _remoteConfig;

  static FirebaseAnalytics get analytics {
    if (_analytics == null) {
      throw Exception('Firebase no ha sido inicializado');
    }
    return _analytics!;
  }

  static FirebaseCrashlytics get crashlytics {
    if (_crashlytics == null) {
      throw Exception('Firebase no ha sido inicializado');
    }
    return _crashlytics!;
  }

  static FirebaseRemoteConfig get remoteConfig {
    if (_remoteConfig == null) {
      throw Exception('Firebase no ha sido inicializado');
    }
    return _remoteConfig!;
  }

  /// Inicializa Firebase con la configuración apropiada
  static Future<void> initialize() async {
    try {
      // Inicializar Firebase
      await Firebase.initializeApp();

      // Configurar Analytics (solo en prod y staging)
      if (AppConfig.instance.enableAnalytics) {
        _analytics = FirebaseAnalytics.instance;
        await _analytics!.setAnalyticsCollectionEnabled(true);

        // Configurar propiedades de usuario
        await _analytics!.setUserProperty(
          name: 'environment',
          value: AppConfig.instance.flavor.name,
        );
      }

      // Configurar Crashlytics
      _crashlytics = FirebaseCrashlytics.instance;

      // Habilitar crashlytics según el ambiente
      if (AppConfig.instance.isProd || AppConfig.instance.isStaging) {
        await _crashlytics!.setCrashlyticsCollectionEnabled(true);
      } else {
        await _crashlytics!.setCrashlyticsCollectionEnabled(false);
      }

      // Capturar errores de Flutter
      FlutterError.onError = (errorDetails) {
        _crashlytics!.recordFlutterFatalError(errorDetails);
      };

      // Capturar errores asíncronos
      PlatformDispatcher.instance.onError = (error, stack) {
        _crashlytics!.recordError(error, stack, fatal: true);
        return true;
      };

      // Configurar Remote Config
      _remoteConfig = FirebaseRemoteConfig.instance;
      await _remoteConfig!.setConfigSettings(
        RemoteConfigSettings(
          fetchTimeout: const Duration(minutes: 1),
          minimumFetchInterval: AppConfig.instance.isProd
              ? const Duration(hours: 12)
              : const Duration(minutes: 5),
        ),
      );

      // Valores por defecto de Remote Config
      await _remoteConfig!.setDefaults({
        'min_app_version': '1.0.0',
        'force_update': false,
        'maintenance_mode': false,
        'featured_ratio': 0.4,
        'max_image_upload_size_mb': 10,
        'enable_notifications': true,
      });

      // Fetch remoto
      await _remoteConfig!.fetchAndActivate();

      if (kDebugMode) {
        print('✅ Firebase inicializado correctamente');
        print('Environment: ${AppConfig.instance.flavor.name}');
        print('Analytics enabled: ${AppConfig.instance.enableAnalytics}');
      }
    } catch (e, stack) {
      if (kDebugMode) {
        print('❌ Error inicializando Firebase: $e');
        print(stack);
      }
      // No lanzar excepción para permitir que la app funcione sin Firebase
    }
  }

  /// Log de evento personalizado
  static Future<void> logEvent({
    required String name,
    Map<String, dynamic>? parameters,
  }) async {
    if (_analytics != null && AppConfig.instance.enableAnalytics) {
      try {
        await _analytics!.logEvent(
          name: name,
          parameters: parameters,
        );
      } catch (e) {
        if (kDebugMode) {
          print('Error logging event: $e');
        }
      }
    }
  }

  /// Log de pantalla vista
  static Future<void> logScreenView({
    required String screenName,
    String? screenClass,
  }) async {
    if (_analytics != null && AppConfig.instance.enableAnalytics) {
      try {
        await _analytics!.logScreenView(
          screenName: screenName,
          screenClass: screenClass ?? screenName,
        );
      } catch (e) {
        if (kDebugMode) {
          print('Error logging screen view: $e');
        }
      }
    }
  }

  /// Registrar error no fatal
  static Future<void> logError(
    dynamic error,
    StackTrace? stackTrace, {
    String? reason,
    Map<String, dynamic>? information,
  }) async {
    if (_crashlytics != null) {
      try {
        await _crashlytics!.recordError(
          error,
          stackTrace,
          reason: reason,
          information: information?.entries.map((e) => '${e.key}: ${e.value}').toList() ?? [],
          fatal: false,
        );
      } catch (e) {
        if (kDebugMode) {
          print('Error logging to crashlytics: $e');
        }
      }
    }
  }

  /// Establecer ID de usuario
  static Future<void> setUserId(String userId) async {
    try {
      if (_analytics != null && AppConfig.instance.enableAnalytics) {
        await _analytics!.setUserId(id: userId);
      }
      if (_crashlytics != null) {
        await _crashlytics!.setUserIdentifier(userId);
      }
    } catch (e) {
      if (kDebugMode) {
        print('Error setting user ID: $e');
      }
    }
  }

  /// Limpiar ID de usuario (al hacer logout)
  static Future<void> clearUserId() async {
    try {
      if (_analytics != null && AppConfig.instance.enableAnalytics) {
        await _analytics!.setUserId(id: null);
      }
      if (_crashlytics != null) {
        await _crashlytics!.setUserIdentifier('');
      }
    } catch (e) {
      if (kDebugMode) {
        print('Error clearing user ID: $e');
      }
    }
  }

  /// Obtener valor de Remote Config
  static T getRemoteConfigValue<T>(String key, T defaultValue) {
    if (_remoteConfig == null) return defaultValue;

    try {
      final value = _remoteConfig!.getValue(key);

      if (T == String) {
        return value.asString() as T;
      } else if (T == int) {
        return value.asInt() as T;
      } else if (T == double) {
        return value.asDouble() as T;
      } else if (T == bool) {
        return value.asBool() as T;
      }

      return defaultValue;
    } catch (e) {
      if (kDebugMode) {
        print('Error getting remote config value: $e');
      }
      return defaultValue;
    }
  }

  /// Verificar si hay mantenimiento programado
  static bool get isMaintenanceMode {
    return getRemoteConfigValue<bool>('maintenance_mode', false);
  }

  /// Verificar si se requiere actualización forzada
  static bool get requiresForceUpdate {
    return getRemoteConfigValue<bool>('force_update', false);
  }

  /// Obtener versión mínima requerida
  static String get minAppVersion {
    return getRemoteConfigValue<String>('min_app_version', '1.0.0');
  }
}

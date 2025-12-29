import 'package:flutter/services.dart';
import 'package:local_auth/local_auth.dart';

/// Biometric Authentication Service - Sprint 7 AE-003
/// Maneja autenticación con Face ID / Touch ID
/// Features:
/// - Verificación de disponibilidad de biometría
/// - Autenticación con Face ID/Touch ID
/// - Fallback a password si falla
/// - Manejo de errores y permisos

class BiometricAuthService {
  final LocalAuthentication _localAuth = LocalAuthentication();

  /// Verifica si el dispositivo tiene capacidad biométrica
  Future<bool> isDeviceSupported() async {
    try {
      return await _localAuth.isDeviceSupported();
    } catch (e) {
      return false;
    }
  }

  /// Verifica si hay biometría configurada
  Future<bool> isBiometricAvailable() async {
    try {
      final isSupported = await _localAuth.isDeviceSupported();
      if (!isSupported) return false;

      final canCheck = await _localAuth.canCheckBiometrics;
      return canCheck;
    } catch (e) {
      return false;
    }
  }

  /// Obtiene los tipos de biometría disponibles
  Future<List<BiometricType>> getAvailableBiometrics() async {
    try {
      return await _localAuth.getAvailableBiometrics();
    } catch (e) {
      return [];
    }
  }

  /// Autentica con biometría
  /// Retorna true si la autenticación fue exitosa
  Future<BiometricAuthResult> authenticate({
    String reason = 'Por favor autentícate para continuar',
    bool useErrorDialogs = true,
    bool stickyAuth = false,
  }) async {
    try {
      final isAvailable = await isBiometricAvailable();
      if (!isAvailable) {
        return BiometricAuthResult(
          success: false,
          errorType: BiometricErrorType.notAvailable,
          errorMessage: 'Biometría no disponible',
        );
      }

      final authenticated = await _localAuth.authenticate(
        localizedReason: reason,
      );

      return BiometricAuthResult(
        success: authenticated,
        errorType:
            authenticated ? null : BiometricErrorType.authenticationFailed,
        errorMessage: authenticated ? null : 'Autenticación fallida',
      );
    } on PlatformException catch (e) {
      return _handlePlatformException(e);
    } catch (e) {
      return BiometricAuthResult(
        success: false,
        errorType: BiometricErrorType.unknown,
        errorMessage: 'Error desconocido: ${e.toString()}',
      );
    }
  }

  /// Maneja excepciones de la plataforma
  BiometricAuthResult _handlePlatformException(PlatformException e) {
    BiometricErrorType errorType;
    String errorMessage;

    switch (e.code) {
      case 'NotAvailable':
        errorType = BiometricErrorType.notAvailable;
        errorMessage = 'Biometría no disponible en este dispositivo';
        break;
      case 'NotEnrolled':
        errorType = BiometricErrorType.notEnrolled;
        errorMessage = 'No hay datos biométricos registrados';
        break;
      case 'LockedOut':
        errorType = BiometricErrorType.lockedOut;
        errorMessage = 'Autenticación bloqueada temporalmente';
        break;
      case 'PermanentlyLockedOut':
        errorType = BiometricErrorType.permanentlyLockedOut;
        errorMessage = 'Autenticación bloqueada permanentemente';
        break;
      case 'OtherOperatingSystem':
        errorType = BiometricErrorType.notSupported;
        errorMessage = 'Sistema operativo no soportado';
        break;
      default:
        errorType = BiometricErrorType.unknown;
        errorMessage = e.message ?? 'Error desconocido';
    }

    return BiometricAuthResult(
      success: false,
      errorType: errorType,
      errorMessage: errorMessage,
    );
  }

  /// Detiene la autenticación en curso
  Future<void> stopAuthentication() async {
    try {
      await _localAuth.stopAuthentication();
    } catch (e) {
      // Ignorar errores al detener
    }
  }
}

/// Resultado de autenticación biométrica
class BiometricAuthResult {
  final bool success;
  final BiometricErrorType? errorType;
  final String? errorMessage;

  BiometricAuthResult({
    required this.success,
    this.errorType,
    this.errorMessage,
  });

  bool get shouldFallbackToPassword =>
      errorType == BiometricErrorType.authenticationFailed ||
      errorType == BiometricErrorType.notEnrolled ||
      errorType == BiometricErrorType.lockedOut;
}

/// Tipos de error en autenticación biométrica
enum BiometricErrorType {
  notAvailable,
  notEnrolled,
  notSupported,
  authenticationFailed,
  lockedOut,
  permanentlyLockedOut,
  unknown,
}

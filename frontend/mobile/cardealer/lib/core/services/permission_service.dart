import 'package:permission_handler/permission_handler.dart';
import 'package:flutter/material.dart';

/// Centralized permission service for contextual permission requests
///
/// Usage:
/// ```dart
/// final granted = await PermissionService.requestNotifications(context);
/// if (granted) {
///   // Enable notifications
/// }
/// ```
class PermissionService {
  PermissionService._();

  /// Request notification permission with context explanation
  static Future<bool> requestNotifications(BuildContext context) async {
    final status = await Permission.notification.status;

    if (status.isGranted) {
      return true;
    }

    if (status.isDenied) {
      // Show rationale dialog
      if (!context.mounted) return false;

      final shouldRequest = await _showPermissionRationale(
        context,
        title: 'Mantente Informado',
        description:
            'Recibe notificaciones sobre nuevos vehículos que coincidan con tus preferencias, respuestas de vendedores y ofertas especiales.',
        icon: Icons.notifications_active,
      );

      if (!shouldRequest) return false;

      final result = await Permission.notification.request();
      return result.isGranted;
    }

    if (status.isPermanentlyDenied) {
      if (!context.mounted) return false;

      await _showOpenSettingsDialog(
        context,
        title: 'Permisos de Notificaciones',
        description:
            'Para recibir notificaciones, necesitas habilitar los permisos en la configuración de tu dispositivo.',
      );
      return false;
    }

    return false;
  }

  /// Request location permission with context explanation
  static Future<bool> requestLocation(BuildContext context,
      {bool precise = false}) async {
    final permission =
        precise ? Permission.location : Permission.locationWhenInUse;
    final status = await permission.status;

    if (status.isGranted) {
      return true;
    }

    if (status.isDenied) {
      if (!context.mounted) return false;

      final shouldRequest = await _showPermissionRationale(
        context,
        title: 'Encuentra Autos Cerca de Ti',
        description:
            'Permítenos acceder a tu ubicación para mostrarte vehículos disponibles en tu área y calcular distancias.',
        icon: Icons.location_on,
      );

      if (!shouldRequest) return false;

      final result = await permission.request();
      return result.isGranted;
    }

    if (status.isPermanentlyDenied) {
      if (!context.mounted) return false;

      await _showOpenSettingsDialog(
        context,
        title: 'Permisos de Ubicación',
        description:
            'Para ver vehículos cerca de ti, necesitas habilitar los permisos de ubicación en la configuración.',
      );
      return false;
    }

    return false;
  }

  /// Request camera permission for taking photos
  static Future<bool> requestCamera(BuildContext context) async {
    final status = await Permission.camera.status;

    if (status.isGranted) {
      return true;
    }

    if (status.isDenied) {
      if (!context.mounted) return false;

      final shouldRequest = await _showPermissionRationale(
        context,
        title: 'Toma Fotos de Tu Vehículo',
        description:
            'Necesitamos acceso a la cámara para que puedas tomar fotos profesionales de tu vehículo al publicar un anuncio.',
        icon: Icons.camera_alt,
      );

      if (!shouldRequest) return false;

      final result = await Permission.camera.request();
      return result.isGranted;
    }

    if (status.isPermanentlyDenied) {
      if (!context.mounted) return false;

      await _showOpenSettingsDialog(
        context,
        title: 'Permisos de Cámara',
        description:
            'Para tomar fotos, necesitas habilitar el acceso a la cámara en la configuración.',
      );
      return false;
    }

    return false;
  }

  /// Request photo library permission
  static Future<bool> requestPhotos(BuildContext context) async {
    final status = await Permission.photos.status;

    if (status.isGranted || status.isLimited) {
      return true;
    }

    if (status.isDenied) {
      if (!context.mounted) return false;

      final shouldRequest = await _showPermissionRationale(
        context,
        title: 'Accede a Tus Fotos',
        description:
            'Selecciona fotos de tu galería para crear un anuncio atractivo de tu vehículo.',
        icon: Icons.photo_library,
      );

      if (!shouldRequest) return false;

      final result = await Permission.photos.request();
      return result.isGranted || result.isLimited;
    }

    if (status.isPermanentlyDenied) {
      if (!context.mounted) return false;

      await _showOpenSettingsDialog(
        context,
        title: 'Permisos de Galería',
        description:
            'Para seleccionar fotos, necesitas habilitar el acceso a la galería en la configuración.',
      );
      return false;
    }

    return false;
  }

  /// Check if permission is granted
  static Future<bool> isGranted(Permission permission) async {
    final status = await permission.status;
    return status.isGranted;
  }

  /// Check notification permission status
  static Future<bool> isNotificationsGranted() async {
    return await isGranted(Permission.notification);
  }

  /// Check location permission status
  static Future<bool> isLocationGranted() async {
    return await isGranted(Permission.locationWhenInUse);
  }

  /// Show permission rationale dialog
  static Future<bool> _showPermissionRationale(
    BuildContext context, {
    required String title,
    required String description,
    required IconData icon,
  }) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
        ),
        title: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Theme.of(context).primaryColor.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(
                icon,
                color: Theme.of(context).primaryColor,
                size: 28,
              ),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Text(
                title,
                style: const TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ),
          ],
        ),
        content: Text(
          description,
          style: TextStyle(
            fontSize: 16,
            color: Colors.grey[700],
            height: 1.5,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(
              'Ahora No',
              style: TextStyle(
                color: Colors.grey[600],
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          ElevatedButton(
            onPressed: () => Navigator.of(context).pop(true),
            style: ElevatedButton.styleFrom(
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
              padding: const EdgeInsets.symmetric(
                horizontal: 24,
                vertical: 12,
              ),
            ),
            child: const Text(
              'Permitir',
              style: TextStyle(
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );

    return result ?? false;
  }

  /// Show dialog to open settings
  static Future<void> _showOpenSettingsDialog(
    BuildContext context, {
    required String title,
    required String description,
  }) async {
    await showDialog(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
        ),
        title: Text(
          title,
          style: const TextStyle(
            fontSize: 20,
            fontWeight: FontWeight.w600,
          ),
        ),
        content: Text(
          description,
          style: TextStyle(
            fontSize: 16,
            color: Colors.grey[700],
            height: 1.5,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: Text(
              'Cancelar',
              style: TextStyle(
                color: Colors.grey[600],
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              openAppSettings();
            },
            style: ElevatedButton.styleFrom(
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
              padding: const EdgeInsets.symmetric(
                horizontal: 24,
                vertical: 12,
              ),
            ),
            child: const Text(
              'Abrir Configuración',
              style: TextStyle(
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

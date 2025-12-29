// Script para generar icono adaptativo con padding correcto
// Los iconos adaptativos de Android requieren que el contenido esté en el 66% central
// Ejecutar: dart run tool/generate_adaptive_icon.dart

import 'dart:io';
import 'package:image/image.dart' as img;

void main() async {
  print('🎨 Generando icono adaptativo para Android...\n');

  // Leer el icono original
  final iconFile = File('assets/logos/icon.png');
  if (!await iconFile.exists()) {
    print('❌ Error: No se encontró assets/logos/icon.png');
    exit(1);
  }

  final bytes = await iconFile.readAsBytes();
  final originalIcon = img.decodeImage(bytes);

  if (originalIcon == null) {
    print('❌ Error: No se pudo decodificar la imagen');
    exit(1);
  }

  print('📐 Icono original: ${originalIcon.width}x${originalIcon.height}');

  // El foreground adaptativo debe ser 432x432 o 1024x1024
  // El contenido visible debe estar en el 66% central (safe zone)
  // Esto significa padding del 17% en cada lado

  const targetSize = 1024;
  const safeZoneRatio = 0.66;
  final contentSize = (targetSize * safeZoneRatio).round(); // ~676
  final padding = ((targetSize - contentSize) / 2).round(); // ~174

  print('📏 Target size: ${targetSize}x$targetSize');
  print('📏 Content size: ${contentSize}x$contentSize');
  print('📏 Padding: $padding px en cada lado');

  // Crear imagen nueva con fondo transparente
  final foreground = img.Image(width: targetSize, height: targetSize);

  // Llenar con transparente
  img.fill(foreground, color: img.ColorRgba8(0, 0, 0, 0));

  // Redimensionar el icono original al tamaño del contenido
  final resizedIcon = img.copyResize(
    originalIcon,
    width: contentSize,
    height: contentSize,
    interpolation: img.Interpolation.linear,
  );

  // Pegar el icono centrado
  img.compositeImage(
    foreground,
    resizedIcon,
    dstX: padding,
    dstY: padding,
  );

  // Guardar el foreground
  final outputPath = 'assets/icons/app_icon_foreground.png';
  final outputFile = File(outputPath);
  await outputFile.writeAsBytes(img.encodePng(foreground));

  print('✅ Foreground guardado en: $outputPath');

  // También copiar el original como app_icon para legacy
  await iconFile.copy('assets/icons/app_icon.png');
  print('✅ Icono legacy copiado a: assets/icons/app_icon.png');

  print('\n🎉 ¡Listo! Ahora ejecuta:');
  print('   flutter pub run flutter_launcher_icons');
}

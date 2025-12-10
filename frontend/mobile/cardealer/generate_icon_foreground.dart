import 'dart:io';
import 'package:image/image.dart' as img;

void main() async {
  // Lee el icono original
  final originalFile = File('assets/icons/app_icon.png');
  if (!originalFile.existsSync()) {
    print('Error: No se encontró assets/icons/app_icon.png');
    return;
  }

  final originalBytes = await originalFile.readAsBytes();
  final original = img.decodeImage(originalBytes);
  
  if (original == null) {
    print('Error: No se pudo decodificar la imagen');
    return;
  }

  // Crear nueva imagen de 1024x1024 con fondo transparente
  final foreground = img.Image(width: 1024, height: 1024);
  
  // Llenar con transparente
  img.fill(foreground, color: img.ColorRgba8(0, 0, 0, 0));
  
  // Calcular dimensiones con 17% de padding en cada lado
  // Esto deja un área segura del 66% en el centro
  final padding = (1024 * 0.17).round();
  final safeSize = 1024 - (padding * 2);
  
  // Redimensionar el icono original al área segura
  final resized = img.copyResize(
    original,
    width: safeSize,
    height: safeSize,
    interpolation: img.Interpolation.linear,
  );
  
  // Copiar el icono redimensionado al centro
  img.compositeImage(
    foreground,
    resized,
    dstX: padding,
    dstY: padding,
  );
  
  // Guardar el resultado
  final outputFile = File('assets/icons/app_icon_foreground.png');
  await outputFile.writeAsBytes(img.encodePng(foreground));
  
  print('✓ Icono foreground generado: ${outputFile.path}');
  print('  Tamaño: 1024x1024');
  print('  Padding: ${padding}px (${(padding / 1024 * 100).toStringAsFixed(1)}%)');
  print('  Área segura: ${safeSize}x${safeSize}');
}

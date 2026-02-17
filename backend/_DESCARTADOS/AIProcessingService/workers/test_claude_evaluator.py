#!/usr/bin/env python3
"""
Test Claude Vision via GitHub Models
=====================================

Este script prueba el evaluador de imágenes usando Claude 3.5 Sonnet
a través de GitHub Models (gratis para usuarios de GitHub).

Requisitos:
    export GITHUB_TOKEN='ghp_tu_token_aquí'

Uso:
    python3 test_claude_evaluator.py
    python3 test_claude_evaluator.py --image input/car.jpg
"""

import os
import sys
import cv2
import argparse
from pathlib import Path

# Agregar el directorio actual al path
sys.path.insert(0, str(Path(__file__).parent))

from ai_evaluator import VisionLLMEvaluator, HybridEvaluator


def check_github_token():
    """Verificar que GITHUB_TOKEN está configurado"""
    token = os.environ.get('GITHUB_TOKEN')
    if not token:
        print("=" * 60)
        print("❌ ERROR: GITHUB_TOKEN no está configurado")
        print("=" * 60)
        print()
        print("Para usar Claude via GitHub Models, necesitas:")
        print()
        print("1. Ir a: https://github.com/settings/tokens")
        print("2. Click 'Generate new token (classic)'")
        print("3. Seleccionar scope 'repo' o 'repo:read'")
        print("4. Copiar el token generado")
        print("5. Ejecutar:")
        print()
        print("   export GITHUB_TOKEN='ghp_tu_token_aquí'")
        print()
        print("=" * 60)
        return False
    print(f"✅ GITHUB_TOKEN configurado ({len(token)} caracteres)")
    return True


def test_single_image(image_path: Path, cutout_path: Path = None):
    """Evaluar una sola imagen con Claude"""
    
    if not check_github_token():
        return
    
    # Cargar imagen original
    if not image_path.exists():
        print(f"❌ No se encontró: {image_path}")
        return
        
    original = cv2.imread(str(image_path))
    if original is None:
        print(f"❌ No se pudo cargar: {image_path}")
        return
    
    print(f"\n📷 Original: {image_path.name} ({original.shape[1]}x{original.shape[0]})")
    
    # Buscar cutout si no se especificó
    if cutout_path is None:
        output_dir = Path(__file__).parent / "output_v2"
        cutout_name = image_path.stem + "_v2.png"
        cutout_path = output_dir / cutout_name
        
    if not cutout_path.exists():
        print(f"❌ No se encontró cutout: {cutout_path}")
        print("   Ejecuta primero: python3 pipeline_v2.py")
        return
        
    cutout = cv2.imread(str(cutout_path), cv2.IMREAD_UNCHANGED)
    print(f"📷 Cutout: {cutout_path.name}")
    
    # Crear evaluador
    print("\n🤖 Inicializando Claude 3.5 Sonnet via GitHub Models...")
    evaluator = VisionLLMEvaluator(provider="github")
    
    # Evaluar
    print("🔍 Evaluando imagen...")
    result = evaluator.evaluate(original, cutout)
    
    if result:
        print("\n" + "=" * 60)
        print("📊 RESULTADO DE EVALUACIÓN (Claude 3.5 Sonnet)")
        print("=" * 60)
        print(f"🎯 Score: {result.score:.0f}/100")
        print()
        print("📋 Detalles:")
        print(f"   Vehículo completo: {'✅' if result.vehicle_complete else '❌'}")
        print(f"   Ruedas intactas: {'✅' if result.wheels_intact else '❌'}")
        print(f"   Sin sombras: {'✅' if result.no_shadows else '❌'}")
        print(f"   Bordes limpios: {'✅' if result.clean_edges else '❌'}")
        print(f"   Sin fondo: {'✅' if result.no_background else '❌'}")
        
        if result.issues:
            print(f"\n⚠️ Problemas detectados:")
            for issue in result.issues:
                print(f"   • {issue}")
                
        if result.suggestions:
            print(f"\n💡 Sugerencias:")
            for suggestion in result.suggestions:
                print(f"   • {suggestion}")
                
        if result.description:
            print(f"\n📝 Descripción: {result.description}")
            
        print("=" * 60)
        
        # Clasificación
        if result.score >= 90:
            print("🌟 Clasificación: EXCELENTE - Listo para producción")
        elif result.score >= 75:
            print("✅ Clasificación: BUENO - Pequeñas mejoras posibles")
        elif result.score >= 60:
            print("⚠️ Clasificación: ACEPTABLE - Requiere revisión")
        else:
            print("❌ Clasificación: RECHAZADO - Problemas significativos")
    else:
        print("❌ La evaluación falló - revisa el GITHUB_TOKEN y logs")


def test_all_images():
    """Evaluar todas las imágenes en output_v2/"""
    
    if not check_github_token():
        return
        
    input_dir = Path(__file__).parent / "input"
    output_dir = Path(__file__).parent / "output_v2"
    
    if not output_dir.exists():
        print(f"❌ No existe directorio: {output_dir}")
        print("   Ejecuta primero: python3 pipeline_v2.py")
        return
    
    # Obtener pares de imágenes
    pairs = []
    for cutout_path in output_dir.glob("*_v2.png"):
        original_name = cutout_path.stem.replace("_v2", "")
        original_path = input_dir / f"{original_name}.jpg"
        if not original_path.exists():
            original_path = input_dir / f"{original_name}.png"
        if original_path.exists():
            pairs.append((original_path, cutout_path))
    
    if not pairs:
        print("❌ No se encontraron pares de imágenes")
        return
        
    print(f"\n🚗 Evaluando {len(pairs)} imágenes con Claude 3.5 Sonnet...\n")
    
    # Crear evaluador
    evaluator = VisionLLMEvaluator(provider="github")
    
    results = []
    for i, (original_path, cutout_path) in enumerate(pairs, 1):
        print(f"\n[{i}/{len(pairs)}] {original_path.name}")
        
        original = cv2.imread(str(original_path))
        cutout = cv2.imread(str(cutout_path), cv2.IMREAD_UNCHANGED)
        
        result = evaluator.evaluate(original, cutout)
        if result:
            emoji = "🌟" if result.score >= 90 else "✅" if result.score >= 75 else "⚠️" if result.score >= 60 else "❌"
            print(f"   {emoji} Score: {result.score:.0f}/100")
            results.append((original_path.name, result))
        else:
            print(f"   ❌ Evaluación falló")
    
    # Resumen
    if results:
        print("\n" + "=" * 60)
        print("📊 RESUMEN")
        print("=" * 60)
        
        scores = [r[1].score for r in results]
        excellent = sum(1 for s in scores if s >= 90)
        good = sum(1 for s in scores if 75 <= s < 90)
        acceptable = sum(1 for s in scores if 60 <= s < 75)
        rejected = sum(1 for s in scores if s < 60)
        
        print(f"Total evaluadas: {len(results)}")
        print(f"🌟 Excelentes: {excellent}")
        print(f"✅ Buenas: {good}")
        print(f"⚠️ Aceptables: {acceptable}")
        print(f"❌ Rechazadas: {rejected}")
        print(f"📈 Promedio: {sum(scores)/len(scores):.1f}/100")


def main():
    parser = argparse.ArgumentParser(
        description="Evaluar imágenes de vehículos con Claude 3.5 Sonnet"
    )
    parser.add_argument(
        "--image", "-i",
        type=str,
        help="Ruta a imagen específica para evaluar"
    )
    parser.add_argument(
        "--cutout", "-c",
        type=str,
        help="Ruta al cutout (opcional, busca automáticamente)"
    )
    parser.add_argument(
        "--all", "-a",
        action="store_true",
        help="Evaluar todas las imágenes en output_v2/"
    )
    
    args = parser.parse_args()
    
    print("=" * 60)
    print("🤖 CLAUDE 3.5 SONNET - EVALUADOR DE IMÁGENES")
    print("   via GitHub Models (gratis)")
    print("=" * 60)
    
    if args.all:
        test_all_images()
    elif args.image:
        image_path = Path(args.image)
        cutout_path = Path(args.cutout) if args.cutout else None
        test_single_image(image_path, cutout_path)
    else:
        # Por defecto, probar la primera imagen
        input_dir = Path(__file__).parent / "input"
        images = list(input_dir.glob("*.jpg")) + list(input_dir.glob("*.png"))
        if images:
            test_single_image(images[0])
        else:
            print("❌ No se encontraron imágenes en input/")
            print("   Uso: python3 test_claude_evaluator.py --image path/to/image.jpg")


if __name__ == "__main__":
    main()

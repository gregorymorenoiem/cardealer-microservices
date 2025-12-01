import time
import pyautogui

# Mensaje que quieres enviar (puedes cambiarlo)
mensaje = "Si"

print("=" * 60)
print("INSTRUCCIONES:")
print("=" * 60)
print("1. Haz clic en el campo de texto del chat de Copilot")
print("2. El script escribirá el mensaje automáticamente en 5 segundos")
print("=" * 60)
print(f"\nMensaje que se enviará: '{mensaje}'")
print("\nEsperando 5 segundos...")

# Cuenta regresiva visual
for i in range(5, 0, -1):
    print(f"{i}...")
    time.sleep(1)

print("\n¡Escribiendo mensaje!")

# Escribe el mensaje (usa write en lugar de typewrite para soportar caracteres en español)
pyautogui.write(mensaje, interval=0.05)

# Pequeña pausa antes de presionar Enter
time.sleep(0.2)

# Presiona Enter para enviar el mensaje
pyautogui.press('enter')

print("✓ Mensaje enviado exitosamente!")

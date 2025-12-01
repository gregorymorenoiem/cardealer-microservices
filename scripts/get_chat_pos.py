import time
import pyautogui
import pyperclip

# Coordenadas del INPUT del chat
CHAT_X = 140
CHAT_Y = 914

# Mensaje CON acentos
mensaje = "Sí, continúa con la  Política 15: Disaster Recovery y Backup"

print("Tienes 5 segundos para poner en foco la ventana de VS Code...")
time.sleep(5)

# 1. Click en el input del chat
pyautogui.click(CHAT_X, CHAT_Y)

# 2. Copiar el mensaje al portapapeles
pyperclip.copy(mensaje)

# 3. Pegar (Ctrl+V) en el chat
pyautogui.hotkey("ctrl", "v")

# 4. Enviar (Enter)
pyautogui.press("enter")

print("Mensaje enviado al chat.")

import time
import pyautogui

print("Coloca el mouse sobre el INPUT del chat en 5 segundos y no lo muevas...")
time.sleep(5)

pos = pyautogui.position()
print(f"Coordenadas del mouse: x={pos.x}, y={pos.y}")

#!/usr/bin/env python3
"""
Simple HTTP server for AI Comparison Viewer
Serves on port 8888

Usage: python3 serve.py
Open: http://localhost:8888
"""

import http.server
import socketserver
import os

PORT = 8888
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
VIEWER_PATH = os.path.join(BASE_DIR, "viewer_v2.html")

class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=BASE_DIR, **kwargs)
    
    def do_GET(self):
        # Serve viewer at root
        if self.path == "/" or self.path == "/index.html" or self.path == "/viewer.html":
            self.send_response(200)
            self.send_header("Content-type", "text/html")
            self.end_headers()
            with open(VIEWER_PATH, "rb") as f:
                self.wfile.write(f.read())
            return
        
        # Serve originals and processed images
        return super().do_GET()
    
    def end_headers(self):
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Cache-Control", "no-cache, no-store, must-revalidate")
        super().end_headers()

if __name__ == "__main__":
    print("=" * 60)
    print("🖼️  AI Processing - Local Viewer")
    print("=" * 60)
    print(f"\n🌐 Open in browser: http://localhost:{PORT}")
    print(f"\n📁 Base directory: {BASE_DIR}")
    print(f"📷 Originals:      {os.path.join(BASE_DIR, 'originals')}")
    print(f"✅ Processed:      {os.path.join(BASE_DIR, 'processed')}")
    print("\n⏹️  Press Ctrl+C to stop\n")
    print("=" * 60)
    
    with socketserver.TCPServer(("", PORT), Handler) as httpd:
        httpd.serve_forever()

import 'package:flutter/material.dart';
import '../../../../domain/entities/vehicle.dart';

/// Sticky bottom action bar with contact buttons
class ContactActionBar extends StatelessWidget {
  final Vehicle vehicle;
  final VoidCallback onCall;
  final VoidCallback onWhatsApp;
  final VoidCallback onMessage;

  const ContactActionBar({
    super.key,
    required this.vehicle,
    required this.onCall,
    required this.onWhatsApp,
    required this.onMessage,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.1),
            blurRadius: 8,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      padding: const EdgeInsets.all(16),
      child: SafeArea(
        top: false,
        child: Row(
          children: [
            // Call button
            Expanded(
              child: OutlinedButton.icon(
                onPressed: onCall,
                icon: const Icon(Icons.phone),
                label: const Text('Llamar'),
                style: OutlinedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 12),
                ),
              ),
            ),
            const SizedBox(width: 8),
            
            // WhatsApp button
            Expanded(
              child: OutlinedButton.icon(
                onPressed: onWhatsApp,
                icon: const Icon(Icons.chat),
                label: const Text('WhatsApp'),
                style: OutlinedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  foregroundColor: Colors.green,
                  side: const BorderSide(color: Colors.green),
                ),
              ),
            ),
            const SizedBox(width: 8),
            
            // Message button
            Expanded(
              child: ElevatedButton.icon(
                onPressed: onMessage,
                icon: const Icon(Icons.message),
                label: const Text('Mensaje'),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 12),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

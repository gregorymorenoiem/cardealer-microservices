import 'package:flutter/material.dart';
import '../../../../core/responsive/responsive_helper.dart';
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
    final responsive = context.responsive;
    
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.1),
            blurRadius: 8,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      padding: EdgeInsets.all(responsive.horizontalPadding),
      child: SafeArea(
        top: false,
        child: Row(
          children: [
            // Call button
            Expanded(
              child: OutlinedButton.icon(
                onPressed: onCall,
                icon: Icon(Icons.phone, size: responsive.iconSize),
                label: Text('Llamar', style: TextStyle(fontSize: responsive.bodyFontSize)),
                style: OutlinedButton.styleFrom(
                  padding: EdgeInsets.symmetric(vertical: responsive.cardSpacing),
                ),
              ),
            ),
            SizedBox(width: responsive.cardSpacing * 0.6),

            // WhatsApp button
            Expanded(
              child: OutlinedButton.icon(
                onPressed: onWhatsApp,
                icon: Icon(Icons.chat, size: responsive.iconSize),
                label: Text('WhatsApp', style: TextStyle(fontSize: responsive.bodyFontSize)),
                style: OutlinedButton.styleFrom(
                  padding: EdgeInsets.symmetric(vertical: responsive.cardSpacing),
                  foregroundColor: Colors.green,
                  side: const BorderSide(color: Colors.green),
                ),
              ),
            ),
            SizedBox(width: responsive.cardSpacing * 0.6),

            // Message button
            Expanded(
              child: ElevatedButton.icon(
                onPressed: onMessage,
                icon: Icon(Icons.message, size: responsive.iconSize),
                label: Text('Mensaje', style: TextStyle(fontSize: responsive.bodyFontSize)),
                style: ElevatedButton.styleFrom(
                  padding: EdgeInsets.symmetric(vertical: responsive.cardSpacing),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

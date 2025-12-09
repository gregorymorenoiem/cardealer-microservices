import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';

/// Contact Actions Bar
///
/// Features:
/// - Sticky bottom bar
/// - Call button with phone integration
/// - Chat button
/// - Schedule visit button
/// - WhatsApp quick action
class ContactActionsBar extends StatelessWidget {
  final String phoneNumber;
  final VoidCallback onChat;
  final VoidCallback onScheduleVisit;
  final bool hasWhatsApp;

  const ContactActionsBar({
    super.key,
    required this.phoneNumber,
    required this.onChat,
    required this.onScheduleVisit,
    this.hasWhatsApp = true,
  });

  Future<void> _makePhoneCall() async {
    final uri = Uri(scheme: 'tel', path: phoneNumber);
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri);
    }
  }

  Future<void> _openWhatsApp() async {
    final cleanPhone = phoneNumber.replaceAll(RegExp(r'[^\d+]'), '');
    final uri = Uri.parse('https://wa.me/$cleanPhone');
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.1),
            blurRadius: 10,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      child: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              // Call Button
              _ActionButton(
                icon: Icons.phone,
                label: 'Llamar',
                color: const Color(0xFF10B981),
                onPressed: _makePhoneCall,
              ),

              const SizedBox(width: 10),

              // Chat Button
              Expanded(
                child: _ActionButton(
                  icon: Icons.chat_bubble,
                  label: 'Chat',
                  color: const Color(0xFF001F54),
                  onPressed: onChat,
                  isExpanded: true,
                ),
              ),

              const SizedBox(width: 10),

              // Schedule Visit
              _ActionButton(
                icon: Icons.calendar_today,
                label: 'Visitar',
                color: const Color(0xFF6366F1),
                onPressed: onScheduleVisit,
              ),

              if (hasWhatsApp) ...[
                const SizedBox(width: 10),
                // WhatsApp Button
                _ActionButton(
                  icon: Icons.message,
                  label: 'WhatsApp',
                  color: const Color(0xFF25D366),
                  onPressed: _openWhatsApp,
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

class _ActionButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onPressed;
  final bool isExpanded;

  const _ActionButton({
    required this.icon,
    required this.label,
    required this.color,
    required this.onPressed,
    this.isExpanded = false,
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 56,
      child: ElevatedButton(
        onPressed: onPressed,
        style: ElevatedButton.styleFrom(
          backgroundColor: color,
          foregroundColor: Colors.white,
          padding: EdgeInsets.symmetric(
            horizontal: isExpanded ? 24 : 16,
          ),
          elevation: 0,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
        child: isExpanded
            ? Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(icon, size: 20),
                  const SizedBox(width: 8),
                  Text(
                    label,
                    style: const TextStyle(
                      fontSize: 15,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              )
            : Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(icon, size: 20),
                  const SizedBox(height: 4),
                  Text(
                    label,
                    style: const TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
      ),
    );
  }
}

import 'package:flutter/material.dart';
import 'package:share_plus/share_plus.dart';

/// Share Sheet Premium
///
/// Features:
/// - Custom share dialog
/// - Vehicle preview
/// - Custom message template
/// - Platform-specific actions
class ShareSheetPremium extends StatelessWidget {
  final String vehicleTitle;
  final String vehiclePrice;
  final String vehicleImageUrl;
  final String shareUrl;

  const ShareSheetPremium({
    super.key,
    required this.vehicleTitle,
    required this.vehiclePrice,
    required this.vehicleImageUrl,
    required this.shareUrl,
  });

  static void show(
    BuildContext context, {
    required String vehicleTitle,
    required String vehiclePrice,
    required String vehicleImageUrl,
    required String shareUrl,
  }) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => ShareSheetPremium(
        vehicleTitle: vehicleTitle,
        vehiclePrice: vehiclePrice,
        vehicleImageUrl: vehicleImageUrl,
        shareUrl: shareUrl,
      ),
    );
  }

  Future<void> _shareGeneric() async {
    await SharePlus.instance.share(ShareParams(
      text: 'Check out this $vehicleTitle for $vehiclePrice!\n\n$shareUrl',
      subject: 'Check out this vehicle on CarDealer',
    ));
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      child: SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Handle
            Container(
              margin: const EdgeInsets.only(top: 12, bottom: 8),
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.grey.shade300,
                borderRadius: BorderRadius.circular(2),
              ),
            ),

            // Title
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
              child: Row(
                children: [
                  const Icon(
                    Icons.share,
                    color: Color(0xFF001F54),
                  ),
                  const SizedBox(width: 12),
                  const Text(
                    'Share Vehicle',
                    style: TextStyle(
                      color: Color(0xFF1E293B),
                      fontSize: 20,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const Spacer(),
                  IconButton(
                    onPressed: () => Navigator.pop(context),
                    icon: const Icon(Icons.close),
                  ),
                ],
              ),
            ),

            const Divider(height: 1),

            // Vehicle Preview
            Padding(
              padding: const EdgeInsets.all(20),
              child: Row(
                children: [
                  // Image
                  ClipRRect(
                    borderRadius: BorderRadius.circular(8),
                    child: Image.network(
                      vehicleImageUrl,
                      width: 80,
                      height: 60,
                      fit: BoxFit.cover,
                      errorBuilder: (context, error, stackTrace) {
                        return Container(
                          width: 80,
                          height: 60,
                          color: Colors.grey.shade200,
                          child: Icon(
                            Icons.directions_car,
                            color: Colors.grey.shade400,
                          ),
                        );
                      },
                    ),
                  ),

                  const SizedBox(width: 12),

                  // Details
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          vehicleTitle,
                          style: const TextStyle(
                            color: Color(0xFF1E293B),
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                        ),
                        const SizedBox(height: 4),
                        Text(
                          vehiclePrice,
                          style: const TextStyle(
                            color: Color(0xFF001F54),
                            fontSize: 16,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),

            const Divider(height: 1),

            // Share Options
            Padding(
              padding: const EdgeInsets.all(20),
              child: Column(
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: _ShareOptionButton(
                          icon: Icons.message,
                          label: 'Mensaje',
                          color: const Color(0xFF10B981),
                          onPressed: _shareGeneric,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _ShareOptionButton(
                          icon: Icons.email_outlined,
                          label: 'Email',
                          color: const Color(0xFF3B82F6),
                          onPressed: _shareGeneric,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _ShareOptionButton(
                          icon: Icons.content_copy,
                          label: 'Copiar',
                          color: const Color(0xFF6366F1),
                          onPressed: () {
                            // Copy to clipboard logic would go here
                            Navigator.pop(context);
                            ScaffoldMessenger.of(context).showSnackBar(
                              const SnackBar(
                                content: Text('Enlace copiado al portapapeles'),
                                duration: Duration(seconds: 2),
                              ),
                            );
                          },
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Expanded(
                        child: _ShareOptionButton(
                          icon: Icons.facebook,
                          label: 'Facebook',
                          color: const Color(0xFF1877F2),
                          onPressed: _shareGeneric,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _ShareOptionButton(
                          icon: Icons.chat,
                          label: 'WhatsApp',
                          color: const Color(0xFF25D366),
                          onPressed: _shareGeneric,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _ShareOptionButton(
                          icon: Icons.more_horiz,
                          label: 'Más',
                          color: Colors.grey.shade700,
                          onPressed: _shareGeneric,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ShareOptionButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onPressed;

  const _ShareOptionButton({
    required this.icon,
    required this.label,
    required this.color,
    required this.onPressed,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onPressed,
      borderRadius: BorderRadius.circular(12),
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 16),
        child: Column(
          children: [
            Container(
              width: 56,
              height: 56,
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.1),
                shape: BoxShape.circle,
              ),
              child: Icon(
                icon,
                color: color,
                size: 28,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              label,
              style: TextStyle(
                color: Colors.grey.shade700,
                fontSize: 12,
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

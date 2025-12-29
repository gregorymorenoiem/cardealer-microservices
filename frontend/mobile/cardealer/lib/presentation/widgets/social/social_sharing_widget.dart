import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:share_plus/share_plus.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// SF-007: Social Sharing Premium
/// Sistema avanzado de compartición social con templates personalizados,
/// tracking de shares y análisis de engagement

class SocialSharingWidget extends StatefulWidget {
  final String vehicleId;
  final String vehicleName;
  final String vehiclePrice;
  final String vehicleImageUrl;
  final bool isPremium;

  const SocialSharingWidget({
    super.key,
    required this.vehicleId,
    required this.vehicleName,
    required this.vehiclePrice,
    required this.vehicleImageUrl,
    this.isPremium = false,
  });

  @override
  State<SocialSharingWidget> createState() => _SocialSharingWidgetState();
}

class _SocialSharingWidgetState extends State<SocialSharingWidget> {
  String _selectedTemplate = 'modern';
  bool _includeQR = true;
  bool _includeReferralLink = true;
  Map<String, int> _shareStats = {};

  @override
  void initState() {
    super.initState();
    _loadShareStats();
  }

  Future<void> _loadShareStats() async {
    // Simular carga de estadísticas
    await Future.delayed(const Duration(milliseconds: 500));
    setState(() {
      _shareStats = {
        'whatsapp': 45,
        'facebook': 32,
        'instagram': 28,
        'twitter': 15,
        'email': 12,
        'link': 67,
      };
    });
  }

  @override
  Widget build(BuildContext context) {
    return IconButton(
      icon: const Icon(Icons.share),
      onPressed: () => _showSharingSheet(context),
      tooltip: 'Compartir',
    );
  }

  void _showSharingSheet(BuildContext context) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.7,
        minChildSize: 0.5,
        maxChildSize: 0.95,
        expand: false,
        builder: (context, scrollController) {
          return SingleChildScrollView(
            controller: scrollController,
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Header
                  Center(
                    child: Container(
                      width: 40,
                      height: 4,
                      decoration: BoxDecoration(
                        color: Colors.grey[300],
                        borderRadius: BorderRadius.circular(2),
                      ),
                    ),
                  ),
                  const SizedBox(height: AppSpacing.lg),

                  Text(
                    'Compartir Vehículo',
                    style: AppTypography.h5.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: AppSpacing.md),

                  // Vehicle Preview Card
                  _buildVehiclePreview(),
                  const SizedBox(height: AppSpacing.xl),

                  // Share Options Grid
                  Text(
                    'Compartir en:',
                    style: AppTypography.h6.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: AppSpacing.md),
                  _buildShareOptionsGrid(),

                  if (widget.isPremium) ...[
                    const SizedBox(height: AppSpacing.xl),
                    const Divider(),
                    const SizedBox(height: AppSpacing.md),

                    // Premium Features
                    Text(
                      'Opciones Premium',
                      style: AppTypography.h6.copyWith(
                        fontWeight: FontWeight.w600,
                        color: AppColors.primary,
                      ),
                    ),
                    const SizedBox(height: AppSpacing.md),

                    // Template Selector
                    _buildTemplateSelector(),
                    const SizedBox(height: AppSpacing.md),

                    // Additional Options
                    _buildPremiumOptions(),
                    const SizedBox(height: AppSpacing.md),

                    // Share Stats
                    _buildShareStats(),
                  ],

                  const SizedBox(height: AppSpacing.xl),

                  // Generate Link Button
                  SizedBox(
                    width: double.infinity,
                    child: FilledButton.icon(
                      onPressed: _generateShareLink,
                      icon: const Icon(Icons.link),
                      label: const Text('Generar Link Personalizado'),
                      style: FilledButton.styleFrom(
                        padding: const EdgeInsets.all(AppSpacing.md),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildVehiclePreview() {
    return Card(
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Row(
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: Image.network(
                widget.vehicleImageUrl,
                width: 80,
                height: 60,
                fit: BoxFit.cover,
              ),
            ),
            const SizedBox(width: AppSpacing.md),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    widget.vehicleName,
                    style: AppTypography.bodyLarge.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    widget.vehiclePrice,
                    style: AppTypography.bodyMedium.copyWith(
                      color: AppColors.primary,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildShareOptionsGrid() {
    final options = [
      ShareOption(
        name: 'WhatsApp',
        icon: Icons.message,
        color: const Color(0xFF25D366),
        onTap: () => _shareVia('whatsapp'),
      ),
      ShareOption(
        name: 'Facebook',
        icon: Icons.facebook,
        color: const Color(0xFF1877F2),
        onTap: () => _shareVia('facebook'),
      ),
      ShareOption(
        name: 'Instagram',
        icon: Icons.camera_alt,
        color: const Color(0xFFE4405F),
        onTap: () => _shareVia('instagram'),
      ),
      ShareOption(
        name: 'X (Twitter)',
        icon: Icons.flutter_dash,
        color: Colors.black,
        onTap: () => _shareVia('twitter'),
      ),
      ShareOption(
        name: 'Email',
        icon: Icons.email,
        color: AppColors.info,
        onTap: () => _shareVia('email'),
      ),
      ShareOption(
        name: 'Más',
        icon: Icons.more_horiz,
        color: Colors.grey,
        onTap: () => _shareVia('more'),
      ),
    ];

    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 4,
        crossAxisSpacing: AppSpacing.md,
        mainAxisSpacing: AppSpacing.md,
        childAspectRatio: 0.8,
      ),
      itemCount: options.length,
      itemBuilder: (context, index) {
        final option = options[index];
        return InkWell(
          onTap: option.onTap,
          borderRadius: BorderRadius.circular(12),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Container(
                width: 56,
                height: 56,
                decoration: BoxDecoration(
                  color: option.color.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(
                  option.icon,
                  color: option.color,
                  size: 28,
                ),
              ),
              const SizedBox(height: AppSpacing.xs),
              Text(
                option.name,
                style: AppTypography.bodySmall,
                textAlign: TextAlign.center,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
              if (_shareStats
                  .containsKey(option.name.toLowerCase().split(' ')[0]))
                Text(
                  '${_shareStats[option.name.toLowerCase().split(' ')[0]]}',
                  style: AppTypography.bodySmall.copyWith(
                    color: Colors.grey[600],
                    fontSize: 10,
                  ),
                ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildTemplateSelector() {
    final templates = [
      {'id': 'modern', 'name': 'Moderno', 'icon': Icons.photo_filter},
      {'id': 'minimal', 'name': 'Minimalista', 'icon': Icons.crop_square},
      {'id': 'detailed', 'name': 'Detallado', 'icon': Icons.view_list},
      {'id': 'story', 'name': 'Story', 'icon': Icons.auto_stories},
    ];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Template de compartición',
          style: AppTypography.bodyMedium.copyWith(
            fontWeight: FontWeight.w600,
          ),
        ),
        const SizedBox(height: AppSpacing.sm),
        SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          child: Row(
            children: templates.map((template) {
              final isSelected = _selectedTemplate == template['id'];
              return Padding(
                padding: const EdgeInsets.only(right: AppSpacing.sm),
                child: ChoiceChip(
                  label: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(
                        template['icon'] as IconData,
                        size: 16,
                        color: isSelected ? Colors.white : Colors.grey[700],
                      ),
                      const SizedBox(width: AppSpacing.xs),
                      Text(template['name'] as String),
                    ],
                  ),
                  selected: isSelected,
                  onSelected: (selected) {
                    if (selected) {
                      setState(() {
                        _selectedTemplate = template['id'] as String;
                      });
                    }
                  },
                  selectedColor: AppColors.primary,
                  labelStyle: TextStyle(
                    color: isSelected ? Colors.white : Colors.grey[700],
                  ),
                ),
              );
            }).toList(),
          ),
        ),
      ],
    );
  }

  Widget _buildPremiumOptions() {
    return Column(
      children: [
        SwitchListTile(
          value: _includeQR,
          onChanged: (value) => setState(() => _includeQR = value),
          title: const Text('Incluir código QR'),
          subtitle: const Text('Para escaneo rápido'),
          secondary: const Icon(Icons.qr_code),
          activeThumbColor: AppColors.primary,
        ),
        SwitchListTile(
          value: _includeReferralLink,
          onChanged: (value) => setState(() => _includeReferralLink = value),
          title: const Text('Link de referido'),
          subtitle: const Text('Gana comisión por compartir'),
          secondary: const Icon(Icons.monetization_on),
          activeThumbColor: AppColors.primary,
        ),
      ],
    );
  }

  Widget _buildShareStats() {
    final totalShares =
        _shareStats.values.fold<int>(0, (sum, val) => sum + val);

    return Card(
      color: AppColors.primary.withValues(alpha: 0.1),
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                const Icon(Icons.analytics, color: AppColors.primary, size: 20),
                const SizedBox(width: AppSpacing.sm),
                Text(
                  'Estadísticas de Compartición',
                  style: AppTypography.bodyLarge.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
            const SizedBox(height: AppSpacing.md),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                _buildStatColumn('Total Shares', '$totalShares'),
                _buildStatColumn('Vistas', '${totalShares * 3}'),
                _buildStatColumn('Clicks', '${(totalShares * 0.6).round()}'),
              ],
            ),
            const SizedBox(height: AppSpacing.md),
            LinearProgressIndicator(
              value: 0.65,
              backgroundColor: Colors.grey[300],
              valueColor:
                  const AlwaysStoppedAnimation<Color>(AppColors.primary),
            ),
            const SizedBox(height: AppSpacing.xs),
            Text(
              '65% de engagement rate',
              style: AppTypography.bodySmall.copyWith(
                color: Colors.grey[600],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatColumn(String label, String value) {
    return Column(
      children: [
        Text(
          value,
          style: AppTypography.h5.copyWith(
            fontWeight: FontWeight.bold,
            color: AppColors.primary,
          ),
        ),
        const SizedBox(height: 4),
        Text(
          label,
          style: AppTypography.bodySmall.copyWith(
            color: Colors.grey[600],
          ),
        ),
      ],
    );
  }

  Future<void> _shareVia(String platform) async {
    // Generar contenido según template
    final content = _generateShareContent();

    // Tracking del share
    _trackShare(platform);

    // Compartir según plataforma
    switch (platform) {
      case 'whatsapp':
        // Share via WhatsApp
        await SharePlus.instance.share(
          ShareParams(text: content, title: widget.vehicleName),
        );
        break;
      case 'facebook':
      case 'instagram':
      case 'twitter':
      case 'email':
        await SharePlus.instance.share(
          ShareParams(text: content, title: widget.vehicleName),
        );
        break;
      case 'more':
        await SharePlus.instance.share(
          ShareParams(text: content, title: widget.vehicleName),
        );
        break;
    }

    if (mounted) {
      Navigator.pop(context);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Row(
            children: [
              const Icon(Icons.check_circle, color: Colors.white),
              const SizedBox(width: AppSpacing.sm),
              Text('Compartido en ${platform.toUpperCase()}'),
            ],
          ),
        ),
      );
    }
  }

  String _generateShareContent() {
    final baseUrl = 'https://cardealer.com/vehicle/${widget.vehicleId}';
    final referralCode = _includeReferralLink ? '?ref=USER123' : '';
    final link = '$baseUrl$referralCode';

    switch (_selectedTemplate) {
      case 'minimal':
        return '${widget.vehicleName}\n${widget.vehiclePrice}\n$link';

      case 'detailed':
        return '''
🚗 ${widget.vehicleName}
💰 ${widget.vehiclePrice}

✨ Características destacadas
🔧 Excelente estado
📍 Disponible ahora

Ver más detalles: $link

#CarDealer #VehículosEnVenta
        ''';

      case 'story':
        return '''
¡Mira este increíble vehículo! 🤩

${widget.vehicleName}
${widget.vehiclePrice}

¡No te lo pierdas! 👇
$link
        ''';

      default: // modern
        return '''
✨ ${widget.vehicleName}

💰 ${widget.vehiclePrice}
🚗 Excelente oportunidad

Ver detalles: $link
        ''';
    }
  }

  void _trackShare(String platform) {
    setState(() {
      final key = platform.toLowerCase();
      _shareStats[key] = (_shareStats[key] ?? 0) + 1;
    });
  }

  Future<void> _generateShareLink() async {
    final link = 'https://cardealer.com/vehicle/${widget.vehicleId}';
    final referralCode = _includeReferralLink ? '?ref=USER123' : '';
    final fullLink = '$link$referralCode';

    await Clipboard.setData(ClipboardData(text: fullLink));

    if (mounted) {
      Navigator.pop(context);

      showDialog(
        context: context,
        builder: (context) => AlertDialog(
          title: const Row(
            children: [
              Icon(Icons.check_circle, color: AppColors.success),
              SizedBox(width: AppSpacing.sm),
              Text('Link Generado'),
            ],
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text('Tu link personalizado:'),
              const SizedBox(height: AppSpacing.md),
              Container(
                padding: const EdgeInsets.all(AppSpacing.md),
                decoration: BoxDecoration(
                  color: Colors.grey[100],
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.grey[300]!),
                ),
                child: SelectableText(
                  fullLink,
                  style: AppTypography.bodySmall.copyWith(
                    fontFamily: 'monospace',
                  ),
                ),
              ),
              if (_includeQR) ...[
                const SizedBox(height: AppSpacing.md),
                const Text('Código QR:'),
                const SizedBox(height: AppSpacing.sm),
                Center(
                  child: Container(
                    width: 150,
                    height: 150,
                    decoration: BoxDecoration(
                      border: Border.all(color: Colors.grey[300]!),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: const Center(
                      child: Icon(Icons.qr_code, size: 100),
                    ),
                  ),
                ),
              ],
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Cerrar'),
            ),
            FilledButton.icon(
              onPressed: () async {
                await Clipboard.setData(ClipboardData(text: fullLink));
                if (context.mounted) {
                  Navigator.pop(context);
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Link copiado')),
                  );
                }
              },
              icon: const Icon(Icons.copy),
              label: const Text('Copiar'),
            ),
          ],
        ),
      );
    }
  }
}

class ShareOption {
  final String name;
  final IconData icon;
  final Color color;
  final VoidCallback onTap;

  ShareOption({
    required this.name,
    required this.icon,
    required this.color,
    required this.onTap,
  });
}

/// Widget compacto para botón de compartir rápido
class QuickShareButton extends StatelessWidget {
  final String vehicleId;
  final String vehicleName;
  final String vehiclePrice;
  final String vehicleImageUrl;

  const QuickShareButton({
    super.key,
    required this.vehicleId,
    required this.vehicleName,
    required this.vehiclePrice,
    required this.vehicleImageUrl,
  });

  @override
  Widget build(BuildContext context) {
    return IconButton(
      icon: const Icon(Icons.share),
      onPressed: () async {
        final content = '''
$vehicleName
$vehiclePrice

https://cardealer.com/vehicle/$vehicleId
        ''';

        await SharePlus.instance.share(
          ShareParams(text: content, title: vehicleName),
        );
      },
    );
  }
}

/// Widget para mostrar historial de shares
class ShareHistoryWidget extends StatefulWidget {
  final String userId;

  const ShareHistoryWidget({
    super.key,
    required this.userId,
  });

  @override
  State<ShareHistoryWidget> createState() => _ShareHistoryWidgetState();
}

class _ShareHistoryWidgetState extends State<ShareHistoryWidget> {
  List<ShareHistoryItem> _history = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadHistory();
  }

  Future<void> _loadHistory() async {
    setState(() => _isLoading = true);
    await Future.delayed(const Duration(seconds: 1));

    setState(() {
      _history = List.generate(10, (index) {
        return ShareHistoryItem(
          vehicleName: 'Toyota Corolla ${2020 + index}',
          platform: ['WhatsApp', 'Facebook', 'Instagram', 'Twitter'][index % 4],
          timestamp: DateTime.now().subtract(Duration(days: index)),
          views: 15 + index * 3,
          clicks: 5 + index,
        );
      });
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    return ListView.builder(
      padding: const EdgeInsets.all(AppSpacing.md),
      itemCount: _history.length,
      itemBuilder: (context, index) {
        final item = _history[index];
        return Card(
          margin: const EdgeInsets.only(bottom: AppSpacing.md),
          child: ListTile(
            leading: CircleAvatar(
              backgroundColor: AppColors.primary.withValues(alpha: 0.1),
              child: const Icon(Icons.share, color: AppColors.primary),
            ),
            title: Text(item.vehicleName),
            subtitle: Text(
              '${item.platform} • ${_formatDate(item.timestamp)}',
              style: AppTypography.bodySmall,
            ),
            trailing: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Text(
                  '${item.views} vistas',
                  style: AppTypography.bodySmall.copyWith(
                    color: AppColors.success,
                  ),
                ),
                Text(
                  '${item.clicks} clicks',
                  style: AppTypography.bodySmall.copyWith(
                    color: AppColors.primary,
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  String _formatDate(DateTime date) {
    final now = DateTime.now();
    final diff = now.difference(date);

    if (diff.inDays == 0) return 'Hoy';
    if (diff.inDays == 1) return 'Ayer';
    if (diff.inDays < 7) return 'Hace ${diff.inDays}d';
    return '${date.day}/${date.month}/${date.year}';
  }
}

class ShareHistoryItem {
  final String vehicleName;
  final String platform;
  final DateTime timestamp;
  final int views;
  final int clicks;

  ShareHistoryItem({
    required this.vehicleName,
    required this.platform,
    required this.timestamp,
    required this.views,
    required this.clicks,
  });
}

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:intl/intl.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// SF-009: Referral System UI
/// Sistema completo de referidos con tracking, recompensas y gamificación

class ReferralSystemPage extends StatefulWidget {
  const ReferralSystemPage({super.key});

  @override
  State<ReferralSystemPage> createState() => _ReferralSystemPageState();
}

class _ReferralSystemPageState extends State<ReferralSystemPage>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;

  // User data
  final String _referralCode = 'CARLOS2024';
  final String _referralLink = 'https://cardealer.com/ref/CARLOS2024';
  int _totalReferrals = 0;
  double _totalEarnings = 0;
  int _pendingReferrals = 0;
  final int _currentLevel = 3;
  // ignore: unused_field
  final int _pointsToNextLevel = 250;

  List<ReferralActivity> _activities = [];
  List<ReferralReward> _rewards = [];
  List<ReferralTier> _tiers = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _loadData();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadData() async {
    setState(() => _isLoading = true);
    await Future.delayed(const Duration(seconds: 1));

    setState(() {
      _totalReferrals = 12;
      _totalEarnings = 1450.00;
      _pendingReferrals = 3;
      _activities = _generateMockActivities();
      _rewards = _generateMockRewards();
      _tiers = _generateMockTiers();
      _isLoading = false;
    });
  }

  List<ReferralActivity> _generateMockActivities() {
    final now = DateTime.now();
    return [
      ReferralActivity(
        name: 'Juan Pérez',
        email: 'juan@email.com',
        status: ReferralStatus.completed,
        reward: 150.0,
        date: now.subtract(const Duration(days: 2)),
        vehiclePurchased: 'Toyota Corolla 2022',
      ),
      ReferralActivity(
        name: 'María García',
        email: 'maria@email.com',
        status: ReferralStatus.pending,
        reward: 100.0,
        date: now.subtract(const Duration(days: 5)),
      ),
      ReferralActivity(
        name: 'Pedro López',
        email: 'pedro@email.com',
        status: ReferralStatus.registered,
        reward: 50.0,
        date: now.subtract(const Duration(days: 7)),
      ),
    ];
  }

  List<ReferralReward> _generateMockRewards() {
    return [
      ReferralReward(
        title: '5 Referidos',
        description: '\$500 de bono',
        requiredReferrals: 5,
        currentProgress: 5,
        isCompleted: true,
        rewardAmount: 500.0,
      ),
      ReferralReward(
        title: '10 Referidos',
        description: '\$1,200 de bono',
        requiredReferrals: 10,
        currentProgress: 10,
        isCompleted: true,
        rewardAmount: 1200.0,
      ),
      ReferralReward(
        title: '20 Referidos',
        description: '\$3,000 de bono + Premium',
        requiredReferrals: 20,
        currentProgress: 12,
        isCompleted: false,
        rewardAmount: 3000.0,
      ),
      ReferralReward(
        title: '50 Referidos',
        description: '\$10,000 de bono + Viaje',
        requiredReferrals: 50,
        currentProgress: 12,
        isCompleted: false,
        rewardAmount: 10000.0,
      ),
    ];
  }

  List<ReferralTier> _generateMockTiers() {
    return [
      ReferralTier(
        level: 1,
        name: 'Bronce',
        icon: Icons.workspace_premium,
        color: const Color(0xFFCD7F32),
        minReferrals: 0,
        commissionRate: 5.0,
      ),
      ReferralTier(
        level: 2,
        name: 'Plata',
        icon: Icons.workspace_premium,
        color: const Color(0xFFC0C0C0),
        minReferrals: 5,
        commissionRate: 7.5,
      ),
      ReferralTier(
        level: 3,
        name: 'Oro',
        icon: Icons.workspace_premium,
        color: const Color(0xFFFFD700),
        minReferrals: 10,
        commissionRate: 10.0,
      ),
      ReferralTier(
        level: 4,
        name: 'Platino',
        icon: Icons.diamond,
        color: const Color(0xFFE5E4E2),
        minReferrals: 20,
        commissionRate: 12.5,
      ),
      ReferralTier(
        level: 5,
        name: 'Diamante',
        icon: Icons.diamond,
        color: const Color(0xFF00BFFF),
        minReferrals: 50,
        commissionRate: 15.0,
      ),
    ];
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    final currentTier = _tiers.firstWhere((t) => t.level == _currentLevel);

    return Scaffold(
      body: CustomScrollView(
        slivers: [
          // Hero Header
          SliverAppBar(
            expandedHeight: 280,
            pinned: true,
            flexibleSpace: FlexibleSpaceBar(
              background: Container(
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                    colors: [
                      AppColors.primary,
                      AppColors.primary.withValues(alpha: 0.7),
                    ],
                  ),
                ),
                child: SafeArea(
                  child: Padding(
                    padding: const EdgeInsets.all(AppSpacing.lg),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Spacer(),
                        Row(
                          children: [
                            Icon(
                              currentTier.icon,
                              color: currentTier.color,
                              size: 40,
                            ),
                            const SizedBox(width: AppSpacing.md),
                            Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  'Nivel ${currentTier.name}',
                                  style: AppTypography.h5.copyWith(
                                    color: Colors.white,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                Text(
                                  'Comisión: ${currentTier.commissionRate}%',
                                  style: AppTypography.bodyMedium.copyWith(
                                    color: Colors.white70,
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                        const SizedBox(height: AppSpacing.lg),
                        Row(
                          children: [
                            Expanded(
                              child: _buildStatCard(
                                'Total Referidos',
                                '$_totalReferrals',
                                Icons.people,
                              ),
                            ),
                            const SizedBox(width: AppSpacing.md),
                            Expanded(
                              child: _buildStatCard(
                                'Ganancias',
                                '\$${NumberFormat('#,###').format(_totalEarnings)}',
                                Icons.attach_money,
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ),

          // Tabs
          SliverPersistentHeader(
            pinned: true,
            delegate: _SliverTabBarDelegate(
              TabBar(
                controller: _tabController,
                tabs: const [
                  Tab(text: 'Compartir'),
                  Tab(text: 'Actividad'),
                  Tab(text: 'Recompensas'),
                ],
              ),
            ),
          ),

          // Tab Content
          SliverFillRemaining(
            child: TabBarView(
              controller: _tabController,
              children: [
                _buildShareTab(),
                _buildActivityTab(),
                _buildRewardsTab(),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatCard(String label, String value, IconData icon) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.2),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, color: Colors.white, size: 20),
              const Spacer(),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          Text(
            value,
            style: AppTypography.h4.copyWith(
              color: Colors.white,
              fontWeight: FontWeight.bold,
            ),
          ),
          Text(
            label,
            style: AppTypography.bodySmall.copyWith(
              color: Colors.white70,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildShareTab() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Referral Code Card
          Card(
            elevation: 4,
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: Column(
                children: [
                  Text(
                    'Tu Código de Referido',
                    style: AppTypography.h6.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: AppSpacing.md),
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.xl,
                      vertical: AppSpacing.md,
                    ),
                    decoration: BoxDecoration(
                      color: AppColors.primary.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(
                        color: AppColors.primary,
                        width: 2,
                        style: BorderStyle.solid,
                      ),
                    ),
                    child: Text(
                      _referralCode,
                      style: AppTypography.h4.copyWith(
                        fontWeight: FontWeight.bold,
                        color: AppColors.primary,
                        letterSpacing: 4,
                      ),
                    ),
                  ),
                  const SizedBox(height: AppSpacing.lg),
                  Row(
                    children: [
                      Expanded(
                        child: OutlinedButton.icon(
                          onPressed: _copyCode,
                          icon: const Icon(Icons.copy),
                          label: const Text('Copiar Código'),
                        ),
                      ),
                      const SizedBox(width: AppSpacing.md),
                      Expanded(
                        child: FilledButton.icon(
                          onPressed: _shareCode,
                          icon: const Icon(Icons.share),
                          label: const Text('Compartir'),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.xl),

          // Referral Link
          Text(
            'Link de Referido',
            style: AppTypography.h6.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: Row(
                children: [
                  Expanded(
                    child: SelectableText(
                      _referralLink,
                      style: AppTypography.bodyMedium.copyWith(
                        color: AppColors.primary,
                      ),
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.copy),
                    onPressed: _copyLink,
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.xl),

          // Share Options
          Text(
            'Compartir en Redes Sociales',
            style: AppTypography.h6.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          _buildSocialShareButtons(),
          const SizedBox(height: AppSpacing.xl),

          // How it Works
          Card(
            color: AppColors.info.withValues(alpha: 0.1),
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.info_outline, color: AppColors.info),
                      const SizedBox(width: AppSpacing.sm),
                      Text(
                        '¿Cómo Funciona?',
                        style: AppTypography.h6.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: AppSpacing.md),
                  _buildStep('1', 'Comparte tu código con amigos'),
                  _buildStep('2', 'Tus amigos se registran usando tu código'),
                  _buildStep('3', 'Cuando compren un vehículo, ambos ganan'),
                  _buildStep('4', 'Acumula puntos y sube de nivel'),
                ],
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.xl),

          // Level Progress
          _buildLevelProgress(),
        ],
      ),
    );
  }

  Widget _buildSocialShareButtons() {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceAround,
      children: [
        _buildSocialButton(
          icon: Icons.message,
          label: 'WhatsApp',
          color: const Color(0xFF25D366),
          onTap: () => _shareOnSocial('whatsapp'),
        ),
        _buildSocialButton(
          icon: Icons.facebook,
          label: 'Facebook',
          color: const Color(0xFF1877F2),
          onTap: () => _shareOnSocial('facebook'),
        ),
        _buildSocialButton(
          icon: Icons.flutter_dash,
          label: 'X',
          color: Colors.black,
          onTap: () => _shareOnSocial('twitter'),
        ),
        _buildSocialButton(
          icon: Icons.email,
          label: 'Email',
          color: AppColors.info,
          onTap: () => _shareOnSocial('email'),
        ),
      ],
    );
  }

  Widget _buildSocialButton({
    required IconData icon,
    required String label,
    required Color color,
    required VoidCallback onTap,
  }) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(12),
      child: Column(
        children: [
          Container(
            width: 56,
            height: 56,
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(icon, color: color, size: 28),
          ),
          const SizedBox(height: AppSpacing.xs),
          Text(label, style: AppTypography.bodySmall),
        ],
      ),
    );
  }

  Widget _buildStep(String number, String text) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.sm),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          CircleAvatar(
            radius: 14,
            backgroundColor: AppColors.primary,
            child: Text(
              number,
              style: AppTypography.bodySmall.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: Padding(
              padding: const EdgeInsets.only(top: 2),
              child: Text(text, style: AppTypography.bodyMedium),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLevelProgress() {
    final nextTier = _tiers.firstWhere((t) => t.level == _currentLevel + 1);
    final progress = _totalReferrals / nextTier.minReferrals;

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                const Icon(Icons.trending_up, color: AppColors.primary),
                const SizedBox(width: AppSpacing.sm),
                Text(
                  'Progreso al Siguiente Nivel',
                  style: AppTypography.h6.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
            const SizedBox(height: AppSpacing.md),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Nivel ${nextTier.name}',
                  style: AppTypography.bodyLarge.copyWith(
                    fontWeight: FontWeight.w600,
                    color: nextTier.color,
                  ),
                ),
                Text(
                  '$_totalReferrals / ${nextTier.minReferrals}',
                  style: AppTypography.bodyMedium.copyWith(
                    color: Colors.grey[600],
                  ),
                ),
              ],
            ),
            const SizedBox(height: AppSpacing.sm),
            LinearProgressIndicator(
              value: progress.clamp(0.0, 1.0),
              backgroundColor: Colors.grey[200],
              valueColor: AlwaysStoppedAnimation<Color>(nextTier.color),
            ),
            const SizedBox(height: AppSpacing.sm),
            Text(
              'Faltan ${nextTier.minReferrals - _totalReferrals} referidos',
              style: AppTypography.bodySmall.copyWith(
                color: Colors.grey[600],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildActivityTab() {
    if (_activities.isEmpty) {
      return _buildEmptyState('No hay actividad aún', Icons.people_outline);
    }

    return ListView.builder(
      padding: const EdgeInsets.all(AppSpacing.md),
      itemCount: _activities.length + 1,
      itemBuilder: (context, index) {
        if (index == 0) {
          return _buildPendingCard();
        }
        final activity = _activities[index - 1];
        return _buildActivityCard(activity);
      },
    );
  }

  Widget _buildPendingCard() {
    if (_pendingReferrals == 0) return const SizedBox.shrink();

    return Card(
      color: AppColors.warning.withValues(alpha: 0.1),
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Row(
          children: [
            const Icon(Icons.hourglass_empty, color: AppColors.warning),
            const SizedBox(width: AppSpacing.md),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '$_pendingReferrals referidos pendientes',
                    style: AppTypography.bodyLarge.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    'En proceso de validación',
                    style: AppTypography.bodySmall.copyWith(
                      color: Colors.grey[600],
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

  Widget _buildActivityCard(ReferralActivity activity) {
    return Card(
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                CircleAvatar(
                  backgroundColor:
                      _getStatusColor(activity.status).withValues(alpha: 0.2),
                  child: Text(
                    activity.name[0].toUpperCase(),
                    style: TextStyle(
                      color: _getStatusColor(activity.status),
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                const SizedBox(width: AppSpacing.md),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        activity.name,
                        style: AppTypography.bodyLarge.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Text(
                        activity.email,
                        style: AppTypography.bodySmall.copyWith(
                          color: Colors.grey[600],
                        ),
                      ),
                    ],
                  ),
                ),
                _buildStatusChip(activity.status),
              ],
            ),
            if (activity.vehiclePurchased != null) ...[
              const SizedBox(height: AppSpacing.sm),
              Row(
                children: [
                  const Icon(Icons.directions_car,
                      size: 16, color: Colors.grey),
                  const SizedBox(width: AppSpacing.xs),
                  Text(
                    activity.vehiclePurchased!,
                    style: AppTypography.bodySmall,
                  ),
                ],
              ),
            ],
            const SizedBox(height: AppSpacing.sm),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  DateFormat('d MMM yyyy', 'es').format(activity.date),
                  style: AppTypography.bodySmall.copyWith(
                    color: Colors.grey[600],
                  ),
                ),
                Text(
                  '+\$${activity.reward.toStringAsFixed(2)}',
                  style: AppTypography.bodyLarge.copyWith(
                    color: AppColors.success,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatusChip(ReferralStatus status) {
    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm,
        vertical: 4,
      ),
      decoration: BoxDecoration(
        color: _getStatusColor(status).withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        _getStatusText(status),
        style: AppTypography.bodySmall.copyWith(
          color: _getStatusColor(status),
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }

  Widget _buildRewardsTab() {
    return ListView.builder(
      padding: const EdgeInsets.all(AppSpacing.md),
      itemCount: _rewards.length,
      itemBuilder: (context, index) {
        final reward = _rewards[index];
        return _buildRewardCard(reward);
      },
    );
  }

  Widget _buildRewardCard(ReferralReward reward) {
    final progress = reward.currentProgress / reward.requiredReferrals;

    return Card(
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(
                  reward.isCompleted ? Icons.check_circle : Icons.emoji_events,
                  color: reward.isCompleted
                      ? AppColors.success
                      : AppColors.primary,
                  size: 32,
                ),
                const SizedBox(width: AppSpacing.md),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        reward.title,
                        style: AppTypography.h6.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Text(
                        reward.description,
                        style: AppTypography.bodyMedium.copyWith(
                          color: AppColors.primary,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ],
                  ),
                ),
                if (reward.isCompleted)
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.sm,
                      vertical: 4,
                    ),
                    decoration: BoxDecoration(
                      color: AppColors.success.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Text(
                      'Completado',
                      style: AppTypography.bodySmall.copyWith(
                        color: AppColors.success,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
              ],
            ),
            if (!reward.isCompleted) ...[
              const SizedBox(height: AppSpacing.md),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    'Progreso',
                    style: AppTypography.bodyMedium.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  Text(
                    '${reward.currentProgress} / ${reward.requiredReferrals}',
                    style: AppTypography.bodyMedium.copyWith(
                      color: Colors.grey[600],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.sm),
              LinearProgressIndicator(
                value: progress,
                backgroundColor: Colors.grey[200],
                valueColor:
                    const AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildEmptyState(String message, IconData icon) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, size: 80, color: Colors.grey[300]),
          const SizedBox(height: AppSpacing.lg),
          Text(
            message,
            style: AppTypography.h6.copyWith(
              color: Colors.grey[600],
            ),
          ),
        ],
      ),
    );
  }

  Color _getStatusColor(ReferralStatus status) {
    switch (status) {
      case ReferralStatus.completed:
        return AppColors.success;
      case ReferralStatus.pending:
        return AppColors.warning;
      case ReferralStatus.registered:
        return AppColors.info;
    }
  }

  String _getStatusText(ReferralStatus status) {
    switch (status) {
      case ReferralStatus.completed:
        return 'Completado';
      case ReferralStatus.pending:
        return 'Pendiente';
      case ReferralStatus.registered:
        return 'Registrado';
    }
  }

  void _copyCode() {
    Clipboard.setData(ClipboardData(text: _referralCode));
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Código copiado al portapapeles')),
    );
  }

  void _copyLink() {
    Clipboard.setData(ClipboardData(text: _referralLink));
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Link copiado al portapapeles')),
    );
  }

  void _shareCode() {
    // Implement share functionality
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Compartiendo código...')),
    );
  }

  void _shareOnSocial(String platform) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Compartiendo en ${platform.toUpperCase()}...')),
    );
  }
}

// Tab Bar Delegate
class _SliverTabBarDelegate extends SliverPersistentHeaderDelegate {
  _SliverTabBarDelegate(this._tabBar);

  final TabBar _tabBar;

  @override
  double get minExtent => _tabBar.preferredSize.height;
  @override
  double get maxExtent => _tabBar.preferredSize.height;

  @override
  Widget build(
      BuildContext context, double shrinkOffset, bool overlapsContent) {
    return Container(
      color: Theme.of(context).scaffoldBackgroundColor,
      child: _tabBar,
    );
  }

  @override
  bool shouldRebuild(_SliverTabBarDelegate oldDelegate) {
    return false;
  }
}

// Models
class ReferralActivity {
  final String name;
  final String email;
  final ReferralStatus status;
  final double reward;
  final DateTime date;
  final String? vehiclePurchased;

  ReferralActivity({
    required this.name,
    required this.email,
    required this.status,
    required this.reward,
    required this.date,
    this.vehiclePurchased,
  });
}

class ReferralReward {
  final String title;
  final String description;
  final int requiredReferrals;
  final int currentProgress;
  final bool isCompleted;
  final double rewardAmount;

  ReferralReward({
    required this.title,
    required this.description,
    required this.requiredReferrals,
    required this.currentProgress,
    required this.isCompleted,
    required this.rewardAmount,
  });
}

class ReferralTier {
  final int level;
  final String name;
  final IconData icon;
  final Color color;
  final int minReferrals;
  final double commissionRate;

  ReferralTier({
    required this.level,
    required this.name,
    required this.icon,
    required this.color,
    required this.minReferrals,
    required this.commissionRate,
  });
}

enum ReferralStatus {
  completed,
  pending,
  registered,
}

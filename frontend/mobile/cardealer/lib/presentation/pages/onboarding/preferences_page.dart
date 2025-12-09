import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';
import '../../widgets/custom_button.dart';
import '../home/home_page.dart';

/// Category option model
class CategoryOption {
  final String id;
  final String name;
  final IconData icon;
  final Color color;

  const CategoryOption({
    required this.id,
    required this.name,
    required this.icon,
    required this.color,
  });
}

/// Price range option model
class PriceRangeOption {
  final String id;
  final String label;
  final String range;
  final IconData icon;

  const PriceRangeOption({
    required this.id,
    required this.label,
    required this.range,
    required this.icon,
  });
}

/// Preferences selection page shown after onboarding
class PreferencesPage extends StatefulWidget {
  const PreferencesPage({super.key});

  @override
  State<PreferencesPage> createState() => _PreferencesPageState();
}

class _PreferencesPageState extends State<PreferencesPage>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;

  // User selections
  final Set<String> _selectedCategories = {};
  String? _selectedPriceRange;
  final Set<String> _selectedLocations = {};

  final List<CategoryOption> _categories = [
    const CategoryOption(
      id: 'sedan',
      name: 'Sedán',
      icon: Icons.directions_car,
      color: AppColors.primary,
    ),
    const CategoryOption(
      id: 'suv',
      name: 'SUV',
      icon: Icons.car_rental,
      color: AppColors.accent,
    ),
    const CategoryOption(
      id: 'pickup',
      name: 'Pickup',
      icon: Icons.local_shipping,
      color: AppColors.secondary,
    ),
    const CategoryOption(
      id: 'luxury',
      name: 'Lujo',
      icon: Icons.star,
      color: AppColors.warning,
    ),
    const CategoryOption(
      id: 'electric',
      name: 'Eléctrico',
      icon: Icons.electric_car,
      color: AppColors.success,
    ),
    const CategoryOption(
      id: 'sport',
      name: 'Deportivo',
      icon: Icons.sports_score,
      color: AppColors.error,
    ),
  ];

  final List<PriceRangeOption> _priceRanges = [
    const PriceRangeOption(
      id: 'budget',
      label: 'Económico',
      range: 'Hasta \$10,000',
      icon: Icons.savings,
    ),
    const PriceRangeOption(
      id: 'mid',
      label: 'Rango Medio',
      range: '\$10,000 - \$30,000',
      icon: Icons.account_balance_wallet,
    ),
    const PriceRangeOption(
      id: 'premium',
      label: 'Premium',
      range: '\$30,000 - \$60,000',
      icon: Icons.workspace_premium,
    ),
    const PriceRangeOption(
      id: 'luxury',
      label: 'Lujo',
      range: '\$60,000+',
      icon: Icons.diamond,
    ),
  ];

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 800),
      vsync: this,
    );

    _fadeAnimation = CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeIn,
    );

    _slideAnimation = Tween<Offset>(
      begin: const Offset(0, 0.1),
      end: Offset.zero,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeOutCubic,
    ));

    _animationController.forward();
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  Future<void> _completeSetup() async {
    final prefs = await SharedPreferences.getInstance();

    // Save preferences
    await prefs.setStringList(
        'selected_categories', _selectedCategories.toList());
    if (_selectedPriceRange != null) {
      await prefs.setString('price_range', _selectedPriceRange!);
    }
    await prefs.setStringList(
        'selected_locations', _selectedLocations.toList());
    await prefs.setBool('preferences_completed', true);

    if (!mounted) return;

    // Navigate to home
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(builder: (_) => const HomePage()),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: FadeTransition(
          opacity: _fadeAnimation,
          child: SlideTransition(
            position: _slideAnimation,
            child: CustomScrollView(
              slivers: [
                // Header
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.all(AppSpacing.xl),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Container(
                          padding: const EdgeInsets.all(AppSpacing.md),
                          decoration: BoxDecoration(
                            gradient: LinearGradient(
                              colors: [
                                AppColors.primary.withValues(alpha: 0.15),
                                AppColors.primary.withValues(alpha: 0.05),
                              ],
                            ),
                            borderRadius: BorderRadius.circular(16),
                          ),
                          child: const Icon(
                            Icons.tune,
                            size: 48,
                            color: AppColors.primary,
                          ),
                        ),
                        const SizedBox(height: AppSpacing.lg),
                        Text(
                          'Personaliza Tu Experiencia',
                          style: AppTypography.h1.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        const SizedBox(height: AppSpacing.sm),
                        Text(
                          'Ayúdanos a mostrarte los autos perfectos para ti',
                          style: AppTypography.bodyLarge.copyWith(
                            color: AppColors.textSecondary,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),

                // Categories Section
                SliverToBoxAdapter(
                  child: Padding(
                    padding:
                        const EdgeInsets.symmetric(horizontal: AppSpacing.xl),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Tipo de Vehículo',
                          style: AppTypography.h3.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        const SizedBox(height: AppSpacing.xs),
                        Text(
                          'Selecciona uno o más',
                          style: AppTypography.bodyMedium.copyWith(
                            color: AppColors.textSecondary,
                          ),
                        ),
                        const SizedBox(height: AppSpacing.md),
                      ],
                    ),
                  ),
                ),

                // Categories Grid
                SliverPadding(
                  padding:
                      const EdgeInsets.symmetric(horizontal: AppSpacing.xl),
                  sliver: SliverGrid(
                    gridDelegate:
                        const SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 2,
                      mainAxisSpacing: AppSpacing.md,
                      crossAxisSpacing: AppSpacing.md,
                      childAspectRatio: 1.2,
                    ),
                    delegate: SliverChildBuilderDelegate(
                      (context, index) {
                        final category = _categories[index];
                        final isSelected =
                            _selectedCategories.contains(category.id);

                        return InkWell(
                          onTap: () {
                            setState(() {
                              if (isSelected) {
                                _selectedCategories.remove(category.id);
                              } else {
                                _selectedCategories.add(category.id);
                              }
                            });
                          },
                          borderRadius: BorderRadius.circular(16),
                          child: AnimatedContainer(
                            duration: const Duration(milliseconds: 200),
                            decoration: BoxDecoration(
                              gradient: isSelected
                                  ? LinearGradient(
                                      begin: Alignment.topLeft,
                                      end: Alignment.bottomRight,
                                      colors: [
                                        category.color.withValues(alpha: 0.2),
                                        category.color.withValues(alpha: 0.05),
                                      ],
                                    )
                                  : null,
                              color: !isSelected ? AppColors.surface : null,
                              border: Border.all(
                                color: isSelected
                                    ? category.color
                                    : AppColors.border,
                                width: isSelected ? 2 : 1,
                              ),
                              borderRadius: BorderRadius.circular(16),
                              boxShadow: isSelected
                                  ? [
                                      BoxShadow(
                                        color: category.color
                                            .withValues(alpha: 0.3),
                                        blurRadius: 12,
                                        offset: const Offset(0, 4),
                                      ),
                                    ]
                                  : null,
                            ),
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(
                                  category.icon,
                                  size: 40,
                                  color: isSelected
                                      ? category.color
                                      : AppColors.textSecondary,
                                ),
                                const SizedBox(height: AppSpacing.sm),
                                Text(
                                  category.name,
                                  style: AppTypography.labelLarge.copyWith(
                                    color: isSelected
                                        ? category.color
                                        : AppColors.textPrimary,
                                    fontWeight: isSelected
                                        ? FontWeight.w600
                                        : FontWeight.normal,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        );
                      },
                      childCount: _categories.length,
                    ),
                  ),
                ),

                // Price Range Section
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(
                      AppSpacing.xl,
                      AppSpacing.xxl,
                      AppSpacing.xl,
                      AppSpacing.md,
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Rango de Precio',
                          style: AppTypography.h3.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        const SizedBox(height: AppSpacing.xs),
                        Text(
                          'Selecciona tu presupuesto',
                          style: AppTypography.bodyMedium.copyWith(
                            color: AppColors.textSecondary,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),

                // Price Range Options
                SliverPadding(
                  padding:
                      const EdgeInsets.symmetric(horizontal: AppSpacing.xl),
                  sliver: SliverList(
                    delegate: SliverChildBuilderDelegate(
                      (context, index) {
                        final priceRange = _priceRanges[index];
                        final isSelected = _selectedPriceRange == priceRange.id;

                        return Padding(
                          padding: const EdgeInsets.only(bottom: AppSpacing.md),
                          child: InkWell(
                            onTap: () {
                              setState(() {
                                _selectedPriceRange = priceRange.id;
                              });
                            },
                            borderRadius: BorderRadius.circular(16),
                            child: AnimatedContainer(
                              duration: const Duration(milliseconds: 200),
                              padding: const EdgeInsets.all(AppSpacing.lg),
                              decoration: BoxDecoration(
                                gradient: isSelected
                                    ? LinearGradient(
                                        colors: [
                                          AppColors.primary
                                              .withValues(alpha: 0.15),
                                          AppColors.primary
                                              .withValues(alpha: 0.05),
                                        ],
                                      )
                                    : null,
                                color: !isSelected ? AppColors.surface : null,
                                border: Border.all(
                                  color: isSelected
                                      ? AppColors.primary
                                      : AppColors.border,
                                  width: isSelected ? 2 : 1,
                                ),
                                borderRadius: BorderRadius.circular(16),
                                boxShadow: isSelected
                                    ? [
                                        BoxShadow(
                                          color: AppColors.primary
                                              .withValues(alpha: 0.2),
                                          blurRadius: 12,
                                          offset: const Offset(0, 4),
                                        ),
                                      ]
                                    : null,
                              ),
                              child: Row(
                                children: [
                                  Container(
                                    padding:
                                        const EdgeInsets.all(AppSpacing.md),
                                    decoration: BoxDecoration(
                                      color: isSelected
                                          ? AppColors.primary
                                              .withValues(alpha: 0.15)
                                          : AppColors.surfaceVariant,
                                      borderRadius: BorderRadius.circular(12),
                                    ),
                                    child: Icon(
                                      priceRange.icon,
                                      color: isSelected
                                          ? AppColors.primary
                                          : AppColors.textSecondary,
                                    ),
                                  ),
                                  const SizedBox(width: AppSpacing.md),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment:
                                          CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          priceRange.label,
                                          style:
                                              AppTypography.bodyLarge.copyWith(
                                            color: AppColors.textPrimary,
                                            fontWeight: isSelected
                                                ? FontWeight.w600
                                                : FontWeight.normal,
                                          ),
                                        ),
                                        Text(
                                          priceRange.range,
                                          style:
                                              AppTypography.bodySmall.copyWith(
                                            color: AppColors.textSecondary,
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                  if (isSelected)
                                    const Icon(
                                      Icons.check_circle,
                                      color: AppColors.primary,
                                    ),
                                ],
                              ),
                            ),
                          ),
                        );
                      },
                      childCount: _priceRanges.length,
                    ),
                  ),
                ),

                // Bottom spacing and button
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.all(AppSpacing.xl),
                    child: Column(
                      children: [
                        CustomButton(
                          text: 'Comenzar a Explorar',
                          onPressed: _selectedCategories.isNotEmpty ||
                                  _selectedPriceRange != null
                              ? _completeSetup
                              : null,
                          variant: ButtonVariant.primary,
                        ),
                        const SizedBox(height: AppSpacing.md),
                        TextButton(
                          onPressed: _completeSetup,
                          child: Text(
                            'Omitir por ahora',
                            style: AppTypography.labelLarge.copyWith(
                              color: AppColors.textSecondary,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

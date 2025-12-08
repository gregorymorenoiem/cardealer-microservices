import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../widgets/custom_button.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';
import '../auth/login_page.dart';

/// Onboarding page with 3 screens
class OnboardingPage extends StatefulWidget {
  const OnboardingPage({Key? key}) : super(key: key);

  @override
  State<OnboardingPage> createState() => _OnboardingPageState();
}

class _OnboardingPageState extends State<OnboardingPage> {
  final PageController _pageController = PageController();
  int _currentPage = 0;

  final List<OnboardingScreen> _screens = [
    const OnboardingScreen(
      title: 'Find Your Dream Car',
      description:
          'Browse thousands of verified vehicles from trusted dealers across the country',
      imagePath: 'assets/illustrations/onboarding_1.png',
      icon: Icons.directions_car,
    ),
    const OnboardingScreen(
      title: 'Connect with Dealers',
      description:
          'Chat directly with sellers, schedule test drives, and negotiate the best price',
      imagePath: 'assets/illustrations/onboarding_2.png',
      icon: Icons.chat_bubble_outline,
    ),
    const OnboardingScreen(
      title: 'Buy with Confidence',
      description:
          'Secure payments, verified listings, and complete vehicle history reports',
      imagePath: 'assets/illustrations/onboarding_3.png',
      icon: Icons.verified_user_outlined,
    ),
  ];

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  Future<void> _completeOnboarding() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool('onboarding_complete', true);

    if (mounted) {
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const LoginPage()),
      );
    }
  }

  void _nextPage() {
    if (_currentPage < _screens.length - 1) {
      _pageController.nextPage(
        duration: const Duration(milliseconds: 300),
        curve: Curves.easeInOut,
      );
    } else {
      _completeOnboarding();
    }
  }

  void _skipOnboarding() {
    _completeOnboarding();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            // Skip Button
            Align(
              alignment: Alignment.topRight,
              child: Padding(
                padding: const EdgeInsets.all(AppSpacing.md),
                child: TextButton(
                  onPressed: _skipOnboarding,
                  child: Text(
                    'Skip',
                    style: AppTypography.labelLarge.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ),
              ),
            ),

            // PageView
            Expanded(
              child: PageView.builder(
                controller: _pageController,
                onPageChanged: (index) {
                  setState(() {
                    _currentPage = index;
                  });
                },
                itemCount: _screens.length,
                itemBuilder: (context, index) => _screens[index],
              ),
            ),

            // Dots Indicator
            Padding(
              padding: const EdgeInsets.symmetric(vertical: AppSpacing.lg),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: List.generate(
                  _screens.length,
                  (index) => AnimatedContainer(
                    duration: const Duration(milliseconds: 300),
                    margin: const EdgeInsets.symmetric(horizontal: 4),
                    height: 8,
                    width: _currentPage == index ? 24 : 8,
                    decoration: BoxDecoration(
                      color: _currentPage == index
                          ? AppColors.primary
                          : AppColors.border,
                      borderRadius: BorderRadius.circular(4),
                    ),
                  ),
                ),
              ),
            ),

            // Next/Get Started Button
            Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: CustomButton(
                text: _currentPage == _screens.length - 1
                    ? 'Get Started'
                    : 'Next',
                onPressed: _nextPage,
                variant: ButtonVariant.primary,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Onboarding screen model
class OnboardingScreen extends StatelessWidget {
  final String title;
  final String description;
  final String imagePath;
  final IconData icon;

  const OnboardingScreen({
    Key? key,
    required this.title,
    required this.description,
    required this.imagePath,
    required this.icon,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(AppSpacing.xl),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          // Illustration or Icon (using icon for now since we don't have images)
          Container(
            width: 200,
            height: 200,
            decoration: BoxDecoration(
              color: AppColors.primary.withOpacity(0.1),
              shape: BoxShape.circle,
            ),
            child: Icon(
              icon,
              size: 100,
              color: AppColors.primary,
            ),
          ),
          const SizedBox(height: AppSpacing.xxl),

          // Title
          Text(
            title,
            style: AppTypography.h1.copyWith(
              color: AppColors.textPrimary,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: AppSpacing.md),

          // Description
          Text(
            description,
            style: AppTypography.bodyLarge.copyWith(
              color: AppColors.textSecondary,
            ),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }
}

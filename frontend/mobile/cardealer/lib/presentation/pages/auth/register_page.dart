import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../bloc/auth/auth_bloc.dart';
import '../../bloc/auth/auth_event.dart';
import '../../bloc/auth/auth_state.dart';
import '../../widgets/custom_button.dart';
import '../../widgets/custom_text_field.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';
import '../../../core/utils/validators.dart';
import '../../../core/responsive/responsive_utils.dart';
import '../../../core/responsive/responsive_padding.dart';
import '../../../domain/entities/user.dart';
import 'login_page.dart';
import '../home/home_page.dart';

/// Registration page with role selection - RESPONSIVE VERSION
class RegisterPage extends StatefulWidget {
  const RegisterPage({super.key});

  @override
  State<RegisterPage> createState() => _RegisterPageState();
}

class _RegisterPageState extends State<RegisterPage> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  final _dealershipNameController = TextEditingController();

  UserRole _selectedRole = UserRole.individual;
  bool _acceptTerms = false;

  @override
  void dispose() {
    _firstNameController.dispose();
    _lastNameController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    _dealershipNameController.dispose();
    super.dispose();
  }

  void _handleRegister() {
    if (!_acceptTerms) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Please accept the Terms and Conditions'),
          backgroundColor: AppColors.error,
        ),
      );
      return;
    }

    if (_formKey.currentState!.validate()) {
      context.read<AuthBloc>().add(
            AuthRegisterRequested(
              email: _emailController.text.trim(),
              password: _passwordController.text,
              firstName: _firstNameController.text.trim(),
              lastName: _lastNameController.text.trim(),
              phoneNumber: _phoneController.text.trim(),
              role: _selectedRole.toShortString(),
              dealershipName: _selectedRole == UserRole.dealer
                  ? _dealershipNameController.text.trim()
                  : null,
            ),
          );
    }
  }

  void _handleGoogleRegister() {
    context.read<AuthBloc>().add(const AuthGoogleLoginRequested());
  }

  void _handleAppleRegister() {
    context.read<AuthBloc>().add(const AuthAppleLoginRequested());
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: AppColors.textPrimary),
          onPressed: () => Navigator.of(context).pop(),
        ),
      ),
      body: BlocConsumer<AuthBloc, AuthState>(
        listener: (context, state) {
          if (state is AuthAuthenticated) {
            Navigator.of(context).pushAndRemoveUntil(
              MaterialPageRoute(builder: (_) => const HomePage()),
              (route) => false,
            );
          } else if (state is AuthError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: AppColors.error,
              ),
            );
          }
        },
        builder: (context, state) {
          final isLoading = state is AuthLoading;

          return ResponsiveLayout(
            mobile: _buildMobileLayout(isLoading),
            tablet: _buildTabletLayout(isLoading),
            desktop: _buildDesktopLayout(isLoading),
          );
        },
      ),
    );
  }

  Widget _buildMobileLayout(bool isLoading) {
    return SafeArea(
      child: SingleChildScrollView(
        padding: EdgeInsets.all(context.responsivePadding),
        child: _buildForm(isLoading),
      ),
    );
  }

  Widget _buildTabletLayout(bool isLoading) {
    return SafeArea(
      child: Center(
        child: SingleChildScrollView(
          child: ResponsiveContainer(
            maxWidth: 700,
            child: ResponsivePadding(
              multiplier: 1.5,
              child: _buildForm(isLoading),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildDesktopLayout(bool isLoading) {
    return SafeArea(
      child: Center(
        child: SingleChildScrollView(
          child: ResponsiveContainer(
            maxWidth: 800,
            child: Card(
              elevation: 4,
              margin: EdgeInsets.all(context.responsivePadding),
              child: Padding(
                padding: EdgeInsets.all(context.spacing(3)),
                child: _buildForm(isLoading),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildForm(bool isLoading) {
    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          _buildHeader(),
          SizedBox(height: context.spacing(2)),
          _buildRoleSelector(),
          SizedBox(height: context.spacing(1.5)),
          _buildNameFields(isLoading),
          SizedBox(height: context.spacing(1)),
          _buildEmailField(isLoading),
          SizedBox(height: context.spacing(1)),
          _buildPhoneField(isLoading),
          if (_selectedRole == UserRole.dealer) ...[
            SizedBox(height: context.spacing(1)),
            _buildDealershipField(isLoading),
          ],
          SizedBox(height: context.spacing(1)),
          _buildPasswordFields(isLoading),
          SizedBox(height: context.spacing(1)),
          _buildTermsCheckbox(),
          SizedBox(height: context.spacing(1.5)),
          _buildRegisterButton(isLoading),
          SizedBox(height: context.spacing(1.5)),
          _buildDivider(),
          SizedBox(height: context.spacing(1.5)),
          _buildSocialButtons(isLoading),
          SizedBox(height: context.spacing(2)),
          _buildLoginLink(),
        ],
      ),
    );
  }

  Widget _buildHeader() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Create Account',
          style: AppTypography.h1.copyWith(
            color: AppColors.textPrimary,
            fontSize: ResponsiveUtils.responsiveFontSize(context, 32),
          ),
        ),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'Sign up to get started',
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
            fontSize: ResponsiveUtils.responsiveFontSize(context, 16),
          ),
        ),
      ],
    );
  }

  Widget _buildRoleSelector() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'I am a',
          style: AppTypography.labelLarge.copyWith(
            color: AppColors.textPrimary,
          ),
        ),
        const SizedBox(height: AppSpacing.sm),
        Row(
          children: [
            Expanded(
              child: _buildRoleCard(
                role: UserRole.individual,
                icon: Icons.person_outline,
                title: 'Individual',
                subtitle: 'Buying for personal use',
              ),
            ),
            SizedBox(width: context.spacing(0.75)),
            Expanded(
              child: _buildRoleCard(
                role: UserRole.dealer,
                icon: Icons.business_outlined,
                title: 'Dealer',
                subtitle: 'Selling vehicles',
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildRoleCard({
    required UserRole role,
    required IconData icon,
    required String title,
    required String subtitle,
  }) {
    final isSelected = _selectedRole == role;
    final iconSize = context.isMobile ? 32.0 : 40.0;
    
    return GestureDetector(
      onTap: () => setState(() => _selectedRole = role),
      child: Container(
        padding: EdgeInsets.all(context.spacing(0.75)),
        decoration: BoxDecoration(
          color: isSelected
              ? AppColors.primary.withValues(alpha: 0.1)
              : AppColors.surface,
          border: Border.all(
            color: isSelected ? AppColors.primary : AppColors.border,
            width: 2,
          ),
          borderRadius: BorderRadius.circular(12),
        ),
        child: Column(
          children: [
            Icon(
              icon,
              size: iconSize,
              color: isSelected ? AppColors.primary : AppColors.textSecondary,
            ),
            const SizedBox(height: AppSpacing.sm),
            Text(
              title,
              style: AppTypography.labelLarge.copyWith(
                color: isSelected ? AppColors.primary : AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: AppSpacing.xs),
            Text(
              subtitle,
              style: AppTypography.bodySmall.copyWith(
                color: AppColors.textSecondary,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildNameFields(bool isLoading) {
    if (context.isMobile) {
      return Column(
        children: [
          CustomTextField(
            controller: _firstNameController,
            labelText: 'First Name',
            hintText: 'John',
            prefixIcon: const Icon(Icons.person_outline),
            validator: Validators.validateRequired,
            enabled: !isLoading,
            textInputAction: TextInputAction.next,
          ),
          SizedBox(height: context.spacing(1)),
          CustomTextField(
            controller: _lastNameController,
            labelText: 'Last Name',
            hintText: 'Doe',
            validator: Validators.validateRequired,
            enabled: !isLoading,
            textInputAction: TextInputAction.next,
          ),
        ],
      );
    }

    return Row(
      children: [
        Expanded(
          child: CustomTextField(
            controller: _firstNameController,
            labelText: 'First Name',
            hintText: 'John',
            prefixIcon: const Icon(Icons.person_outline),
            validator: Validators.validateRequired,
            enabled: !isLoading,
            textInputAction: TextInputAction.next,
          ),
        ),
        SizedBox(width: context.spacing(0.75)),
        Expanded(
          child: CustomTextField(
            controller: _lastNameController,
            labelText: 'Last Name',
            hintText: 'Doe',
            validator: Validators.validateRequired,
            enabled: !isLoading,
            textInputAction: TextInputAction.next,
          ),
        ),
      ],
    );
  }

  Widget _buildEmailField(bool isLoading) {
    return CustomTextField(
      controller: _emailController,
      labelText: 'Email',
      hintText: 'john.doe@example.com',
      prefixIcon: const Icon(Icons.email_outlined),
      keyboardType: TextInputType.emailAddress,
      validator: Validators.validateEmail,
      enabled: !isLoading,
      textInputAction: TextInputAction.next,
    );
  }

  Widget _buildPhoneField(bool isLoading) {
    return CustomTextField(
      controller: _phoneController,
      labelText: 'Phone Number',
      hintText: '+1 234 567 8900',
      prefixIcon: const Icon(Icons.phone_outlined),
      keyboardType: TextInputType.phone,
      validator: Validators.validatePhone,
      enabled: !isLoading,
      textInputAction: TextInputAction.next,
    );
  }

  Widget _buildDealershipField(bool isLoading) {
    return CustomTextField(
      controller: _dealershipNameController,
      labelText: 'Dealership Name',
      hintText: 'Your Dealership Name',
      prefixIcon: const Icon(Icons.business_outlined),
      validator: Validators.validateRequired,
      enabled: !isLoading,
      textInputAction: TextInputAction.next,
    );
  }

  Widget _buildPasswordFields(bool isLoading) {
    return Column(
      children: [
        CustomTextField(
          controller: _passwordController,
          labelText: 'Password',
          hintText: '••••••••',
          prefixIcon: const Icon(Icons.lock_outline),
          obscureText: true,
          validator: (value) {
            if (value == null || value.isEmpty) {
              return 'Password is required';
            }
            if (value.length < 8) {
              return 'Password must be at least 8 characters';
            }
            return null;
          },
          enabled: !isLoading,
          textInputAction: TextInputAction.next,
        ),
        SizedBox(height: context.spacing(1)),
        CustomTextField(
          controller: _confirmPasswordController,
          labelText: 'Confirm Password',
          hintText: '••••••••',
          prefixIcon: const Icon(Icons.lock_outline),
          obscureText: true,
          validator: (value) {
            if (value != _passwordController.text) {
              return 'Passwords do not match';
            }
            return null;
          },
          enabled: !isLoading,
          textInputAction: TextInputAction.done,
          onSubmitted: (_) => _handleRegister(),
        ),
      ],
    );
  }

  Widget _buildTermsCheckbox() {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          height: 24,
          width: 24,
          child: Checkbox(
            value: _acceptTerms,
            onChanged: (value) => setState(() => _acceptTerms = value ?? false),
            activeColor: AppColors.primary,
          ),
        ),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: GestureDetector(
            onTap: () => setState(() => _acceptTerms = !_acceptTerms),
            child: Text.rich(
              TextSpan(
                text: 'I agree to the ',
                style: AppTypography.bodySmall.copyWith(
                  color: AppColors.textSecondary,
                ),
                children: [
                  TextSpan(
                    text: 'Terms and Conditions',
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.primary,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  TextSpan(
                    text: ' and ',
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                  TextSpan(
                    text: 'Privacy Policy',
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.primary,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildRegisterButton(bool isLoading) {
    return CustomButton(
      text: 'Create Account',
      onPressed: isLoading ? null : _handleRegister,
      variant: ButtonVariant.primary,
      size: ButtonSize.large,
    );
  }

  Widget _buildDivider() {
    return Row(
      children: [
        const Expanded(child: Divider()),
        Padding(
          padding: EdgeInsets.symmetric(horizontal: context.spacing(0.75)),
          child: Text(
            'OR',
            style: AppTypography.bodySmall.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
        ),
        const Expanded(child: Divider()),
      ],
    );
  }

  Widget _buildSocialButtons(bool isLoading) {
    if (context.isMobile) {
      return Column(
        children: [
          CustomButton(
            text: 'Continue with Google',
            onPressed: isLoading ? null : _handleGoogleRegister,
            variant: ButtonVariant.outline,
            icon: Icons.g_mobiledata,
          ),
          SizedBox(height: context.spacing(0.75)),
          CustomButton(
            text: 'Continue with Apple',
            onPressed: isLoading ? null : _handleAppleRegister,
            variant: ButtonVariant.outline,
            icon: Icons.apple,
          ),
        ],
      );
    }

    return Row(
      children: [
        Expanded(
          child: CustomButton(
            text: 'Google',
            onPressed: isLoading ? null : _handleGoogleRegister,
            variant: ButtonVariant.outline,
            icon: Icons.g_mobiledata,
          ),
        ),
        SizedBox(width: context.spacing(0.75)),
        Expanded(
          child: CustomButton(
            text: 'Apple',
            onPressed: isLoading ? null : _handleAppleRegister,
            variant: ButtonVariant.outline,
            icon: Icons.apple,
          ),
        ),
      ],
    );
  }

  Widget _buildLoginLink() {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Text(
          'Already have an account? ',
          style: AppTypography.bodyMedium.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
        GestureDetector(
          onTap: () {
            Navigator.of(context).pushReplacement(
              MaterialPageRoute(builder: (_) => const LoginPage()),
            );
          },
          child: Text(
            'Sign In',
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.primary,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
      ],
    );
  }
}

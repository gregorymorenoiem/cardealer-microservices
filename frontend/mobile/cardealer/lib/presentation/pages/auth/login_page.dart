import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../bloc/auth/auth_bloc.dart';
import '../../bloc/auth/auth_event.dart';
import '../../bloc/auth/auth_state.dart';
import '../../widgets/custom_button.dart';
import '../../widgets/custom_text_field.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/typography.dart';
import '../../../core/utils/validators.dart';
import '../../../core/responsive/responsive_utils.dart';
import '../../../core/responsive/responsive_padding.dart';
import 'register_page.dart';
import 'forgot_password_page.dart';
import '../main/main_navigation_page.dart';

/// Login page with email/password and social login options - RESPONSIVE VERSION
class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  void _handleLogin() {
    if (_formKey.currentState!.validate()) {
      context.read<AuthBloc>().add(
            AuthLoginRequested(
              email: _emailController.text.trim(),
              password: _passwordController.text,
            ),
          );
    }
  }

  void _handleGoogleLogin() {
    context.read<AuthBloc>().add(const AuthGoogleLoginRequested());
  }

  void _handleAppleLogin() {
    context.read<AuthBloc>().add(const AuthAppleLoginRequested());
  }

  void _navigateToRegister() {
    Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => const RegisterPage()),
    );
  }

  void _navigateToForgotPassword() {
    Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => const ForgotPasswordPage()),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: BlocConsumer<AuthBloc, AuthState>(
        listener: (context, state) {
          if (state is AuthError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: AppColors.error,
              ),
            );
          } else if (state is AuthAuthenticated) {
            Navigator.pushReplacement(
              context,
              MaterialPageRoute(builder: (_) => const MainNavigationPage()),
            );
          }
        },
        builder: (context, state) {
          if (state is AuthLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          return ResponsiveLayout(
            mobile: _buildMobileLayout(),
            tablet: _buildTabletLayout(),
            desktop: _buildDesktopLayout(),
          );
        },
      ),
    );
  }

  // Mobile layout - compact, optimized for small screens
  Widget _buildMobileLayout() {
    return SafeArea(
      child: SingleChildScrollView(
        padding: EdgeInsets.all(context.responsivePadding),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              SizedBox(height: context.spacing(4)),
              _buildLogo(100),
              SizedBox(height: context.spacing(3)),
              _buildEmailField(),
              const SizedBox(height: 16),
              _buildPasswordField(),
              SizedBox(height: context.spacing(0.5)),
              _buildForgotPassword(),
              SizedBox(height: context.spacing(2)),
              _buildLoginButton(),
              SizedBox(height: context.spacing(2)),
              _buildDivider(),
              SizedBox(height: context.spacing(2)),
              _buildSocialButtons(),
              SizedBox(height: context.spacing(2)),
              _buildRegisterLink(),
            ],
          ),
        ),
      ),
    );
  }

  // Tablet layout - medium spacing
  Widget _buildTabletLayout() {
    return SafeArea(
      child: Center(
        child: SingleChildScrollView(
          child: ResponsiveContainer(
            maxWidth: 500,
            child: ResponsivePadding(
              multiplier: 1.5,
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    SizedBox(height: context.spacing(4)),
                    _buildLogo(120),
                    SizedBox(height: context.spacing(3)),
                    _buildEmailField(),
                    const SizedBox(height: 20),
                    _buildPasswordField(),
                    SizedBox(height: context.spacing(0.5)),
                    _buildForgotPassword(),
                    SizedBox(height: context.spacing(2.5)),
                    _buildLoginButton(),
                    SizedBox(height: context.spacing(2)),
                    _buildDivider(),
                    SizedBox(height: context.spacing(2)),
                    _buildSocialButtons(),
                    SizedBox(height: context.spacing(2)),
                    _buildRegisterLink(),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  // Desktop layout - card design with elevation, larger spacing
  Widget _buildDesktopLayout() {
    return SafeArea(
      child: Center(
        child: SingleChildScrollView(
          child: ResponsiveContainer(
            maxWidth: 480,
            child: Card(
              elevation: 8,
              margin: EdgeInsets.all(context.responsivePadding),
              child: Padding(
                padding: EdgeInsets.all(context.spacing(4)),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      SizedBox(height: context.spacing(1)),
                      _buildLogo(140),
                      SizedBox(height: context.spacing(3)),
                      _buildEmailField(),
                      const SizedBox(height: 24),
                      _buildPasswordField(),
                      SizedBox(height: context.spacing(0.5)),
                      _buildForgotPassword(),
                      SizedBox(height: context.spacing(2.5)),
                      _buildLoginButton(),
                      SizedBox(height: context.spacing(2)),
                      _buildDivider(),
                      SizedBox(height: context.spacing(2)),
                      _buildSocialButtons(),
                      SizedBox(height: context.spacing(2)),
                      _buildRegisterLink(),
                      SizedBox(height: context.spacing(1)),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildLogo(double size) {
    return Center(
      child: Image.asset(
        'assets/logos/logo.png',
        height: size,
        fit: BoxFit.contain,
      ),
    );
  }

  Widget _buildEmailField() {
    return CustomTextField(
      controller: _emailController,
      labelText: 'Correo electrónico',
      hintText: 'ejemplo@correo.com',
      keyboardType: TextInputType.emailAddress,
      prefixIcon: const Icon(Icons.email_outlined),
      validator: Validators.validateEmail,
      textInputAction: TextInputAction.next,
    );
  }

  Widget _buildPasswordField() {
    return CustomTextField(
      controller: _passwordController,
      labelText: 'Contraseña',
      hintText: '••••••••',
      obscureText: true,
      prefixIcon: const Icon(Icons.lock_outlined),
      validator: Validators.validateRequired,
      textInputAction: TextInputAction.done,
      onSubmitted: (_) => _handleLogin(),
    );
  }

  Widget _buildForgotPassword() {
    return Align(
      alignment: Alignment.centerRight,
      child: TextButton(
        onPressed: _navigateToForgotPassword,
        child: Text(
          '¿Olvidaste tu contraseña?',
          style: AppTypography.labelMedium.copyWith(
            color: AppColors.primary,
          ),
        ),
      ),
    );
  }

  Widget _buildLoginButton() {
    return SizedBox(
      width: double.infinity,
      child: CustomButton(
        text: 'Iniciar Sesión',
        onPressed: _handleLogin,
        variant: ButtonVariant.primary,
        size: ButtonSize.large,
      ),
    );
  }

  Widget _buildDivider() {
    return Row(
      children: [
        Expanded(child: Divider(color: Theme.of(context).dividerColor)),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 12),
          child: Text(
            'o',
            style: TextStyle(
              color: Theme.of(context).colorScheme.onSurfaceVariant,
              fontSize: 14,
            ),
          ),
        ),
        Expanded(child: Divider(color: Theme.of(context).dividerColor)),
      ],
    );
  }

  Widget _buildSocialButtons() {
    return Row(
      children: [
        Expanded(
          child: CustomButton(
            text: 'Google',
            onPressed: _handleGoogleLogin,
            variant: ButtonVariant.outline,
            size: ButtonSize.large,
            icon: Icons.g_mobiledata,
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: CustomButton(
            text: 'Apple',
            onPressed: _handleAppleLogin,
            variant: ButtonVariant.outline,
            size: ButtonSize.large,
            icon: Icons.apple,
          ),
        ),
      ],
    );
  }

  Widget _buildRegisterLink() {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Text(
          '¿No tienes cuenta? ',
          style: AppTypography.bodyMedium.copyWith(
            color: Theme.of(context).colorScheme.onSurfaceVariant,
          ),
        ),
        TextButton(
          onPressed: _navigateToRegister,
          style: TextButton.styleFrom(
            padding: EdgeInsets.zero,
            minimumSize: Size.zero,
            tapTargetSize: MaterialTapTargetSize.shrinkWrap,
          ),
          child: Text(
            'Regístrate',
            style: AppTypography.labelLarge.copyWith(
              color: AppColors.primary,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
      ],
    );
  }
}

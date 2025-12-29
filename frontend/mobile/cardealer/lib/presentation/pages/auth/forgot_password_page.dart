import 'package:flutter/material.dart';
import '../../widgets/custom_button.dart';
import '../../widgets/custom_text_field.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';
import '../../../core/utils/validators.dart';
import '../../../core/responsive/responsive_utils.dart';
import '../../../core/responsive/responsive_padding.dart';

/// Forgot Password page - request password reset - RESPONSIVE VERSION
class ForgotPasswordPage extends StatefulWidget {
  const ForgotPasswordPage({super.key});

  @override
  State<ForgotPasswordPage> createState() => _ForgotPasswordPageState();
}

class _ForgotPasswordPageState extends State<ForgotPasswordPage> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  bool _isLoading = false;
  bool _emailSent = false;

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

  Future<void> _handleRequestReset() async {
    if (_formKey.currentState!.validate()) {
      setState(() => _isLoading = true);

      // Simulate API call
      await Future.delayed(const Duration(seconds: 2));

      setState(() {
        _isLoading = false;
        _emailSent = true;
      });

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Password reset email sent! Check your inbox.'),
            backgroundColor: AppColors.success,
          ),
        );
      }
    }
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
      body: ResponsiveLayout(
        mobile: _buildMobileLayout(),
        tablet: _buildTabletLayout(),
        desktop: _buildDesktopLayout(),
      ),
    );
  }

  Widget _buildMobileLayout() {
    return SafeArea(
      child: SingleChildScrollView(
        padding: EdgeInsets.all(context.responsivePadding),
        child: _buildForm(),
      ),
    );
  }

  Widget _buildTabletLayout() {
    return SafeArea(
      child: Center(
        child: SingleChildScrollView(
          child: ResponsiveContainer(
            maxWidth: 500,
            child: ResponsivePadding(
              multiplier: 1.5,
              child: _buildForm(),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildDesktopLayout() {
    return SafeArea(
      child: Center(
        child: SingleChildScrollView(
          child: ResponsiveContainer(
            maxWidth: 450,
            child: Card(
              elevation: 4,
              margin: EdgeInsets.all(context.responsivePadding),
              child: Padding(
                padding: EdgeInsets.all(context.spacing(3)),
                child: _buildForm(),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildForm() {
    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          _buildHeader(),
          SizedBox(height: context.spacing(2)),
          if (!_emailSent) ...[
            _buildEmailField(),
            SizedBox(height: context.spacing(2)),
            _buildResetButton(),
          ] else ...[
            _buildSuccessIcon(),
            SizedBox(height: context.spacing(2)),
            _buildBackButton(),
            SizedBox(height: context.spacing(1)),
            _buildResendButton(),
          ],
        ],
      ),
    );
  }

  Widget _buildHeader() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Forgot Password?',
          style: AppTypography.h1.copyWith(
            color: AppColors.textPrimary,
            fontSize: ResponsiveUtils.responsiveFontSize(context, 32),
          ),
        ),
        const SizedBox(height: AppSpacing.xs),
        Text(
          _emailSent
              ? 'We\'ve sent you an email with instructions to reset your password.'
              : 'Enter your email address and we\'ll send you a link to reset your password.',
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
            fontSize: ResponsiveUtils.responsiveFontSize(context, 16),
          ),
        ),
      ],
    );
  }

  Widget _buildEmailField() {
    return CustomTextField(
      controller: _emailController,
      labelText: 'Email',
      hintText: 'john.doe@example.com',
      prefixIcon: const Icon(Icons.email_outlined),
      keyboardType: TextInputType.emailAddress,
      validator: Validators.validateEmail,
      enabled: !_isLoading,
      textInputAction: TextInputAction.done,
      onSubmitted: (_) => _handleRequestReset(),
    );
  }

  Widget _buildResetButton() {
    return CustomButton(
      text: 'Send Reset Link',
      onPressed: _isLoading ? null : _handleRequestReset,
      variant: ButtonVariant.primary,
      size: ButtonSize.large,
    );
  }

  Widget _buildSuccessIcon() {
    final iconSize = context.isMobile ? 80.0 : 100.0;
    return Center(
      child: Icon(
        Icons.mark_email_read_outlined,
        size: iconSize,
        color: AppColors.success,
      ),
    );
  }

  Widget _buildBackButton() {
    return CustomButton(
      text: 'Back to Login',
      onPressed: () => Navigator.of(context).pop(),
      variant: ButtonVariant.primary,
      size: ButtonSize.large,
    );
  }

  Widget _buildResendButton() {
    return CustomButton(
      text: 'Resend Email',
      onPressed: () {
        setState(() => _emailSent = false);
        _handleRequestReset();
      },
      variant: ButtonVariant.text,
    );
  }
}

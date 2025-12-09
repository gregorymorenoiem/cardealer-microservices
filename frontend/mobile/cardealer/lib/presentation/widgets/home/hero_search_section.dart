import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// Hero search section with prominent search bar and quick suggestions
class HeroSearchSection extends StatefulWidget {
  final VoidCallback? onSearchTap;
  final VoidCallback? onVoiceSearchTap;
  final Function(String)? onSearchSubmitted;
  final List<String> quickSuggestions;

  const HeroSearchSection({
    super.key,
    this.onSearchTap,
    this.onVoiceSearchTap,
    this.onSearchSubmitted,
    this.quickSuggestions = const [],
  });

  @override
  State<HeroSearchSection> createState() => _HeroSearchSectionState();
}

class _HeroSearchSectionState extends State<HeroSearchSection>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _fadeAnimation;
  final FocusNode _focusNode = FocusNode();
  bool _isFocused = false;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 300),
      vsync: this,
    );

    _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOut),
    );

    _focusNode.addListener(() {
      setState(() {
        _isFocused = _focusNode.hasFocus;
        if (_isFocused) {
          _controller.forward();
        } else {
          _controller.reverse();
        }
      });
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    _focusNode.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [
            AppColors.primary.withValues(alpha: 0.05),
            Colors.transparent,
          ],
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Headline
          Text(
            'Encuentra tu auto perfecto',
            style: AppTypography.h2.copyWith(
              color: AppColors.textPrimary,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: AppSpacing.xs),
          Text(
            'Miles de vehículos esperándote',
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
          const SizedBox(height: AppSpacing.md),

          // Search bar
          GestureDetector(
            onTap: widget.onSearchTap,
            child: Container(
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                boxShadow: [
                  BoxShadow(
                    color: _isFocused
                        ? AppColors.primary.withValues(alpha: 0.2)
                        : Colors.black.withValues(alpha: 0.08),
                    blurRadius: _isFocused ? 12 : 8,
                    offset: const Offset(0, 4),
                  ),
                ],
                border: Border.all(
                  color: _isFocused ? AppColors.primary : Colors.transparent,
                  width: 2,
                ),
              ),
              child: Row(
                children: [
                  Padding(
                    padding: const EdgeInsets.all(AppSpacing.md),
                    child: Icon(
                      Icons.search,
                      color: _isFocused
                          ? AppColors.primary
                          : AppColors.textSecondary,
                      size: 24,
                    ),
                  ),
                  Expanded(
                    child: TextField(
                      focusNode: _focusNode,
                      decoration: InputDecoration(
                        hintText: 'Buscar por marca, modelo, año...',
                        hintStyle: AppTypography.bodyMedium.copyWith(
                          color: AppColors.textSecondary,
                        ),
                        border: InputBorder.none,
                      ),
                      style: AppTypography.bodyMedium,
                      onSubmitted: widget.onSearchSubmitted,
                    ),
                  ),
                  // Voice search button
                  IconButton(
                    icon: const Icon(
                      Icons.mic,
                      color: AppColors.accent,
                    ),
                    tooltip: 'Búsqueda por voz',
                    onPressed: widget.onVoiceSearchTap,
                  ),
                  const SizedBox(width: AppSpacing.xs),
                ],
              ),
            ),
          ),

          // Quick suggestions
          if (widget.quickSuggestions.isNotEmpty) ...[
            const SizedBox(height: AppSpacing.md),
            FadeTransition(
              opacity: _fadeAnimation,
              child: Wrap(
                spacing: AppSpacing.sm,
                runSpacing: AppSpacing.sm,
                children: widget.quickSuggestions.map((suggestion) {
                  return _QuickSuggestionChip(
                    label: suggestion,
                    onTap: () {
                      widget.onSearchSubmitted?.call(suggestion);
                    },
                  );
                }).toList(),
              ),
            ),
          ],
        ],
      ),
    );
  }
}

/// Quick suggestion chip
class _QuickSuggestionChip extends StatelessWidget {
  final String label;
  final VoidCallback? onTap;

  const _QuickSuggestionChip({
    required this.label,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(20),
      child: Container(
        padding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.sm,
        ),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: AppColors.border,
            width: 1,
          ),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(
              Icons.search,
              size: 16,
              color: AppColors.textSecondary,
            ),
            const SizedBox(width: AppSpacing.xs),
            Text(
              label,
              style: AppTypography.bodySmall.copyWith(
                color: AppColors.textPrimary,
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

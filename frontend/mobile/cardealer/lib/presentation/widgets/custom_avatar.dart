import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../core/theme/colors.dart';

enum AvatarSize {
  xs, // 24px
  sm, // 32px
  md, // 40px
  lg, // 56px
  xl, // 72px
  xxl, // 96px
}

class CustomAvatar extends StatelessWidget {
  final String? imageUrl;
  final String? name;
  final AvatarSize size;
  final Color? backgroundColor;
  final Color? textColor;
  final VoidCallback? onTap;
  final bool showBadge;
  final Color? badgeColor;
  final Widget? badge;

  const CustomAvatar({
    Key? key,
    this.imageUrl,
    this.name,
    this.size = AvatarSize.md,
    this.backgroundColor,
    this.textColor,
    this.onTap,
    this.showBadge = false,
    this.badgeColor,
    this.badge,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final avatarSize = _getSize();
    final bgColor = backgroundColor ?? _getDefaultBackgroundColor();
    final txColor = textColor ?? Colors.white;

    Widget avatarContent;

    if (imageUrl != null && imageUrl!.isNotEmpty) {
      // Image avatar
      avatarContent = CachedNetworkImage(
        imageUrl: imageUrl!,
        imageBuilder: (context, imageProvider) => Container(
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            image: DecorationImage(
              image: imageProvider,
              fit: BoxFit.cover,
            ),
          ),
        ),
        placeholder: (context, url) => _buildPlaceholder(bgColor, txColor),
        errorWidget: (context, url, error) => _buildInitials(bgColor, txColor),
      );
    } else {
      // Initials avatar
      avatarContent = _buildInitials(bgColor, txColor);
    }

    Widget avatar = GestureDetector(
      onTap: onTap,
      child: Container(
        width: avatarSize,
        height: avatarSize,
        decoration: BoxDecoration(
          shape: BoxShape.circle,
          border: Border.all(
            color: Colors.white,
            width: size == AvatarSize.xxl ? 3 : 2,
          ),
        ),
        child: ClipOval(child: avatarContent),
      ),
    );

    // Add badge if needed
    if (showBadge || badge != null) {
      return Stack(
        clipBehavior: Clip.none,
        children: [
          avatar,
          Positioned(
            right: 0,
            bottom: 0,
            child: badge ??
                Container(
                  width: _getBadgeSize(),
                  height: _getBadgeSize(),
                  decoration: BoxDecoration(
                    color: badgeColor ?? AppColors.success,
                    shape: BoxShape.circle,
                    border: Border.all(
                      color: Colors.white,
                      width: 2,
                    ),
                  ),
                ),
          ),
        ],
      );
    }

    return avatar;
  }

  double _getSize() {
    switch (size) {
      case AvatarSize.xs:
        return 24;
      case AvatarSize.sm:
        return 32;
      case AvatarSize.md:
        return 40;
      case AvatarSize.lg:
        return 56;
      case AvatarSize.xl:
        return 72;
      case AvatarSize.xxl:
        return 96;
    }
  }

  double _getBadgeSize() {
    switch (size) {
      case AvatarSize.xs:
      case AvatarSize.sm:
        return 8;
      case AvatarSize.md:
        return 10;
      case AvatarSize.lg:
      case AvatarSize.xl:
        return 12;
      case AvatarSize.xxl:
        return 16;
    }
  }

  double _getFontSize() {
    switch (size) {
      case AvatarSize.xs:
        return 10;
      case AvatarSize.sm:
        return 12;
      case AvatarSize.md:
        return 16;
      case AvatarSize.lg:
        return 20;
      case AvatarSize.xl:
        return 28;
      case AvatarSize.xxl:
        return 36;
    }
  }

  Widget _buildPlaceholder(Color bgColor, Color txColor) {
    return Container(
      color: bgColor,
      child: Center(
        child: CircularProgressIndicator(
          strokeWidth: 2,
          valueColor: AlwaysStoppedAnimation<Color>(txColor),
        ),
      ),
    );
  }

  Widget _buildInitials(Color bgColor, Color txColor) {
    final initials = _getInitials();
    return Container(
      color: bgColor,
      child: Center(
        child: Text(
          initials,
          style: TextStyle(
            color: txColor,
            fontSize: _getFontSize(),
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
    );
  }

  String _getInitials() {
    if (name == null || name!.isEmpty) return '?';

    final parts = name!.trim().split(' ');
    if (parts.length >= 2) {
      return '${parts[0][0]}${parts[1][0]}'.toUpperCase();
    } else if (parts.isNotEmpty && parts[0].isNotEmpty) {
      return parts[0][0].toUpperCase();
    }
    return '?';
  }

  Color _getDefaultBackgroundColor() {
    if (name == null || name!.isEmpty) return AppColors.textDisabled;

    // Generate consistent color based on name
    final hash = name!.hashCode;
    final colors = [
      AppColors.primary,
      AppColors.secondary,
      AppColors.accent,
      AppColors.info,
      const Color(0xFF8B5CF6), // purple
      const Color(0xFFEC4899), // pink
      const Color(0xFF14B8A6), // teal
    ];

    return colors[hash.abs() % colors.length];
  }
}

/// Avatar group for displaying multiple avatars
class AvatarGroup extends StatelessWidget {
  final List<String?> imageUrls;
  final List<String?> names;
  final AvatarSize size;
  final int maxVisible;
  final VoidCallback? onShowAll;

  const AvatarGroup({
    Key? key,
    required this.imageUrls,
    required this.names,
    this.size = AvatarSize.sm,
    this.maxVisible = 3,
    this.onShowAll,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final visibleCount =
        imageUrls.length > maxVisible ? maxVisible : imageUrls.length;
    final remainingCount = imageUrls.length - visibleCount;
    final avatarSize = _getAvatarSize();
    final overlap = avatarSize * 0.3;

    return SizedBox(
      height: avatarSize,
      child: Stack(
        children: [
          // Avatars
          ...List.generate(visibleCount, (index) {
            return Positioned(
              left: index * (avatarSize - overlap),
              child: CustomAvatar(
                imageUrl: imageUrls[index],
                name: names.length > index ? names[index] : null,
                size: size,
              ),
            );
          }),
          // Remaining count
          if (remainingCount > 0)
            Positioned(
              left: visibleCount * (avatarSize - overlap),
              child: GestureDetector(
                onTap: onShowAll,
                child: Container(
                  width: avatarSize,
                  height: avatarSize,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: AppColors.textDisabled,
                    border: Border.all(
                      color: Colors.white,
                      width: 2,
                    ),
                  ),
                  child: Center(
                    child: Text(
                      '+$remainingCount',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: _getFontSize(),
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  double _getAvatarSize() {
    switch (size) {
      case AvatarSize.xs:
        return 24;
      case AvatarSize.sm:
        return 32;
      case AvatarSize.md:
        return 40;
      case AvatarSize.lg:
        return 56;
      case AvatarSize.xl:
        return 72;
      case AvatarSize.xxl:
        return 96;
    }
  }

  double _getFontSize() {
    switch (size) {
      case AvatarSize.xs:
        return 10;
      case AvatarSize.sm:
        return 12;
      case AvatarSize.md:
        return 14;
      case AvatarSize.lg:
        return 16;
      case AvatarSize.xl:
        return 20;
      case AvatarSize.xxl:
        return 24;
    }
  }
}

import 'package:flutter/material.dart';

/// App color palette based on Tailwind CSS
/// Matching cardealer web theme
class AppColors {
  // Private constructor to prevent instantiation
  AppColors._();

  // Primary - Blue theme (cars marketplace)
  static const primary = Color(0xFF2563EB); // blue-600
  static const primaryDark = Color(0xFF1E40AF); // blue-700
  static const primaryLight = Color(0xFF3B82F6); // blue-500
  static const primary50 = Color(0xFFEFF6FF); // blue-50
  static const primary100 = Color(0xFFDBEAFE); // blue-100

  // Secondary - Emerald (success states)
  static const secondary = Color(0xFF10B981); // emerald-500
  static const secondaryDark = Color(0xFF059669); // emerald-600
  static const secondaryLight = Color(0xFF34D399); // emerald-400

  // Accent - Amber (featured, highlights)
  static const accent = Color(0xFFF59E0B); // amber-500
  static const accentDark = Color(0xFFD97706); // amber-600
  static const accentLight = Color(0xFFFBBF24); // amber-400

  // Semantic colors
  static const error = Color(0xFFEF4444); // red-500
  static const errorDark = Color(0xFFDC2626); // red-600
  static const errorLight = Color(0xFFF87171); // red-400

  static const success = Color(0xFF22C55E); // green-500
  static const successDark = Color(0xFF16A34A); // green-600
  static const successLight = Color(0xFF4ADE80); // green-400

  static const warning = Color(0xFFF59E0B); // amber-500
  static const warningDark = Color(0xFFD97706); // amber-600
  static const warningLight = Color(0xFFFBBF24); // amber-400

  static const info = Color(0xFF3B82F6); // blue-500
  static const infoDark = Color(0xFF2563EB); // blue-600
  static const infoLight = Color(0xFF60A5FA); // blue-400

  // Neutrals
  static const background = Color(0xFFF9FAFB); // gray-50
  static const surface = Color(0xFFFFFFFF); // white
  static const surfaceVariant = Color(0xFFF3F4F6); // gray-100
  static const surfaceDark = Color(0xFF1F2937); // gray-800

  // Text colors
  static const textPrimary = Color(0xFF111827); // gray-900
  static const textSecondary = Color(0xFF6B7280); // gray-500
  static const textTertiary = Color(0xFF9CA3AF); // gray-400
  static const textDisabled = Color(0xFFD1D5DB); // gray-300
  static const textOnPrimary = Color(0xFFFFFFFF); // white
  static const textOnDark = Color(0xFFFFFFFF); // white

  // Borders
  static const border = Color(0xFFE5E7EB); // gray-200
  static const borderDark = Color(0xFFD1D5DB); // gray-300
  static const divider = Color(0xFFF3F4F6); // gray-100

  // Dealer plan badge colors
  static const planFree = Color(0xFFD1D5DB); // gray-300
  static const planBasic = Color(0xFF34D399); // emerald-400
  static const planPro = Color(0xFF3B82F6); // blue-500
  static const planEnterprise = Color(0xFF9333EA); // purple-600

  // Shadow colors
  static const shadowLight = Color(0x0F000000); // 6% black
  static const shadowMedium = Color(0x1A000000); // 10% black
  static const shadowDark = Color(0x29000000); // 16% black

  // Overlay colors
  static const overlay = Color(0x80000000); // 50% black
  static const overlayLight = Color(0x40000000); // 25% black

  // Featured badge gradient
  static const featuredGradientStart = Color(0xFFF59E0B); // amber-500
  static const featuredGradientEnd = Color(0xFFD97706); // amber-600

  // Dark theme colors
  static const backgroundDark = Color(0xFF111827); // gray-900
  static const surfaceDark1 = Color(0xFF1F2937); // gray-800
  static const surfaceDark2 = Color(0xFF374151); // gray-700
}

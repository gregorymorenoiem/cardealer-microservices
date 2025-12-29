import 'package:flutter/material.dart';

/// App typography system - Premium redesign
/// Using Poppins for headlines and Inter for body text
class AppTypography {
  // Private constructor
  AppTypography._();

  // Font families
  static const String headlineFamily = 'Poppins'; // For headlines
  static const String bodyFamily = 'Inter'; // For body text

  // Headings (using Poppins for impact)
  static const h1 = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 32,
    fontWeight: FontWeight.w700, // Bold
    height: 1.2,
    letterSpacing: -0.5,
  );

  static const h2 = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 24,
    fontWeight: FontWeight.w600, // SemiBold
    height: 1.3,
    letterSpacing: -0.3,
  );

  static const h3 = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 20,
    fontWeight: FontWeight.w600, // SemiBold
    height: 1.4,
  );

  static const h4 = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 18,
    fontWeight: FontWeight.w600, // SemiBold
    height: 1.4,
  );

  static const h5 = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 16,
    fontWeight: FontWeight.w600, // SemiBold
    height: 1.5,
  );

  static const h6 = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 14,
    fontWeight: FontWeight.w600, // SemiBold
    height: 1.5,
  );

  // Body text (using Inter for readability)
  static const bodyLarge = TextStyle(
    fontFamily: bodyFamily,
    fontSize: 16,
    fontWeight: FontWeight.w400, // Regular
    height: 1.5,
  );

  static const bodyMedium = TextStyle(
    fontFamily: bodyFamily,
    fontSize: 14,
    fontWeight: FontWeight.w400, // Regular
    height: 1.5,
  );

  static const bodySmall = TextStyle(
    fontFamily: bodyFamily,
    fontSize: 12,
    fontWeight: FontWeight.w400, // Regular
    height: 1.5,
  );

  // Labels (using Inter for consistency)
  static const labelLarge = TextStyle(
    fontFamily: bodyFamily,
    fontSize: 14,
    fontWeight: FontWeight.w500, // Medium
    height: 1.4,
  );

  static const labelMedium = TextStyle(
    fontFamily: bodyFamily,
    fontSize: 12,
    fontWeight: FontWeight.w500, // Medium
    height: 1.4,
  );

  static const labelSmall = TextStyle(
    fontFamily: bodyFamily,
    fontSize: 10,
    fontWeight: FontWeight.w500, // Medium
    height: 1.4,
  );

  // Caption
  static const caption = TextStyle(
    fontFamily: bodyFamily,
    fontSize: 12,
    fontWeight: FontWeight.w400, // Regular
    height: 1.4,
    letterSpacing: 0.4,
  );

  // Overline (for small uppercase labels)
  static const overline = TextStyle(
    fontFamily: bodyFamily,
    fontSize: 10,
    fontWeight: FontWeight.w600, // SemiBold
    height: 1.5,
    letterSpacing: 1.5,
  );

  // Button text (using Poppins for CTAs)
  static const button = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 14,
    fontWeight: FontWeight.w600, // SemiBold
    height: 1.2,
    letterSpacing: 0.4,
  );

  static const buttonLarge = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 16,
    fontWeight: FontWeight.w600, // SemiBold
    height: 1.2,
    letterSpacing: 0.4,
  );

  static const buttonSmall = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 12,
    fontWeight: FontWeight.w600, // SemiBold
    height: 1.2,
    letterSpacing: 0.4,
  );

  // Special styles
  static const price = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 24,
    fontWeight: FontWeight.w700, // Bold
    height: 1.2,
  );

  static const priceLarge = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 32,
    fontWeight: FontWeight.w700, // Bold
    height: 1.2,
  );

  static const priceSmall = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 18,
    fontWeight: FontWeight.w700, // Bold
    height: 1.2,
  );

  // Card title
  static const cardTitle = TextStyle(
    fontFamily: headlineFamily,
    fontSize: 16,
    fontWeight: FontWeight.w600, // SemiBold
    height: 1.3,
  );

  static const cardSubtitle = TextStyle(
    fontFamily: bodyFamily,
    fontSize: 14,
    fontWeight: FontWeight.w400, // Regular
    height: 1.4,
  );
}

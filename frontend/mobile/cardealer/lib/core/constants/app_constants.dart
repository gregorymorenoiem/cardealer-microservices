import 'package:flutter/material.dart';

/// App-wide constants
class AppConstants {
  // Private constructor
  AppConstants._();

  // App info
  static const String appName = 'CarDealer';
  static const String appVersion = '1.0.0';
  static const String appBuildNumber = '1';

  // Supported languages
  static const String languageEnglish = 'en';
  static const String languageSpanish = 'es';
  static const List<String> supportedLanguages = [
    languageEnglish,
    languageSpanish,
  ];

  // Default values
  static const String defaultLanguage = languageSpanish;
  static const int defaultPageSize = 20;
  static const int maxImageUploadSize = 10 * 1024 * 1024; // 10MB

  // Vehicle categories
  static const String categorySedans = 'sedans';
  static const String categorySuvs = 'suvs';
  static const String categoryTrucks = 'trucks';
  static const String categoryCoupes = 'coupes';
  static const String categoryConvertibles = 'convertibles';
  static const String categoryElectric = 'electric';
  static const String categoryHybrid = 'hybrid';
  static const String categoryLuxury = 'luxury';

  // Account types
  static const String accountTypeIndividual = 'INDIVIDUAL';
  static const String accountTypeDealer = 'DEALER';
  static const String accountTypeDealerEmployee = 'DEALER_EMPLOYEE';
  static const String accountTypeAdmin = 'ADMIN';
  static const String accountTypePlatformEmployee = 'PLATFORM_EMPLOYEE';

  // Dealer plans
  static const String planFree = 'FREE';
  static const String planBasic = 'BASIC';
  static const String planPro = 'PRO';
  static const String planEnterprise = 'ENTERPRISE';

  // Plan limits
  static const Map<String, Map<String, int>> planLimits = {
    planFree: {'listings': 5, 'featured': 1},
    planBasic: {'listings': 20, 'featured': 3},
    planPro: {'listings': 200, 'featured': 10},
    planEnterprise: {'listings': -1, 'featured': -1}, // -1 = unlimited
  };

  // Featured ratio (40% from ranking algorithm)
  static const double featuredRatio = 0.4;

  // Cache durations
  static const Duration cacheShortDuration = Duration(minutes: 5);
  static const Duration cacheMediumDuration = Duration(minutes: 30);
  static const Duration cacheLongDuration = Duration(hours: 24);

  // Animation durations
  static const Duration animationFast = Duration(milliseconds: 150);
  static const Duration animationNormal = Duration(milliseconds: 300);
  static const Duration animationSlow = Duration(milliseconds: 500);

  // Debounce durations
  static const Duration debounceShort = Duration(milliseconds: 300);
  static const Duration debounceMedium = Duration(milliseconds: 500);
  static const Duration debounceLong = Duration(milliseconds: 1000);

  // Image quality
  static const int imageQualityLow = 50;
  static const int imageQualityMedium = 75;
  static const int imageQualityHigh = 90;

  // Carousel
  static const Duration carouselAutoPlayDuration = Duration(seconds: 5);
  static const Curve carouselAnimationCurve = Curves.easeInOut;

  // HomePage sections
  static const int heroCarouselCount = 5;
  static const int featuredGridCount = 6;
  static const int featuredSectionCount = 10;
  static const int totalHomePageVehicles = 71;

  // Validation
  static const int minPasswordLength = 8;
  static const int maxPasswordLength = 128;
  static const int minNameLength = 2;
  static const int maxNameLength = 100;
  static const int maxDescriptionLength = 2000;

  // Contact
  static const String supportEmail = 'support@cardealer.com';
  static const String supportPhone = '+1-555-0123';

  // Social media
  static const String facebookUrl = 'https://facebook.com/cardealer';
  static const String twitterUrl = 'https://twitter.com/cardealer';
  static const String instagramUrl = 'https://instagram.com/cardealer';

  // Legal
  static const String termsUrl = 'https://cardealer.com/terms';
  static const String privacyUrl = 'https://cardealer.com/privacy';
}

import 'package:intl/intl.dart';

/// Formatters utilities
class Formatters {
  // Private constructor
  Formatters._();

  /// Format price with currency symbol
  static String formatPrice(
    double price, {
    String symbol = '\$',
    String locale = 'es_MX',
  }) {
    final formatter = NumberFormat.currency(
      locale: locale,
      symbol: symbol,
      decimalDigits: 0,
    );
    return formatter.format(price);
  }

  /// Format number with thousands separator
  static String formatNumber(int number, {String locale = 'es_MX'}) {
    final formatter = NumberFormat.decimalPattern(locale);
    return formatter.format(number);
  }

  /// Format mileage
  static String formatMileage(int mileage, {String locale = 'es_MX'}) {
    final formatter = NumberFormat.decimalPattern(locale);
    return '${formatter.format(mileage)} km';
  }

  /// Format date
  static String formatDate(DateTime date, {String format = 'dd/MM/yyyy'}) {
    final formatter = DateFormat(format);
    return formatter.format(date);
  }

  /// Format date time
  static String formatDateTime(
    DateTime dateTime, {
    String format = 'dd/MM/yyyy HH:mm',
  }) {
    final formatter = DateFormat(format);
    return formatter.format(dateTime);
  }

  /// Format relative time (e.g., "hace 2 horas")
  static String formatRelativeTime(DateTime dateTime) {
    final now = DateTime.now();
    final difference = now.difference(dateTime);

    if (difference.inDays > 365) {
      final years = (difference.inDays / 365).floor();
      return 'hace ${years} año${years > 1 ? 's' : ''}';
    } else if (difference.inDays > 30) {
      final months = (difference.inDays / 30).floor();
      return 'hace ${months} mes${months > 1 ? 'es' : ''}';
    } else if (difference.inDays > 0) {
      return 'hace ${difference.inDays} día${difference.inDays > 1 ? 's' : ''}';
    } else if (difference.inHours > 0) {
      return 'hace ${difference.inHours} hora${difference.inHours > 1 ? 's' : ''}';
    } else if (difference.inMinutes > 0) {
      return 'hace ${difference.inMinutes} minuto${difference.inMinutes > 1 ? 's' : ''}';
    } else {
      return 'hace unos segundos';
    }
  }
}

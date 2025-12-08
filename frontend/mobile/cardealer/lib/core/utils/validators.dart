/// Validators utilities
class Validators {
  // Private constructor
  Validators._();

  /// Validate email
  static String? validateEmail(String? value) {
    if (value == null || value.isEmpty) {
      return 'El correo electrónico es requerido';
    }

    final emailRegex = RegExp(
      r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$',
    );

    if (!emailRegex.hasMatch(value)) {
      return 'Ingresa un correo electrónico válido';
    }

    return null;
  }

  /// Validate password
  static String? validatePassword(String? value, {int minLength = 8}) {
    if (value == null || value.isEmpty) {
      return 'La contraseña es requerida';
    }

    if (value.length < minLength) {
      return 'La contraseña debe tener al menos $minLength caracteres';
    }

    // Check for at least one uppercase letter
    if (!value.contains(RegExp(r'[A-Z]'))) {
      return 'La contraseña debe contener al menos una letra mayúscula';
    }

    // Check for at least one lowercase letter
    if (!value.contains(RegExp(r'[a-z]'))) {
      return 'La contraseña debe contener al menos una letra minúscula';
    }

    // Check for at least one number
    if (!value.contains(RegExp(r'[0-9]'))) {
      return 'La contraseña debe contener al menos un número';
    }

    return null;
  }

  /// Validate required field
  static String? validateRequired(
    String? value, {
    String fieldName = 'Este campo',
  }) {
    if (value == null || value.isEmpty) {
      return '$fieldName es requerido';
    }
    return null;
  }

  /// Validate phone number
  static String? validatePhone(String? value) {
    if (value == null || value.isEmpty) {
      return 'El teléfono es requerido';
    }

    final phoneRegex = RegExp(r'^\+?[0-9]{10,15}$');

    if (!phoneRegex.hasMatch(value.replaceAll(RegExp(r'[\s-]'), ''))) {
      return 'Ingresa un número de teléfono válido';
    }

    return null;
  }

  /// Validate minimum length
  static String? validateMinLength(
    String? value,
    int minLength, {
    String fieldName = 'Este campo',
  }) {
    if (value == null || value.isEmpty) {
      return '$fieldName es requerido';
    }

    if (value.length < minLength) {
      return '$fieldName debe tener al menos $minLength caracteres';
    }

    return null;
  }

  /// Validate maximum length
  static String? validateMaxLength(
    String? value,
    int maxLength, {
    String fieldName = 'Este campo',
  }) {
    if (value != null && value.length > maxLength) {
      return '$fieldName no puede tener más de $maxLength caracteres';
    }

    return null;
  }

  /// Validate number
  static String? validateNumber(
    String? value, {
    String fieldName = 'Este campo',
  }) {
    if (value == null || value.isEmpty) {
      return '$fieldName es requerido';
    }

    if (double.tryParse(value) == null) {
      return '$fieldName debe ser un número válido';
    }

    return null;
  }

  /// Validate number range
  static String? validateNumberRange(
    String? value,
    double min,
    double max, {
    String fieldName = 'Este campo',
  }) {
    if (value == null || value.isEmpty) {
      return '$fieldName es requerido';
    }

    final number = double.tryParse(value);
    if (number == null) {
      return '$fieldName debe ser un número válido';
    }

    if (number < min || number > max) {
      return '$fieldName debe estar entre $min y $max';
    }

    return null;
  }
}

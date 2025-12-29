import 'package:equatable/equatable.dart';

/// User entity representing an authenticated user
class User extends Equatable {
  final String id;
  final String email;
  final String firstName;
  final String lastName;
  final String? phoneNumber;
  final UserRole role;
  final String? avatarUrl;
  final bool isVerified;
  final String? dealershipName; // For dealers
  final DateTime createdAt;

  const User({
    required this.id,
    required this.email,
    required this.firstName,
    required this.lastName,
    this.phoneNumber,
    required this.role,
    this.avatarUrl,
    required this.isVerified,
    this.dealershipName,
    required this.createdAt,
  });

  /// Get user's full name
  String get fullName => '$firstName $lastName';

  /// Get user's initials for avatar
  String get initials {
    final first = firstName.isNotEmpty ? firstName[0] : '';
    final last = lastName.isNotEmpty ? lastName[0] : '';
    return '$first$last'.toUpperCase();
  }

  /// Check if user is a dealer
  bool get isDealer => role == UserRole.dealer;

  /// Check if user is an admin
  bool get isAdmin => role == UserRole.admin;

  /// Check if user is an individual buyer
  bool get isIndividual => role == UserRole.individual;

  /// Copy with method for immutability
  User copyWith({
    String? id,
    String? email,
    String? firstName,
    String? lastName,
    String? phoneNumber,
    UserRole? role,
    String? avatarUrl,
    bool? isVerified,
    String? dealershipName,
    DateTime? createdAt,
  }) {
    return User(
      id: id ?? this.id,
      email: email ?? this.email,
      firstName: firstName ?? this.firstName,
      lastName: lastName ?? this.lastName,
      phoneNumber: phoneNumber ?? this.phoneNumber,
      role: role ?? this.role,
      avatarUrl: avatarUrl ?? this.avatarUrl,
      isVerified: isVerified ?? this.isVerified,
      dealershipName: dealershipName ?? this.dealershipName,
      createdAt: createdAt ?? this.createdAt,
    );
  }

  @override
  List<Object?> get props => [
        id,
        email,
        firstName,
        lastName,
        phoneNumber,
        role,
        avatarUrl,
        isVerified,
        dealershipName,
        createdAt,
      ];
}

/// User roles in the system
enum UserRole {
  individual,
  dealer,
  admin,
}

/// Extension to convert string to UserRole
extension UserRoleExtension on String {
  UserRole toUserRole() {
    switch (toLowerCase()) {
      case 'individual':
        return UserRole.individual;
      case 'dealer':
        return UserRole.dealer;
      case 'admin':
        return UserRole.admin;
      default:
        return UserRole.individual;
    }
  }
}

/// Extension to convert UserRole to string
extension UserRoleStringExtension on UserRole {
  String toShortString() {
    return toString().split('.').last;
  }

  String toDisplayString() {
    switch (this) {
      case UserRole.individual:
        return 'Individual Buyer';
      case UserRole.dealer:
        return 'Dealer';
      case UserRole.admin:
        return 'Administrator';
    }
  }
}

import '../../domain/entities/user.dart';

/// User model for data layer
/// Handles JSON serialization/deserialization
class UserModel extends User {
  const UserModel({
    required super.id,
    required super.email,
    required super.firstName,
    required super.lastName,
    super.phoneNumber,
    required super.role,
    super.avatarUrl,
    required super.isVerified,
    super.dealershipName,
    required super.createdAt,
  });

  /// Create UserModel from JSON
  factory UserModel.fromJson(Map<String, dynamic> json) {
    return UserModel(
      id: json['id'] as String,
      email: json['email'] as String,
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      phoneNumber: json['phoneNumber'] as String?,
      role: (json['role'] as String).toUserRole(),
      avatarUrl: json['avatarUrl'] as String?,
      isVerified: json['isVerified'] as bool? ?? false,
      dealershipName: json['dealershipName'] as String?,
      createdAt: DateTime.parse(json['createdAt'] as String),
    );
  }

  /// Convert UserModel to JSON
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'email': email,
      'firstName': firstName,
      'lastName': lastName,
      'phoneNumber': phoneNumber,
      'role': role.toShortString(),
      'avatarUrl': avatarUrl,
      'isVerified': isVerified,
      'dealershipName': dealershipName,
      'createdAt': createdAt.toIso8601String(),
    };
  }

  /// Create UserModel from User entity
  factory UserModel.fromEntity(User user) {
    return UserModel(
      id: user.id,
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      phoneNumber: user.phoneNumber,
      role: user.role,
      avatarUrl: user.avatarUrl,
      isVerified: user.isVerified,
      dealershipName: user.dealershipName,
      createdAt: user.createdAt,
    );
  }

  /// Convert to User entity
  User toEntity() {
    return User(
      id: id,
      email: email,
      firstName: firstName,
      lastName: lastName,
      phoneNumber: phoneNumber,
      role: role,
      avatarUrl: avatarUrl,
      isVerified: isVerified,
      dealershipName: dealershipName,
      createdAt: createdAt,
    );
  }

  /// Copy with method
  @override
  UserModel copyWith({
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
    return UserModel(
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
}

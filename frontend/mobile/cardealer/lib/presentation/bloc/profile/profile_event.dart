import 'package:equatable/equatable.dart';

/// Base event for profile
abstract class ProfileEvent extends Equatable {
  const ProfileEvent();

  @override
  List<Object?> get props => [];
}

/// Load user profile
class LoadProfile extends ProfileEvent {}

/// Update profile information
class UpdateProfileEvent extends ProfileEvent {
  final String? firstName;
  final String? lastName;
  final String? phoneNumber;
  final String? avatarUrl;

  const UpdateProfileEvent({
    this.firstName,
    this.lastName,
    this.phoneNumber,
    this.avatarUrl,
  });

  @override
  List<Object?> get props => [firstName, lastName, phoneNumber, avatarUrl];
}

/// Upload avatar image
class UploadAvatar extends ProfileEvent {
  final String imagePath;

  const UploadAvatar(this.imagePath);

  @override
  List<Object?> get props => [imagePath];
}

/// Delete user account
class DeleteAccount extends ProfileEvent {}

/// Refresh profile
class RefreshProfile extends ProfileEvent {}

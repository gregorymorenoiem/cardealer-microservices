import 'package:equatable/equatable.dart';
import '../../../domain/entities/user.dart';

/// Base state for profile
abstract class ProfileState extends Equatable {
  const ProfileState();

  @override
  List<Object?> get props => [];
}

/// Initial state
class ProfileInitial extends ProfileState {}

/// Loading user profile
class ProfileLoading extends ProfileState {}

/// Profile loaded successfully
class ProfileLoaded extends ProfileState {
  final User user;

  const ProfileLoaded(this.user);

  @override
  List<Object?> get props => [user];
}

/// Updating profile in progress
class ProfileUpdating extends ProfileState {
  final User currentUser;

  const ProfileUpdating(this.currentUser);

  @override
  List<Object?> get props => [currentUser];
}

/// Profile updated successfully
class ProfileUpdated extends ProfileState {
  final User user;

  const ProfileUpdated(this.user);

  @override
  List<Object?> get props => [user];
}

/// Error state
class ProfileError extends ProfileState {
  final String message;

  const ProfileError(this.message);

  @override
  List<Object?> get props => [message];
}

/// Avatar uploading state
class ProfileAvatarUploading extends ProfileState {
  final User currentUser;

  const ProfileAvatarUploading(this.currentUser);

  @override
  List<Object?> get props => [currentUser];
}

/// Deleting account state
class ProfileDeleting extends ProfileState {}

/// Account deleted successfully
class ProfileDeleted extends ProfileState {}

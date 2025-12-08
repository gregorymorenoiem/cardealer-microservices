import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../domain/usecases/profile/get_user_profile.dart';
import '../../../domain/usecases/profile/update_profile.dart';
import 'profile_event.dart';
import 'profile_state.dart';

/// BLoC for managing user profile
class ProfileBloc extends Bloc<ProfileEvent, ProfileState> {
  final GetUserProfile getUserProfile;
  final UpdateProfile updateProfile;

  ProfileBloc({
    required this.getUserProfile,
    required this.updateProfile,
  }) : super(ProfileInitial()) {
    on<LoadProfile>(_onLoadProfile);
    on<UpdateProfileEvent>(_onUpdateProfile);
    on<UploadAvatar>(_onUploadAvatar);
    on<DeleteAccount>(_onDeleteAccount);
    on<RefreshProfile>(_onRefreshProfile);
  }

  Future<void> _onLoadProfile(
    LoadProfile event,
    Emitter<ProfileState> emit,
  ) async {
    emit(ProfileLoading());

    final result = await getUserProfile();

    result.fold(
      (failure) => emit(ProfileError(failure.message)),
      (user) => emit(ProfileLoaded(user)),
    );
  }

  Future<void> _onUpdateProfile(
    UpdateProfileEvent event,
    Emitter<ProfileState> emit,
  ) async {
    if (state is! ProfileLoaded) return;

    final currentUser = (state as ProfileLoaded).user;
    emit(ProfileUpdating(currentUser));

    final params = UpdateProfileParams(
      userId: currentUser.id,
      firstName: event.firstName,
      lastName: event.lastName,
      phoneNumber: event.phoneNumber,
      avatarUrl: event.avatarUrl,
    );

    final result = await updateProfile(params);

    result.fold(
      (failure) => emit(ProfileError(failure.message)),
      (user) => emit(ProfileUpdated(user)),
    );
  }

  Future<void> _onUploadAvatar(
    UploadAvatar event,
    Emitter<ProfileState> emit,
  ) async {
    if (state is! ProfileLoaded) return;

    final currentUser = (state as ProfileLoaded).user;
    emit(ProfileAvatarUploading(currentUser));

    // TODO: Implement actual upload to server
    // For now, just use the local path
    await Future.delayed(const Duration(seconds: 1));

    final params = UpdateProfileParams(
      userId: currentUser.id,
      avatarUrl: event.imagePath,
    );

    final result = await updateProfile(params);

    result.fold(
      (failure) => emit(ProfileError(failure.message)),
      (user) => emit(ProfileUpdated(user)),
    );
  }

  Future<void> _onDeleteAccount(
    DeleteAccount event,
    Emitter<ProfileState> emit,
  ) async {
    emit(ProfileDeleting());

    // TODO: Implement actual account deletion
    await Future.delayed(const Duration(seconds: 2));

    emit(ProfileDeleted());
  }

  Future<void> _onRefreshProfile(
    RefreshProfile event,
    Emitter<ProfileState> emit,
  ) async {
    final result = await getUserProfile();

    result.fold(
      (failure) => emit(ProfileError(failure.message)),
      (user) => emit(ProfileLoaded(user)),
    );
  }
}

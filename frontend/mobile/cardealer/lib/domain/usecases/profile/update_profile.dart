import 'package:dartz/dartz.dart';
import '../../../core/errors/failures.dart';
import '../../entities/user.dart';
import '../../repositories/auth_repository.dart';

/// Parameters for updating user profile
class UpdateProfileParams {
  final String userId;
  final String? firstName;
  final String? lastName;
  final String? phoneNumber;
  final String? avatarUrl;

  UpdateProfileParams({
    required this.userId,
    this.firstName,
    this.lastName,
    this.phoneNumber,
    this.avatarUrl,
  });

  bool get hasChanges =>
      firstName != null ||
      lastName != null ||
      phoneNumber != null ||
      avatarUrl != null;
}

/// Use case to update the current user's profile information
class UpdateProfile {
  final AuthRepository repository;

  UpdateProfile(this.repository);

  Future<Either<Failure, User>> call(UpdateProfileParams params) async {
    if (!params.hasChanges) {
      return const Left(ValidationFailure('No changes provided'));
    }

    // Basic validation
    if (params.phoneNumber != null && !_isValidPhone(params.phoneNumber!)) {
      return const Left(ValidationFailure('Invalid phone format'));
    }

    if (params.firstName != null && params.firstName!.trim().isEmpty) {
      return const Left(
          ValidationFailure('First name cannot be empty'));
    }

    if (params.lastName != null && params.lastName!.trim().isEmpty) {
      return const Left(
          ValidationFailure('Last name cannot be empty'));
    }

    return repository.updateProfile(
      userId: params.userId,
      firstName: params.firstName,
      lastName: params.lastName,
      phoneNumber: params.phoneNumber,
      avatarUrl: params.avatarUrl,
    );
  }

  bool _isValidPhone(String phone) {
    final phoneRegex = RegExp(r'^\+?[\d\s\-()]{10,}$');
    return phoneRegex.hasMatch(phone);
  }
}

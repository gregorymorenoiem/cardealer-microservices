import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/dealer_listing.dart';
import '../../repositories/dealer_repository.dart';

class CreateListing implements UseCase<DealerListing, DealerListing> {
  final DealerRepository repository;

  CreateListing(this.repository);

  @override
  Future<Either<Failure, DealerListing>> call(DealerListing listing) async {
    // Validation
    if (listing.title.trim().isEmpty) {
      return const Left(ValidationFailure(message: 'El título es requerido'));
    }

    if (listing.description.trim().isEmpty) {
      return const Left(ValidationFailure(message: 'La descripción es requerida'));
    }

    if (listing.images.isEmpty) {
      return const Left(
          ValidationFailure(message: 'Se requiere al menos una imagen'));
    }

    if (listing.price <= 0) {
      return const Left(ValidationFailure(message: 'El precio debe ser mayor a 0'));
    }

    return await repository.createListing(listing);
  }
}

class UpdateListing implements UseCase<DealerListing, DealerListing> {
  final DealerRepository repository;

  UpdateListing(this.repository);

  @override
  Future<Either<Failure, DealerListing>> call(DealerListing listing) async {
    // Validation
    if (listing.title.trim().isEmpty) {
      return const Left(ValidationFailure(message: 'El título es requerido'));
    }

    if (listing.description.trim().isEmpty) {
      return const Left(ValidationFailure(message: 'La descripción es requerida'));
    }

    if (listing.images.isEmpty) {
      return const Left(
          ValidationFailure(message: 'Se requiere al menos una imagen'));
    }

    if (listing.price <= 0) {
      return const Left(ValidationFailure(message: 'El precio debe ser mayor a 0'));
    }

    return await repository.updateListing(listing);
  }
}

class DeleteListing implements UseCase<void, String> {
  final DealerRepository repository;

  DeleteListing(this.repository);

  @override
  Future<Either<Failure, void>> call(String listingId) async {
    return await repository.deleteListing(listingId);
  }
}

class PublishListing implements UseCase<DealerListing, String> {
  final DealerRepository repository;

  PublishListing(this.repository);

  @override
  Future<Either<Failure, DealerListing>> call(String listingId) async {
    return await repository.publishListing(listingId);
  }
}

import 'package:dartz/dartz.dart';
import 'package:equatable/equatable.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/lead.dart';
import '../../repositories/dealer_repository.dart';

class UpdateLeadStatus implements UseCase<Lead, UpdateLeadStatusParams> {
  final DealerRepository repository;

  UpdateLeadStatus(this.repository);

  @override
  Future<Either<Failure, Lead>> call(UpdateLeadStatusParams params) async {
    return await repository.updateLeadStatus(
      leadId: params.leadId,
      status: params.status,
    );
  }
}

class UpdateLeadStatusParams extends Equatable {
  final String leadId;
  final LeadStatus status;

  const UpdateLeadStatusParams({
    required this.leadId,
    required this.status,
  });

  @override
  List<Object> get props => [leadId, status];
}

class UpdateLeadNotes implements UseCase<Lead, UpdateLeadNotesParams> {
  final DealerRepository repository;

  UpdateLeadNotes(this.repository);

  @override
  Future<Either<Failure, Lead>> call(UpdateLeadNotesParams params) async {
    if (params.notes.trim().isEmpty) {
      return const Left(
          ValidationFailure(message: 'Las notas no pueden estar vacías'));
    }

    return await repository.updateLeadNotes(
      leadId: params.leadId,
      notes: params.notes,
    );
  }
}

class UpdateLeadNotesParams extends Equatable {
  final String leadId;
  final String notes;

  const UpdateLeadNotesParams({
    required this.leadId,
    required this.notes,
  });

  @override
  List<Object> get props => [leadId, notes];
}

class ScheduleFollowUp implements UseCase<Lead, ScheduleFollowUpParams> {
  final DealerRepository repository;

  ScheduleFollowUp(this.repository);

  @override
  Future<Either<Failure, Lead>> call(ScheduleFollowUpParams params) async {
    if (params.followUpDate.isBefore(DateTime.now())) {
      return const Left(ValidationFailure(
        message: 'La fecha de seguimiento debe ser futura',
      ));
    }

    return await repository.scheduleFollowUp(
      leadId: params.leadId,
      followUpDate: params.followUpDate,
    );
  }
}

class ScheduleFollowUpParams extends Equatable {
  final String leadId;
  final DateTime followUpDate;

  const ScheduleFollowUpParams({
    required this.leadId,
    required this.followUpDate,
  });

  @override
  List<Object> get props => [leadId, followUpDate];
}

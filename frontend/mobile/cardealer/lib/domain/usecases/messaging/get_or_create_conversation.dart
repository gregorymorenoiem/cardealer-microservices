import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../entities/conversation.dart';
import '../../repositories/messaging_repository.dart';

/// Use case to get or create a conversation with another user
class GetOrCreateConversation {
  final MessagingRepository repository;

  GetOrCreateConversation(this.repository);

  Future<Either<Failure, Conversation>> call(
    GetOrCreateConversationParams params,
  ) async {
    return await repository.getOrCreateConversation(
      otherUserId: params.otherUserId,
      vehicleId: params.vehicleId,
    );
  }
}

/// Parameters for getting or creating a conversation
class GetOrCreateConversationParams {
  final String otherUserId;
  final String? vehicleId;

  const GetOrCreateConversationParams({
    required this.otherUserId,
    this.vehicleId,
  });
}

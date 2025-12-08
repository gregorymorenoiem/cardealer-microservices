import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../entities/message.dart';
import '../../repositories/messaging_repository.dart';

/// Use case to get messages for a specific conversation
class GetMessages {
  final MessagingRepository repository;

  GetMessages(this.repository);

  Future<Either<Failure, List<Message>>> call(GetMessagesParams params) async {
    return await repository.getMessages(
      conversationId: params.conversationId,
      limit: params.limit,
      beforeMessageId: params.beforeMessageId,
    );
  }
}

/// Parameters for getting messages
class GetMessagesParams {
  final String conversationId;
  final int? limit;
  final String? beforeMessageId; // For pagination

  const GetMessagesParams({
    required this.conversationId,
    this.limit = 50,
    this.beforeMessageId,
  });
}

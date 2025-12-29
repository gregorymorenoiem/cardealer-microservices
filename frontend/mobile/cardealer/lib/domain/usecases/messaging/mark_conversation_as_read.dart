import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../repositories/messaging_repository.dart';

/// Use case to mark a conversation as read
class MarkConversationAsRead {
  final MessagingRepository repository;

  MarkConversationAsRead(this.repository);

  Future<Either<Failure, void>> call(String conversationId) async {
    return await repository.markConversationAsRead(conversationId);
  }
}

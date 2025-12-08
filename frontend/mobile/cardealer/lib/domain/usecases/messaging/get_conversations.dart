import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../entities/conversation.dart';
import '../../repositories/messaging_repository.dart';

/// Use case to get all conversations for current user
class GetConversations {
  final MessagingRepository repository;

  GetConversations(this.repository);

  Future<Either<Failure, List<Conversation>>> call() async {
    return await repository.getConversations();
  }
}

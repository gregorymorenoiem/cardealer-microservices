import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../entities/message.dart';
import '../../repositories/messaging_repository.dart';

/// Use case to send a message in a conversation
class SendMessage {
  final MessagingRepository repository;

  SendMessage(this.repository);

  Future<Either<Failure, Message>> call(SendMessageParams params) async {
    // Validate message content
    if (params.content.trim().isEmpty && params.type == MessageType.text) {
      return const Left(
        ValidationFailure(message: 'Message content cannot be empty'),
      );
    }

    return await repository.sendMessage(
      conversationId: params.conversationId,
      content: params.content,
      type: params.type,
      metadata: params.metadata,
    );
  }
}

/// Parameters for sending a message
class SendMessageParams {
  final String conversationId;
  final String content;
  final MessageType type;
  final Map<String, dynamic>? metadata;

  const SendMessageParams({
    required this.conversationId,
    required this.content,
    this.type = MessageType.text,
    this.metadata,
  });
}

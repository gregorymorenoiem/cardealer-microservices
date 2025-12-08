import 'package:dartz/dartz.dart';
import '../../core/error/failures.dart';
import '../entities/conversation.dart';
import '../entities/message.dart';

/// Repository interface for messaging operations
abstract class MessagingRepository {
  /// Get all conversations for current user
  Future<Either<Failure, List<Conversation>>> getConversations();

  /// Get conversation by ID
  Future<Either<Failure, Conversation>> getConversationById(
      String conversationId);

  /// Get or create conversation with specific user about a vehicle
  Future<Either<Failure, Conversation>> getOrCreateConversation({
    required String otherUserId,
    String? vehicleId,
  });

  /// Get messages for a conversation
  Future<Either<Failure, List<Message>>> getMessages({
    required String conversationId,
    int? limit,
    String? beforeMessageId,
  });

  /// Send a new message
  Future<Either<Failure, Message>> sendMessage({
    required String conversationId,
    required String content,
    required MessageType type,
    Map<String, dynamic>? metadata,
  });

  /// Mark message as read
  Future<Either<Failure, void>> markMessageAsRead(String messageId);

  /// Mark all messages in conversation as read
  Future<Either<Failure, void>> markConversationAsRead(String conversationId);

  /// Delete message
  Future<Either<Failure, void>> deleteMessage(String messageId);

  /// Delete conversation
  Future<Either<Failure, void>> deleteConversation(String conversationId);

  /// Listen to new messages in real-time
  Stream<Message> listenToMessages(String conversationId);

  /// Listen to conversation updates
  Stream<Conversation> listenToConversations();

  /// Get unread messages count
  Future<Either<Failure, int>> getUnreadCount();
}

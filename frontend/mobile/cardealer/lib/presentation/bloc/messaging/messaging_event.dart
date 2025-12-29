import 'package:equatable/equatable.dart';
import '../../../domain/entities/message.dart';

/// Base class for all messaging events
abstract class MessagingEvent extends Equatable {
  const MessagingEvent();

  @override
  List<Object?> get props => [];
}

/// Load all conversations
class LoadConversations extends MessagingEvent {}

/// Refresh conversations
class RefreshConversations extends MessagingEvent {}

/// Load messages for a specific conversation
class LoadMessages extends MessagingEvent {
  final String conversationId;
  final String? beforeMessageId; // For pagination

  const LoadMessages({
    required this.conversationId,
    this.beforeMessageId,
  });

  @override
  List<Object?> get props => [conversationId, beforeMessageId];
}

/// Send a message
class SendMessageEvent extends MessagingEvent {
  final String conversationId;
  final String content;
  final MessageType type;
  final Map<String, dynamic>? metadata;

  const SendMessageEvent({
    required this.conversationId,
    required this.content,
    this.type = MessageType.text,
    this.metadata,
  });

  @override
  List<Object?> get props => [conversationId, content, type, metadata];
}

/// Mark conversation as read
class MarkAsRead extends MessagingEvent {
  final String conversationId;

  const MarkAsRead(this.conversationId);

  @override
  List<Object?> get props => [conversationId];
}

/// Delete conversation
class DeleteConversationEvent extends MessagingEvent {
  final String conversationId;

  const DeleteConversationEvent(this.conversationId);

  @override
  List<Object?> get props => [conversationId];
}

/// Listen to new messages (real-time)
class ListenToMessagesEvent extends MessagingEvent {
  final String conversationId;

  const ListenToMessagesEvent(this.conversationId);

  @override
  List<Object?> get props => [conversationId];
}

/// Stop listening to messages
class StopListeningToMessagesEvent extends MessagingEvent {
  final String conversationId;

  const StopListeningToMessagesEvent(this.conversationId);

  @override
  List<Object?> get props => [conversationId];
}

import 'package:equatable/equatable.dart';
import '../../../domain/entities/conversation.dart';
import '../../../domain/entities/message.dart';

/// Base class for all messaging states
abstract class MessagingState extends Equatable {
  const MessagingState();

  @override
  List<Object?> get props => [];
}

/// Initial state
class MessagingInitial extends MessagingState {}

/// Loading conversations
class ConversationsLoading extends MessagingState {}

/// Conversations loaded successfully
class ConversationsLoaded extends MessagingState {
  final List<Conversation> conversations;
  final int unreadCount;

  const ConversationsLoaded({
    required this.conversations,
    this.unreadCount = 0,
  });

  @override
  List<Object?> get props => [conversations, unreadCount];
}

/// Empty conversations
class ConversationsEmpty extends MessagingState {}

/// Error loading conversations
class ConversationsError extends MessagingState {
  final String message;

  const ConversationsError(this.message);

  @override
  List<Object?> get props => [message];
}

/// Loading messages for a conversation
class MessagesLoading extends MessagingState {
  final String conversationId;

  const MessagesLoading(this.conversationId);

  @override
  List<Object?> get props => [conversationId];
}

/// Messages loaded successfully
class MessagesLoaded extends MessagingState {
  final String conversationId;
  final List<Message> messages;
  final bool hasMore; // For pagination

  const MessagesLoaded({
    required this.conversationId,
    required this.messages,
    this.hasMore = false,
  });

  @override
  List<Object?> get props => [conversationId, messages, hasMore];
}

/// Sending a message
class MessageSending extends MessagingState {
  final String conversationId;
  final String content;

  const MessageSending({
    required this.conversationId,
    required this.content,
  });

  @override
  List<Object?> get props => [conversationId, content];
}

/// Message sent successfully
class MessageSent extends MessagingState {
  final Message message;

  const MessageSent(this.message);

  @override
  List<Object?> get props => [message];
}

/// Error sending message
class MessageSendError extends MessagingState {
  final String message;

  const MessageSendError(this.message);

  @override
  List<Object?> get props => [message];
}

/// New message received (real-time)
class MessageReceived extends MessagingState {
  final Message message;

  const MessageReceived(this.message);

  @override
  List<Object?> get props => [message];
}

/// Conversation deleted
class ConversationDeleted extends MessagingState {
  final String conversationId;

  const ConversationDeleted(this.conversationId);

  @override
  List<Object?> get props => [conversationId];
}

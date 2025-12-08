import 'dart:async';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../domain/usecases/messaging/get_conversations.dart';
import '../../../domain/usecases/messaging/get_messages.dart';
import '../../../domain/usecases/messaging/send_message.dart';
import '../../../domain/usecases/messaging/mark_conversation_as_read.dart';
import '../../../domain/repositories/messaging_repository.dart';
import 'messaging_event.dart';
import 'messaging_state.dart';

/// BLoC for managing messaging state
class MessagingBloc extends Bloc<MessagingEvent, MessagingState> {
  final GetConversations getConversations;
  final GetMessages getMessages;
  final SendMessage sendMessage;
  final MarkConversationAsRead markConversationAsRead;
  final MessagingRepository messagingRepository;

  StreamSubscription? _messageSubscription;

  MessagingBloc({
    required this.getConversations,
    required this.getMessages,
    required this.sendMessage,
    required this.markConversationAsRead,
    required this.messagingRepository,
  }) : super(MessagingInitial()) {
    on<LoadConversations>(_onLoadConversations);
    on<RefreshConversations>(_onRefreshConversations);
    on<LoadMessages>(_onLoadMessages);
    on<SendMessageEvent>(_onSendMessage);
    on<MarkAsRead>(_onMarkAsRead);
    on<DeleteConversationEvent>(_onDeleteConversation);
    on<ListenToMessagesEvent>(_onListenToMessages);
    on<StopListeningToMessagesEvent>(_onStopListeningToMessages);
  }

  Future<void> _onLoadConversations(
    LoadConversations event,
    Emitter<MessagingState> emit,
  ) async {
    emit(ConversationsLoading());

    final result = await getConversations();

    result.fold(
      (failure) => emit(ConversationsError(failure.message)),
      (conversations) {
        if (conversations.isEmpty) {
          emit(ConversationsEmpty());
        } else {
          // Calculate total unread count
          final unreadCount = conversations.fold<int>(
            0,
            (sum, conv) => sum + conv.unreadCount,
          );

          emit(ConversationsLoaded(
            conversations: conversations,
            unreadCount: unreadCount,
          ));
        }
      },
    );
  }

  Future<void> _onRefreshConversations(
    RefreshConversations event,
    Emitter<MessagingState> emit,
  ) async {
    // Don't show loading state on refresh, just update data
    final result = await getConversations();

    result.fold(
      (failure) => emit(ConversationsError(failure.message)),
      (conversations) {
        if (conversations.isEmpty) {
          emit(ConversationsEmpty());
        } else {
          final unreadCount = conversations.fold<int>(
            0,
            (sum, conv) => sum + conv.unreadCount,
          );

          emit(ConversationsLoaded(
            conversations: conversations,
            unreadCount: unreadCount,
          ));
        }
      },
    );
  }

  Future<void> _onLoadMessages(
    LoadMessages event,
    Emitter<MessagingState> emit,
  ) async {
    emit(MessagesLoading(event.conversationId));

    final result = await getMessages(
      GetMessagesParams(
        conversationId: event.conversationId,
        beforeMessageId: event.beforeMessageId,
      ),
    );

    result.fold(
      (failure) => emit(ConversationsError(failure.message)),
      (messages) {
        emit(MessagesLoaded(
          conversationId: event.conversationId,
          messages: messages,
          hasMore: messages.length >= 50, // Assuming limit of 50
        ));
      },
    );
  }

  Future<void> _onSendMessage(
    SendMessageEvent event,
    Emitter<MessagingState> emit,
  ) async {
    emit(MessageSending(
      conversationId: event.conversationId,
      content: event.content,
    ));

    final result = await sendMessage(
      SendMessageParams(
        conversationId: event.conversationId,
        content: event.content,
        type: event.type,
        metadata: event.metadata,
      ),
    );

    result.fold(
      (failure) => emit(MessageSendError(failure.message)),
      (message) {
        emit(MessageSent(message));

        // Reload messages to show the new message
        add(LoadMessages(conversationId: event.conversationId));
      },
    );
  }

  Future<void> _onMarkAsRead(
    MarkAsRead event,
    Emitter<MessagingState> emit,
  ) async {
    await markConversationAsRead(event.conversationId);

    // Refresh conversations to update unread count
    add(RefreshConversations());
  }

  Future<void> _onDeleteConversation(
    DeleteConversationEvent event,
    Emitter<MessagingState> emit,
  ) async {
    final result = await messagingRepository.deleteConversation(
      event.conversationId,
    );

    result.fold(
      (failure) => emit(ConversationsError(failure.message)),
      (_) {
        emit(ConversationDeleted(event.conversationId));

        // Reload conversations
        add(LoadConversations());
      },
    );
  }

  Future<void> _onListenToMessages(
    ListenToMessagesEvent event,
    Emitter<MessagingState> emit,
  ) async {
    // Cancel existing subscription if any
    await _messageSubscription?.cancel();

    // Start listening to new messages
    _messageSubscription = messagingRepository
        .listenToMessages(event.conversationId)
        .listen((message) {
      emit(MessageReceived(message));

      // Reload messages to include the new one
      add(LoadMessages(conversationId: event.conversationId));
    });
  }

  Future<void> _onStopListeningToMessages(
    StopListeningToMessagesEvent event,
    Emitter<MessagingState> emit,
  ) async {
    await _messageSubscription?.cancel();
    _messageSubscription = null;
  }

  @override
  Future<void> close() {
    _messageSubscription?.cancel();
    return super.close();
  }
}

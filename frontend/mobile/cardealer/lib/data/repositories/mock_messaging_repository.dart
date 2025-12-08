import 'dart:async';
import 'package:dartz/dartz.dart';
import '../../core/error/failures.dart';
import '../../domain/entities/conversation.dart';
import '../../domain/entities/message.dart';
import '../../domain/repositories/messaging_repository.dart';
import '../models/conversation_model.dart';
import '../models/message_model.dart';

/// Mock implementation of MessagingRepository
/// TODO: Replace with real API calls when backend is ready
class MockMessagingRepository implements MessagingRepository {
  // In-memory storage for mock data
  final List<ConversationModel> _conversations = [];
  final Map<String, List<MessageModel>> _messages = {};
  
  // Stream controllers for real-time updates
  final StreamController<Message> _messageStreamController =
      StreamController<Message>.broadcast();
  final StreamController<Conversation> _conversationStreamController =
      StreamController<Conversation>.broadcast();

  final String _currentUserId = 'current-user-id'; // Mock current user ID

  MockMessagingRepository() {
    _initializeMockData();
  }

  /// Initialize with mock conversations and messages
  void _initializeMockData() {
    final now = DateTime.now();
    
    // Mock conversation 1
    final conv1 = ConversationModel(
      id: 'conv-1',
      userId: 'user-1',
      userName: 'Carlos Rodríguez',
      userAvatar: 'https://i.pravatar.cc/150?img=12',
      vehicleId: 'vehicle-1',
      vehicleTitle: 'Toyota Corolla 2022',
      vehicleImage: 'https://images.unsplash.com/photo-1623869675781-80aa0ba4f4fd',
      lastMessage: MessageModel(
        id: 'msg-1-3',
        conversationId: 'conv-1',
        senderId: 'user-1',
        senderName: 'Carlos Rodríguez',
        receiverId: _currentUserId,
        content: '¿Podemos agendar una prueba de manejo para mañana?',
        type: MessageType.text,
        status: MessageStatus.delivered,
        createdAt: now.subtract(const Duration(minutes: 5)),
      ),
      unreadCount: 1,
      isOnline: true,
      createdAt: now.subtract(const Duration(days: 2)),
      updatedAt: now.subtract(const Duration(minutes: 5)),
    );

    // Mock messages for conversation 1
    _messages['conv-1'] = [
      MessageModel(
        id: 'msg-1-1',
        conversationId: 'conv-1',
        senderId: _currentUserId,
        senderName: 'Tú',
        receiverId: 'user-1',
        content: 'Hola, estoy interesado en el Toyota Corolla',
        type: MessageType.text,
        status: MessageStatus.read,
        createdAt: now.subtract(const Duration(hours: 2)),
        readAt: now.subtract(const Duration(hours: 1, minutes: 58)),
      ),
      MessageModel(
        id: 'msg-1-2',
        conversationId: 'conv-1',
        senderId: 'user-1',
        senderName: 'Carlos Rodríguez',
        receiverId: _currentUserId,
        content: '¡Perfecto! ¿Te gustaría agendar una prueba de manejo?',
        type: MessageType.text,
        status: MessageStatus.read,
        createdAt: now.subtract(const Duration(hours: 1, minutes: 55)),
        readAt: now.subtract(const Duration(hours: 1, minutes: 50)),
      ),
      MessageModel(
        id: 'msg-1-3',
        conversationId: 'conv-1',
        senderId: 'user-1',
        senderName: 'Carlos Rodríguez',
        receiverId: _currentUserId,
        content: '¿Podemos agendar una prueba de manejo para mañana?',
        type: MessageType.text,
        status: MessageStatus.delivered,
        createdAt: now.subtract(const Duration(minutes: 5)),
      ),
    ];

    // Mock conversation 2
    final conv2 = ConversationModel(
      id: 'conv-2',
      userId: 'user-2',
      userName: 'María García',
      userAvatar: 'https://i.pravatar.cc/150?img=47',
      vehicleId: 'vehicle-5',
      vehicleTitle: 'Honda CR-V 2023',
      vehicleImage: 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6',
      lastMessage: MessageModel(
        id: 'msg-2-2',
        conversationId: 'conv-2',
        senderId: _currentUserId,
        senderName: 'Tú',
        receiverId: 'user-2',
        content: 'Gracias por la información',
        type: MessageType.text,
        status: MessageStatus.read,
        createdAt: now.subtract(const Duration(hours: 3)),
        readAt: now.subtract(const Duration(hours: 2, minutes: 50)),
      ),
      unreadCount: 0,
      isOnline: false,
      lastSeenAt: now.subtract(const Duration(minutes: 30)),
      createdAt: now.subtract(const Duration(days: 1)),
      updatedAt: now.subtract(const Duration(hours: 3)),
    );

    _messages['conv-2'] = [
      MessageModel(
        id: 'msg-2-1',
        conversationId: 'conv-2',
        senderId: 'user-2',
        senderName: 'María García',
        receiverId: _currentUserId,
        content: 'El vehículo está en excelentes condiciones, revisión reciente',
        type: MessageType.text,
        status: MessageStatus.read,
        createdAt: now.subtract(const Duration(hours: 4)),
        readAt: now.subtract(const Duration(hours: 3, minutes: 55)),
      ),
      MessageModel(
        id: 'msg-2-2',
        conversationId: 'conv-2',
        senderId: _currentUserId,
        senderName: 'Tú',
        receiverId: 'user-2',
        content: 'Gracias por la información',
        type: MessageType.text,
        status: MessageStatus.read,
        createdAt: now.subtract(const Duration(hours: 3)),
        readAt: now.subtract(const Duration(hours: 2, minutes: 50)),
      ),
    ];

    _conversations.addAll([conv1, conv2]);
  }

  @override
  Future<Either<Failure, List<Conversation>>> getConversations() async {
    try {
      await Future.delayed(const Duration(milliseconds: 500)); // Simulate network delay
      
      // Sort by updated date
      final sorted = List<ConversationModel>.from(_conversations)
        ..sort((a, b) => b.updatedAt.compareTo(a.updatedAt));
      
      return Right(sorted);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Conversation>> getConversationById(
    String conversationId,
  ) async {
    try {
      await Future.delayed(const Duration(milliseconds: 300));
      
      final conversation = _conversations.firstWhere(
        (c) => c.id == conversationId,
        orElse: () => throw Exception('Conversation not found'),
      );
      
      return Right(conversation);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Conversation>> getOrCreateConversation({
    required String otherUserId,
    String? vehicleId,
  }) async {
    try {
      await Future.delayed(const Duration(milliseconds: 400));
      
      // Try to find existing conversation
      try {
        final existing = _conversations.firstWhere(
          (c) => c.userId == otherUserId && c.vehicleId == vehicleId,
        );
        return Right(existing);
      } catch (_) {
        // Create new conversation
        final now = DateTime.now();
        final newConv = ConversationModel(
          id: 'conv-${_conversations.length + 1}',
          userId: otherUserId,
          userName: 'Usuario $otherUserId',
          userAvatar: 'https://i.pravatar.cc/150?img=${_conversations.length + 10}',
          vehicleId: vehicleId,
          vehicleTitle: vehicleId != null ? 'Vehículo $vehicleId' : null,
          unreadCount: 0,
          isOnline: false,
          createdAt: now,
          updatedAt: now,
        );
        
        _conversations.add(newConv);
        _messages[newConv.id] = [];
        
        return Right(newConv);
      }
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Message>>> getMessages({
    required String conversationId,
    int? limit,
    String? beforeMessageId,
  }) async {
    try {
      await Future.delayed(const Duration(milliseconds: 400));
      
      final messages = _messages[conversationId] ?? [];
      
      if (messages.isEmpty) {
        return const Right([]);
      }
      
      // Apply pagination if beforeMessageId is provided
      var result = List<MessageModel>.from(messages);
      if (beforeMessageId != null) {
        final index = result.indexWhere((m) => m.id == beforeMessageId);
        if (index > 0) {
          result = result.sublist(0, index);
        }
      }
      
      // Apply limit
      if (limit != null && result.length > limit) {
        result = result.sublist(result.length - limit);
      }
      
      return Right(result);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Message>> sendMessage({
    required String conversationId,
    required String content,
    required MessageType type,
    Map<String, dynamic>? metadata,
  }) async {
    try {
      await Future.delayed(const Duration(milliseconds: 600));
      
      final now = DateTime.now();
      final newMessage = MessageModel(
        id: 'msg-${conversationId}-${DateTime.now().millisecondsSinceEpoch}',
        conversationId: conversationId,
        senderId: _currentUserId,
        senderName: 'Tú',
        receiverId: 'other-user',
        content: content,
        type: type,
        status: MessageStatus.sent,
        createdAt: now,
        metadata: metadata,
      );
      
      // Add to messages
      if (_messages[conversationId] == null) {
        _messages[conversationId] = [];
      }
      _messages[conversationId]!.add(newMessage);
      
      // Update conversation
      final convIndex = _conversations.indexWhere((c) => c.id == conversationId);
      if (convIndex != -1) {
        final conv = _conversations[convIndex];
        _conversations[convIndex] = ConversationModel.fromEntity(
          conv.copyWith(
            lastMessage: newMessage,
            updatedAt: now,
          ),
        );
      }
      
      // Emit to stream
      _messageStreamController.add(newMessage);
      
      return Right(newMessage);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> markMessageAsRead(String messageId) async {
    try {
      await Future.delayed(const Duration(milliseconds: 200));
      return const Right(null);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> markConversationAsRead(
    String conversationId,
  ) async {
    try {
      await Future.delayed(const Duration(milliseconds: 300));
      
      // Update unread count
      final convIndex = _conversations.indexWhere((c) => c.id == conversationId);
      if (convIndex != -1) {
        final conv = _conversations[convIndex];
        _conversations[convIndex] = ConversationModel.fromEntity(
          conv.copyWith(unreadCount: 0),
        );
      }
      
      return const Right(null);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deleteMessage(String messageId) async {
    try {
      await Future.delayed(const Duration(milliseconds: 300));
      
      // Find and remove message
      for (final messages in _messages.values) {
        messages.removeWhere((m) => m.id == messageId);
      }
      
      return const Right(null);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deleteConversation(String conversationId) async {
    try {
      await Future.delayed(const Duration(milliseconds: 300));
      
      _conversations.removeWhere((c) => c.id == conversationId);
      _messages.remove(conversationId);
      
      return const Right(null);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Stream<Message> listenToMessages(String conversationId) {
    return _messageStreamController.stream
        .where((message) => message.conversationId == conversationId);
  }

  @override
  Stream<Conversation> listenToConversations() {
    return _conversationStreamController.stream;
  }

  @override
  Future<Either<Failure, int>> getUnreadCount() async {
    try {
      await Future.delayed(const Duration(milliseconds: 200));
      
      final total = _conversations.fold<int>(
        0,
        (sum, conv) => sum + conv.unreadCount,
      );
      
      return Right(total);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  void dispose() {
    _messageStreamController.close();
    _conversationStreamController.close();
  }
}

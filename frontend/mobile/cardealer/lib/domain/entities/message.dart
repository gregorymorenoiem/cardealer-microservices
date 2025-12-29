import 'package:equatable/equatable.dart';

/// Message entity representing a chat message between users
class Message extends Equatable {
  final String id;
  final String conversationId;
  final String senderId;
  final String senderName;
  final String? senderAvatar;
  final String receiverId;
  final String content;
  final MessageType type;
  final MessageStatus status;
  final DateTime createdAt;
  final DateTime? readAt;
  final Map<String, dynamic>? metadata; // For media attachments

  const Message({
    required this.id,
    required this.conversationId,
    required this.senderId,
    required this.senderName,
    this.senderAvatar,
    required this.receiverId,
    required this.content,
    required this.type,
    required this.status,
    required this.createdAt,
    this.readAt,
    this.metadata,
  });

  /// Check if message is from current user
  bool isFromMe(String currentUserId) => senderId == currentUserId;

  /// Check if message has been read
  bool get isRead => readAt != null;

  /// Get media URL from metadata
  String? get mediaUrl => metadata?['url'] as String?;

  /// Get thumbnail URL from metadata
  String? get thumbnailUrl => metadata?['thumbnail'] as String?;

  /// Copy with method for immutability
  Message copyWith({
    String? id,
    String? conversationId,
    String? senderId,
    String? senderName,
    String? senderAvatar,
    String? receiverId,
    String? content,
    MessageType? type,
    MessageStatus? status,
    DateTime? createdAt,
    DateTime? readAt,
    Map<String, dynamic>? metadata,
  }) {
    return Message(
      id: id ?? this.id,
      conversationId: conversationId ?? this.conversationId,
      senderId: senderId ?? this.senderId,
      senderName: senderName ?? this.senderName,
      senderAvatar: senderAvatar ?? this.senderAvatar,
      receiverId: receiverId ?? this.receiverId,
      content: content ?? this.content,
      type: type ?? this.type,
      status: status ?? this.status,
      createdAt: createdAt ?? this.createdAt,
      readAt: readAt ?? this.readAt,
      metadata: metadata ?? this.metadata,
    );
  }

  @override
  List<Object?> get props => [
        id,
        conversationId,
        senderId,
        senderName,
        senderAvatar,
        receiverId,
        content,
        type,
        status,
        createdAt,
        readAt,
        metadata,
      ];
}

/// Message type enum
enum MessageType {
  text,
  image,
  video,
  document,
  location,
  vehicleCard, // Special type for sharing vehicle info
}

/// Message delivery/read status
enum MessageStatus {
  sending,
  sent,
  delivered,
  read,
  failed,
}

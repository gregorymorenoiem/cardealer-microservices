import '../../domain/entities/conversation.dart';
import 'message_model.dart';

/// Model class for Conversation with JSON serialization
class ConversationModel extends Conversation {
  const ConversationModel({
    required super.id,
    required super.userId,
    required super.userName,
    super.userAvatar,
    super.vehicleId,
    super.vehicleTitle,
    super.vehicleImage,
    super.lastMessage,
    required super.unreadCount,
    required super.isOnline,
    super.lastSeenAt,
    required super.createdAt,
    required super.updatedAt,
  });

  /// Create from JSON
  factory ConversationModel.fromJson(Map<String, dynamic> json) {
    return ConversationModel(
      id: json['id'] as String,
      userId: json['userId'] as String,
      userName: json['userName'] as String,
      userAvatar: json['userAvatar'] as String?,
      vehicleId: json['vehicleId'] as String?,
      vehicleTitle: json['vehicleTitle'] as String?,
      vehicleImage: json['vehicleImage'] as String?,
      lastMessage: json['lastMessage'] != null
          ? MessageModel.fromJson(json['lastMessage'] as Map<String, dynamic>)
          : null,
      unreadCount: json['unreadCount'] as int,
      isOnline: json['isOnline'] as bool,
      lastSeenAt: json['lastSeenAt'] != null
          ? DateTime.parse(json['lastSeenAt'] as String)
          : null,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
    );
  }

  /// Convert to JSON
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'userId': userId,
      'userName': userName,
      'userAvatar': userAvatar,
      'vehicleId': vehicleId,
      'vehicleTitle': vehicleTitle,
      'vehicleImage': vehicleImage,
      'lastMessage': lastMessage != null
          ? MessageModel.fromEntity(lastMessage!).toJson()
          : null,
      'unreadCount': unreadCount,
      'isOnline': isOnline,
      'lastSeenAt': lastSeenAt?.toIso8601String(),
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
    };
  }

  /// Create from domain entity
  factory ConversationModel.fromEntity(Conversation conversation) {
    return ConversationModel(
      id: conversation.id,
      userId: conversation.userId,
      userName: conversation.userName,
      userAvatar: conversation.userAvatar,
      vehicleId: conversation.vehicleId,
      vehicleTitle: conversation.vehicleTitle,
      vehicleImage: conversation.vehicleImage,
      lastMessage: conversation.lastMessage,
      unreadCount: conversation.unreadCount,
      isOnline: conversation.isOnline,
      lastSeenAt: conversation.lastSeenAt,
      createdAt: conversation.createdAt,
      updatedAt: conversation.updatedAt,
    );
  }
}

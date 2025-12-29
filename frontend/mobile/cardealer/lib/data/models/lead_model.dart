import '../../domain/entities/lead.dart';

class LeadModel extends Lead {
  const LeadModel({
    required super.id,
    required super.dealerId,
    required super.listingId,
    required super.userId,
    required super.userName,
    required super.userEmail,
    super.userPhone,
    super.userAvatar,
    required super.vehicleTitle,
    super.vehicleImage,
    required super.vehiclePrice,
    required super.status,
    required super.priority,
    required super.source,
    super.message,
    super.notes,
    super.budget,
    super.needsFinancing,
    super.hasTradeIn,
    super.tradeInDetails,
    required super.messageCount,
    required super.callCount,
    super.lastContactedAt,
    super.nextFollowUpAt,
    super.assignedTo,
    super.offeredPrice,
    super.lostReason,
    super.convertedAt,
    super.soldPrice,
    required super.createdAt,
    required super.updatedAt,
  });

  factory LeadModel.fromJson(Map<String, dynamic> json) {
    return LeadModel(
      id: json['id'] as String,
      dealerId: json['dealerId'] as String,
      listingId: json['listingId'] as String,
      userId: json['userId'] as String,
      userName: json['userName'] as String,
      userEmail: json['userEmail'] as String,
      userPhone: json['userPhone'] as String?,
      userAvatar: json['userAvatar'] as String?,
      vehicleTitle: json['vehicleTitle'] as String,
      vehicleImage: json['vehicleImage'] as String?,
      vehiclePrice: (json['vehiclePrice'] as num).toDouble(),
      status: LeadStatus.values.firstWhere(
        (e) => e.name == json['status'],
      ),
      priority: LeadPriority.values.firstWhere(
        (e) => e.name == json['priority'],
      ),
      source: LeadSource.values.firstWhere(
        (e) => e.name == json['source'],
      ),
      message: json['message'] as String?,
      notes: json['notes'] as String?,
      budget:
          json['budget'] != null ? (json['budget'] as num).toDouble() : null,
      needsFinancing: json['needsFinancing'] as bool?,
      hasTradeIn: json['hasTradeIn'] as bool?,
      tradeInDetails: json['tradeInDetails'] as String?,
      messageCount: json['messageCount'] as int,
      callCount: json['callCount'] as int,
      lastContactedAt: json['lastContactedAt'] != null
          ? DateTime.parse(json['lastContactedAt'] as String)
          : null,
      nextFollowUpAt: json['nextFollowUpAt'] != null
          ? DateTime.parse(json['nextFollowUpAt'] as String)
          : null,
      assignedTo: json['assignedTo'] as String?,
      offeredPrice: json['offeredPrice'] != null
          ? (json['offeredPrice'] as num).toDouble()
          : null,
      lostReason: json['lostReason'] as String?,
      convertedAt: json['convertedAt'] != null
          ? DateTime.parse(json['convertedAt'] as String)
          : null,
      soldPrice: json['soldPrice'] != null
          ? (json['soldPrice'] as num).toDouble()
          : null,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'dealerId': dealerId,
      'listingId': listingId,
      'userId': userId,
      'userName': userName,
      'userEmail': userEmail,
      'userPhone': userPhone,
      'userAvatar': userAvatar,
      'vehicleTitle': vehicleTitle,
      'vehicleImage': vehicleImage,
      'vehiclePrice': vehiclePrice,
      'status': status.name,
      'priority': priority.name,
      'source': source.name,
      'message': message,
      'notes': notes,
      'budget': budget,
      'needsFinancing': needsFinancing,
      'hasTradeIn': hasTradeIn,
      'tradeInDetails': tradeInDetails,
      'messageCount': messageCount,
      'callCount': callCount,
      'lastContactedAt': lastContactedAt?.toIso8601String(),
      'nextFollowUpAt': nextFollowUpAt?.toIso8601String(),
      'assignedTo': assignedTo,
      'offeredPrice': offeredPrice,
      'lostReason': lostReason,
      'convertedAt': convertedAt?.toIso8601String(),
      'soldPrice': soldPrice,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
    };
  }

  factory LeadModel.fromEntity(Lead lead) {
    return LeadModel(
      id: lead.id,
      dealerId: lead.dealerId,
      listingId: lead.listingId,
      userId: lead.userId,
      userName: lead.userName,
      userEmail: lead.userEmail,
      userPhone: lead.userPhone,
      userAvatar: lead.userAvatar,
      vehicleTitle: lead.vehicleTitle,
      vehicleImage: lead.vehicleImage,
      vehiclePrice: lead.vehiclePrice,
      status: lead.status,
      priority: lead.priority,
      source: lead.source,
      message: lead.message,
      notes: lead.notes,
      budget: lead.budget,
      needsFinancing: lead.needsFinancing,
      hasTradeIn: lead.hasTradeIn,
      tradeInDetails: lead.tradeInDetails,
      messageCount: lead.messageCount,
      callCount: lead.callCount,
      lastContactedAt: lead.lastContactedAt,
      nextFollowUpAt: lead.nextFollowUpAt,
      assignedTo: lead.assignedTo,
      offeredPrice: lead.offeredPrice,
      lostReason: lead.lostReason,
      convertedAt: lead.convertedAt,
      soldPrice: lead.soldPrice,
      createdAt: lead.createdAt,
      updatedAt: lead.updatedAt,
    );
  }
}

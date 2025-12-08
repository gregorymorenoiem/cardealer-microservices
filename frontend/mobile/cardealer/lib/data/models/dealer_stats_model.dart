import '../../domain/entities/dealer_stats.dart';

class DealerStatsModel extends DealerStats {
  const DealerStatsModel({
    required super.totalListings,
    required super.activeListings,
    required super.pendingListings,
    required super.soldListings,
    required super.totalViews,
    required super.totalLeads,
    required super.totalConversions,
    required super.conversionRate,
    required super.averageResponseTime,
    required super.revenue,
    required super.monthlyRevenue,
    required super.viewsByMonth,
    required super.leadsByMonth,
    required super.topPerformingVehicles,
  });

  factory DealerStatsModel.fromJson(Map<String, dynamic> json) {
    return DealerStatsModel(
      totalListings: json['totalListings'] as int,
      activeListings: json['activeListings'] as int,
      pendingListings: json['pendingListings'] as int,
      soldListings: json['soldListings'] as int,
      totalViews: json['totalViews'] as int,
      totalLeads: json['totalLeads'] as int,
      totalConversions: json['totalConversions'] as int,
      conversionRate: (json['conversionRate'] as num).toDouble(),
      averageResponseTime: (json['averageResponseTime'] as num).toDouble(),
      revenue: (json['revenue'] as num).toDouble(),
      monthlyRevenue: (json['monthlyRevenue'] as num).toDouble(),
      viewsByMonth: Map<String, int>.from(json['viewsByMonth'] as Map),
      leadsByMonth: Map<String, int>.from(json['leadsByMonth'] as Map),
      topPerformingVehicles: (json['topPerformingVehicles'] as List)
          .map((v) =>
              TopPerformingVehicleModel.fromJson(v as Map<String, dynamic>))
          .toList(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'totalListings': totalListings,
      'activeListings': activeListings,
      'pendingListings': pendingListings,
      'soldListings': soldListings,
      'totalViews': totalViews,
      'totalLeads': totalLeads,
      'totalConversions': totalConversions,
      'conversionRate': conversionRate,
      'averageResponseTime': averageResponseTime,
      'revenue': revenue,
      'monthlyRevenue': monthlyRevenue,
      'viewsByMonth': viewsByMonth,
      'leadsByMonth': leadsByMonth,
      'topPerformingVehicles': topPerformingVehicles
          .map((v) => TopPerformingVehicleModel.fromEntity(v).toJson())
          .toList(),
    };
  }

  factory DealerStatsModel.fromEntity(DealerStats stats) {
    return DealerStatsModel(
      totalListings: stats.totalListings,
      activeListings: stats.activeListings,
      pendingListings: stats.pendingListings,
      soldListings: stats.soldListings,
      totalViews: stats.totalViews,
      totalLeads: stats.totalLeads,
      totalConversions: stats.totalConversions,
      conversionRate: stats.conversionRate,
      averageResponseTime: stats.averageResponseTime,
      revenue: stats.revenue,
      monthlyRevenue: stats.monthlyRevenue,
      viewsByMonth: stats.viewsByMonth,
      leadsByMonth: stats.leadsByMonth,
      topPerformingVehicles: stats.topPerformingVehicles,
    );
  }
}

class TopPerformingVehicleModel extends TopPerformingVehicle {
  const TopPerformingVehicleModel({
    required super.vehicleId,
    required super.title,
    required super.imageUrl,
    required super.views,
    required super.leads,
    required super.conversionRate,
  });

  factory TopPerformingVehicleModel.fromJson(Map<String, dynamic> json) {
    return TopPerformingVehicleModel(
      vehicleId: json['vehicleId'] as String,
      title: json['title'] as String,
      imageUrl: json['imageUrl'] as String,
      views: json['views'] as int,
      leads: json['leads'] as int,
      conversionRate: (json['conversionRate'] as num).toDouble(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'vehicleId': vehicleId,
      'title': title,
      'imageUrl': imageUrl,
      'views': views,
      'leads': leads,
      'conversionRate': conversionRate,
    };
  }

  factory TopPerformingVehicleModel.fromEntity(TopPerformingVehicle vehicle) {
    return TopPerformingVehicleModel(
      vehicleId: vehicle.vehicleId,
      title: vehicle.title,
      imageUrl: vehicle.imageUrl,
      views: vehicle.views,
      leads: vehicle.leads,
      conversionRate: vehicle.conversionRate,
    );
  }
}

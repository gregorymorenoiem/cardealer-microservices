import 'package:equatable/equatable.dart';

/// Dealer statistics entity
class DealerStats extends Equatable {
  final int totalListings;
  final int activeListings;
  final int pendingListings;
  final int soldListings;
  final int totalViews;
  final int totalLeads;
  final int totalConversions;
  final double conversionRate;
  final double averageResponseTime; // in hours
  final double revenue; // total revenue
  final double monthlyRevenue;
  final Map<String, int> viewsByMonth; // Month -> views
  final Map<String, int> leadsByMonth; // Month -> leads
  final List<TopPerformingVehicle> topPerformingVehicles;

  const DealerStats({
    required this.totalListings,
    required this.activeListings,
    required this.pendingListings,
    required this.soldListings,
    required this.totalViews,
    required this.totalLeads,
    required this.totalConversions,
    required this.conversionRate,
    required this.averageResponseTime,
    required this.revenue,
    required this.monthlyRevenue,
    required this.viewsByMonth,
    required this.leadsByMonth,
    required this.topPerformingVehicles,
  });

  @override
  List<Object?> get props => [
        totalListings,
        activeListings,
        pendingListings,
        soldListings,
        totalViews,
        totalLeads,
        totalConversions,
        conversionRate,
        averageResponseTime,
        revenue,
        monthlyRevenue,
        viewsByMonth,
        leadsByMonth,
        topPerformingVehicles,
      ];
}

/// Top performing vehicle stats
class TopPerformingVehicle extends Equatable {
  final String vehicleId;
  final String title;
  final String imageUrl;
  final int views;
  final int leads;
  final double conversionRate;

  const TopPerformingVehicle({
    required this.vehicleId,
    required this.title,
    required this.imageUrl,
    required this.views,
    required this.leads,
    required this.conversionRate,
  });

  @override
  List<Object?> get props => [
        vehicleId,
        title,
        imageUrl,
        views,
        leads,
        conversionRate,
      ];
}

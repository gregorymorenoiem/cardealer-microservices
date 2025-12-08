import 'package:dartz/dartz.dart';
import '../entities/dealer_listing.dart';
import '../entities/dealer_stats.dart';
import '../entities/lead.dart';
import '../../core/error/failures.dart';

/// Repository interface for dealer operations
abstract class DealerRepository {
  // Stats
  Future<Either<Failure, DealerStats>> getDealerStats(String dealerId);
  
  // Listings management
  Future<Either<Failure, List<DealerListing>>> getListings({
    required String dealerId,
    ListingStatus? status,
    String? searchQuery,
    int? limit,
    int? offset,
  });
  
  Future<Either<Failure, DealerListing>> getListingById(String listingId);
  
  Future<Either<Failure, DealerListing>> createListing(DealerListing listing);
  
  Future<Either<Failure, DealerListing>> updateListing(DealerListing listing);
  
  Future<Either<Failure, void>> deleteListing(String listingId);
  
  Future<Either<Failure, void>> bulkUpdateListingStatus({
    required List<String> listingIds,
    required ListingStatus status,
  });
  
  Future<Either<Failure, DealerListing>> publishListing(String listingId);
  
  Future<Either<Failure, DealerListing>> markListingAsSold({
    required String listingId,
    required double soldPrice,
  });
  
  // Leads management
  Future<Either<Failure, List<Lead>>> getLeads({
    required String dealerId,
    LeadStatus? status,
    LeadPriority? priority,
    String? searchQuery,
    int? limit,
    int? offset,
  });
  
  Future<Either<Failure, Lead>> getLeadById(String leadId);
  
  Future<Either<Failure, Lead>> updateLeadStatus({
    required String leadId,
    required LeadStatus status,
  });
  
  Future<Either<Failure, Lead>> updateLeadPriority({
    required String leadId,
    required LeadPriority priority,
  });
  
  Future<Either<Failure, Lead>> updateLeadNotes({
    required String leadId,
    required String notes,
  });
  
  Future<Either<Failure, Lead>> scheduleFollowUp({
    required String leadId,
    required DateTime followUpDate,
  });
  
  Future<Either<Failure, Lead>> convertLead({
    required String leadId,
    required double soldPrice,
  });
  
  Future<Either<Failure, Lead>> markLeadAsLost({
    required String leadId,
    required String reason,
  });
  
  // Analytics
  Future<Either<Failure, Map<String, int>>> getViewsByMonth({
    required String dealerId,
    required int year,
  });
  
  Future<Either<Failure, Map<String, int>>> getLeadsByMonth({
    required String dealerId,
    required int year,
  });
  
  Future<Either<Failure, List<DealerListing>>> getTopPerformingListings({
    required String dealerId,
    int limit = 10,
  });
}

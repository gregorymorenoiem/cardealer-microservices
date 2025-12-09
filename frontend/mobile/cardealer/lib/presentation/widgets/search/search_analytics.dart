import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'dart:convert';

/// SE-011: Search Analytics
/// Track and display trending searches and analytics
class SearchAnalytics {
  static const String _searchHistoryKey = 'search_history';
  static const String _popularSearchesKey = 'popular_searches';
  static const String _zeroResultsKey = 'zero_results_searches';
  static const String _filterUsageKey = 'filter_usage';

  // Singleton pattern
  static final SearchAnalytics _instance = SearchAnalytics._internal();
  factory SearchAnalytics() => _instance;
  SearchAnalytics._internal();

  SharedPreferences? _prefs;

  Future<void> initialize() async {
    _prefs = await SharedPreferences.getInstance();
  }

  /// Track a search query
  Future<void> trackSearch(String query) async {
    if (_prefs == null) await initialize();

    final searches = await getSearchHistory();
    final timestamp = DateTime.now().toIso8601String();

    searches.add({
      'query': query,
      'timestamp': timestamp,
    });

    // Keep only last 100 searches
    if (searches.length > 100) {
      searches.removeRange(0, searches.length - 100);
    }

    await _prefs!.setString(_searchHistoryKey, jsonEncode(searches));
    await _incrementPopularSearch(query);
  }

  /// Track zero results searches
  Future<void> trackZeroResults(String query) async {
    if (_prefs == null) await initialize();

    final zeroResults = await getZeroResultsSearches();
    zeroResults.add({
      'query': query,
      'timestamp': DateTime.now().toIso8601String(),
    });

    // Keep only last 50
    if (zeroResults.length > 50) {
      zeroResults.removeRange(0, zeroResults.length - 50);
    }

    await _prefs!.setString(_zeroResultsKey, jsonEncode(zeroResults));
  }

  /// Track filter usage
  Future<void> trackFilterUsage(String filterType) async {
    if (_prefs == null) await initialize();

    final usage = await getFilterUsage();
    usage[filterType] = (usage[filterType] ?? 0) + 1;

    await _prefs!.setString(_filterUsageKey, jsonEncode(usage));
  }

  /// Get search history
  Future<List<Map<String, dynamic>>> getSearchHistory() async {
    if (_prefs == null) await initialize();

    final String? historyJson = _prefs!.getString(_searchHistoryKey);
    if (historyJson == null) return [];

    final List<dynamic> decoded = jsonDecode(historyJson);
    return decoded.cast<Map<String, dynamic>>();
  }

  /// Get popular searches (top 10)
  Future<List<PopularSearch>> getPopularSearches() async {
    if (_prefs == null) await initialize();

    final String? popularJson = _prefs!.getString(_popularSearchesKey);
    if (popularJson == null) return [];

    final Map<String, dynamic> decoded = jsonDecode(popularJson);
    final List<PopularSearch> popular = [];

    decoded.forEach((query, count) {
      popular.add(PopularSearch(query: query, count: count as int));
    });

    // Sort by count descending
    popular.sort((a, b) => b.count.compareTo(a.count));

    return popular.take(10).toList();
  }

  /// Get trending searches (searches in last 24 hours)
  Future<List<String>> getTrendingSearches() async {
    final searches = await getSearchHistory();
    final yesterday = DateTime.now().subtract(const Duration(hours: 24));

    final recent = searches
        .where((search) {
          final timestamp = DateTime.parse(search['timestamp'] as String);
          return timestamp.isAfter(yesterday);
        })
        .map((s) => s['query'] as String)
        .toList();

    // Count occurrences
    final Map<String, int> counts = {};
    for (final query in recent) {
      counts[query] = (counts[query] ?? 0) + 1;
    }

    // Sort by count and return top 5
    final sorted = counts.entries.toList()
      ..sort((a, b) => b.value.compareTo(a.value));

    return sorted.take(5).map((e) => e.key).toList();
  }

  /// Get zero results searches
  Future<List<Map<String, dynamic>>> getZeroResultsSearches() async {
    if (_prefs == null) await initialize();

    final String? zeroJson = _prefs!.getString(_zeroResultsKey);
    if (zeroJson == null) return [];

    final List<dynamic> decoded = jsonDecode(zeroJson);
    return decoded.cast<Map<String, dynamic>>();
  }

  /// Get filter usage statistics
  Future<Map<String, int>> getFilterUsage() async {
    if (_prefs == null) await initialize();

    final String? usageJson = _prefs!.getString(_filterUsageKey);
    if (usageJson == null) return {};

    final Map<String, dynamic> decoded = jsonDecode(usageJson);
    return decoded.map((key, value) => MapEntry(key, value as int));
  }

  /// Get most used filters (top 5)
  Future<List<FilterUsage>> getMostUsedFilters() async {
    final usage = await getFilterUsage();
    final List<FilterUsage> filters = [];

    usage.forEach((filterType, count) {
      filters.add(FilterUsage(filterType: filterType, count: count));
    });

    filters.sort((a, b) => b.count.compareTo(a.count));
    return filters.take(5).toList();
  }

  /// Clear all analytics data
  Future<void> clearAnalytics() async {
    if (_prefs == null) await initialize();

    await _prefs!.remove(_searchHistoryKey);
    await _prefs!.remove(_popularSearchesKey);
    await _prefs!.remove(_zeroResultsKey);
    await _prefs!.remove(_filterUsageKey);
  }

  // Private helper to increment popular search count
  Future<void> _incrementPopularSearch(String query) async {
    final String? popularJson = _prefs!.getString(_popularSearchesKey);
    Map<String, dynamic> popular = {};

    if (popularJson != null) {
      popular = jsonDecode(popularJson);
    }

    popular[query] = (popular[query] ?? 0) + 1;
    await _prefs!.setString(_popularSearchesKey, jsonEncode(popular));
  }
}

/// Data class for popular search
class PopularSearch {
  final String query;
  final int count;

  PopularSearch({
    required this.query,
    required this.count,
  });
}

/// Data class for filter usage
class FilterUsage {
  final String filterType;
  final int count;

  FilterUsage({
    required this.filterType,
    required this.count,
  });
}

/// Widget to display trending searches
class TrendingSearchesWidget extends StatelessWidget {
  final Function(String) onSearchTap;

  const TrendingSearchesWidget({
    super.key,
    required this.onSearchTap,
  });

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<String>>(
      future: SearchAnalytics().getTrendingSearches(),
      builder: (context, snapshot) {
        if (!snapshot.hasData || snapshot.data!.isEmpty) {
          return const SizedBox.shrink();
        }

        final trending = snapshot.data!;

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  Icon(
                    Icons.trending_up,
                    color: Theme.of(context).primaryColor,
                    size: 24,
                  ),
                  const SizedBox(width: 8),
                  const Text(
                    'Trending Now',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),
            SizedBox(
              height: 40,
              child: ListView.separated(
                scrollDirection: Axis.horizontal,
                padding: const EdgeInsets.symmetric(horizontal: 16),
                itemCount: trending.length,
                separatorBuilder: (context, index) => const SizedBox(width: 8),
                itemBuilder: (context, index) {
                  final query = trending[index];
                  return InkWell(
                    onTap: () => onSearchTap(query),
                    borderRadius: BorderRadius.circular(20),
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 8,
                      ),
                      decoration: BoxDecoration(
                        color: Theme.of(context)
                            .primaryColor
                            .withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(20),
                        border: Border.all(
                          color: Theme.of(context).primaryColor,
                          width: 1,
                        ),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            Icons.local_fire_department,
                            size: 16,
                            color: Theme.of(context).primaryColor,
                          ),
                          const SizedBox(width: 6),
                          Text(
                            query,
                            style: TextStyle(
                              fontSize: 14,
                              fontWeight: FontWeight.w600,
                              color: Theme.of(context).primaryColor,
                            ),
                          ),
                        ],
                      ),
                    ),
                  );
                },
              ),
            ),
            const SizedBox(height: 16),
          ],
        );
      },
    );
  }
}

import 'package:dartz/dartz.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../core/error/failures.dart';

/// Search history item
class SearchHistoryItem {
  final String query;
  final DateTime timestamp;

  SearchHistoryItem({
    required this.query,
    required this.timestamp,
  });

  Map<String, dynamic> toJson() => {
        'query': query,
        'timestamp': timestamp.toIso8601String(),
      };

  factory SearchHistoryItem.fromJson(Map<String, dynamic> json) {
    return SearchHistoryItem(
      query: json['query'] as String,
      timestamp: DateTime.parse(json['timestamp'] as String),
    );
  }
}

/// Use case to retrieve search history
class GetSearchHistory {
  final SharedPreferences sharedPreferences;

  static const String _historyKey = 'search_history';
  static const int _maxHistoryItems = 10;

  GetSearchHistory(this.sharedPreferences);

  /// Get search history, sorted by most recent first
  Future<Either<Failure, List<SearchHistoryItem>>> call() async {
    try {
      final historyJson = sharedPreferences.getStringList(_historyKey) ?? [];

      final history = historyJson.map((json) {
        final map = _parseJson(json);
        return SearchHistoryItem.fromJson(map);
      }).toList();

      // Sort by most recent first
      history.sort((a, b) => b.timestamp.compareTo(a.timestamp));

      return Right(history);
    } catch (e) {
      return Left(CacheFailure(
          message: 'Failed to load search history: ${e.toString()}'));
    }
  }

  /// Add a search query to history
  Future<Either<Failure, bool>> addToHistory(String query) async {
    try {
      if (query.trim().isEmpty) {
        return const Left(
            ValidationFailure(message: 'Search query cannot be empty'));
      }

      final historyJson = sharedPreferences.getStringList(_historyKey) ?? [];

      // Parse existing history
      final history = historyJson.map((json) {
        final map = _parseJson(json);
        return SearchHistoryItem.fromJson(map);
      }).toList();

      // Remove duplicate if exists
      history.removeWhere(
          (item) => item.query.toLowerCase() == query.toLowerCase());

      // Add new item at the beginning
      history.insert(
          0,
          SearchHistoryItem(
            query: query,
            timestamp: DateTime.now(),
          ));

      // Keep only last N items
      final trimmedHistory = history.take(_maxHistoryItems).toList();

      // Save back to SharedPreferences
      final jsonList =
          trimmedHistory.map((item) => _stringifyJson(item.toJson())).toList();

      final success =
          await sharedPreferences.setStringList(_historyKey, jsonList);

      return Right(success);
    } catch (e) {
      return Left(CacheFailure(
          message: 'Failed to add to search history: ${e.toString()}'));
    }
  }

  /// Clear all search history
  Future<Either<Failure, bool>> clearHistory() async {
    try {
      final success = await sharedPreferences.remove(_historyKey);
      return Right(success);
    } catch (e) {
      return Left(CacheFailure(
          message: 'Failed to clear search history: ${e.toString()}'));
    }
  }

  Map<String, dynamic> _parseJson(String json) {
    // Simple JSON parser for our use case
    final cleaned = json.substring(1, json.length - 1); // Remove {}
    final parts = cleaned.split(', ');
    final map = <String, dynamic>{};

    for (final part in parts) {
      final kv = part.split(': ');
      if (kv.length == 2) {
        final key = kv[0];
        final value = kv[1].replaceAll('"', '');
        map[key] = value;
      }
    }

    return map;
  }

  String _stringifyJson(Map<String, dynamic> map) {
    final entries = map.entries.map((e) => '${e.key}: "${e.value}"').join(', ');
    return '{$entries}';
  }
}

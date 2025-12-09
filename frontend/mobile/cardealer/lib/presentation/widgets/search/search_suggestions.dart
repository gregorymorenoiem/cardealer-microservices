import 'package:flutter/material.dart';

/// SE-003: Search Suggestions
/// Real-time suggestions with categories and popular searches
class SearchSuggestions extends StatelessWidget {
  final String query;
  final Function(String) onSuggestionTap;

  const SearchSuggestions({
    super.key,
    required this.query,
    required this.onSuggestionTap,
  });

  // Popular searches - could come from SE-011: Search Analytics
  static const List<Map<String, dynamic>> _popularSearches = [
    {'text': 'Toyota Camry', 'icon': Icons.trending_up, 'category': 'Popular'},
    {'text': 'Honda Civic', 'icon': Icons.trending_up, 'category': 'Popular'},
    {'text': 'Ford F-150', 'icon': Icons.trending_up, 'category': 'Popular'},
    {'text': 'Tesla Model 3', 'icon': Icons.trending_up, 'category': 'Popular'},
    {'text': 'SUV', 'icon': Icons.directions_car, 'category': 'Categoría'},
    {'text': 'Sedán', 'icon': Icons.directions_car, 'category': 'Categoría'},
    {'text': 'Pickup', 'icon': Icons.local_shipping, 'category': 'Categoría'},
    {'text': 'Eléctrico', 'icon': Icons.electric_car, 'category': 'Categoría'},
  ];

  List<Map<String, dynamic>> _getFilteredSuggestions() {
    if (query.isEmpty) {
      return _popularSearches;
    }

    return _popularSearches
        .where((item) =>
            item['text'].toString().toLowerCase().contains(query.toLowerCase()))
        .toList();
  }

  @override
  Widget build(BuildContext context) {
    final suggestions = _getFilteredSuggestions();

    if (suggestions.isEmpty) {
      return const SizedBox.shrink();
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(20, 24, 20, 12),
          child: Text(
            query.isEmpty ? 'Búsquedas Populares' : 'Sugerencias',
            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
          ),
        ),
        ListView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: suggestions.length,
          itemBuilder: (context, index) {
            final suggestion = suggestions[index];
            return _SuggestionTile(
              text: suggestion['text'] as String,
              icon: suggestion['icon'] as IconData,
              category: suggestion['category'] as String,
              query: query,
              onTap: () => onSuggestionTap(suggestion['text'] as String),
            );
          },
        ),
      ],
    );
  }
}

class _SuggestionTile extends StatelessWidget {
  final String text;
  final IconData icon;
  final String category;
  final String query;
  final VoidCallback onTap;

  const _SuggestionTile({
    required this.text,
    required this.icon,
    required this.category,
    required this.query,
    required this.onTap,
  });

  TextSpan _buildHighlightedText(BuildContext context) {
    if (query.isEmpty) {
      return TextSpan(
        text: text,
        style: const TextStyle(
          fontSize: 15,
          color: Colors.black87,
        ),
      );
    }

    final lowerText = text.toLowerCase();
    final lowerQuery = query.toLowerCase();
    final index = lowerText.indexOf(lowerQuery);

    if (index == -1) {
      return TextSpan(
        text: text,
        style: const TextStyle(
          fontSize: 15,
          color: Colors.black87,
        ),
      );
    }

    return TextSpan(
      children: [
        if (index > 0)
          TextSpan(
            text: text.substring(0, index),
            style: const TextStyle(
              fontSize: 15,
              color: Colors.black87,
            ),
          ),
        TextSpan(
          text: text.substring(index, index + query.length),
          style: TextStyle(
            fontSize: 15,
            color: Theme.of(context).primaryColor,
            fontWeight: FontWeight.bold,
          ),
        ),
        if (index + query.length < text.length)
          TextSpan(
            text: text.substring(index + query.length),
            style: const TextStyle(
              fontSize: 15,
              color: Colors.black87,
            ),
          ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
        child: Row(
          children: [
            // Icon
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: Colors.grey.shade100,
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(
                icon,
                color: Colors.grey.shade700,
                size: 20,
              ),
            ),
            const SizedBox(width: 16),

            // Text with highlight
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  RichText(
                    text: _buildHighlightedText(context),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    category,
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey.shade600,
                    ),
                  ),
                ],
              ),
            ),

            // Arrow icon
            Icon(
              Icons.north_west,
              size: 18,
              color: Colors.grey.shade400,
            ),
          ],
        ),
      ),
    );
  }
}

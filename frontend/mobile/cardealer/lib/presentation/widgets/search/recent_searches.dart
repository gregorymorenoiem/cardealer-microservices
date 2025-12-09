import 'package:flutter/material.dart';

/// Recent Searches Widget
/// Shows list of recent searches with remove and clear all options
class RecentSearches extends StatelessWidget {
  final List<String> searches;
  final Function(String) onSearchTap;
  final Function(String) onRemove;
  final VoidCallback onClearAll;

  const RecentSearches({
    super.key,
    required this.searches,
    required this.onSearchTap,
    required this.onRemove,
    required this.onClearAll,
  });

  @override
  Widget build(BuildContext context) {
    if (searches.isEmpty) {
      return const SizedBox.shrink();
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Header
        Padding(
          padding: const EdgeInsets.fromLTRB(20, 24, 20, 12),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Recent Searches',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
              ),
              TextButton(
                onPressed: onClearAll,
                child: const Text('Clear All'),
              ),
            ],
          ),
        ),

        // Recent searches list
        ListView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: searches.length,
          itemBuilder: (context, index) {
            final search = searches[index];
            return _RecentSearchTile(
              text: search,
              onTap: () => onSearchTap(search),
              onRemove: () => onRemove(search),
            );
          },
        ),
      ],
    );
  }
}

class _RecentSearchTile extends StatelessWidget {
  final String text;
  final VoidCallback onTap;
  final VoidCallback onRemove;

  const _RecentSearchTile({
    required this.text,
    required this.onTap,
    required this.onRemove,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
        child: Row(
          children: [
            // History icon
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: Colors.grey.shade100,
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(
                Icons.history,
                color: Colors.grey.shade700,
                size: 20,
              ),
            ),
            const SizedBox(width: 16),

            // Search text
            Expanded(
              child: Text(
                text,
                style: const TextStyle(
                  fontSize: 15,
                  color: Colors.black87,
                ),
              ),
            ),

            // Remove button
            IconButton(
              onPressed: onRemove,
              icon: Icon(
                Icons.close,
                size: 20,
                color: Colors.grey.shade600,
              ),
              tooltip: 'Remove',
            ),
          ],
        ),
      ),
    );
  }
}

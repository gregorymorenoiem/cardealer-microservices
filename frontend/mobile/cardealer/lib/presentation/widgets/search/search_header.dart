import 'package:flutter/material.dart';
import 'dart:async';
import 'voice_search_button.dart';

/// SE-001: Search Page Header
/// Premium search bar with clear, cancel, and voice search buttons
class SearchHeader extends StatefulWidget {
  final TextEditingController controller;
  final FocusNode focusNode;
  final Function(String) onSearch;
  final VoidCallback onClear;
  final VoidCallback onBack;
  final VoidCallback? onFilterTap;
  final Function(String)? onVoiceSearch;

  const SearchHeader({
    super.key,
    required this.controller,
    required this.focusNode,
    required this.onSearch,
    required this.onClear,
    required this.onBack,
    this.onFilterTap,
    this.onVoiceSearch,
  });

  @override
  State<SearchHeader> createState() => _SearchHeaderState();
}

class _SearchHeaderState extends State<SearchHeader>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  Timer? _debounce;
  bool _showClear = false;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 300),
      vsync: this,
    );

    _fadeAnimation = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeOut,
    ));

    widget.controller.addListener(_onTextChanged);
    _animationController.forward();
  }

  @override
  void dispose() {
    _animationController.dispose();
    _debounce?.cancel();
    widget.controller.removeListener(_onTextChanged);
    super.dispose();
  }

  void _onTextChanged() {
    setState(() {
      _showClear = widget.controller.text.isNotEmpty;
    });

    // Debounce search suggestions
    if (_debounce?.isActive ?? false) _debounce!.cancel();
    _debounce = Timer(const Duration(milliseconds: 500), () {
      // Trigger suggestions update if needed
    });
  }

  @override
  Widget build(BuildContext context) {
    return FadeTransition(
      opacity: _fadeAnimation,
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: Colors.white,
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.05),
              blurRadius: 4,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          children: [
            Row(
              children: [
                // Back button
                IconButton(
                  onPressed: widget.onBack,
                  icon: const Icon(Icons.arrow_back),
                  tooltip: 'Back',
                ),
                const SizedBox(width: 8),

                // Search field
                Expanded(
                  child: Container(
                    decoration: BoxDecoration(
                      color: Colors.grey.shade100,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(
                        color: widget.focusNode.hasFocus
                            ? Theme.of(context).primaryColor
                            : Colors.transparent,
                        width: 2,
                      ),
                    ),
                    child: TextField(
                      controller: widget.controller,
                      focusNode: widget.focusNode,
                      autofocus: true,
                      decoration: InputDecoration(
                        hintText: 'Search vehicles...',
                        prefixIcon: Icon(
                          Icons.search,
                          color: Colors.grey.shade600,
                        ),
                        suffixIcon: _showClear
                            ? Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  // Voice search button
                                  IconButton(
                                    onPressed: () {
                                      // TODO: SE-002 Voice search
                                    },
                                    icon: Icon(
                                      Icons.mic_outlined,
                                      color: Colors.grey.shade600,
                                    ),
                                    tooltip: 'Voice search',
                                  ),
                                  // Clear button
                                  IconButton(
                                    onPressed: () {
                                      widget.controller.clear();
                                      widget.onClear();
                                    },
                                    icon: Icon(
                                      Icons.close,
                                      color: Colors.grey.shade600,
                                    ),
                                    tooltip: 'Clear',
                                  ),
                                ],
                              )
                            : widget.onVoiceSearch != null
                                ? VoiceSearchButton(
                                    onVoiceResult: (text) {
                                      widget.controller.text = text;
                                      widget.onVoiceSearch!(text);
                                    },
                                    iconColor: Colors.grey.shade600,
                                  )
                                : IconButton(
                                    onPressed: () {},
                                    icon: Icon(
                                      Icons.mic_outlined,
                                      color: Colors.grey.shade600,
                                    ),
                                    tooltip: 'Voice search',
                                  ),
                        border: InputBorder.none,
                        contentPadding: const EdgeInsets.symmetric(
                          horizontal: 16,
                          vertical: 14,
                        ),
                      ),
                      textInputAction: TextInputAction.search,
                      onSubmitted: widget.onSearch,
                    ),
                  ),
                ),

                // Filter button
                if (widget.onFilterTap != null) ...[
                  const SizedBox(width: 8),
                  IconButton(
                    onPressed: widget.onFilterTap,
                    icon: const Icon(Icons.tune),
                    tooltip: 'Filters',
                  ),
                ],
              ],
            ),
          ],
        ),
      ),
    );
  }
}

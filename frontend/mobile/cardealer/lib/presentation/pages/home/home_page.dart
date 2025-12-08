import 'package:flutter/material.dart';

/// Placeholder home page
/// TODO: Implement full home page in Sprint 3
class HomePage extends StatelessWidget {
  const HomePage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('CarDealer'),
      ),
      body: const Center(
        child: Text('Home Page - Coming Soon'),
      ),
    );
  }
}

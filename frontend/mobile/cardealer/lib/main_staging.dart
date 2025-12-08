import 'package:flutter/material.dart';
import 'app_config.dart';
import 'main.dart' as app;

void main() {
  AppConfig.initialize(Flavor.staging);
  app.main();
}

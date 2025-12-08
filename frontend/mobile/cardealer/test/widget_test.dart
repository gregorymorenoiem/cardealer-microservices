import 'package:flutter_test/flutter_test.dart';
import 'package:cardealer_mobile/main.dart';

void main() {
  testWidgets('App should start without errors', (WidgetTester tester) async {
    // Build our app and trigger a frame.
    await tester.pumpWidget(const CarDealerApp());

    // Verify that the splash page is displayed
    expect(find.text('CarDealer'), findsOneWidget);
    expect(find.text('Tu marketplace de vehículos'), findsOneWidget);
  });
}

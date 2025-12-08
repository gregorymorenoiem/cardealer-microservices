import 'package:flutter/material.dart';
import 'package:widgetbook/widgetbook.dart';
import 'package:widgetbook_annotation/widgetbook_annotation.dart' as widgetbook;

// Import theme
import '../lib/core/theme/app_theme.dart';

// Import widgets
import '../lib/presentation/widgets/custom_button.dart';
import '../lib/presentation/widgets/custom_text_field.dart';
import '../lib/presentation/widgets/custom_app_bar.dart';
import '../lib/presentation/widgets/loading_indicator.dart';
import '../lib/presentation/widgets/empty_state_widget.dart';
import '../lib/presentation/widgets/custom_error_widget.dart';
import '../lib/presentation/widgets/vehicle_card.dart';
import '../lib/presentation/widgets/vehicle_card_horizontal.dart';
import '../lib/presentation/widgets/vehicle_card_grid.dart';
import '../lib/presentation/widgets/featured_badge.dart';
import '../lib/presentation/widgets/custom_bottom_nav_bar.dart';
import '../lib/presentation/widgets/custom_snackbar.dart';
import '../lib/presentation/widgets/custom_chip.dart';
import '../lib/presentation/widgets/custom_avatar.dart';
import '../lib/presentation/widgets/price_tag.dart';

void main() {
  runApp(const WidgetbookApp());
}

@widgetbook.App()
class WidgetbookApp extends StatelessWidget {
  const WidgetbookApp({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Widgetbook.material(
      directories: [
        // Theme
        WidgetbookFolder(
          name: 'Theme',
          children: [
            WidgetbookComponent(
              name: 'Colors',
              useCases: [
                WidgetbookUseCase(
                  name: 'Color Palette',
                  builder: (context) => const ColorPaletteWidget(),
                ),
              ],
            ),
          ],
        ),

        // Components
        WidgetbookFolder(
          name: 'Components',
          children: [
            // Buttons
            WidgetbookComponent(
              name: 'CustomButton',
              useCases: [
                WidgetbookUseCase(
                  name: 'Primary',
                  builder: (context) => CustomButton(
                    text: context.knobs
                        .string(label: 'Text', initialValue: 'Primary Button'),
                    onPressed: () {},
                    size: ButtonSize.medium,
                  ),
                ),
                WidgetbookUseCase(
                  name: 'Secondary',
                  builder: (context) => CustomButton(
                    text: context.knobs.string(
                        label: 'Text', initialValue: 'Secondary Button'),
                    onPressed: () {},
                    variant: ButtonVariant.secondary,
                  ),
                ),
                WidgetbookUseCase(
                  name: 'Loading',
                  builder: (context) => CustomButton(
                    text: 'Loading Button',
                    onPressed: () {},
                    isLoading: true,
                  ),
                ),
              ],
            ),

            // TextField
            WidgetbookComponent(
              name: 'CustomTextField',
              useCases: [
                WidgetbookUseCase(
                  name: 'Default',
                  builder: (context) => CustomTextField(
                    label: context.knobs
                        .string(label: 'Label', initialValue: 'Email'),
                    hint: 'example@email.com',
                  ),
                ),
                WidgetbookUseCase(
                  name: 'Password',
                  builder: (context) => const CustomTextField(
                    label: 'Password',
                    isPassword: true,
                  ),
                ),
              ],
            ),

            // Loading
            WidgetbookComponent(
              name: 'LoadingIndicator',
              useCases: [
                WidgetbookUseCase(
                  name: 'Circular',
                  builder: (context) => const LoadingIndicator(
                    type: LoadingType.circular,
                  ),
                ),
                WidgetbookUseCase(
                  name: 'Shimmer',
                  builder: (context) => const LoadingIndicator(
                    type: LoadingType.shimmer,
                  ),
                ),
              ],
            ),

            // Avatar
            WidgetbookComponent(
              name: 'CustomAvatar',
              useCases: [
                WidgetbookUseCase(
                  name: 'With Initials',
                  builder: (context) => CustomAvatar(
                    name: context.knobs
                        .string(label: 'Name', initialValue: 'John Doe'),
                    size: AvatarSize.lg,
                  ),
                ),
                WidgetbookUseCase(
                  name: 'With Badge',
                  builder: (context) => const CustomAvatar(
                    name: 'Jane Smith',
                    size: AvatarSize.lg,
                    showBadge: true,
                  ),
                ),
              ],
            ),

            // Chips
            WidgetbookComponent(
              name: 'CustomChip',
              useCases: [
                WidgetbookUseCase(
                  name: 'Filled',
                  builder: (context) => CustomChip(
                    label: context.knobs
                        .string(label: 'Label', initialValue: 'Featured'),
                    variant: ChipVariant.filled,
                  ),
                ),
                WidgetbookUseCase(
                  name: 'Outlined',
                  builder: (context) => const CustomChip(
                    label: 'New',
                    variant: ChipVariant.outlined,
                  ),
                ),
              ],
            ),

            // Price Tags
            WidgetbookComponent(
              name: 'PriceTag',
              useCases: [
                WidgetbookUseCase(
                  name: 'Simple',
                  builder: (context) => PriceTag(
                    price: context.knobs.double
                        .input(label: 'Price', initialValue: 25000),
                    size: PriceSize.large,
                  ),
                ),
                WidgetbookUseCase(
                  name: 'With Discount',
                  builder: (context) => const PriceTag(
                    price: 20000,
                    originalPrice: 25000,
                    size: PriceSize.large,
                  ),
                ),
              ],
            ),

            // Vehicle Cards
            WidgetbookComponent(
              name: 'VehicleCard',
              useCases: [
                WidgetbookUseCase(
                  name: 'List Card',
                  builder: (context) => VehicleCard(
                    imageUrl: 'https://via.placeholder.com/400x300',
                    make: 'Toyota',
                    model: 'Corolla',
                    year: 2023,
                    price: 25000,
                    mileage: 15000,
                    location: 'Ciudad de México',
                    isFeatured: context.knobs
                        .boolean(label: 'Featured', initialValue: true),
                    onTap: () {},
                  ),
                ),
                WidgetbookUseCase(
                  name: 'Horizontal Card',
                  builder: (context) => VehicleCardHorizontal(
                    imageUrl: 'https://via.placeholder.com/400x300',
                    make: 'Honda',
                    model: 'Civic',
                    year: 2023,
                    price: 28000,
                    isFeatured: true,
                    onTap: () {},
                  ),
                ),
                WidgetbookUseCase(
                  name: 'Grid Card',
                  builder: (context) => VehicleCardGrid(
                    imageUrl: 'https://via.placeholder.com/400x300',
                    make: 'Mazda',
                    model: 'CX-5',
                    year: 2023,
                    price: 32000,
                    isFeatured: false,
                    onTap: () {},
                  ),
                ),
              ],
            ),

            // Empty State
            WidgetbookComponent(
              name: 'EmptyStateWidget',
              useCases: [
                WidgetbookUseCase(
                  name: 'With Action',
                  builder: (context) => EmptyStateWidget(
                    icon: Icons.search_off,
                    title: 'No results found',
                    message: 'Try adjusting your search filters',
                    actionLabel: 'Clear Filters',
                    onAction: () {},
                  ),
                ),
              ],
            ),
          ],
        ),
      ],
      appInfo: AppInfo(name: 'CarDealer Components'),
      themes: [
        WidgetbookTheme(
          name: 'Light',
          data: AppTheme.lightTheme,
        ),
        WidgetbookTheme(
          name: 'Dark',
          data: AppTheme.darkTheme,
        ),
      ],
    );
  }
}

class ColorPaletteWidget extends StatelessWidget {
  const ColorPaletteWidget({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(
        child: Text('Color Palette - TODO: Implement color swatches'),
      ),
    );
  }
}

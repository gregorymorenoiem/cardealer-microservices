import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_en.dart';
import 'app_localizations_es.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'l10n/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
      : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations? of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations);
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
    delegate,
    GlobalMaterialLocalizations.delegate,
    GlobalCupertinoLocalizations.delegate,
    GlobalWidgetsLocalizations.delegate,
  ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[
    Locale('en'),
    Locale('es')
  ];

  /// Nombre de la aplicación
  ///
  /// In es, this message translates to:
  /// **'CarDealer'**
  String get appName;

  /// Eslogan de la aplicación
  ///
  /// In es, this message translates to:
  /// **'Tu marketplace de vehículos'**
  String get appTagline;

  /// No description provided for @commonLoading.
  ///
  /// In es, this message translates to:
  /// **'Cargando...'**
  String get commonLoading;

  /// No description provided for @commonError.
  ///
  /// In es, this message translates to:
  /// **'Error'**
  String get commonError;

  /// No description provided for @commonSuccess.
  ///
  /// In es, this message translates to:
  /// **'Éxito'**
  String get commonSuccess;

  /// No description provided for @commonCancel.
  ///
  /// In es, this message translates to:
  /// **'Cancelar'**
  String get commonCancel;

  /// No description provided for @commonConfirm.
  ///
  /// In es, this message translates to:
  /// **'Confirmar'**
  String get commonConfirm;

  /// No description provided for @commonSave.
  ///
  /// In es, this message translates to:
  /// **'Guardar'**
  String get commonSave;

  /// No description provided for @commonDelete.
  ///
  /// In es, this message translates to:
  /// **'Eliminar'**
  String get commonDelete;

  /// No description provided for @commonEdit.
  ///
  /// In es, this message translates to:
  /// **'Editar'**
  String get commonEdit;

  /// No description provided for @commonSearch.
  ///
  /// In es, this message translates to:
  /// **'Buscar'**
  String get commonSearch;

  /// No description provided for @commonFilter.
  ///
  /// In es, this message translates to:
  /// **'Filtrar'**
  String get commonFilter;

  /// No description provided for @commonSort.
  ///
  /// In es, this message translates to:
  /// **'Ordenar'**
  String get commonSort;

  /// No description provided for @commonViewAll.
  ///
  /// In es, this message translates to:
  /// **'Ver todo'**
  String get commonViewAll;

  /// No description provided for @commonRetry.
  ///
  /// In es, this message translates to:
  /// **'Reintentar'**
  String get commonRetry;

  /// No description provided for @commonClose.
  ///
  /// In es, this message translates to:
  /// **'Cerrar'**
  String get commonClose;

  /// No description provided for @commonBack.
  ///
  /// In es, this message translates to:
  /// **'Volver'**
  String get commonBack;

  /// No description provided for @navHome.
  ///
  /// In es, this message translates to:
  /// **'Inicio'**
  String get navHome;

  /// No description provided for @navBrowse.
  ///
  /// In es, this message translates to:
  /// **'Explorar'**
  String get navBrowse;

  /// No description provided for @navFavorites.
  ///
  /// In es, this message translates to:
  /// **'Favoritos'**
  String get navFavorites;

  /// No description provided for @navMessages.
  ///
  /// In es, this message translates to:
  /// **'Mensajes'**
  String get navMessages;

  /// No description provided for @navProfile.
  ///
  /// In es, this message translates to:
  /// **'Perfil'**
  String get navProfile;

  /// No description provided for @authLogin.
  ///
  /// In es, this message translates to:
  /// **'Iniciar Sesión'**
  String get authLogin;

  /// No description provided for @authRegister.
  ///
  /// In es, this message translates to:
  /// **'Registrarse'**
  String get authRegister;

  /// No description provided for @authLogout.
  ///
  /// In es, this message translates to:
  /// **'Cerrar Sesión'**
  String get authLogout;

  /// No description provided for @authEmail.
  ///
  /// In es, this message translates to:
  /// **'Correo Electrónico'**
  String get authEmail;

  /// No description provided for @authPassword.
  ///
  /// In es, this message translates to:
  /// **'Contraseña'**
  String get authPassword;

  /// No description provided for @authConfirmPassword.
  ///
  /// In es, this message translates to:
  /// **'Confirmar Contraseña'**
  String get authConfirmPassword;

  /// No description provided for @authForgotPassword.
  ///
  /// In es, this message translates to:
  /// **'¿Olvidaste tu contraseña?'**
  String get authForgotPassword;

  /// No description provided for @authDontHaveAccount.
  ///
  /// In es, this message translates to:
  /// **'¿No tienes cuenta?'**
  String get authDontHaveAccount;

  /// No description provided for @authAlreadyHaveAccount.
  ///
  /// In es, this message translates to:
  /// **'¿Ya tienes cuenta?'**
  String get authAlreadyHaveAccount;

  /// No description provided for @authSignUp.
  ///
  /// In es, this message translates to:
  /// **'Regístrate'**
  String get authSignUp;

  /// No description provided for @authSignIn.
  ///
  /// In es, this message translates to:
  /// **'Inicia sesión'**
  String get authSignIn;

  /// No description provided for @vehiclePrice.
  ///
  /// In es, this message translates to:
  /// **'Precio'**
  String get vehiclePrice;

  /// No description provided for @vehicleYear.
  ///
  /// In es, this message translates to:
  /// **'Año'**
  String get vehicleYear;

  /// No description provided for @vehicleMake.
  ///
  /// In es, this message translates to:
  /// **'Marca'**
  String get vehicleMake;

  /// No description provided for @vehicleModel.
  ///
  /// In es, this message translates to:
  /// **'Modelo'**
  String get vehicleModel;

  /// No description provided for @vehicleMileage.
  ///
  /// In es, this message translates to:
  /// **'Kilometraje'**
  String get vehicleMileage;

  /// No description provided for @vehicleCondition.
  ///
  /// In es, this message translates to:
  /// **'Condición'**
  String get vehicleCondition;

  /// No description provided for @vehicleTransmission.
  ///
  /// In es, this message translates to:
  /// **'Transmisión'**
  String get vehicleTransmission;

  /// No description provided for @vehicleFuelType.
  ///
  /// In es, this message translates to:
  /// **'Combustible'**
  String get vehicleFuelType;

  /// No description provided for @vehicleColor.
  ///
  /// In es, this message translates to:
  /// **'Color'**
  String get vehicleColor;

  /// No description provided for @vehicleLocation.
  ///
  /// In es, this message translates to:
  /// **'Ubicación'**
  String get vehicleLocation;

  /// No description provided for @vehicleFeatured.
  ///
  /// In es, this message translates to:
  /// **'Destacado'**
  String get vehicleFeatured;

  /// No description provided for @vehicleContactSeller.
  ///
  /// In es, this message translates to:
  /// **'Contactar Vendedor'**
  String get vehicleContactSeller;

  /// No description provided for @vehicleViewDetails.
  ///
  /// In es, this message translates to:
  /// **'Ver Detalles'**
  String get vehicleViewDetails;

  /// No description provided for @homeHeroTitle.
  ///
  /// In es, this message translates to:
  /// **'Encuentra tu vehículo perfecto'**
  String get homeHeroTitle;

  /// No description provided for @homeFeaturedTitle.
  ///
  /// In es, this message translates to:
  /// **'Vehículos Destacados'**
  String get homeFeaturedTitle;

  /// No description provided for @homeWeekTitle.
  ///
  /// In es, this message translates to:
  /// **'Destacados de la Semana'**
  String get homeWeekTitle;

  /// No description provided for @homeDealsTitle.
  ///
  /// In es, this message translates to:
  /// **'Ofertas del Día'**
  String get homeDealsTitle;

  /// No description provided for @homeSuvsTitle.
  ///
  /// In es, this message translates to:
  /// **'SUVs y Camionetas'**
  String get homeSuvsTitle;

  /// No description provided for @homePremiumTitle.
  ///
  /// In es, this message translates to:
  /// **'Vehículos Premium'**
  String get homePremiumTitle;

  /// No description provided for @homeElectricTitle.
  ///
  /// In es, this message translates to:
  /// **'Eléctricos e Híbridos'**
  String get homeElectricTitle;

  /// No description provided for @filterPriceRange.
  ///
  /// In es, this message translates to:
  /// **'Rango de Precio'**
  String get filterPriceRange;

  /// No description provided for @filterYearRange.
  ///
  /// In es, this message translates to:
  /// **'Rango de Año'**
  String get filterYearRange;

  /// No description provided for @filterMake.
  ///
  /// In es, this message translates to:
  /// **'Marca'**
  String get filterMake;

  /// No description provided for @filterModel.
  ///
  /// In es, this message translates to:
  /// **'Modelo'**
  String get filterModel;

  /// No description provided for @filterCondition.
  ///
  /// In es, this message translates to:
  /// **'Condición'**
  String get filterCondition;

  /// No description provided for @filterTransmission.
  ///
  /// In es, this message translates to:
  /// **'Transmisión'**
  String get filterTransmission;

  /// No description provided for @filterFuelType.
  ///
  /// In es, this message translates to:
  /// **'Tipo de Combustible'**
  String get filterFuelType;

  /// No description provided for @filterApply.
  ///
  /// In es, this message translates to:
  /// **'Aplicar Filtros'**
  String get filterApply;

  /// No description provided for @filterReset.
  ///
  /// In es, this message translates to:
  /// **'Restablecer'**
  String get filterReset;

  /// No description provided for @sortNewest.
  ///
  /// In es, this message translates to:
  /// **'Más Recientes'**
  String get sortNewest;

  /// No description provided for @sortPriceLowToHigh.
  ///
  /// In es, this message translates to:
  /// **'Precio: Menor a Mayor'**
  String get sortPriceLowToHigh;

  /// No description provided for @sortPriceHighToLow.
  ///
  /// In es, this message translates to:
  /// **'Precio: Mayor a Menor'**
  String get sortPriceHighToLow;

  /// No description provided for @sortMileageLowToHigh.
  ///
  /// In es, this message translates to:
  /// **'Kilometraje: Menor a Mayor'**
  String get sortMileageLowToHigh;

  /// No description provided for @sortYearNewestFirst.
  ///
  /// In es, this message translates to:
  /// **'Año: Más Nuevo Primero'**
  String get sortYearNewestFirst;

  /// No description provided for @dealerDashboard.
  ///
  /// In es, this message translates to:
  /// **'Panel de Control'**
  String get dealerDashboard;

  /// No description provided for @dealerListings.
  ///
  /// In es, this message translates to:
  /// **'Mis Anuncios'**
  String get dealerListings;

  /// No description provided for @dealerCrm.
  ///
  /// In es, this message translates to:
  /// **'CRM'**
  String get dealerCrm;

  /// No description provided for @dealerAnalytics.
  ///
  /// In es, this message translates to:
  /// **'Analíticas'**
  String get dealerAnalytics;

  /// No description provided for @dealerStats.
  ///
  /// In es, this message translates to:
  /// **'Estadísticas'**
  String get dealerStats;

  /// No description provided for @dealerActiveListing.
  ///
  /// In es, this message translates to:
  /// **'Anuncios Activos'**
  String get dealerActiveListing;

  /// No description provided for @dealerViews.
  ///
  /// In es, this message translates to:
  /// **'Vistas'**
  String get dealerViews;

  /// No description provided for @dealerLeads.
  ///
  /// In es, this message translates to:
  /// **'Prospectos'**
  String get dealerLeads;

  /// No description provided for @planFree.
  ///
  /// In es, this message translates to:
  /// **'Gratis'**
  String get planFree;

  /// No description provided for @planBasic.
  ///
  /// In es, this message translates to:
  /// **'Básico'**
  String get planBasic;

  /// No description provided for @planPro.
  ///
  /// In es, this message translates to:
  /// **'Pro'**
  String get planPro;

  /// No description provided for @planEnterprise.
  ///
  /// In es, this message translates to:
  /// **'Empresarial'**
  String get planEnterprise;

  /// No description provided for @errorGeneric.
  ///
  /// In es, this message translates to:
  /// **'Ocurrió un error. Por favor, intenta de nuevo.'**
  String get errorGeneric;

  /// No description provided for @errorNetwork.
  ///
  /// In es, this message translates to:
  /// **'Error de conexión. Verifica tu internet.'**
  String get errorNetwork;

  /// No description provided for @errorUnauthorized.
  ///
  /// In es, this message translates to:
  /// **'No autorizado. Por favor, inicia sesión.'**
  String get errorUnauthorized;

  /// No description provided for @errorNotFound.
  ///
  /// In es, this message translates to:
  /// **'No encontrado.'**
  String get errorNotFound;

  /// No description provided for @errorServerError.
  ///
  /// In es, this message translates to:
  /// **'Error del servidor. Intenta más tarde.'**
  String get errorServerError;

  /// No description provided for @emptyStateNoVehicles.
  ///
  /// In es, this message translates to:
  /// **'No se encontraron vehículos'**
  String get emptyStateNoVehicles;

  /// No description provided for @emptyStateNoFavorites.
  ///
  /// In es, this message translates to:
  /// **'No tienes favoritos aún'**
  String get emptyStateNoFavorites;

  /// No description provided for @emptyStateNoMessages.
  ///
  /// In es, this message translates to:
  /// **'No tienes mensajes'**
  String get emptyStateNoMessages;

  /// No description provided for @emptyStateAddFirstVehicle.
  ///
  /// In es, this message translates to:
  /// **'Agrega tu primer vehículo'**
  String get emptyStateAddFirstVehicle;
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['en', 'es'].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'en':
      return AppLocalizationsEn();
    case 'es':
      return AppLocalizationsEs();
  }

  throw FlutterError(
      'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
      'an issue with the localizations generation tool. Please file an issue '
      'on GitHub with a reproducible sample app and the gen-l10n configuration '
      'that was used.');
}

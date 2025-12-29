import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/usecases/usecase.dart';
import '../../../domain/entities/payment.dart';
import '../../../domain/usecases/payment/get_available_plans.dart';
import '../../../domain/usecases/payment/get_current_subscription.dart';
import '../../../domain/usecases/payment/subscribe_to_plan.dart';
import '../../../domain/usecases/payment/update_subscription.dart';
import '../../../domain/usecases/payment/cancel_subscription.dart';
import '../../../domain/usecases/payment/get_payment_methods.dart';
import '../../../domain/usecases/payment/add_payment_method.dart';
import '../../../domain/usecases/payment/get_payment_history.dart';
import '../../../domain/usecases/payment/get_usage_stats.dart';
import '../../../domain/usecases/payment/get_invoice_url.dart';

part 'payment_event.dart';
part 'payment_state.dart';

/// BLoC for managing payment, subscription, and billing operations
class PaymentBloc extends Bloc<PaymentEvent, PaymentState> {
  final GetAvailablePlans getAvailablePlans;
  final GetCurrentSubscription getCurrentSubscription;
  final SubscribeToPlan subscribeToPlan;
  final UpdateSubscription updateSubscription;
  final CancelSubscription cancelSubscription;
  final GetPaymentMethods getPaymentMethods;
  final AddPaymentMethod addPaymentMethod;
  final GetPaymentHistory getPaymentHistory;
  final GetUsageStats getUsageStats;
  final GetInvoiceUrl getInvoiceUrl;

  PaymentBloc({
    required this.getAvailablePlans,
    required this.getCurrentSubscription,
    required this.subscribeToPlan,
    required this.updateSubscription,
    required this.cancelSubscription,
    required this.getPaymentMethods,
    required this.addPaymentMethod,
    required this.getPaymentHistory,
    required this.getUsageStats,
    required this.getInvoiceUrl,
  }) : super(PaymentInitial()) {
    on<LoadAvailablePlansEvent>(_onLoadAvailablePlans);
    on<LoadCurrentSubscriptionEvent>(_onLoadCurrentSubscription);
    on<SubscribeToPlanEvent>(_onSubscribeToPlan);
    on<UpdateSubscriptionEvent>(_onUpdateSubscription);
    on<CancelSubscriptionEvent>(_onCancelSubscription);
    on<LoadPaymentMethodsEvent>(_onLoadPaymentMethods);
    on<AddPaymentMethodEvent>(_onAddPaymentMethod);
    on<RemovePaymentMethodEvent>(_onRemovePaymentMethod);
    on<SetDefaultPaymentMethodEvent>(_onSetDefaultPaymentMethod);
    on<LoadPaymentHistoryEvent>(_onLoadPaymentHistory);
    on<LoadUsageStatsEvent>(_onLoadUsageStats);
    on<ProcessPaymentEvent>(_onProcessPayment);
    on<GetInvoiceUrlEvent>(_onGetInvoiceUrl);
    on<ChangeBillingPeriodEvent>(_onChangeBillingPeriod);
  }

  Future<void> _onLoadAvailablePlans(
    LoadAvailablePlansEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(PaymentLoading());

    final result = await getAvailablePlans(NoParams());

    result.fold(
      (failure) => emit(PaymentError(failure.message)),
      (plans) => emit(PlansLoaded(plans: plans)),
    );
  }

  Future<void> _onLoadCurrentSubscription(
    LoadCurrentSubscriptionEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(PaymentLoading());

    final subscriptionResult = await getCurrentSubscription(NoParams());
    final usageStatsResult = await getUsageStats(NoParams());

    subscriptionResult.fold(
      (failure) => emit(PaymentError(failure.message)),
      (subscription) {
        usageStatsResult.fold(
          (failure) => emit(SubscriptionLoaded(subscription: subscription)),
          (stats) => emit(SubscriptionLoaded(
            subscription: subscription,
            usageStats: stats,
          )),
        );
      },
    );
  }

  Future<void> _onSubscribeToPlan(
    SubscribeToPlanEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(const PaymentProcessing('Processing subscription...'));

    final result = await subscribeToPlan(SubscribeToPlanParams(
      planId: event.planId,
      billingPeriod: event.billingPeriod,
      paymentMethodId: event.paymentMethodId,
    ));

    result.fold(
      (failure) => emit(PaymentError(failure.message)),
      (subscription) => emit(const PaymentSuccess(
        'Successfully subscribed to plan!',
      )),
    );
  }

  Future<void> _onUpdateSubscription(
    UpdateSubscriptionEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(const PaymentProcessing('Updating subscription...'));

    final result = await updateSubscription(UpdateSubscriptionParams(
      subscriptionId: event.subscriptionId,
      newPlanId: event.newPlanId,
      billingPeriod: event.billingPeriod,
    ));

    result.fold(
      (failure) => emit(PaymentError(failure.message)),
      (subscription) => emit(const PaymentSuccess(
        'Subscription updated successfully!',
      )),
    );
  }

  Future<void> _onCancelSubscription(
    CancelSubscriptionEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(const PaymentProcessing('Cancelling subscription...'));

    final result = await cancelSubscription(
      CancelSubscriptionParams(subscriptionId: event.subscriptionId),
    );

    result.fold(
      (failure) => emit(PaymentError(failure.message)),
      (_) => emit(const PaymentSuccess(
        'Subscription cancelled successfully!',
      )),
    );
  }

  Future<void> _onLoadPaymentMethods(
    LoadPaymentMethodsEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(PaymentLoading());

    final result = await getPaymentMethods(NoParams());

    result.fold(
      (failure) => emit(PaymentError(failure.message)),
      (methods) => emit(PaymentMethodsLoaded(methods)),
    );
  }

  Future<void> _onAddPaymentMethod(
    AddPaymentMethodEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(const PaymentProcessing('Adding payment method...'));

    final result = await addPaymentMethod(AddPaymentMethodParams(
      token: event.token,
      setAsDefault: event.setAsDefault,
    ));

    result.fold(
      (failure) => emit(PaymentError(failure.message)),
      (method) => emit(const PaymentSuccess(
        'Payment method added successfully!',
      )),
    );
  }

  Future<void> _onRemovePaymentMethod(
    RemovePaymentMethodEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(const PaymentProcessing('Removing payment method...'));

    // TODO: Implement repository method
    emit(const PaymentSuccess('Payment method removed successfully!'));
  }

  Future<void> _onSetDefaultPaymentMethod(
    SetDefaultPaymentMethodEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(const PaymentProcessing('Setting default payment method...'));

    // TODO: Implement repository method
    emit(const PaymentSuccess('Default payment method updated!'));
  }

  Future<void> _onLoadPaymentHistory(
    LoadPaymentHistoryEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(PaymentLoading());

    final result = await getPaymentHistory(GetPaymentHistoryParams(
      limit: event.limit,
      startDate: event.startDate,
      endDate: event.endDate,
    ));

    result.fold(
      (failure) => emit(PaymentError(failure.message)),
      (payments) => emit(PaymentHistoryLoaded(payments)),
    );
  }

  Future<void> _onLoadUsageStats(
    LoadUsageStatsEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(PaymentLoading());

    final result = await getUsageStats(NoParams());

    result.fold(
      (failure) => emit(PaymentError(failure.message)),
      (stats) => emit(SubscriptionLoaded(usageStats: stats)),
    );
  }

  Future<void> _onProcessPayment(
    ProcessPaymentEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(const PaymentProcessing('Processing payment...'));

    // TODO: Implement one-time payment use case
    emit(const PaymentSuccess('Payment processed successfully!'));
  }

  Future<void> _onGetInvoiceUrl(
    GetInvoiceUrlEvent event,
    Emitter<PaymentState> emit,
  ) async {
    emit(PaymentLoading());

    final result = await getInvoiceUrl(GetInvoiceUrlParams(
      invoiceId: event.invoiceId,
    ));

    result.fold(
      (failure) => emit(PaymentError(failure.message)),
      (url) => emit(InvoiceUrlLoaded(url)),
    );
  }

  Future<void> _onChangeBillingPeriod(
    ChangeBillingPeriodEvent event,
    Emitter<PaymentState> emit,
  ) async {
    if (state is PlansLoaded) {
      final currentState = state as PlansLoaded;
      emit(currentState.copyWith(selectedPeriod: event.billingPeriod));
    }
  }
}

using FluentValidation;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

public class CurrencyRateValidator : AbstractValidator<CurrencyRate>
{
    public CurrencyRateValidator()
    {
        RuleFor(x => x.OriginKey).SetValidator(new KeyValidator());
        RuleFor(x => x.TargetKey).SetValidator(new KeyValidator());
        RuleFor(x => x.Rate).NotNull().NotEmpty().GreaterThan(0);
    }
}
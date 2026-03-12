using Autofac;
using FluentValidation;
using MediatR;
using WellAnalyzing.Application.Services;
using WellAnalyzing.Application.Services.Interfaces;
using WellAnalyzing.Application.Validators;
using WellAnalyzing.Domain.Entities;

namespace WellAnalyzing.Application;

/// <summary>
/// Модуль регистрации зависимостей прикладного уровня.
/// </summary>
public sealed class ApplicationModule : Module
{
    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterType<WellReportSummaryService>()
            .As<IWellReportSummaryService>()
            .SingleInstance();

        builder
            .RegisterType<WellAnalyzer>()
            .As<IWellAnalyzer>()
            .SingleInstance();

        builder
            .RegisterType<WellValidator>()
            .As<IValidator<Well>>();

        builder
            .RegisterAssemblyTypes(typeof(IMediator).Assembly)
            .AsImplementedInterfaces();

        builder
            .RegisterAssemblyTypes(typeof(ApplicationModule).Assembly)
            .AsClosedTypesOf(typeof(IRequestHandler<,>))
            .AsImplementedInterfaces();
    }
}

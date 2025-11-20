using ErrorOr;
using FluentValidation;
using Fora.Application.Applications;
using Fora.Application.AutoMapper;
using Fora.Application.Handlers.Base;
using Fora.Application.Interfaces;
using Fora.Application.Interfaces.Base;
using Fora.Application.Modules.Companies.Contracts;
using Fora.Application.Modules.Companies.Pipelines;
using Fora.Application.Modules.Login.Contracts;
using Fora.Application.Modules.Login.Pipelines;
using Fora.Domain.Interfaces.Infra.Data;
using Fora.Domain.Interfaces.Infra.ExternalCommunication;
using Fora.Domain.Interfaces.Services;
using Fora.Domain.Services;
using Fora.Infra.Data.Context;
using Fora.Infra.Data.Repository;
using Fora.Infra.Data.Repository.Base;
using Fora.Infra.Data.UnitOfWork;
using Fora.Infra.ExternalCommunication.EdgarApiClient;
using Fora.Infra.Security.Interfaces;
using Fora.Infra.Security.Web;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Fora.Infra.CrossCutting
{
    public class ForaContainer
    {
        public static ServiceProvider Register(IServiceCollection services)
        {

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<DomainToDomainMapper>();
                cfg.AddProfile<ModelToDomainMapper>();
                cfg.AddProfile<DomainToModelMapper>();
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IForaHandlerAssemblyRegistrator).Assembly));
            services.AddValidatorsFromAssemblyContaining<IForaHandlerAssemblyRegistrator>(includeInternalTypes: true, lifetime: ServiceLifetime.Singleton);

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidateRequestPipelineBehavior<,>));
            services.AddScoped<IPipelineBehavior<ForaLoginRequest, ErrorOr<ForaLoginResponse>>, ForaLoginPipelineBehavior>();
            services.AddScoped<IPipelineBehavior<GetCompaniesFundingRequest, ErrorOr<GetCompaniesFundingResponse>>, GetCompaniesFundingPipelineBehavior>();


            services.AddScoped(typeof(IImporterApplication), typeof(ImporterApplication));
            services.AddScoped(typeof(IImporterApplication), typeof(ImporterApplication));


            services.AddScoped(typeof(IImporterService), typeof(ImporterService));
            services.AddScoped(typeof(ICompanyService), typeof(CompanyService));
            services.AddScoped(typeof(IForaAuthenticationService), typeof(ForaAuthenticationService));


            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(ICompanyRepository), typeof(CompanyRepository));
            services.AddScoped(typeof(IWebUserRepository), typeof(WebUserRepository));

            services.AddScoped(typeof(ForaContext));

            services.AddScoped(typeof(IEdgarApiService), typeof(EdgarApiService));

            services.AddScoped(typeof(IPasswordHasher), typeof(PasswordHasher));
            services.AddScoped(typeof(IJwtTokenGenerator), typeof(JwtTokenGenerator));


            return services.BuildServiceProvider();
        }
    }
}

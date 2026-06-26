using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Application.Interfaces.Repositories.Notification;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Repositories.CodeGeneratorservice;
using ERP.Infrastructure.Repositories.common;
using ERP.Infrastructure.Repositories.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Infrastrucure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var dbType = configuration["DatabaseType"];
            var connString = configuration.GetConnectionString(dbType);

            if (string.IsNullOrEmpty(connString))
                throw new Exception("Connection string not found");

                switch (dbType)
                {
                    case "PostgreSQL_Development":
                        services.AddDbContext<AppDbContext>(options =>
                            options.UseNpgsql(connString));

                        services.AddDbContext<AuthDbContext>(options =>
                            options.UseNpgsql(connString));
                        break;
                    case "PostgreSQL_Production": // 👈 Add this line to catch the production label on Render
                        services.AddDbContext<AppDbContext>(options =>
                            options.UseNpgsql(connString));

                        services.AddDbContext<AuthDbContext>(options =>
                            options.UseNpgsql(connString));
                        break;

                    case "SqlServer":
                            services.AddDbContext<AppDbContext>(options =>
                                options.UseSqlServer(connString));
                            break;

                    case "Oracle":
                        services.AddDbContext<AppDbContext>(options =>
                            options.UseOracle(connString));
                        break;
                }

            // Register Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IQuotationService, QuotationService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<ISalesService, SalesService>();
            services.AddScoped<ICollectionService, CollectionService>();
            services.AddScoped<IAiService, AiService>();
            services.AddScoped<ISoaService, SoaService>();
            services.AddScoped<IExcelService, ExcelService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IWhatsappService, WhatsAppService>();
            services.AddScoped<IAgingService, AgingService>();
            services.AddScoped<IOutstandingService, OutstandingService>();
            services.AddScoped<ISalesReturnService, SalesReturnService>();
            services.AddScoped<IPurchaseOrderSerice, PurchaseOrderService>();
            services.AddScoped<IGRNService, GRNService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<ICurrentTenantService, CurrentTenantService>();
            services.AddScoped<ISignupService, SignupService>();
            // Register Services End

            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using MiniB2B.Business.Services.Implementations;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Security;

namespace MiniB2B.Business.Extensions;

public static class ServiceRegistration
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGridConfigService, GridConfigService>();
        services.AddScoped<ISliderService, SliderService>();

        return services;
    }
}

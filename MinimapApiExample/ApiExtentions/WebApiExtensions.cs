namespace MinimapApiExample.ApiExtentions;

using FluentValidation;

public static class WebApiExtensions
{
    public static async Task RequestHandler<TRequest>(HttpContext httpContext)
        where TRequest: IRequest, new()
    {
        var request = await httpContext.ModelBindAsync<TRequest>();
        var validationResult = await httpContext.ValidateAsync(request);
        
        if (validationResult.IsSuccess is not true)
        {
            await httpContext.Response.WriteAsJsonAsync(new
            {
                Message = "Validation failed",
                Errors = validationResult.ValidationMessages
            });

            httpContext.Response.StatusCode = 400;
            return;
        }

        await httpContext.HandleAsync(request);
    }

    private static async Task<TRequest> ModelBindAsync<TRequest>(this HttpContext httpContext)
        where TRequest: IRequest, new()
    {
        var requestType = typeof(TRequest);
        var interfaces = requestType.GetInterfaces();

        TRequest result = interfaces.Any(x => x.Equals(typeof(IFromJsonBody)))
            ? (TRequest)await httpContext.Request.ReadFromJsonAsync(requestType)
            : new TRequest();

        if (result is IFromRoute fromRoute)
        {
            fromRoute.BindFromRoute(httpContext.Request.RouteValues);
        }

        if (result is IFromQuery fromQuery)
        {
            fromQuery.BindFromQuery(httpContext.Request.Query);
        }

        if (result is IWithUserContext withUserContext)
        {
            withUserContext.BindFromUser(httpContext.User);
        }

        return result;
    }

    private class ValidationResult
    {
        public bool IsSuccess { get; set; }
        
        public Dictionary<string, string> ValidationMessages { get; set; }
    }

    private static async Task<ValidationResult> ValidateAsync<TRequest>(this HttpContext httpContext, TRequest request)
    {
        var validatorInterfaceType = typeof(IValidator<>).MakeGenericType(typeof(TRequest));
        var validator = httpContext.RequestServices.GetService(validatorInterfaceType) as IValidator;
        var validationResult = new ValidationResult
        {
            IsSuccess = true
        };
        
        if (validator != null)
        {
            var context = new ValidationContext<TRequest>(request);
            var validation = await validator.ValidateAsync(context);

            validationResult.IsSuccess = false;
            validationResult.ValidationMessages = new Dictionary<string, string>();

            foreach (var error in validation.Errors)
            {
                validationResult.ValidationMessages[error.PropertyName] = error.ErrorMessage;
            }
        }

        return validationResult;
    }

    private static async Task HandleAsync<TRequest>(this HttpContext httpContext, TRequest request)
        where TRequest : IRequest
    {
        var returnType = typeof(TRequest).GetInterfaces()[0].GetGenericArguments()[0];
        var handlerType = typeof(Handler<,>).MakeGenericType(typeof(TRequest), returnType);
        var handler = httpContext.RequestServices.GetService(handlerType) as IHandler<TRequest>;

        var result = await handler.RunAsync(request);
        await httpContext.Response.WriteAsJsonAsync(result);
    }
}
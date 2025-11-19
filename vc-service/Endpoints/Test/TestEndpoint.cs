using FastEndpoints;

namespace SpendingAnalyzer.Endpoints.Test;

public class TestEndpoint : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get("/api/test");
        AllowAnonymous();
        Description(q => q.WithTags("Test").Produces<string>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // In FastEndpoints 7+, use Response property
        Response = "Test";
        
        // For 404:
        // HttpContext.Response.StatusCode = 404;
        
        // For 204:
        // HttpContext.Response.StatusCode = 204;
        
        // For errors:
        // AddError("field", "message");
        // ThrowIfAnyErrors();
        
        // For Created:
        //
    }
}

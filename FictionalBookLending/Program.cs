using FictionalBookLending.src.Application.CQRS.Command;
using FictionalBookLending.src.Application.CQRS.Command.AddBook;
using FictionalBookLending.src.Application.CQRS.Command.BookCheckOut;
using FictionalBookLending.src.Application.CQRS.Command.ReturnBook;
using FictionalBookLending.src.Application.CQRS.Query;
using FictionalBookLending.src.Application.CQRS.Query.GetAllBook;
using FictionalBookLending.src.Application.Middlewares;
using FictionalBookLending.src.Infrastructure.Configuration.ServiceConfiguration;
using FictionalBookLending.src.Infrastructure.Persistence.DB;
using FictionalBookLending.src.Presentation.EndPoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ICommandHandler<AddBookCommand>, AddBookCommadHandler>();
builder.Services.AddTransient<IQueryHandler<BaseQuery>, GetAllBooksQueryHandler>();
builder.Services.AddTransient<ICommandHandler<CheckOutBookCommand>, CheckoutCommandHandler>();
builder.Services.AddTransient<ICommandHandler<ReturnBookCommand>, ReturnBookCommandHandler>();
var sqsConfig = await DependencyInjection.GetSqsConfigurationAsync(builder.Configuration, builder.Environment);
builder.Services.AddSingleton(sqsConfig);



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var bootstrapper = scope.ServiceProvider.GetRequiredService<IDynamoDbBootstrapper>();
    
    await bootstrapper.InitializeAsync(CancellationToken.None);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();
app.MapBookEndpoints();
app.UseHttpsRedirection();
app.Run();



using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FictionalBookLending.src.Application.DTOs;
using FictionalBookLending.src.Application.CQRS.Command;
using FictionalBookLending.src.Application.Contracts;
using FictionalBookLending.src.Application.CQRS.Query;
using FictionalBookLending.src.Application.CQRS.Command.AddBook;
using System.Xml.Linq;
using FictionalBookLending.src.Application.CQRS.Query.GetAllBook;
using FictionalBookLending.src.Application.CQRS.Command.BookCheckOut;
using FictionalBookLending.src.Application.CQRS.Command.ReturnBook;

namespace FictionalBookLending.src.Presentation.EndPoints
{

    public static class BookEndpoints
    {
        public static IEndpointConventionBuilder MapBookEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            // Group routes under /api/books
            var routeGroup = endpoints.MapGroup("/api/books")
                .WithTags("Books")
                .WithOpenApi();

            
            routeGroup.MapPost(
                "/",
                async (
                    [FromBody] AddBookDTO req,
                    [FromServices] ICommandHandler<AddBookCommand> handler,
                    CancellationToken ct) =>
                {
                    var command = new AddBookCommand(req.Title, req.Author, req.ISBN);
                    var result = await handler.Handle(command, ct);
                    return result;
                })
                .AllowAnonymous()
                .WithName("AddBook")
                .Produces<ApiResponse<AddBookDTO>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .WithSummary("Add a new book to the catalog")
                .WithDescription("Creates a new book record in the system using the provided details.");

            routeGroup.MapGet(
                "/",
                async ( [FromServices] IQueryHandler<BaseQuery> query,CancellationToken ct) =>
                {
                    var result = await query.Handle(null, ct);
                    return result;
                })
                .WithName("GetAllBooks")
                .Produces<ApiResponse<IEnumerable<GetAllBooksDTO>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .WithSummary("Retrieve all books")
                .WithDescription("Returns a list of all books in the catalog.");

            routeGroup.MapPost(
                "/{id:guid}/checkout",
                async (
                    [FromRoute] Guid id, [FromServices] ICommandHandler<CheckOutBookCommand> handler,
                    CancellationToken ct) =>
                {
                    var result = await handler.Handle(new CheckOutBookCommand(id), ct);
                    return result;
                })
                .WithName("CheckoutBook")
                .WithSummary("Check out a book")
                .WithDescription("Marks the book as checked out and publishes a checkout event.")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound);

            routeGroup.MapPost(
                "/{id:guid}/return",
                async (
                    [FromRoute] Guid id,
                    [FromServices] ICommandHandler<ReturnBookCommand> handler,
                    CancellationToken ct) =>
                {
                    var result = await handler.Handle(new ReturnBookCommand(id), ct);
                    return result;
                })
                .WithName("ReturnBook")
                .WithSummary("Return a checked-out book")
                .WithDescription("Marks the book as returned and publishes a return event.")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound);


            return routeGroup;
        }
    }

}

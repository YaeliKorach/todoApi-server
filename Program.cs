using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddCors();
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API Name", Version = "v1" });
// });
var app = builder.Build();


app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

// app.UseSwagger();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API Name V1");
//     c.RoutePrefix = string.Empty; // להציג את Swagger UI בכתובת הבסיס
// });

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.MapGet("/items", async ([FromServices] ToDoDbContext Todos) =>
{
    var allTodos = await Todos.Items.ToListAsync();
    return allTodos;
});


app.MapPost("/item", async ([FromServices] ToDoDbContext todos,[FromBody] Item newItem) =>
{
    todos.Items.Add(newItem);
    await todos.SaveChangesAsync();
    return Results.Created($"/todos/{newItem.Id}", newItem);
});

app.MapPut("/item/{id}", async ([FromServices] ToDoDbContext todos, int id,[FromBody] Item updatedItem) =>
{
    var existingItem = await todos.Items.FindAsync(id);
    if (existingItem == null)
    {
        return Results.NotFound();
    }

   
    existingItem.IsComplete = updatedItem.IsComplete;

    await todos.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/item/{id}", async ([FromServices] ToDoDbContext todos, int id) =>
{
    var itemToRemove = await todos.Items.FindAsync(id);
    if (itemToRemove == null)
    {
        return Results.NotFound();
    }

    todos.Items.Remove(itemToRemove);
    await todos.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/", () => "Hello World!");


app.Run();


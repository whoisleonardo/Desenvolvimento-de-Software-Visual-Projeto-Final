using System;
using System.ComponentModel.DataAnnotations;
using MediaShelf.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();

var users = new List<User>();
var nextId = 1;
users.Add(new User
{
	Id = nextId++,
	Name = "Usuário de Teste",
	Email = "teste@example.com",
	Password = null,
	CreatedAt = DateTime.Now
});

app.MapGet("/", () => Results.Ok(new { message = "MediaShelf API" }));

app.MapPost("/users", (User input) =>
{
	var validationResults = new List<ValidationResult>();
	var ctx = new ValidationContext(input);
	if (!Validator.TryValidateObject(input, ctx, validationResults, true))
	{
		var errors = validationResults.Select(v => v.ErrorMessage).ToArray();
		return Results.BadRequest(new { errors });
	}
	if (string.IsNullOrWhiteSpace(input.Email) || !input.Email.Contains("@"))
	{
		return Results.BadRequest(new { errors = new[] { "Email deve conter '@'." } });
	}

	if (string.IsNullOrWhiteSpace(input.Name))
	{
		return Results.BadRequest(new { errors = new[] { "Name não pode ser vazio." } });
	}

	input.Id = nextId++;
	input.CreatedAt = DateTime.Now;
	users.Add(input);

	return Results.Created($"/users/{input.Id}", input);
});

app.MapGet("/users", () => Results.Ok(users));
app.MapGet("/users/{name}", (string name) =>
{
	var user = users.FirstOrDefault(u => string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase));
	return user is not null ? Results.Ok(user) : Results.NotFound(new { error = "Usuário não encontrado." });
});

app.MapPut("/users/{id:int}", (int id, User input) =>
{
	var existing = users.FirstOrDefault(u => u.Id == id);
	if (existing is null)
		return Results.NotFound(new { error = "Usuário não encontrado." });

	var validationResults = new List<ValidationResult>();
	var ctx = new ValidationContext(input);
	if (!Validator.TryValidateObject(input, ctx, validationResults, true))
	{
		var errors = validationResults.Select(v => v.ErrorMessage).ToArray();
		return Results.BadRequest(new { errors });
	}

	if (string.IsNullOrWhiteSpace(input.Email) || !input.Email.Contains("@"))
		return Results.BadRequest(new { errors = new[] { "Email deve conter '@'." } });

	if (string.IsNullOrWhiteSpace(input.Name))
		return Results.BadRequest(new { errors = new[] { "Name não pode ser vazio." } });

	existing.Name = input.Name;
	existing.Email = input.Email;
	existing.Password = input.Password;

	return Results.Ok(existing);
});

app.MapDelete("/users/{id:int}", (int id) =>
{
	var existing = users.FirstOrDefault(u => u.Id == id);
	if (existing is null)
		return Results.NotFound(new { error = "Usuário não encontrado." });

	users.Remove(existing);
	return Results.NoContent();
});

app.MapDelete("/api/usuario/remover/{id}", ([FromRoute] int id, [FromServices] AppDataContext ctx) =>
{
	var usuario = ctx.Usuarios?.Find(id);
	if (usuario is null)
	{
		return Results.NotFound("Usuário não encontrado");
	}
	ctx.Usuarios!.Remove(usuario);
	ctx.SaveChanges();
	return Results.Ok(usuario);
});

app.MapPatch("/api/usuario/alterar/{id}", ([FromRoute] int id, [FromBody] User usuarioAlterado, [FromServices] AppDataContext ctx) =>
{
	var resultado = ctx.Usuarios?.Find(id);
	if (resultado is null)
	{
		return Results.NotFound("Usuário não encontrado");
	}
	resultado.Name = usuarioAlterado.Name;
	resultado.Email = usuarioAlterado.Email;
	resultado.Password = usuarioAlterado.Password;
	ctx.Usuarios!.Update(resultado);
	ctx.SaveChanges();
	return Results.Ok(resultado);
});

app.Run();



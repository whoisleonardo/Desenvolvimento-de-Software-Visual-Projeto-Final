using System;
using System.ComponentModel.DataAnnotations;
using MediaShelf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { message = "MediaShelf API" }));

app.MapGet("/users/listar", ([FromServices] AppDataContext ctx) =>
{
	var users = ctx.Usuarios?.ToList() ?? new List<User>();
	return Results.Ok(users);
});

app.MapPost("/users/registrar", ([FromBody] User input, [FromServices] AppDataContext ctx) =>
{
	var validationResults = new List<ValidationResult>();
	var validationContext = new ValidationContext(input);
	if (!Validator.TryValidateObject(input, validationContext, validationResults, true))
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

	input.CreatedAt = DateTime.Now;
	ctx.Usuarios!.Add(input);
	ctx.SaveChanges();

	return Results.Created($"/users/{input.Id}", input);
});

app.MapPut("/users/atualizar/{id:int}", (int id, [FromBody] User input, [FromServices] AppDataContext ctx) =>
{
	var existing = ctx.Usuarios?.Find(id);
	if (existing is null)
		return Results.NotFound(new { error = "Usuário não encontrado." });

	var validationResults = new List<ValidationResult>();
	var validationContext = new ValidationContext(input);
	if (!Validator.TryValidateObject(input, validationContext, validationResults, true))
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
	
	ctx.Usuarios!.Update(existing);
	ctx.SaveChanges();

	return Results.Ok(existing);
});

app.MapDelete("/users/delete/{id:int}", ([FromRoute] int id, [FromServices] AppDataContext ctx) =>
{
	var usuario = ctx.Usuarios?.Find(id);
	if (usuario is null)
	{
		return Results.NotFound(new { error = "Usuário não encontrado." });
	}
	ctx.Usuarios!.Remove(usuario);
	ctx.SaveChanges();
	return Results.Ok(usuario);
});

app.MapPatch("/users/alterar/{id:int}", ([FromRoute] int id, [FromBody] User usuarioAlterado, [FromServices] AppDataContext ctx) =>
{
	var resultado = ctx.Usuarios?.Find(id);
	if (resultado is null)
	{
		return Results.NotFound(new { error = "Usuário não encontrado." });
	}

	if (!string.IsNullOrWhiteSpace(usuarioAlterado.Email) && !usuarioAlterado.Email.Contains("@"))
	{
		return Results.BadRequest(new { errors = new[] { "Email deve conter '@'." } });
	}

	if (!string.IsNullOrWhiteSpace(usuarioAlterado.Name))
	{
		resultado.Name = usuarioAlterado.Name;
	}
	
	if (!string.IsNullOrWhiteSpace(usuarioAlterado.Email))
	{
		resultado.Email = usuarioAlterado.Email;
	}
	
	if (!string.IsNullOrWhiteSpace(usuarioAlterado.Password))
	{
		resultado.Password = usuarioAlterado.Password;
	}

	ctx.Usuarios!.Update(resultado);
	ctx.SaveChanges();
	return Results.Ok(resultado);
});

// ==================== ENDPOINTS DE MEDIA ====================

app.MapGet("/media/listar", ([FromServices] AppDataContext ctx) =>
{
	if (ctx.Medias == null)
		return Results.Ok(new List<object>());
	
	var medias = ctx.Medias
		.Include(m => m.User)
		.Include(m => m.Reviews)
		.Select(m => new
		{
			m.Id,
			m.Title,
			m.Description,
			//m.CoverImagePath,
			m.CreatedAt,
			User = new { m.User.Id, m.User.Name, m.User.Email },
			AverageRating = m.Reviews.Any() ? Math.Round(m.Reviews.Average(r => r.Rating), 1) : 0,
			TotalReviews = m.Reviews.Count
		})
		.ToList();
	
	return Results.Ok(medias);
});

app.MapGet("/media/pesquisar/{id:int}", (int id, [FromServices] AppDataContext ctx) =>
{
	if (ctx.Medias == null)
		return Results.NotFound(new { error = "Mídia não encontrada." });
	
	var media = ctx.Medias
		.Include(m => m.User)
		.Include(m => m.Reviews)
		.ThenInclude(r => r.User)
		.Where(m => m.Id == id)
		.Select(m => new
		{
			m.Id,
			m.Title,
			m.Description,
			//m.CoverImagePath,
			m.CreatedAt,
			User = new { m.User.Id, m.User.Name, m.User.Email },
			AverageRating = m.Reviews.Any() ? Math.Round(m.Reviews.Average(r => r.Rating), 1) : 0,
			TotalReviews = m.Reviews.Count,
			Reviews = m.Reviews.Select(r => new
			{
				r.Id,
				r.Rating,
				r.Comment,
				r.CreatedAt,
				User = new { r.User.Id, r.User.Name }
			})
		})
		.FirstOrDefault();
	
	if (media == null)
		return Results.NotFound(new { error = "Mídia não encontrada." });
	
	return Results.Ok(media);
});

app.MapPost("/media/criar", ([FromBody] Media input, [FromServices] AppDataContext ctx) =>
{
	if (string.IsNullOrWhiteSpace(input.Title))
		return Results.BadRequest(new { errors = new[] { "Title não pode ser vazio." } });
	
	if (string.IsNullOrWhiteSpace(input.Description))
		return Results.BadRequest(new { errors = new[] { "Description não pode ser vazia." } });
	
	var user = ctx.Usuarios?.Find(input.UserId);
	if (user == null)
		return Results.BadRequest(new { errors = new[] { "Usuário não encontrado." } });
	
	var media = new Media
	{
		Title = input.Title,
		Description = input.Description,
		UserId = input.UserId,
		CreatedAt = DateTime.Now
	};
	
	// if (coverImage != null && coverImage.Length > 0)
	// {
	// 	var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
	// 	Directory.CreateDirectory(uploadsFolder);
		
	// 	var fileName = $"{Guid.NewGuid()}_{coverImage.FileName}";
	// 	var filePath = Path.Combine(uploadsFolder, fileName);
		
	// 	using (var stream = new FileStream(filePath, FileMode.Create))
	// 	{
	// 		await coverImage.CopyToAsync(stream);
	// 	}
		
	// 	media.CoverImagePath = $"/uploads/{fileName}";
	// }
	
	ctx.Medias!.Add(media);
	ctx.SaveChanges();
	
	return Results.Created($"/media/{media.Id}", new
	{
		media.Id,
		media.Title,
		media.Description,
		//media.CoverImagePath,
		media.UserId,
		media.CreatedAt
	});
});

app.MapPut("/media/atualizar/{id:int}", (int id, [FromBody] Media input, [FromServices] AppDataContext ctx) =>
{
	var media = ctx.Medias?.Find(id);
	if (media == null)
		return Results.NotFound(new { error = "Mídia não encontrada." });
	
	if (!string.IsNullOrWhiteSpace(input.Title))
		media.Title = input.Title;
	
	if (!string.IsNullOrWhiteSpace(input.Description))
		media.Description = input.Description;
	
	// if (coverImage != null && coverImage.Length > 0)
	// {
	// 	// Remover imagem antiga se existir
	// 	if (!string.IsNullOrEmpty(media.CoverImagePath))
	// 	{
	// 		var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", media.CoverImagePath.TrimStart('/'));
	// 		if (File.Exists(oldFilePath))
	// 			File.Delete(oldFilePath);
	// 	}
	// 	
	// 	var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
	// 	Directory.CreateDirectory(uploadsFolder);
	// 	
	// 	var fileName = $"{Guid.NewGuid()}_{coverImage.FileName}";
	// 	var filePath = Path.Combine(uploadsFolder, fileName);
	// 	
	// 	using (var stream = new FileStream(filePath, FileMode.Create))
	// 	{
	// 		await coverImage.CopyToAsync(stream);
	// 	}
	// 	
	// 	media.CoverImagePath = $"/uploads/{fileName}";
	// }
	
	ctx.Medias!.Update(media);
	ctx.SaveChanges();
	
	return Results.Ok(new
	{
		media.Id,
		media.Title,
		media.Description,
		//media.CoverImagePath,
		media.UserId,
		media.CreatedAt
	});
});

app.MapDelete("/media/remover/{id:int}", ([FromRoute] int id, [FromServices] AppDataContext ctx) =>
{
	var media = ctx.Medias?.Find(id);
	if (media == null)
		return Results.NotFound(new { error = "Mídia não encontrada." });
	
	// if (!string.IsNullOrEmpty(media.CoverImagePath))
	// {
	// 	var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", media.CoverImagePath.TrimStart('/'));
	// 	if (File.Exists(filePath))
	// 		File.Delete(filePath);
	// }
	
	ctx.Medias!.Remove(media);
	ctx.SaveChanges();
	
	return Results.Ok(new { message = "Mídia removida com sucesso.", media });
});

app.Run();



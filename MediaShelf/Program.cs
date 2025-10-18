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

// ==================== ENDPOINTS DE USERS ====================

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

// ==================== ENDPOINT DE LOGIN ====================

app.MapPost("/users/login", ([FromBody] LoginRequest loginRequest, [FromServices] AppDataContext ctx) =>
{
	// Validação dos dados de entrada
	var validationResults = new List<ValidationResult>();
	var validationContext = new ValidationContext(loginRequest);
	if (!Validator.TryValidateObject(loginRequest, validationContext, validationResults, true))
	{
		var errors = validationResults.Select(v => v.ErrorMessage).ToArray();
		return Results.BadRequest(new { errors });
	}

	// Buscar usuário pelo email
	var user = ctx.Usuarios?.FirstOrDefault(u => u.Email == loginRequest.Email);
	if (user == null)
	{
		return Results.BadRequest(new { 
			error = "Credenciais inválidas.", 
			message = "Email ou senha incorretos." 
		});
	}

	// Verificar senha (comparação simples - em produção usar hash)
	if (user.Password != loginRequest.Password)
	{
		return Results.BadRequest(new { 
			error = "Credenciais inválidas.", 
			message = "Email ou senha incorretos." 
		});
	}

	// Login bem-sucedido
	var loginResponse = new LoginResponse
	{
		Id = user.Id,
		Name = user.Name,
		Email = user.Email,
		CreatedAt = user.CreatedAt,
		Message = "Login realizado com sucesso!"
	};

	return Results.Ok(loginResponse);
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
			AverageRating = m.Reviews.Any() ? Math.Round(m.Reviews.Average(r => r.Rating), 2) : 0,
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
			AverageRating = m.Reviews.Any() ? Math.Round(m.Reviews.Average(r => r.Rating), 2) : 0,
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

// ==================== ENDPOINTS DE REVIEWS ====================

app.MapPost("/reviews/criar", ([FromBody] Review input, [FromServices] AppDataContext ctx) =>
{
	if (input.Rating < 0 || input.Rating > 5)
		return Results.BadRequest(new { errors = new[] { "A nota deve estar entre 0 e 5." } });
	
	if (string.IsNullOrWhiteSpace(input.Comment))
		return Results.BadRequest(new { errors = new[] { "O comentário não pode ser vazio." } });
	
	if (input.Comment.Length > 1000)
		return Results.BadRequest(new { errors = new[] { "O comentário não pode exceder 1000 caracteres." } });
	
	var user = ctx.Usuarios?.Find(input.UserId);
	if (user == null)
		return Results.NotFound(new { error = "Usuário não encontrado." });
	
	var media = ctx.Medias?.Find(input.MediaId);
	if (media == null)
		return Results.NotFound(new { error = "Mídia não encontrada." });
	
	var existingReview = ctx.Reviews?
		.FirstOrDefault(r => r.UserId == input.UserId && r.MediaId == input.MediaId);
	
	if (existingReview != null)
		return Results.BadRequest(new { errors = new[] { "Você já avaliou esta mídia. Use PUT para atualizar." } });
	
	var review = new Review
	{
		Rating = input.Rating,
		Comment = input.Comment,
		UserId = input.UserId,
		MediaId = input.MediaId,
		CreatedAt = DateTime.Now
	};
	
	ctx.Reviews!.Add(review);
	ctx.SaveChanges();
	
	return Results.Created($"/reviews/{review.Id}", new
	{
		review.Id,
		review.Rating,
		review.Comment,
		review.CreatedAt,
		review.UserId,
		UserName = user.Name,
		review.MediaId,
		MediaTitle = media.Title
	});
});

app.MapGet("/reviews/listar", ([FromServices] AppDataContext ctx) =>
{
	if (ctx.Reviews == null)
		return Results.Ok(new List<object>());
	
	var reviews = ctx.Reviews
		.Include(r => r.User)
		.Include(r => r.Media)
		.Select(r => new
		{
			r.Id,
			r.Rating,
			r.Comment,
			r.CreatedAt,
			User = new { r.User.Id, r.User.Name },
			Media = new { r.Media.Id, r.Media.Title }
		})
		.ToList();
	
	return Results.Ok(reviews);
});

app.MapGet("/reviews/pesquisar/{id:int}", (int id, [FromServices] AppDataContext ctx) =>
{
	if (ctx.Reviews == null)
		return Results.NotFound(new { error = "Avaliação não encontrada." });
	
	var review = ctx.Reviews
		.Include(r => r.User)
		.Include(r => r.Media)
		.Where(r => r.Id == id)
		.Select(r => new
		{
			r.Id,
			r.Rating,
			r.Comment,
			r.CreatedAt,
			User = new { r.User.Id, r.User.Name },
			Media = new { r.Media.Id, r.Media.Title }
		})
		.FirstOrDefault();
	
	if (review == null)
		return Results.NotFound(new { error = "Avaliação não encontrada." });
	
	return Results.Ok(review);
});

app.MapGet("/reviews/media/{mediaId:int}", (int mediaId, [FromServices] AppDataContext ctx) =>
{
	if (ctx.Medias == null)
		return Results.NotFound(new { error = "Mídia não encontrada." });
	
	var media = ctx.Medias
		.Include(m => m.Reviews)
		.ThenInclude(r => r.User)
		.FirstOrDefault(m => m.Id == mediaId);
	
	if (media == null)
		return Results.NotFound(new { error = "Mídia não encontrada." });
	
	var reviews = media.Reviews.Select(r => new
	{
		r.Id,
		r.Rating,
		r.Comment,
		r.CreatedAt,
		User = new { r.User.Id, r.User.Name }
	}).ToList();
	
	return Results.Ok(new
	{
		MediaId = mediaId,
		MediaTitle = media.Title,
		AverageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 2) : 0,
		TotalReviews = reviews.Count,
		Reviews = reviews
	});
});

app.MapGet("/reviews/usuario/{userId:int}", (int userId, [FromServices] AppDataContext ctx) =>
{
	if (ctx.Reviews == null)
		return Results.Ok(new List<object>());
	
	var reviews = ctx.Reviews
		.Include(r => r.User)
		.Include(r => r.Media)
		.Where(r => r.UserId == userId)
		.Select(r => new
		{
			r.Id,
			r.Rating,
			r.Comment,
			r.CreatedAt,
			Media = new { r.Media.Id, r.Media.Title }
		})
		.ToList();
	
	return Results.Ok(reviews);
});

app.MapPut("/reviews/atualizar/{id:int}", (int id, [FromBody] Review input, [FromServices] AppDataContext ctx) =>
{
	var review = ctx.Reviews?.Find(id);
	if (review == null)
		return Results.NotFound(new { error = "Avaliação não encontrada." });
	
	if (input.Rating < 0 || input.Rating > 5)
		return Results.BadRequest(new { errors = new[] { "A nota deve estar entre 0 e 5." } });
	
	if (!string.IsNullOrWhiteSpace(input.Comment))
	{
		if (input.Comment.Length > 1000)
			return Results.BadRequest(new { errors = new[] { "O comentário não pode exceder 1000 caracteres." } });
		
		review.Comment = input.Comment;
	}
	
	review.Rating = input.Rating;
	
	ctx.Reviews!.Update(review);
	ctx.SaveChanges();
	
	return Results.Ok(new
	{
		review.Id,
		review.Rating,
		review.Comment,
		review.CreatedAt,
		review.UserId,
		review.MediaId
	});
});

app.MapDelete("/reviews/remover/{id:int}", (int id, [FromServices] AppDataContext ctx) =>
{
	var review = ctx.Reviews?.Find(id);
	if (review == null)
		return Results.NotFound(new { error = "Avaliação não encontrada." });
	
	ctx.Reviews!.Remove(review);
	ctx.SaveChanges();
	
	return Results.Ok(new { message = "Avaliação removida com sucesso.", review });
});

app.Run();
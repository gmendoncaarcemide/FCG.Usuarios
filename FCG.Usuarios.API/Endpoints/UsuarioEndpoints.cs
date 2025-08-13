using System.ComponentModel.DataAnnotations;
using FCG.Usuarios.Application.Usuarios.Interfaces;
using FCG.Usuarios.Application.Usuarios.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Usuarios.API.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/usuarios", async (IUsuarioService service, [FromBody] CriarUsuarioRequest request) =>
        {
            try
            {
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults);
                }

                var usuario = await service.CriarAsync(request);
                return Results.Created($"/usuarios/{usuario.Id}", usuario);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("CriarUsuario")
        .WithSummary("Cria um novo usuário")
        .WithOpenApi();

        routes.MapPost("/usuarios/login", async (IUsuarioService service, [FromBody] LoginRequest request) =>
        {
            try
            {
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults);
                }

                var response = await service.LoginAsync(request);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("LoginUsuario")
        .WithSummary("Realiza login do usuário")
        .WithOpenApi();

        routes.MapGet("/usuarios", async (IUsuarioService service) =>
        {
            try
            {
                var usuarios = await service.ObterTodosAsync();
                return Results.Ok(usuarios);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .RequireAuthorization("Admin")
        .WithName("ObterTodosUsuarios")
        .WithSummary("Obtém todos os usuários")
        .WithOpenApi();

        routes.MapGet("/usuarios/{id:guid}", async (IUsuarioService service, Guid id) =>
        {
            try
            {
                var usuario = await service.ObterPorIdAsync(id);
                if (usuario == null)
                    return Results.NotFound($"Usuário com ID {id} não encontrado");

                return Results.Ok(usuario);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .RequireAuthorization("Admin")
        .WithName("ObterUsuarioPorId")
        .WithSummary("Obtém um usuário por ID")
        .WithOpenApi();

        routes.MapGet("/usuarios/email/{email}", async (IUsuarioService service, string email) =>
        {
            try
            {
                var usuario = await service.ObterPorEmailAsync(email);
                if (usuario == null)
                    return Results.NotFound($"Usuário com email {email} não encontrado");

                return Results.Ok(usuario);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .RequireAuthorization("Admin")
        .WithName("ObterUsuarioPorEmail")
        .WithSummary("Obtém um usuário por email")
        .WithOpenApi();

        routes.MapPut("/usuarios/{id:guid}", async (IUsuarioService service, Guid id, [FromBody] AtualizarUsuarioRequest request) =>
        {
            try
            {
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults);
                }

                var usuario = await service.AtualizarAsync(id, request);
                return Results.Ok(usuario);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("não encontrado"))
                    return Results.NotFound(ex.Message);
                if (ex.Message.Contains("já está em uso"))
                    return Results.Conflict(ex.Message);
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .RequireAuthorization("Admin")
        .WithName("AtualizarUsuario")
        .WithSummary("Atualiza um usuário existente")
        .WithOpenApi();

        routes.MapDelete("/usuarios/{id:guid}", async (IUsuarioService service, Guid id) =>
        {
            try
            {
                var sucesso = await service.ExcluirAsync(id);
                if (!sucesso)
                    return Results.NotFound($"Usuário com ID {id} não encontrado");

                return Results.Ok($"Usuário com ID {id} excluído com sucesso");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .RequireAuthorization("Admin")
        .WithName("ExcluirUsuario")
        .WithSummary("Exclui um usuário")
        .WithOpenApi();

        routes.MapPost("/usuarios/{id:guid}/alterar-senha", async (IUsuarioService service, Guid id, [FromBody] AlterarSenhaRequest request) =>
        {
            try
            {
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults);
                }

                var sucesso = await service.AlterarSenhaAsync(id, request.SenhaAtual, request.NovaSenha);
                if (!sucesso)
                    return Results.BadRequest("Senha atual incorreta ou usuário não encontrado");

                return Results.Ok("Senha alterada com sucesso");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .RequireAuthorization("Admin")
        .WithName("AlterarSenha")
        .WithSummary("Altera a senha de um usuário")
        .WithOpenApi();

        routes.MapGet("/usuarios/verificar-email/{email}", async (IUsuarioService service, string email) =>
        {
            try
            {
                var existe = await service.VerificarEmailAsync(email);
                return Results.Ok(new { Email = email, Existe = existe });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("VerificarEmail")
        .WithSummary("Verifica se um email já está cadastrado")
        .WithOpenApi();
    }
}

public class AlterarSenhaRequest
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Nova senha deve ter entre 6 e 100 caracteres")]
    public string NovaSenha { get; set; } = string.Empty;
} 
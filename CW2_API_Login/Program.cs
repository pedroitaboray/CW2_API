using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<PrestadorDb>(opt => opt.UseInMemoryDatabase("UsuarioList"));
var app = builder.Build();

app.MapGet("/prestadores", async (PrestadorDb db) =>
    await db.Usuarios.Select(x => new PrestadorDTO(x)).ToListAsync());

app.MapGet("/prestadores/{id}", async (int id, PrestadorDb db) =>
    await db.Usuarios.FindAsync(id)
        is Prestador usuario
            ? Results.Ok(new PrestadorDTO(usuario))
            : Results.NotFound());

app.MapPost("/prestadores", async (Prestador usuario, PrestadorDb db) =>
{
    db.Usuarios.Add(usuario);
    await db.SaveChangesAsync();

    return Results.Created($"/prestadores/{usuario.Id}", new PrestadorDTO(usuario));
});

app.MapPut("/prestadores/{id}", async (int id, PrestadorDTO usuarioDTO, PrestadorDb db) =>
{
    var usuario = await db.Usuarios.FindAsync(id);

    if (usuario is null) return Results.NotFound();

    usuario.Nome = usuarioDTO.Nome;
    usuario.Ativo = usuarioDTO.Ativo;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/prestadores/{id}", async (int id, PrestadorDb db) =>
{
    if (await db.Usuarios.FindAsync(id) is Prestador usuario)
    {
        db.Usuarios.Remove(usuario);
        await db.SaveChangesAsync();
        return Results.Ok(new PrestadorDTO(usuario));
    }

    return Results.NotFound();
});

app.Run();

public class Prestador
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? cgccpf { get; set; }
    public bool Ativo { get; set; }
}

public class PrestadorDTO
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? cgccpf { get; set; }
    public bool Ativo { get; set; }

    public PrestadorDTO() { }
    public PrestadorDTO(Prestador usuario) =>
    (Id, Nome, Ativo, cgccpf) = (usuario.Id, usuario.Nome, usuario.Ativo, usuario.cgccpf);
}


class PrestadorDb : DbContext
{
    public PrestadorDb(DbContextOptions<PrestadorDb> options)
    : base(options) { }

    public DbSet<Prestador> Usuarios => Set<Prestador>();
}
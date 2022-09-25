using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<PrestadorDb>(opt => opt.UseInMemoryDatabase("PrestadorList"));
var app = builder.Build();

app.MapGet("/prestadores", async (PrestadorDb db) =>
    await db.Prestadores.Select(x => new PrestadorDTO(x)).ToListAsync());

app.MapGet("/prestadores/{id}", async (int id, PrestadorDb db) =>
    await db.Prestadores.FindAsync(id)
        is Prestador prestador
            ? Results.Ok(new PrestadorDTO(prestador))
            : Results.NotFound());

app.MapPost("/prestadores", async (Prestador prestador, PrestadorDb db) =>
{
    db.Prestadores.Add(prestador);
    await db.SaveChangesAsync();

    return Results.Created($"/prestadores/{prestador.Id}", new PrestadorDTO(prestador));
});

app.MapPut("/prestadores/{id}", async (int id, PrestadorDTO prestadorDTO, PrestadorDb db) =>
{
    var prestador = await db.Prestadores.FindAsync(id);

    if (prestador is null) return Results.NotFound();

    prestador.Nome = prestadorDTO.Nome;
    prestador.Ativo = prestadorDTO.Ativo;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/prestadores/{id}", async (int id, PrestadorDb db) =>
{
    if (await db.Prestadores.FindAsync(id) is Prestador prestador)
    {
        db.Prestadores.Remove(prestador);
        await db.SaveChangesAsync();
        return Results.Ok(new PrestadorDTO(prestador));
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

    public DbSet<Prestador> Prestadores => Set<Prestador>();
}
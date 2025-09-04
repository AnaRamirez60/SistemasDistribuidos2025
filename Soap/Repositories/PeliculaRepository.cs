using Microsoft.EntityFrameworkCore;
using PeliculaApi.Infrastructure;
using PeliculaApi.Models;
using PeliculaApi.Mappers;

namespace PeliculaApi.Repositories;

public class PeliculaRepository : IPeliculaRepository
    {
        private readonly RelationalDbContext _context;

        public PeliculaRepository(RelationalDbContext context)
        {
            _context = context;

        }

        public async Task<IReadOnlyList<Pelicula>> GetPeliculasByTitleAsync(string title, CancellationToken cancellationToken)
        {
            var peliculas = await _context.Peliculas.AsNoTracking().Where(s => s.Title.Contains(title)).ToListAsync(cancellationToken);
            return peliculas.ToModel();
        }

        public async Task<IReadOnlyList<Pelicula>> GetPeliculasByGenreAsync(string genre, CancellationToken cancellationToken)
        {
            var peliculas = await _context.Peliculas.AsNoTracking().Where(s => s.Genre.Contains(genre)).ToListAsync(cancellationToken);
            return peliculas.ToModel();
        }
    
        public async Task<Pelicula> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            //select * from peliculas where id = ID
            var pelicula = await _context.Peliculas.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            return pelicula.ToModel();
        }

        public async Task<Pelicula> CreateAsync(Pelicula pelicula, CancellationToken cancellationToken)
        {
            var peliculaToCreate = pelicula.ToEntity();
            peliculaToCreate.Id = Guid.NewGuid();
            await _context.Peliculas.AddAsync(peliculaToCreate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return peliculaToCreate.ToModel();
        }
    }
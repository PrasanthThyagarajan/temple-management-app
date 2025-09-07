using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class PoojaService : IPoojaService
    {
        private readonly IRepository<Pooja> _poojaRepository;

        public PoojaService(IRepository<Pooja> poojaRepository)
        {
            _poojaRepository = poojaRepository;
        }

        public async Task<PoojaDto> CreatePoojaAsync(CreatePoojaDto createPoojaDto)
        {
            var pooja = new Pooja
            {
                Name = createPoojaDto.Name,
                Description = createPoojaDto.Description,
                Price = createPoojaDto.Price
            };

            await _poojaRepository.AddAsync(pooja);

            return MapToDto(pooja);
        }

        public async Task<PoojaDto?> GetPoojaByIdAsync(int id)
        {
            var pooja = await _poojaRepository.GetByIdAsync(id);
            
            return pooja != null ? MapToDto(pooja) : null;
        }

        public async Task<IEnumerable<PoojaDto>> GetAllPoojasAsync()
        {
            var poojas = await _poojaRepository.GetAllAsync();
            
            return poojas.Select(MapToDto);
        }

        public async Task<PoojaDto> UpdatePoojaAsync(int id, CreatePoojaDto updatePoojaDto)
        {
            var pooja = await _poojaRepository.GetByIdAsync(id);
            if (pooja == null)
            {
                throw new KeyNotFoundException("Pooja not found.");
            }

            pooja.Name = updatePoojaDto.Name;
            pooja.Description = updatePoojaDto.Description;
            pooja.Price = updatePoojaDto.Price;

            await _poojaRepository.UpdateAsync(pooja);
            return MapToDto(pooja);
        }

        public async Task<bool> DeletePoojaAsync(int id)
        {
            return await _poojaRepository.SoftDeleteAsync(id);
        }

        private static PoojaDto MapToDto(Pooja pooja)
        {
            return new PoojaDto
            {
                Id = pooja.Id,
                Name = pooja.Name,
                Description = pooja.Description,
                Price = pooja.Price,
                CreatedAt = pooja.CreatedAt,
                UpdatedAt = pooja.UpdatedAt
            };
        }
    }
}

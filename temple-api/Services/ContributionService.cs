using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Repositories.Interfaces;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class ContributionService : IContributionService
    {
        private readonly IContributionRepository _repository;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<Devotee> _devoteeRepository;
        private readonly IRepository<ContributionSetting> _contributionSettingRepository;
        private readonly ILogger<ContributionService> _logger;

        public ContributionService(
            IContributionRepository repository,
            IRepository<Event> eventRepository,
            IRepository<Devotee> devoteeRepository,
            IRepository<ContributionSetting> contributionSettingRepository,
            ILogger<ContributionService> logger)
        {
            _repository = repository;
            _eventRepository = eventRepository;
            _devoteeRepository = devoteeRepository;
            _contributionSettingRepository = contributionSettingRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ContributionDto>> GetAllAsync()
        {
            try
            {
                var contributions = await _repository.GetAllAsync();
                return contributions.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all contributions");
                throw;
            }
        }

        public async Task<IEnumerable<ContributionDto>> GetByEventIdAsync(int eventId)
        {
            try
            {
                var contributions = await _repository.GetByEventIdAsync(eventId);
                return contributions.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contributions for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<IEnumerable<ContributionDto>> GetByDevoteeIdAsync(int devoteeId)
        {
            try
            {
                var contributions = await _repository.GetByDevoteeIdAsync(devoteeId);
                return contributions.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contributions for devotee {DevoteeId}", devoteeId);
                throw;
            }
        }

        public async Task<ContributionDto?> GetByIdAsync(int id)
        {
            try
            {
                var contribution = await _repository.GetByIdAsync(id);
                return contribution != null ? MapToDto(contribution) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contribution {Id}", id);
                throw;
            }
        }

        public async Task<ContributionDto> CreateAsync(CreateContributionDto dto)
        {
            try
            {
                // Validate event exists and is active
                var eventEntity = await _eventRepository.GetByIdAsync(dto.EventId);
                if (eventEntity == null || !eventEntity.IsActive)
                {
                    throw new InvalidOperationException($"Event with ID {dto.EventId} not found or inactive");
                }

                // Validate devotee exists and is active
                var devotee = await _devoteeRepository.GetByIdAsync(dto.DevoteeId);
                if (devotee == null || !devotee.IsActive)
                {
                    throw new InvalidOperationException($"Devotee with ID {dto.DevoteeId} not found or inactive");
                }

                // Validate contribution setting exists and is active
                var contributionSetting = await _contributionSettingRepository.GetByIdAsync(dto.ContributionSettingId);
                if (contributionSetting == null || !contributionSetting.IsActive)
                {
                    throw new InvalidOperationException($"Contribution setting with ID {dto.ContributionSettingId} not found or inactive");
                }

                // Validate that contribution setting belongs to the selected event
                if (contributionSetting.EventId != dto.EventId)
                {
                    throw new InvalidOperationException("Selected contribution setting does not belong to the selected event");
                }

                // Validate amount is positive
                if (dto.Amount <= 0)
                {
                    throw new InvalidOperationException("Contribution amount must be greater than zero");
                }

                var contribution = new Contribution
                {
                    EventId = dto.EventId,
                    DevoteeId = dto.DevoteeId,
                    ContributionSettingId = dto.ContributionSettingId,
                    Amount = dto.Amount,
                    ContributionDate = dto.ContributionDate,
                    Notes = dto.Notes,
                    PaymentMethod = dto.PaymentMethod,
                    TransactionReference = dto.TransactionReference,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _repository.AddAsync(contribution);
                _logger.LogInformation("Created contribution {Id} for devotee {DevoteeId} and event {EventId}", 
                    created.Id, created.DevoteeId, created.EventId);
                
                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contribution");
                throw;
            }
        }

        public async Task<ContributionDto?> UpdateAsync(int id, CreateContributionDto dto)
        {
            try
            {
                var contribution = await _repository.GetByIdAsync(id);
                if (contribution == null)
                {
                    return null;
                }

                // Validate event exists and is active
                var eventEntity = await _eventRepository.GetByIdAsync(dto.EventId);
                if (eventEntity == null || !eventEntity.IsActive)
                {
                    throw new InvalidOperationException($"Event with ID {dto.EventId} not found or inactive");
                }

                // Validate devotee exists and is active
                var devotee = await _devoteeRepository.GetByIdAsync(dto.DevoteeId);
                if (devotee == null || !devotee.IsActive)
                {
                    throw new InvalidOperationException($"Devotee with ID {dto.DevoteeId} not found or inactive");
                }

                // Validate contribution setting exists and is active
                var contributionSetting = await _contributionSettingRepository.GetByIdAsync(dto.ContributionSettingId);
                if (contributionSetting == null || !contributionSetting.IsActive)
                {
                    throw new InvalidOperationException($"Contribution setting with ID {dto.ContributionSettingId} not found or inactive");
                }

                // Validate that contribution setting belongs to the selected event
                if (contributionSetting.EventId != dto.EventId)
                {
                    throw new InvalidOperationException("Selected contribution setting does not belong to the selected event");
                }

                // Validate amount is positive
                if (dto.Amount <= 0)
                {
                    throw new InvalidOperationException("Contribution amount must be greater than zero");
                }

                contribution.EventId = dto.EventId;
                contribution.DevoteeId = dto.DevoteeId;
                contribution.ContributionSettingId = dto.ContributionSettingId;
                contribution.Amount = dto.Amount;
                contribution.ContributionDate = dto.ContributionDate;
                contribution.Notes = dto.Notes;
                contribution.PaymentMethod = dto.PaymentMethod;
                contribution.TransactionReference = dto.TransactionReference;
                contribution.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(contribution);
                _logger.LogInformation("Updated contribution {Id}", id);
                
                return MapToDto(contribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contribution {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var contribution = await _repository.GetByIdAsync(id);
                if (contribution == null)
                {
                    return false;
                }

                contribution.IsActive = false;
                contribution.UpdatedAt = DateTime.UtcNow;
                
                await _repository.UpdateAsync(contribution);
                _logger.LogInformation("Soft deleted contribution {Id}", id);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contribution {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ContributionDto>> GetActiveContributionsAsync()
        {
            try
            {
                var contributions = await _repository.GetActiveContributionsAsync();
                return contributions.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active contributions");
                throw;
            }
        }

        public async Task<decimal> GetTotalContributionsByEventAsync(int eventId)
        {
            try
            {
                return await _repository.GetTotalContributionsByEventAsync(eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total contributions for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<IEnumerable<ContributionDto>> GetContributionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var contributions = await _repository.GetContributionsByDateRangeAsync(startDate, endDate);
                return contributions.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contributions by date range");
                throw;
            }
        }

        public async Task<IEnumerable<ContributionSummaryDto>> GetContributionSummaryByEventAsync()
        {
            try
            {
                var contributions = await _repository.GetActiveContributionsAsync();
                var summary = contributions
                    .GroupBy(c => new { c.EventId, c.Event?.Name })
                    .Select(g => new ContributionSummaryDto
                    {
                        EventId = g.Key.EventId,
                        EventName = g.Key.Name,
                        TotalAmount = g.Sum(c => c.Amount),
                        ContributionCount = g.Count(),
                        LastContributionDate = g.Max(c => c.ContributionDate)
                    })
                    .OrderByDescending(s => s.TotalAmount)
                    .ToList();

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contribution summary by event");
                throw;
            }
        }

        private ContributionDto MapToDto(Contribution contribution)
        {
            return new ContributionDto
            {
                Id = contribution.Id,
                EventId = contribution.EventId,
                EventName = contribution.Event?.Name,
                DevoteeId = contribution.DevoteeId,
                DevoteeName = contribution.Devotee?.FullName,
                ContributionSettingId = contribution.ContributionSettingId,
                ContributionSettingName = contribution.ContributionSetting?.Name,
                Amount = contribution.Amount,
                ContributionDate = contribution.ContributionDate,
                Notes = contribution.Notes,
                PaymentMethod = contribution.PaymentMethod,
                TransactionReference = contribution.TransactionReference,
                IsActive = contribution.IsActive,
                CreatedAt = contribution.CreatedAt,
                UpdatedAt = contribution.UpdatedAt ?? contribution.CreatedAt
            };
        }
    }
}

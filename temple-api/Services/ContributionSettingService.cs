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
    public class ContributionSettingService : IContributionSettingService
    {
        private readonly IContributionSettingRepository _repository;
        private readonly IRepository<Event> _eventRepository;
        private readonly ILogger<ContributionSettingService> _logger;

        public ContributionSettingService(
            IContributionSettingRepository repository,
            IRepository<Event> eventRepository,
            ILogger<ContributionSettingService> logger)
        {
            _repository = repository;
            _eventRepository = eventRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ContributionSettingDto>> GetAllAsync()
        {
            try
            {
                var contributions = await _repository.GetAllAsync();
                return contributions.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all contribution settings");
                throw;
            }
        }

        public async Task<IEnumerable<ContributionSettingDto>> GetByEventIdAsync(int eventId)
        {
            try
            {
                var contributions = await _repository.GetByEventIdAsync(eventId);
                return contributions.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contribution settings for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<ContributionSettingDto?> GetByIdAsync(int id)
        {
            try
            {
                var contribution = await _repository.GetByIdAsync(id);
                return contribution != null ? MapToDto(contribution) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contribution setting {Id}", id);
                throw;
            }
        }

        public async Task<ContributionSettingDto> CreateAsync(CreateContributionSettingDto dto)
        {
            try
            {
                // Validate event exists
                var eventExists = await _eventRepository.GetByIdAsync(dto.EventId);
                if (eventExists == null)
                {
                    throw new InvalidOperationException($"Event with ID {dto.EventId} not found");
                }

                // Validate contribution type
                if (dto.ContributionType != "Single" && dto.ContributionType != "Recurring")
                {
                    throw new InvalidOperationException("Contribution type must be 'Single' or 'Recurring'");
                }

                // Validate recurring settings
                if (dto.ContributionType == "Recurring")
                {
                    if (!dto.RecurringDay.HasValue || dto.RecurringDay < 1 || dto.RecurringDay > 31)
                    {
                        throw new InvalidOperationException("Recurring contributions must have a valid day (1-31)");
                    }
                    
                    if (string.IsNullOrWhiteSpace(dto.RecurringFrequency))
                    {
                        throw new InvalidOperationException("Recurring contributions must have a frequency");
                    }
                }

                var contribution = new ContributionSetting
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    EventId = dto.EventId,
                    ContributionType = dto.ContributionType,
                    Amount = dto.Amount,
                    RecurringDay = dto.RecurringDay,
                    RecurringFrequency = dto.RecurringFrequency,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _repository.AddAsync(contribution);
                _logger.LogInformation("Created contribution setting {Id} for event {EventId}", created.Id, created.EventId);
                
                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contribution setting");
                throw;
            }
        }

        public async Task<ContributionSettingDto?> UpdateAsync(int id, CreateContributionSettingDto dto)
        {
            try
            {
                var contribution = await _repository.GetByIdAsync(id);
                if (contribution == null)
                {
                    return null;
                }

                // Validate event exists
                var eventExists = await _eventRepository.GetByIdAsync(dto.EventId);
                if (eventExists == null)
                {
                    throw new InvalidOperationException($"Event with ID {dto.EventId} not found");
                }

                // Validate contribution type
                if (dto.ContributionType != "Single" && dto.ContributionType != "Recurring")
                {
                    throw new InvalidOperationException("Contribution type must be 'Single' or 'Recurring'");
                }

                // Validate recurring settings
                if (dto.ContributionType == "Recurring")
                {
                    if (!dto.RecurringDay.HasValue || dto.RecurringDay < 1 || dto.RecurringDay > 31)
                    {
                        throw new InvalidOperationException("Recurring contributions must have a valid day (1-31)");
                    }
                    
                    if (string.IsNullOrWhiteSpace(dto.RecurringFrequency))
                    {
                        throw new InvalidOperationException("Recurring contributions must have a frequency");
                    }
                }

                contribution.Name = dto.Name;
                contribution.Description = dto.Description;
                contribution.EventId = dto.EventId;
                contribution.ContributionType = dto.ContributionType;
                contribution.Amount = dto.Amount;
                contribution.RecurringDay = dto.RecurringDay;
                contribution.RecurringFrequency = dto.RecurringFrequency;
                contribution.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(contribution);
                _logger.LogInformation("Updated contribution setting {Id}", id);
                
                return MapToDto(contribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contribution setting {Id}", id);
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
                _logger.LogInformation("Soft deleted contribution setting {Id}", id);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contribution setting {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ContributionSettingDto>> GetActiveContributionsAsync()
        {
            try
            {
                var contributions = await _repository.GetActiveContributionsAsync();
                return contributions.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active contribution settings");
                throw;
            }
        }

        private ContributionSettingDto MapToDto(ContributionSetting contribution)
        {
            return new ContributionSettingDto
            {
                Id = contribution.Id,
                Name = contribution.Name,
                Description = contribution.Description,
                EventId = contribution.EventId,
                EventName = contribution.Event?.Name,
                ContributionType = contribution.ContributionType,
                Amount = contribution.Amount,
                RecurringDay = contribution.RecurringDay,
                RecurringFrequency = contribution.RecurringFrequency,
                IsActive = contribution.IsActive,
                CreatedAt = contribution.CreatedAt,
                UpdatedAt = contribution.UpdatedAt ?? contribution.CreatedAt
            };
        }
    }
}

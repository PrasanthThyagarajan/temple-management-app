using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace TempleApi.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IRepository<Expense> _ExpenseRepository;
        private readonly IRepository<EventExpense> _EventExpenseRepository;
        private readonly IRepository<TempleApi.Domain.Entities.ExpenseService> _ExpenseServiceRepository;
        private readonly IRepository<Event> _EventRepository;
        private readonly IRepository<User> _UserRepository;
        private readonly IRepository<UserRole> _UserRoleRepository;
        private readonly IRepository<Role> _RoleRepository;
        private readonly ILogger<ExpenseService> _logger;
        private readonly IRepository<EventApprovalRoleConfiguration> _eventApprovalConfigRepo;
        private readonly IRepository<ExpenseApprovalRoleConfiguration> _ExpenseApprovalConfigRepo;

        public ExpenseService(
            IRepository<Expense> ExpenseRepository,
            IRepository<EventExpense> EventExpenseRepository,
            IRepository<TempleApi.Domain.Entities.ExpenseService> ExpenseServiceRepository,
            IRepository<Event> EventRepository,
            IRepository<User> UserRepository,
            IRepository<UserRole> UserRoleRepository,
            IRepository<Role> RoleRepository,
            IRepository<EventApprovalRoleConfiguration> eventApprovalConfigRepo,
            IRepository<ExpenseApprovalRoleConfiguration> ExpenseApprovalConfigRepo,
            ILogger<ExpenseService> logger)
        {
            _ExpenseRepository = ExpenseRepository;
            _EventExpenseRepository = EventExpenseRepository;
            _ExpenseServiceRepository = ExpenseServiceRepository;
            _EventRepository = EventRepository;
            _UserRepository = UserRepository;
            _UserRoleRepository = UserRoleRepository;
            _RoleRepository = RoleRepository;
            _eventApprovalConfigRepo = eventApprovalConfigRepo;
            _ExpenseApprovalConfigRepo = ExpenseApprovalConfigRepo;
            _logger = logger;
        }

        private async Task<(string? UserName, string? UserRole)> GetUserInfoWithRoleAsync(int? userId)
        {
            if (!userId.HasValue) return (null, null);

            var user = await _UserRepository.GetByIdAsync(userId.Value);
            if (user == null) return (null, null);

            // Get the user's primary role (first role if multiple)
            var userRoles = await _UserRoleRepository.GetAllAsync();
            var userRole = userRoles.FirstOrDefault(ur => ur.UserId == userId.Value);
            
            string? roleName = null;
            if (userRole != null)
            {
                var role = await _RoleRepository.GetByIdAsync(userRole.RoleId);
                roleName = role?.RoleName;
            }

            return (user.Username, roleName);
        }

        public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto createDto)
        {
            // Validate selection
            EventExpense? eventExpense = null;
            TempleApi.Domain.Entities.ExpenseService? expenseService = null;

            if (createDto.EventExpenseId.HasValue)
            {
                eventExpense = await _EventExpenseRepository.GetByIdAsync(createDto.EventExpenseId.Value);
                if (eventExpense == null)
                {
                    throw new ArgumentException("Expense item not found.");
                }
            }
            else if (createDto.ExpenseServiceId.HasValue)
            {
                expenseService = await _ExpenseServiceRepository.GetByIdAsync(createDto.ExpenseServiceId.Value);
                if (expenseService == null)
                {
                    throw new ArgumentException("Expense service not found.");
                }
            }

            // Get the event to check if it requires approval
            var eventEntity = await _EventRepository.GetByIdAsync(createDto.EventId);
            if (eventEntity == null)
            {
                throw new ArgumentException("Event not found.");
            }
            
            // Determine if approval is needed based on Event, EventExpense, or ExpenseService
            bool isApprovalNeed = false;
            
            // Check if Event requires approval
            if (eventEntity.IsApprovalNeeded)
            {
                isApprovalNeed = true;
            }
            // Check if EventExpense (item) requires approval
            else if (eventExpense != null && eventExpense.IsApprovalNeeded)
            {
                isApprovalNeed = true;
            }
            // Check if ExpenseService requires approval
            else if (expenseService != null && expenseService.IsApprovalNeeded)
            {
                isApprovalNeed = true;
            }

            var Expense = new Expense
            {
                EventExpenseId = createDto.EventExpenseId,
                ExpenseServiceId = createDto.ExpenseServiceId,
                EventId = createDto.EventId,
                Price = createDto.Price,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsApprovalNeed = isApprovalNeed,
                IsApproved = false,
                RequestedBy = createDto.RequestedBy ?? 1 // Default to user ID 1 if not provided
            };

            var createdExpense = await _ExpenseRepository.AddAsync(Expense);

            return new ExpenseDto
            {
                Id = createdExpense.Id,
                EventExpenseId = createdExpense.EventExpenseId,
                ExpenseServiceId = createdExpense.ExpenseServiceId,
                EventId = createdExpense.EventId,
                Price = createdExpense.Price,
                CreatedAt = createdExpense.CreatedAt,
                IsApprovalNeed = createdExpense.IsApprovalNeed,
                IsApproved = createdExpense.IsApproved,
                RequestedBy = createdExpense.RequestedBy,
                EventExpense = eventExpense != null ? new EventExpenseDto
                {
                    Id = eventExpense.Id,
                    Name = eventExpense.Name,
                    Description = eventExpense.Description,
                    CreatedAt = eventExpense.CreatedAt,
                    IsActive = eventExpense.IsActive,
                    ApprovalRoleId = eventExpense.ApprovalRoleId
                } : null,
                ExpenseService = expenseService != null ? new ExpenseServiceDto
                {
                    Id = expenseService.Id,
                    Name = expenseService.Name,
                    Description = expenseService.Description,
                    CreatedAt = expenseService.CreatedAt,
                    IsActive = expenseService.IsActive,
                    ApprovalRoleId = expenseService.ApprovalRoleId
                } : null
            };
        }

        public async Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync()
        {
            try
            {
                var Expenses = await _ExpenseRepository.GetAllAsync();
                var eventExpenses = await _EventExpenseRepository.GetAllAsync();
                var expenseServices = await _ExpenseServiceRepository.GetAllAsync();
                
                var result = new List<ExpenseDto>();
                
                foreach (var p in Expenses)
                {
                    // Get user information for RequestedBy and ApprovedBy (handle missing fields gracefully)
                    var (requestedByUserName, requestedByUserRole) = (null as string, null as string);
                    var (approvedByUserName, approvedByUserRole) = (null as string, null as string);
                    
                    try
                    {
                        // Check if the properties exist (for backward compatibility)
                        var requestedBy = p.RequestedBy;
                        var approvedBy = p.ApprovedBy;
                        
                        if (requestedBy.HasValue)
                        {
                            (requestedByUserName, requestedByUserRole) = await GetUserInfoWithRoleAsync(requestedBy);
                        }
                        if (approvedBy.HasValue)
                        {
                            (approvedByUserName, approvedByUserRole) = await GetUserInfoWithRoleAsync(approvedBy);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get user info for expense {ExpenseId}", p.Id);
                        // Continue with null values
                    }
                
                var dto = new ExpenseDto
                {
                    Id = p.Id,
                    EventExpenseId = p.EventExpenseId,
                    ExpenseServiceId = p.ExpenseServiceId,
                    EventId = p.EventId,
                    Price = p.Price,
                    CreatedAt = p.CreatedAt,
                    IsApprovalNeed = p.IsApprovalNeed,
                    IsApproved = p.IsApproved,
                    RequestedBy = p.RequestedBy,
                    RequestedByUserName = requestedByUserName,
                    RequestedByUserRole = requestedByUserRole,
                    ApprovedBy = p.ApprovedBy,
                    ApprovedByUserName = approvedByUserName,
                    ApprovedByUserRole = approvedByUserRole,
                    ApprovedOn = p.ApprovedOn,
                    EventExpense = p.EventExpenseId.HasValue 
                        ? (eventExpenses.FirstOrDefault(e => e.Id == p.EventExpenseId.Value) is EventExpense ee ? new EventExpenseDto
                        {
                            Id = ee.Id,
                            Name = ee.Name,
                            Description = ee.Description,
                            CreatedAt = ee.CreatedAt,
                            IsActive = ee.IsActive,
                            ApprovalRoleId = ee.ApprovalRoleId
                        } : null) : null,
                    ExpenseService = p.ExpenseServiceId.HasValue 
                        ? (expenseServices.FirstOrDefault(s => s.Id == p.ExpenseServiceId.Value) is TempleApi.Domain.Entities.ExpenseService es ? new ExpenseServiceDto
                        {
                            Id = es.Id,
                            Name = es.Name,
                            Description = es.Description,
                            CreatedAt = es.CreatedAt,
                            IsActive = es.IsActive,
                            ApprovalRoleId = es.ApprovalRoleId
                        } : null) : null
                };
                
                result.Add(dto);
            }
            
            return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all expenses");
                return new List<ExpenseDto>(); // Return empty list on error
            }
        }

        public async Task<ExpenseDto?> GetExpenseByIdAsync(int id)
        {
            var Expense = await _ExpenseRepository.GetByIdAsync(id);
            if (Expense == null) return null;

            EventExpense? eventExpense = null;
            TempleApi.Domain.Entities.ExpenseService? expenseService = null;

            if (Expense.EventExpenseId.HasValue)
            {
                eventExpense = await _EventExpenseRepository.GetByIdAsync(Expense.EventExpenseId.Value);
            }
            else if (Expense.ExpenseServiceId.HasValue)
            {
                expenseService = await _ExpenseServiceRepository.GetByIdAsync(Expense.ExpenseServiceId.Value);
            }

            // Get user information for RequestedBy and ApprovedBy
            var (requestedByUserName, requestedByUserRole) = (null as string, null as string);
            var (approvedByUserName, approvedByUserRole) = (null as string, null as string);
            
            try
            {
                if (Expense.RequestedBy.HasValue)
                {
                    (requestedByUserName, requestedByUserRole) = await GetUserInfoWithRoleAsync(Expense.RequestedBy);
                }
                if (Expense.ApprovedBy.HasValue)
                {
                    (approvedByUserName, approvedByUserRole) = await GetUserInfoWithRoleAsync(Expense.ApprovedBy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user info for expense {ExpenseId}", Expense.Id);
            }

            return new ExpenseDto
            {
                Id = Expense.Id,
                EventExpenseId = Expense.EventExpenseId,
                ExpenseServiceId = Expense.ExpenseServiceId,
                EventId = Expense.EventId,
                Price = Expense.Price,
                CreatedAt = Expense.CreatedAt,
                IsApprovalNeed = Expense.IsApprovalNeed,
                IsApproved = Expense.IsApproved,
                RequestedBy = Expense.RequestedBy,
                RequestedByUserName = requestedByUserName,
                RequestedByUserRole = requestedByUserRole,
                ApprovedBy = Expense.ApprovedBy,
                ApprovedByUserName = approvedByUserName,
                ApprovedByUserRole = approvedByUserRole,
                ApprovedOn = Expense.ApprovedOn,
                EventExpense = eventExpense != null ? new EventExpenseDto
                {
                    Id = eventExpense.Id,
                    Name = eventExpense.Name,
                    Description = eventExpense.Description,
                    CreatedAt = eventExpense.CreatedAt,
                    IsActive = eventExpense.IsActive,
                    ApprovalRoleId = eventExpense.ApprovalRoleId
                } : null,
                ExpenseService = expenseService != null ? new ExpenseServiceDto
                {
                    Id = expenseService.Id,
                    Name = expenseService.Name,
                    Description = expenseService.Description,
                    CreatedAt = expenseService.CreatedAt,
                    IsActive = expenseService.IsActive,
                    ApprovalRoleId = expenseService.ApprovalRoleId
                } : null
            };
        }

        public async Task<ExpenseDto> UpdateExpenseAsync(int id, UpdateExpenseDto updateDto)
        {
            var Expense = await _ExpenseRepository.GetByIdAsync(id);
            if (Expense == null)
            {
                throw new ArgumentException("Expense not found.");
            }

            Expense.Price = updateDto.Price;
            Expense.UpdatedAt = DateTime.UtcNow;

            await _ExpenseRepository.UpdateAsync(Expense);

            // Get related entities for response
            EventExpense? eventExpense = null;
            TempleApi.Domain.Entities.ExpenseService? expenseService = null;

            if (Expense.EventExpenseId.HasValue)
            {
                eventExpense = await _EventExpenseRepository.GetByIdAsync(Expense.EventExpenseId.Value);
            }
            else if (Expense.ExpenseServiceId.HasValue)
            {
                expenseService = await _ExpenseServiceRepository.GetByIdAsync(Expense.ExpenseServiceId.Value);
            }

            return new ExpenseDto
            {
                Id = Expense.Id,
                EventExpenseId = Expense.EventExpenseId,
                ExpenseServiceId = Expense.ExpenseServiceId,
                EventId = Expense.EventId,
                Price = Expense.Price,
                CreatedAt = Expense.CreatedAt,
                IsApprovalNeed = Expense.IsApprovalNeed,
                IsApproved = Expense.IsApproved,
                RequestedBy = Expense.RequestedBy,
                ApprovedBy = Expense.ApprovedBy,
                ApprovedOn = Expense.ApprovedOn,
                EventExpense = eventExpense != null ? new EventExpenseDto
                {
                    Id = eventExpense.Id,
                    Name = eventExpense.Name,
                    Description = eventExpense.Description,
                    CreatedAt = eventExpense.CreatedAt,
                    IsActive = eventExpense.IsActive,
                    ApprovalRoleId = eventExpense.ApprovalRoleId
                } : null,
                ExpenseService = expenseService != null ? new ExpenseServiceDto
                {
                    Id = expenseService.Id,
                    Name = expenseService.Name,
                    Description = expenseService.Description,
                    CreatedAt = expenseService.CreatedAt,
                    IsActive = expenseService.IsActive,
                    ApprovalRoleId = expenseService.ApprovalRoleId
                } : null
            };
        }

        public async Task<ExpenseDto> ApproveExpenseAsync(int id, int approvedByUserId)
        {
            var expense = await _ExpenseRepository.GetByIdAsync(id);
            if (expense == null)
            {
                throw new ArgumentException("Expense not found.");
            }

            // Update all approval fields
            expense.IsApproved = true;
            expense.ApprovedBy = approvedByUserId;
            expense.ApprovedOn = DateTime.UtcNow;
            expense.UpdatedAt = DateTime.UtcNow; // Also update the UpdatedAt timestamp

            // Log the values before update
            _logger.LogInformation($"Approving expense {id}: IsApproved={expense.IsApproved}, ApprovedBy={expense.ApprovedBy}, ApprovedOn={expense.ApprovedOn:yyyy-MM-dd HH:mm:ss}");

            await _ExpenseRepository.UpdateAsync(expense);

            // Return the updated expense
            return await GetExpenseByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated expense.");
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            return await _ExpenseRepository.DeleteByIdAsync(id);
        }
    }
}

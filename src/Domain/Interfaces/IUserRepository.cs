using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
}

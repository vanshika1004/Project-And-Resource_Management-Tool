using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByUsernameAsync(string username);

    Task<bool> UsernameExistsAsync(string username);

    Task<bool> EmailExistsAsync(string email);

    Task AddAsync(User user);

    void Update(User user);

    Task SaveChangesAsync();
}
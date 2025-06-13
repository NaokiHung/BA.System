using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;
using BA.Server.Data.Contexts;

namespace BA.Server.Data.Repositories
{
    /// <summary>
    /// 使用者 Repository 實作
    /// </summary>
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(UserDbContext context, ILogger<BaseRepository<User>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// 根據使用者名稱查詢使用者
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據使用者名稱查詢失敗: {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// 根據電子郵件查詢使用者
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據電子郵件查詢失敗: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// 檢查使用者名稱是否已存在
        /// </summary>
        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            try
            {
                return await _dbSet.AnyAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查使用者名稱是否存在失敗: {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// 檢查電子郵件是否已存在
        /// </summary>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            try
            {
                return await _dbSet.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查電子郵件是否存在失敗: {Email}", email);
                throw;
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using BA.Server.Core.Interfaces;

namespace BA.Server.Data.Repositories
{
    /// <summary>
    /// 泛型基礎 Repository 實作
    /// 修正重點：確保所有方法簽章與 IBaseRepository<T> 介面完全一致
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<BaseRepository<T>> _logger;

        /// <summary>
        /// BaseRepository 建構子
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="logger">日誌記錄器</param>
        public BaseRepository(DbContext context, ILogger<BaseRepository<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 根據整數 ID 取得實體
        /// </summary>
        /// <param name="id">實體 ID</param>
        /// <returns>實體物件或 null</returns>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogDebug("正在取得實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得實體時發生錯誤，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// 根據字串 ID 取得實體
        /// </summary>
        /// <param name="id">實體 ID</param>
        /// <returns>實體物件或 null</returns>
        public virtual async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                _logger.LogDebug("正在取得實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得實體時發生錯誤，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// 取得所有實體
        /// </summary>
        /// <returns>實體集合</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("正在取得所有實體，類型：{EntityType}", typeof(T).Name);
                return await _dbSet.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有實體時發生錯誤，類型：{EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// 新增實體
        /// 注意：回傳型別必須與介面一致 Task<T>
        /// </summary>
        /// <param name="entity">要新增的實體</param>
        /// <returns>新增後的實體</returns>
        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                _logger.LogDebug("正在新增實體，類型：{EntityType}", typeof(T).Name);
                
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功新增實體，類型：{EntityType}", typeof(T).Name);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增實體時發生錯誤，類型：{EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// 更新實體
        /// 重要修正：回傳型別從 Task<T> 改為 Task（與介面一致）
        /// </summary>
        /// <param name="entity">要更新的實體</param>
        /// <returns>Task（無回傳值）</returns>
        public virtual async Task UpdateAsync(T entity)
        {
            try
            {
                _logger.LogDebug("正在更新實體，類型：{EntityType}", typeof(T).Name);
                
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功更新實體，類型：{EntityType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新實體時發生錯誤，類型：{EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// 根據整數 ID 刪除實體
        /// 重要修正：回傳型別從 Task<bool> 改為 Task（與介面一致）
        /// </summary>
        /// <param name="id">要刪除的實體 ID</param>
        /// <returns>Task（無回傳值）</returns>
        public virtual async Task DeleteAsync(int id)
        {
            try
            {
                _logger.LogDebug("正在刪除實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                
                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("找不到要刪除的實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                    // 根據介面定義，不回傳 bool，直接完成
                    return;
                }

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功刪除實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除實體時發生錯誤，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// 根據字串 ID 刪除實體
        /// 新增方法：原本 BaseRepository 沒有實作這個方法
        /// </summary>
        /// <param name="id">要刪除的實體 ID</param>
        /// <returns>Task（無回傳值）</returns>
        public virtual async Task DeleteAsync(string id)
        {
            try
            {
                _logger.LogDebug("正在刪除實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                
                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("找不到要刪除的實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                    return;
                }

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功刪除實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除實體時發生錯誤，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// 檢查整數 ID 的實體是否存在
        /// 新增方法：原本 BaseRepository 的 ExistsAsync 方法參數不同
        /// </summary>
        /// <param name="id">實體 ID</param>
        /// <returns>是否存在</returns>
        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                _logger.LogDebug("正在檢查實體是否存在，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                
                var entity = await _dbSet.FindAsync(id);
                return entity != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查實體存在性時發生錯誤，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// 檢查字串 ID 的實體是否存在
        /// 新增方法：原本 BaseRepository 的 ExistsAsync 方法參數不同
        /// </summary>
        /// <param name="id">實體 ID</param>
        /// <returns>是否存在</returns>
        public virtual async Task<bool> ExistsAsync(string id)
        {
            try
            {
                _logger.LogDebug("正在檢查實體是否存在，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                
                var entity = await _dbSet.FindAsync(id);
                return entity != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查實體存在性時發生錯誤，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// 根據條件查詢實體
        /// 這個方法原本就存在，保持不變
        /// </summary>
        /// <param name="predicate">查詢條件</param>
        /// <returns>符合條件的實體集合</returns>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug("正在執行條件查詢，類型：{EntityType}", typeof(T).Name);
                return await _dbSet.Where(predicate).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "條件查詢時發生錯誤，類型：{EntityType}", typeof(T).Name);
                throw;
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using BA.Server.Core.Interfaces;

namespace BA.Server.Data.Repositories
{
    /// <summary>
    /// 泛型基礎 Repository 實作
    /// 為什麼要使用泛型？
    /// 1. 避免重複程式碼，提供統一的 CRUD 操作
    /// 2. 支援不同實體類型的資料存取
    /// 3. 維持強型別的安全性
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<BaseRepository<T>> _logger;

        /// <summary>
        /// BaseRepository 建構子
        /// 為什麼改用泛型 DbContext？
        /// 1. 原本的設計要求注入具體的 DbContext 實作（如 UserDbContext）
        /// 2. 但 DI 容器無法自動推斷要注入哪一個具體實作
        /// 3. 改用泛型約束，讓呼叫端明確指定要使用的 DbContext 類型
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
        /// 使用時機：當實體的主鍵是 int 類型時
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
        /// 使用時機：當實體的主鍵是 string 類型時（如 GUID）
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
        /// 為什麼使用 AsNoTracking？
        /// 1. 提升查詢效能
        /// 2. 減少記憶體使用
        /// 3. 適用於唯讀查詢情境
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
        /// 根據條件查詢實體
        /// 使用時機：當需要根據複雜條件查詢時
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

        /// <summary>
        /// 新增實體
        /// 流程說明：
        /// 1. 將實體加入 DbSet
        /// 2. 呼叫 SaveChangesAsync 將變更保存到資料庫
        /// 3. 回傳新增的實體（包含自動生成的 ID）
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
        /// 流程說明：
        /// 1. 將實體標記為已修改
        /// 2. EF Core 會追蹤實體的變更
        /// 3. SaveChangesAsync 時只更新有變更的欄位
        /// </summary>
        /// <param name="entity">要更新的實體</param>
        /// <returns>更新後的實體</returns>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                _logger.LogDebug("正在更新實體，類型：{EntityType}", typeof(T).Name);

                _dbSet.Update(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功更新實體，類型：{EntityType}", typeof(T).Name);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新實體時發生錯誤，類型：{EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// 根據 ID 刪除實體
        /// 流程說明：
        /// 1. 先查詢實體是否存在
        /// 2. 如果存在則進行刪除
        /// 3. 回傳是否成功刪除
        /// </summary>
        /// <param name="id">要刪除的實體 ID</param>
        /// <returns>是否成功刪除</returns>
        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogDebug("正在刪除實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);

                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("找不到要刪除的實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                    return false;
                }

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功刪除實體，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除實體時發生錯誤，類型：{EntityType}，ID：{Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// 檢查實體是否存在
        /// 使用時機：當需要驗證實體存在性而不需要取得實體內容時
        /// </summary>
        /// <param name="predicate">查詢條件</param>
        /// <returns>是否存在</returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug("正在檢查實體是否存在，類型：{EntityType}", typeof(T).Name);
                return await _dbSet.AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查實體存在性時發生錯誤，類型：{EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// 取得實體總數
        /// 使用時機：分頁查詢或統計需求
        /// </summary>
        /// <returns>實體總數</returns>
        public virtual async Task<int> CountAsync()
        {
            try
            {
                _logger.LogDebug("正在計算實體總數，類型：{EntityType}", typeof(T).Name);
                return await _dbSet.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算實體總數時發生錯誤，類型：{EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// 分頁查詢
        /// 使用時機：當需要分頁顯示大量資料時
        /// </summary>
        /// <param name="pageNumber">頁碼（從 1 開始）</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>分頁資料</returns>
        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogDebug("正在執行分頁查詢，類型：{EntityType}，頁碼：{PageNumber}，每頁大小：{PageSize}",
                    typeof(T).Name, pageNumber, pageSize);

                return await _dbSet
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分頁查詢時發生錯誤，類型：{EntityType}", typeof(T).Name);
                throw;
            }
        }
    }
}
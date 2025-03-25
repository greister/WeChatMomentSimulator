using System.Threading.Tasks;

namespace WeChatMomentSimulator.Core.Interfaces.Services
{
    /// <summary>
    /// 文件服务接口
    /// </summary>
    public interface IFileService
    {
        // 接口中使用常量
        const string DefaultSearchPattern = "*.*";
        /// <summary>
        /// 异步读取文本文件内容
        /// </summary>
        /// <param name=""filePath"">文件路径</param>
        /// <returns>文件内容</returns>
        Task<string> ReadTextAsync(string filePath);

        /// <summary>
        /// 异步写入文本内容到文件
        /// </summary>
        /// <param name=""filePath"">文件路径</param>
        /// <param name=""content"">要写入的内容</param>
        /// <returns>异步任务</returns>
        Task WriteTextAsync(string filePath, string content);

        /// <summary>
        /// 确保目录存在，如不存在则创建
        /// </summary>
        /// <param name=""directory"">目录路径</param>
        void EnsureDirectoryExists(string directory);

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name=""filePath"">文件路径</param>
        /// <returns>是否存在</returns>
        bool FileExists(string filePath);
        
        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name=""sourcePath"">源文件路径</param>
        /// <param name=""targetPath"">目标文件路径</param>
        /// <param name=""overwrite"">是否覆盖已存在文件</param>
        /// <returns>异步任务</returns>
        Task CopyFileAsync(string sourcePath, string targetPath, bool overwrite = true);
        
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name=""filePath"">文件路径</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteFileAsync(string filePath);
        
        /// <summary>
        /// 获取目录中的所有文件
        /// </summary>
        /// <param name=""directory"">目录路径</param>
        /// <param name=""searchPattern"">搜索模式</param>
        /// <param name=""recursive"">是否递归搜索</param>
        /// <returns>文件路径列表</returns>
        Task<string[]> GetFilesAsync(string directory, string searchPattern = DefaultSearchPattern, bool recursive = false);
        
    }
}
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeChatMomentSimulator.Services.Services
{
    /// <summary>
    /// 文件查找服务接口
    /// </summary>
    public interface IFileFinder
    {
        /// <summary>
        /// 在指定目录中查找文件
        /// </summary>
        /// <param name="fileName">要查找的文件名</param>
        /// <param name="searchDirectories">要搜索的目录列表</param>
        /// <returns>文件路径，未找到时返回null</returns>
        Task<string> FindFileAsync(string fileName, params string[] searchDirectories);
        
        /// <summary>
        /// 在指定目录中查找所有匹配的文件
        /// </summary>
        /// <param name="searchPattern">搜索模式</param>
        /// <param name="searchDirectories">要搜索的目录列表</param>
        /// <returns>找到的文件路径列表</returns>
        Task<IEnumerable<string>> FindFilesAsync(string searchPattern, params string[] searchDirectories);
    }
}
namespace WeChatMomentSimulator.Core.Interfaces
{
    /// <summary>
    /// 提供应用程序中所有文件路径的统一管理接口
    /// </summary>
    public interface IPathService
    {
        /// <summary>
        /// 获取根目录路径
        /// </summary>
        string GetRootDirectory();
        
        /// <summary>
        /// 获取模板目录路径
        /// </summary>
        string GetTemplatesDirectory();
        
        /// <summary>
        /// 获取模板缩略图目录路径
        /// </summary>
        string GetTemplateThumbnailsDirectory();
        
        /// <summary>
        /// 获取指定ID模板文件的完整路径
        /// </summary>
        string GetTemplateFilePath(string templateId);
        
        /// <summary>
        /// 获取指定ID模板缩略图的完整路径
        /// </summary>
        string GetTemplateThumbnailPath(string templateId);
        
        /// <summary>
        /// 获取资源根目录
        /// </summary>
        string GetAssetsDirectory();
        
        /// <summary>
        /// 获取头像目录
        /// </summary>
        string GetAvatarsDirectory();
        
        /// <summary>
        /// 获取背景图片目录
        /// </summary>
        string GetBackgroundsDirectory();
        
        /// <summary>
        /// 获取状态栏资源目录
        /// </summary>
        string GetStatusBarDirectory();
        
        /// <summary>
        /// 获取导出文件目录
        /// </summary>
        string GetExportsDirectory();
        
        /// <summary>
        /// 获取设置文件目录
        /// </summary>
        string GetSettingsDirectory();
        
        /// <summary>
        /// 获取设置文件路径
        /// </summary>
        string GetSettingsFilePath();
        
        /// <summary>
        /// 设置新的根目录
        /// </summary>
        /// <param name="newPath">新根目录路径</param>
        /// <param name="moveExistingFiles">是否移动现有文件</param>
        /// <returns>操作是否成功</returns>
        bool SetRootDirectory(string newPath, bool moveExistingFiles = true);
        
        /// <summary>
        /// 确保所有必要的子目录结构存在
        /// </summary>
        void EnsureAllDirectoriesExist();
    }
}
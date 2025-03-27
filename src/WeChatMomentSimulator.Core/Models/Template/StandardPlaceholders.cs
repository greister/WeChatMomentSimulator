using System;
using System.Collections.Generic;

namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 标准占位符定义
    /// </summary>
    public static class StandardPlaceholders
    {
        /// <summary>
        /// 获取所有标准占位符定义
        /// </summary>
        public static IEnumerable<PlaceholderDefinition> GetAll()
        {
            return new List<PlaceholderDefinition>
            {
                // 状态栏占位符
                new PlaceholderDefinition 
                { 
                    Name = "time", 
                    Type = PlaceholderType.Text, 
                    Description = "状态栏时间",
                    DefaultValue = "12:34",
                    Category = "状态栏"
                },
                new PlaceholderDefinition 
                { 
                    Name = "battery", 
                    Type = PlaceholderType.Text, 
                    Description = "电池电量",
                    DefaultValue = "80%",
                    Category = "状态栏"
                },
                new PlaceholderDefinition 
                { 
                    Name = "networkType", 
                    Type = PlaceholderType.Text, 
                    Description = "网络类型",
                    DefaultValue = "5G",
                    Options = new[] { "5G", "4G", "WIFI" },
                    Category = "状态栏"
                },
                new PlaceholderDefinition 
                { 
                    Name = "signalStrength", 
                    Type = PlaceholderType.Number, 
                    Description = "信号强度(1-5)",
                    DefaultValue = 5,
                    Category = "状态栏"
                },
                
                // 用户信息占位符
                new PlaceholderDefinition 
                { 
                    Name = "userName", 
                    Type = PlaceholderType.Text, 
                    Description = "用户名称",
                    DefaultValue = "张三",
                    Required = true,
                    Category = "用户信息"
                },
                new PlaceholderDefinition 
                { 
                    Name = "userAvatar", 
                    Type = PlaceholderType.Image, 
                    Description = "用户头像",
                    DefaultValue = "",
                    Category = "用户信息"
                },
                
                // 内容占位符
                new PlaceholderDefinition 
                { 
                    Name = "content", 
                    Type = PlaceholderType.Text, 
                    Description = "文本内容",
                    DefaultValue = "这是一条朋友圈内容...",
                    Category = "内容"
                },
                new PlaceholderDefinition 
                { 
                    Name = "timeText", 
                    Type = PlaceholderType.Text, 
                    Description = "发布时间文本",
                    DefaultValue = "10分钟前",
                    Category = "内容"
                },
                new PlaceholderDefinition 
                { 
                    Name = "postImages", 
                    Type = PlaceholderType.List, 
                    Description = "图片列表",
                    DefaultValue = new List<string>(),
                    Category = "内容"
                },
                new PlaceholderDefinition 
                { 
                    Name = "hasImages", 
                    Type = PlaceholderType.Boolean, 
                    Description = "是否包含图片",
                    DefaultValue = false,
                    Category = "内容"
                },
                
                // 互动信息占位符
                new PlaceholderDefinition 
                { 
                    Name = "likes", 
                    Type = PlaceholderType.Number, 
                    Description = "点赞数量",
                    DefaultValue = 0,
                    Category = "互动"
                },
                new PlaceholderDefinition 
                { 
                    Name = "comments", 
                    Type = PlaceholderType.List, 
                    Description = "评论列表",
                    DefaultValue = new List<object>(),
                    Category = "互动"
                },
                new PlaceholderDefinition 
                { 
                    Name = "hasComments", 
                    Type = PlaceholderType.Boolean, 
                    Description = "是否有评论",
                    DefaultValue = false,
                    Category = "互动"
                }
            };
        }
        
        /// <summary>
        /// 按类别分组获取标准占位符
        /// </summary>
        public static Dictionary<string, List<PlaceholderDefinition>> GetGroupedByCategory()
        {
            var result = new Dictionary<string, List<PlaceholderDefinition>>();
            
            foreach (var placeholder in GetAll())
            {
                if (!result.ContainsKey(placeholder.Category))
                {
                    result[placeholder.Category] = new List<PlaceholderDefinition>();
                }
                
                result[placeholder.Category].Add(placeholder);
            }
            
            return result;
        }
    }
}
using System.Reflection;

namespace SplusXBTMeter.Core
{
    /// <summary>
    /// 应用程序信息静态类
    /// 提供对程序集元数据的统一访问接口
    /// </summary>
    /// <remarks>
    /// 该类通过反射从 AssemblyInfo.cs 中读取程序集信息，
    /// 避免在代码中硬编码版本号、公司名称等信息，
    /// 确保信息的一致性和可维护性。
    /// </remarks>
    public static class AppInfo
    {
        /// <summary>
        /// 获取应用程序标题
        /// </summary>
        /// <value>
        /// 程序集标题，通常显示在窗口标题、关于对话框等位置。
        /// 对应 AssemblyInfo.cs 中的 [AssemblyTitle] 特性。
        /// </value>
        /// <example>
        /// "SplusXBTMeter蓝牙设备电量显示"
        /// </example>
        public static string Title =>
            Assembly.GetExecutingAssembly()
                   .GetCustomAttribute<AssemblyTitleAttribute>()?
                   .Title ?? "Unknown";

        /// <summary>
        /// 获取开发公司名称
        /// </summary>
        /// <value>
        /// 开发该软件的公司或组织名称。
        /// 对应 AssemblyInfo.cs 中的 [AssemblyCompany] 特性。
        /// </value>
        /// <example>
        /// "Splusx"
        /// </example>
        public static string Author =>
            Assembly.GetExecutingAssembly()
                   .GetCustomAttribute<AssemblyCompanyAttribute>()?
                   .Company ?? "Unknown";

        /// <summary>
        /// 获取产品名称
        /// </summary>
        /// <value>
        /// 产品的正式名称，可能与标题略有不同。
        /// 对应 AssemblyInfo.cs 中的 [AssemblyProduct] 特性。
        /// </value>
        /// <example>
        /// "SplusXBTMeter"
        /// </example>
        public static string Product =>
            Assembly.GetExecutingAssembly()
                   .GetCustomAttribute<AssemblyProductAttribute>()?
                   .Product ?? "Unknown";

        /// <summary>
        /// 获取版权信息
        /// </summary>
        /// <value>
        /// 软件的版权声明，通常包含年份和版权所有者。
        /// 对应 AssemblyInfo.cs 中的 [AssemblyCopyright] 特性。
        /// </value>
        /// <example>
        /// "Copyright © Splusx 2026"
        /// </example>
        public static string Copyright =>
            Assembly.GetExecutingAssembly()
                   .GetCustomAttribute<AssemblyCopyrightAttribute>()?
                   .Copyright ?? "Unknown";

        /// <summary>
        /// 获取应用程序版本号
        /// </summary>
        /// <value>
        /// 程序集的完整版本号（主版本.次版本.生成号.修订号）。
        /// 对应 AssemblyInfo.cs 中的 [AssemblyVersion] 特性。
        /// </value>
        /// <example>
        /// "0.2.2.0"
        /// </example>
        /// <remarks>
        /// 版本号格式：主版本.次版本.生成号.修订号
        /// - 主版本：重大更新或不兼容的API更改
        /// - 次版本：向后兼容的功能新增
        /// - 生成号：向后兼容的问题修复
        /// - 修订号：紧急修复或小更新
        /// </remarks>
        public static string Version =>
            Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

        /// <summary>
        /// 获取应用程序版本号（不含修订号）
        /// </summary>
        /// <value>
        /// 程序集的主版本号和次版本号（主版本.次版本）。
        /// 常用于显示简化的版本信息。
        /// </value>
        /// <example>
        /// "0.2"
        /// </example>
        public static string ShortVersion
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return version != null ? $"{version.Major}.{version.Minor}" : "0.0";
            }
        }

        /// <summary>
        /// 获取应用程序完整版本号（用于内部判断）
        /// </summary>
        /// <returns>Version 对象，可用于版本比较</returns>
        /// <example>
        /// <code>
        /// if (AppInfo.GetVersion().Major >= 1)
        /// {
        ///     // 执行某些操作
        /// }
        /// </code>
        /// </example>
        public static Version GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version ?? new Version("0.0.0");
        }

        /// <summary>
        /// 获取程序集描述信息
        /// </summary>
        /// <value>
        /// 程序集的描述性文本。
        /// 对应 AssemblyInfo.cs 中的 [AssemblyDescription] 特性。
        /// </value>
        public static string Description =>
            Assembly.GetExecutingAssembly()
                   .GetCustomAttribute<AssemblyDescriptionAttribute>()?
                   .Description ?? "蓝牙设备电量显示工具";

        /// <summary>
        /// 获取程序集配置信息（Debug/Release）
        /// </summary>
        /// <value>
        /// 程序集的编译配置，通常为 "Debug" 或 "Release"。
        /// 对应 AssemblyInfo.cs 中的 [AssemblyConfiguration] 特性。
        /// </value>
        public static string Configuration =>
            Assembly.GetExecutingAssembly()
                   .GetCustomAttribute<AssemblyConfigurationAttribute>()?
                   .Configuration ?? "Unknown";
        /// <summary>
        /// 获取程序github地址
        /// </summary>
        public static string GitHubUrl =>
           "https://github.com/SpLlry/SplusXBTMeter";
        /// <summary>
        /// 获取程序gitee地址
        /// </summary>
        public static string GiteeUrl =>
           "https://gitee.com/spllr/SplusXBTMeter";
        /// <summary>
        /// 获取程序gitee地址
        /// </summary>
        public static string GiteeLastReleases =>
           "https://gitee.com/api/v5/repos/spllr/SplusXBTMeter/releases/latest";
        /// <summary>
        /// 获取程序gitee地址
        /// </summary>
        public static string GiteeReleases =>
            "https://gitee.com/spllr/SplusXBTMeter/releases";
    }
}
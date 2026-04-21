#nullable enable
using System.IO;

namespace BTBatteryDisplayApp
{
    /// <summary>
    /// 参照 Python configparser 实现的 C# INI 配置类
    /// 接口/逻辑/延迟保存 完全一致
    /// </summary>
    public class Config
    {
        // 核心配置存储：Section -> Key -> Value
        private readonly Dictionary<string, Dictionary<string, string>> _config = new();
        private readonly string _filePath;
        private bool _dirty = false;
        private System.Timers.Timer? _saveTimer = null;
        private readonly double _saveDelay = 1000; // 1秒延迟保存，和Python一致

        /// <summary>
        /// 构造函数：传入配置文件路径
        /// </summary>
        public Config(string configFilePath)
        {
            _filePath = configFilePath;

            // 自动创建目录
            string? dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // 加载配置文件
            LoadConfig();
        }

        #region 核心方法（和 Python 接口完全一致）
        /// <summary>
        /// 获取值
        /// </summary>
        public string? getVal(string section, string? key = null, string? defaultValue = null)
        {
            if (!has_section(section) || key==null)
                return defaultValue;

           
                if (!has_option(section, key))
                    return defaultValue;
                return _config[section][key];

        }

        /// <summary>
        /// 设置值
        /// </summary>
        public void setVal(string section, string key, object value)
        {
            if (!_config.ContainsKey(section))
            {
                _config[section] = new Dictionary<string, string>();
            }

            _config[section][key] = value.ToString() ?? "";
            _markDirty();
        }

        /// <summary>
        /// 是否存在节点
        /// </summary>
        public bool has_section(string section)
        {
            return _config.ContainsKey(section);
        }

        /// <summary>
        /// 是否存在节点下的键
        /// </summary>
        public bool has_option(string section, string key)
        {
            return has_section(section) && _config[section].ContainsKey(key);
        }

        /// <summary>
        /// 获取节点下所有键值对
        /// </summary>
        public List<KeyValuePair<string, string>> items(string section)
        {
            if (!has_section(section))
                return new List<KeyValuePair<string, string>>();

            return _config[section].ToList();
        }

        /// <summary>
        /// 删除键/节点
        /// </summary>
        public void del_Val(string section, string? key = null)
        {
            if (!has_section(section))
                return;

            if (key != null)
            {
                if (_config[section].ContainsKey(key))
                {
                    _config[section].Remove(key);
                    _markDirty();
                }
            }
            else
            {
                _config.Remove(section);
                _markDirty();
            }
        }

        /// <summary>
        /// 获取所有配置
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> all()
        {
            return new Dictionary<string, Dictionary<string, string>>(_config);
        }

        /// <summary>
        /// 立即刷新保存
        /// </summary>
        public void flush()
        {
            _saveTimer?.Stop();
            _flush();
        }
        #endregion

        #region 延迟保存（和 Python 逻辑一致）
        private void _markDirty()
        {
            _dirty = true;

            // 取消旧定时器
            _saveTimer?.Stop();
            _saveTimer?.Dispose();

            // 新建定时器
            _saveTimer = new System.Timers.Timer(_saveDelay);
            _saveTimer.Elapsed += (s, e) => _flush();
            _saveTimer.AutoReset = false;
            _saveTimer.Start();
        }

        private void _flush()
        {
            if (!_dirty) return;

            try
            {
                // 写入 INI 文件（UTF-8 编码）
                using (StreamWriter sw = new StreamWriter(_filePath, false, System.Text.Encoding.UTF8))
                {
                    foreach (var section in _config)
                    {
                        sw.WriteLine($"[{section.Key}]");
                        foreach (var kv in section.Value)
                        {
                            sw.WriteLine($"{kv.Key}={kv.Value}");
                        }
                        sw.WriteLine();
                    }
                }

                _dirty = false;
                Console.WriteLine($"✅ 配置已保存：{_filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 保存失败：{ex.Message}");
            }
        }
        #endregion

        #region 加载配置文件
        private void LoadConfig()
        {
            if (!File.Exists(_filePath))
                return;

            try
            {
                string currentSection = "";
                foreach (string line in File.ReadAllLines(_filePath, System.Text.Encoding.UTF8))
                {
                    string trimLine = line.Trim();

                    // 跳过空行/注释
                    if (string.IsNullOrEmpty(trimLine) || trimLine.StartsWith('#') || trimLine.StartsWith('/'))
                        continue;

                    // 读取节点 [Section]
                    if (trimLine.StartsWith('[') && trimLine.EndsWith(']'))
                    {
                        currentSection = trimLine.Substring(1, trimLine.Length - 2);
                        if (!_config.ContainsKey(currentSection))
                        {
                            _config[currentSection] = new Dictionary<string, string>();
                        }
                        continue;
                    }

                    // 读取键值对 key=value
                    int index = trimLine.IndexOf('=');
                    if (index > 0 && !string.IsNullOrEmpty(currentSection))
                    {
                        string key = trimLine.Substring(0, index).Trim();
                        string value = trimLine.Substring(index + 1).Trim();
                        _config[currentSection][key] = value;
                    }
                }
            }
            catch
            {
                // 加载失败忽略
            }
        }
        #endregion
    }
}
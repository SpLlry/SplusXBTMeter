#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using SplusXBTMeter;
using SplusXBTMeter.core;
using SplusXBTMeter.ViewModels;

namespace SplusXBTMeter
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : HandyControl.Controls.Window
    {
        private SettingViewModel? _viewModel;

        public SettingWindow()
        {
            InitializeComponent();
            _viewModel = new SettingViewModel();
            DataContext = _viewModel;
            Loaded += Loaded_;
            Closed += Closed_;
        }

        private void Loaded_(object? sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel?.LoadSettings();
            }
            catch (Exception ex)
            {
                // 完全还原你原版的 HandyControl MessageBox
                HandyControl.Controls.MessageBox.Show($"失败：{ex.Message}");
                Close();
            }
        }
        private void Closed_(object? sender, EventArgs e)
        {
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 现在可以通过 ViewModel 的 SelectedSkin 属性获取选中的值
            if (_viewModel != null)
            {
                Console.WriteLine($"选中的Skin: {_viewModel.SelectedSkin}");
            }
        }

        // 🔥 双击列表项弹出编辑窗口
        private void ListBox_MouseDoubleClick(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            var listBox = sender as ListBox;
            // 获取双击的设备对象
            var device = listBox?.SelectedItem as DeviceBatteryInfo;
            if (device == null)
                return;
            _viewModel?.EditDevice(device);
        }

        // 事件处理方法
        private void IsStartup_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                var checkBox = sender as System.Windows.Controls.CheckBox;
                _viewModel.IsStartup = checkBox?.IsChecked == true;
            }
        }

        private void IsShowTaskBar_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                var checkBox = sender as System.Windows.Controls.CheckBox;
                _viewModel.IsShowTaskBar = checkBox?.IsChecked == true;
            }
        }

        private void IsShowMain_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                var checkBox = sender as System.Windows.Controls.CheckBox;
                _viewModel.IsShowMain = checkBox?.IsChecked == true;
            }
        }
    }
}
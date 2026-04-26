# SplusXBTMeter 项目说明
## 1. 项目概述
SplusXBTMeter是一个实用的蓝牙设备电量监控工具,基于c#开发,通过直观的系统托盘和任务栏帮助用户实时了解蓝牙设备的电量状态，避免因设备电量耗尽而影响使用。项目采用现代化的技术栈，具有良好的用户体验和系统集成度。

### 1.1下载地址

蓝奏云：https://bigsu.lanzoul.com/b02z2q0zhe
密码:cn27

### 1.2开源地址
* GitHub:[https://github.com/SpLlry/BTPowerNotice/](https://github.com/SpLlry/BTPowerNotice/)
* Gitee:[https://gitee.com/spllr/BTPowerNotice/](https://gitee.com/spllr/BTPowerNotice/)
* GitHub:[https://github.com/SpLlry/SplusXBTMeter](https://github.com/SpLlry/SplusXBTMeter)
* Gitee:[https://gitee.com/spllr/SplusXBTMeter](https://gitee.com/spllr/SplusXBTMeter)

> *托盘*
> 
> ![托盘](https://gh-proxy.com/https://raw.githubusercontent.com/SpLlry/BTPowerNotice/refs/heads/main/resource/Snipaste_2026-03-20_17-09-08.png)
>
> *任务栏*
> 
> ![任务栏样式](https://gh-proxy.com/https://raw.githubusercontent.com/SpLlry/BTPowerNotice/refs/heads/main/resource/566750244-4cc02b2b-0175-42b7-9058-4ce39f48aa5a.png)
>
> ![任务栏样式](https://gh-proxy.com/https://raw.githubusercontent.com/SpLlry/BTPowerNotice/refs/heads/main/resource/Snipaste_2026-03-20_17-08-45.png)

## 2. 核心功能
### 2.1 蓝牙设备扫描
- 支持 BLE (低功耗蓝牙) 和 BTC (经典蓝牙) 设备的扫描
- 每 5 秒自动扫描一次设备状态
- 实时获取设备连接状态和电量信息
### 2.2 设备状态监控
- 跟踪设备连接/断开状态变化
- 监控设备电量变化，当电量低于 20% 时显示警告
- 记录设备状态变化日志
### 2.3 系统集成
- 系统托盘图标显示，可查看设备电量
- 任务栏小部件显示，支持最多 4 个设备的电量状态
- 自动适应 Windows 系统主题（浅色/深色）
- 支持 Windows 10/11 任务栏对齐方式
### 2.4 错误处理
- 全局异常捕获和详细日志记录
- 蓝牙设备扫描异常处理
- 设备电量获取失败处理
## 3. 技术架构
### 3.1 技术栈
- 编程语言 ：C# .net8
- GUI 框架 ：HandyControl
- 蓝牙 API ：
  - WinRT (用于 BLE 设备)
  - Windows API (用于经典蓝牙设备)
- 系统集成 ：
  - Windows 注册表操作
  - 系统主题检测
  - 任务栏对齐方式检测

### 3.3 核心流程
1. 程序启动 ：初始化主窗口、系统托盘和任务栏小部件
2. 设备扫描 ：启动蓝牙扫描线程，扫描 BLE 和 BTC 设备
3. 数据处理 ：
   - 处理设备连接状态变化
   - 处理设备电量变化
   - 更新设备状态缓存
4. UI 更新 ：
   - 更新系统托盘图标显示
   - 更新任务栏小部件显示
5. 循环执行 ：每 5 秒重复扫描和更新流程

## 4. 关键实现细节
### 4.1 蓝牙设备扫描
- BLE 设备 ：使用 WinRT API 扫描 BLE 设备，通过 GATT 服务获取电量
- 经典蓝牙设备 ：使用 Windows API 扫描经典蓝牙设备，通过设备属性获取电量
### 4.2 任务栏小部件
- 支持最多显示 4 个设备的电量状态
- 自动适应任务栏对齐方式（居中/左对齐）
- 支持系统主题切换（浅色/深色）
### 4.3 异常处理
- 全局异常捕获，记录详细错误信息
- 蓝牙操作异常处理，确保程序稳定运行
## 5. 系统要求
- 操作系统 ：Windows 10 或 Windows 11


## 6. 安装与运行
暂无
## 7. 使用说明
1. 首次运行 ：程序会自动扫描附近的蓝牙设备
2. 系统托盘 ：
   - 点击托盘图标可查看设备电量
   - 右键菜单可进行相关操作
3. 任务栏 ：
   - 显示已连接设备的电量状态
   - 电量低于 20% 时显示警告图标
4. 日志 ：
   - 程序运行日志保存在指定位置
   - 记录设备连接状态和电量变化
## 8. 未来扩展
- 支持更多蓝牙设备类型
- 添加设备管理功能（忽略/优先显示特定设备）
- 增加电量历史记录和统计
- 支持自定义通知方式
- 跨平台支持（ macOS、Linux ）
## 捐赠:
**[赞助名单](https://www.splusx.com/sponsor.html)**

<div>开发维护不易，请我喝杯可乐吧~(请附上您的GitHub用户名或QQ等联系方式,感谢您的支持)</div>

爱发电：[https://ifdian.net/a/BTPowerNotice](https://ifdian.net/a/BTPowerNotice)

| 支付宝                                  | 微信                                      |
|--------------------------------------|-----------------------------------------|
| ![支付宝扫码](https://gh-proxy.com/https://raw.githubusercontent.com/SpLlry/BTPowerNotice/refs/heads/main/resource/20260408174618_230.jpg) | ![微信扫码](https://gh-proxy.com/https://raw.githubusercontent.com/SpLlry/BTPowerNotice/refs/heads/main/resource/a3b2e1d764ad207c66f7aad7e7e6578f.jpg) |




